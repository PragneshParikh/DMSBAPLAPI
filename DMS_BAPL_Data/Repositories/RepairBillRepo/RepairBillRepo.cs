using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.JobCardRepo;
using DMS_BAPL_Data.Services.MaterialTransferService;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Data.Repositories.RepairBillRepo
{
    public class RepairBillRepo : IRepairBillRepo
    {

        private readonly BapldmsvadContext _context;
        private readonly IMaterialTransferService _materialTransferService;
        private readonly IJobCardRepo _jobCardRepo;

        public RepairBillRepo(BapldmsvadContext context, IMaterialTransferService  materialTransferService, IJobCardRepo jobCardRepo)
        {
            _context = context;
            _materialTransferService = materialTransferService;
            _jobCardRepo = jobCardRepo;
        }

        public async Task<int> InsertRepairBill(RepairBillInsertVM model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var RepairBillheader = new RepairBillHeader
                {
                    LocationCode = model.RepairBillheader.LocationCode,
                    DealerCode = model.RepairBillheader.DealerCode,
                    Prefix = model.RepairBillheader.Prefix,
                    BillNo = model.RepairBillheader.BillNo,
                    BillType = model.RepairBillheader.BillType,
                    CashAccount = model.RepairBillheader.CashAccount,
                    CustomerLedgerId = model.RepairBillheader.CustomerLedgerId == 0 ? null : model.RepairBillheader.CustomerLedgerId,
                    JobId = model.RepairBillheader.JobId,
                    InsuranceId = model.RepairBillheader.InsuranceId == 0 ? null : model.RepairBillheader.InsuranceId,
                    InsDecription = model.RepairBillheader.insDescription,
                    SurveyorName = model.RepairBillheader.SurveyorName,
                    ContactNumber = model.RepairBillheader.ContactNumber,
                    PolicyNo = model.RepairBillheader.policyNo,
                    InsValidTill = model.RepairBillheader.insValidTill,
                    ZeroDepo = model.RepairBillheader.zeroDepo,
                    Remarks = model.RepairBillheader.Remarks,
                    TotalDiscount = model.RepairBillheader.TotalDiscount,
                    TotalTaxableAmount = model.RepairBillheader.TotalTaxableAmount ?? 0,
                    TotalNetAmount = model.RepairBillheader.TotalNetAmount ?? 0,
                    AmountReceived = model.RepairBillheader.AmountRecived ?? 0,
                    IsActive = model.RepairBillheader.IsActive,
                    IsSavedPerforma = model.RepairBillheader.IsSavedPerforma,
                    IsSavedInvoice = model.RepairBillheader.IsSavedInvoice,
                    RepairbillStatus = model.RepairBillheader.RepairBillStatus,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.RepairBillHeaders.Add(RepairBillheader);

                await _context.SaveChangesAsync();

                foreach (var item in model.RepairBillDetail)
                {
                    var RepairBillDetail = new RepairBillDetail
                    {
                        RepairBillId = RepairBillheader.Id,

                        ItemType = item.ItemType,

                        MaterialId = item.MaterialId == 0 ? null : item.MaterialId,

                        LabourMasterId = item.LabourId == 0 ? null : item.LabourId,
                        PartWiseLabourId = item.PartWiseLabourId == 0 ? null : item.PartWiseLabourId,

                        PartItemId = item.PartItemId == 0 ? null : item.PartItemId,

                        LabourQty = item.Qty,
                        LabourRate = item.Rate,

                        PartQty = item.PartQty,
                        PartRate = item.PartRate,

                        Fscrate = item.FscRate,

                        DiscountValue = item.DiscountValue,
                        LabourDiscount = item.Discount,
                        DiscountType = item.DiscountType,
                        PartDiscount = item.PartDiscount,
                        /// = item.DiscountType,




                        Igstamount = item.IgstAmount,
                        Cgstamount = item.CgstAmount,
                        Sgstamount = item.SgstAmount,

                        LabourTaxblAmount = item.TaxableAmount,
                        LabourNetAmount = item.NetAmount ?? 0,
                        PartTaxblAmount = item.PartTaxbleAmount,
                        PartNetAmount = item.PartNetAmount ?? 0,
                        TotalTaxPer = item.TotalTaxPer ?? 0,
                        IssutypeId = item.IssueType,


                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    _context.RepairBillDetails.Add(RepairBillDetail);
                }

                // 2026-09-07 fix ("in jobcard invoiceno why not update?" - JobCardHeader.InvoiceNo
                // was never written by this flow at all, on either the proforma or invoice step).
                // Most repair bills go through "Save as Proforma" first (RepairbillStatus something
                // like "Performa created") and only become "Billed" via UpdateRepairBill below - but
                // if a bill can ever be created already-Billed directly through InsertRepairBill in
                // your flow, this keeps JobCardHeader.InvoiceNo in sync for that case too. Guarded on
                // BillNo actually being assigned so this never writes a blank/placeholder invoice no.
                // (BillNo is a plain non-nullable int on this model, not int? - checked as > 0.)
                if (string.Equals(RepairBillheader.RepairbillStatus, "Billed", StringComparison.OrdinalIgnoreCase)
                    && RepairBillheader.BillNo > 0
                    && RepairBillheader.JobId > 0)
                {
                    var job = await _context.JobCardHeaders.FirstOrDefaultAsync(j => j.Id == RepairBillheader.JobId);
                    if (job != null)
                    {
                        job.InvoiceNo = $"{RepairBillheader.Prefix}{RepairBillheader.BillNo}";
                    }
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RepairBillheader.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<RepairBillListVM>> GetAllRepairBillList(RepairBillSearchVM search)
        {
            try
            {
                var query =
                    from rb in _context.RepairBillHeaders

                    join jh in _context.JobCardHeaders
                        on rb.JobId equals jh.Id into jhJoin
                    from jh in jhJoin.DefaultIfEmpty()

                    join jc in _context.JobCardCustomers
                        on jh.Id equals jc.JobCardHeaderId into jcJoin
                    from jc in jcJoin.DefaultIfEmpty()
                    join lg in _context.LedgerMasters
                        on rb.CustomerLedgerId equals lg.Id into lgJoin
                    from lg in lgJoin.DefaultIfEmpty()
                    select new
                    {
                        RepairBill = rb,
                        ledger = lg,
                        JobCard = jh,
                        Customer = jc,

                    };
                // DealerCode

                if (!string.IsNullOrWhiteSpace(search.DealerCode))
                {
                    query = query.Where(x =>
                        x.RepairBill.DealerCode == search.DealerCode);
                }
                // Location
                if (!string.IsNullOrWhiteSpace(search.LocationCode))
                {
                    query = query.Where(x =>
                        x.RepairBill.LocationCode == search.LocationCode);
                }

                // Bill No
                if (search.BillNo.HasValue)
                {
                    query = query.Where(x =>
                        x.RepairBill.BillNo == search.BillNo.Value);
                }

                // Job No
                if (search.JobNo.HasValue)
                {
                    query = query.Where(x =>
                        x.JobCard != null &&
                        x.JobCard.JobNo == search.JobNo.Value);
                }

                // Chassis No
                if (!string.IsNullOrWhiteSpace(search.ChassisNo))
                {
                    query = query.Where(x =>
                        x.Customer != null &&
                        x.Customer.ChassisNo.Contains(search.ChassisNo));
                }

                // Date From
                if (search.DateFrom.HasValue)
                {
                    query = query.Where(x =>
                        x.RepairBill.CreatedDate >= search.DateFrom.Value);
                }

                // Date To
                if (search.DateTo.HasValue)
                {
                    var toDate = search.DateTo.Value.Date.AddDays(1);

                    query = query.Where(x =>
                        x.RepairBill.CreatedDate < toDate);
                }

                var result = await query
                    .Select(x => new RepairBillListVM
                    {
                        Id = x.RepairBill.Id,
                        LocationCode = x.RepairBill.LocationCode,
                        CashAccount = x.RepairBill.CashAccount,
                        Prefix = x.RepairBill.Prefix,
                        BillNo = x.RepairBill.BillNo,
                        BillType = x.RepairBill.BillType,
                        PartyName = x.ledger.LedgerName,
                        IsDelete = x.RepairBill.IsDelete,

                        ChassisNumber = x.Customer != null ? x.Customer.ChassisNo : null,
                        RegistrationNo = x.Customer != null ? x.Customer.RegisterNo : null,
                        JobCardNo = x.JobCard != null ? x.JobCard.JobNo : null,
                        TotalNetAmount = x.RepairBill.TotalNetAmount,
                        RepairBillStatus = x.RepairBill.RepairbillStatus,
                        CreatedBy = x.RepairBill.DealerCode,
                        CreatedDate = x.RepairBill.CreatedDate,
                        UpdatedBy = x.RepairBill.DealerCode,

                    })
                    .Where(x => x.IsDelete != true) // Exclude deleted records
                    .OrderByDescending(x => x.Id)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<RepairBillUpdateVM?> GetRepairBillById(int id)
        {
            try
            {
                var billedResult = await (
               from rbh in _context.RepairBillHeaders

               join jh in _context.JobCardHeaders
               on rbh.JobId equals jh.Id into jobJoin
               from jh in jobJoin.DefaultIfEmpty()

               join jc in _context.JobCardCustomers
                   on jh.Id equals jc.JobCardHeaderId into custJoin
               from jc in custJoin.DefaultIfEmpty()

               join st in _context.States
                   on rbh.CustomerLedger.State equals st.StateId into stJoin
               from st in stJoin.DefaultIfEmpty()
               join dl in _context.DealerMasters
                   on rbh.DealerCode equals dl.Dealercode into dlJoin
               from dl in dlJoin.DefaultIfEmpty()

               where rbh.Id == id

               select new RepairBillUpdateVM
               {
                   RepairBillheader = new RepairBillUpdateHeaderVM
                   {
                       Id = rbh.Id,
                       LocationCode = rbh.LocationCode,
                       DealerCode = rbh.DealerCode,
                       Prefix = rbh.Prefix,
                       BillNo = rbh.BillNo,
                       BillType = rbh.BillType,
                       CashAccount = rbh.CashAccount,
                       DealerState = dl != null ? dl.State : null,

                       CustomerLedgerId =
                           rbh.CustomerLedgerId == 0
                           ? null
                           : rbh.CustomerLedgerId,

                       PartyName =
                           jh != null && jh.Jobtype == 1
                               ? jc.CustomerName
                               : rbh.CustomerLedger != null
                                   ? rbh.CustomerLedger.LedgerName
                                   : null,

                       MobileNumber =
                          jh != null && jh.Jobtype == 1
                              ? jc.CustomerMobile
                              : rbh.CustomerLedger != null
                                  ? rbh.CustomerLedger.MobileNumber
                                  : null,

                       PartyState = st != null
                           ? st.StateName
                           : null,

                       JobId = rbh.JobId,
                       JobInDate = jh.JobinDate,
                       JobNo = jh.JobNo,
                       RegisterNo = jc.RegisterNo,
                       ModelName = jc.ModelName,
                       VehicleKms = jh.Vehiclekms,
                       ChassisNo = jc.ChassisNo,
                       Technician = jh.Technician,
                       InsuranceId = rbh.InsuranceId ?? 0,
                       insDescription = rbh.InsDecription,
                       SurveyorName = rbh.SurveyorName,
                       ContactNumber = rbh.ContactNumber,
                       policyNo = rbh.PolicyNo,
                       InsValidTill = rbh.InsValidTill,
                       zeroDepo = rbh.ZeroDepo ?? false,
                       Remarks = rbh.Remarks,
                       RepairBillStatus = rbh.RepairbillStatus,
                       TotalDiscount = rbh.TotalDiscount ?? 0,
                       TotalTaxableAmount = rbh.TotalTaxableAmount ?? 0,
                       TotalNetAmount = rbh.TotalNetAmount ?? 0,
                       AmountRecived = rbh.AmountReceived ?? 0
                   },

                   RepairBillDetail = rbh.RepairBillDetails
                       .Select(d => new RepairBillUpdateDetailVM
                       {
                           Id = d.Id,
                           ItemType = d.ItemType,

                           MaterialId = d.MaterialId ?? 0,
                           PartItemId = d.PartItemId ?? 0,
                           LabourId = d.LabourMasterId ?? 0,
                           PartWiseLabourId = d.PartWiseLabourId ?? 0,
                           DealerState = dl != null ? dl.State : null,



                           // NEW
                           PartCode = d.PartItem != null
                                ? d.PartItem.Itemcode
                                : "",

                           PartDesc = d.PartItem != null
                           ? d.PartItem.Itemdesc
                           : "",
                           Cgst = d.LabourMasterId > 0
                           ? d.LabourMaster.Cgst
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.Cgst
                           : 0,
                           Sgst = d.LabourMasterId > 0
                           ? d.LabourMaster.Sgst
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.Sgst
                           : 0,
                           Igst = d.LabourMasterId > 0
                           ? d.LabourMaster.Igst
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.Igst
                           : 0,
                           //Cgst = d.PartItem != null
                           //? d.PartItem.Cgst
                           //: 0,
                           //Sgst = d.PartItem != null
                           //? d.PartItem.Sgst
                           //: 0,
                           //Igst = d.PartItem != null
                           //? d.PartItem.Igst
                           //: 0,
                           LabourCode = d.LabourMasterId > 0
                           ? d.LabourMaster.LabourCode
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.LabourCode
                           : "",

                           LabourDescription = d.LabourMasterId > 0
                           ? d.LabourMaster.LabourDescription
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.LabourName
                           : "",

                           Qty = d.LabourQty ?? 0,
                           Rate = d.LabourRate ?? 0,
                           PartHsnCode = d.PartItem != null
                           ? d.PartItem.Hsncode
                           : "",
                           PartMRP = d.PartItem != null
                            ? d.PartItem.Custprice
                            : 0,
                           LabourHsnCode = d.LabourMasterId > 0
                           ? d.LabourMaster.Hsncode
                           : d.PartWiseLabour != null
                           ? d.PartWiseLabour.Hsncode : "",

                           PartQty = d.PartQty ?? 0,
                           PartRate = d.PartRate ?? 0,
                           TotalTaxPer = d.TotalTaxPer ?? 0,

                           DiscountValue = d.DiscountValue ?? 0,
                           Discount = d.LabourDiscount ?? 0,
                           DiscountType = d.DiscountType,

                           PartDiscount = d.PartDiscount ?? 0,


                           TaxableAmount = d.LabourTaxblAmount ?? 0,
                           NetAmount = d.LabourNetAmount ?? 0,

                           PartTaxbleAmount = d.PartTaxblAmount ?? 0,
                           PartNetAmount = d.PartNetAmount ?? 0,

                           CgstAmount = d.Cgstamount ?? 0,
                           SgstAmount = d.Sgstamount ?? 0,
                           IgstAmount = d.Igstamount ?? 0,

                           IssueType = d.IssutypeId ?? 0,

                           //IsAutoGenerated = d.IsAutoGenerated
                       })
                       .ToList()
               }
           ).FirstOrDefaultAsync();

                return billedResult;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<bool> UpdateRepairBill(RepairBillUpdateVM model, string userId)
        {

            try
            {
                var header = await _context.RepairBillHeaders
                .Include(x => x.RepairBillDetails)
                .Include(x => x.Job) // 2026-09-07 fix: needed so header.Job.InvoiceNo below is
                                     // actually tracked/loaded, not null - see fix block after
                                     // header.RepairbillStatus is set.
                .FirstOrDefaultAsync(x => x.Id == model.RepairBillheader.Id);

                if (header == null)
                    return false;

                // Header Update
                header.BillType = model.RepairBillheader.BillType;
                header.CashAccount = model.RepairBillheader.CashAccount;
                header.CustomerLedgerId = model.RepairBillheader.CustomerLedgerId;
                header.InsuranceId = model.RepairBillheader.InsuranceId == 0 ? null : model.RepairBillheader.InsuranceId;
                header.InsDecription = model.RepairBillheader.insDescription;
                header.SurveyorName = model.RepairBillheader.SurveyorName;
                header.ContactNumber = model.RepairBillheader.ContactNumber;
                header.PolicyNo = model.RepairBillheader.policyNo;
                header.InsValidTill = model.RepairBillheader.InsValidTill;
                header.ZeroDepo = model.RepairBillheader.zeroDepo;
                header.Remarks = model.RepairBillheader.Remarks;
                header.TotalDiscount = model.RepairBillheader.TotalDiscount;
                header.TotalTaxableAmount = model.RepairBillheader.TotalTaxableAmount;
                header.TotalNetAmount = model.RepairBillheader.TotalNetAmount;
                header.AmountReceived = model.RepairBillheader.AmountRecived;
                header.IsSavedInvoice = model.RepairBillheader.IsSavedInvoice;
                header.RepairbillStatus = model.RepairBillheader.RepairBillStatus;
                header.UpdatedBy = userId;
                header.UpdatedDate = DateTime.Now;

                // 2026-09-07 fix ("in jobcard invoiceno why not update? ... thats why in our
                // jobscanner invoice not download") - this is the actual "Save as Invoice" step
                // (Angular sets RepairBillStatus to "Billed" here), and it never wrote the invoice
                // number back onto the job it belongs to. JobCardScanner (and anything else reading
                // JobCardHeader directly) only ever sees InvoiceNo, never RepairBillHeader - so
                // without this, that column just stays NULL forever even after a bill is fully
                // invoiced. Guarded on BillNo actually being assigned so this can't write a blank
                // invoice number if RepairbillStatus is set to "Billed" before BillNo is finalized -
                // if that can happen in your flow, this block needs to move to wherever BillNo is
                // actually assigned instead. (BillNo is a plain non-nullable int here, not int? -
                // checked as > 0, same as the InsertRepairBill block above.)
                if (string.Equals(header.RepairbillStatus, "Billed", StringComparison.OrdinalIgnoreCase)
                    && header.BillNo > 0)
                {
                    if (header.Job == null)
                    {
                        // Include(x => x.Job) above should normally cover this, but fall back to an
                        // explicit lookup in case JobId ever points at a header not already tracked.
                        header.Job = await _context.JobCardHeaders.FirstOrDefaultAsync(j => j.Id == header.JobId);
                    }
                    if (header.Job != null)
                    {
                        header.Job.InvoiceNo = $"{header.Prefix}{header.BillNo}";
                    }
                }

                // Existing DB Details
                var existingDetails = header.RepairBillDetails.ToList();

                // Delete Removed Rows
                var incomingIds = model.RepairBillDetail
                    .Where(x => x.Id > 0)
                    .Select(x => x.Id)
                    .ToList();

                var deleteRows = existingDetails
                    .Where(x => !incomingIds.Contains(x.Id))
                    .ToList();

                _context.RepairBillDetails.RemoveRange(deleteRows);

                foreach (var item in model.RepairBillDetail)
                {
                    // Existing Row Update
                    if (item.Id > 0)
                    {
                        var detail = existingDetails
                            .FirstOrDefault(x => x.Id == item.Id);

                        if (detail != null)
                        {
                            detail.ItemType = item.ItemType;
                            detail.MaterialId = item.MaterialId > 0 ? item.MaterialId : null;
                            detail.PartItemId = item.PartItemId;

                            detail.LabourMasterId = item.LabourId > 0 ? item.LabourId : null;

                            detail.PartWiseLabourId = item.PartWiseLabourId > 0 ? item.PartWiseLabourId : null;

                            detail.PartQty = item.PartQty;
                            detail.PartRate = item.PartRate;

                            detail.LabourQty = item.Qty;
                            detail.LabourRate = item.Rate;

                            detail.DiscountValue = item.DiscountValue;
                            detail.PartDiscount = item.PartDiscount;
                            detail.LabourDiscount = item.Discount;

                            detail.DiscountType = item.DiscountType;

                            detail.PartTaxblAmount = item.PartTaxbleAmount;
                            detail.PartNetAmount = item.PartNetAmount;

                            detail.LabourTaxblAmount = item.TaxableAmount;
                            detail.LabourNetAmount = item.NetAmount;

                            detail.Cgstamount = item.CgstAmount;
                            detail.Sgstamount = item.SgstAmount;
                            detail.Igstamount = item.IgstAmount;
                            detail.TotalTaxPer = item.TotalTaxPer;

                            detail.IssutypeId = item.IssueType;
                            detail.UpdatedBy = userId;
                            detail.UpdatedDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        // New Row Insert
                        var newDetail = new RepairBillDetail
                        {
                            RepairBillId = header.Id,

                            ItemType = item.ItemType,
                            MaterialId = item.MaterialId > 0 ? item.MaterialId : null,
                            PartItemId = item.PartItemId > 0 ? item.PartItemId : null,
                            LabourMasterId = item.LabourId > 0 ? item.LabourId : null,
                            PartWiseLabourId = item.PartWiseLabourId > 0 ? item.PartWiseLabourId : null,

                            PartQty = item.PartQty,
                            PartRate = item.PartRate,

                            LabourQty = item.Qty,
                            LabourRate = item.Rate,

                            DiscountValue = item.DiscountValue,
                            PartDiscount = item.PartDiscount,
                            LabourDiscount = item.Discount,

                            DiscountType = item.DiscountType,

                            PartTaxblAmount = item.PartTaxbleAmount,
                            PartNetAmount = item.PartNetAmount,

                            LabourTaxblAmount = item.TaxableAmount,
                            LabourNetAmount = item.NetAmount,

                            Cgstamount = item.CgstAmount,
                            Sgstamount = item.SgstAmount,
                            Igstamount = item.IgstAmount,
                            TotalTaxPer = item.TotalTaxPer,

                            IssutypeId = item.IssueType,
                            CreatedBy = userId,
                            CreatedDate = DateTime.Now
                        };

                        _context.RepairBillDetails.Add(newDetail);
                    }
                }
                await _context.SaveChangesAsync();
                return true;


            }
            catch (Exception ex)
            {
                return false;
                throw new Exception(ex.Message);
            }
        }
        //public async Task<int> DeleteRepairbill(int repairbillId, string role, string userId)
        //{
        //    if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        //        throw new Exception("Only SuperAdmin can delete repair bills.");

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var repairbillHeader = await _context.RepairBillHeaders
        //            .FirstOrDefaultAsync(x => x.Id == repairbillId);

        //        if (repairbillHeader == null)
        //            throw new Exception("Repair bill header not found");

        //        repairbillHeader.IsDelete = true;
        //        repairbillHeader.UpdatedBy = userId;   // now a real, non-null value from the token
        //        repairbillHeader.UpdatedDate = DateTime.UtcNow;

        //        var result = await _context.SaveChangesAsync();

        //        if (repairbillHeader.JobId > 0)
        //        {
        //            int jobId = repairbillHeader.JobId;

        //            await _materialTransferService.DeleteMaterialsByJobId(
        //                jobId, isSuperAdmin: true, createdBy: userId);

        //            await _jobCardRepo.UpdateMaterialTransferStatus(jobId, false);
        //        }

        //        await transaction.CommitAsync();
        //        return result;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        //public async Task<int> DeleteRepairbill(int repairbillId, string role, string userId)
        //{
        //    if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        //        throw new Exception("Only SuperAdmin can delete repair bills.");

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var repairbillHeader = await _context.RepairBillHeaders
        //            .FirstOrDefaultAsync(x => x.Id == repairbillId);

        //        if (repairbillHeader == null)
        //            throw new Exception("Repair bill header not found");

        //        repairbillHeader.IsDelete = true;
        //        repairbillHeader.UpdatedBy = userId;
        //        repairbillHeader.UpdatedDate = DateTime.UtcNow;

        //        var result = await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        return result;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        //public async Task<int> DeleteRepairbill(int repairbillId, string role, string userId)
        //{
        //    if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
        //        throw new Exception("Only SuperAdmin can delete repair bills.");

        //    using var transaction = await _context.Database.BeginTransactionAsync();

        //    try
        //    {
        //        var repairbillHeader = await _context.RepairBillHeaders
        //            .FirstOrDefaultAsync(x => x.Id == repairbillId);

        //        if (repairbillHeader == null)
        //            throw new Exception("Repair bill header not found");

        //        repairbillHeader.IsDelete = true;
        //        repairbillHeader.UpdatedBy = userId;
        //        repairbillHeader.UpdatedDate = DateTime.UtcNow;

        //        var result = await _context.SaveChangesAsync();

        //        await transaction.CommitAsync();
        //        return result;
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task<int> DeleteRepairbill(int repairbillId, string role, string userId)
        {
            if (!string.Equals(role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
                throw new Exception("Only SuperAdmin can delete repair bills.");

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var repairbillHeader = await _context.RepairBillHeaders
                    .FirstOrDefaultAsync(x => x.Id == repairbillId);

                if (repairbillHeader == null)
                    throw new Exception("Repair bill header not found");

                repairbillHeader.IsDelete = true;
                repairbillHeader.UpdatedBy = userId;
                repairbillHeader.UpdatedDate = DateTime.UtcNow;

                var result = await _context.SaveChangesAsync();


                if (repairbillHeader.JobId > 0)
                {
                    var jobCardHeader = await _context.JobCardHeaders
                        .FirstOrDefaultAsync(x => x.Id == repairbillHeader.JobId);

                    if (jobCardHeader != null)
                    {
                        var ffir = await _context.Ffirheaders
                            .Where(f => f.JobCardHeaderId == jobCardHeader.Id)
                            .OrderByDescending(f => f.CreatedDate)
                            .FirstOrDefaultAsync();

                        jobCardHeader.JobStatus =
                            jobCardHeader.IsMaterialTransfer == true ? "Material Transfer"
                            : ffir != null && ffir.Ffirstatus == "Closed" ? "FFIR Closed"
                            : ffir != null ? "FFIR Created"
                            : "Open";

                        await _context.SaveChangesAsync();
                    }
                }

                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<RepairBillPerformaVM?> generateRepairBillPerformaDetails(string dealerCode, int repairBillId)
        {
            var result = await (
                from rbh in _context.RepairBillHeaders

                join dl in _context.DealerMasters
                    on rbh.DealerCode equals dl.Dealercode into dlJoin
                from dl in dlJoin.DefaultIfEmpty()

                join jh in _context.JobCardHeaders
                    on rbh.JobId equals jh.Id into jhJoin
                from jh in jhJoin.DefaultIfEmpty()

                join jobType in _context.JobTypes
                    on jh.Jobtype equals jobType.Id into jobTypeJoin
                from jobType in jobTypeJoin.DefaultIfEmpty()

                join jobSource in _context.JobSources
                    on jh.JobSource equals jobSource.Id into jobSourceJoin
                from jobSource in jobSourceJoin.DefaultIfEmpty()

                join jc in _context.JobCardCustomers
                    on jh.Id equals jc.JobCardHeaderId into jcJoin
                from jc in jcJoin.DefaultIfEmpty()

                join cust in _context.LedgerMasters
                    on rbh.CustomerLedgerId equals cust.Id into custJoin
                from cust in custJoin.DefaultIfEmpty()

                join ins in _context.LedgerMasters
                    on rbh.InsuranceId equals ins.Id into insJoin
                from ins in insJoin.DefaultIfEmpty()

                join st in _context.States
                    on cust.State equals st.StateId into stJoin
                from st in stJoin.DefaultIfEmpty()

                join ct in _context.Cities
                    on cust.City equals ct.CityId into ctJoin
                from ct in ctJoin.DefaultIfEmpty()

                join insState in _context.States
                    on ins.State equals insState.StateId into insStateJoin
                from insState in insStateJoin.DefaultIfEmpty()

                join insCity in _context.Cities
                    on ins.City equals insCity.CityId into insCityJoin
                from insCity in insCityJoin.DefaultIfEmpty()

                join itemModel in _context.ItemMasters
                    on jc.ModelName equals itemModel.Itemname into itemModelJoin
                from itemModel in itemModelJoin.DefaultIfEmpty()

                join modelColor in _context.ColorMasters
                    on itemModel.Colorcode equals modelColor.Colorcode into modelColorJoin
                from modelColor in modelColorJoin.DefaultIfEmpty()

                where rbh.Id == repairBillId

                select new RepairBillPerformaVM
                {
                    Id = rbh.Id,
                    BillType = rbh.BillType,
                    Remarks = rbh.Remarks,
                    DealerName = dl != null ? dl.Compname : "",
                    DealerAddress = dl != null ? dl.Adress1 : "",
                    DealerPhoneNo = dl != null ? dl.Mobile : "",
                    DealerEmail = dl != null ? dl.Email : "",
                    DealergstNo = dl != null ? dl.CompgstinNo : "",
                    DealerPanNo = dl != null ? dl.Pan : "",
                    DealerPinNo = dl != null ? dl.Pin : "",
                    DealerState = dl != null ? dl.State : "",
                    DealerCity = dl != null ? dl.City : "",
                    ContactPerson = dl != null ? dl.Contactperson : "",

                    InsuranceName = ins != null ? ins.LedgerName : "",
                    InsGSTINNo = ins != null ? ins.Gstno : "",
                    InsAddress = ins != null ? ins.Address : "",
                    InsState = insState != null ? insState.StateName : "",
                    InsCity = insCity != null ? insCity.CityName : "",
                    InsPinNo = ins != null ? ins.Pin : "",

                    CustomerName = cust != null ? cust.LedgerName : "",
                    CustomerPhoneNo = cust != null ? cust.MobileNumber : "",
                    CustomerAddress = cust != null ? cust.Address : "",
                    CustomerGSTINNo = cust != null ? cust.Gstno : "",
                    CustomerState = st != null ? st.StateName : "",
                    CustomerCity = ct != null ? ct.CityName : "",
                    CustomerPincode = cust != null ? cust.Pin : "",

                    TechnicianName = jh != null ? jh.Technician : "",
                    InvoiceNo = jh != null ? jh.InvoiceNo : "",
                    ChassisNo = jc != null ? jc.ChassisNo : "",
                    RegisterationNo = jc != null ? jc.RegisterNo : "",
                    MotorNo = jc != null ? jc.MotorNo : "",
                    ModelName = jc != null ? jc.ModelName : "",
                    Color = modelColor != null ? modelColor.Colorname : "",
                    JobNo = jh != null ? jh.JobNo : 0,
                    JobInDate = jh != null ? jh.JobinDate : null,
                    JobTypeName = jobType != null ? jobType.JobTypeName : "",
                    JobSourceName = jobSource != null ? jobSource.JobSourceName : "",
                    VehicleSaleDate = jc != null ? jc.SaleDate : null,
                    VehicleExpiryDate = jc != null ? jc.InsuranceExpDate : null
                })
                .FirstOrDefaultAsync();

            if (result == null)
                return null;

            result.RepairBillDetail = await _context.RepairBillDetails
                        .Include(x => x.PartItem)
                        .Include(x => x.LabourMaster)
                        .Include(x => x.PartWiseLabour)

                        .Where(x => x.RepairBillId == repairBillId)

                        .Select(d => new RepairBillPerformaUpdateDetailVM
                        {
                            ItemType = d.ItemType,

                            MaterialId = d.MaterialId ?? 0,
                            LabourId = d.LabourMasterId ?? 0,
                            PartWiseLabourId = d.PartWiseLabourId ?? 0,
                            PartItemId = d.PartItemId,

                            PartCode = d.PartItem != null
                                ? d.PartItem.Itemcode
                                : "",

                            PartDesc = d.PartItem != null
                                ? d.PartItem.Itemdesc
                                : "",

                            Cgst = d.PartItem != null
                                ? d.PartItem.Cgst
                                : 0,

                            Sgst = d.PartItem != null
                                ? d.PartItem.Sgst
                                : 0,

                            Igst = d.PartItem != null
                                ? d.PartItem.Igst
                                : 0,

                            LabourCode = d.LabourMaster != null
                                ? d.LabourMaster.LabourCode
                                : d.PartWiseLabour != null
                                    ? d.PartWiseLabour.LabourCode
                                    : "",

                            LabourDescription = d.LabourMaster != null
                                ? d.LabourMaster.LabourDescription
                                : d.PartWiseLabour != null
                                    ? d.PartWiseLabour.LabourName
                                    : "",

                            Qty = d.LabourQty ?? 0,
                            Rate = d.LabourRate ?? 0,

                            PartQty = d.PartQty ?? 0,
                            PartRate = d.PartRate ?? 0,

                            Discount = d.LabourDiscount ?? 0,
                            DiscountType = d.DiscountType,

                            PartDiscount = d.PartDiscount ?? 0,

                            TaxableAmount = d.LabourTaxblAmount ?? 0,
                            NetAmount = d.LabourNetAmount ?? 0,

                            PartTaxbleAmount = d.PartTaxblAmount ?? 0,
                            PartNetAmount = d.PartNetAmount ?? 0,

                            LabourHSNCode = d.LabourMaster != null ? d.LabourMaster.Hsncode : "",
                            PartHSNCode = d.PartItem != null ? d.PartItem.Hsncode : "",

                            CgstAmount = d.PartItem != null ? d.PartItem.Cgst : 0,
                            SgstAmount = d.PartItem != null ? d.PartItem.Sgst : 0,
                            IgstAmount = d.PartItem != null ? d.PartItem.Igst : 0,
                            IssueType = d.IssutypeId ?? 0,
                        })
                        .ToListAsync();

            result.TotalPartQty = (int?)result.RepairBillDetail
                    .Where(x => x.ItemType == "Part")
                    .Sum(x => x.PartQty);

            result.TotalPartTaxbleAmount = result.RepairBillDetail
                .Where(x => x.ItemType == "Part")
                .Sum(x => x.PartTaxbleAmount);

            result.TotalPartSGST = result.RepairBillDetail
                .Where(x => x.ItemType == "Part")
                .Sum(x => x.SgstAmount);

            result.TotalPartCGST = result.RepairBillDetail
                .Where(x => x.ItemType == "Part")
                .Sum(x => x.CgstAmount);

            result.TotalPartNetAmount = result.RepairBillDetail
                .Where(x => x.ItemType == "Part")
                .Sum(x => x.PartNetAmount);


            // LABOUR TOTALS
            result.TotalLabourQty = (int?)result.RepairBillDetail
                .Where(x => x.ItemType == "Labour")
                .Sum(x => x.Qty);

            result.TotalLabourTaxbleAmount = result.RepairBillDetail
                .Where(x => x.ItemType == "Labour")
                .Sum(x => x.TaxableAmount);

            result.TotalLabourSGST = result.RepairBillDetail
                .Where(x => x.ItemType == "Labour")
                .Sum(x => x.SgstAmount);

            result.TotalLabourCGST = result.RepairBillDetail
                .Where(x => x.ItemType == "Labour")
                .Sum(x => x.CgstAmount);

            result.TotalLabourNetAmount = result.RepairBillDetail
                .Where(x => x.ItemType == "Labour")
                .Sum(x => x.NetAmount);

            result.SubTotal = result.TotalPartNetAmount + result.TotalLabourNetAmount;
            result.Roundoff = Math.Round(result.SubTotal ?? 0) - (result.SubTotal ?? 0);

            result.HSNCODeTaxSummary = result.RepairBillDetail

                   .GroupBy(x => new
                   {
                       HSNCode = x.ItemType == "Part"
                           ? x.PartHSNCode
                           : x.LabourHSNCode,

                       SGST = x.Sgst,
                       CGST = x.Cgst,
                       IGST = x.Igst
                   })

                   .Select(g => new RepairBillTaxSummaryVM
                   {
                       HSNCode = g.Key.HSNCode,

                       TaxableValue = g.Sum(x =>
                           x.ItemType == "Part"
                               ? x.PartTaxbleAmount
                               : x.TaxableAmount),

                       SGSTRate = g.Key.SGST,
                       SGSTAmount = g.Sum(x => x.SgstAmount),

                       CGSTRate = g.Key.CGST,
                       CGSTAmount = g.Sum(x => x.CgstAmount),

                       IGSTRate = g.Key.IGST,
                       IGSTAmount = g.Sum(x => x.IgstAmount)
                   })

                   .OrderBy(x => x.HSNCode)
                   .ToList();
            var totalTaxable = result.HSNCODeTaxSummary.Sum(x => x.TaxableValue);

            var totalSGST = result.HSNCODeTaxSummary.Sum(x => x.SGSTAmount);

            var totalCGST = result.HSNCODeTaxSummary.Sum(x => x.CGSTAmount);

            var totalIGST = result.HSNCODeTaxSummary.Sum(x => x.IGSTAmount);

            return result;
        }


    }


}