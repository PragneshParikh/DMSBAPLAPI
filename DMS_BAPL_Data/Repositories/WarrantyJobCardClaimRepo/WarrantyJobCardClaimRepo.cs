using DMS_BAPL_Data.CustomModel;
using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DMS_BAPL_Data.Repositories.WarrantyJobCardClaimRepo
{
    public class WarrantyJobCardClaimRepo : IWarrantyJobCardClaimRepo
    {
        private readonly BapldmsvadContext _context;

        public WarrantyJobCardClaimRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        [HttpPost("InsertWarrantyJCClaim")]
        [ProducesResponseType(typeof(PagedResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<int> InsertWarrantyJCClaim(WarrantyJCClaimViewModel model, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //=========================
                // Insert Header
                //=========================
                var header = new WarrantyJcclaim
                {
                    DealerCode = model.DealerCode,
                    ClaimPrefix = model.ClaimPrefix,
                    ClaimNo = model.ClaimNo,
                    ClaimDate = model.ClaimDate,
                    ChassisNo = model.ChassisNo,
                    SupplierId = model.SupplierId,
                    JobCardHeaderId = model.JobCardHeaderId,
                    CustomerLedgerId = model.CustomerLedgerId,
                    RepairBillHeaderId = model.RepairBillHeaderId,
                    Ffirid = model.FFIRId,
                    ClaimAccount = model.ClaimAccount,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    //IsActive = true
                };

                _context.WarrantyJcclaims.Add(header);
                await _context.SaveChangesAsync();

                //=========================
                // Insert Details
                //=========================
                if (model.repairBillDetails != null && model.repairBillDetails.Any())
                {
                    var details = model.repairBillDetails.Select(x => new WarrantyJcclaimDetail
                    {
                        WarrantyJcclaimHeaderId = header.Id,

                        RepairBillDetailId = x.RepairBillDetailsId,

                        ItemType = x.ItemType,

                        MaterialId = x.MaterialId,
                        LabourMasterId = x.LabourId,
                        PartWiseLabourId = x.PartWiseLabourId,
                        PartItemId = x.PartItemId,

                        Qty = x.ItemType == "Labour"
                         ? (x.LabourQty ?? 0)
                         : x.PartItemQty,

                        Rate = x.ItemType == "Labour"
                         ? (x.LabourRate ?? 0)
                         : (x.PartItemRate ?? 0),

                        // Calculate Amount based on whether it is a Labour or Part item
                        Amount = x.IgstAmount,

                        // Calculate TaxAmount (If you have a tax percentage property, e.g., x.TaxPercentage)
                        // If you don't have tax percentage, you will need to pass it from the model or database.
                        TaxAmount = x.TotalWithTax ?? 0,

                        // Calculate TotalAmount by adding Amount and TaxAmount
                        TotalAmount = x.IgstAmount + x.TotalWithTax,


                        ClaimType = "Warranty",
                        DealerObservation = x.DealerObservation,
                        RootCauseAnalysis = x.RootCauseAnalysis,

                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    }).ToList();

                    _context.WarrantyJcclaimDetails.AddRange(details);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                model.RepairBillHeaderId = header.Id;

                return header.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



    }
}
