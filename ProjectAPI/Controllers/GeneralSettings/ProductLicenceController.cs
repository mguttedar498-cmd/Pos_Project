using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.GeneralSettings
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductLicenceController : BaseController
    {
        private readonly ILogger<UtilitySettingController> _logger;
        private readonly IProductLicence_Service _productLicenceService;

        public ProductLicenceController(ILogger<UtilitySettingController> logger, IProductLicence_Service productLicenceService)
        {
            _logger = logger;
            _productLicenceService = productLicenceService;
        }

        [HttpGet("GetProductLicenceKey")]
        public async Task<IActionResult> GetProductLicenceKey([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _productLicenceService.GetProductLicenceKey(branchcode);

            return Ok(ApiResponse<ProductLicenceModel>
                .SuccessResult(data, "Product licence key fetched successfully"));
        }

        [HttpPost("SaveProductLicenceKey")]
        public async Task<IActionResult> SaveProductLicenceKey([FromBody] InsertProductLicenceModel model)
        {
            if (model == null)
                return BadRequest(ApiResponse<string>.Failure("Invalid request data"));

            var result = await _productLicenceService.SaveProductLicenceKey(model);

            if (!result.Success)
                return Fail(result.Message);

            return Success(result.Data , "Product licence key saved successfully");


            //return Ok(ApiResponse<ProductLicenceServiceResult<int>>
            //    .SuccessResult(result, "Product licence key saved successfully"));
        }
    }
}
