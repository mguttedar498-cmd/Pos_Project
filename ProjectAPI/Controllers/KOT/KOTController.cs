using Azure.Core;
using HMS_360_PMS.API.Controllers.POS;
using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using HMS_360_PMS.Services_Layers.KOT.Interface;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace HMS_360_PMS.API.Controllers.KOT
{

    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class KOTController : ControllerBase
    {
        private readonly ILogger<KOTController> _logger;
        private readonly IKOT_Service _service;
        private readonly IGeneralManager_Service _generalManagerService;

        public KOTController(IKOT_Service Service, IGeneralManager_Service generalManagerService, ILogger<KOTController> logger)
        {
            _service = Service;
            _logger = logger;
            _generalManagerService = generalManagerService;
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("Dayopen")]
        public async Task<IActionResult> DayOpen([FromBody] OpenDayRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            var result = await _service.DayOpenAsync(request);

            if (!result.Success)
                return StatusCode(500, result);

            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("Dayclose")]
        public async Task<IActionResult> DayClose([FromBody] CloseDayRequest request)
        {
            var result = await _service.DayCloseAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("GetOpenDayDetais")]
        public async Task<IActionResult> GetOpenDayDetais(int userid, string branchCode)
        {
            try
            {
                var result = await _service.GetOpenDayDetais(userid, branchCode);
                if (result == null)
                    return NotFound($"No response list ");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");

            }
        }

        #region Scanner Controllers

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getfoodcategories")]
        public async Task<IActionResult> GetFoodCategories([FromQuery] string Branchcode)
        {
            try
            {
                var data = await _service.GetFoodCategories(Branchcode);
                if (data == null || !data.Any())
                    return NotFound($"No ItemCategory list");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getfoodsimage")]
        public async Task<IActionResult> GetFoodsinImage(int outlet, int category, string filter, string Branchcode)
        {
            try
            {
                var response = await _service.GetFoodsinImage(outlet, category, filter, Branchcode);
                if (response == null)
                    return NotFound($"No response list ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("Submitorder")]
        public async Task<IActionResult> Submitorder([FromBody] KOTCartModel request)
        {
            try
            {
                var response = await _service.Submitorder(request);
                if (response)
                    return Ok(response);
                else
                    return BadRequest("Order failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // IMPORTANT
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getstewards")]
        public async Task<IActionResult> GetStewards(string BranchCode)
        {
            try
            {
                var data = await _service.GetStewards(BranchCode);
                if (data == null || !data.Any())
                    return NotFound($"No Steward found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetOutletsforUser")]
        public async Task<IActionResult> GetOutletsforUser(string username)
        {
            try
            {
                var data = await _service.GetOutletsforUser(username);
                if (data == null || !data.Any())
                    return NotFound($"No Outlet found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getoldcartforscanner")]
        public async Task<IActionResult> Getoldcartforscanner(string tableno, string outlet, string subtable, string branchCode)
        {
            try
            {
                var data = await _service.Getoldcartforscanner(tableno, outlet, subtable, branchCode);
                if (data == null)
                    return NotFound($"No cart found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetBranch")]
        public async Task<ActionResult> GetBranch()
        {
            try
            {
                var result = await _service.GetBranch();
                if (result == null || !result.Any())
                    return NotFound($"No branches found ");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetMenuListByOlt")]
        public async Task<IActionResult> GetMenuListByOlt(int oltCode, string branchCode)
        {
            try
            {
                var data = await _service.GetMenuListByolt(oltCode, branchCode);
                if (data == null || !data.Any())
                    return NotFound($"No menu found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetOutletType")]
        public async Task<IActionResult> GetOutletType(string oltCode, string branchCode)
        {
            try
            {
                var data = await _service.GetOutletTypebyId(oltCode, branchCode);
                if (data == null)
                    return NotFound($"No Outlet found");

                return Ok(data);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("PGCreatePayment")]
        public async Task<IActionResult> PGCreatePayment(int Amount, string RedirectURL)
        {
            try
            {
                var result = await _service.CreatePaymentAsync(Amount, RedirectURL);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("PGPaymentStatus")]
        public async Task<IActionResult> PGPaymentStatus(string MerchantOrderId)
        {
            try
            {
                var result = await _service.GetPaymentStatusAsync(MerchantOrderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("KotGetBill")]
        public async Task<IActionResult> GetBill([FromBody] KOTCartModel cart)
        {
            try
            {
                var response = await _service.GetBill(cart);
                if (response == null)
                    return NotFound($"No response list for GetBill ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("KotPostbill")]
        public async Task<IActionResult> PostBill([FromBody] KOTBillModel bill)
        {
            try
            {
                //var response = await _service.PostBill(bill);
                //if (response == null)
                //    return NotFound($"No response list for PostBill ");
                //return Ok(response);

                var response = await _service.PostBill(bill);
                if (response)
                    return Ok(response);
                else
                    return BadRequest("Order failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("SubmitOrderfastfoodbill")]
        public async Task<IActionResult> SubmitOrderfastfoodbill([FromBody] KOTBillModel bill)
        {
            try
            {
                //var result = await _service.SubmitOrderFastFoodBill(bill);
                //return Ok(result);

                var response = await _service.SubmitOrderFastFoodBill(bill);
                if (response)
                    return Ok(response);
                else
                    return BadRequest("Order failed");
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getroomsevicedetails")]
        public async Task<IActionResult> GetRoomserviceDetails(string roomno)
        {
            try
            {
                var Roomdetails = await _service.GetRoomserviceDetails(roomno);
                if (Roomdetails == null)
                    return NotFound($"No Room Details");

                return Ok(Roomdetails);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getcompanyinfobill")]
        public async Task<IActionResult> GetCompanyinfoBill()
        {
            try
            {
                var companyInfo = await _service.GetCompanyinfoBill();
                if (companyInfo == null)
                    return NotFound($"No company info found for company code");

                return Ok(companyInfo);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getbillnouseorderid")]
        public async Task<IActionResult> Getbillnouseorderid(string OrderId, int Oltcode, string Branchcode)
        {
            try
            {
                if (string.IsNullOrEmpty(OrderId))
                    return BadRequest("Invalid Orderno number");

                var statusResponse = await _service.Getbillnowithorderid(OrderId, Oltcode, Branchcode);

                return Ok(statusResponse);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        #endregion

        #region WhatsAppConfig

        [HttpGet("WhatsAppConfig")]
        public async Task<IActionResult> WhatsAppConfig()
        {
            try
            {
                var result = await _service.WhatsappConfiguration();

                if (result == null)
                    return NotFound("Configuration not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion WhatsAppConfig

        #region QRPayment Controllers

        [HttpGet("GetOtpNo")]
        public async Task<IActionResult> GenerateingOtp(string MobileNo)
        {
            try
            {
                var response = await _generalManagerService.GenerateingOtp(MobileNo);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        #endregion

        #region OnlinePaymentType

        [HttpGet("OnlinePaymentType")]
        public async Task<IActionResult> OnlinePaymentType()
        {
            try
            {
                var result = await _service.OnlinePaymentType();

                if (result == null)
                    return NotFound("Configuration not found");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion OnlinePaymentType

        #region POS Device

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("submitOrderdirectbillnew")]
        public async Task<IActionResult> submitOrderdirectbillnew([FromBody] KOTBillModel request)
        {
            try
            {
                var response = await _service.submitOrderdirectbillnew(request);
                if (response)
                    return Ok(response);
                else
                    return BadRequest("Order failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // IMPORTANT
            }
        }
        #endregion

        #region EmailSender
        [AllowAnonymous]
        [HttpPost("EmailRequest")]
        public async Task<IActionResult> EmailRequestAsync([FromBody] EmailRequest request)
        {
            var result = await _service.EmailRequestAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion
    }
}
