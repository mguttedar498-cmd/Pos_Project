using HMS_360_PMS.API.Controllers.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.Payment.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HMS_360_PMS.ProjectAPI.Controllers.Payment
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PhonePeDQRDeviceController : ControllerBase
    {
        private readonly ILogger<KOTController> _logger;
        public readonly IPhonePeDQR_Service _service;

        public PhonePeDQRDeviceController(ILogger<KOTController> logger, IPhonePeDQR_Service service)
        {
            _logger = logger;
            _service = service;
        }

        #region DQR Device Payment

        [HttpPost("SendPaymentRequestDQRDevice")]
        public async Task<IActionResult> SendPaymentRequestDQRDevice(int amount, string TransNo)
        {
            try
            {
                var data = await _service.SendPaymentRequestDQRDevice(amount, TransNo);

                if (data == null)
                    return NotFound("No ItemCategory list");

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendPaymentRequestDQRDevice");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("PaymentStatusRequestDQRDevice/{transNo}")]
        public async Task<IActionResult> PaymentStatusRequestDQRDevice(string transNo)
        {
            try
            {
                var result = await _service.SendCheckPaymentStatusRequestDQRDevice(transNo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckStatus");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion


        #region Own Device Payment
        [HttpPost("SendPaymentRequestOwnDevice")]
        public async Task<IActionResult> SendPaymentRequestOwnDevice(int Amount, string Transno)
        {
            try
            {
                if (Amount <= 0)
                    return BadRequest("Invalid request");

                var result = await _service.SendPaymentRequestOwnDevice(Amount, Transno);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("CheckOwnDevicePaymentStatus")]
        public async Task<IActionResult> CheckOwnDevicePaymentStatus(string transno)
        {
            try
            {
                if (string.IsNullOrEmpty(transno))
                    return BadRequest("Invalid transaction number");

                var statusResponse = await _service.CheckOwnDevicePaymentStatus(transno);

                return Ok(statusResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion
    }
}
