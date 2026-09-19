using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices
{
    public interface IInventoryMasterService
    {
        #region Supplier Master
        Task<IEnumerable<InventorySupplierMaster>> GetSupplierMasterList(string branchcode);
        Task<int> CreateSupplierMaster(InventorySupplierMaster supplier);
        Task<bool> UpdateSupplierMaster(InventorySupplierMaster supplier);
        Task<bool> DeleteSupplierMaster(int SupCode, string branchcode);
        #endregion

        #region Category Master
        Task<IEnumerable<InventoryCategoryMaster>> GetCategoryMasterList(string branchcode);

        Task<int> CreateCategoryMaster(InventoryCategoryMaster category);

        Task<bool> UpdateCategoryMaster(InventoryCategoryMaster category);

        Task<bool> DeleteCategoryMaster(int categoryCode, string branchcode);

        #endregion

        #region Sub Category Master
        Task<IEnumerable<InventorySubCategoryMaster>> GetSubCategoryMasterList(string branchcode);

        Task<int> CreateSubCategoryMaster(InventorySubCategoryMaster subCategory);

        Task<bool> UpdateSubCategoryMaster(InventorySubCategoryMaster subCategory);

        Task<bool> DeleteSubCategoryMaster(int subCategoryCode, string branchcode);

        #endregion

        #region Inventory Store Master
        Task<IEnumerable<InventoryStoreMaster>> GetInventoryStoreMaster(string branchcode);
        Task<int> CreateInventoryStoreMaster(InventoryStoreMaster inventorystore);
        Task<bool> UpdateInventoryStoreMaster(InventoryStoreMaster inventorystore);
        Task<bool> DeleteInventoryStoreMaster(int StoreId, string branchcode);
        #endregion

        #region Inventory Item Master

        Task<IEnumerable<InventoryMasterItemModel>> GetItemMasterList(string branchcode);
        Task<InventoryServiceResult> CreateItemMaster(InventoryMasterItemModel item);
        Task<bool> UpdateItemMaster(InventoryMasterItemModel item);
        Task<InventoryServiceResult> DeleteItemMaster(int itemCode, string branchcode);
        #endregion

        #region Inventory Miscellaneous
        Task<IEnumerable<MiscellaneousInventory>> GetInventoryMiscList(string branchcode);
        Task<int> CreateInventoryMisc(MiscellaneousInventory misc);
        Task<bool> UpdateInventoryMisc(MiscellaneousInventory misc);
        Task<bool> DeleteInventoryMisc(int chargeId, string branchcode);
        #endregion

        #region Inventory GRN Miscellaneous
        Task<IEnumerable<GRNMiscellaneousInventory>> GetInventoryGRNMiscList(string branchcode);
        Task<int> CreateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc);
        Task<bool> UpdateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc);
        Task<bool> DeleteInventoryGRNMisc(int GRNId, string branchcode);
        #endregion

        #region Import Item Master
        Task<InventoryImportResult> ValidateFileAsync(IFormFile file, string BranchCode);

        Task<InventoryImportSummary> ImportItemsAsync(List<InventoryImportItemRow> items, string usercode, string branchcode, List<int> Storeids);

        #endregion

        #region Unit Conversion Master
        Task<IEnumerable<UnitConversionMaster>> GetUnitConversionList(string branch);

        Task<int> CreateUnitConversion(UnitConversionMaster model);

        Task<bool> UpdateUnitConversion(UnitConversionMaster model);

        Task<bool> DeleteUnitConversion(int UnitCode, string branch);
        #endregion

        #region Terms And Conditions Master
        Task<IEnumerable<TermsAndConditionsMaster>> GetTermsAndConditionsList(string branch);

        Task<int> CreateTermsAndConditions(TermsAndConditionsMaster model);

        Task<bool> UpdateTermsAndConditions(TermsAndConditionsMaster model);

        Task<bool> DeleteTermsAndConditions(int TermsCode, string branch);

        #endregion

    }
}
