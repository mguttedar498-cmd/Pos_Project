using Dapper;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using System.Data;
using System.Text;

namespace HMS_360_PMS.ProjectInfrastructure.GeneralSettings
{
    public class MSIC_DAL : IMSIC_Repository
    {
        private readonly DbConnectionFactory _factory;

        public MSIC_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<bool> POSMSICResetTransaction()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            rowsAffected = await connection.QuerySingleAsync<int>("SP_POSMSICResetTransaction", commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> POSMSICResetTransactionFROMandTO(string Branchcode, DateTime Fromdate, DateTime Todate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            var parameters = new DynamicParameters();
            parameters.Add("@branchcode", Branchcode);
            parameters.Add("@fromdate", Fromdate);
            parameters.Add("@todate", Todate);

            rowsAffected = await connection.QuerySingleAsync<int>("SP_POSMSICResetTransactionFromAndTo", parameters, commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> POSMSICResetMasters()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            rowsAffected = await connection.QuerySingleAsync<int>("SP_POSMSICResetMasters", commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> POSMSICTruncateAll()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            rowsAffected = await connection.QuerySingleAsync<int>("SP_POSMSICTruncateforNewClient", commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> POSMSICUpdateBranch(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            var parameters = new DynamicParameters();
            parameters.Add("@branchcode", branchcode);

            rowsAffected = await connection.QuerySingleAsync<int>("SP_POSMSICUpdatebranchcode", parameters, commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }

        public async Task<bool> POSMSICCreateUserMaster(int usercode, string username, string password, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            var parameters = new DynamicParameters();
            parameters.Add("@UserCode", usercode);
            parameters.Add("@UserName", username);
            parameters.Add("@UserPassword", password);
            parameters.Add("@UserPrivilege", '1');
            parameters.Add("@EnteredBy", '0');
            parameters.Add("@LastModify", DateTime.Now);
            parameters.Add("@Branch_code", branchcode);
            parameters.Add("@DisPercent", 1);
            parameters.Add("@DisAmount", 100);
            parameters.Add("@RoleId", 1);

            rowsAffected = await connection.QuerySingleAsync<int>("sp_CreateUserDetailsMaster", parameters, commandType: CommandType.StoredProcedure);

            return rowsAffected > 0;
        }
    }
}
