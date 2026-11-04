using DMS_BAPL_Utils.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DMS_BAPL_Api.Controllers
{
    /// <summary>
    /// Live status of a running Excel import (Vehicle Sale Import, Vehicle Dispatch Import).
    ///   GET api/import-progress/{progressId}   → phase, rows read, bills / chassis saved, batch n of m
    /// The page creates the id, sends it as options.progressId with the import request and polls this
    /// while that request is still running. Read-only, in-memory; returns 404 until the import has started.
    /// </summary>
    [Route("api/import-progress")]
    [ApiController]
    public class ImportProgressController : ControllerBase
    {
        [HttpGet("{progressId}")]
        public IActionResult Get(string progressId)
        {
            if (User?.Identity?.IsAuthenticated != true) return Unauthorized();
            var p = ImportProgress.Get(progressId);
            return p == null ? NotFound(new { success = false, message = "Not started yet." }) : Ok(p);
        }
    }
}
