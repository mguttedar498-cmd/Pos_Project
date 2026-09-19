using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.Payment;

namespace HMS_360_PMS.ProjectServiceLayer.Payment.Interface
{
    public interface IPhonePeDQR_Service
    {
        #region DQR Device Payment

        Task<PhonePeCollectResponseBody> SendPaymentRequestDQRDevice(int amount, string TransNo);

        Task<PhonePeCollectResponseBodyDQRDevice> SendCheckPaymentStatusRequestDQRDevice(string transNo);

        #endregion

        #region Own Device Payment

        Task<PhonePeCollectResponseBody> SendPaymentRequestOwnDevice(int amount, string TransNo);

        Task<PhonePeCollectResponseBody> CheckOwnDevicePaymentStatus(string transno);

        #endregion

    }
}
