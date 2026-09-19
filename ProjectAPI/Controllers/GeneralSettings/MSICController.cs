using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.GeneralSettings
{
    [Route("api/[controller]")]
    [ApiController]
    public class MSICController : BaseController
    {
        private readonly ILogger<MSICController> _logger;
        private readonly IMSIC_Services _service;

        public MSICController(ILogger<MSICController> logger, IMSIC_Services service)
        {
            _logger = logger;
            _service = service;

        }

        [HttpPost("POSMSICResetTransaction")]
        public async Task<IActionResult> POSMSICResetTransaction()
        {
            var result = await _service.POSMSICResetTransaction();

            return CreatedAtAction(nameof(POSMSICResetTransaction), new { result }, ApiResponse<bool>.SuccessResult(result, "Transaction reset successfully"));
        }

        [HttpPost("POSMSICResetTransactionFROMandTO")]
        public async Task<IActionResult> POSMSICResetTransactionFROMandTO([FromQuery] string Branchcode, [FromQuery] DateTime Fromdate, [FromQuery] DateTime Todate)
        {
            var result = await _service.POSMSICResetTransactionFROMandTO(Branchcode, Fromdate, Todate);

            return CreatedAtAction(nameof(POSMSICResetTransactionFROMandTO), new { result }, ApiResponse<bool>.SuccessResult(result, "Transaction reset successfully"));
        }

        [HttpPost("POSMSICResetMasters")]
        public async Task<IActionResult> POSMSICResetMasters()
        {
            var result = await _service.POSMSICResetMasters();

            return CreatedAtAction(nameof(POSMSICResetMasters), new { result }, ApiResponse<bool>.SuccessResult(result, "Masters reset successfully"));
        }

        [HttpPost("POSMSICTruncateAll")]
        public async Task<IActionResult> POSMSICTruncateAll()
        {
            var result = await _service.POSMSICTruncateAll();

            return CreatedAtAction(nameof(POSMSICTruncateAll), new { result }, ApiResponse<bool>.SuccessResult(result, "All data truncated successfully"));
        }

        [HttpPost("POSMSICUpdateBranch")]
        public async Task<IActionResult> POSMSICUpdateBranch([FromQuery] string Branchcode)
        {
            if (Branchcode == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");


            var result = await _service.POSMSICUpdateBranch(Branchcode);

            return CreatedAtAction(nameof(POSMSICUpdateBranch), new { result }, ApiResponse<bool>.SuccessResult(result, "Branch updated successfully"));
        }

        [HttpPost("POSMSICCreateUserMaster")]
        public async Task<IActionResult> POSMSICCreateUserMaster([FromQuery] int usercode, [FromQuery] string username, [FromQuery] string password, [FromQuery] string branchcode)
        {
            if (username == null || password == null || branchcode == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");


            var result = await _service.POSMSICCreateUserMaster(usercode, username, password, branchcode);

            return CreatedAtAction(nameof(POSMSICCreateUserMaster), new { result }, ApiResponse<bool>.SuccessResult(result, "User master created successfully"));
        }
    }
}
