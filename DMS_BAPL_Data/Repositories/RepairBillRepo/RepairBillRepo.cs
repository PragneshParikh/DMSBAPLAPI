using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DMS_BAPL_Utils.ViewModels.RepairBillViewModel;

namespace DMS_BAPL_Data.Repositories.RepairBillRepo
{
    public class RepairBillRepo : IRepairBillRepo
    {

        private readonly BapldmsvadContext _context;

        public RepairBillRepo(BapldmsvadContext context)
        {
            _context = context;
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

                        PartItemId = item.PartItemId,

                        LabourQty = item.Qty,
                        LabourRate = item.Rate,

                        PartQty = item.PartQty,
                        PartRate = item.PartRate,

                        Fscrate = item.FscRate,


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
                        IssutypeId = item.IssueType,


                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };

                    _context.RepairBillDetails.Add(RepairBillDetail);
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
                        Prefix = x.RepairBill.Prefix,
                        BillNo = x.RepairBill.BillNo,
                        BillType = x.RepairBill.BillType,
                        PartyName = x.ledger.LedgerName,
                        

                        ChassisNumber = x.Customer != null ? x.Customer.ChassisNo : null,
                        RegistrationNo = x.Customer != null ? x.Customer.RegisterNo : null,
                        JobCardNo = x.JobCard != null ? x.JobCard.JobNo : null,
                        TotalNetAmount = x.RepairBill.TotalNetAmount,
                        RepairBillStatus = x.RepairBill.RepairbillStatus,
                        CreatedBy = x.RepairBill.DealerCode,
                        CreatedDate = x.RepairBill.CreatedDate,
                        UpdatedBy = x.RepairBill.DealerCode,

                    })
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
            var billedResult = await (
                from rbh in _context.RepairBillHeaders

                join st in _context.States
                    on rbh.CustomerLedger.State equals st.StateId into stJoin
                from st in stJoin.DefaultIfEmpty()

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

                        CustomerLedgerId =
                            rbh.CustomerLedgerId == 0
                            ? null
                            : rbh.CustomerLedgerId,

                        PartyName = rbh.CustomerLedger != null
                            ? rbh.CustomerLedger.LedgerName
                            : null,

                        MobileNumber = rbh.CustomerLedger != null
                            ? rbh.CustomerLedger.MobileNumber
                            : null,

                        PartyState = st != null
                            ? st.StateName
                            : null,

                        JobId = rbh.JobId,
                        InsuranceId = rbh.InsuranceId ?? 0,
                        insDescription = rbh.InsDecription,
                        SurveyorName = rbh.SurveyorName,
                        ContactNumber = rbh.ContactNumber,
                        policyNo = rbh.PolicyNo,
                        ValidTill = rbh.InsValidTill,
                        zeroDepo = rbh.ZeroDepo ?? false,
                        Remarks = rbh.Remarks,
                        TotalDiscount = rbh.TotalDiscount ?? 0,
                        TotalTaxableAmount = rbh.TotalTaxableAmount ?? 0,
                        TotalNetAmount = rbh.TotalNetAmount ?? 0,
                        AmountRecived = rbh.AmountReceived ?? 0
                    },

                    RepairBillDetail = rbh.RepairBillDetails
                        .Select(d => new RepairBillDetailVM
                        {
                            Id = d.Id,
                            ItemType = d.ItemType,
                            MaterialId = d.MaterialId ?? 0,
                            LabourId = d.LabourMasterId ?? 0,
                            PartWiseLabourId = d.PartWiseLabourId ?? 0,
                            PartItemId = d.PartItemId,

                            Qty = d.LabourQty ?? 0,
                            Rate = d.LabourRate ?? 0,

                            PartQty = d.PartQty ?? 0,
                            PartRate = d.PartRate ?? 0,

                            FscRate = d.Fscrate ?? 0,

                            Discount = d.LabourDiscount ?? 0,
                            DiscountType = d.DiscountType,

                            PartDiscount = d.PartDiscount ?? 0,

                            IgstAmount = d.Igstamount ?? 0,
                            CgstAmount = d.Cgstamount ?? 0,
                            SgstAmount = d.Sgstamount ?? 0,

                            TaxableAmount = d.LabourTaxblAmount ?? 0,
                            NetAmount = d.LabourNetAmount ?? 0,

                            PartTaxbleAmount = d.PartTaxblAmount ?? 0,
                            PartNetAmount = d.PartNetAmount ?? 0,

                            IssueType = d.IssutypeId ?? 0
                        })
                        .ToList()
                }
            ).FirstOrDefaultAsync();

            return billedResult;
        }
    }
}
