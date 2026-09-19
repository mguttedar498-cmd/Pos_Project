using HMS_360_PMS.ProjectEntitiesModels.KOT;

namespace HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces
{
    public interface IBasicSettingsManager
    {
        Task<List<SmsRights>> GetSmsRights();

        //Task<SmsSender> GetBaseSettings();
        Task<SmsSettings> GetBaseSettings();

        Task<PurcharserDetalis> GetPurcharserDetalis();

        #region  Email Notfication

        Task<EmailSenderModel> GetEmailSenderData();

        Task<IEnumerable<EmailReceiver>> GetEmailReceiverData();

        #endregion
    }
}
