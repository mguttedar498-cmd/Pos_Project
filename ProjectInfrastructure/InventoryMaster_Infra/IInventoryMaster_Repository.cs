using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using System.Data;

namespace HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra
{
    public interface IInventoryMaster_Repository
    {
        #region SupplierMaster
        public Task<IEnumerable<InventorySupplierMaster>> GetSupplierList(string branchcode);

        public Task<int> InsertSupplierDetails(InventorySupplierMaster supplier);

        public Task<bool> UpdateSupplierDetails(InventorySupplierMaster supplier);

        public Task<bool> DeleteSupplierDetails(int SupCode, string branchcode);

        #endregion

        #region CategoryMaster

        public Task<IEnumerable<InventoryCategoryMaster>> GetCategoryList(string branchcode);

        public Task<int> InsertCategoryDetails(InventoryCategoryMaster category);

        public Task<bool> UpdateCategoryDetails(InventoryCategoryMaster category);

        public Task<bool> DeleteCategoryDetails(int categoryCode, string branchcode);

        #endregion

        #region SubCategoryMaster
        public Task<IEnumerable<InventorySubCategoryMaster>> GetSubCategoryList(string branchcode);

        public Task<int> InsertSubCategoryDetails(InventorySubCategoryMaster subCategory);

        public Task<bool> UpdateSubCategoryDetails(InventorySubCategoryMaster subCategory);

        public Task<bool> DeleteSubCategoryDetails(int subCategoryCode, string branchcode);

        #endregion

        #region SubCategoryMaster
        public Task<IEnumerable<InventoryStoreMaster>> GetInventoryStoreMaster(string branchcode);

        public Task<int> CreateInventoryStoreMaster(InventoryStoreMaster inventorystore);

        public Task<bool> UpdateInventoryStoreMaster(InventoryStoreMaster inventorystore);

        public Task<bool> DeleteInventoryStoreMaster(int StoreId, string branchcode);

        #endregion

        #region Item Master

        Task<IEnumerable<InventoryMasterItemModel>> GetItemsList(string branchcode);

        public Task<int> InsertItemDetails(InventoryMasterItemModel item);

        Task<bool> UpdateItemDetails(InventoryMasterItemModel item);

        public Task<bool> DeleteItemDetails(int itemcode, string branchcode);

        public Task<bool> ItemNameExist(int id, string itemName, string branchcode);

        Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode);

        Task<int> DeleteExtraChargesAsync(int itemCode, string branchCode, string Storeid, string outletname, IDbConnection con);

        Task<int> InsertExtraChargesAsync(int chargeCode, string chargeName, int itemCode, string branchCode, DateTime currentTime, string Storeid, string outletName, IDbConnection con);
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
        Task<bool> ItemExists(int itemCode, string branch);

        Task<bool> ItemNameExists(string itemname, string branch);

        Task<string> GetOrCreate(string table, string codeCol, string nameCol, string value, string branchcode, string usercode, DateTime currentTime, InventoryImportItemRow item);

        Task<string> GetOrCreateSubCategory(string category, string subCategory, string branchcode, DateTime currentTime, string userCode);

        Task InsertItemMaster(InventoryImportItemRow item, List<int> Storeids, string catcode, string subCatcode, string groupcode,  string unitcode, string taxcode, string taxname, string branchcode, string usercode, DateTime currentdate);

        #endregion

        #region Unit Conversion Master
        Task<IEnumerable<UnitConversionMaster>> GetUnitConversionList(string branch);
        Task<int> CreateUnitConversion(UnitConversionMaster model);
        Task<bool> UpdateUnitConversion(UnitConversionMaster model);
        Task<bool> DeleteUnitConversion(int id, string branch);
        #endregion

        #region Terms And Conditions Master
        Task<IEnumerable<TermsAndConditionsMaster>> GetTermsAndConditionsList(string branch);
        Task<int> CreateTermsAndConditions(TermsAndConditionsMaster model);
        Task<bool> UpdateTermsAndConditions(TermsAndConditionsMaster model);
        Task<bool> DeleteTermsAndConditions(int TermsCode, string branch);
        #endregion

    }
}
