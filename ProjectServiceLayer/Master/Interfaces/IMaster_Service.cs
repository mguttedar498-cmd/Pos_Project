using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Interfaces
{
    public interface IMaster_Service
    {
        #region CompanyMaster

        Task<IEnumerable<CompaniesMaster>> GetCompaniesList(string branchcode);

        Task<int> CreateCompany(CompaniesMaster company);

        Task<bool> UpdateCompanyMaster(CompaniesMaster company);

        Task<bool> DeleteCompany(int companyCode, string branchcode);

        #endregion

        #region TaxMaster

        Task<IEnumerable<BillTaxMaster>> GetTaxMasterList(string branchcode);

        Task<int> CreateTaxMaster(BillTaxMaster taxmaster);

        Task<bool> UpdateTaxMaster(BillTaxMaster taxmaster);

        Task<bool> DeleteTaxMaster(int taxCode, string branchcode);

        Task<IEnumerable<BillTaxDescription>> GetTaxDescriptionList(string branchcode);

        Task<int> CreateTaxDescription(BillTaxDescription taxdescription);

        Task<bool> UpdateTaxDescription(BillTaxDescription taxdescription);

        Task<bool> DeleteTaxDescription(int taxCode, string branchcode);

        #endregion

        #region DepartmentMaster

        Task<IEnumerable<DepartmentMaster>> GetDepartmentMasterList(string branchcode);

        Task<int> CreateDepartmentMaster(DepartmentMaster department);

        Task<bool> UpdateDepartmentMaster(DepartmentMaster department);

        Task<bool> DeleteDepartmentMaster(int departmentCode, string branchcode);

        #endregion

        #region OutletMaster

        Task<IEnumerable<OutletsMaster>> GetOutletMasterList(string branchcode);

        Task<int> CreateOutletMaster(OutletsMaster outlet);

        Task<bool> UpdateOutletMaster(OutletsMaster outlet);

        Task<bool> DeleteOutletMaster(int outletCode, string branchcode);

        #endregion

        #region TableMaster

        Task<IEnumerable<TablesMaster>> GetTableMasterList(string branchcode);

        Task<int> CreateTableMaster(InsertUpdateTableMaster table);

        Task<bool> UpdateTableMaster(InsertUpdateTableMaster table);

        Task<bool> DeleteTableMaster(int tableCode, string branchcode);

        Task<string> ServerName();
        
        Task<string> GetOutletName(int outlet, string branchcode);

        #endregion

        #region UnitMaster

        Task<IEnumerable<UnitMaster>> GetUnitMasterList(string branchcode);

        Task<int> CreateUnitMaster(UnitMaster unit);

        Task<bool> UpdateUnitMaster(UnitMaster unit);

        Task<bool> DeleteUnitMaster(int unitCode, string branchcode);

        #endregion

        #region GroupMaster

        Task<IEnumerable<GroupMaster>> GetGroupMasterList(string branchcode);

        Task<int> CreateGroupMaster(GroupMaster group);

        Task<bool> UpdateGroupMaster(GroupMaster group);

        Task<bool> DeleteGroupMaster(int groupCode, string branchcode);

        #endregion

        #region CategoryMaster

        Task<IEnumerable<CategoryMaster>> GetCategoryMasterList(string branchcode);

        Task<int> CreateCategoryMaster(CategoryMaster category);

        Task<bool> UpdateCategoryMaster(CategoryMaster category);

        Task<bool> DeleteCategoryMaster(int categoryCode, string branchcode);

        #endregion

        #region SubCategoryMaster

        Task<IEnumerable<SubCategoryMaster>> GetSubCategoryMasterList(string branchcode);

        Task<int> CreateSubCategoryMaster(SubCategoryMaster subCategory);

        Task<bool> UpdateSubCategoryMaster(SubCategoryMaster subCategory);

        Task<bool> DeleteSubCategoryMaster(int subCategoryCode, string branchcode);

        #endregion

        #region ItemMaster

        Task<IEnumerable<MasterItemModel>> GetItemMasterList(string branchcode);

        Task<ServiceResult> CreateItemMaster(InsertMasterItemModel item);

        Task<bool> UpdateItemMaster(InsertMasterItemModel item); 
        
        Task<ServiceResult> DeleteItemMaster(int itemCode, string branchcode, List<int> oltcode);

        #endregion

        #region ImportItemMaster

        Task<ImportResult> ValidateFileAsync(IFormFile file, string BranchCode);

        Task<ImportSummary> ImportItemsAsync(List<ImportItemRow> items, string usercode, string branchcode, List<int> OltCodes);

        #endregion

        #region StewardMaster

        Task<IEnumerable<MasterSteward>> GetStewardMasterList(string branchcode);

        Task<ServiceResult> CreateStewardMaster(MasterSteward steward);

        Task<bool> UpdateStewardMaster(MasterSteward steward);

        Task<ServiceResult> DeleteStewardMaster(int stewardCode, string branchcode);

        #endregion

        #region NCDepartmentMaster

        Task<IEnumerable<NCDepartmentMaster>> GetNCDepartmentMasterList(string branchcode);

        Task<ServiceResult> CreateNCDepartmentMaster(NCDepartmentMaster ncdepartment);

        Task<bool> UpdateNCDepartmentMaster(NCDepartmentMaster ncdepartment);

        Task<ServiceResult> DeleteNCDepartmentMaster(int ncdepartmentCode, string branchcode);

        #endregion


        #region PrintingMaster

        Task<IEnumerable<PrintingMaster>> GetPrintingMasterList(string branchcode);

        Task<ServiceResult> CreatePrintingMaster(PrintingMaster printingmaster);

        Task<bool> UpdatePrintingMaster(PrintingMaster printingmaster);

        Task<ServiceResult> DeletePrintingMaster(int printingMasterCode, string branchcode);

        #endregion


        #region OutletItemDetails

        Task<IEnumerable<OutletItemDetails>> GetOutletItemDetailsList(string branchcode, string oltcode, bool isavaliable);

        Task<ServiceResult> CreateOltItemMaster(CreateOutletRequest requestdetails);


        #endregion

        #region PropertyMaster

        Task<IEnumerable<PropertyMasterModel>> GetPropertyDetailsList();

        Task<ServiceResult> CreatePropertyDetailsMaster(PropertyMasterModel propertymaster);

        Task<ServiceResult> UpdatePropertyDetailsMaster(PropertyMasterModel propertymaster);

        Task<ServiceResult> DeletePropertyDetailsMaster(int companycode);

        #endregion

        #region BranchMaster

        Task<IEnumerable<BranchMasterModel>> GetBranchDetailsList(int propertyid);

        Task<ServiceResult> CreateBranchDetailsMaster(BranchMasterModel branchmaster);
        
        Task<ServiceResult> UpdateBranchDetailsMaster(BranchMasterModel branchmaster);

        Task<ServiceResult> DeleteBranchDetailsMaster(int companycode);

        #endregion


        #region Common Methods

        Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch);
        #endregion

        #region AddOnItems

        Task<IEnumerable<AddOnItemModel>> GetItemWiseAddOnDetailsList(string branchcode, int itemcode);

        Task<IEnumerable<AdditionalOnItemModel>> GetAdditionalAddonDetailsList(int Itemcode, string branchcode);

        Task<ServiceResult> InsertorUpdateAddOnDetails(List<AddOnItemModel> details);

        #endregion
    }
}
