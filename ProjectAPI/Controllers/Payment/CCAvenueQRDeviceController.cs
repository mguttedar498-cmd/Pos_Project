using HMS_360_PMS.API.Controllers.KOT;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.Payment.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    public class CCAvenueQRDeviceController : ControllerBase
    {
        private readonly ILogger<KOTController> _logger;
        public readonly ICCAvenueQRDevice_Service _service;

        public CCAvenueQRDeviceController(ILogger<KOTController> logger, ICCAvenueQRDevice_Service service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("SendCCAvenueRequestQRDevice")]
        public async Task<IActionResult> SendCCAvenueRequestQRDevice([FromBody] MakePaymentRequest request)
        {
            try
            {

                var data = await _service.MakePaymentAsync(request);

                if (data == null)
                    return NotFound("No Request found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendCCAvenueRequestQRDevice");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("PaymentStatusRequestCCAvenueQRDevice/{transNo}")]
        public async Task<IActionResult> PaymentStatusRequestCCAvenueQRDevice(string transNo)
        {
            try
            {
                var result = await _service.CheckTransactionStatus(transNo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentStatusRequestCCAvenueQRDevice");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[HttpGet("terminals")]
        //public async Task<IActionResult> GetTerminals()
        //{
        //    var result = await _service.FetchTerminalDetailsAsync();

        //    return Ok(result);
        //}
    }
}
