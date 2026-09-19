using Dapper;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure.POS
{
    public class KotBillSettlement_DAL
    {
        private readonly DbConnectionFactory _factory;

        public KotBillSettlement_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        // 1. Get unsettled KOTs
        public async Task<IEnumerable<dynamic>> GetUnsettledKOTs(string tableNo, string subTable)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM KotMaster WHERE kottblno = @TableNo AND subtable = @SubTable AND KOTSETTLED = 0;";

            return await connection.QueryAsync(sql, new { TableNo = tableNo, SubTable = subTable });
        }

        // 2. Update KOTSettlementMaster after saving KOT
        public async Task<int> UpdateKOTSettlementMaster(int ksmId, string branch, double serviceCharge)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE KOTSettlementMaster 
                       SET Isuploaded = '0', Ismodified = '0', AccountType = 'F', KSMServiceCharge = @ServiceCharge
                       WHERE KSMID = @KSMId AND Branch_Code = @Branch;";

            return await connection.ExecuteAsync(sql, new { KSMId = ksmId, Branch = branch, ServiceCharge = serviceCharge });
        }

        // 3. Check room service bill exists
        //public IEnumerable<dynamic> GetFoodBill(DateTime trDate, string rcptNo, string checkInNo, int outlet)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    string sql = @"SELECT * FROM Tbl_FoodBills WHERE Trdate = @TrDate AND RcptNo = @RcptNo AND CheckInNo = @CheckInNo AND OutLateName = @Outlet;";
        //    return connection.Query(sql, new { TrDate = trDate, RcptNo = rcptNo, CheckInNo = checkInNo, Outlet = outlet });

        //}

        public async Task<List<FoodBillDto>> GetFoodBill(DateTime posEntryDate, string rcptNo, string checkInNo, int outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            string sql = @" SELECT * FROM Tbl_FoodBills WHERE TrDate = @TrDate AND RcptNo = @RcptNo AND CheckInNo = @CheckInNo AND OutlateName = @Outlet;";

            return (await connection.QueryAsync<FoodBillDto>(sql, new { TrDate = posEntryDate.Date, RcptNo = rcptNo, CheckInNo = checkInNo, Outlet = outlet })).ToList();
        }

        // 4. Insert room service bill
        //public int InsertFoodBill(dynamic bill)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    string sql = @"INSERT INTO Tbl_FoodBills 
        //                (RNo, TrDate, RcptNo, GuestCode, GuestName, CheckInNo, RoomNo, BillAmt, SplitId, BillStatus, OutlateName, Tax1, Tax2, TaxAmt1, Tax3, TaxAmt2)
        //               VALUES (@RNo, @TrDate, @RcptNo, @GuestCode, @GuestName, @CheckInNo, @RoomNo, @BillAmt, 0, 'N', @Outlet, @CGST, @SGST, @TaxVal, @ExTax1, @ExTax2);";
        //    return connection.Execute(sql, bill);
        //}

        public async Task<int> InsertFoodBill(FoodBill bill)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);
            connection.Open(); // Ensure connection is open

            string sql = @"
        INSERT INTO Tbl_FoodBills 
        (RNo, TrDate, RcptNo, GuestCode, GuestName, CheckInNo, RoomNo, BillAmt, SplitId, BillStatus, OutlateName, Tax1, Tax2, TaxAmt1, Tax3, TaxAmt2)
        VALUES
        (@RNo, @TrDate, @RcptNo, @GuestCode, @GuestName, @CheckInNo, @RoomNo, @BillAmt, 0, 'N', @Outlet, @CGST, @SGST, @TaxVal, @ExTax1, @ExTax2);";

            return await connection.ExecuteAsync(sql, bill);
        }

        // 5. Update KOTSettlementMaster for room service
        public async Task<int> UpdateKOTSettlementRoomService(string billNo, int outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE KOTSettlementMaster 
                       SET Isuploaded = '0', Ismodified = '1', KSMBillSettled = '1', KSMBillTransfered = 1, KSMIsRoomService = 1
                       WHERE KSMBillNo = @BillNo AND oltcode = @Outlet AND Branch_Code = @Branch AND AccountType = 'F';";
            return await connection.ExecuteAsync(sql, new { BillNo = billNo, Outlet = outlet, Branch = branch });
        }

        // 6. Update KOTSettlementMaster for parcel service
        public async Task<int> UpdateKOTSettlementParcelService(string billNo, string outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE KOTSettlementMaster 
                       SET Isuploaded = '0', Ismodified = '1', KSMBillTransfered=1, KSMIsParcelService=1
                       WHERE KSMBillNo = @BillNo AND oltcode = @Outlet AND Branch_Code = @Branch AND AccountType = 'F';";

            return await connection.ExecuteAsync(sql, new { BillNo = billNo, Outlet = outlet, Branch = branch });
        }

        // 7. Get fast food token number
        public async Task<string> GetFastFoodToken(int ksmId, string billNo, string outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT TokenNo  FROM KOTSettlementMaster 
                       WHERE KSMId = @KSMId  AND KSMBillNo = @BillNo AND oltcode = @Outlet AND Branch_Code = @Branch AND AccountType = 'F';";

            return await connection.QueryFirstOrDefaultAsync<string>(sql, new { KSMId = ksmId, BillNo = billNo, Outlet = outlet, Branch = branch });
        }

        // 8. Get KOT details for settlement
        public async Task<IEnumerable<dynamic>> GetKOTDetails(string table, string subTable, int outlet, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM KotMaster km
                       JOIN KotDetails kd ON km.KOTNO = kd.KOTNo
                       WHERE KotTblNo = @Table
                         AND Subtable = @SubTable AND OltCode = @Outlet AND km.Branch_Code = @Branch
                         AND KotSettled = 0 AND KotCancelled = 0 AND KotChargeable = 1;";

            return await connection.QueryAsync<dynamic>(sql, new { Table = table, SubTable = subTable, Outlet = outlet, Branch = branch });
        }

        // 9. Check KOTSettlementDetails
        public async Task<IEnumerable<dynamic>> GetKOTSettlementDetails(int kotId, int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM KOTSettlementDetails
                       WHERE KotId = @KotId AND KSMId = @KSMId AND Branch_Code = @Branch;";

            return await connection.QueryAsync<dynamic>(sql, new { KotId = kotId, KSMId = ksmId, Branch = branch });
        }

        // 10. Insert KOTSettlementDetails
        public async Task<int> InsertKOTSettlementDetails(int kotId, int ksmId, string branch, string fincode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"INSERT INTO KOTSettlementDetails (KOTId, KSMId, KotSplit, Branch_Code, Isuploaded, FinCode)
                       VALUES (@KotId, @KSMId, 0, @Branch, '0', @Fincode);";

            return await connection.ExecuteAsync(sql, new { KotId = kotId, KSMId = ksmId, Branch = branch , Fincode = fincode });
        }

        // 11. Update KOTMaster after settlement
        public async Task<int> UpdateKOTMasterAfterSettlement(int kotNo, int outlet, string branch, string tokenNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE KOTMaster
                       SET Isuploaded = '0', KOTSettled = 1, KotOrderNo = @TokenNo
                       WHERE KOTNo = @KOTNo AND OltCode = @Outlet AND Branch_Code = @Branch AND KOTSettled = 0;";

            return await connection.ExecuteAsync(sql, new { KOTNo = kotNo, Outlet = outlet, Branch = branch, TokenNo = tokenNo });
        }

        public async Task<IEnumerable<dynamic>> GetKOTDisplay(int kotId)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"select * from Tbl_Kot_Display where kotno = @KotId";

            return await connection.QueryAsync<dynamic>(sql, new { KotId = kotId});
        }

        // 12. Update KOT display table
        public async Task<int> UpdateKOTDisplay(int kotNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"UPDATE Tbl_Kot_Display SET barked = 1, ready = 1, Picked = 1
                       WHERE KOTNo = @KOTNo AND Picked = 0;";

            return await connection.ExecuteAsync(sql, new { KOTNo = kotNo });
        }

        // 13. Delete KOTSettlementMaster if not saved
        public async Task<int> DeleteKOTSettlementMaster(int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"DELETE FROM KOTSettlementMaster WHERE KSMID = @KSMId AND Branch_Code = @Branch AND DayEnd = 'N';";

            return await connection.ExecuteAsync(sql, new { KSMId = ksmId, Branch = branch });
        }

        // 14. Delete KOTSettlementDetails if not saved
        public async Task<int> DeleteKOTSettlementDetails(int ksmId, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"DELETE FROM KOTSettlementDetails WHERE KSMID = @KSMId AND Branch_Code = @Branch;";

            return await connection.ExecuteAsync(sql, new { KSMId = ksmId, Branch = branch });
        }

        // 15. Delete room service food bills if not saved
        public async Task<int> DeleteFoodBill(string rcptNo, DateTime trDate)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            string sql = @"DELETE FROM Tbl_FoodBills WHERE RcptNo = @RcptNo AND TrDate = @TrDate";

            return await connection.ExecuteAsync(sql, new { RcptNo = rcptNo, TrDate = trDate });
        }

        public async Task<string> GetOutletName(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select OltName from OutletMaster where OltCode= @Outlet AND branch_code = @BranchCode";
            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { Outlet = outlet, BranchCode = branchCode });
        }
    }
}



