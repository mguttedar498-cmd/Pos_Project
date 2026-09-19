
using HMS_360_PMS.ProjectEntitiesModels.Payment;

namespace HMS_360_PMS.ProjectServiceLayer.Payment.Interface
{
    public interface ICCAvenueQRDevice_Service
    {
        Task<MakePaymentResponse> MakePaymentAsync(MakePaymentRequest request);

        Task<TransactionStatusResponse> CheckTransactionStatus(string transNo);
    }
}
