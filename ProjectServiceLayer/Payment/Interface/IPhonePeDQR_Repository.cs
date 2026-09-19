using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.Payment;

namespace HMS_360_PMS.ProjectServiceLayer.Payment.Interface
{
    public interface IPhonePeDQR_Repository
    {
        Task<PhonePeTransaction> GetPhonePeTransaction();

        Task<PhonePeImageRequestModel> GetPhonePeImageRequest();

    }
}
