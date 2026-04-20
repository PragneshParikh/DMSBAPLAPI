using DMS_BAPL_Data.DBModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.StateRepo
{
    public partial class StateRepo : IStateRepo
    {
        private readonly BapldmsvadContext _context;
        public StateRepo(BapldmsvadContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<State>> GetStatesAsync()
        {
            return await _context.States.ToListAsync();
        }
    }
}
