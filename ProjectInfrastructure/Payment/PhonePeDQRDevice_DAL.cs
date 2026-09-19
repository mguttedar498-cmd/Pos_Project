using Dapper;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.Payment.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Payment.Interface;

namespace HMS_360_PMS.ProjectInfrastructure.Payment
{
    public class PhonePeDQRDevice_DAL : IPhonePeDQR_Repository
    {
        private readonly DbConnectionFactory _factory;

        public PhonePeDQRDevice_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<PhonePeTransaction> GetPhonePeTransaction()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "Select Id, BaseUrl, MerchantKey, MerchantId, StoreId, TerminalId, ExpiresIn, ProviderId, CallbackUrl, TransactionId From PhonePeRequestMaster";

            return await connection.QueryFirstOrDefaultAsync<PhonePeTransaction>(query);
        }

        public async Task<PhonePeImageRequestModel> GetPhonePeImageRequest()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "Select Id, BaseUrl, MerchantKey, MerchantId, StoreId, TerminalId, ExpiresIn, ProviderId, CallbackUrl, TransactionId From PhonePeImageRequestMaster";

            return await connection.QueryFirstOrDefaultAsync<PhonePeImageRequestModel>(query);
        }
    }
}
