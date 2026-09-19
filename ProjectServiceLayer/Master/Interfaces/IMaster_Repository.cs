using DocumentFormat.OpenXml.Office2010.Excel;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using System.Data;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Interfaces
{
    public interface IMaster_Repository
    {
        #region CompanyMaster

        Task<IEnumerable<CompaniesMaster>> GetCompaniesList(string branchcode);

        Task<int> InsertCompanyDetails(CompaniesMaster company);

        Task<bool> UpdateCompanyDetails(CompaniesMaster company);

        Task<bool> DeleteCompanyDetails(int companyCode, string branchcode);

        #endregion


        #region TaxMaster

        Task<IEnumerable<BillTaxMaster>> GetTaxMasterList(string branchcode);

        Task<int> InsertTaxMasterDetails(BillTaxMaster taxmaster);

        Task<bool> UpdateTaxMasterDetails(BillTaxMaster taxmaster); 

        Task<bool> DeleteTaxMasterDetails(int taxMasterCode, string branchcode);

        Task<IEnumerable<BillTaxDescription>> GetTaxDescriptionList(string branchcode);

        Task<int> InsertTaxDescriptionDetails(BillTaxDescription taxdescription);   

        Task<bool> UpdateTaxDescriptionDetails(BillTaxDescription taxdescription);

        Task<bool> DeleteTaxDescriptionDetails(int taxDescriptionCode, string branchcode);

        #endregion

        #region DepartmentMaster

        Task<IEnumerable<DepartmentMaster>> GetDepartmentList(string branchcode);

        Task<int> InsertDepartmentDetails(DepartmentMaster department);

        Task<bool> UpdateDepartmentDetails(DepartmentMaster department);

        Task<bool> DeleteDepartmentDetails(int departmentCode, string branchcode);

        #endregion

        #region OutletMaster

        Task<IEnumerable<OutletsMaster>> GetOutletsList(string branchcode);

        Task<int> InsertOutletDetails(OutletsMaster outlet);

        Task<bool> UpdateOutletDetails(OutletsMaster outlet);

        Task<bool> DeleteOutletDetails(int outletCode, string branchcode);

        #endregion

        #region TableMaster

        Task<IEnumerable<TablesMaster>> GetTablesList(string branchcode);

        Task<int> InsertTableDetails(InsertUpdateTableMaster table);

        Task<bool> UpdateTableDetails(InsertUpdateTableMaster table);

        Task<bool> DeleteTableDetails(int tableCode, string branchcode);

        Task<string> ServerName();

        Task<string> GetOutletName(int outlet, string branchcode);


        #endregion

        #region UserMaster

        Task<IEnumerable<UnitMaster>> GetUnitList(string branchcode);

        Task<int> InsertUnitDetails(UnitMaster unit);

        Task<bool> UpdateUnitDetails(UnitMaster unit);

        Task<bool> DeleteUnitDetails(int unitCode, string branchcode);

        #endregion

        #region GroupMaster

        public Task<IEnumerable<GroupMaster>> GetGroupList(string branchcode);

        public Task<int> InsertGroupDetails(GroupMaster group);

        public Task<bool> UpdateGroupDetails(GroupMaster group);

        public Task<bool> DeleteGroupDetails(int groupCode, string branchcode);

        #endregion

        #region CategoryMaster

        public Task<IEnumerable<CategoryMaster>> GetCategoryList(string branchcode);

        public Task<int> InsertCategoryDetails(CategoryMaster category);

        public Task<bool> UpdateCategoryDetails(CategoryMaster category);

        public Task<bool> DeleteCategoryDetails(int categoryCode, string branchcode);

        #endregion

        #region SubCategoryMaster

        public Task<IEnumerable<SubCategoryMaster>> GetSubCategoryList(string branchcode);

        public Task<int> InsertSubCategoryDetails(SubCategoryMaster subCategory);

        public Task<bool> UpdateSubCategoryDetails(SubCategoryMaster subCategory);

        public Task<bool> DeleteSubCategoryDetails(int subCategoryCode, string branchcode);

        #endregion

        #region ItemMaster

        public Task<IEnumerable<MasterItemModel>> GetItemsList(string branchcode);

        public Task<int> InsertItemDetails(InsertMasterItemModel item);

        public Task<bool> UpdateItemDetails(InsertMasterItemModel item);

        public Task<bool> DeleteItemDetails(int itemcode, string branchcode, List<int> oltcode);

        Task<bool> IsDeletable(int itemcode, string branchcode);

        public Task<bool> ItemNameExist(int id, string itemName, string branchcode);

        Task<int> DeleteExtraChargesAsync(int itemCode, string branchCode, List<int> oltcode, string outletname, IDbConnection con);

        Task<int> InsertExtraChargesAsync(int chargeCode, string chargeName, int itemCode, string branchCode, DateTime currentTime, List<int> oltCode, string outletName, IDbConnection con);

        #endregion

        #region StewardMaster

        public Task<IEnumerable<MasterSteward>> GetStewardList(string branchcode);

        public Task<int> InsertStewardDetails(MasterSteward steward);

        public Task<bool> UpdateStewardDetails(MasterSteward steward);

        public Task<bool> DeleteStewardDetails(int stewardCode, string branchcode);

        public Task<bool> IsStewardDeletable(int id, string branchcode);

        #endregion

        #region NCDepartmentMaster

        public Task<IEnumerable<NCDepartmentMaster>> GetNCDepartmentList(string branchcode);

        public Task<int> InsertNCDepartmentDetails(NCDepartmentMaster ncdepartment);

        public Task<bool> UpdateNCDepartmentDetails(NCDepartmentMaster ncdepartment);

        public Task<bool> DeleteNCDepartmentDetails(int ncdepartmentCode, string branchcode);

        public Task<bool> IsNCDepartmentDeletable(int id, string branchcode);

        #endregion

        #region PrintingMaster

        public Task<IEnumerable<PrintingMaster>> GetPrintingMasterList(string branchcode);

        public Task<int> InsertPrintingMasterDetails(PrintingMaster printingmaster);

        public Task<bool> UpdatePrintingMasterDetails(PrintingMaster printingmaster);

        public Task<bool> DeletePrintingMasterDetails(int printingmasterCode, string branchcode);

        #endregion


        #region ImportItemMaster

        Task<bool> ItemExists(int itemCode, string branch);

        Task<bool> ItemNameExists(string itemname, string branch);

        Task<string> GetOrCreate(string table, string codeCol, string nameCol, string value, string branchcode, string usercode, DateTime currentTime, ImportItemRow item);
        //Task<string> GetOrCreateCategory(string category);

        Task<string> GetOrCreateSubCategory(string category, string subCategory, string branchcode, DateTime currentTime, string userCode);

        Task InsertItemMaster(ImportItemRow item, List<int> oltcodes, string catcode, string subCatcode, string groupcode, string deptcode, string printdeptcode, string unitcode, string taxcode, string taxname, string dep, string branchcode, string usercode, DateTime currentdate);

        //Task InsertItem(ImportItemRow item, string tabcode, string deptcode, string branchcode);

        #endregion

        #region OutletItemDetails

        Task<IEnumerable<OutletItemDetails>> GetOutletItemDetailsList(string branchcode, string oltcode, bool isavaliable);

        Task<int> InsertTmpItemAsync(OutletItemDetails item, string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> DeleteItemAsync(int itemCode, string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> InsertItemAsync(OutletItemDetails item, string outletCode, string branchCode, int oidCode, DateTime currenttime, string usercode, IDbTransaction transaction);

        Task<int> CleanupTmpAsync(string branchCode, IDbTransaction transaction);

        Task<int> CountExistingAsync(int itemCode, string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> InsertExtraCharge(int itemcode, string chargecode, string chargename, string outletCode, DateTime currentTime, string branchCode, IDbTransaction transaction);

        Task<int> DeleteExtraCharge(int itemCode, string chargeCode, string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> ElseDeleteItemAsync(string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> ElseInsertOltItemAsync(string outletCode, string branchCode, IDbTransaction transaction);

        Task<int> ElseDeleteTempItemAsync(string outletCode, string branchCode, IDbTransaction transaction);

        #endregion

        #region PropertyMaster

        Task<IEnumerable<PropertyMasterModel>> GetPropertyDetailsList();

        Task<int> CreatePropertyDetailsMaster(PropertyMasterModel propertyMaster);

        Task<bool> UpdatePropertyDetailsMaster(PropertyMasterModel propertyMaster);

        Task<bool> DeletePropertyDetailsMaster(int id);

        #endregion

        #region BranchMaster

        Task<IEnumerable<BranchMasterModel>> GetBranchDetailsList(int propertyid);

        Task<int> CreateBranchDetailsMaster(BranchMasterModel branchMaster);

        Task<bool> UpdateBranchDetailsMaster(BranchMasterModel branchMaster);

        Task<bool> DeleteBranchDetailsMaster(int id);

        #endregion

        #region Common Methods

        Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch);

        Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode);
        #endregion

        #region AddOnItems

        Task<IEnumerable<AddOnItemModel>> GetItemWiseAddOnDetailsList(string branchcode, int itemcode);

        Task<IEnumerable<AdditionalOnItemModel>> GetAdditionalAddonDetailsList(int Itemcode, string branchcode);

        Task<bool> InsertorUpdateAddOnDetails(List<AddOnItemModel> details);

        #endregion
    }
}
