using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using HMS_360_PMS.ProjectServiceLayer.Master.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace HMS_360_PMS.ProjectAPI.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : BaseController
    {
        private readonly ILogger<MasterController> _logger;
        private readonly IMaster_Service _service;

        public MasterController(ILogger<MasterController> logger, IMaster_Service service)
        {
            _logger = logger;
            _service = service;
        }

        #region CompanyMaster

        [HttpGet("GetCompanieslist")]
        public async Task<IActionResult> GetCompaniesList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetCompaniesList(branchcode);

            return Ok(ApiResponse<IEnumerable<CompaniesMaster>>
                .SuccessResult(data, "Companies fetched successfully")); 
        }

        [HttpPost("CreateCompany")]
        public async Task<IActionResult> CreateCompanyMaster([FromBody] CompaniesMaster company)
        {
            if (company == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(company.CompanyName))
                return Fail("Company name is required");

            var id = await _service.CreateCompany(company);

            return CreatedAtAction(nameof(GetCompaniesList),new { id },ApiResponse<int>.SuccessResult(id, "Company created successfully"));
        }

        [HttpPut("UpdateCompany")]
        public async Task<IActionResult> UpdateCompanyMaster([FromBody] CompaniesMaster company)
        {
            if (company == null || company.CompanyCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateCompanyMaster(company);

            if (!success)
                return NotFoundResponse("Company not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteCompany")]
        public async Task<IActionResult> DeleteCompany([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteCompany(id, branchcode);

            if (!success)
                return NotFoundResponse("Company not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region TaxMaster


        [HttpGet("GetTaxMasterlist")]
        public async Task<IActionResult> GetTaxMasterlist([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetTaxMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<BillTaxMaster>>
                .SuccessResult(data, "Tax masters fetched successfully"));
        }

        [HttpPost("CreateTaxMaster")]
        public async Task<IActionResult> CreateTaxMaster([FromBody] BillTaxMaster taxmaster)
        {
            if (taxmaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(taxmaster.TaxName))
                return Fail("Tax name is required");

            var id = await _service.CreateTaxMaster(taxmaster);

            return CreatedAtAction(nameof(GetTaxMasterlist), new { id }, ApiResponse<int>.SuccessResult(id, "Taxmaster created successfully"));
        }

        [HttpPut("UpdateTaxMaster")]
        public async Task<IActionResult> UpdateTaxMaster([FromBody] BillTaxMaster taxmaster)
        {
            if (taxmaster == null || taxmaster.TaxCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateTaxMaster(taxmaster);

            if (!success)
                return NotFoundResponse("Tax master not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteTaxMaster")]
        public async Task<IActionResult> DeleteTaxMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteTaxMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Tax master not found");

            return Success(true, "Deleted successfully");
        }


        [HttpGet("GetTaxDescriptionlist")]
        public async Task<IActionResult> GetTaxDescriptionlist([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetTaxDescriptionList(branchcode);

            return Ok(ApiResponse<IEnumerable<BillTaxDescription>>
                .SuccessResult(data, "Tax descriptions fetched successfully"));
        }

        [HttpPost("CreateTaxDescription")]
        public async Task<IActionResult> CreateTaxDescription([FromBody] BillTaxDescription taxDescription)
        {
            if (taxDescription == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(taxDescription.TaxDescription))
                return Fail("TaxDescription is required");

            var id = await _service.CreateTaxDescription(taxDescription);

            return CreatedAtAction(nameof(GetTaxDescriptionlist), new { id }, ApiResponse<int>.SuccessResult(id, "Tax Description created successfully"));
        }

        [HttpPut("UpdateTaxDescription")]
        public async Task<IActionResult> UpdateTaxDescription([FromBody] BillTaxDescription taxDescription)
        {
            if (taxDescription == null || taxDescription.TaxCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateTaxDescription(taxDescription);

            if (!success)
                return NotFoundResponse("Tax Description not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteTaxDescription")]
        public async Task<IActionResult> DeleteTaxDescription([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteTaxDescription(id, branchcode);

            if (!success)
                return NotFoundResponse("Tax Description not found");

            return Success(true, "Deleted successfully");
        }
        #endregion

        #region DepartmentMaster


        [HttpGet("GetDepartmentList")]
        public async Task<IActionResult> GetDepartmentList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetDepartmentMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<DepartmentMaster>>
                .SuccessResult(data, "Departments fetched successfully"));
        }

        [HttpPost("CreateDepartment")]
        public async Task<IActionResult> CreateDepartmentMaster([FromBody] DepartmentMaster department)
        {
            if (department == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(department.DepName))
                return Fail("Department name is required");

            var id = await _service.CreateDepartmentMaster(department);

            return CreatedAtAction(nameof(GetDepartmentList), new { id }, ApiResponse<int>.SuccessResult(id, "Department created successfully"));
        }

        [HttpPut("UpdateDepartment")]
        public async Task<IActionResult> UpdateDepartmentMaster([FromBody] DepartmentMaster department)
        {
            if (department == null || department.DepCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateDepartmentMaster(department);

            if (!success)
                return NotFoundResponse("Department not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteDepartment")]
        public async Task<IActionResult> DeleteDepartment([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteDepartmentMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Department not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region OutletMaster


        [HttpGet("GetOutletList")]
        public async Task<IActionResult> GetOutletList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetOutletMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<OutletsMaster>>
                .SuccessResult(data, "Outlets fetched successfully"));
        }

        [HttpPost("CreateOutlet")]
        public async Task<IActionResult> CreateOutletMaster([FromBody] OutletsMaster outlet)
        {
            if (outlet == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(outlet.OltName))
                return Fail("Outlet name is required");

            var id = await _service.CreateOutletMaster(outlet);

            return CreatedAtAction(nameof(GetOutletList), new { id }, ApiResponse<int>.SuccessResult(id, "Outlet created successfully"));
        }

        [HttpPut("UpdateOutlet")]
        public async Task<IActionResult> UpdateOutletMaster([FromBody] OutletsMaster outlet)
        {
            if (outlet == null || outlet.OltCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateOutletMaster(outlet);

            if (!success)
                return NotFoundResponse("Outlet not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteOutlet")]
        public async Task<IActionResult> DeleteOutlet([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteOutletMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Outlet not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region  TableMaster

        [HttpGet("GetTableMasterList")]
        public async Task<IActionResult> GetTableMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetTableMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<TablesMaster>>
                .SuccessResult(data, "Table Masters fetched successfully"));
        }

        [HttpPost("CreateTableMaster")]
        public async Task<IActionResult> CreateTableMaster([FromBody] InsertUpdateTableMaster tableMaster)
        {
            if (tableMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(tableMaster.TblNo))
                return Fail("Table number is required");

            var id = await _service.CreateTableMaster(tableMaster);

            return CreatedAtAction(nameof(GetTableMasterList), new { id }, ApiResponse<int>.SuccessResult(id, "Table created successfully"));
        }

        [HttpPut("UpdateTableMaster")]
        public async Task<IActionResult> UpdateTableMaster([FromBody] InsertUpdateTableMaster tableMaster)
        {
            if (tableMaster == null || tableMaster.TblCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateTableMaster(tableMaster);

            if (!success)
                return NotFoundResponse("Table not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteTableMaster")]
        public async Task<IActionResult> DeleteTableMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteTableMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Table not found");

            return Success(true, "Deleted successfully");
        }

        [HttpGet("TableGenerateQR")]
        public async Task<IActionResult> GenerateQR(string tbl, int olt, string branchcode)
        {
            var serverName = await _service.ServerName();
            var outletName = await _service.GetOutletName(olt, branchcode);
            if (string.IsNullOrWhiteSpace(tbl))
                return Fail("Invalid table");
            string qrText = $"{serverName}#/cin/0/tbl/{tbl}/Olt/{olt}/OtName/{Uri.EscapeDataString(outletName)}/TrNo/0/Stw/1/Qrcodes/1/guestname/0/HotelName/{Uri.EscapeDataString(branchcode)}";

            string uniqueId = Guid.NewGuid().ToString();

            var qrFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "TableQR");

            if (!Directory.Exists(qrFolder))
                Directory.CreateDirectory(qrFolder);

            string fileName = $"{uniqueId}.png";
            string filePath = Path.Combine(qrFolder, fileName);

            using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
            using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q))
            {
                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrBytes = qrCode.GetGraphic(20);

                await System.IO.File.WriteAllBytesAsync(filePath, qrBytes);
            }

            return Success(new
            {
                Id = uniqueId,
                QRText = qrText
            }, "QR generated successfully");
        }

        #endregion

        #region UnitMaster

        [HttpGet("GetUnitMasterList")]
        public async Task<IActionResult> GetUnitMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetUnitMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<UnitMaster>>
                .SuccessResult(data, "Unit Masters fetched successfully"));
        }

        [HttpPost("CreateUnitMaster")]
        public async Task<IActionResult> CreateUnitMaster([FromBody] UnitMaster unitMaster)
        {
            if (unitMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(unitMaster.UnitName))
                return Fail("Unit name is required");

            var id = await _service.CreateUnitMaster(unitMaster);

            return CreatedAtAction(nameof(GetUnitMasterList), new { id }, ApiResponse<int>.SuccessResult(id, "Unit created successfully"));
        }

        [HttpPut("UpdateUnitMaster")]
        public async Task<IActionResult> UpdateUnitMaster([FromBody] UnitMaster unitMaster)
        {
            if (unitMaster == null || unitMaster.UnitCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateUnitMaster(unitMaster);

            if (!success)
                return NotFoundResponse("Unit not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteUnitMaster")]
        public async Task<IActionResult> DeleteUnitMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteUnitMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Unit not found");

            return Success(true, "Deleted successfully");
        }


        #endregion

        #region GroupMaster

        [HttpGet("GetGroupMasterList")]
        public async Task<IActionResult> GetGroupMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetGroupMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<GroupMaster>>
                .SuccessResult(data, "Group Masters fetched successfully"));
        }

        [HttpPost("CreateGroupMaster")]
        public async Task<IActionResult> CreateGroupMaster([FromBody] GroupMaster groupMaster)
        {
            if (groupMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(groupMaster.GrpName))
                return Fail("Group Name is required");

            var id = await _service.CreateGroupMaster(groupMaster);

            return CreatedAtAction(nameof(GetGroupMasterList), new { id }, ApiResponse<int>.SuccessResult(id, "Group created successfully"));
        }

        [HttpPut("UpdateGroupMaster")]
        public async Task<IActionResult> UpdateGroupMaster([FromBody] GroupMaster groupMaster)
        {
            if (groupMaster == null || groupMaster.GrpCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateGroupMaster(groupMaster);

            if (!success)
                return NotFoundResponse("Group not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteGroupMaster")]
        public async Task<IActionResult> DeleteGroupMaster([FromQuery] int id, [FromQuery] string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var success = await _service.DeleteGroupMaster(id, branchcode);

            if (!success)
                return NotFoundResponse("Group not found");

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region CategoryMaster


        [HttpGet("GetCategoryMasterList")]
        public async Task<IActionResult> GetCategoryMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetCategoryMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<CategoryMaster>>
                .SuccessResult(data, "Category Masters fetched successfully"));
        }

        [HttpPost("CreateCategoryMaster")]
        public async Task<IActionResult> CreateCategoryMaster([FromBody] CategoryMaster categoryMaster)
        {
            if (categoryMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(categoryMaster.CatName))
                return Fail("Category Name is required");

            var id = await _service.CreateCategoryMaster(categoryMaster);

            return CreatedAtAction(nameof(GetCategoryMasterList), new { id }, ApiResponse<int>.SuccessResult(id, "Category created successfully"));
        }

        [HttpPut("UpdateCategoryMaster")]
        public async Task<IActionResult> UpdateCategoryMaster([FromBody] CategoryMaster categoryMaster)
        {
            if (categoryMaster == null || categoryMaster.CatCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateCategoryMaster(categoryMaster);

            if (!success)
                return NotFoundResponse("Category not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteCategoryMaster")]
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

        #region SubCategoryMaster


        [HttpGet("GetSubCategoryMasterList")]
        public async Task<IActionResult> GetSubCategoryMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetSubCategoryMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<SubCategoryMaster>>
                .SuccessResult(data, "SubCategory Masters fetched successfully"));
        }

        [HttpPost("CreateSubCategoryMaster")]
        public async Task<IActionResult> CreateSubCategoryMaster([FromBody] SubCategoryMaster subCategoryMaster)
        {
            if (subCategoryMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(subCategoryMaster.SubCatName))
                return Fail("SubCategory Name is required");

            var id = await _service.CreateSubCategoryMaster(subCategoryMaster);

            return CreatedAtAction(nameof(GetSubCategoryMasterList), new { id }, ApiResponse<int>.SuccessResult(id, "SubCategory created successfully"));
        }

        [HttpPut("UpdateSubCategoryMaster")]
        public async Task<IActionResult> UpdateSubCategoryMaster([FromBody] SubCategoryMaster subCategoryMaster)
        {
            if (subCategoryMaster == null || subCategoryMaster.SubCatCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateSubCategoryMaster(subCategoryMaster);

            if (!success)
                return NotFoundResponse("SubCategory not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteSubCategoryMaster")]
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

        #region ItemMaster

        [HttpGet("GetItemMasterList")]
        public async Task<IActionResult> GetItemMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetItemMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<MasterItemModel>>
                .SuccessResult(data, "Item Masters fetched successfully"));
        }

        [HttpPost("CreateItemMasterWithImage")]
        public async Task<IActionResult> CreateItemMasterWithImage([FromForm] ProductCreate imagedto)
        {
            if (imagedto.Image == null || imagedto.Image.Length == 0)
                return Fail("Image is required");

            var extension = Path.GetExtension(imagedto.Image.FileName).ToLower();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
                return Fail("Only image files are allowed");

            // Generate GUID filename
            var fileName = $"{Guid.NewGuid()}{extension}";

            // Folder path
            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Images");

            // Create folder if not exists
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Full path
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save image
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imagedto.Image.CopyToAsync(stream);
            }

            //var imageUrl = $"{Request.Scheme}://{Request.Host}/Images/{fileName}";

            //return Success(imageUrl, "Image uploaded successfully");

            // Return GUID/image name
            return Success(fileName, "Image uploaded successfully");
        }

        [HttpPost("CreateItemMaster")]
        public async Task<IActionResult> CreateItemMaster([FromBody] InsertMasterItemModel itemMaster)
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

            return CreatedAtAction(nameof(GetItemMasterList), new { id = result.Data }, ApiResponse<int>.SuccessResult(result.Data.Value, result.Message));
        }

        [HttpPut("UpdateItemMaster")]
        public async Task<IActionResult> UpdateItemMaster([FromBody] InsertMasterItemModel itemMaster)
        {
            if (itemMaster == null || itemMaster.ItemCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateItemMaster(itemMaster);

            if (!success)
                return NotFoundResponse("Item not found");

            return Success(true, "Updated successfully");
        }

        //[HttpPut("UpdateItemMaster")]
        //public async Task<IActionResult> UpdateItemMaster([FromForm] InsertMasterItemModel itemMaster)
        //{
        //    if (itemMaster == null || itemMaster.ItemCode <= 0)
        //        return Fail("Invalid data");

        //    // Upload new image if provided
        //    if (itemMaster.Image != null && itemMaster.Image.Length > 0)
        //    {
        //        var extension = Path.GetExtension(itemMaster.Image.FileName).ToLower();

        //        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

        //        if (!allowedExtensions.Contains(extension))
        //            return Fail("Only image files are allowed");

        //        // Generate unique file name
        //        var fileName = $"{Guid.NewGuid()}{extension}";

        //        // Folder path
        //        var uploadsFolder = Path.Combine(
        //            Directory.GetCurrentDirectory(),
        //            "wwwroot",
        //            "Images");

        //        // Create folder if not exists
        //        if (!Directory.Exists(uploadsFolder))
        //            Directory.CreateDirectory(uploadsFolder);

        //        var filePath = Path.Combine(uploadsFolder, fileName);

        //        // Save image
        //        using (var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await itemMaster.Image.CopyToAsync(stream);
        //        }

        //        // Save image path/url
        //        itemMaster.Thumb = $"/Images/{fileName}";
        //    }

        //    var success = await _service.UpdateItemMaster(itemMaster);

        //    if (!success)
        //        return NotFoundResponse("Item not found");

        //    return Success(true, "Updated successfully");
        //}

        [HttpDelete("DeleteItemMaster")]
        public async Task<IActionResult> DeleteItemMaster([FromQuery] int id, [FromQuery] string branchcode, List<int> oltcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteItemMaster(id, branchcode, oltcode);

            if (!result.Success)
                return Fail(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region ItemMasterTemplateDownload
        //[HttpGet("ItemMasterDownloadExcel")]
        //public async Task<IActionResult> DownloadExcel( string BranchCode)
        //{
        //    using var workbook = new XLWorkbook();
        //    var ws = workbook.Worksheets.Add("ItemImport");
        //    var lookupSheet = workbook.Worksheets.Add("Lookup");

        //    var outletdata = await _service.GetOutletMasterList(BranchCode);
        //    var outletfilterdata = outletdata .Select(o => new { o.OltCode, o.OltName }) .ToList(); 

        //    var categorydata = await _service.GetCategoryMasterList(BranchCode);
        //    var categoryfilterdata = categorydata .Select(c => new { c.CatCode, c.CatName }) .ToList(); 

        //    var SubCategorydata = await _service.GetSubCategoryMasterList(BranchCode);
        //    var SubCategoryfilterdata = SubCategorydata .Select(s => new { s.SubCatCode, s.SubCatName }) .ToList();

        //    var Groupdata = await _service.GetGroupMasterList(BranchCode);
        //    var Groupfilterdata = Groupdata .Select(g => new { g.GrpCode, g.GrpName }) .ToList();

        //    string[] columns = {
        //            "ITEMCODE", "ITEMNAME", "OUTLET", "CATEGORY", "SUBCATEGORY", "GROUP",
        //            "DEPARTMENT", "UNIT", "RATE", "TAB", "PRINT DEPT", "SACCODE"
        //            };

        //    // Header
        //    for (int i = 0; i < columns.Length; i++)
        //    {
        //        ws.Cell(1, i + 1).Value = columns[i];
        //        ws.Cell(1, i + 1).Style.Font.Bold = true;
        //    }

        //    // Empty rows
        //    for (int row = 2; row <= 201; row++)
        //    {
        //        for (int col = 1; col <= columns.Length; col++)
        //        {
        //            ws.Cell(row, col).Value = "";
        //        }
        //    }

        //    // ============================
        //    // FILL LOOKUP SHEET
        //    // ============================

        //    // Outlet Names
        //    int outletRow = 1;

        //    foreach (var item in outletfilterdata)
        //    {
        //        lookupSheet.Cell(outletRow, 1).Value = item.OltName;
        //        outletRow++;
        //    }

        //    // Category Names
        //    int categoryRow = 1;

        //    foreach (var item in categoryfilterdata)
        //    {
        //        lookupSheet.Cell(categoryRow, 2).Value = item.CatName;
        //        categoryRow++;
        //    }

        //    // SubCategory Names
        //    int subCategoryRow = 1;

        //    foreach (var item in SubCategoryfilterdata)
        //    {
        //        lookupSheet.Cell(subCategoryRow, 3).Value = item.SubCatName;
        //        subCategoryRow++;
        //    }

        //    // Group Names
        //    int groupRow = 1;

        //    foreach (var item in Groupfilterdata)
        //    {
        //        lookupSheet.Cell(groupRow, 4).Value = item.GrpName;
        //        groupRow++;
        //    }

        //    // ============================
        //    // APPLY DROPDOWNS
        //    // ============================

        //    var oltValidation = ws.Range("C2:C201").SetDataValidation();
        //    oltValidation.List($"=Lookup!$A$1:$A${outletRow - 1}");
        //    oltValidation.InCellDropdown = true;

        //    var categoryValidation = ws.Range("D2:D201").SetDataValidation();
        //    categoryValidation.List($"=Lookup!$B$1:$B${categoryRow - 1}");
        //    categoryValidation.InCellDropdown = true;

        //    var subCategoryValidation = ws.Range("E2:E201").SetDataValidation();
        //    subCategoryValidation.List($"=Lookup!$C$1:$C${subCategoryRow - 1}");
        //    subCategoryValidation.InCellDropdown = true;

        //    var groupValidation = ws.Range("F2:F201").SetDataValidation();
        //    groupValidation.List($"=Lookup!$D$1:$D${groupRow - 1}");
        //    groupValidation.InCellDropdown = true;

        //    // =========================
        //    // HIDE LOOKUP SHEET
        //    // =========================
        //    lookupSheet.Visibility = XLWorksheetVisibility.VeryHidden;

        //    // =========================
        //    // HIGHLIGHT DROPDOWN COLUMNS
        //    // =========================
        //    var dropdownRanges = new[] { "C2:C201", "D2:D201", "E2:E201", "F2:F201" };

        //    foreach (var rangeAddress in dropdownRanges)
        //    {
        //        var range = ws.Range(rangeAddress);

        //        range.Style.Fill.BackgroundColor = XLColor.LightYellow;
        //        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        //        range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        //    }


        //    // ============================
        //    // AUTO FIT COLUMNS
        //    // ============================

        //    ws.Columns().AdjustToContents();
        //    for (int i = 0; i < columns.Length; i++)
        //    {
        //        var col = ws.Column(i + 1);
        //        if (col.Width < columns[i].Length + 4)
        //        {
        //            col.Width = columns[i].Length + 4;
        //        }
        //    }

        //    using var stream = new MemoryStream();
        //    workbook.SaveAs(stream);

        //    return File
        //        (stream.ToArray(),
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "ItemImporttest.xlsx");
        //}

        [HttpGet("ItemMasterDownloadExcel")]
        public async Task<IActionResult> DownloadExcel(string BranchCode)
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("ItemImport");

            string[] columns = {
                    "ITEMCODE", "ITEMNAME", "CATEGORY", "SUBCATEGORY", "GROUP",
                    "DEPARTMENT", "UNIT", "RATE", "TAX", "PRINT DEPT", "SACCODE", "IsVEG"
                    };

            // Header
            for (int i = 0; i < columns.Length; i++)
            {
                ws.Cell(1, i + 1).Value = columns[i];
                ws.Cell(1, i + 1).Style.Font.Bold = true;
            }

            // Empty rows
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
                "ItemImport.xlsx");
        }


        #endregion

        #region ImportItemMaster

        [HttpPost("uploadItemMasterFromExcel")]
        public async Task<IActionResult> Upload(IFormFile file, string BranchCode)
        {
            var result = await _service.ValidateFileAsync(file, BranchCode);

            return Ok(result);
        }

        [HttpPost("ImportItemMasterFromExcel")]
        public async Task<IActionResult> Import(ImportItemRequest rowsitems)
        {
            var result = await _service.ImportItemsAsync(rowsitems.Items, rowsitems.UserCode, rowsitems.BranchCode, rowsitems.OltCodes);
            return Ok(result);
        }

        #endregion

        #region StewardMaster

        [HttpGet("GetStewardMasterList")]
        public async Task<IActionResult> GetStewardMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetStewardMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<MasterSteward>>
                .SuccessResult(data, "Steward Masters fetched successfully"));
        }

        [HttpPost("CreateStewardMaster")]
        public async Task<IActionResult> CreateStewardMaster([FromBody] MasterSteward stewardmaster)
        {
            if (stewardmaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(stewardmaster.StwName))
                return Fail("Steward Name is required");

            var result = await _service.CreateStewardMaster(stewardmaster);
            return CreatedAtAction(nameof(GetStewardMasterList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Steward created successfully"));
        }

        [HttpPut("UpdateStewardMaster")]
        public async Task<IActionResult> UpdateStewardMaster([FromBody] MasterSteward stewardmaster)
        {
            if (stewardmaster == null || stewardmaster.StwCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateStewardMaster(stewardmaster);

            if (!success)
                return NotFoundResponse("Steward not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteStewardMaster")]
        public async Task<IActionResult> DeleteStewardMaster([FromQuery] int id, string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteStewardMaster(id, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region NCDepartmentMaster

        [HttpGet("GetNCDepartmentMasterList")]
        public async Task<IActionResult> GetNCDepartmentMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetNCDepartmentMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<NCDepartmentMaster>>
                .SuccessResult(data, "NC Department Masters fetched successfully"));
        }

        [HttpPost("CreateNCDepartmentMaster")]
        public async Task<IActionResult> CreateNCDepartmentMaster([FromBody] NCDepartmentMaster ncdepartment)
        {
            if (ncdepartment == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(ncdepartment.NCDepName))
                return Fail("NC Department Name is required");

            var result = await _service.CreateNCDepartmentMaster(ncdepartment);
            return CreatedAtAction(nameof(GetNCDepartmentMasterList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "NC Department created successfully"));
        }

        [HttpPut("UpdateNCDepartmentMaster")]
        public async Task<IActionResult> UpdateNCDepartmentMaster([FromBody] NCDepartmentMaster ncdepartment)
        {
            if (ncdepartment == null || ncdepartment.NCDepCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateNCDepartmentMaster(ncdepartment);

            if (!success)
                return NotFoundResponse("NC Department not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteNCDepartmentMaster")]
        public async Task<IActionResult> DeleteNCDepartmentMaster([FromQuery] int id, string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteNCDepartmentMaster(id, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region PrintingMaster

        [HttpGet("GetPrintingMasterList")]
        public async Task<IActionResult> GetPrintingMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetPrintingMasterList(branchcode);

            return Ok(ApiResponse<IEnumerable<PrintingMaster>>
                .SuccessResult(data, "Printing Masters fetched successfully"));
        }

        [HttpPost("CreatePrintingMaster")]
        public async Task<IActionResult> CreatePrintingMaster([FromBody] PrintingMaster printingmaster)
        {
            if (printingmaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(printingmaster.DepName))
                return Fail("Printing Master Name is required");

            var result = await _service.CreatePrintingMaster(printingmaster);
            return CreatedAtAction(nameof(GetPrintingMasterList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Printing Master created successfully"));
        }

        [HttpPut("UpdatePrintingMaster")]
        public async Task<IActionResult> UpdatePrintingMaster([FromBody] PrintingMaster printingmaster)
        {
            if (printingmaster == null || printingmaster.DepCode <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdatePrintingMaster(printingmaster);

            if (!success)
                return NotFoundResponse("Printing Master not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeletePrintingMaster")]
        public async Task<IActionResult> DeletePrintingMaster([FromQuery] int id, string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeletePrintingMaster(id, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion


        #region OutletItemDetails

        [HttpGet("GetOutletItemList")]
        public async Task<IActionResult> GetOutletItemList([FromQuery] string branchcode, string oltcode, bool isavaliable)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _service.GetOutletItemDetailsList(branchcode, oltcode, isavaliable);

            if(!data.Any())
            return Ok(ApiResponse<IEnumerable<OutletItemDetails>>
                .SuccessResult(data, "No Outlet Item Details Found"));

            return Ok(ApiResponse<IEnumerable<OutletItemDetails>>
                .SuccessResult(data, "Outlet Item Details fetched successfully"));
        }

        [HttpPost("CreateOltItemMaster")]
        public async Task<IActionResult> CreateOltItemMaster([FromBody] CreateOutletRequest requestdetails)
        {
            if (requestdetails == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            var result = await _service.CreateOltItemMaster(requestdetails);
            return Ok(result);
        }

        [HttpPost("bulkincrement")]
        public IActionResult BulkAmountIncrement([FromBody] IncrementRequest request)
        {
            try
            {
                if (request.Items == null || request.Items.Count == 0)
                    return BadRequest("No Item in the list!!!");

                if (request.Amount.HasValue && request.Percentage.HasValue)
                    return BadRequest("Enter only one: Amount OR Percentage");

                var FilteredList = request.Items.Where(i => i.GrpCode == request.GrpCode).ToList();

                foreach (var item in FilteredList)
                {
                    if (request.Percentage.HasValue)
                    {
                        var perc = request.Percentage.Value;

                        if (perc > 0)
                        {
                            var increase = (decimal)item.OIDRate * perc / 100;
                            item.OIDRate = (double)Math.Round((decimal)item.OIDRate + increase, 0);
                        }
                        else
                        {
                            decimal value = (decimal)item.OIDRate / (Math.Abs(perc) + 100);
                            value = value * 100;
                            item.OIDRate = (double)Math.Round(value, 2);
                        }
                    }
                    else if (request.Amount.HasValue)
                    {
                        item.OIDRate += (double)request.Amount.Value;
                    }
                }

                return Ok(request.Items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        #endregion


        #region PropertyMaster

        [HttpGet("GetPropertyDetailsList")]
        public async Task<IActionResult> GetPropertyDetailsList()
        {
            var data = await _service.GetPropertyDetailsList();

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<PropertyMasterModel>>
                    .SuccessResult(data, "No Property Details Found"));
            return Ok(ApiResponse<IEnumerable<PropertyMasterModel>>
                .SuccessResult(data, "Property Details fetched successfully"));
        }

        [HttpPost("CreatePropertyDetailsMaster")]
        public async Task<IActionResult> CreatePropertyDetailsMaster([FromBody] PropertyMasterModel propertymaster)
        {
            if (propertymaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(propertymaster.Company_Name))
                return Fail("Company Name is required");

            var result = await _service.CreatePropertyDetailsMaster(propertymaster);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(CreatePropertyDetailsMaster), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Property Master created successfully"));
        }

        [HttpPut("UpdatePropertyDetailsMaster")]
        public async Task<IActionResult> UpdatePropertyDetailsMaster([FromBody] PropertyMasterModel propertymaster)
        {
            if (propertymaster == null || propertymaster.Company_code <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdatePropertyDetailsMaster(propertymaster);

            if (!success.Success)
                return NotFoundResponse("Property Details Master not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeletePropertyDetailsMaster")]
        public async Task<IActionResult> DeletePropertyDetailsMaster([FromQuery] int id)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeletePropertyDetailsMaster(id);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion

        #region  BranchMaster

        [HttpGet("GetBranchDetailsList")]
        public async Task<IActionResult> GetBranchDetailsList([FromQuery] int propertyid)
        {
            if (propertyid <= 0)
                return BadRequest(ApiResponse<string>.Failure("Property ID is required"));

            var data = await _service.GetBranchDetailsList(propertyid);

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<BranchMasterModel>>
                    .SuccessResult(data, "No Branch Details Found"));
            return Ok(ApiResponse<IEnumerable<BranchMasterModel>>
                .SuccessResult(data, "Branch Details fetched successfully"));
        }

        [HttpPost("CreateBranchDetailsMaster")]
        public async Task<IActionResult> CreateBranchDetailsMaster([FromBody] BranchMasterModel branchmaster)
        {
            if ( branchmaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(branchmaster.Branch_name))
                return Fail("Branch Name is required");

            var result = await _service.CreateBranchDetailsMaster(branchmaster);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(CreateBranchDetailsMaster), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Branch Master created successfully"));
        }

        [HttpPut("UpdateBranchDetailsMaster")]
        public async Task<IActionResult> UpdateBranchDetailsMaster([FromBody] BranchMasterModel branchmaster)
        {
            if (branchmaster == null || branchmaster.BrId <= 0)
                return Fail("Invalid data");

            var success = await _service.UpdateBranchDetailsMaster(branchmaster);

            if (!success.Success)
                return NotFoundResponse("Branch Details Master not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteBranchDetailsMaster")]
        public async Task<IActionResult> DeleteBranchDetailsMaster([FromQuery] int id)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _service.DeleteBranchDetailsMaster(id);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        #endregion


        #region AddOnItems

        [HttpGet("GetItemWiseAddOnDetailsList")]
        public async Task<IActionResult> GetItemWiseAddOnDetailsList([FromQuery] string BranchCode, [FromQuery] int Itemcode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return BadRequest(ApiResponse<string>.Failure("Branch Code is required"));

            var data = await _service.GetItemWiseAddOnDetailsList(BranchCode, Itemcode);

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<AddOnItemModel>>
                    .SuccessResult(data, "No Add-On Details Found"));
            return Ok(ApiResponse<IEnumerable<AddOnItemModel>>
                .SuccessResult(data, "Add-On Details fetched successfully"));
        }

        [HttpGet("GetAdditionalAddonDetailsList")]
        public async Task<IActionResult> GetAdditionalAddonDetailsList([FromQuery] int Itemcode, [FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return BadRequest(ApiResponse<string>.Failure("Branch Code is required"));

            var data = await _service.GetAdditionalAddonDetailsList(Itemcode, BranchCode);

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<AdditionalOnItemModel>>
                    .SuccessResult(data, "No Additional Add-On Details Found"));
            return Ok(ApiResponse<IEnumerable<AdditionalOnItemModel>>
                .SuccessResult(data, "Additional Add-On Details fetched successfully"));
        }

        [HttpPost("InsertorUpdateAddOnDetails")]
        public async Task<IActionResult> InsertorUpdateAddOnDetails([FromBody] List<AddOnItemModel> details)
        {
            if (details == null)
                return Fail("Invalid data");

            var success = await _service.InsertorUpdateAddOnDetails(details);

            if (!success.Success)
                return NotFoundResponse("Add-On Details Master not found");

            return Success(true, "Add-On Details Updated successfully");
        }

        #endregion


        #region Common Methods

        [HttpGet("GetFindnextnumber")]
        public async Task<IActionResult> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            var data = await _service.Findnextnumber(table_name, column_name, condition_name, branch);

            return Ok(ApiResponse<int>
                .SuccessResult(data, "Next Number fetched successfully"));
        }

        #endregion  
    }
}
