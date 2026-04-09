using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.StateRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.StateService
{
    public partial class StateService : IStateService
    {
        private readonly IStateRepo _stateRepo;
        public StateService(IStateRepo stateRepo)
        {
            _stateRepo = stateRepo;
        }
        Task<IEnumerable<State>> IStateService.GetStatesAsync() => _stateRepo.GetStatesAsync();
    }
}
