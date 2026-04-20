using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DMS_BAPL_Data.Services.TaxServices
{
    public class TaxServices: ITaxServices
    {
        private readonly BapldmsvadContext _context;
        public TaxServices(BapldmsvadContext context)
        {
          _context = context;  
        }

        public async Task<List<TaxDetailViewModel>> GetTaxDetailsAsync(string itemCode, string userLocation)
        {
            try{

            // Step 1: Get item with HSN
            var item = await _context.ItemMasters
          .Where(i => i.Itemcode == itemCode).FirstOrDefaultAsync();



            if (item == null)
                throw new Exception("Item not found");



            var hsnCode = item.Hsncode;



            // Step 2: Determine State Flag
            string companyLocation = StringConstants.CompanyLocation; 



            string stateFlag = userLocation == companyLocation ? "S" : "O";



            // Step 3: Get AtaxCode from HSN mapping
            var hsnTax = await _context.HsnwiseTaxCodes
          .Where(h => h.Hsncode == hsnCode && h.StateFlag == stateFlag)
          .OrderByDescending(h => h.EffectiveDate)
          .FirstOrDefaultAsync();



            if (hsnTax == null)
                throw new Exception("No tax mapping found for HSN");



            var ataxCode = hsnTax.AtaxCode;



            // Step 4: Get tax breakup
            var taxDetails = await _context.AggregateTaxCodes
          .Where(a => a.AtaxCode == ataxCode)
          .OrderBy(a => a.SrNo)
          .Select(a => new TaxDetailViewModel
          {
              SrNo = a.SrNo,
              TaxCode = a.TaxCode,
              TaxRate = a.TaxRate
          })
          .ToListAsync();



            return taxDetails;
            }
            catch
            {
                throw;
            }
        }
    }
}
