using ClosedXML.Excel;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace HMS_360_PMS.ProjectAPI.Controllers.InventoryMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryPurchaseController : BaseController
    {
        private readonly IInventoryPurchaseService _service;

        public InventoryPurchaseController(IInventoryPurchaseService Service)
        {
            _service = Service;
        }

        #region Coomon Methods
        [HttpPost("PurchaseOrderCalculation")]
        public async Task<IActionResult> PurchaseOrderCalculation([FromBody] PurchaseOrderSubmitModel purchase)
        {
            try
            {
                var response = await _service.PurchaseOrderCalculation(purchase);
                if (response == null)
                    return NotFound($"No response list for PurchaseOrderCalculation ");
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("ItemStoreGetListByStoreId")]
        public async Task<IActionResult> ItemStoreGetListByStoreId([FromQuery] string branchcode, string Storeid)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.ItemStoreGetListByStoreId(branchcode, Storeid);

            return Ok(ApiResponse<IEnumerable<InventoryMasterItemModel>>
                .SuccessResult(data, "Item Masters fetched successfully"));
        }
        #endregion

        #region Purchase Order
        [HttpPost("CreatePurchaseOrder")]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] InventoryPurchase request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (request.Master == null)
                return Fail("Purchase order master data is required");

            if (request.Details == null || request.Details.Count == 0)
                return Fail("Purchase order item details are required");

            var poNo = await _service.CreatePurchaseOrder(request);

            return CreatedAtAction(nameof(GetPurchaseOrderList),new { id = poNo },
              ApiResponse<int>.SuccessResult(poNo,"Purchase Order created successfully"));
        }

        [HttpDelete("DeletePurchaseOrder")]
        public async Task<IActionResult> DeletePurchaseOrder([FromQuery] int poNo,[FromQuery] string branchCode,
            [FromQuery] string reasonDelete)
        {
            if (poNo <= 0)
                return Fail("Invalid Purchase Order number");

            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (string.IsNullOrWhiteSpace(reasonDelete))
                return Fail("Reason for deletion is required");

            var deleted = await _service.DeletePurchaseOrder(poNo,branchCode,reasonDelete);

            if (!deleted) {
                return Fail("Purchase Order not found");
            }
            return Ok( ApiResponse<bool>.SuccessResult(true,"Purchase Order deleted successfully"));
        }

        [HttpGet("PrintPurchaseOrder")]
        public async Task<IActionResult> PrintPurchaseOrder([FromQuery] int poNo,[FromQuery] string branchCode)
        {
            if (poNo <= 0)
                return Fail("Invalid Purchase Order number");

            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderForPrint(poNo,branchCode);

            if (result == null)
               return Fail("Purchase Order not found");

            return Ok(ApiResponse<PurchaseOrderPrintResponse>.SuccessResult(result, 
                "Purchase Order print data retrieved successfully"));
        }
        #endregion

        #region Purchase Approval Order
        [HttpGet("GetPurchaseOrderList")]
        public async Task<IActionResult> GetPurchaseOrderList([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderList(branchCode);

            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.SuccessResult
                (result, "Purchase Order list retrieved successfully"));
        }

        [HttpPost("CreatePurchaseOrderApproval")]
        public async Task<IActionResult> CreatePurchaseOrderApproval([FromBody] InventoryPurchase request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Master == null)
                return Fail("Purchase order master data is required");

            if (request.Master.PONo <= 0)
                return Fail("Purchase Order number is required");

            if (string.IsNullOrWhiteSpace(request.Master.Branch_Code))
                return Fail("Branch code is required");

            if (request.Details == null || request.Details.Count == 0)
            {
                return Fail("Purchase order item details are required");
            }

            var updated = await _service.CreatePurchaseOrderApproval(request);

            if (!updated)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<bool>.SuccessResult(true, "Purchase Order Approved successfully"));
        }

        [HttpGet("GetPurchaseOrderApprovalList")]
        public async Task<IActionResult> GetPurchaseOrderApprovalList([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderApprovalList(branchCode);

            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.SuccessResult
                (result, "Purchase Order Approval list retrieved successfully"));
        }

        [HttpGet("GetPurchaseOrderApprovalPrint")]
        public async Task<IActionResult> GetPurchaseOrderApprovalPrint([FromQuery] int poNo, [FromQuery] string branchCode)
        {
            if (poNo <= 0)
                return Fail("Invalid Purchase Order number");

            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderApprovalPrint(poNo, branchCode);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<PurchaseOrderPrintResponse>.SuccessResult(result,
                "Purchase Order Approval print data retrieved successfully"));
        }
        #endregion

        #region Goods Received Note (GRN)

        [HttpGet("GetPurchaseOrderNumber")]
        public async Task<IActionResult> GetPurchaseOrderNumber([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderNumber(branchCode);

            return Ok(ApiResponse<List<PurchaseOrderPonoRequest>>.SuccessResult
                (result, "Purchase Order list retrieved successfully"));
        }

        [HttpGet("GetPurchaseOrderGRNList")]
        public async Task<IActionResult> GetPurchaseOrderGRNList([FromQuery] int poNo,[FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderGRNList(poNo,branchCode);

            return Ok(ApiResponse<List<PurchaseOrderListResponse>>.SuccessResult
                (result, "Purchase Order list retrieved successfully"));
        }

        [HttpPost("CreatePurchaseGoodsReceivedNote")]
        public async Task<IActionResult> CreatePurchaseOrderGoodsReceivedNote([FromBody] InventoryPurchaseGRN request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (request.Details == null || request.Details.Count == 0)
                return Fail("Goods Received Note item details are required");

            var result = await _service.CreatePurchaseOrderGRN(request);

            return CreatedAtAction(nameof(GetPurchaseOrderGRNList), new { id = result },
              ApiResponse<string>.SuccessResult(result, "Goods Received Note created successfully"));
        }

        [HttpGet("GetGoodsReceivedList")]
        public async Task<IActionResult> GetGoodsReceivedList([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetGoodsReceivedList(branchCode);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<GoodsReceivedNoteListResponse>>.SuccessResult(result,
                "Goods Received Note Print data retrieved successfully"));
        }

        [HttpGet("GetGoodsReceivedNotePrint")]
        public async Task<IActionResult> GetGoodsReceivedNotePrint([FromQuery] int poNo, [FromQuery] string GrnNo, [FromQuery] string branchCode)
        {
            if (poNo <= 0)
                return Fail("Invalid Purchase Order number");

            if (string.IsNullOrWhiteSpace(GrnNo) || GrnNo == "0")
                return Fail("Invalid GrnNo Order number");

            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetGoodsReceivedNotePrint(poNo, GrnNo, branchCode);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<GoodsReceivedNotePrintResponse>.SuccessResult(result,
                "Goods Received Note Print data retrieved successfully"));
        }

        [HttpDelete("DeletePurchaseOrderGRN")]
        public async Task<IActionResult> DeletePurchaseOrderGRN(string grnNo, string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode)){
                return Fail("Branch code is required");
            }
            if (string.IsNullOrWhiteSpace(grnNo) || grnNo == "0"){
                return Fail("Invalid GrnNo Order number");
            }
            var deleted = await _service.DeletePurchaseOrderGRN(grnNo,branchCode);

            if (deleted == null){
                return Fail("Goods Received Note not found");
            }
            return Ok(ApiResponse<bool>.SuccessResult
                (true,"Goods Received Note deleted successfully"));
        }

        #endregion

        #region Direct Purchase & Direct Issue
        [HttpGet("GetPurchaseOrderGRNNumber")]
        public async Task<IActionResult> GetPurchaseOrderGRNNumber([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderGRNNumber(branchCode);

            return Ok(ApiResponse<List<PurchaseOrderGRNNUmberRequest>>.SuccessResult
                (result, "Purchase Order GRN numbers retrieved successfully"));
        }

        [HttpGet("GetPurchaseGoodsReceivedList")]
        public async Task<IActionResult> GetPurchaseGoodsReceivedList([FromQuery] string branchCode, [FromQuery] string GrnNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (string.IsNullOrWhiteSpace(GrnNo) || GrnNo == "0")
                return Fail("Invalid GrnNo Order number");

            var result = await _service.GetPurchaseGoodsReceivedList(branchCode, GrnNo);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<GoodsReceivedNoteListResponse>>.SuccessResult(result,
                "Goods Received Note Print data retrieved successfully"));
        }

        [HttpGet("GetPurchaseList")]
        public async Task<IActionResult> GetPurchaseList([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseList(branchCode);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<PurchaseOrderList>>.SuccessResult(result,
                "Purchase Order data retrieved successfully"));
        }

        [HttpPost("CreateDirectPurchase")]
        public async Task<IActionResult> CreateDirectPurchase([FromBody] PurchaseSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (request.Details == null || request.Details.Count == 0)
                return Fail("Purchase details are required");

            var result = await _service.CreatePurchase(request);

            return CreatedAtAction(nameof(GetPurchaseGoodsReceivedList), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Purchase created successfully"));
        }

        [HttpDelete("DeleteDirectPurchase")]
        public async Task<IActionResult> DeleteDirectPurchase([FromQuery] int pNo, [FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode)){
                return Fail("Branch code is required");
            }
            if (pNo <= 0){
                return Fail("Invalid Purchase number");
            }
            var deleted = await _service.DeleteDirectPurchase(pNo, branchCode);

            if (!deleted){
                return Fail("Purchase not found");
            }
            return Ok(ApiResponse<bool>.SuccessResult(true, "Purchase deleted successfully"));
        }

        [HttpGet("GetPurchasePrintList")]
        public async Task<IActionResult> GetPurchasePrintList([FromQuery] string branchCode, [FromQuery] int pNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (pNo <= 0)
                return Fail("Invalid Purchase number");

            var result = await _service.GetPurchasePrintList(branchCode, pNo);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<PurchaseOrderList>>.SuccessResult(result,
                "Purchase Order data retrieved successfully"));
        }

        [HttpGet("DirectPurchaseExcelDownload")]
        public async Task<IActionResult> DirectPurchaseExcelDownload(string BranchCode)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("PurchaseOrder");

            string[] columns = {
                     "ItemCode", "ItemName", "ItemQty", "ItemRate","NoOfDays","ExpiryDate",
                    };
            for (int i = 0; i < columns.Length; i++)
            {
                ws.Cell(1, i + 1).Value = columns[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            for (int row = 2; row <= 201; row++)
            {
                for (int col = 1; col <= columns.Length; col++)
                {
                    ws.Cell(row, col).Value = "";
                }
            }

            ws.Columns().AdjustToContents();
            for (int i = 0; i < columns.Length; i++)
            {
                var col = ws.Column(i + 1);
                if (col.Width < columns[i].Length + 3)
                {
                    col.Width = columns[i].Length + 3;
                }
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return File
                (stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DirectPurchaseImport.xlsx");
        }
        #endregion

        #region Purchase Return Order

        [HttpGet("GetPurchaseOrderReturnNumber")]
        public async Task<IActionResult> GetPurchaseOrderNumberReturn([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseOrderNumberReturn(branchCode);

            return Ok(ApiResponse<List<PurchaseOrderRequestNumber>>.SuccessResult
                (result, "Purchase Order Return numbers retrieved successfully"));
        }

        [HttpGet("GetItemPurchaseOrderList")]
        public async Task<IActionResult> GetItemPurchaseOrderList([FromQuery] string branchCode, [FromQuery] int pNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (pNo <= 0)
                return Fail("Invalid Purchase number");

            var result = await _service.GetItemPurchaseOrderList(branchCode, pNo);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<PurchaseOrderReturnList>>.SuccessResult(result,
                "Purchase Return Order data retrieved successfully"));
        }

        [HttpPost("SavePurchaseReturnOrder")]
        public async Task<IActionResult> SavePurchaseReturnOrder([FromBody] PurchaseReturnSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Details == null || request.Details.Count == 0)
                return Fail("Purchase details are required");

            var result = await _service.SavePurchaseReturnOrder(request);

            return CreatedAtAction(nameof(GetPurchaseReturnOrderList), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Purchase return order saved successfully"));
        }

        [HttpGet("GetPurchaseReturnOrderList")]
        public async Task<IActionResult> GetPurchaseReturnOrderList([FromQuery] string branchCode)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            var result = await _service.GetPurchaseReturnOrderList(branchCode);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<PurchaseOrderReturnListResponse>>.SuccessResult(result,
                "Purchase Return Order data retrieved successfully"));
        }

        [HttpGet("GetPurchaseReturnOrderPrintList")]
        public async Task<IActionResult> GetPurchaseReturnOrderPrintList([FromQuery] string branchCode, [FromQuery] int PRNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (PRNo <= 0)
                return Fail("Invalid Purchase number");

            var result = await _service.GetPurchaseReturnOrderPrintList(branchCode, PRNo);

            if (result == null)
                return Fail("Purchase Order not found");

            return Ok(ApiResponse<List<PurchaseOrderReturnListResponse>>.SuccessResult(result,
                "Purchase Return Order data retrieved successfully"));
        }

        #endregion

        #region  Item Damage Entry
        [HttpPost("LoadPurchaseDetailData")]
        public async Task<IActionResult> LoadPurchaseDetailData([FromQuery] string branchCode, [FromQuery] int ItemCode)
        {
           if (ItemCode <= 0){
              return Fail("Invalid ItemCode.");
           }
           if (string.IsNullOrWhiteSpace(branchCode)){
               return Fail("Branch code is required.");
           }
           var result =await _service.LoadPurchaseDetailReturnData(branchCode, ItemCode);

           return Ok(ApiResponse<List<PurchaseDetailResponse>>.SuccessResult(
               result,"Purchase details loaded successfully"));
        }

        [HttpPost("PurchaseItemDamageSave")]
        public async Task<IActionResult> PurchaseItemDamageSave([FromBody] ItemDamageSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Details == null || request.Details.Count == 0)
                return Fail("Purchase details are required");

            var result = await _service.SaveItemDamageAsync(request);

            return CreatedAtAction(nameof(GetPurchaseReturnOrderList), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Item damage saved successfully"));
        }

        [HttpGet("GetPurchaseItemDamageList")]
        public async Task<IActionResult> GetPurchaseItemDamageList([FromQuery] string branchCode, [FromQuery] int damageNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (damageNo <= 0)
                return Fail("Invalid Damage number");

            var result = await _service.GetPurchaseOrderItemDamageList(branchCode, damageNo);

            return Ok(ApiResponse<List<ItemDamageListResponselist>>.SuccessResult(result,
                "Item damage data retrieved successfully"));
        }

        [HttpGet("GetPurchaseItemDamagePrintList")]
        public async Task<IActionResult> GetPurchaseItemDamagePrintList([FromQuery] string branchCode, [FromQuery] int damageNo)
        {
            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (damageNo <= 0)
                return Fail("Invalid Damage number");

            var result = await _service.GetPurchaseOrderItemDamageList(branchCode, damageNo);

            return Ok(ApiResponse<List<ItemDamageListResponselist>>.SuccessResult(result,
                "Item damage data retrieved successfully"));
        }
        #endregion

        #region Indent Order
        [HttpGet("GetItemDetailsIndentOrder")]
        public async Task<IActionResult?> GetItemDetailsIndentOrder([FromQuery] string branchCode, [FromQuery] string StoreId, [FromQuery] int ItemCode)
        {
            GetItemDetailsRequest request = new GetItemDetailsRequest();
            request.BranchCode = branchCode;
            request.ItemCode = ItemCode;
            request.StoreId = StoreId;

            if (string.IsNullOrWhiteSpace(branchCode))
                return Fail("Branch code is required");

            if (ItemCode <= 0)
                return Fail("Invalid Item code");

            if (string.IsNullOrWhiteSpace(StoreId))
                return Fail("Store Id is required");

            var result = await _service.GetItemDetailsIndentOrder(request);

            return Ok(ApiResponse<List<ItemDetailsResponse>>.SuccessResult(result,
                "Item details retrieved successfully"));
        }

        [HttpPost("SaveIndentOrder")]
        public async Task<IActionResult> SaveIndentOrder([FromBody] IndentOrderSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Items == null || request.Items.Count == 0)
                return Fail("Indent details are required");

            var result = await _service.SaveIndentOrderAsync(request);

            return CreatedAtAction(nameof(GetPurchaseReturnOrderList), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Indent order saved successfully"));
        }
        [HttpGet("GetIndentOrderPrintList")]
        public async Task<IActionResult?> GetIndentOrderPrintList([FromQuery] string BranchCode, [FromQuery] int IONo)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            if (IONo <= 0)
                return Fail("Invalid Indent Order No.");

            var result = await _service.GetIndentOrderListPrint(IONo, BranchCode);

            return Ok(ApiResponse<List<IndentOrderListResponse>>.SuccessResult(result,
                "Indent order list retrieved successfully"));
        }
        #endregion

        #region Indent Order Approval 
        [HttpGet("GetIndentOrderList")]
        public async Task<IActionResult?> GetIndentOrderList([FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            var result = await _service.GetIndentOrderList(BranchCode);

            return Ok(ApiResponse<List<IndentOrderListResponse>>.SuccessResult(result,
                "Indent order list retrieved successfully"));
        }

        [HttpPost("IndentOrderApprovalSave")]
        public async Task<IActionResult> IndentOrderApprovalSave([FromBody] IndentOrderApprovalSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Items == null || request.Items.Count == 0)
                return Fail("Indent details are required");

            var result = await _service.IndentOrderApprovalSave(request);

            return CreatedAtAction(nameof(GetPurchaseReturnOrderList), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Indent order approved successfully"));
        }

        [HttpGet("GetIndentOrderApprovalPrintList")]
        public async Task<IActionResult?> GetIndentOrderApprovalPrintList([FromQuery] string BranchCode, [FromQuery] int IONo)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            if (IONo <= 0)
                return Fail("Invalid Indent Order No.");

            var result = await _service.GetIndentOrderApprovalPrintList(BranchCode, IONo);

            return Ok(ApiResponse<List<IndentOrderApprovalListDto>>.SuccessResult(result,
                "Indent Approval order list retrieved successfully"));
        }
        #endregion

        #region Item Issue Order
        [HttpGet("SearchIndentOrder")]
        public async Task<IActionResult> SearchIndentOrder([FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode)){
                return Fail("Branch code is required");
            }
            var result = await _service.SearchIndentOrderAsync(BranchCode);

            return Ok(ApiResponse<List<IndentOrderSearchResponse>>.SuccessResult(result,
                "Indent order search results retrieved successfully"));
        }

        [HttpGet("GetIndentOrderApprovalData")]
        public async Task<IActionResult> GetIndentOrderApprovalData([FromQuery] string BranchCode, [FromQuery] int IONo)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
            {
                return Fail("Branch code is required");
            }
            if (IONo <= 0)
            {
                return Fail("Invalid Indent Order No.");
            }
            var result = await _service.GetIndentOrderApprovalData(BranchCode, IONo);

            return Ok(ApiResponse<List<IndentOrderApprovalListDto>>.SuccessResult(result,
                "Indent order approval data retrieved successfully"));
        }

        [HttpPost("ItemIssueSave")]
        public async Task<IActionResult> ItemIssueSave([FromBody] ItemIssueSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Items == null || request.Items.Count == 0)
                return Fail("Indent details are required");

            var result = await _service.ItemIssueSave(request);

            return CreatedAtAction(nameof(GetItemIssuePrintData), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Item issue saved successfully"));
        }

        [HttpGet("GetItemIssuePrintData")]
        public async Task<IActionResult?> GetItemIssuePrintData([FromQuery] string BranchCode, [FromQuery] int ItemissueNo)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            if (ItemissueNo <=0 )
                return Fail("Invalid Item Issue No.");

            var result = await _service.GetItemIssuePrintData(BranchCode, ItemissueNo);

            return Ok(ApiResponse<List<ItemIssueListDto>>.SuccessResult(result,
                "Item issue data retrieved successfully"));
        }
        #endregion

        #region Item Issue Return Data
        [HttpGet("GetItemIssueData")]
        public async Task<IActionResult?> GetItemIssueData([FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            var result = await _service.GetItemIssueData(BranchCode);

            return Ok(ApiResponse<List<ItemIssueListDto>>.SuccessResult(result,
                "Item issue data retrieved successfully"));
        }

        [HttpPost("ItemIssueReturnSave")]
        public async Task<IActionResult> ItemIssueReturnSave([FromBody] ItemIssueReturnSaveRequest request)
        {
            if (request == null)
                return Fail("Invalid request payload");

            if (request.Items == null || request.Items.Count == 0)
                return Fail("Indent details are required");

            var result = await _service.ItemIssueReturnSave(request);

            return CreatedAtAction(nameof(GetItemIssueReturnPrintData), new { id = result },
              ApiResponse<int>.SuccessResult(result, "Item issue returned successfully"));
        }

        [HttpGet("GetItemIssueReturnPrintData")]
        public async Task<IActionResult?> GetItemIssueReturnPrintData([FromQuery] string BranchCode, [FromQuery] int iNo)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return Fail("Branch code is required");

            if (iNo <= 0)
                return Fail("Invalid Item Issue Return No.");

            var result = await _service.GetItemIssueReturnPrintData(BranchCode, iNo);

            return Ok(ApiResponse<List<ItemIssueListDto>>.SuccessResult(result,
                "Item issue return data retrieved successfully"));
        }
        #endregion

        //#region Opening Stock
        //[HttpGet("GetOpeningStockList")]
        //public async Task<IActionResult> GetOpeningStockList([FromQuery] string BranchCode, [FromQuery] int StoreId) 
        //{ 
        //    if (string.IsNullOrWhiteSpace(BranchCode)) 
        //        return Fail("Branch code is required"); 

        //    if (StoreId <= 0) return Fail("Invalid Store Id."); 

        //    var result = await _service.GetOpeningStockListAsync(BranchCode, StoreId); 

        //    return Ok(ApiResponse<List<ItemOpeningStock>>.SuccessResult(
        //        result, "Opening stock list retrieved successfully")); 
        //}
        //[HttpPost("OpenStockDataSave")] 
        //public async Task<IActionResult> OpenStockDataSave([FromBody] ItemOpeningStock request) 
        //{ 
        //    if (request == null)
        //    {
        //        return Fail("Invalid request payload");
        //    }
        //    var result = await _service.SaveOpeningStockAsync(request); 

        //    return Ok(ApiResponse<int>.SuccessResult(result, 
        //        "Opening stock saved successfully")); 
        //}
        //[HttpPut("OpenStockDataUpdate")] 
        //public async Task<IActionResult> OpenStockDataUpdate([FromBody] ItemOpeningStock request) 
        //{ 
        //    if (request == null) return Fail("Invalid request payload"); 

        //    var result = await _service.UpdateOpeningStockAsync(request); 

        //    if (!result) return Fail("Opening stock not found or update failed."); 

        //    return Ok(ApiResponse<bool>.SuccessResult(true, "Opening stock updated successfully")); 
        //}
        //[HttpDelete("DeleteOpeningStock")] 
        //public async Task<IActionResult> DeleteOpeningStock([FromQuery] int openingStockId, [FromQuery] int modifiedBy) 
        //{ 
        //    if (openingStockId <= 0) 
        //        return Fail("Invalid Opening StockId"); 

        //    if (modifiedBy <= 0) return Fail("Invalid ModifiedBy"); 

        //    var deleted = await _service.DeleteOpeningStockAsync(openingStockId, modifiedBy); 

        //    if (!deleted) return Fail("Opening stock not found or already deleted."); 

        //    return Ok(ApiResponse<bool>.SuccessResult(true, 
        //        "Opening stock deleted successfully")); 
        //}
        //#endregion
    }
}
