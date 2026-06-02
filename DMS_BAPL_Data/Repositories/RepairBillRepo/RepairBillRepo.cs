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
                    PartyName = model.RepairBillheader.PartyName,
                    MobileNumber = model.RepairBillheader.MobileNumber,
                    JobId = model.RepairBillheader.JobId,
                    InsuranceId = model.RepairBillheader.InsuranceId,
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

                    select new
                    {
                        RepairBill = rb,
                        JobCard = jh,
                        Customer = jc
                    };
                // DealerCode

                if(!string.IsNullOrWhiteSpace(search.DealerCode))
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
                        PartyName = x.RepairBill.PartyName,
                       
                        ChassisNumber = x.Customer != null ? x.Customer.ChassisNo : null,
                        RegistrationNo = x.Customer != null ? x.Customer.RegisterNo : null,
                        JobCardNo = x.JobCard != null ? x.JobCard.JobNo : null,
                        TotalNetAmount = x.RepairBill.TotalNetAmount,
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
    }
}
