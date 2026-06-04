using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Repositories.CircularDealerAssignmentRepo;
using DMS_BAPL_Utils.ViewModels;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.CircularDealerAssignmentService
{
    public partial class CircularDealerAssignmentService : ICircularDealerAssignmentService
    {
        private readonly ICircularDealerAssignmentRepo _circularDealerAssignmentRepo;

        public CircularDealerAssignmentService(ICircularDealerAssignmentRepo circularDealerAssignmentRepo)
        {
            _circularDealerAssignmentRepo = circularDealerAssignmentRepo;
        }

        Task<IEnumerable<object>> ICircularDealerAssignmentService.GetAssignmentByCircularId(int circularId) => _circularDealerAssignmentRepo.GetAssignmentByCircularId(circularId);


        Task<bool> ICircularDealerAssignmentService.AssignDealersToCircular(int circularId, List<CircularDealerAssignmentViewModel> circularDealerAssignmentViewModel)
        {
            var _assignmentList = new List<CircularDealerAssignment>();

            foreach (var item in circularDealerAssignmentViewModel)
            {
                var dealerAssignment = new CircularDealerAssignment
                {
                    CircularId = circularId,
                    DealerCode = item.DealerCode,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate
                };

                _assignmentList.Add(dealerAssignment);
            }

            return _circularDealerAssignmentRepo.AssignDealersToCircular(_assignmentList);
        }

        Task<bool> ICircularDealerAssignmentService.DeleteDealersCircularPermission(int circularId, List<CircularDealerAssignmentViewModel> circularDealerAssignmentViewModel)
        {
            var _assignmentList = new List<CircularDealerAssignment>();

            foreach (var item in circularDealerAssignmentViewModel)
            {
                var dealerAssignment = new CircularDealerAssignment
                {
                    Id = item.Id,
                    CircularId = circularId,
                    DealerCode = item.DealerCode,
                    CreatedBy = item.CreatedBy,
                    CreatedDate = item.CreatedDate
                };

                _assignmentList.Add(dealerAssignment);
            }

            return _circularDealerAssignmentRepo.DeleteDealersCircularPermission(_assignmentList);
        }
    }
}
