using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DMS_BAPL_Data.Repositories.HSNCodeMaterRepo
{
    public class HSNCodeMaterRepo : IHSNCodeMaterRepo
    {
        private readonly BapldmsvadContext _bapldmsvadContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HSNCodeMaterRepo(BapldmsvadContext bapldmsvadContext, IHttpContextAccessor httpContextAccessor)
        {
            _bapldmsvadContext = bapldmsvadContext;
            _httpContextAccessor = httpContextAccessor;
        }



        public async Task<List<HsncodeMaster>> GetAllHSNCodeListAsync(string? search)
        {

            IQueryable<HsncodeMaster> query = _bapldmsvadContext.HsncodeMasters;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    EF.Functions.Like(d.Hsncode, $"%{search}%") ||
                    EF.Functions.Like(d.Description, $"%{search}%") ||
                    EF.Functions.Like(d.Type, $"%{search}%")
                );
            }

            return await query.ToListAsync();
        }
        public async Task<HsncodeMaster?> GetByIdAsync(int id)
        {
            return await _bapldmsvadContext.HsncodeMasters
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<HsncodeMaster> AddAsync(HSNCodeMasterViewModel entity)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var exists = await _bapldmsvadContext.HsncodeMasters.AnyAsync(x => x.Hsncode == entity.Hsncode);

            if (exists)
            {
                throw new ApplicationException(StringConstants.HSNCodeExists);
            }
            var newHsnCode = new HsncodeMaster
            {
                Hsncode = entity.Hsncode,
                Description = entity.Description,
                Type = entity.Type,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,

            };

            await _bapldmsvadContext.HsncodeMasters.AddAsync(newHsnCode);

            await _bapldmsvadContext.SaveChangesAsync();

            return newHsnCode;

        }

        public async Task<bool> UpdateAsync(int id, HSNCodeMasterViewModel entity)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var exists = await _bapldmsvadContext.HsncodeMasters.AnyAsync(x => x.Hsncode == entity.Hsncode && x.Id != id);

            if (exists)
            {
                throw new Exception(StringConstants.HSNCodeExists);
            }
            var existing = await _bapldmsvadContext.HsncodeMasters
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existing == null)
                return false;

            existing.Hsncode = entity.Hsncode;
            existing.Description = entity.Description;
            existing.Type = entity.Type;
            existing.UpdatedBy = userId;
            existing.UpdatedDate = DateTime.UtcNow;

            await _bapldmsvadContext.SaveChangesAsync();

            return true;
        }



    }
}