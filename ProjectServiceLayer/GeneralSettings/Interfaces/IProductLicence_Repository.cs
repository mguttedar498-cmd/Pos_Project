using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces
{
    public interface IProductLicence_Repository
    {
        Task<ProductLicenceModel> GetProductLicenceKey(string branchCode);

        Task<int> SaveProductLicenceKey(InsertProductLicenceModel model, int totaldays, DateTime intimationdate, int indimationdays, string encryptedserialkey, string encryptedToDate, string encryptedIntimationDate);
    }
}
