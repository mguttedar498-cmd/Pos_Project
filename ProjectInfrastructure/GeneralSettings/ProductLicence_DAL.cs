using Dapper;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;

namespace HMS_360_PMS.ProjectInfrastructure.GeneralSettings
{
    public class ProductLicence_DAL : IProductLicence_Repository
    {

        private readonly DbConnectionFactory _factory;

        public ProductLicence_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<ProductLicenceModel> GetProductLicenceKey( string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            
            const string sql = " select SerialKey, ProductKey, TrDate, ValidDate, NoDays, IntimationDate, IndimationDays, ClientName, MachineName, ServerName, Status, BranchCode, Encryptedserialkey, EncryptedToDate, EncryptedIntimationDate " +
                " From Tbl_Config_Man WHERE BranchCode = @BranchCode";

            var result = await connection.QueryFirstOrDefaultAsync<ProductLicenceModel>(sql, new { BranchCode = branchCode });
            return result;
        }

        public async Task<int> SaveProductLicenceKey(InsertProductLicenceModel model, int TotalDays, DateTime IntimationDate, int IndimationDays, string Encryptedserialkey, string EncryptedToDate, string EncryptedIntimationDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @"
            IF EXISTS (SELECT 1 FROM Tbl_Config_Man WHERE BranchCode = @BranchCode)
            BEGIN
            UPDATE Tbl_Config_Man
            SET
                SerialKey = @serialkey,
                ProductKey = @productkey,
                TrDate = @trdate,
                ValidDate = @validdate,
                NoDays = @nodays,
                IntimationDate = @intimationdate,
                IndimationDays = @indimationdays,
                MachineName = @machinename,
                Status = @status,
                Encryptedserialkey = @encryptedserialkey,
                EncryptedToDate = @encryptedtodate,
                EncryptedIntimationDate = @encryptedIntimationDate
            WHERE ClientName = @clientname And BranchCode = @branchcode;
            END
            ELSE
            BEGIN
            INSERT INTO Tbl_Config_Man
            ( SerialKey, ProductKey, TrDate, ValidDate, NoDays, IntimationDate, IndimationDays, ClientName, MachineName, Status, BranchCode, Encryptedserialkey, EncryptedToDate, EncryptedIntimationDate )
            VALUES
            ( @serialkey, @productkey, @trdate, @validdate, @nodays, @intimationdate, @indimationdays, @clientname, @machinename, @status, @branchcode, @encryptedserialkey, @encryptedtodate, @encryptedintimationdate );
            END";

            var parameters = new
            {
                branchcode = model.BranchCode,
                serialkey = model.SerialKey,
                productkey = model.ProductKey,
                trdate = model.TrDate.Date,
                validdate = model.ValidDate.Date,
                nodays = TotalDays,
                intimationdate = IntimationDate,
                indimationdays = IndimationDays,
                clientname = model.ClientName,
                machinename = Environment.MachineName,
                status = true,
                encryptedserialkey = Encryptedserialkey,
                encryptedIntimationDate = EncryptedIntimationDate,
                encryptedtodate = EncryptedToDate
            };
            int rowaffected = await connection.ExecuteAsync(sql, parameters);
            return rowaffected;
        }
    }
}
