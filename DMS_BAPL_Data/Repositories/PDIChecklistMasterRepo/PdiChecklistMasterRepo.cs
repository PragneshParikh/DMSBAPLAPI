using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Utils.Constants;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.PDIChecklistMasterRepo
{
    public class PdiChecklistMasterRepo : IPdiCheckListMaster
    {
        private readonly BapldmsvadContext _context;

        public PdiChecklistMasterRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> InsertPdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster)
        {
            try
            {
                var entity = new PdichecklistMaster
                {
                    PdiheadName = "PDI Check List",
                    PdicheckName = pdiChecklistMaster.ChecklistName,
                    Pdidescription = pdiChecklistMaster.ChecklistName,
                    Isactive = pdiChecklistMaster.IsActive,
                    CreatedBy = "Admin",
                    CreatedDate = DateTime.Now
                };
                await _context.PdichecklistMasters.AddAsync(entity);
                await _context.SaveChangesAsync();
                return (true, "Inserted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error occurred while inserting: {ex.Message}");
            }

        }
        public async Task<(bool Success, string Message)> UpdatePdiChecklistMaster(PdiChecklistMasterViemModel pdiChecklistMaster)
        {
            try
            {
                var entity = await _context.PdichecklistMasters
                    .FirstOrDefaultAsync(x => x.Id == pdiChecklistMaster.Id);

                if (entity == null)
                    return (false, "Record not found");

                // update fields        
                entity.PdicheckName = pdiChecklistMaster.ChecklistName;
                entity.Pdidescription = pdiChecklistMaster.ChecklistName;
                entity.Isactive = pdiChecklistMaster.IsActive;
                entity.UpdatedBy = "Admin";
                entity.UpdatedDated = DateTime.Now;

                await _context.SaveChangesAsync();

                return (true, "Updated successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string Message)> DeletePdiChecklistMaster(int pdicheckId)
        {
            try
            {
                var entity = await _context.PdichecklistMasters
                    .FirstOrDefaultAsync(x => x.Id == pdicheckId);

                if (entity == null)
                    return (false, "Record not found");

                //  Check relation in child table
                bool isUsed = await _context.PdichecklistChassisWises
                    .AnyAsync(x => x.PdichecklistMasterId == pdicheckId);

                if (isUsed)
                {
                    //  Relation exists → DO NOT DELETE
                    entity.Isactive = false;
                    entity.UpdatedBy = "Admin"; // Or set to current user
                    entity.UpdatedDated = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return (false, StringConstants.DeletePdiChecklist);
                }

                //  No relation → HARD DELETE
                _context.PdichecklistMasters.Remove(entity);

                await _context.SaveChangesAsync();

                return (true, "Deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PdiChecklistMasterViemModel>> GetPdiChecklistMasterList(string? pdicheckName)
        {
            try
            {
                var query = _context.PdichecklistMasters.AsQueryable();

                //  Search filter
                if (!string.IsNullOrWhiteSpace(pdicheckName))
                {
                    query = query.Where(x => x.PdicheckName.Contains(pdicheckName));
                }

                //  Select projection
                var result = await query
                        .OrderByDescending(x => x.CreatedDate) // DB level sorting
                        .Select(pcm => new PdiChecklistMasterViemModel
                        {
                            Id = pcm.Id,
                            ChecklistName = pcm.PdicheckName,
                            IsActive = pcm.Isactive
                        })
                        .ToListAsync();

                return result;
            }
            catch (Exception)
            {
                return new List<PdiChecklistMasterViemModel>();
            }
        }
    }
}
