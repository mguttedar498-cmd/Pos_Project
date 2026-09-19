using Azure;
using Azure.Core;
using DocumentFormat.OpenXml.EMMA;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using HMS_360_PMS.Services_Layers.POS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HMS_360_PMS.API.Controllers.POS
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class POSController : ControllerBase
    {
        private readonly ILogger<POSController> _logger;
        private readonly IPOS_Services _service;
        private readonly IGraceTimeValidate_Service _graceservice;
        private const string LicenseOwner = "cogwave - 1 WebApp Lic - 1 WebServer Lic - (Ultimate Edition);poswebsite.cogwave.in";
        private const string LicenseKey = "08965AFBE8C13AFBCB90D35751B5F04EAC71012B";
        //private const string LicenseOwner = "cogwave - 1 WebApp Lic - 1 WebServer Lic - (Ultimate Edition);poswebsite.cogwave.in#qrcodeweb.cogwave.in";
        //private const string LicenseKey = "DB6D71DA4830B3BD5CDCC4BD490127EF6B820CE0";

        public POSController(IPOS_Services Service, IGraceTimeValidate_Service Graceservice, ILogger<POSController> logger)
        {
            _service = Service;
            _graceservice = Graceservice;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("GetBranchesByUser")]
        public async Task<ActionResult<IEnumerable<BranchDto>>> GetBranchesByUser(string username)
        {
            try
            {
                var result = await _service.GetBranchesByUsername(username);
                if (result == null || !result.Any())
                    return NotFound($"No branches found for user '{username}'");
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpPost("Login")]
        //public async Task<IActionResult> Login(LoginRequestDto request)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(request.Branch_code))
        //        {
        //            return BadRequest("Branch is required. Please select branch.");
        //        }

        //        if (string.IsNullOrWhiteSpace(request.Username))
        //        {
        //            return BadRequest("Username is required.");
        //        }

        //        if (string.IsNullOrWhiteSpace(request.Password))
        //        {
        //            return BadRequest("Password is required.");
        //        }
        //        CompanyInfoDto Companyresult = await _service.GetCompanyInfoByBranchCode(request.Branch_code);
        //        var result = await _service.Login(request);

        //        var billConfig = await _service.GetBillConfig(result.UserCode, result.UserName, result.Branch_code);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Unauthorized(ex.Message);
        //    }
        //}

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("BtnSubmitLogin")]
        public async Task<IActionResult> SubmitLogin(LoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Branch_code))
                {
                    return BadRequest("Branch is required. Please select branch.");
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest("Username is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Password is required.");
                }
                var response = await _service.LoginWithConfig(request);
                if (response == null)
                {
                    return NotFound($"No response for Login ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetStewardList")]
        public async Task<IActionResult> GetStewardList(string branchcode)
        {
            var username = User.Identity?.Name;
            var branchcode12 = User.FindFirst("Branch_code")?.Value;

            //if (string.IsNullOrEmpty(username))
            //    return Unauthorized("Invalid token.");

            var data = await _service.GetStewardList(branchcode);
            if (data == null || !data.Any())
                return NotFound($"No Steward found for Branchcode '{branchcode}'");

            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetSystemOutletMaster")]
        public async Task<IActionResult> GetSystemOutletMaster()
        {
            try
            {
                var response = await _service.GetSystemOutlet();

                if (response == null)
                    return NotFound($"No response list");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetCombinedOutletandtablemasterList")]
        public async Task<IActionResult> GetCombinedOutletandtablemasterList(int usercode, string branchcode)
        {
            var data = await _service.GetCombinedOutletandtablemasterList(usercode, branchcode);
            if (data == null || !data.Any())
                return NotFound($"No Combined list of OutletMaster and TableMaster for Branchcode '{branchcode}'");

            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetItemMasterList")]
        public async Task<IActionResult> GetItemMasterList(string branchcode)
        {
            var data = await _service.GetItemMasterList(branchcode);
            if (data == null || !data.Any())
                return NotFound($"No ItemMaster list for Branchcode '{branchcode}'");

            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetSpecialInfo")]
        public async Task<IActionResult> GetSpecialInfo()
        {
            var data = await _service.GetSpecialInfo();
            if (data == null || !data.Any())
                return NotFound($"No SpecialInfo list");

            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetItemCategoryList")]
        public async Task<IActionResult> GetItemCategoryList(string branchcode)
        {
            var data = await _service.GetItemCategoryList(branchcode);
            if (data == null || !data.Any())
                return NotFound($"No ItemCategory list for Branchcode '{branchcode}'");

            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetItemGroupList")]
        public async Task<IActionResult> GetItemGroupList(string branchcode)
        {
            var data = await _service.GetItemGroupList(branchcode);
            if (data == null || !data.Any())
                return NotFound($"No ItemGroup list for Branchcode '{branchcode}'");

            return Ok(data);
        }


        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetCombinedOltItemList")]
        public async Task<IActionResult> GetCombinedOltItemList(int oltcode, int grpcode, string branchcode)
        {
            var data = await _service.GetCombinedOltItemList(oltcode, grpcode, branchcode);
            //if (data == null || !data.Any())
            //    return NotFound($"No Combined list of outletitem details for Branchcode '{branchcode}'");

            //return Ok(data);
            return Ok(data ?? Enumerable.Empty<CategoryListDto>());
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("BtnSubmitposorder")]
        public async Task<IActionResult> SubmitPOSOrder([FromBody] CartModel request)
        {
            try
            {
                var response = await _service.SubmitPOSOrder(request);
                //if (response == null)
                //    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetOldCart")]
        public async Task<IActionResult> GetOldCart(string tableno, string outlet, char subtable, string branchcode)
        {
            try
            {
                var response = await _service.GetOldCart(tableno, outlet, subtable, branchcode);
                if (response == null)
                    return NotFound($"No response list for tableno");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");

            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getsubtables")]
        public async Task<IActionResult> GetSubTables(string outlet, string tableno, string branchcode)
        {
            try
            {
                var response = await _service.GetSubTables(outlet, tableno, branchcode);
                if (response == null)
                    return NotFound($"No response list for tableno ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Getnckot")]
        public async Task<IActionResult> GetNCKOT(string branchcode)
        {
            try
            {
                var response = await _service.GetNCKOT(branchcode);
                if (response == null)
                    return NotFound($"No response list for NCKot ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetTaxSettings")]
        public async Task<IActionResult> GetTaxSettings(string branchcode)
        {
            var data = await _service.GetTaxSettings(branchcode);
            if (data == null)
                return NotFound($"No TimerSettings list for Branchcode '{branchcode}'");
            return Ok(data);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetDiscountModeMaster")]
        public async Task<IActionResult> GetDiscountModeMaster(string branchcode)
        {
            try
            {
                var response = await _service.GetDiscountModeMaster(branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("GetBill")]
        public async Task<IActionResult> GetBill([FromBody] CartModel cart)
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
        [HttpPost("Postbill")]
        public async Task<IActionResult> PostBill([FromBody] BillModel bill)
        {
            try
            {
                var response = await _service.PostBill(bill);
                if (response == null)
                    return NotFound($"No response list for PostBill ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetPrinterDetails")]
        public async Task<IActionResult> GetPrinterDetails(int oltcode, string branchcode)
        {
            try
            {
                var response = await _service.GetPrinterDetails(oltcode, branchcode);
                if (response == null)
                    return NotFound($"No response list for Printdetails ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetCompanyInfo")]
        public async Task<IActionResult> GetCompanyInfo(string branchcode, int companycode)
        {
            try
            {
                var response = await _service.GetCompanyInfoByBranchCode(branchcode, companycode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetFastfoodDetails")]
        public async Task<IActionResult> GetFastfoodDetails(int outlet, string branchcode)
        {
            try
            {
                var response = await _service.GetFastfoodDetails(outlet, branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("SettleBill")]
        public async Task<IActionResult> SettleBill([FromBody] SettlementBillModel settlement)
        {
            try
            {
                var response = await _service.SettleBill(settlement, "newbill"); 
                if (response == null)
                {
                    return NotFound($"No response for Settlement Bill");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }

        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetCompanyMaster")]
        public async Task<IActionResult> GetCompanyMaster(string branchcode)
        {
            try
            {
                var response = await _service.GetCompanyMaster(branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetPaymentModeMaster")]
        public async Task<IActionResult> GetPaymentModeMaster(string branchcode)
        {
            try
            {
                var response = await _service.GetPaymentModeMaster(branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetUnbillDetails")]
        public async Task<IActionResult> GetUnbillDetails(int billno, string tblno, string outlet, string branchcode)
        {
            try
            {
                var response = await _service.GetUnbillDetails(billno, tblno, outlet, branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetUserRightsAccess")]
        public async Task<IActionResult> GetUserRightsAccess(int usercode, string username, string branchcode)
        {
            try
            {
                var response = await _service.GetPosUserAccessRight(usercode, username, branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetKotTransfertype")]
        public async Task<IActionResult> GetKotTransferTypeMaster(string branchcode)
        {
            try
            {
                var response = await _service.GetKotTransferTypeMaster(branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("Kottransfertable")]
        public async Task<IActionResult> KOTTransferTable([FromBody] KOTTransferRequest request)
        {
            try
            {
                var response = await _service.KOTTransferTable(request);

                if (!response)
                    return NotFound("Transfer failed");

                return Ok("Transfer successful");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("kot2nckot")]
        public async Task<IActionResult> KOT2NCKOT([FromBody] KOT2NCKOTRequest request)
        {
            try
            {
                var response = await _service.KOT2NCKOT(request);
                if (!response)
                    return NotFound("No records updated");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetFilteredBillDetails")]
        public async Task<IActionResult> GetFilteredBillDetails([FromQuery] KOTBillSettlementFilter filter)
        {
            try
            {
                var response = await _service.GetFilteredBillDetails(filter);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [Authorize]
        [AllowAnonymous]
        [HttpGet("GetReprintOutletMaster")]
        public async Task<IActionResult> GetReprintOutletMaster(int usercode, string branchcode)
        {
            try
            {
                var response = await _service.GetReprintOutletMaster(usercode, branchcode);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetReprintBill")]
        public async Task<IActionResult> GetReprintBill([FromQuery] GSTBillDetailModel gstdetails)
        {
            try
            {
                var response = await _service.GetReprintBill(gstdetails);
                if (response == null)
                    return NotFound($"No response list for Branchcode ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("validateday")]
        public async Task<IActionResult> ValidateDay([FromBody] DayValidationRequest request)
        {
            var result = await _graceservice.ValidateDayAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        //[HttpPost("SendPaymentRequest")]
        //public async Task<IActionResult> SendPaymentRequest(int Amount, string Transno)
        //{
        //    try
        //    {
        //        if (Amount <= 0)
        //            return BadRequest("Invalid request");

        //        var result = await _service.SendPaymentRequest(Amount, Transno);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}

        //[HttpGet("CheckPaymentStatus")]
        //public async Task<IActionResult> CheckPaymentStatus(string transno)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(transno))
        //            return BadRequest("Invalid transaction number");

        //        var statusResponse = await _service.SendCheckPaymentStatusRequest(transno);

        //        return Ok(statusResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, "An unexpected error occurred.");
        //    }
        //}

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("CancelBill")]
        public async Task<IActionResult> CancelBill([FromBody] CancelBillModel cancelbill)
        {
            try
            {
                var response = await _service.CancelBill(cancelbill);
                if (response == null)
                {
                    return NotFound($"No response list for CancelBill ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetBillDetails")]
        public async Task<ActionResult> GetBillDetails([FromQuery] CancelBillListModel request)
        {
            var result = await _service.GetBillDetails(request);
            return Ok(result);
        }


        //[Authorize]
        [AllowAnonymous]
        [HttpPost("SettlementBillModify")]
        public async Task<IActionResult> ModifySettleBill([FromBody] SettlementBillModel modifybill)
        {
            try
            {
                var response = await _service.ModifySettleBill(modifybill);
                if (response == null)
                {
                    return NotFound($"No response for SettlementBillModify ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetcompanyTransferbills")]
        public async Task<IActionResult> GetCompanyBills( string companyCode, string branchCode)
        {
            var response = await _service.GetCompanyBillsAsync( companyCode, branchCode);
            if (response == null)
            {
                return NotFound($"No response for SettlementBillModify ");
            }
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpPost("SaveCompanyBillSettlement")]
        public async Task<IActionResult> SaveCompanyBillSettlement(CompanyBillSettlementRequest request)
        {
            var response = await _service.SaveCompanyBillSettlement(request);
            if (response == null)
            {
                return NotFound($"No response for SettlementBillModify ");
            }
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetChargesDetails")]
        public async Task<ActionResult> GetChargesDetails(string branchCode)
        {
            var result = await _service.GetChargesDetails(branchCode);
            return Ok(result);
        }


        #region ModifyBill

        [AllowAnonymous]
        [HttpGet("GetModifyBillData")]
        public async Task<IActionResult> GetModifyBillData(string KOTId, int oltcode, int billno, string branchcode, DateTime settledDate)
        {
            try
            {
                var statusResponse = await _service.GetModifyBillData(KOTId, oltcode, billno, branchcode, settledDate);

                return Ok(statusResponse);
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        ////[Authorize]
        //[AllowAnonymous]
        //[HttpDelete("ModifyUnsettledBillDelete")]
        //public async Task<IActionResult> ModifyUnsettledBillDelete([FromQuery] int KSMId, [FromQuery] int oltcode, [FromQuery] int usercode, [FromQuery] string Branch)
        //{
        //    var result = await _service.UnsettledBillDelete(KSMId, oltcode, usercode, Branch);
        //    if (!result.Success)
        //        return NotFound(result.Message);

        //    return Ok(result);
        //}

        //[Authorize]
        //[AllowAnonymous]
        //[HttpDelete("ModifyUnsettledKotBillDelete")]
        //public async Task<IActionResult> ModifyUnsettledKotBillDelete([FromQuery] int KOTId, [FromQuery] int itemcode, [FromQuery] string Branch)
        //{
        //    var result = await _service.UnsettledKotBillDelete(KOTId, itemcode, Branch);

        //    if (!result.Success)
        //        return NotFound(result.Message);

        //    return Ok(result);
        //}

        [AllowAnonymous]
        [HttpPost("ModifyBillCalculation")]
        public async Task<IActionResult> ModifyBillCalculation([FromBody] CartModel request)
        {
            try
            {
                var response = await _service.ModifyBillCalculation(request);
                if (response == null)
                {
                    return NotFound($"No response for Bill Calculation on Tax");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("ModifyBillCreateUpdate")]
        public async Task<IActionResult> ModifyBillCreateUpdate([FromBody] PosModifyBillSettlement request)
        {
            try
            {
                var response = await _service.ModifyBillCreateUpdate(request);
                if (response == null)
                {
                    return NotFound($"No response for Bill Modification ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion

        #region printer Licence

        [HttpGet("jspm")]
        //public async Task<IActionResult> GetprinterLicence()
        //{
        //    //if (string.IsNullOrWhiteSpace(timestamp))
        //    //    return BadRequest("Timestamp is required.");

        //    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");


        //    using var sha256 = SHA256.Create();

        //    var hashBytes = sha256.ComputeHash(
        //        Encoding.UTF8.GetBytes(LicenseKey + timestamp));

        //    var hash = BitConverter.ToString(hashBytes)
        //        .Replace("-", "")
        //        .ToLowerInvariant();

        //    return Content($"{LicenseOwner}|{hash}", "text/plain");
        //}
        //public async Task<IActionResult> GetprinterLicence()
        //{
        //    //DO NOT MODIFY THE FOLLOWING CODE
        //    string license_hash = "";
        //    string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        //    using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
        //    {
        //        license_hash = BitConverter.ToString(sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(LicenseKey + timestamp))).Replace("-", "").ToLower();
        //    }

        //    return File(System.Text.Encoding.UTF8.GetBytes(LicenseOwner + '|' + license_hash), "text/plain");
        //}

        public async Task<IActionResult> jspm(string timestamp)
        {
            // DO NOT MODIFY THE FOLLOWING CODE
            string license_hash = "";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                license_hash = BitConverter.ToString(
                    sha256Hash.ComputeHash(
                        Encoding.UTF8.GetBytes(LicenseKey + timestamp)
                    )
                )
                .Replace("-", "")
                .ToLower();
            }

            return File(
                Encoding.UTF8.GetBytes(LicenseOwner + "|" + license_hash),
                "text/plain"
            );
        }

        #endregion

        #region Bill Adjustment

        [AllowAnonymous]
        [HttpPost("GetAdjustmentLoadData")]
        public async Task<IActionResult> GetAdjustmentLoadData([FromBody] BillAdjustmentRequest request)
        {
            try
            {
                var response = await _service.GetAdjustmentLoadData(request);
                if (response == null)
                {
                    return NotFound($"No response for Bill Modification ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetCalculateRankAmount")]
        public async Task<IActionResult> GetCalculateRankAmount( int RankId, decimal TotalAmount, string BranchCode)
        {
            try
            {
                var response = await _service.GetCalculateRankAmount( RankId, TotalAmount, BranchCode);
                if (response == null)
                {
                    return NotFound($"No response for Rank Amount Calculation ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("NewbiddingCheck")]
        public async Task<IActionResult> NewbiddingCheck(int RankId, decimal BidAmount, string BranchCode)
        {
            try
            {
                var response = await _service.NewbiddingCheck(RankId, BidAmount, BranchCode);
                if (response == null)
                {
                    return NotFound($"No response for Rank Amount Calculation ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("SaveBidChanges")]
        public async Task<IActionResult> SaveBidChanges([FromBody] SaveBidRequest request)
        {
            try
            {
                var response = await _service.SaveBidChanges(request);
                if (response == null)
                {
                    return NotFound($"No response for Rank Amount Calculation ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion

        #region POS Room Service

        [AllowAnonymous]
        [HttpGet("GetTableListForRoomService")]
        public async Task<IActionResult> GetTableListForRoomService(int Oltcode, string Branchcode)
        {
            try
            {
                var response = await _service.GetTableListForRoomService(Oltcode, Branchcode);
                if (response == null)
                {
                    return NotFound($"No response Selected Outlet ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetRoomInActive")]
        public async Task<IActionResult> GetRoomInActive(string RoomNo, int BillNo)
        {
            try
            {
                var response = await _service.GetRoomInActive(RoomNo, BillNo);
                if (response == null)
                {
                    return NotFound($"No response for Selected Bill ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion


        #region Unsettled KOT and Bill Details

        [AllowAnonymous]
        [HttpGet("GetUnsettledKOTDetails")]
        public async Task<IActionResult> GetUnsettledKOTDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            try
            {
                var response = await _service.GetUnsettledKOTDetails(fromdate, todate, Branchcode);
                if (response == null)
                {
                    return NotFound($"No response for specified parameters");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }


        [AllowAnonymous]
        [HttpPost("UpdateUnsettledKOT")]
        public async Task<IActionResult> UpdateUnsettledKOT([FromBody] List<UpdateUnsettledKOTRequest> request)
        {
            try
            {
                var response = await _service.UpdateUnsettledKOT(request);
                if (response == null)
                {
                    return NotFound($"No response for Unsettled KOT ");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpGet("GetUnsettledBillDetails")]
        public async Task<IActionResult> GetUnsettledBillDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            try
            {
                var response = await _service.GetUnsettledBillDetails(fromdate, todate, Branchcode);
                if (response == null)
                {
                    return NotFound($"No response for specified parameters");
                }
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        #endregion
    }
}