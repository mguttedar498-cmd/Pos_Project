using HMS_360_PMS.API.Controllers.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.KOT;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace HMS_360_PMS.HMS_360_PMS.API.Controllers.KOT
{
    [ApiController]
    [Route("api/[controller]")]
    public class KOTDisplayController : ControllerBase
    {
        private readonly ILogger<KOTDisplayController> _logger;
        private readonly IKOTDisplay_Service _service;

        public KOTDisplayController(IKOTDisplay_Service Service, IGraceTimeValidate_Service Graceservice, ILogger<KOTDisplayController> logger)
        {
            _service = Service;
            _logger = logger;
        }


        [HttpGet("loaditems")]
        public async Task<IActionResult> LoadItems(int flag, int depcode, string kotno, int priority, string itemcode, string tblno)
        {
            try
            {
                return flag switch
                {
                    0 => Ok(await _service.GetloadItems0()),
                    1 => Ok(await _service.GetloadItems1(depcode)),
                    2 => Ok(await _service.GetloadItems2(kotno, priority, itemcode)),
                    3 => Ok(await _service.GetloadItems3(depcode)),
                    4 => Ok(await _service.GetloadItems4(kotno, priority, tblno)),
                    5 => Ok(await _service.GetloadItems5(kotno)),
                    6 => Ok(await _service.GetloadItems6()),
                    7 => Ok(await _service.GetloadItems7(kotno, itemcode)),
                    8 => Ok(await _service.GetloadItems8(kotno, itemcode)),
                    9 => Ok(await _service.GetloadItems9(kotno, itemcode)),
                    10 => Ok(await _service.GetloadItems10(tblno)),
                    11 => Ok(await _service.GetloadItems11(depcode, tblno)),
                    12 => Ok(await _service.GetloadItems12()),
                    13 => Ok(await _service.GetloadItems13()),
                    14 => Ok(await _service.GetloadItems14(tblno)),
                    _ => BadRequest("Invalid flag")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while loading items.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("ready")]
        public async Task<IActionResult> Ready(TblKOTDisplayModel model)
        {
            try
            {
                var response = await _service.Ready(model);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the KOT display.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
