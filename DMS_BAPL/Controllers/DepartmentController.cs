using DMS_BAPL_Data.DBModels;
using DMS_BAPL_Data.Services.DepartmentService;
using DMS_BAPL_Utils.Helpers;
using DMS_BAPL_Utils.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    [Route("api/department")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService _departmentServie;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentServie = departmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentMaster>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DepartmentMaster>>> Get()
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var department = await _departmentServie.Get();

                return Ok(department);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status501NotImplemented)]
        public async Task<ActionResult> Insert([FromBody] DepartmentViewModel departmentViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var department = await _departmentServie.Insert(departmentViewModel);

                return Ok(department);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DepartmentMaster>> Update(DepartmentViewModel departmentViewModel)
        {
            try
            {
                string userId = GetUserInfoFromToken.GetUserIdFromToken(HttpContext);

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not authorized");

                var department = await _departmentServie.Update(departmentViewModel);

                return Ok(department);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
