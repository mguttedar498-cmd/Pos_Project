using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using System.Reflection;

namespace HMS_360_PMS.HMS_360_PMS.API.Controllers.KOT
{

    [ApiController]
    [Route("api/[controller]")]
    public class KOTPickUpController : ControllerBase
    {

        private readonly ILogger<KOTPickUpController> _logger;
        private readonly IKOTPickUp_Service _service;

        public KOTPickUpController(IKOTPickUp_Service Service, IGraceTimeValidate_Service Graceservice, ILogger<KOTPickUpController> logger)
        {
            _service = Service;
            _logger = logger;
        }

        [HttpPost("pick")]
        public async Task<IActionResult> Pick(PFoodModel food)
        {
            try
            {
                var response = await _service.Pick(food);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while picking the food.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
