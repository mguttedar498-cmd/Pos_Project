using Azure.Core;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra;
using HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using OfficeOpenXml;
using System.Data;

namespace HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices
{
    public class InventoryMasterService : IInventoryMasterService
    {
        private readonly IInventoryMaster_Repository _repository;
        private readonly DbConnectionFactory _factory;

        public InventoryMasterService(IInventoryMaster_Repository repository, DbConnectionFactory factory)
        {
            _repository = repository;
            _factory = factory;
        }

        #region Supplier Master
        public async Task<IEnumerable<InventorySupplierMaster>> GetSupplierMasterList(string branchcode)
        {
            var supplierList = await _repository.GetSupplierList(branchcode);
            return supplierList;
        }

        public async Task<int> CreateSupplierMaster(InventorySupplierMaster supplier)
        {
            return await _repository.InsertSupplierDetails(supplier);
        }

        public async Task<bool> UpdateSupplierMaster(InventorySupplierMaster supplier)
        {
            return await _repository.UpdateSupplierDetails(supplier);
        }

        public async Task<bool> DeleteSupplierMaster(int id, string branchcode)
        {
            return await _repository.DeleteSupplierDetails(id, branchcode);
        }

        #endregion

        #region Inventory Category Master
        public async Task<IEnumerable<InventoryCategoryMaster>> GetCategoryMasterList(string branchcode)
        {
            var categoryList = await _repository.GetCategoryList(branchcode);
            return categoryList;
        }

        public async Task<int> CreateCategoryMaster(InventoryCategoryMaster category)
        {
            return await _repository.InsertCategoryDetails(category);
        }

        public async Task<bool> UpdateCategoryMaster(InventoryCategoryMaster category)
        {
            return await _repository.UpdateCategoryDetails(category);
        }

        public async Task<bool> DeleteCategoryMaster(int id, string branchcode)
        {
            return await _repository.DeleteCategoryDetails(id, branchcode);
        }
        #endregion

        #region Sub Category Master
        public async Task<IEnumerable<InventorySubCategoryMaster>> GetSubCategoryMasterList(string branchcode)
        {
            var subCategoryList = await _repository.GetSubCategoryList(branchcode);
            return subCategoryList;
        }

        public async Task<int> CreateSubCategoryMaster(InventorySubCategoryMaster subCategory)
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

        public async Task<bool> UpdateSubCategoryMaster(InventorySubCategoryMaster subCategory)
        {
            return await _repository.UpdateSubCategoryDetails(subCategory);
        }

        public async Task<bool> DeleteSubCategoryMaster(int id, string branchcode)
        {
            return await _repository.DeleteSubCategoryDetails(id, branchcode);
        }

        #endregion

        #region Inventory Store Master
        public async Task<IEnumerable<InventoryStoreMaster>> GetInventoryStoreMaster(string branchcode)
        {
            var subCategoryList = await _repository.GetInventoryStoreMaster(branchcode);
            return subCategoryList;
        }

        public async Task<int> CreateInventoryStoreMaster(InventoryStoreMaster inventorystore)
        {
            return await _repository.CreateInventoryStoreMaster(inventorystore);
        }

        public async Task<bool> UpdateInventoryStoreMaster(InventoryStoreMaster inventorystore)
        {
            return await _repository.UpdateInventoryStoreMaster(inventorystore);
        }

        public async Task<bool> DeleteInventoryStoreMaster(int id, string branchcode)
        {
            return await _repository.DeleteInventoryStoreMaster(id, branchcode);
        }

        #endregion

        #region Item Master

        public async Task<IEnumerable<InventoryMasterItemModel>> GetItemMasterList(string branchcode)
        {
            var itemList = await _repository.GetItemsList(branchcode);
            return itemList;
        }

        public async Task<InventoryServiceResult> CreateItemMaster(InventoryMasterItemModel item)
        {
            bool itemExists = await ItemNameExist(item.ItemCode, item.ItemName, item.Branch_Code);

            if (itemExists)
            {
                return new InventoryServiceResult
                {
                    Success = false,
                    Message = "Item already exists",
                    Data = null
                };
            }
            else
            {
                int recordexist = await RecordsExist("InventoryItemMaster", "ItemCode", item.ItemCode, item.Branch_Code);
                if (recordexist == 1)
                {
                    return new InventoryServiceResult
                    {
                        Success = false,
                        Message = "Item Code already Exists try again!!!",
                        Data = null
                    };
                }
                else
                {
                    var result = await _repository.InsertItemDetails(item);
                    if (result > 0)
                    {
                        if (item.TaxCode > 0 && item.TaxName != null)
                        {
                            bool extraChargesSaved = await SaveExtraChargesAsync(item.TaxCode, item.TaxName, item.ItemCode, item.Branch_Code, item.Storeid, "null");
                        }
                        return new InventoryServiceResult
                        {
                            Success = true,
                            Message = "Item inserted successfully",
                            Data = result
                        };
                    }
                    else
                    {
                        return new InventoryServiceResult
                        {
                            Success = false,
                            Message = "Failed to insert item",
                            Data = null
                        };
                    }
                }
            }
        }

        public async Task<bool> UpdateItemMaster(InventoryMasterItemModel item)
        {
            bool result = await _repository.UpdateItemDetails(item);
            if (result)
            {
                if (item.TaxCode > 0 && item.TaxName != null)
                {
                    bool extraChargesSaved = await SaveExtraChargesAsync(item.TaxCode, item.TaxName, item.ItemCode, item.Branch_Code, item.Storeid, "null");
                }
            }
            return result;
        }

        public async Task<InventoryServiceResult> DeleteItemMaster(int id, string branchcode)
        {
            var success = await _repository.DeleteItemDetails(id, branchcode);
            if (!success)
            {
                return new InventoryServiceResult
                {
                    Success = false,
                    Message = "Item not found",
                    Data = null
                };
            }

            return new InventoryServiceResult
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
        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;
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

        public async Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode)
        {
            return await _repository.RecordsExist(tablename, columnname, parameter, BranchCode);
        }
        public async Task<bool> SaveExtraChargesAsync(int chargeCode, string chargeName, int itemCode, string branchCode, string Storeid, string outletName)
        {
            using var con = _factory.CreateConnection(DbNames.POS);
            DateTime currentTime = ConvertUtcToIst();

            int result = await _repository.DeleteExtraChargesAsync(itemCode, branchCode, Storeid, outletName, con);

            int insertResult = await _repository.InsertExtraChargesAsync(chargeCode, chargeName, itemCode, branchCode, currentTime, Storeid, outletName, con);

            return insertResult > 0;
        }
        #endregion

        #region Inventory Miscellaneous
        public async Task<IEnumerable<MiscellaneousInventory>> GetInventoryMiscList(string branchcode)
        {
            var miscList = await _repository.GetInventoryMiscList(branchcode);
            return miscList;
        }

        public async Task<int> CreateInventoryMisc(MiscellaneousInventory misc)
        {
            return await _repository.CreateInventoryMisc(misc);
        }

        public async Task<bool> UpdateInventoryMisc(MiscellaneousInventory misc)
        {
            return await _repository.UpdateInventoryMisc(misc);
        }

        public async Task<bool> DeleteInventoryMisc(int id, string branchcode)
        {
            return await _repository.DeleteInventoryMisc(id, branchcode);
        }
        #endregion

        #region Inventory GRN Miscellaneous
        public async Task<IEnumerable<GRNMiscellaneousInventory>> GetInventoryGRNMiscList(string branchcode)
        {
            var grnMiscList = await _repository.GetInventoryGRNMiscList(branchcode);
            return grnMiscList;
        }

        public async Task<int> CreateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc)
        {
            return await _repository.CreateInventoryGRNMisc(grnMisc);
        }

        public async Task<bool> UpdateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc)
        {
            return await _repository.UpdateInventoryGRNMisc(grnMisc);
        }

        public async Task<bool> DeleteInventoryGRNMisc(int id, string branchcode)
        {
            return await _repository.DeleteInventoryGRNMisc(id, branchcode);
        }
        #endregion

        #region ImportItemMaster
        public async Task<InventoryImportResult> ValidateFileAsync(IFormFile file, string BranchCode)
        {
            var result = new InventoryImportResult();

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

            ExcelPackage.License.SetNonCommercialOrganization("Cogwave");

            using var package = new ExcelPackage(stream);
            var sheet = package.Workbook.Worksheets[0];

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    bool isRowEmpty = true;

                    for (int col = 1; col <= 11; col++)
                    {
                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, col].Text))
                        {
                            isRowEmpty = false;
                            break;
                        }
                    }

                    if (isRowEmpty)
                        continue;

                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text))
                        throw new Exception("ItemCode missing");

                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 2].Text))
                        throw new Exception("ItemName required");

                    if (string.IsNullOrWhiteSpace(sheet.Cells[row, 8].Text))
                        throw new Exception("ItemRate required");

                    var item = new InventoryImportItemRow
                    {
                        ItemCode = Convert.ToInt32(sheet.Cells[row, 1].Text),
                        ItemName = sheet.Cells[row, 2].Text,
                        Category = sheet.Cells[row, 3].Text,
                        SubCategory = sheet.Cells[row, 4].Text,
                        Group = sheet.Cells[row, 5].Text,
                        Unit = sheet.Cells[row, 6].Text,
                        ItemRate = Convert.ToDecimal(sheet.Cells[row, 7].Text),
                        Tax = sheet.Cells[row, 8].Text,
                        NoofUnits = Convert.ToInt32(sheet.Cells[row, 9].Text),
                        PurchaseRate = Convert.ToDecimal(sheet.Cells[row, 10].Text)
                    };

                    if (await _repository.ItemExists(item.ItemCode, BranchCode))
                        throw new Exception($"ItemCode already exists: {item.ItemCode}");

                    if (await _repository.ItemNameExists(item.ItemName, BranchCode))
                        throw new Exception($"ItemName already exists: {item.ItemName}");

                    result.ValidRows.Add(item);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Row {row}: {ex.Message}");
                }
            }
            return result;
        }

        public async Task<InventoryImportSummary> ImportItemsAsync(List<InventoryImportItemRow> items, string usercode, string branchcode, List<int> Storeids)
        {
            var result = new InventoryImportSummary
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

                    var nameexists = await _repository.ItemNameExists(item.ItemName, branchcode);

                    if (nameexists)
                    {
                        result.Skipped++;
                        result.Errors.Add($"ItemName exists: {item.ItemName}");
                        continue;
                    }

                    var catcode = await _repository.GetOrCreate("InventoryItemCategory", "CatCode", "CatName", item.Category, branchcode, usercode, currentTime, item);
                    var subCatcode = await _repository.GetOrCreateSubCategory(item.Category, item.SubCategory, branchcode, currentTime, usercode);
                    var groupcode = await _repository.GetOrCreate("ItemGroup", "GrpCode", "GrpName", item.Group, branchcode, usercode, currentTime, item);
                    var unitcode = await _repository.GetOrCreate("UnitMaster", "UnitCode", "UnitName", item.Unit, branchcode, usercode, currentTime, item);
                    var taxcode = await _repository.GetOrCreate("BillTaxMaster", "TaxCode", "TaxName", item.Tax, branchcode, usercode, currentTime, item);

                    await _repository.InsertItemMaster(item, Storeids, catcode, subCatcode, groupcode, unitcode, taxcode, item.Tax,  branchcode, usercode, currentTime);

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
        #endregion

        #region Unit Conversion Master
        public async Task<IEnumerable<UnitConversionMaster>> GetUnitConversionList(string branch)
        {
            var unitList = await _repository.GetUnitConversionList(branch);
            return unitList;
        }
        public async Task<int> CreateUnitConversion(UnitConversionMaster unit)
        {
            return await _repository.CreateUnitConversion(unit);
        }
        public async Task<bool> UpdateUnitConversion(UnitConversionMaster unit)
        {
            return await _repository.UpdateUnitConversion(unit);
        }
        public async Task<bool> DeleteUnitConversion(int UnitCode, string branch)
        {
            return await _repository.DeleteUnitConversion(UnitCode, branch);
        }

        #endregion

        #region Terms And Conditions Master
        public async Task<IEnumerable<TermsAndConditionsMaster>> GetTermsAndConditionsList(string branch)
        {
            var termsList = await _repository.GetTermsAndConditionsList(branch);
            return termsList;
        }
        public async Task<int> CreateTermsAndConditions(TermsAndConditionsMaster model)
        {
            return await _repository.CreateTermsAndConditions(model);
        }
        public async Task<bool> UpdateTermsAndConditions(TermsAndConditionsMaster model)
        {
            return await _repository.UpdateTermsAndConditions(model);
        }
        public async Task<bool> DeleteTermsAndConditions(int TermsCode,string branch)
        {
            return await _repository.DeleteTermsAndConditions(TermsCode, branch);
        }
        #endregion
    }
}
