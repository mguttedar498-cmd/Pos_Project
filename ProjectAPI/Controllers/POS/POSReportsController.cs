using HMS_360_PMS.API.Controllers.POS;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectAPI.Controllers;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.HMS_360_PMS.API.Controllers.POS
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class POSReportsController : BaseController
    {
        private readonly ILogger<POSReportsController> _logger;
        private readonly IPOSReports_Service _reportservice;

        public POSReportsController(IPOSReports_Service Service,
                     ILogger<POSReportsController> logger)
        {
            _reportservice = Service;
            _logger = logger;
        }

        //[HttpGet("outlets")]
        //public async Task<ActionResult> GetOutlets()
        //{
        //    var result = await _reportservice.GetOutlets();
        //    return Ok(result);
        //}

        //[HttpGet("tables")]
        //public async Task<ActionResult> GetTables(string outlet)
        //{
        //    var result = await _reportservice.GetTables(outlet);
        //    return Ok(result);
        //}

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Dailysales")]
        public async Task<ActionResult> GetDailySales( DateTime fromdate, DateTime todate, string outlet)
        {
            try
            {
                // ✅ Validation
                if (string.IsNullOrWhiteSpace(outlet))
                    return BadRequest("Outlet is required.");

                if (fromdate > todate)
                    return BadRequest("FromDate cannot be greater than ToDate.");

                var result = await _reportservice.GetDailySales(fromdate, todate, outlet);

                //if (result == null || !result.Any())
                //    return NotFound($"No daily sales found for outlet '{outlet}'");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDailySales");
                return StatusCode(500, "An error occurred while processing your request.");

            }
        }

        //[HttpGet("item-groups")]
        //public async Task<ActionResult> GetItemGroups()
        //{
        //    var result = await _reportservice.GetItemGroups();
        //    return Ok(result);
        //}

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Itemsales")]
        public async Task<ActionResult> GetItemSales(DateTime fromdate, DateTime todate, string outlet)
        {
            var result = await _reportservice.GetItemSales(fromdate, todate, outlet);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Chancesheet")]
        public async Task<ActionResult> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode)
        {
            var result = await _reportservice.GetChancesheet(fromdate, todate, outlet, branchcode);
            return Ok(result);
        }

        //[Authorize]
        //[HttpGet("Saleschart")]
        //public async Task<ActionResult> GetSalesChart(DateTime fromdate, DateTime todate)
        //{
        //    var result = await _reportservice.GetSalesChart(fromdate, todate);
        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet("Outletsale")]
        //public async Task<ActionResult> GetOutletSale(DateTime fromdate, int oltcode)
        //{
        //    var result = await _reportservice.GetOutletSale(fromdate, oltcode);
        //    return Ok(result);
        //}

        //[Authorize]
        //[HttpGet("Itemdetail")]
        //public async Task<ActionResult> GetItemDetail(string tableNo, string oltCode)
        //{
        //    var result = await _reportservice.GetItemDetail(tableNo, oltCode);
        //    return Ok(result);
        //}

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Voidkot")]
        public async Task<ActionResult> GetVoidData(DateTime fromdate, DateTime todate, string outlet)
        {
            var result = await _reportservice.GetVoidData(fromdate, todate, outlet);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("Nckot")]
        public async Task<ActionResult> GetNCData(DateTime fromdate, DateTime todate, string outlet)
        {
            var result = await _reportservice.GetNCData(fromdate, todate, outlet);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("KotCancellation")]
        public async Task<ActionResult> GetKotCancellation([FromQuery] KotCancellationModel request)
        {
            var result = await _reportservice.GetKotCancellation(request);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("BillCancellation")]
        public async Task<ActionResult> GetBillCancellation([FromQuery] BillCancellationModel request)
        {
            var result = await _reportservice.GetBillCancellation(request);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("CreditOutstanding")]
        public async Task<ActionResult> GetCreditOutstanding([FromQuery] CreditOutstandingModel request)
        {
            var result = await _reportservice.GetCreditOutstanding(request);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("DailysaleCategorywise")]
        public async Task<ActionResult> GetDailysaleCategorywise([FromQuery] DailySaleCategorywiseModel request)
        {
            var result = await _reportservice.GetDailysaleCategorywise(request);
            return Ok(result);
        }

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("KotRegister")]
        public async Task<ActionResult> GetKotRegister([FromQuery] KOTRegisterModel request)
        {
            var result = await _reportservice.GetKotRegister(request);
            return Ok(result);
        }

        #region POS Dashboard

        //[Authorize]
        [AllowAnonymous]
        [HttpGet("GetDashboardData")]
        public async Task<IActionResult> GetDashboardData(string Branchcode)
        {
            try
            {
                var response = await _reportservice.GetDashboardData(Branchcode);
                if (response != null)
                    return Ok(response);
                else
                    return BadRequest("List fetching failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // IMPORTANT
            }
        }

        #endregion
    }
}
