using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces
{
    public interface IProductLicence_Service
    {
        Task<ProductLicenceModel> GetProductLicenceKey(string branchCode);

        Task<ProductLicenceServiceResult<int>> SaveProductLicenceKey(InsertProductLicenceModel model);
    }
}
