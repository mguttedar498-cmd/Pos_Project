using Azure.Core;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using OfficeOpenXml;
using System.Data;
using System.Reflection.Emit;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Services
{
    public class Master_Service : IMaster_Service
    {
        private readonly IMaster_Repository _repository;
        private readonly DbConnectionFactory _factory;

        public Master_Service(IMaster_Repository repository, DbConnectionFactory factory)
        {
            _repository = repository;
            _factory = factory;
        }

        #region CompanyMaster

        public async Task<IEnumerable<CompaniesMaster>> GetCompaniesList(string branchcode)
        {
            var companylist = await _repository.GetCompaniesList(branchcode);
            return companylist;
        }

        public async Task<int> CreateCompany(CompaniesMaster company)
        {
            DateTime currentTime = ConvertUtcToIst();
            company.LastModify = currentTime;
            return await _repository.InsertCompanyDetails(company);
        }

        public async Task<bool> UpdateCompanyMaster(CompaniesMaster company)
        {
            DateTime currentTime = ConvertUtcToIst();
            company.LastModify = currentTime;
            return await _repository.UpdateCompanyDetails(company);
        }

        public async Task<bool> DeleteCompany(int id, string branchcode)
        {
            return await _repository.DeleteCompanyDetails(id, branchcode);
        }

        #endregion


        #region TaxMaster

        public async Task<IEnumerable<BillTaxMaster>> GetTaxMasterList(string branchcode)
        {
            var taxMasterList = await _repository.GetTaxMasterList(branchcode);
            return taxMasterList;
        }

        public async Task<int> CreateTaxMaster(BillTaxMaster taxmaster)
        {
            return await _repository.InsertTaxMasterDetails(taxmaster);
        }

        public async Task<bool> UpdateTaxMaster(BillTaxMaster taxmaster)
        {
            return await _repository.UpdateTaxMasterDetails(taxmaster);
        }

        public async Task<bool> DeleteTaxMaster(int id, string branchcode)
        {
            return await _repository.DeleteTaxMasterDetails(id, branchcode);
        }

        public async Task<IEnumerable<BillTaxDescription>> GetTaxDescriptionList(string branchcode)
        {
            var taxDescriptionList = await _repository.GetTaxDescriptionList(branchcode);
            return taxDescriptionList;
        }

        public async Task<int> CreateTaxDescription(BillTaxDescription taxdescription)
        {
            return await _repository.InsertTaxDescriptionDetails(taxdescription);
        }

        public async Task<bool> UpdateTaxDescription(BillTaxDescription taxdescription)
        {
            return await _repository.UpdateTaxDescriptionDetails(taxdescription);
        }

        public async Task<bool> DeleteTaxDescription(int id, string branchcode)
        {
            return await _repository.DeleteTaxDescriptionDetails(id, branchcode);
        }

        #endregion

        #region DepartmentMaster

        public async Task<IEnumerable<DepartmentMaster>> GetDepartmentMasterList(string branchcode)
        {
            var departmentList = await _repository.GetDepartmentList(branchcode);
            return departmentList;
        }

        public async Task<int> CreateDepartmentMaster(DepartmentMaster department)
        {   
            return await _repository.InsertDepartmentDetails(department);
        }

        public async Task<bool> UpdateDepartmentMaster(DepartmentMaster department)
        {
            return await _repository.UpdateDepartmentDetails(department);
        }

        public async Task<bool> DeleteDepartmentMaster(int id, string branchcode)
        {
            return await _repository.DeleteDepartmentDetails(id, branchcode);
        }

        #endregion

        #region OutletMaster

        public async Task<IEnumerable<OutletsMaster>> GetOutletMasterList(string branchcode)
        {
            var outletList = await _repository.GetOutletsList(branchcode);
            return outletList;
        }

        public async Task<int> CreateOutletMaster(OutletsMaster outlet)
        {
            return await _repository.InsertOutletDetails(outlet);
        }

        public async Task<bool> UpdateOutletMaster(OutletsMaster outlet)
        {
            return await _repository.UpdateOutletDetails(outlet);
        }

        public async Task<bool> DeleteOutletMaster(int id, string branchcode)
        {
            return await _repository.DeleteOutletDetails(id, branchcode);
        }

        #endregion

        #region TableMaster

        public async Task<IEnumerable<TablesMaster>> GetTableMasterList(string branchcode)
        {
            var tableList = await _repository.GetTablesList(branchcode);
            return tableList;
        }

        public async Task<int> CreateTableMaster(InsertUpdateTableMaster table)
        {
            return await _repository.InsertTableDetails(table);
        }

        public async Task<bool> UpdateTableMaster(InsertUpdateTableMaster table)
        {
            return await _repository.UpdateTableDetails(table);
        }

        public async Task<bool> DeleteTableMaster(int id, string branchcode)
        {
            return await _repository.DeleteTableDetails(id, branchcode);
        }

        public async Task<string> ServerName()
        {
            return await _repository.ServerName();
        }

        public async Task<string> GetOutletName(int outlet, string branchcode)
        {
            return await _repository.GetOutletName(outlet, branchcode);
        }


        #endregion

        #region UserMaster

        public async Task<IEnumerable<UnitMaster>> GetUnitMasterList(string branchcode)
        {
            var unitList = await _repository.GetUnitList(branchcode);
            return unitList;
        }

        public async Task<int> CreateUnitMaster(UnitMaster unit)
        {
            return await _repository.InsertUnitDetails(unit);
        }

        public async Task<bool> UpdateUnitMaster(UnitMaster unit)
        {
            return await _repository.UpdateUnitDetails(unit);
        }

        public async Task<bool> DeleteUnitMaster(int id, string branchcode)
        {
            return await _repository.DeleteUnitDetails(id, branchcode);
        }

        #endregion

        #region GroupMaster

        public async Task<IEnumerable<GroupMaster>> GetGroupMasterList(string branchcode)
        {
            var groupList = await _repository.GetGroupList(branchcode);
            return groupList;
        }

        public async Task<int> CreateGroupMaster(GroupMaster group)
        {
            return await _repository.InsertGroupDetails(group);
        }

        public async Task<bool> UpdateGroupMaster(GroupMaster group)
        {
            return await _repository.UpdateGroupDetails(group);
        }

        public async Task<bool> DeleteGroupMaster(int id, string branchcode)
        {
            return await _repository.DeleteGroupDetails(id, branchcode);
        }

        #endregion

        #region CategoryMaster

        public async Task<IEnumerable<CategoryMaster>> GetCategoryMasterList(string branchcode)
        {
            var categoryList = await _repository.GetCategoryList(branchcode);
            return categoryList;
        }

        public async Task<int> CreateCategoryMaster(CategoryMaster category)
        {
            if (category.SubCat == "Restaurant")
            {
                category.SubCat = "0";
            }
            else
            {
                category.SubCat = "1";
            }
            return await _repository.InsertCategoryDetails(category);
        }

        public async Task<bool> UpdateCategoryMaster(CategoryMaster category)
        {
            return await _repository.UpdateCategoryDetails(category);
        }

        public async Task<bool> DeleteCategoryMaster(int id, string branchcode)
        {
            return await _repository.DeleteCategoryDetails(id, branchcode);
        }

        #endregion

        #region SubCategoryMaster

        public async Task<IEnumerable<SubCategoryMaster>> GetSubCategoryMasterList(string branchcode)
        {
            var subCategoryList = await _repository.GetSubCategoryList(branchcode);
            return subCategoryList;
        }

        public async Task<int> CreateSubCategoryMaster(SubCategoryMaster subCategory)
        {
            if (subCategory.SubCat == "Restaurant")
            {
                subCategory.SubCat = "0";
            }
            else
            {
                subCategory.SubCat = "1";
            }

            return await _repository.InsertSubCategoryDetails(subCategory);
        }

        public async Task<bool> UpdateSubCategoryMaster(SubCategoryMaster subCategory)
        {
            return await _repository.UpdateSubCategoryDetails(subCategory);
        }

        public async Task<bool> DeleteSubCategoryMaster(int id, string branchcode)
        {
            return await _repository.DeleteSubCategoryDetails(id, branchcode);
        }

        #endregion

        #region ItemMaster

        public async Task<IEnumerable<MasterItemModel>> GetItemMasterList(string branchcode)
        {
            var itemList = await _repository.GetItemsList(branchcode);
            return itemList;
        }

        public async Task<ServiceResult> CreateItemMaster(InsertMasterItemModel item)
        {
            bool itemExists = await ItemNameExist(item.ItemCode, item.ItemName, item.BranchCode);

            if (itemExists)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Item already exists",
                    Data = null
                };
            }
            else
            {
                int recordexist = await RecordsExist("ItemMaster", "ItemCode", item.ItemCode, item.BranchCode);
                if (recordexist == 1)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Item Code already Exists try again!!!",
                        Data = null
                    };
                }
                else
                {
                    var result = await _repository.InsertItemDetails(item);
                    if (result == 0)
                    {
                        if(item.TaxCode > 0 && item.TaxName != null)
                        {
                            bool extraChargesSaved = await SaveExtraChargesAsync(item.TaxCode, item.TaxName, item.ItemCode, item.BranchCode, item.OltCodes, "null");
                        }
                        return new ServiceResult
                        {
                            Success = true,
                            Message = "Item inserted successfully",
                            Data = result
                        };
                    }
                    else
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Failed to insert item",
                            Data = null
                        };
                    }
                }
            }
        }

        public async Task<bool> SaveExtraChargesAsync(int chargeCode, string chargeName, int itemCode, string branchCode, List<int> oltCode, string outletName)
        {
            using var con = _factory.CreateConnection(DbNames.POS);
            DateTime currentTime = ConvertUtcToIst();

            int result = await _repository.DeleteExtraChargesAsync(itemCode, branchCode, oltCode, outletName, con);

            int insertResult = await _repository.InsertExtraChargesAsync( chargeCode, chargeName, itemCode, branchCode, currentTime, oltCode, outletName, con);

            return insertResult > 0;
        }

        public async Task<bool> UpdateItemMaster(InsertMasterItemModel item)
        {
            bool result = await _repository.UpdateItemDetails(item);
            if(result)
            {
                if (item.TaxCode > 0 && item.TaxName != null)
                {
                    bool extraChargesSaved = await SaveExtraChargesAsync(item.TaxCode, item.TaxName, item.ItemCode, item.BranchCode, item.OltCodes, "null");
                }
            }
            return result;
        }

        public async Task<ServiceResult> DeleteItemMaster(int id, string branchcode, List<int> oltcode)
        {
            bool isDeletable = await _repository.IsDeletable(id, branchcode);
            if (isDeletable)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Item Code Present In KOT Can't Delete",
                    Data = null
                };
            }

            var success = await _repository.DeleteItemDetails(id, branchcode, oltcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Item not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Item deleted successfully",
                Data = null
            };
        }

        public async Task<bool> ItemNameExist(int id, string itemName, string branchcode)
        {
            return await _repository.ItemNameExist(id, itemName, branchcode);
        }

        #endregion

        #region StewardMaster

        public async Task<IEnumerable<MasterSteward>> GetStewardMasterList(string branchcode)
        {
            var stewardList = await _repository.GetStewardList(branchcode);
            return stewardList;
        }

        public async Task<ServiceResult> CreateStewardMaster(MasterSteward steward)
        {

            int recordexist = await RecordsExist("StewardMaster", "StwCode", steward.StwCode, steward.Branch_Code);
            if (recordexist == 1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Item Code already Exists try again!!!",
                    Data = null
                };
            }
            else
            {
                var result = await _repository.InsertStewardDetails(steward);

                if (result == 0)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Steward inserted successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Failed to insert Steward",
                        Data = null
                    };
                }
            }
        }

        public async Task<bool> UpdateStewardMaster(MasterSteward steward)
        {
            return await _repository.UpdateStewardDetails(steward);
        }

        public async Task<ServiceResult> DeleteStewardMaster(int id, string branchcode)
        {
            bool isDeletable = await _repository.IsStewardDeletable(id, branchcode);
            if (isDeletable)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Steward Code Present In KOT Can't Delete",
                    Data = null
                };
            }

            var success = await _repository.DeleteStewardDetails(id, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Steward not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Steward deleted successfully",
                Data = null
            };
        }

        #endregion

        #region NCDepartmentMaster

        public async Task<IEnumerable<NCDepartmentMaster>> GetNCDepartmentMasterList(string branchcode)
        {
            var ncDepartmentList = await _repository.GetNCDepartmentList(branchcode);
            return ncDepartmentList;
        }

        public async Task<ServiceResult> CreateNCDepartmentMaster(NCDepartmentMaster ncdepartment)
        {

            int recordexist = await RecordsExist("NCDepartment", "NCDepCode", ncdepartment.NCDepCode, ncdepartment.Branch_Code);
            if (recordexist == 1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "NC Department Code already Exists try again!!!",
                    Data = null
                };
            }
            else
            {
                var result = await _repository.InsertNCDepartmentDetails(ncdepartment);

                if (result == 0)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "NC Department inserted successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Failed to insert NC Department",
                        Data = null
                    };
                }
            }
        }

        public async Task<bool> UpdateNCDepartmentMaster(NCDepartmentMaster ncdepartment)
        {
            return await _repository.UpdateNCDepartmentDetails(ncdepartment);
        }

        public async Task<ServiceResult> DeleteNCDepartmentMaster(int id, string branchcode)
        {
            bool isDeletable = await _repository.IsNCDepartmentDeletable(id, branchcode);
            if (isDeletable)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "NC Departmenta Code Present In KOT Can't Delete",
                    Data = null
                };
            }

            var success = await _repository.DeleteNCDepartmentDetails(id, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "NC Department not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "NC Department deleted successfully",
                Data = null
            };
        }

        #endregion

        #region PrintingMaster

        public async Task<IEnumerable<PrintingMaster>> GetPrintingMasterList(string branchcode)
        {
            var printingMasterList = await _repository.GetPrintingMasterList(branchcode);
            return printingMasterList;
        }

        public async Task<ServiceResult> CreatePrintingMaster(PrintingMaster printingmaster)
        {

            int recordexist = await RecordsExist("PrintingDepartment", "DepCode", printingmaster.DepCode, printingmaster.Branch_Code);
            if (recordexist == 1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Printing Department Code already Exists try again!!!",
                    Data = null
                };
            }
            else
            {
                var result = await _repository.InsertPrintingMasterDetails(printingmaster);

                if (result == 0)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Printing Department inserted successfully",
                        Data = result
                    };
                }
                else
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Failed to insert Printing Department",
                        Data = null
                    };
                }
            }
        }

        public async Task<bool> UpdatePrintingMaster(PrintingMaster printingmaster)
        {
            return await _repository.UpdatePrintingMasterDetails(printingmaster);
        }

        public async Task<ServiceResult> DeletePrintingMaster(int id, string branchcode)
        {
            var success = await _repository.DeletePrintingMasterDetails(id, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Printing Department not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Printing Department deleted successfully",
                Data = null
            };
        }

        #endregion


        #region ImportItemMaster

        public async Task<ImportResult> ValidateFileAsync(IFormFile file, string BranchCode)
        {
            var result = new ImportResult();

            if (file == null || file.Length == 0)
            {
                result.Errors.Add("File is empty");
                return result;
            }

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("Only .xlsx files allowed");
                return result;
            }

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage.License.SetNonCommercialOrganization("Cogwave");

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Check whether entire row is empty
                    bool isRowEmpty = true;

                    for (int col = 1; col <= 11; col++)
                    {
                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, col].Text))
                        {
                            isRowEmpty = false;
                            break;
                        }
                    }

                    // Skip fully empty rows
                    if (isRowEmpty)
                        continue;

                    // Required field validation
                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text))
                        throw new Exception("ItemCode missing");

                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 2].Text))
                        throw new Exception("ItemName required");

                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 8].Text))
                        throw new Exception("Rate required");

                    var item = new ImportItemRow
                    {
                        ItemCode = Convert.ToInt32(sheet.Cells[row, 1].Text),
                        ItemName = sheet.Cells[row, 2].Text,
                        Category = sheet.Cells[row, 3].Text,
                        SubCategory = sheet.Cells[row, 4].Text,
                        Group = sheet.Cells[row, 5].Text,
                        Department = sheet.Cells[row, 6].Text,
                        Unit = sheet.Cells[row, 7].Text,
                        Rate = Convert.ToDecimal(sheet.Cells[row, 8].Text),
                        Tax = sheet.Cells[row, 9].Text,
                        PrintDept = sheet.Cells[row, 10].Text,
                        SacCode = sheet.Cells[row, 11].Text,
                        IsVeg = bool.Parse(sheet.Cells[row, 12].Text)
                    };

                    // DB validation
                    if (await _repository.ItemExists(item.ItemCode, BranchCode))
                        throw new Exception($"ItemCode already exists: {item.ItemCode}");

                    if (await _repository.ItemNameExists(item.ItemName, BranchCode))
                        throw new Exception($"ItemName already exists: {item.ItemCode}");

                    result.ValidRows.Add(item);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }
            }

            //for (int row = 2; row <= rowCount; row++) // skip header
            //{
            //    try
            //    {
            //        var item = new ImportItemRow
            //        {
            //            ItemCode = Convert.ToInt32(sheet.Cells[row, 1].Text),
            //            ItemName = sheet.Cells[row, 2].Text,
            //            Category = sheet.Cells[row, 3].Text,
            //            SubCategory = sheet.Cells[row, 5].Text,
            //            Group = sheet.Cells[row, 5].Text,
            //            Department = sheet.Cells[row, 6].Text,
            //            Unit = sheet.Cells[row, 7].Text,
            //            Rate = Convert.ToDecimal(sheet.Cells[row, 8].Text),
            //            Tab = sheet.Cells[row, 9].Text,
            //            PrintDept = sheet.Cells[row, 10].Text,
            //            SacCode = sheet.Cells[row, 11].Text,
            //        };

            //        // Basic validation
            //        if (item.ItemCode == 0)
            //            throw new Exception("ItemCode missing");

            //        if (string.IsNullOrEmpty(item.ItemName))
            //            throw new Exception("ItemName required");

            //        // DB validation
            //        if (await _repository.ItemExists(item.ItemCode, item.BranchCode))
            //            throw new Exception($"ItemCode already exists: {item.ItemCode}");

            //        result.ValidRows.Add(item);
            //    }
            //    catch (Exception ex)
            //    {
            //        result.Errors.Add($"Row {row}: {ex.Message}");
            //    }
            //}
            return result;
        }

        public async Task<ImportSummary> ImportItemsAsync(List<ImportItemRow> items, string usercode, string branchcode, List<int> OltCodes)
        {
            var result = new ImportSummary
            {
                Total = items.Count
            };

            foreach (var item in items)
            {
                try
                {
                    DateTime currentTime = ConvertUtcToIst();

                    if (item.ItemCode == 0)
                    {
                        result.Skipped++;
                        result.Errors.Add($"ItemCode missing");
                        continue;
                    }

                    var exists = await _repository.ItemExists(item.ItemCode, branchcode);

                    if (exists)
                    {
                        result.Skipped++;
                        result.Errors.Add($"ItemCode exists: {item.ItemCode}");
                        continue;
                    }

                    var nameexists = await _repository.ItemNameExists( item.ItemName, branchcode);

                    if (nameexists)
                    {
                        result.Skipped++;
                        result.Errors.Add($"ItemName exists: {item.ItemName}");
                        continue;
                    }

                    //var catcode = await _repository.GetOrCreateCategory(item.Category);
                    //var oltcode = await _repository.GetOrCreate("OutletMaster", "OltCode", "OltName", item.Outlet, branchcode, usercode, currentTime, item);
                    var catcode = await _repository.GetOrCreate("ItemCategory", "CatCode", "CatName", item.Category, branchcode, usercode, currentTime, item);
                    var subCatcode = await _repository.GetOrCreateSubCategory(item.Category, item.SubCategory, branchcode, currentTime, usercode);
                    var groupcode = await _repository.GetOrCreate("ItemGroup", "GrpCode", "GrpName", item.Group, branchcode, usercode, currentTime, item);
                    var deptcode = await _repository.GetOrCreate("Department", "DepCode", "DepName", item.Department, branchcode, usercode, currentTime, item);
                    var printdeptcode = await _repository.GetOrCreate("PrintingDepartment", "DepCode", "DepName", item.PrintDept, branchcode, usercode, currentTime, item);
                    var unitcode = await _repository.GetOrCreate("UnitMaster", "UnitCode", "UnitName", item.Unit, branchcode, usercode, currentTime, item);
                    var taxcode = await _repository.GetOrCreate("BillTaxMaster", "TaxCode", "TaxName", item.Tax, branchcode, usercode, currentTime, item);
                    //var tabcode = await _repository.GetOrCreate("ItemCategoryNew", "CatgoryId", "CategoryName", item.Tab, branchcode, usercode, currentTime, item);

                    var dep = string.IsNullOrEmpty(item.Department) ? "" : item.Department.Substring(0, 1);

                    await _repository.InsertItemMaster(item, OltCodes, catcode, subCatcode, groupcode, deptcode, printdeptcode, unitcode, taxcode, item.Tax, dep, branchcode, usercode, currentTime);

                    //await _repository.InsertItem(item, tabcode, deptcode, item.BranchCode);

                    result.Inserted++;

                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"ItemCode {item.ItemCode}: {ex.Message}");
                }
            }

            return result;
        }
        //public async Task<string> ImportItemsAsync(List<ImportItemRow> items)
        //{
        //    var result = new ImportSummary
        //    {
        //        Total = items.Count
        //    };

        //    foreach (var item in items)
        //    {
        //        DateTime currentTime = ConvertUtcToIst();

        //        if (item.ItemCode == 0)
        //            continue;

        //        var exists = await _repository.ItemExists(item.ItemCode, item.BranchCode);

        //        if (exists)
        //            continue;

        //        //var catcode = await _repository.GetOrCreateCategory(item.Category);
        //        var catcode = await _repository.GetOrCreate("ItemCategory", "CatCode", "CatName", item.Category, item.BranchCode, currentTime, item);
        //        var subCatcode = await _repository.GetOrCreateSubCategory(item.Category, item.SubCategory, item.BranchCode, currentTime, item.UserCode);
        //        var groupcode = await _repository.GetOrCreate("ItemGroup", "GrpCode", "GrpName", item.Group, item.BranchCode, currentTime, item);
        //        var deptcode = await _repository.GetOrCreate("Department", "DepCode", "DepName", item.Department, item.BranchCode, currentTime, item);
        //        var printdeptcode = await _repository.GetOrCreate("PrintingDepartment", "DepCode", "DepName", item.PrintDept, item.BranchCode, currentTime, item);
        //        var unitcode = await _repository.GetOrCreate("UnitMaster", "UnitCode", "UnitName", item.Unit, item.BranchCode, currentTime, item);
        //        var tabcode = await _repository.GetOrCreate("ItemCategoryNew", "CatgoryId", "CategoryName", item.Tab, item.BranchCode, currentTime, item);

        //        var dep = string.IsNullOrEmpty(item.SubGroup) ? "" : item.SubGroup.Substring(0, 1);

        //        await _repository.InsertItemMaster(item, catcode, subCatcode, groupcode, deptcode, printdeptcode, unitcode, tabcode, dep, item.BranchCode, currentTime);

        //        //await _repository.InsertItem(item, tabcode, deptcode, item.BranchCode);
        //    }

        //    return "Item Import Completed";
        //}

        #endregion

        #region OutletItemDetails

        public async Task<IEnumerable<OutletItemDetails>> GetOutletItemDetailsList(string branchcode, string oltcode, bool isavaliable)
        {
            var outletItemDetailsList = await _repository.GetOutletItemDetailsList(branchcode, oltcode, isavaliable);
            return outletItemDetailsList;
        }

        public async Task<ServiceResult> CreateOltItemMaster(CreateOutletRequest requestdetails)
        {
            using var con = _factory.CreateConnection(DbNames.POS);
            con.Open();
            using var transaction = con.BeginTransaction();

            try
            {
                string branchCode = requestdetails.BranchCode;
                DateTime currentTime = ConvertUtcToIst();

                if (requestdetails.IsTaxIncluded)
                {
                    if (string.IsNullOrEmpty(requestdetails.TaxName))
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Select Tax Percentage!!!",
                            Data = null
                        };

                    //bool taxExists = await _repository.TaxExists(requestdetails.TaxName, branchCode, transaction);

                    //if (!taxExists)
                    //    return new ServiceResult
                    //    {
                    //        Success = false,
                    //        Message = "Tax Not Found",
                    //        Data = null
                    //    };

                    foreach (var item in requestdetails.OltDetails)
                    {
                        await _repository.DeleteExtraCharge( item.ItemCode, requestdetails.TaxCode, requestdetails.OltCode, branchCode, transaction);

                        await _repository.InsertExtraCharge( item.ItemCode, requestdetails.TaxCode, requestdetails.TaxName, requestdetails.OltCode, currentTime,  branchCode,  transaction);
                    }

                    transaction.Commit();

                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Tax applied successfully",
                        Data = requestdetails.OltDetails.Count
                    };
                }
                else
                {
                    bool saveSuccess = true;

                    int oidCode = await _repository.Findnextnumber("OltItemDetails", "OIDCode", "Branch_Code", branchCode);

                    // Backup and delete old items
                    foreach (var item in requestdetails.OltDetails)
                    {
                        await _repository.InsertTmpItemAsync(item, requestdetails.OltCode, branchCode, transaction);

                        await _repository.DeleteItemAsync(item.ItemCode, requestdetails.OltCode, branchCode, transaction);
                    }

                    // Insert new items
                    foreach (var item in requestdetails.OltDetails)
                    {
                        var exists = await _repository.CountExistingAsync(item.ItemCode, requestdetails.OltCode, branchCode, transaction);
                        if (exists == 0)
                        {
                            var result = await _repository.InsertItemAsync(item, requestdetails.OltCode, branchCode, oidCode++, currentTime, requestdetails.UserCode, transaction);
                            if (result == 0)
                            {
                                saveSuccess = false;
                                //break;
                            }
                        }
                    }

                    if (saveSuccess) 
                    {
                        await _repository.CleanupTmpAsync(branchCode, transaction);
                    }
                    else
                    {
                        await _repository.ElseDeleteItemAsync(requestdetails.OltCode, branchCode, transaction);

                        await _repository.ElseInsertOltItemAsync(requestdetails.OltCode, branchCode, transaction);

                        await _repository.ElseDeleteTempItemAsync(requestdetails.OltCode, branchCode, transaction);

                    }
                    transaction.Commit();
                    return new ServiceResult
                    {
                        Success = saveSuccess,
                        Message = saveSuccess ? "Outlet Item Details inserted successfully" : "Failed to insert Outlet Item Details",
                        Data = requestdetails.OltDetails.Count
                    };
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert Outlet Item Details",
                    Data = null
                };
            }
        }

        #endregion


        #region PropertyMaster

        public async Task<IEnumerable<PropertyMasterModel>> GetPropertyDetailsList()
        {
            var propertyDetailsList = await _repository.GetPropertyDetailsList();
            return propertyDetailsList;
        }

        public async Task<ServiceResult> CreatePropertyDetailsMaster(PropertyMasterModel propertyMaster)
        {

            var result = await _repository.CreatePropertyDetailsMaster(propertyMaster);

            if (result > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Property Detail inserted successfully",
                    Data = result
                };
            }
            else if (result == -1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Company Name already exists. Please don't use Same Company Name.",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert Property Detail",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> UpdatePropertyDetailsMaster(PropertyMasterModel propertyMaster)
        {
            var UpdateUser = await _repository.UpdatePropertyDetailsMaster(propertyMaster);
            if (UpdateUser)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Property Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Property Detail not found",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> DeletePropertyDetailsMaster(int id)
        {
            var success = await _repository.DeletePropertyDetailsMaster(id);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Property Detail not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Property Detail deleted successfully",
                Data = null
            };
        }


        #endregion

        #region BranchMaster

        public async Task<IEnumerable<BranchMasterModel>> GetBranchDetailsList(int propertyid)
        {
            var branchDetailsList = await _repository.GetBranchDetailsList(propertyid);
            return branchDetailsList;
        }

        public async Task<ServiceResult> CreateBranchDetailsMaster(BranchMasterModel branchMaster)
        {

            var result = await _repository.CreateBranchDetailsMaster(branchMaster);

            if (result > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Branch Detail inserted successfully",
                    Data = result
                };
            }
            else if (result == -1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Branch code already exists. Please don't use the same Branch code.",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert Branch Detail",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> UpdateBranchDetailsMaster(BranchMasterModel branchMaster)
        {
            var UpdateUser = await _repository.UpdateBranchDetailsMaster(branchMaster);
            if (UpdateUser)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Branch Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Branch Detail not found",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> DeleteBranchDetailsMaster(int id)
        {
            var success = await _repository.DeleteBranchDetailsMaster(id);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Branch Detail not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Branch Detail deleted successfully",
                Data = null
            };
        }


        #endregion

        #region AddOnItems

        public async Task<IEnumerable<AddOnItemModel>> GetItemWiseAddOnDetailsList(string branchcode, int itemcode)
        {
            var addOnDetailsList = await _repository.GetItemWiseAddOnDetailsList(branchcode, itemcode);
            return addOnDetailsList;
        }

        public async Task<IEnumerable<AdditionalOnItemModel>> GetAdditionalAddonDetailsList(int Itemcode, string branchcode)
        {
            var DetailsList = await _repository.GetAdditionalAddonDetailsList(Itemcode, branchcode);
            return DetailsList;
        }

        public async Task<ServiceResult> InsertorUpdateAddOnDetails(List<AddOnItemModel> details)
        {
            var result = await _repository.InsertorUpdateAddOnDetails(details);
            if (result)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Add-On Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Add-On Detail not found",
                    Data = null
                };
            }
        }


        #endregion


        #region Common Methods

        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        public async Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            return await _repository.Findnextnumber(table_name, column_name, condition_name, branch);
        }

        public async Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode)
        {
            return await _repository.RecordsExist(tablename, columnname, parameter, BranchCode);
        }
        #endregion
    }
}
