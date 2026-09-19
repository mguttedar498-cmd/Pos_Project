using ClosedXML.Excel;
using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HMS_360_PMS.ProjectAPI.Controllers.InventoryMaster
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryMasterController : BaseController
    {
        private readonly IInventoryMasterService _service;

        public InventoryMasterController(IInventoryMasterService Service)
        {
            _service = Service;
        }

        #region Supplier Master
        [HttpGet("GetSupplierList")]
        public async Task<IActionResult> GetSupplierList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetSupplierMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<InventorySupplierMaster>>
                .SuccessResult(data, "Supplier data fetched successfully"));
        }
        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplierMaster([FromBody] InventorySupplierMaster supplier)
        {
            if (supplier == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(supplier.SupName))
                return Fail("Supplier name is required");

            var id = await _service.CreateSupplierMaster(supplier);

            return CreatedAtAction(nameof(GetSupplierList), 
                new { id }, ApiResponse<int>.SuccessResult(id, "Supplier created successfully"));
        }

        [HttpPut("UpdateSupplier")]
        public async Task<IActionResult> UpdateSupplierMaster([FromBody] InventorySupplierMaster supplier)
        {
            if (supplier == null || supplier.SupCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateSupplierMaster(supplier);

            if (!success)
                return NotFoundResponse("Supplier not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteSupplier")]
        public async Task<IActionResult> DeleteSupplier([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteSupplierMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Supplier not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region Category Master

        [HttpGet("GetInventoryCategoryMasterList")]
        public async Task<IActionResult> GetCategoryMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetCategoryMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<InventoryCategoryMaster>>
                .SuccessResult(data, "Category Masters fetched successfully"));
        }

        [HttpPost("CreateInventoryCategoryMaster")]
        public async Task<IActionResult> CreateCategoryMaster([FromBody] InventoryCategoryMaster categoryMaster)
        {
            if (categoryMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(categoryMaster.CatName))
                return Fail("Category Name is required");

            var id = await _service.CreateCategoryMaster(categoryMaster);

            return CreatedAtAction(nameof(GetCategoryMasterList), new { id },
                ApiResponse<int>.SuccessResult(id, "Category created successfully"));
        }

        [HttpPut("UpdateInventoryCategoryMaster")]
        public async Task<IActionResult> UpdateCategoryMaster([FromBody] InventoryCategoryMaster categoryMaster)
        {
            if (categoryMaster == null || categoryMaster.CatCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateCategoryMaster(categoryMaster);

            if (!success)
                return NotFoundResponse("Category not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventoryCategoryMaster")]
        public async Task<IActionResult> DeleteCategoryMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteCategoryMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Category not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region Sub Category Master
        [HttpGet("GetInventorySubCategoryMasterList")]
        public async Task<IActionResult> GetSubCategoryMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetSubCategoryMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<InventorySubCategoryMaster>>
                .SuccessResult(data, "SubCategory Masters fetched successfully"));
        }

        [HttpPost("CreateInventorySubCategoryMaster")]
        public async Task<IActionResult> CreateSubCategoryMaster([FromBody] InventorySubCategoryMaster subCategoryMaster)
        {
            if (subCategoryMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(subCategoryMaster.SubCatName))
                return Fail("SubCategory Name is required");

            var id = await _service.CreateSubCategoryMaster(subCategoryMaster);

            return CreatedAtAction(nameof(GetSubCategoryMasterList), new { id }, 
                ApiResponse<int>.SuccessResult(id, "SubCategory created successfully"));
        }

        [HttpPut("UpdateInventorySubCategoryMaster")]
        public async Task<IActionResult> UpdateSubCategoryMaster([FromBody] InventorySubCategoryMaster subCategoryMaster)
        
        {
            if (subCategoryMaster == null || subCategoryMaster.SubCatCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateSubCategoryMaster(subCategoryMaster);

            if (!success)
                return NotFoundResponse("SubCategory not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventorySubCategoryMaster")]
        public async Task<IActionResult> DeleteSubCategoryMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteSubCategoryMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("SubCategory not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region Store Master
        [HttpGet("GetInventoryStoreMasterList")]
        public async Task<IActionResult> GetInventoryStoreMasterList([FromQuery] string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetInventoryStoreMaster(branch);

            return Ok(ApiResponse<IEnumerable<InventoryStoreMaster>>
                .SuccessResult(data, "SubCategory Masters fetched successfully"));
        }

        [HttpPost("CreateInventoryStoreMaster")]
        public async Task<IActionResult> CreateInventoryStoreMaster([FromBody] InventoryStoreMaster inventorystore)
        {
            if (inventorystore == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(inventorystore.StoreName))
                return Fail("Inventory Store Name is required");

            var id = await _service.CreateInventoryStoreMaster(inventorystore);

            return CreatedAtAction(nameof(GetSubCategoryMasterList), new { id },
                ApiResponse<int>.SuccessResult(id, "Inventory Store created successfully"));
        }

        [HttpPut("UpdateInventoryStoreMaster")]
        public async Task<IActionResult> UpdateInventoryStoreMaster([FromBody] InventoryStoreMaster inventorystore)
        {
            if (inventorystore == null || inventorystore.StoreId <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateInventoryStoreMaster(inventorystore);

            if (!success)
                return NotFoundResponse("Inventory Store not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventoryStoreMaster")]
        public async Task<IActionResult> DeleteInventoryStoreMaster([FromQuery] int storeId, [FromQuery] string branch)
        {
            if (storeId <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteInventoryStoreMaster(storeId, branch);

            if (!success)
                return NotFoundResponse("Inventory Store not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region Item Master

        [HttpGet("InventoryItemStoreGetList")]
        public async Task<IActionResult> GetItemMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetItemMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<InventoryMasterItemModel>>
                .SuccessResult(data, "Item Masters fetched successfully"));
        }

        [HttpPost("InventoryItemStoreCreate")]
        public async Task<IActionResult> CreateItemMaster([FromBody] InventoryMasterItemModel itemMaster)
        {
            if (itemMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(itemMaster.ItemName))
                return Fail("Item Name is required");

            var result = await _service.CreateItemMaster(itemMaster);

            if (!result.Success)
                return Fail(result.Message);

            return CreatedAtAction(nameof(GetItemMasterList), new { id = result.Data }, 
                ApiResponse<int>.SuccessResult(result.Data.Value, result.Message));
        }

        [HttpPut("InventoryItemStoreUpdate")]
        public async Task<IActionResult> UpdateItemMaster([FromBody] InventoryMasterItemModel itemMaster)
        {
            if (itemMaster == null || itemMaster.ItemCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateItemMaster(itemMaster);

            if (!success)
                return NotFoundResponse("Item not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("InventoryItemStoreDelete")]
        public async Task<IActionResult> DeleteItemMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteItemMaster(id, branchcode);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Deleted successfully");
        }

        [HttpPost("CreateInventoryItemMasterWithImage")]
        public async Task<IActionResult> CreateItemMasterWithImage([FromForm] ProductCreate imagedto)
        {
            if (imagedto.Image == null || imagedto.Image.Length == 0)
                return Fail("Image is required");

            var extension = Path.GetExtension(imagedto.Image.FileName).ToLower();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                return Fail("Only image files are allowed");

            var fileName = $"{Guid.NewGuid()}{extension}";

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "InventoryImages");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagedto.Image.CopyToAsync(stream);
            }
            return Success(fileName, "Image uploaded successfully");
        }
        #endregion

        #region Misc Master
        [HttpGet("GetInventoryMiscList")]
        public async Task<IActionResult> GetInventoryMiscList([FromQuery] string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetInventoryMiscList(branch);

            return Ok(ApiResponse<IEnumerable<MiscellaneousInventory>>
                .SuccessResult(data, "Inventory Miscellaneousfetched successfully"));
        }

        [HttpPost("CreateInventoryMisc")]
        public async Task<IActionResult> CreateInventoryMisc([FromBody] MiscellaneousInventory misc)
        {
            if (misc == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var id = await _service.CreateInventoryMisc(misc);

            return CreatedAtAction(nameof(GetInventoryMiscList), new { id },
                ApiResponse<int>.SuccessResult(id, "Inventory Miscellaneous created successfully"));
        }

        [HttpPut("UpdateInventoryMisc")]
        public async Task<IActionResult> UpdateInventoryMisc([FromBody] MiscellaneousInventory misc)
        {
            if (misc == null || misc.ChargeId <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateInventoryMisc(misc);

            if (!success)
                return NotFoundResponse("Inventory Miscellaneous not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventoryMisc")]
        public async Task<IActionResult> DeleteInventoryMisc([FromQuery] int chargeId, [FromQuery] string branch)
        {
            if (chargeId <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteInventoryMisc(chargeId, branch);

            if (!success)
                return NotFoundResponse("Inventory Miscellaneous not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region GRN Misc Master
        [HttpGet("GetInventoryGRNMiscList")]
        public async Task<IActionResult> GetInventoryGRNMiscList([FromQuery] string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetInventoryGRNMiscList(branch);

            return Ok(ApiResponse<IEnumerable<GRNMiscellaneousInventory>>
                .SuccessResult(data, "Inventory GRN Miscellaneous fetched successfully"));
        }

        [HttpPost("CreateInventoryGRNMisc")]
        public async Task<IActionResult> CreateInventoryGRNMisc([FromBody] GRNMiscellaneousInventory grnMisc)
        {
            if (grnMisc == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var id = await _service.CreateInventoryGRNMisc(grnMisc);

            return CreatedAtAction(nameof(GetInventoryGRNMiscList), new { id },
                ApiResponse<int>.SuccessResult(id, "Inventory GRN Miscellaneous created successfully"));
        }

        [HttpPut("UpdateInventoryGRNMisc")]
        public async Task<IActionResult> UpdateInventoryGRNMisc([FromBody] GRNMiscellaneousInventory grnMisc)
        {
            if (grnMisc == null || grnMisc.GRNId <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateInventoryGRNMisc(grnMisc);

            if (!success)
                return NotFoundResponse("Inventory GRN Miscellaneous not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventoryGRNMisc")]
        public async Task<IActionResult> DeleteInventoryGRNMisc([FromQuery] int GRNId, [FromQuery] string branch)
        {
            if (GRNId <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteInventoryGRNMisc(GRNId, branch);

            if (!success)
                return NotFoundResponse("Inventory GRN Miscellaneous not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region Inventory Master Import/Export
        [HttpGet("DownloadExcelInventoryItemMaster")]
        public async Task<IActionResult> DownloadExcel(string BranchCode)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ItemImport");

            string[] columns = {
                     "ItemCode", "ItemName", "Category", "SubCategory", "Group",
                     "Unit", "ItemRate", "Tax", "NoofUnits","PurchaseRate",
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
                "InventoryItemImport.xlsx");
        }

        [HttpPost("UploadFromExcelInventoryItemMaster")]
        public async Task<IActionResult> Upload(IFormFile file, string BranchCode)
        {
            var result = await _service.ValidateFileAsync(file, BranchCode);

            return Ok(result);
        }

        [HttpPost("ImportFromExcelInventoryItemMaster")]
        public async Task<IActionResult> Import(InventoryImportItemRequest rowsitems)
        {
            var result = await _service.ImportItemsAsync(rowsitems.Items, rowsitems.UserCode, rowsitems.BranchCode, rowsitems.Storeids);
            return Ok(result);
        }
        #endregion

        #region Unit Conversion Master
        [HttpGet("GetInventoryUnitConversionList")]
        public async Task<IActionResult> GetUnitConversionList([FromQuery] string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetUnitConversionList(branch);

            return Ok(ApiResponse<IEnumerable<UnitConversionMaster>>
                .SuccessResult(data, "Unit Conversion fetched successfully"));
        }


        [HttpPost("CreateInventoryUnitConversion")]
        public async Task<IActionResult> CreateUnitConversion([FromBody] UnitConversionMaster model)
        {
            if (model == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var id = await _service.CreateUnitConversion(model);

            return Ok(ApiResponse<int>
                .SuccessResult(id, "Unit Conversion created successfully"));
        }


        [HttpPut("UpdateInventoryUnitConversion")]
        public async Task<IActionResult> UpdateUnitConversion([FromBody] UnitConversionMaster model)
        {
            if (model == null || model.UnitCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateUnitConversion(model);

            if (!success)
                return NotFoundResponse("Unit Conversion not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteInventoryUnitConversion")]
        public async Task<IActionResult> DeleteUnitConversion([FromQuery] int UnitCode, [FromQuery] string branch)
        {
            if (UnitCode <= 0)
                return Fail("Invalid UnitCode");

            if (string.IsNullOrWhiteSpace(branch))
                return Fail("Branch code is required");

            var success = await _service.DeleteUnitConversion(UnitCode, branch);

            if (!success)
                return NotFoundResponse("Unit Conversion not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region Terms And Conditions Master

        [HttpGet("GetTermsAndConditionsList")]
        public async Task<IActionResult> GetTermsAndConditionsList([FromQuery] string branch)
        {
            if (string.IsNullOrWhiteSpace(branch))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetTermsAndConditionsList(branch);

            return Ok(ApiResponse<IEnumerable<TermsAndConditionsMaster>>
                .SuccessResult(data, "Terms and Conditions fetched successfully"));
        }


        [HttpPost("CreateTermsAndConditions")]
        public async Task<IActionResult> CreateTermsAndConditions([FromBody] TermsAndConditionsMaster model)
        {
            if (model == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var id = await _service.CreateTermsAndConditions(model);

            return Ok(ApiResponse<int>
                .SuccessResult(id, "Terms and Conditions created successfully"));
        }


        [HttpPut("UpdateTermsAndConditions")]
        public async Task<IActionResult> UpdateTermsAndConditions([FromBody] TermsAndConditionsMaster model)
        {
            if (model == null || model.TermsCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateTermsAndConditions(model);

            if (!success)
                return NotFoundResponse("Terms and Conditions not found");

            return Success(true, "Updated successfully");
        }


        [HttpDelete("DeleteTermsAndConditions")]
        public async Task<IActionResult> DeleteTermsAndConditions([FromQuery] int TermsCode,[FromQuery] string branch)
        {
            if (TermsCode <= 0)
                return Fail("Invalid TermsCode");

            if (string.IsNullOrWhiteSpace(branch))
                return Fail("Branch code is required");

            var success = await _service.DeleteTermsAndConditions(TermsCode, branch);

            if (!success)
                return NotFoundResponse("Terms and Conditions not found");

            return Success(true, "Deleted successfully");
        }
        #endregion
    }
}