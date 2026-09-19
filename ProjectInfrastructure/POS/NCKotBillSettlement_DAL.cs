using Dapper;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure.POS
{
    public class NCKotBillSettlement_DAL
    {
        private readonly DbConnectionFactory _factory;

        public NCKotBillSettlement_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> UpdateNCKOTSettlementMaster(int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE NCKOTSettlementMaster
                   SET Isuploaded = '0', Ismodified = '0', AccountType = 'F'
                   WHERE KSMID = @KsmID AND Branch_Code = @Branch";

            return await connection.ExecuteAsync(sql, new { KsmID = ksmId, Branch = branch });
        }

        public async Task<IEnumerable<dynamic>> GetNCKOTDetails(string table, string subTable, int outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM KotMaster km JOIN KotDetails kd ON km.kotno = kd.kotno
                   WHERE KotTblNo = @Table AND Subtable = @SubTable AND OltCode = @Outlet AND km.Branch_Code = @Branch AND KotSettled = 0 AND KotCancelled = 0 AND KotChargeable = 0";

            return await connection.QueryAsync(sql, new { Table = table, SubTable = subTable, Outlet = outlet, Branch = branch });
        }

        public async Task<int> UpdateKOTMasterNC(int kotNo, int outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE KOTMaster
                   SET Isuploaded = '0', KOTSettled = 1
                   WHERE KOTNo = @KOTNo AND OltCode = @Outlet AND Branch_Code = @Branch AND KOTSettled = 0";

            return await connection.ExecuteAsync(sql, new { KOTNo = kotNo, Outlet = outlet, Branch = branch });
        }

        public async Task<IEnumerable<dynamic>> GetNCKOTSettlementDetails(int kotId, int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM NCKOTSettlementDetails WHERE KotId = @KotId AND KSMId = @KSMId AND Branch_Code = @Branch";

            return await connection.QueryAsync(sql, new { KotId = kotId, KSMId = ksmId, Branch = branch });
        }

        public async Task<int> InsertNCKOTSettlementDetails(int kotId, int ksmId, string branch, string fincode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"INSERT INTO NCKOTSettlementDetails
                   (KOTId,KSMId,KotSplit,Branch_Code,Isuploaded, FinCode)
                   VALUES (@KotId,@KSMId,0,@Branch,'0', @Fincode)";

            return await connection.ExecuteAsync(sql, new { KotId = kotId, KSMId = ksmId, Branch = branch, Fincode = fincode });
        }

        public async Task<int> DeleteNCKOTSettlementMaster(int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"DELETE FROM NCKOTSettlementMaster
                   WHERE KsmID = @KsmID AND Branch_Code = @Branch AND DayEnd = 'N'";

            return await connection.ExecuteAsync(sql, new { KsmID = ksmId, Branch = branch });
        }

        public async Task<int> DeleteNCKOTSettlementDetails(int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"DELETE FROM NCKOTSettlementDetails
                   WHERE KsmID = @KsmID AND Branch_Code = @Branch";
            
            return await connection.ExecuteAsync(sql, new { KsmID = ksmId, Branch = branch });
        }

        public async Task<int> DeleteFoodBill(string billNo, DateTime posEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            string sql = @"DELETE FROM Tbl_FoodBills
                   WHERE RcptNo = @RcptNo
                   AND TrDate = @TrDate";

            return await connection.ExecuteAsync(sql, new
            {
                RcptNo = billNo,
                TrDate = posEntryDate
            });
        }
    }
}
       