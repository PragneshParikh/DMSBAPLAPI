using DMS_BAPL_Data.DBModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Repositories.LOTInspectionRepo
{
    public class LotInspection : ILotInspection
    {
        private readonly ILotInspection _lotInspection;

        public LotInspection(ILotInspection lotInspection)
        {
            _lotInspection = lotInspection;
        }

        // create insert api 
    }
}
