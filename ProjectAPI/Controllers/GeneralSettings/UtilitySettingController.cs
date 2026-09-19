using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.GeneralSettings
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilitySettingController : BaseController
    {
        private readonly ILogger<UtilitySettingController> _logger;
        private readonly IUtilitySetting_Service _service;
        public UtilitySettingController(ILogger<UtilitySettingController> logger, IUtilitySetting_Service service)
        {
            _logger = logger;
            _service = service;
        }

        #region Happy Hours Setting


        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetHappyHoursSettings")]
        public async Task<IActionResult> GetHappyHoursSettings(string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetHappyHoursSettings(branchcode);

            //if (data == null)
            //{
            //    return Ok(ApiResponse<HappyHoursModel>
            //        .SuccessResult(null, $"No HappyHoursSettings found for Branchcode '{branchcode}'"));
            //}

            return Ok(ApiResponse<HappyHoursModel>
                .SuccessResult(data, "HappyHoursSettings fetched successfully"));
        }

        [HttpPost("SaveorUpdateHappyHoursSettings")]
        public async Task<IActionResult> SaveorUpdateHappyHoursSettings([FromBody] HappyHoursModel request)
        {
            if(request == null)
                return Fail("Invalid data");

            var result = await _service.SaveorUpdateHappyHoursSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update happy hours settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region KOT Timer Setting

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetKOTTimerSettings")]
        public async Task<IActionResult> GetKOTTimerSettings(string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetKOTTimerSettings(branchcode);

            //if (data == null)
            //{
            //    return Ok(ApiResponse<TimerSettingModel>
            //        .SuccessResult(null, $"No KOTTimerSettings for Branchcode '{branchcode}'"));
            //}

            return Ok(ApiResponse<TimerSettingModel>
                .SuccessResult(data, "KOTTimerSettings fetched successfully"));
        }

        [HttpPost("SaveorUpdateKOTTimerSettings")]
        public async Task<IActionResult> SaveorUpdateKOTTimerSettings([FromBody] TimerSettingModel request)
        {
            if (request == null)
                return Fail("Invalid data");

            var result = await _service.SaveorUpdateKOTTimerSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update KOT timer settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region Financial Year Setting

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetFinancialSettings")]
        public async Task<IActionResult> GetFinancialSettings (string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetFinancialSettings(branchcode);

            //if (data == null)
            //    return Ok(ApiResponse<FinancialYearModel>
            //    .SuccessResult(null, $"No FinancialSettings for Branchcode '{branchcode}'"));

            return Ok(ApiResponse<FinancialYearModel>
                .SuccessResult(data, "FinancialSettings fetched successfully"));
        }

        [HttpPost("SaveOrUpdateFinancialSettings")]
        public async Task<IActionResult> SaveOrUpdateFinancialSettings([FromBody] FinancialYearModel request)
        {
            if (request == null)
                return Fail("Invalid data");

            var result = await _service.SaveOrUpdateFinancialSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update financial settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region TaxMode Setting

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetTaxModeSettings")]
        public async Task<IActionResult> GetTaxModeSettings(string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetTaxModeSettings(branchcode);
            //if (data == null)
            //    return NotFound($"No TaxModeSettings for Branchcode '{branchcode}'");

            return Ok(ApiResponse<IEnumerable<TaxModeSettingModel>>
                .SuccessResult(data, "TaxModeSettings fetched successfully"));
        }

        [HttpPost("UpdateTaxModeSettings")]
        public async Task<IActionResult> UpdateTaxModeSettings([FromBody] TaxModeSettingModel request)
        {
            if (request == null)
                return Fail("Invalid data");

            var result = await _service.UpdateTaxModeSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update tax mode settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region DiscountMode Setting

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetDiscountModeSettings")]
        public async Task<IActionResult> GetDiscountModeSettings(string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetDiscountModeSettings(branchcode);
            //if (data == null)
            //    return NotFound($"No DiscountModeSettings for Branchcode '{branchcode}'");

            return Ok(ApiResponse<IEnumerable<DiscountModeSetting>>
                .SuccessResult(data, "DiscountModeSettings fetched successfully"));
        }

        [HttpPost("UpdateDiscountModeSettings")]
        public async Task<IActionResult> UpdateDiscountModeSettings([FromBody] DiscountModeSetting request)
        {
            if (request == null)
                return Fail("Invalid data");

            var result = await _service.UpdateDiscountModeSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update discount mode settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region SMS Sender Setting

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetSMSSenderSettings")]
        public async Task<IActionResult> GetSMSSenderSettings(string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetSMSSenderSettings(branchcode);
            //if (data == null)
            //    return NotFound($"No SMSSenderSettings for Branchcode '{branchcode}'");

            return Ok(ApiResponse<SmsSettingModel>
                .SuccessResult(data, "SMSSenderSettings fetched successfully"));
        }

        [HttpPost("SaveOrUpdateSMSSenderSettings")]
        public async Task<IActionResult> SaveOrUpdateSMSSenderSettings([FromBody] SmsSettingModel request)
        {
            if (request == null)
                return Fail("Invalid data");

            var result = await _service.SaveOrUpdateSMSSenderSettings(request);

            if (!result)
                return NotFoundResponse("Failed to update SMS sender settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region Printer Setting

        [HttpGet("GetPrinterSettings")]
        public async Task<IActionResult> GetPrinterSettings([FromQuery] string branchcode, [FromQuery] int OltCode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetPrinterSettings(branchcode, OltCode);

            return Ok(ApiResponse<PrintConfigResponse>
                .SuccessResult(data, "Printer Setting fetched successfully"));
        }

        [HttpPost("SaveOrUpdateCatGroupSettings")]
        public async Task<IActionResult> SaveOrUpdateCatGroupSettings([FromBody] CartGroupModel GrpRequest)
        {
            if (GrpRequest == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var result = await _service.SaveOrUpdateCatGroupSettings(GrpRequest);

            return CreatedAtAction(nameof(SaveOrUpdateCatGroupSettings), new { result.Data }, ApiResponse<int>.SuccessResult(result.Data ?? 0, "Category group settings created successfully"));
        }

        [HttpPost("SaveOrUpdatePrinterSettings")]
        public async Task<IActionResult> SaveOrUpdatePrinterSettings([FromBody] PrinterConfigModel printerRequest)
        {
            if (printerRequest == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var result = await _service.SaveOrUpdatePrinterSettings(printerRequest);

            return CreatedAtAction(nameof(SaveOrUpdatePrinterSettings), new { result.Data }, ApiResponse<int>.SuccessResult(result.Data ?? 0, "Printer settings created successfully"));
        }

        [HttpDelete("DeleteCatGroupSettings")]
        public async Task<IActionResult> DeleteCatGroupSettings([FromQuery] int GrpCode,[FromQuery] int OltCode, [FromQuery] string Branchcode)
        {
            if (GrpCode <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteCatGroupSettings(GrpCode, OltCode, Branchcode);

            if (!result.Success)
                return NotFoundResponse("CatGroup settings not found");

            return Success(true, "Deleted successfully");
        }

        [HttpDelete("DeletePrinterSettings")]
        public async Task<IActionResult> DeletePrinterSettings([FromQuery] int GrpCode,[FromQuery] int OltCode, [FromQuery] int UserCode, [FromQuery] string Branchcode)
        {
            if (GrpCode <= 0)
                return Fail("Invalid id");

            var result = await _service.DeletePrinterSettings(GrpCode, OltCode, UserCode, Branchcode);

            if (!result.Success)
                return NotFoundResponse("Printer settings not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region For Bill Generaion

        [HttpGet("GetBillGenerationData")]
        public async Task<IActionResult> GetBillGenerationData(string Branch_Code)
        {
            if (string.IsNullOrWhiteSpace(Branch_Code))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var result = await _service.GetBillGenerationData(Branch_Code);
            //if (result == null)
            //    return NotFound($"No Bill Generation Data for Branchcode '{Branch_Code}'");

            return Ok(ApiResponse<IEnumerable<BillGeneration>>
                .SuccessResult(result, "Bill Generation Data fetched successfully"));
        }

        [HttpPost("BillGeneration")]
        public async Task<IActionResult> BillGeneration([FromBody] BillGeneration model)
        {
            var result = await _service.BillGeneration(model);

            if (!result)
                return NotFoundResponse("Failed to update Bill Generation Settings.");

            return Success(true, "Updated successfully");
        }

        

        #endregion

        #region KotConfiguration

        [HttpGet("GetKotConfiguration")]
        public async Task<IActionResult> GetKotConfiguration(string Branch_Code)
        {
            if (string.IsNullOrWhiteSpace(Branch_Code))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var result = await _service.GetKotConfiguration(Branch_Code);
            //if (result == null)
            //    return NotFound($"No Kot Configuration Data for Branchcode '{Branch_Code}'");

            return Ok(ApiResponse<IEnumerable<KotConfig>>
                .SuccessResult(result, "Kot Configuration Data fetched successfully"));
        }

        [HttpPost("KotConfiguration")]
        public async Task<IActionResult> KotConfiguration([FromBody] KotConfig model)
        {
            var result = await _service.KotConfiguration(model);

            if (!result)
                return NotFoundResponse("Failed to update bill generation mode settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region BillConfiguration

        [HttpGet("GetBillConfiguration")]
        public async Task<IActionResult> GetBillConfiguration(string Branch_Code)
        {
            if (string.IsNullOrWhiteSpace(Branch_Code))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var result = await _service.GetBillConfiguration(Branch_Code);
            //if (result == null)
            //    return NotFound($"No Bill Configuration Data for Branchcode '{Branch_Code}'");

            return Ok(ApiResponse<IEnumerable<SaveBillConfigModel>>
                .SuccessResult(result, "Bill Configuration Data fetched successfully"));
        }

        [HttpPost("BillConfiguration")]
        public async Task<IActionResult> BillConfiguration([FromBody] SaveBillConfigModel model)
        {
            var result = await _service.BillConfiguration(model);

            if (!result)
                return NotFoundResponse("Failed to Update Bill Configuration mode settings.");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region PosandKotDeleteDelete

        [HttpDelete("POSandKOTdataDelete")]
        public async Task<IActionResult> PosAndKotDataDelete()
        {
            var result = await _service.PosAndKotDataDelete();

            return Ok(new
            {
                message = result
            });
        }

        #endregion




        #region
        #endregion
        #region
        #endregion
        #region
        #endregion
    }
}
