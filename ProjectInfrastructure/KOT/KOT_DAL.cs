using Azure.Core;
using Dapper;
using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;

namespace HMS_360_PMS.DAL_Layers.KOT
{
    public class KOT_DAL : IKOT_Repository
    {
        private readonly DbConnectionFactory _factory;

        public KOT_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<int> InsertShiftAsync(OpenDayRequest request, string LocalIPAddress)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var cleanTime = new TimeSpan(request.SystemTime.Hour,request.SystemTime.Minute,request.SystemTime.Second);

            var hasBranchCode = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tbl_ShiftMaster' AND COLUMN_NAME = 'BranchCode'");

            var branchColumn = hasBranchCode > 0 ? "BranchCode" : "Branch_Code";

            //string query = @$"
            //INSERT INTO Tbl_ShiftMaster
            //(ShiftOpenedUserId, ShiftOpenTime, ShiftDate, ShiftStatus, Ipaddress, {branchColumn})
            //VALUES
            //(@Userid, @SystemTime, @date, 'O', @ipaddress , @branchcode)";

            //return await connection.ExecuteAsync(query, new { Userid = request.UserId, SystemTime = cleanTime, date = request.SystemDate.Date, ipaddress = LocalIPAddress, branchcode = request.BranchCode });

            string query = $@" IF NOT EXISTS ( SELECT 1 FROM Tbl_ShiftMaster WHERE ShiftStatus = 'O' AND {branchColumn} = @branchcode )
                BEGIN
                INSERT INTO Tbl_ShiftMaster ( ShiftOpenedUserId, ShiftOpenTime, ShiftDate, ShiftStatus, Ipaddress, {branchColumn} )
                VALUES
                ( @Userid, @SystemTime, @ShiftDate, 'O', @Ipaddress, @branchcode );
                SELECT 1;
                END
                ELSE
                BEGIN
                SELECT 0;
                END";

            return await connection.ExecuteScalarAsync<int>(query, new { Userid = request.UserId, SystemTime = cleanTime, ShiftDate = request.SystemDate.Date, Ipaddress = LocalIPAddress, branchcode = request.BranchCode });

            //string query = $@" IF NOT EXISTS ( SELECT 1 FROM Tbl_ShiftMaster WHERE ShiftDate = @ShiftDate AND {branchColumn} = @branchcode )
            //    BEGIN
            //    INSERT INTO Tbl_ShiftMaster ( ShiftOpenedUserId, ShiftOpenTime, ShiftDate, ShiftStatus, Ipaddress, {branchColumn} )
            //    VALUES
            //    ( @Userid, @SystemTime, @ShiftDate, 'O', @Ipaddress, @branchcode );
            //    SELECT 1;
            //    END
            //    ELSE
            //    BEGIN
            //    SELECT 0;
            //    END";

            //return await connection.ExecuteScalarAsync<int>(query, new { Userid = request.UserId, SystemTime = cleanTime, ShiftDate = request.SystemDate.Date, Ipaddress = LocalIPAddress, branchcode = request.BranchCode });
        }

        public async Task<IEnumerable<PurchaseExpiryDto>> GetExpiryItemsAsync(DateTime posEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"
            SELECT PNo, ItemName,PItemQty AS PurchasedQty, PItemReturnQty AS UsedQty, RemQty AS UnUsedQty
            FROM PurchaseExpiry WHERE ExpiryDate = @posEntryDate AND RemQty > 0";

            return await connection.QueryAsync<PurchaseExpiryDto>(query, new { posEntryDate });
        }

        public async Task<int> GetRunningKotsCount(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT COUNT(*) FROM KOTMaster 
                         WHERE KOTCancelled=0 AND KOTSettled=0 
                         AND Branch_Code=@branchCode";

            return await connection.ExecuteScalarAsync<int>(query, new { branchCode });
        }

        public async Task<int> GetPendingBillsCount(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT COUNT(*) FROM kotsettlementmaster 
                         WHERE KSMBillTransfered = 0 AND KSMBillSettled = 0 
                         AND KSMBillCancled = 0 AND BILLCANCELLED = 0 
                         AND KSMBillAmount > 0 AND AccountType = 'F' 
                         AND KsmIsRoomService = '0' AND Branch_Code = @branchCode";

            return await connection.ExecuteScalarAsync<int>(query, new { branchCode });
        }

        public async Task<int> CloseShiftAsync(int userId, DateTime systemTime, DateTime date, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var cleanTime = new TimeSpan(systemTime.Hour, systemTime.Minute, systemTime.Second);


            var hasBranchCode = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tbl_ShiftMaster' AND COLUMN_NAME = 'BranchCode'");

            var branchColumn = hasBranchCode > 0 ? "BranchCode" : "Branch_Code";

            string query = @$"UPDATE top(1) Tbl_ShiftMaster SET ShiftStatus = 'C', ShiftClosedUserId = @Userid, ShiftClosedTime = @SystemTime
                         WHERE ShiftDate = @date AND ShiftStatus = 'O' AND {branchColumn} = @branchCode";

            return await connection.ExecuteAsync(query, new { Userid = userId, SystemTime = cleanTime, date = date.Date, branchCode = branchCode });
        }

        public async Task<int> InsertKotDisplayMaster()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"INSERT INTO Tbl_Kot_Display_Master 
                     SELECT * FROM Tbl_Kot_Display";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> DeleteKotDisplay()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"DELETE FROM Tbl_Kot_Display";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> InsertTmpKotPrint()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"INSERT INTO TMPKOTPRINTMASTER 
                     SELECT * FROM tmpKotPrint 
                     WHERE grpcode = 'Y'";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> DeleteTmpKotPrint()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"DELETE FROM tmpKotPrint 
                     WHERE grpcode = 'Y'";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> InsertTmpKotPrintNew()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"INSERT INTO TMPKOTPRINTNEWMASTER 
                     SELECT * FROM tmpkotprintnew 
                     WHERE pickedstatus = 'Y'";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> DeleteTmpKotPrintNew()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"DELETE FROM tmpkotprintnew 
                     WHERE pickedstatus = 'Y'";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> DeleteLoggers()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"DELETE FROM tbl_logers";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> UpdateSettlement(DateTime date, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @" UPDATE KOTSettlementMaster SET Isuploaded = '0', IsModified = '1', DayEnd = 'Y' 
                              WHERE DayEnd = 'N' AND KSMBillDate <= @date AND Branch_Code = @branchCode";

            return await connection.ExecuteAsync(query, new { date.Date, branchCode });
        }

        public async Task<int> UpdateKotMaster(DateTime date, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"UPDATE KOTMaster SET Isuploaded = '0', DayEnd = 'Y'
                             WHERE DayEnd = 'N' AND KOTDate <= @date AND Branch_Code = @branchCode";

            return await connection.ExecuteAsync(query, new { date.Date, branchCode });
        }

        public async Task<OutletdetailsResponse> GetOutletDetails(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT oltcode, oltname FROM OutletMaster WHERE branch_code = @branchCode";

            var result = await connection.QueryFirstOrDefaultAsync<OutletdetailsResponse>(query, new { branchCode });

            return result;
        }

        public async Task<int> DeleteFromToDate()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"DELETE FROM Tbl_FromToDate";

            return await connection.ExecuteAsync(query);
        }

        public async Task<int> InsertFromToDate(DateTime date)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"INSERT INTO Tbl_FromToDate VALUES (@date, @date)";

            return await connection.ExecuteAsync(query, new { date });
        }

        public async Task<List<double>> GetSMSCollection()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT * FROM SMSCollection";

            var result = await connection.QueryAsync<double>(query);

            return result.ToList();
        }

        public async Task<List<string>> GetEmails()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT EmailId FROM Tbl_Email_Master";

            var result = await connection.QueryAsync<string>(query);

            return result.ToList();
        }

        public async Task<OpenDayDetails> GetOpenDayDetails(int userid, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var hasBranchCode = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tbl_ShiftMaster' AND COLUMN_NAME = 'BranchCode'");

            var branchColumn = hasBranchCode > 0 ? "BranchCode" : "Branch_Code";

            string query = $@"Select ShiftOpenedUserId, ShiftOpenTime, ShiftDate, ShiftStatus, Ipaddress From Tbl_ShiftMaster where ShiftStatus = 'O' and  {branchColumn} = @BranchCode order by ShiftDate desc";

            var result = await connection.QueryFirstOrDefaultAsync<OpenDayDetails>(query, new { BranchCode = branchCode });

            return result;
        }

        #region Scanner

        public async Task<List<CategoryModel>> GetFoodCategories(string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "SELECT CatCode as CategoryId,CatName as Category  FROM ItemCategory where SubCat=0 And Branch_Code = @Branchcode ORDER BY CatCode";

            return connection.Query<CategoryModel>(qry, new { Branchcode }).ToList();
        }

        public async Task<List<FoodImageModel>> GetFoodsinImage(int outlet, int category, string filter, string Branchcode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query;
            string filterCondition = "";

            if (!(string.IsNullOrWhiteSpace(filter) || filter.Trim() == "0"))
            {
                filterCondition = " AND (CAST(im.ItemCode AS NVARCHAR) + '-' + im.ItemName) LIKE @filter ";
            }

            if (category != 0)
            {
                query = @" SELECT im.ItemName, im.ItemCode, 0 as Qty, im.thumb, im.description,
                           (SELECT OIDRate FROM OltItemDetails WHERE itemcode = im.ItemCode AND OltCode = @outlet and Branch_Code = @Branchcode ) as CurrentPrize,
                           im.CatCode, olt.OIDAvailable as Avaliable, (SELECT (OIDRate)-(OIDRate/100) * Discount FROM OltItemDetails WHERE itemcode = im.ItemCode AND OltCode = @outlet and Branch_Code = @Branchcode) as ItemRate, olt.Discount as VATPER, im.IsVeg, ic.CatName as Category
                           FROM ItemMaster im
                           INNER JOIN ItemCategory ic ON im.CatCode = ic.CatCode And ic.Branch_Code = im.branch_code
                           INNER JOIN OltItemDetails olt ON olt.ItemCode = im.ItemCode And olt.Branch_Code = im.branch_code
                           WHERE olt.OltCode = @outlet AND olt.OIDAvailable = 1 AND im.CatCode = @category AND im.branch_code = @Branchcode AND (@filter IS NULL OR (CAST(im.ItemCode AS NVARCHAR) + '-' + im.ItemName) LIKE @filter) 
                           ORDER BY im.ItemCode";
            }
            else
            {
                query = @" SELECT im.ItemName, im.ItemCode, 0 as Qty, im.thumb, im.description,
                           (SELECT OIDRate FROM OltItemDetails WHERE itemcode = im.ItemCode AND OltCode = @outlet and Branch_Code = @Branchcode) as CurrentPrize,
                           im.CatCode, olt.OIDAvailable as Avaliable, (SELECT (OIDRate)-(OIDRate/100) * Discount FROM OltItemDetails WHERE itemcode = im.ItemCode AND OltCode = @outlet and Branch_Code = @Branchcode) as ItemRate, olt.Discount as VATPER, a.Rating, im.IsVeg, ic.CatName as Category
                           FROM ItemMaster im
                           INNER JOIN ItemCategory ic ON im.CatCode = ic.CatCode And ic.Branch_Code = im.branch_code
                           INNER JOIN OltItemDetails olt ON olt.ItemCode = im.ItemCode And olt.Branch_Code = im.branch_code
                           LEFT JOIN FeedbackRatings a ON a.ServiceItem = im.ItemName
                           WHERE olt.OltCode = @outlet AND olt.OIDAvailable = 1 AND im.branch_code = @Branchcode AND (@filter IS NULL OR (CAST(im.ItemCode AS NVARCHAR) + '-' + im.ItemName) LIKE @filter)
                           ORDER BY im.ItemCode";
            }

            return (await con.QueryAsync<FoodImageModel>(query, new
            {
                outlet,
                category,
                Branchcode,
                filter = string.IsNullOrWhiteSpace(filter) || filter.Trim() == "0" ? null : $"%{filter}%"
            })).ToList();
        }

        public async Task<string> GetBrancheName(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"SELECT Branch_name FROM Tbl_branch WHERE Branch_code = @Branchcode ";

            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<int> GetCategoryCodeByNameAsync(string name, string branchCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT TOP 1 CatCode FROM ItemCategory WHERE CatName = @name AND Branch_Code = @branchCode";

            return await con.QueryFirstOrDefaultAsync<int>(sql, new { name, branchCode });
        }

        public async Task<TimingModel> GetDayAsync(string day)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT * FROM Urban_StoreTime WHERE Day = @day";

            return await con.QueryFirstOrDefaultAsync<TimingModel>(sql, new { day });
        }

        public async Task<List<Slots>> GetTimeSlotsAsync(int dayId)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = @"SELECT * 
                    FROM Pos_StoreTimeSlot 
                    WHERE DayId = @dayId 
                    ORDER BY CAST(start_time AS TIME)";

            var result = await con.QueryAsync<Slots>(sql, new { dayId });

            return result.ToList();
        }

        public async Task<List<StewardModel>> GetStewards(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select StwCode as StewardCode,StwName as StewardName,MobNo from StewardMaster Where Branch_Code = @branchCode order by StwName";

            return connection.Query<StewardModel>(qry, new { branchCode }).ToList();
        }

        public async Task<List<OutletSelectModel>> GetOutletsforUser(string username)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var branchCode = await connection.QueryFirstOrDefaultAsync<string>(
                "SELECT Branch_Code FROM Tbl_Branch");

            if (string.IsNullOrEmpty(branchCode))
                return new List<OutletSelectModel>();

            var outletCodes = await connection.QueryFirstOrDefaultAsync<string>(
                @"SELECT Temp4 FROM Tbl_System_Outlet WHERE Temp2 = @username AND Temp1 = @branchCode", new { username, branchCode });

            if (string.IsNullOrWhiteSpace(outletCodes))
                return new List<OutletSelectModel>();

            const string query = @" SELECT OltCode, OltName FROM OutletMaster WHERE Branch_Code = @branchCode AND OltCode IN ( SELECT TRY_CAST(value AS INT) FROM STRING_SPLIT(@outletCodes, ',') ) ORDER BY OltName;";

            return (await connection.QueryAsync<OutletSelectModel>( query, new { branchCode, outletCodes })).ToList();
        }

        public async Task<List<FoodScannerModel>> GetFood(string tableNo, string outlet, string subtable, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = " Select kd.ItemCode as Id, im.ItemName as Food,  SUM(convert(numeric(10,2),kd.KOTDQty)) as Qty, SUM(convert(numeric(10,2),kd.KOTDQty)) as OrigQty, " +
                " kd.KOTDRate as Price, si.SPINFO as Comment, SUM(kd.KOTDQty * kd.KOTDRate) as Total, Convert(nvarchar,km.kotno) as code " +
                " From kotmaster km " +
                " Inner join kotdetails kd on km.kotno = kd.kotno AND kd.branch_code = km.branch_code " +
                " Inner join ItemMaster im on im.ItemCode = kd.ItemCode AND im.branch_code = km.branch_code " +
                " left outer join SpecialInformation si on si.SPID = kd.SplInst And si.OltCode = km.branch_code " +
                " Where km.KOTCancelled = '0' and km.KOTSettled = '0' and km.KOTTblNo = @tableNo and km.oltcode = @outlet and SubTable = @subtable And km.branch_code = @branchCode " +
                " Group by kd.ItemCode, im.ItemName, kd.KOTDRate, si.SPINFO, km.kotno" +
                " Order by kd.ItemCode";

            return connection.Query<FoodScannerModel>(qry, new { tableNo, outlet, subtable, branchCode }).ToList();
        }

        public async Task<WaiterScannerModel> GetWaiter(string tableNo, string outlet, string subtable, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = " Select DRefKOTNo as NCCode, NCKOT_Particulars as NCRemarks, sm.StwCode, sm.StwName, KOTSeatsServed as Pax, KotMobileNo, KOTGuestName " +
                " From kotmaster km " +
                " Inner join StewardMaster sm on km.StwCode = sm.StwCode And sm.Branch_Code = km.branch_code" +
                " where km.KOTCancelled = '0' and km.KOTSettled = '0' and km.KOTTblNo = @tableNo and km.oltcode = @outlet and SubTable = @subtable And km.branch_code = @branchCode " +
                " order by km.kotno";

            return await connection.QueryFirstOrDefaultAsync<WaiterScannerModel>(qry, new { tableNo, outlet, subtable, branchCode });
        }

        public async Task<List<BranchViewModal>> GetBranch()
        {

            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "select Branch_code,Branch_name From Tbl_branch";

            return connection.Query<BranchViewModal>(qry).ToList();
        }

        public async Task<List<FoodImageModel>> GetMenuListByolt(int outlet, string branchCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var qry = "select  ItemName as ItemName, description as description,IsVeg, " +
                " im.ItemCode,(Select OIDRate from oltitemdetails where itemcode = im.ItemCode and OltCode = @Oltcode And Branch_Code = @BranchCode) as " +
                " CurrentPrize, im.CatCode as CatCode,olt.OIDAvailable as Avaliable,ic.CatName as Category " +
                " from ItemMaster im " +
                " inner join ItemCategory ic on im.CatCode = ic.CatCode And ic.Branch_Code = im.branch_code " +
                " inner join OltItemDetails olt on olt.ItemCode = im.ItemCode And olt.Branch_Code = im.branch_code " +
                " where olt.oltcode = @Oltcode and olt.OIDAvailable ='1' and olt.BranchCode = @BranchCode order by im.ItemName; ";

            return (await con.QueryAsync<FoodImageModel>(qry, new { Oltcode = outlet, BranchCode = branchCode })).ToList();
        }

        public async Task<OutletSelectModelType> GetOutletTypebyId(string oltcode, string branchcode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var qry = "select OutletCode, OutletName, OutletType from TBl_OnlineOutlet_Type where OutletCode = @outletcode and BranchCode = @Branch";

            return await con.QueryFirstOrDefaultAsync<OutletSelectModelType>(qry, new { outletcode = oltcode, Branch = branchcode });

        }

        public async Task<string> PGGettokenexpiryTime()
        {
            try
            {
                using var con = (SqlConnection)_factory.CreateConnection(DbNames.POS);

                var accesstoken = "";
                var str = "select top 1 convert(datetime, expires_at) expires_at from AccessTokenResponse order by rlno desc";

                DateTime resulst = await con.QueryFirstOrDefaultAsync<DateTime>(str);

                DateTime lastexpirytime = Convert.ToDateTime(resulst);

                if (DateTime.Now < lastexpirytime)
                {
                    str = "select top 1 access_token  from AccessTokenResponse order by rlno desc";
                    accesstoken = await con.QueryFirstOrDefaultAsync<string>(str); ;
                }
                else
                {
                    accesstoken = "";
                }
                return accesstoken;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task InsertTokenAsync(PGTokenResponseModel data, DateTime issuedAt, DateTime expiresAt)
        {
            using var con = _factory.CreateConnection(DbNames.POS);
            con.Open();

            var query = @"INSERT INTO dbo.AccessTokenResponse 
            (access_token, encrypted_access_token, expires_in, issued_at, expires_at, session_expires_at, token_type) 
            VALUES 
            (@access_token, @encrypted_access_token, @expires_in, @issued_at, @expires_at, @session_expires_at, @token_type)";

            using var cmd = new SqlCommand(query, (SqlConnection)con);

            cmd.Parameters.AddWithValue("@access_token", data.access_token ?? "");
            cmd.Parameters.AddWithValue("@encrypted_access_token", data.encrypted_access_token ?? "");
            cmd.Parameters.AddWithValue("@expires_in", data.expires_in);
            cmd.Parameters.AddWithValue("@issued_at", issuedAt);
            cmd.Parameters.AddWithValue("@expires_at", expiresAt);
            cmd.Parameters.AddWithValue("@session_expires_at", data.session_expires_at);
            cmd.Parameters.AddWithValue("@token_type", data.token_type ?? "");

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertPaymentOrderAsync(PGCreatePaymentResponse data)
        {
            using var conn = _factory.CreateConnection(DbNames.POS);
            conn.Open();

            var query = @"INSERT INTO dbo.PaymentOrders 
                     (expireAt, merchantOrderId, orderId, redirectUrl, state)
                     VALUES (@expireAt, @merchantOrderId, @orderId, @redirectUrl, @state)";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);

            cmd.Parameters.AddWithValue("@expireAt", data.expireAt);
            cmd.Parameters.AddWithValue("@merchantOrderId", data.merchantOrderId);
            cmd.Parameters.AddWithValue("@orderId", data.orderId ?? "");
            cmd.Parameters.AddWithValue("@redirectUrl", data.redirectUrl);
            cmd.Parameters.AddWithValue("@state", data.state);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task InsertPaymentStatusAsync(PGPaymentOrderStatus data, string merchantOrderId, string rawJson)
        {
            using var conn = _factory.CreateConnection(DbNames.POS);
            conn.Open();

            var query = @"INSERT INTO dbo.PaymentStatus 
                     (amount, expireAt, metaInfo, orderId, paymentDetails, state, MerchantOrderId, POSBillno)
                     VALUES (@amount, @expireAt, @metaInfo, @orderId, @paymentDetails, @state, @MerchantOrderId, @POSBillno)";

            using var cmd = new SqlCommand(query, (SqlConnection)conn);

            cmd.Parameters.AddWithValue("@amount", data.amount);
            cmd.Parameters.AddWithValue("@expireAt", data.expireAt);
            cmd.Parameters.AddWithValue("@metaInfo", data.metaInfo?.ToString() ?? "");
            cmd.Parameters.AddWithValue("@orderId", data.orderId ?? "");
            cmd.Parameters.AddWithValue("@paymentDetails", rawJson);
            cmd.Parameters.AddWithValue("@state", data.state ?? "");
            cmd.Parameters.AddWithValue("@MerchantOrderId", merchantOrderId);
            cmd.Parameters.AddWithValue("@POSBillno", "");

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<RoomserviceModel> GetRoomserviceDetails(string roomno)
        {
            try
            {
                using var con = _factory.CreateConnection(DbNames.POS);

                var str = "select rd.CurrentCheckIn as CheckInNo, cm.CustomerCode as GuestCode, gm.GuestName, gm.Mobile " +
                    " from Tbl_Room_Details rd " +
                    " left join Tbl_CheckIn_Master cm on rd.CurrentCheckIn = cm.CheckInNo " +
                    " left join Tbl_Guest_Master gm on cm.CustomerCode = gm.GuestCode " +
                    " where RoomNo = @Roomno and Prioritys = 'P' and status = 'O'";

                var result = await con.QueryFirstOrDefaultAsync<RoomserviceModel>(str, new { Roomno = roomno });

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CompanyInfo> GetCompanyinfoBill()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = $@"SELECT Company_Name, Company_code, StartYear, Address1, Address2, Phone_number, Mob_number, OwnerName, 
                          Owner_Number, Fax_number, Email_id, Tin_no, Licence_number, Branch_code, STDCODE
                          FROM dbo.Tbl_companyinfo ";

            return await connection.QueryFirstOrDefaultAsync<CompanyInfo>(selectquery);
        }

        public async Task<BillModelForBIll> Getbillnowithorderid(string orderid, int Oltcode, string Branchcode)
        {
            BillModelForBIll billdetail = new BillModelForBIll();

            try
            {
                using var con = _factory.CreateConnection(DbNames.POS);

                var str = "select BillView from Tbl_PhonePe_OrderBillDetails where OrderId = @Orderid And Outletcode = @oltcode And Branch_Code = @branchcode";

                var result = await con.QueryFirstOrDefaultAsync<string>(str, new { Orderid = orderid, oltcode = Oltcode, branchcode = Branchcode });

                var Billdata = JsonConvert.DeserializeObject<BillModelForBIll>(result);

                return Billdata;
            }
            catch (Exception ex)
            {
                return billdetail;
            }
        }

        #endregion


        #region Scanner in POS
        public async Task<dynamic> TableReservations(int resid)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT * FROM tablereservations WHERE Id = @ResId";

            return await connection.QueryFirstOrDefaultAsync<dynamic>(selectquery, new { ResId = resid });
        }

        public async Task<dynamic> GetKOTDetails(string table, string subtable, int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " SELECT distinct KOTMaster.KOTTblNo,KOTChargeable FROM ItemMaster " +
                " INNER JOIN  (KOTMaster  INNER JOIN KOTDetails ON KOTMaster.KOTId = KOTDetails.KOTId And KOTMaster.branch_code = KOTDetails.branch_code) " +
                " ON ItemMaster.ItemCode = KOTDetails.ItemCode And ItemMaster.branch_code = KOTDetails.branch_code" +
                " where KOTMaster.KOTSettled=0 AND KOTMaster.KOTCancelled=0 and KOTMaster.StwCode<>'0' and KOTtotal>0 and oltcode = @Outlet and KOTTblNo = @Table and KOTMaster.SubTable = @SubTable and KOTMaster.Branch_Code = @Branchcode";

            return await connection.QueryFirstOrDefaultAsync<dynamic>(selectquery, new { Table = table, SubTable = subtable, Outlet = outlet, Branchcode = branchcode });
        }

        public async Task<BillConfigModel> GetKOTConfig(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select BilltType as BillType, SubBillType from Bill_Config where IsReq = '1' and Branch_Code = @Branchcode and Config = 'KotNo' ";

            return await connection.QueryFirstOrDefaultAsync<BillConfigModel>(selectquery, new { Branchcode = branchCode });
        }

        public async Task<int> GetKOTNo(string subkotType, bool isNCKOT, string branchcode)
        {
            string str = string.Empty;
            var Kot = 0;

            using var connection = _factory.CreateConnection(DbNames.POS);

            if (subkotType == "S")
            {
                if (isNCKOT)
                    str = "Select ISNULL(Max(KOTNo),0) as LastRec from KOTMaster WITH (HOLDLOCK, ROWLOCK) Where POSCode = 1 And KOTChargeable=0 and Branch_Code = @Branchcode and DayEnd='N' ";

                else if (!isNCKOT)
                    str = "Select ISNULL(Max(KOTNo),0) as LastRec from KOTMaster WITH (HOLDLOCK, ROWLOCK) Where POSCode = 1 and KOTChargeable = 1 and Branch_Code = @Branchcode and DayEnd='N' ";
            }
            else
            {
                if (isNCKOT)
                    str = "Select ISNULL(MAX(KOTNo),0) as LastRec from KOTMaster WITH (HOLDLOCK, ROWLOCK) Where POSCode = 1 And KOTChargeable=0 and Branch_Code = @Branchcode";

                else if (!isNCKOT)
                    str = "Select ISNULL(MAX(KOTNo),0) as LastRec from KOTMaster WITH (HOLDLOCK, ROWLOCK) Where POSCode = 1 and KOTChargeable = 1 and Branch_Code = @Branchcode";

            }

            var value = await connection.QueryFirstOrDefaultAsync<int>(str, new { Branchcode = branchcode });
            if (value == 0)
            {
                Kot = 1;
            }
            else
            {
                var KOT = int.Parse(value.ToString());
                Kot = KOT + 1;
            }

            return Kot;
        }

        public async Task<DateTime> GetPOSEntryDate(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var hasBranchCode = await connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tbl_ShiftMaster' AND COLUMN_NAME = 'BranchCode'");

            var branchColumn = hasBranchCode > 0 ? "BranchCode" : "Branch_Code";

            var selectquery = @$"select max(ShiftDate) as PosEntryDate from Tbl_ShiftMaster where ShiftStatus = 'O' and {branchColumn} = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<DateTime>(selectquery, new { BranchCode = branchCode });
        }

        public async Task<FinancialMaster> GetFinancialMasters(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "SELECT FinId, FinFromDate, FinToDate , FincurrentYear , FinEndYear, CurrentStatus, LogUser, IpAddress, FinalClose, FinCode, BranchCode \r\n " +
                " FROM dbo.Tbl_Financial_Master \r\n " +
                " Where CurrentStatus = 1 AND FinalClose = 'N' AND  BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<FinancialMaster>(qry, new { Branch = branchcode }));
        }

        public async Task<KOTModel> SaveKOT(int Outlet, string Table, int Waiter, int Pax, DateTime POSEntryDate, double Total, int UserCode, bool Settled, bool Canceled,
            string SubTable, string BranchCode, string Type, string Remarks, int dkot, string CheckInNo, string GuestName, string GuestCode, string GuestMobileNo, string FinCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var param = new DynamicParameters();

            param.Add("@POSCode", "1");
            param.Add("@OltCode", Outlet);
            param.Add("@KOTTblNo", Table);
            param.Add("@StwCode", Waiter);
            param.Add("@KOTSeatsServed", Pax);
            param.Add("@KOTDate", POSEntryDate.Date); // ✅ better to pass DateTime
            param.Add("@KOTTime", DateTime.Now);

            param.Add("@KOTChargeable", Type == "N" ? false : true);

            param.Add("@KOTTotal", Total);
            param.Add("@CheckinNo", CheckInNo ?? "0");
            param.Add("@KOTGuestName", GuestName ?? "-");
            param.Add("@UserCode", UserCode);
            param.Add("@LastModify", DateTime.Now);
            param.Add("@NCKOT_Particulars", Remarks);
            param.Add("@KOTSettled", Settled);
            param.Add("@KOTCANCELLED", Canceled);
            param.Add("@SUBTABLE", SubTable ?? "A");
            param.Add("@DKOTNO", dkot);
            param.Add("@DAYEND", "N");
            param.Add("@guestcode", 0);
            param.Add("@Branch_Code", BranchCode);
            param.Add("@flag", 1);
            param.Add("@IsOnline", 0);
            param.Add("@KotMobileNo", GuestMobileNo);
            param.Add("@KotOrderNo", 0);
            param.Add("@FinCode", FinCode);

            param.Add("@KOTId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            param.Add("@KOTNo", dbType: DbType.Int32, direction: ParameterDirection.Output);

            await connection.ExecuteAsync("SaveKot", param, commandType: CommandType.StoredProcedure);

            return new KOTModel
            {
                KOTId = param.Get<int>("@KOTId"),
                KOTNo = param.Get<int>("@KOTNo")
            };
        }

        public async Task<int> GetNextDKOT(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"SELECT ISNULL(MAX(DKOTNo),0) as LastRec FROM KOTMaster WITH (HOLDLOCK, ROWLOCK)
            WHERE POSCode = 1 AND KOTChargeable = 1 AND Branch_Code = @Branchcode AND DayEnd='N'";

            var lastRec = await connection.QueryFirstOrDefaultAsync<int>(selectquery, new { Branchcode = branchcode });
            return lastRec + 1;
        }


        public async Task<bool> UpdateKOTMasterAsync(UpdateKOTMasterRequest request)
        {
            var updatequery = @" UPDATE KOTMASTER SET TransNo = @GuestCode, DRefKOTNo = @NCCode, ISUPLOADED = '0'
            WHERE KOTNO = @KOTNo AND KotTblNo = @Table AND SubTable = @SubTable AND KotMobileNo = @KotMobileNo  AND branch_code = @branchcode";

            using var connection = _factory.CreateConnection(DbNames.POS);
            var rows = await connection.ExecuteAsync(updatequery, new
            {
                request.GuestCode,
                request.NCCode,
                request.KOTNo,
                request.Table,
                request.SubTable,
                request.KotMobileNo,
                request.branchcode
            });

            return rows > 0;
        }

        public async Task<bool> UpdateTableReservationAsync(int kotNo, int reservationId)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @"UPDATE TableReservations SET KotId = @KOTNo WHERE Id = @ResId";
            var rows = await connection.ExecuteAsync(updatequery, new { KOTNo = kotNo, ResId = reservationId });

            return rows > 0;
        }

        public async Task<int> GetSpecialInfoId(string specialInfo, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"select spid from SpecialInformation where spinfo = @SpecialInfo and OltCode = @Branch ";

            var spid = await connection.QueryFirstOrDefaultAsync<int>(selectquery, new { SpecialInfo = specialInfo, Branch = branchCode });
            return spid;
        }

        public async Task<List<int>> GetSpecialInfoIds(string specialInfoCsv, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var allIds = new List<int>();

            // Split CSV and trim
            var parts = specialInfoCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim());

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int numericId))
                {
                    // Existing numeric ID
                    allIds.Add(numericId);
                }
                else
                {
                    var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                        "SELECT SPID FROM SpecialInformation WHERE SPINFO LIKE @Text AND OltCode = @Branch",
                        new { Text = "%" + part + "%", Branch = branch });

                    if (existingId.HasValue)
                    {
                        allIds.Add(existingId.Value);
                    }
                    else
                    {
                        // Insert new record and get ID
                        var newId = await connection.QuerySingleAsync<int>(
                            @"INSERT INTO SpecialInformation (SPINFO, OltCode)
                      OUTPUT INSERTED.SPID
                      VALUES (@Text, @Branch)",
                            new { Text = part, Branch = branch });

                        allIds.Add(newId);
                    }
                }
            }

            return allIds;
        }

        public async Task<dynamic> SaveKOTDetailAsync(SaveKOTDetailRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var parameters = new DynamicParameters();
            parameters.Add("@KOTId", request.KOTId);
            parameters.Add("@KOTNO", request.KOTNo);
            parameters.Add("@ItemCode", request.ItemCode);
            parameters.Add("@KOTDRate", request.KOTDRate);
            parameters.Add("@KOTDQty", request.KOTDQty);
            parameters.Add("@SplInst", request.SpecialInstId);
            parameters.Add("@Branch_Code", request.BranchCode);
            parameters.Add("@IsFree", request.IsFree);
            parameters.Add("@ItemDiscount", request.ItemDiscount);
            parameters.Add("@IsOnline", request.IsOnline);
            parameters.Add("@KNQty", request.KNQty);
            parameters.Add("@FinCode", request.FinCode);

            var result = await connection.QueryAsync<dynamic>("SaveKotDetail", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<string> GetOutletName(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select OltName from OutletMaster where OltCode= @Outlet and Branch_Code = @BranchCode";
            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { Outlet = outlet, BranchCode = branchCode });
        }

        public async Task<GlobalSettingsModel> GetGlobalSettings(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select InOrExOfTax,HappyHours,HHFrom,HHTo from TGeneralSettings Where BranchCode = @BranchCode";
            return await connection.QueryFirstOrDefaultAsync<GlobalSettingsModel>(selectquery, new { BranchCode = branchCode });
        }

        public async Task<IEnumerable<FreeItemDetail>> GetFreeItemsAsync(int outletCode, int itemCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @" SELECT FreeItemCode, FreeItemQty, IsFree FROM OltItemDetails WHERE OltCode = @OutletCode AND ItemCode = @ItemCode AND Branch_Code = @BranchCode";

            var freeItems = await connection.QueryAsync<FreeItemDetail>(selectquery, new { OutletCode = outletCode, ItemCode = itemCode, BranchCode = branchCode });

            return freeItems;
        }

        public async Task<dynamic> SaveFreeItemKOTDetailAsync(SaveKOTDetailRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var parameters = new DynamicParameters();
            parameters.Add("@KOTId", request.KOTId);
            parameters.Add("@KOTNO", request.KOTNo);
            parameters.Add("@ItemCode", request.ItemCode);
            parameters.Add("@KOTDRate", request.KOTDRate);
            parameters.Add("@KOTDQty", request.KOTDQty);
            parameters.Add("@SplInst", request.SpecialInstId);
            parameters.Add("@Branch_Code", request.BranchCode);
            parameters.Add("@IsFree", request.IsFree);
            parameters.Add("@ItemDiscount", request.ItemDiscount);
            parameters.Add("@IsOnline", request.IsOnline);
            parameters.Add("@KNQty", request.KNQty);

            var result = await connection.QueryAsync<dynamic>("SaveKotDetail", parameters, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<string> GetKotType(int outletCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"select KotType from kot_config where oltcode = @OutletCode and Branch_Code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { OutletCode = outletCode, BranchCode = branchCode });
        }

        public async Task<bool> InsertTmpKotPrintAsync(TmpKotPrintRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var insertquery = @" INSERT INTO dbo.tmpKotPrint (KotnO, StwNo, TblNo, KotDate, ItemName, qty, CatName, grpcode, ManualKotNo, itemcode) 
                VALUES (@KotNo, @WaiterNo, @TableNo, @KotDate, @ItemName, @Qty, @CategoryName, @GroupCode, @ManualKotNo, @ItemCode)";

            var rows = await connection.ExecuteAsync(insertquery, request);
            return rows > 0;
        }

        public async Task<bool> UpdateKotDetails(int kotId, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @"UPDATE KOTDetails SET ISUPLOADED = '0' WHERE KOTId = @KOTId and Branch_Code = @Branchcode";

            var rows = await connection.ExecuteAsync(updatequery, new { KOTId = kotId, Branchcode = branchcode });
            return rows > 0;
        }

        public async Task<ActiveKotDetail?> GetActiveKotAsync(string table, int outlet, string subTable, int itemCode, string branch, bool isChargeable)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @" SELECT km.kotid, km.kotno, kd.kid, kd.ItemCode, kd.KOTDQty FROM kotmaster km INNER JOIN kotdetails kd ON km.kotid = kd.kotid and kd.branch_code = km.branch_code 
              WHERE kotsettled = 0 AND RAWORNOT IS NULL AND kotcancelled = 0 AND kottblno = @Table AND km.OltCOde = @Outlet AND subtable = @SubTable AND kd.itemcode = @ItemCode AND km.Branch_Code = @Branch AND KotChargeable = @IsChargeable";

            return await connection.QueryFirstOrDefaultAsync<ActiveKotDetail>(sql, new { Table = table, Outlet = outlet, SubTable = subTable, ItemCode = itemCode, Branch = branch, IsChargeable = isChargeable ? 1 : 0 });
        }

        public async Task UpdateKotMasterForVoidAsync(int kotNo, string branch, string remarks)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @" UPDATE KOTMASTER SET RefKOTNo = @KotNo, remarks = @Remarks, ISUPLOADED = '0' WHERE KOTNO = @KotNo AND Branch_Code = @Branch";

            await connection.ExecuteAsync(sql, new { KotNo = kotNo, Remarks = remarks, Branch = branch });
        }

        public async Task GenerateCancelKotAsync(int kotId, int itemCode, int qty, int kid, string branch, bool isChargeable, string tableNo, string subTable, int userCode, string remarks)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Reduce quantity
                var updateQtySql = @" UPDATE KotDetails SET KotdQty = KotdQty - @Qty, Isuploaded = '0'
                                     WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid";

                await connection.ExecuteAsync(updateQtySql,
                    new { KotId = kotId, ItemCode = itemCode, Qty = qty, Branch = branch, Kid = kid }, transaction);

                var username = await connection.QueryFirstOrDefaultAsync<string>(
                                "SELECT UserName FROM UserMaster WHERE UserCode = @UserCode and Branch_code = @Branch",
                                new { UserCode = userCode, Branch = branch }, transaction);

                //string message = "KOT Has Been Void BY USER " + usernameget + " AT " + DateTime.Now.Date + " " + DateTime.Now.TimeOfDay + " KOT NO:" + kotid + " ITEMDETAILS:" + id + " ITEM QTY:" + qty + " REASON: '" + remarks + "' ";
                //this.SendSMS(message);

                // 2️⃣ Get updated quantity
                var detail = await connection.QueryFirstOrDefaultAsync<dynamic>(
                @"SELECT KOTDQty FROM KotDetails WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid",
                  new { KotId = kotId, ItemCode = itemCode, Branch = branch, Kid = kid }, transaction);

                if (detail != null && detail.KOTDQty == 0)
                {
                    // delete main item
                    await connection.ExecuteAsync(@"DELETE FROM KotDetails WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid",
                      new { KotId = kotId, ItemCode = itemCode, Branch = branch, Kid = kid }, transaction);

                    // delete estimation related items
                    await connection.ExecuteAsync(
                    @"DELETE FROM KotDetails WHERE Kotid = @KotId AND Branch_Code = @Branch AND KID = @Kid AND kotno IS NULL AND itemcode IN ( SELECT DISTINCT pro_code FROM estimationmaster WHERE ItemCode = @ItemCode AND Branch_Code = @Branch )",
                      new { KotId = kotId, ItemCode = itemCode, Branch = branch, Kid = kid }, transaction);
                }

                // 3️⃣ Check remaining items
                var remaining = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT COUNT(1) FROM KotDetails WHERE Kotid = @KotId AND Branch_Code = @Branch",
                  new { KotId = kotId, Branch = branch }, transaction);

                if (remaining == 0)
                {
                    await connection.ExecuteAsync(
                    @"DELETE FROM KotMaster WHERE Kotid = @KotId AND Branch_Code = @Branch",
                      new { KotId = kotId, Branch = branch }, transaction);
                }

                // 4️⃣ Auto settle orphan masters
                await connection.ExecuteAsync(@"
                        UPDATE KOTMaster SET KOTSettled = 1, Isuploaded = '0' WHERE kotid NOT IN ( SELECT kotid FROM KOTDetails WHERE Branch_Code = @Branch ) AND Branch_Code = @Branch AND KOTSettled = 0 AND KOTCancelled = 0 AND KotChargeable = @IsChargeable AND KOTTblNo = @TableNo AND SubTable = @SubTable",
                new { Branch = branch, IsChargeable = isChargeable ? 1 : 0, TableNo = tableNo, SubTable = subTable }, transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task InsertKotModifyDetailsAsync(int kotNo, int itemCode, int origQty, int userCode, DateTime lastModify, int outlet, int preQty, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @" INSERT INTO KOTModifyDetails (Kotid, ItemCode, ItemQty, UserCode, LastModify, outCode, Pres_ItemQty, Branch_Code)
              VALUES (@KotId, @ItemCode, @ItemQty, @UserCode, @LastModify, @Outlet, @PresQty, @Branch)";

            await connection.ExecuteAsync(sql, new { KotId = kotNo, ItemCode = itemCode, ItemQty = origQty, UserCode = userCode, LastModify = lastModify, Outlet = outlet, PresQty = preQty, Branch = branch });
        }

        public async Task<BillConfigModel> GetPOSBillConfig(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select BilltType as BillType, SubBillType from Bill_Config where IsReq = '1' and Branch_Code = @BranchCode and Config = 'BillNo' ";
            return await connection.QueryFirstOrDefaultAsync<BillConfigModel>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<OutletSelectModelType> GetOutletType(int oltcode, string branchcode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var qry = "select OutletCode, OutletName, OutletType from  TBl_OnlineOutlet_Type where OutletCode = @outletcode and BranchCode = @Branch";

            return await con.QueryFirstOrDefaultAsync<OutletSelectModelType>(qry, new { outletcode = oltcode, Branch = branchcode });

        }

        public async Task<int> GetNextGuestCodeAsync()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT ISNULL(MAX(GuestCode), 0) + 1 FROM GuestMaster";

            var guestCode = await connection.QueryFirstOrDefaultAsync<int>(sql);
            return guestCode;
        }

        public async Task<bool> InsertGuestAsync(HomeDelivery guest)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var insertquery = @"INSERT INTO GuestMaster (GuestCode, GuestName, Address, City, Phone, Remarks, Branch_code, Email)
            VALUES (@GuestCode, @GuestName, @Address, @City, @Phone, @Remarks, @BranchCode, @Email)";

            var rows = await connection.ExecuteAsync(insertquery, guest);
            return rows > 0;
        }

        public async Task<bool> UpdateGuestAsync(HomeDelivery guest)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var updatequery = @" UPDATE GuestMaster SET GuestName = @GuestName, Address = @Address, City = @City, Remarks = @Remarks, Email = @Email WHERE Phone = @Phone";

            var rows = await connection.ExecuteAsync(updatequery, guest);
            return rows > 0;
        }

        #endregion

        #region Scanner in POS GetBill

        public async Task<IEnumerable<ItemGroup>> GetItemGroupList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT GrpCode, GrpName, LastModify, Branch_Code, Dep FROM dbo.ItemGroup WHERE Branch_Code = @Branchcode";

            return await connection.QueryAsync<ItemGroup>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<KOTTaxModel> GetTaxCharges(int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select * from OutletMaster where OltCode = @Outlet and Branch_Code = @BranchCode ";

            return await connection.QueryFirstOrDefaultAsync<KOTTaxModel>(selectquery, new { Outlet = outlet, BranchCode = branchcode });
        }

        public async Task<double> LoadRule(string ruletype, double BillAmt, double CGST, double SGST, DateTime fromdate, string branchcode)
        {
            double TmpAmount = 0;
            double NetAmount = 0;
            double TType1, TType2, TType3;
            TType2 = 0;
            TType1 = 0;
            TType3 = 0;

            NetAmount = BillAmt + CGST + SGST;
            NetAmount = (Math.Round(NetAmount, 2));
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                var selectquery = "Select * from Tbl_Rule_Master where RuleType = @RuleType and @FromDate between FromDate and ToDate And BranchCode = @BranchCode";
                var record = connection.QueryFirstOrDefault<RuleModel>(selectquery, new { RuleType = ruletype, FromDate = fromdate.ToString("MM/dd/yyyy"), BranchCode = branchcode });
                if (record != null)
                {
                    switch (record.Type1)
                    {
                        case "Bill Amount":
                            TType1 = BillAmt;
                            break;
                        case "Net Amount":
                            TType1 = NetAmount;
                            break;
                        case "No":
                            TType1 = 0;
                            break;
                    }
                    switch (record.Type2)
                    {
                        case "CGST":
                            TType2 = CGST;
                            break;
                        case "SGST":
                            TType2 = SGST;
                            break;
                        case "No":
                            TType2 = 0;
                            break;
                    }
                    switch (record.Type3)
                    {
                        case "CGST":
                            TType3 = CGST;
                            break;
                        case "SGST":
                            TType3 = SGST;
                            break;
                        case "No":
                            break;
                    }
                    switch (record.Operation1)
                    {
                        case "+":
                            TmpAmount = TType1 + TType2;
                            break;
                        case "-":
                            TmpAmount = TType1 - TType2;
                            break;
                        case "*":
                            TmpAmount = TType1 * TType2;
                            break;
                        case "/":
                            TmpAmount = TType1 / TType2;
                            break;
                        case "No":
                            break;
                    }
                    switch (record.Operation2)
                    {
                        case "+":
                            TmpAmount = TmpAmount + TType3;
                            break;
                        case "-":
                            TmpAmount = TmpAmount - TType3;
                            break;
                        case "*":
                            TmpAmount = TmpAmount * TType3;
                            break;
                        case "/":
                            TmpAmount = TmpAmount / TType3;
                            break;
                        case "No":
                            break;
                    }
                }
                return TmpAmount;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Scanner in POS PostBill
        public async Task InsertNCSalesTaxBulk(List<NCSalesTax> taxes)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string insertquery = @"INSERT INTO NCSalesTax
                   (Bill_No, ItemCode, TaxCode, TaxAmount, Branch_Code, OltCode, BillDate, Ref)
                   VALUES(@Bill_No, @ItemCode, @TaxCode, @TaxAmount, @Branch_Code, @OltCode, @BillDate, @Ref)";

            await connection.ExecuteAsync(insertquery, taxes);
        }

        public async Task InsertSalesTaxBulk(List<SalesTax> taxes)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string insertquery = @"INSERT INTO SalesTax
                   (Bill_No, ItemCode, TaxCode, TaxAmount, Branch_Code, OltCode, BillDate, Ref, IsOnline, FinCode, TaxType)
                   VALUES(@Bill_No, @ItemCode, @TaxCode, @TaxAmount, @Branch_Code, @OltCode, @BillDate, @Ref, @IsOnline, @FinCode, @TaxType)";

            await connection.ExecuteAsync(insertquery, taxes);
        }

        public async Task InsertSalesGroupTaxBulk(int billId, string branchCode, int oltCode, KOTTaxModel taxData, DateTime POSEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // 1️⃣ Insert into parent table BillTax
                string insertParent = @"
            INSERT INTO BillTax
            (TotalAmount, TotalQty, CGSTAmt, SGSTAmt, ServiceChargePer, ServiceCharge, GrandTotal,
             DiscountPer, Discount, DiscountRemarks, RoundOff, billId , oltCode, branchCode, BillDate)
            VALUES
            (@TotalAmount, @TotalQty, @CGSTAmt, @SGSTAmt, @ServiceChargePer, @ServiceCharge, @GrandTotal,
             @DiscountPer, @Discount, @DiscountRemarks, @RoundOff, @billId , @oltCode, @branchCode, @billdate);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                // Get the generated BillTaxId
                int billTaxId = await connection.QuerySingleAsync<int>(
                    insertParent,
                    new
                    {
                        taxData.TotalAmount,
                        taxData.TotalQty,
                        taxData.CGSTAmt,
                        taxData.SGSTAmt,
                        taxData.ServiceChargePer,
                        taxData.ServiceCharge,
                        taxData.GrandTotal,
                        taxData.DiscountPer,
                        taxData.Discount,
                        taxData.DiscountRemarks,
                        taxData.RoundOff,
                        billId,
                        oltCode,
                        branchCode,
                        billdate = POSEntryDate
                    },
                    transaction
                );

                // 2️⃣ Insert child table BillTaxDetails
                string insertChild = @"
            INSERT INTO BillTaxDetails
            (BillTaxId, GroupCode, GroupName, TaxName, Taxper, TaxableAmount, TaxAmount, Total, CGST, SGST, billId , oltCode, branchCode, BillDate)
            VALUES
            (@billTaxId, @groupCode, @groupName, @taxName, @taxper, @taxableAmount, @taxAmount, @TotalAmount, @CGST, @SGST, @billId , @oltCode, @branchCode, @billdate);";

                foreach (var detail in taxData.TaxList)
                {
                    await connection.ExecuteAsync(insertChild, new
                    {
                        BillTaxId = billTaxId,
                        groupCode = 0,
                        groupName = "GroupName",
                        taxName = detail.TaxName,
                        taxper = detail.Taxper,
                        taxableAmount = detail.TaxableAmount,
                        taxAmount = detail.TaxAmount,
                        TotalAmount = detail.TaxableAmount + detail.TaxAmount, // ✅ FIX
                        CGST = taxData.CGSTAmt,
                        SGST = taxData.SGSTAmt,
                        BillId = billId,
                        OltCode = oltCode,
                        BranchCode = branchCode,
                        billdate = POSEntryDate
                    }, transaction);
                }

                //foreach (var detail in taxData.TaxList)
                //{
                //    connection.Execute(insertChild, new
                //    {
                //        BillTaxId = billTaxId,
                //        detail.GroupCode,
                //        detail.GroupName,
                //        detail.TaxName,
                //        detail.Taxper,
                //        detail.TaxableAmount,
                //        detail.TaxAmount,
                //        detail.TaxableAmount + detail.TaxAmount,
                //        taxData.CGSTAmt,
                //        taxData.SGSTAmt,
                //        billId,
                //        oltCode,
                //        branchCode
                //    }, transaction);
                //}

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> GetItemMasterGroupList(int itemcode, int catcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select GrpCode from ItemMaster WHERE ItemCode = @ItemCode AND CatCode = @CatCode AND Branch_Code = @Branchcode";
            return await connection.QueryFirstOrDefaultAsync<int>(selectquery, new { ItemCode = itemcode, CatCode = catcode, Branchcode = branchcode });
        }

        public async Task SaveDiscount(string billNo, KOTBillModel bill, DateTime posEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                double dsc = 0.0;

                //string updateKOT = @"UPDATE KOTSettlementMaster SET KSMBillDiscount = 0
                //             WHERE KsmBillNo = @BillNo AND OltCode = @outlet AND Branch_Code = @branch AND AccountType = 'F'";

                //connection.Execute(updateKOT, new { BillNo = billNo, outlet = bill.Cart.Outlet, branch = bill.Cart.Branch });


                if (bill.Cart.Discount == 0 || bill.Tax.Discount == 0)
                    dsc = 0.0;
                if (bill.Cart.Discount > 0 || bill.Tax.Discount > 0)
                {
                    dsc = bill.Cart.Discount;

                    string insertDiscount = @"INSERT INTO ItemDiscount
                                    (BillNo, BillDate, amount, grpcode, amountperc, discamount, Branch_Code, OltCode, Ref, IsOnline)
                                    VALUES
                                    (@BillNo, @BillDate, @Amount, @GrpCode, @AmountPerc, @DiscAmount, @Branchcode, @Outletcode, 'F' , 0)";

                    bool saved = await connection.ExecuteAsync(insertDiscount, new
                    {
                        BillNo = billNo,
                        BillDate = posEntryDate,
                        Amount = bill.Tax.TotalAmount,
                        GrpCode = 0,
                        AmountPerc = bill.Tax.DiscountPer,
                        DiscAmount = bill.Tax.Discount,
                        Branchcode = bill.Cart.Branch,
                        Outletcode = bill.Cart.Outlet
                    }) > 0;

                    //if (saved)
                    //{
                    //    string updateDiscount = @"UPDATE KOTSettlementMaster SET KSMBillDiscount = KSMBillDiscount + @Discount
                    //                      WHERE KsmBillNo = @BillNo AND OltCode = @Outletcode AND Branch_Code = @Branchcode AND AccountType = 'F'";

                    //    connection.Execute(updateDiscount, new
                    //    {
                    //        Discount = dsc,
                    //        BillNo = billNo,
                    //        Outletcode = bill.Cart.Outlet,
                    //        Branchcode = bill.Cart.Branch
                    //    });
                    //}
                }
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ExtraChargeModel>> GetExtraCharges(string itemcode, string branchcode, int outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var selectquery = "Select * from ExtraCharges E inner join TaxMaster T on E.ChargeCode = T.TaxCode and" +
            //    " E.ItemCode = @ItemCode and T.Branch_Code = @BranchCode and" +
            //    " E.Lastmodify is null and OltCode=@Outlet";

            var selectquery = "Select * from ExtraCharges E " +
                " Inner join BillTaxDescription T on E.ChargeCode = T.TaxCode And T.BranchCode = E.Branch_Code " +
                " Where E.ItemCode = @ItemCode and T.BranchCode = @BranchCode and OltCode = @Outlet";

            return (await connection.QueryAsync<ExtraChargeModel>(selectquery, new { ItemCode = itemcode, Outlet = outlet, BranchCode = branchcode })).ToList();

        }

        public async Task<KBillModel> SettleKOTPartI(DateTime posentrydate, KOTCartModel cart, double taxamount, double discount, string Reason, double RoundOff, int TokenNo, string BillingType, string SubBillingType, string GuestName, string GuestMobileNo, string fincode)
        {
            using (IDbConnection dbConnection = _factory.CreateConnection(DbNames.POS))
            {
                var dbillno = "0";
                var billno = "0";
                var billtime = DateTime.Now.ToString("HH:mm");
                double ksmbillamt = 0;
                cart.Food.ForEach(x =>
                {
                    ksmbillamt = ksmbillamt + (x.Qty * x.Price);
                });
                DynamicParameters param = new DynamicParameters();
                param.Add("@POSCode", "1");
                param.Add("@OltCode", cart.Outlet);
                param.Add("@KSMBillDate", posentrydate.ToString("MM/dd/yyyy"));
                param.Add("@KSMBillTime", billtime);
                param.Add("@KSMBillAmount", ksmbillamt);
                param.Add("@KSMBillTaxAmt", taxamount);
                param.Add("@KSMBillDiscount", discount);
                param.Add("@KSMBillCancled", 0);
                param.Add("@KSMBillSettled", 0);
                param.Add("@KSMTblNo", cart.Table);
                param.Add("@KSMSUBTBLNO", cart.SubTable);
                param.Add("@UserCode", cart.UserCode);
                param.Add("@LastModify", DateTime.Now);
                param.Add("@DiscountPercent", 0);
                param.Add("@KSMServiceTaxAmt", 0);
                param.Add("@DCParticulars", Reason);
                param.Add("@STEWCODE", cart.Waiter);
                param.Add("@DKSMBILLNO", dbillno);
                param.Add("@DAYEND", 'N');
                param.Add("@REASON", Reason);
                param.Add("@KSMBillNoofTime", 0);
                param.Add("@famt", RoundOff);
                param.Add("@fdamt", 0);
                param.Add("@fdper", 0);
                param.Add("@lamt", 0);
                param.Add("@ldamt", 0);
                param.Add("@ldper", 0);
                param.Add("@nop", cart.Pax);
                param.Add("@ksmservicecharge", 0);
                param.Add("@TokenNo", TokenNo);
                param.Add("@Branch_Code", cart.Branch);
                param.Add("@BillingType", "C");
                param.Add("@SubBillingType", SubBillingType);
                param.Add("@KSMSBCess", 0);
                param.Add("@KSMKKCess", 0);
                param.Add("@KSMId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@KSMBillNo", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@IsOnline", 0); //kasi
                param.Add("@KotGuestname", GuestName);
                param.Add("@KotMobileNo", GuestMobileNo);
                param.Add("@FinCode", fincode);


                var user = await dbConnection.ExecuteAsync("SaveKotSettlement", param, commandType: CommandType.StoredProcedure);

                var ksmid = 0;
                billno = "0";
                ksmid = param.Get<int>("@KSMId");
                billno = param.Get<int>("@KSMBillNo").ToString();

                return new KBillModel()
                {
                    ksmid = param.Get<int>("@KSMId"),
                    billno = param.Get<int>("@KSMBillNo").ToString()
                };

            }
        }

        public async Task<KBillModel> SettleNCKOTPartI(DateTime posentrydate, KOTCartModel cart, double taxamount, double discount, double SerTax, string Reason, double RoundOff, double SerCharge, int TokenNo, string BillingType, string SubBillingType, string fincode)
        {
            using (IDbConnection dbConnection = _factory.CreateConnection(DbNames.POS))
            {
                dbConnection.Open();

                // Calculate total KSM bill amount from cart items
                double ksmbillamt = 0;
                foreach (var item in cart.Food)
                {
                    ksmbillamt += item.Qty * item.Price;
                }

                var param = new DynamicParameters();

                param.Add("@POSCode", "1");
                param.Add("@OltCode", cart.Outlet);
                param.Add("@KSMBillDate", posentrydate);
                param.Add("@KSMBillTime", DateTime.Now.ToString("HH:mm"));
                param.Add("@KSMBillAmount", ksmbillamt);
                param.Add("@KSMBillTaxAmt", taxamount);
                param.Add("@KSMBillDiscount", discount);
                param.Add("@KSMBillCancled", 0);
                param.Add("@KSMBillSettled", 1);
                param.Add("@KSMTblNo", cart.Table);
                param.Add("@KSMSUBTBLNO", cart.SubTable);
                param.Add("@UserCode", cart.UserCode);
                param.Add("@LastModify", DateTime.Now);
                param.Add("@DiscountPercent", 0);
                param.Add("@KSMServiceTaxAmt", SerTax);
                param.Add("@DCParticulars", Reason);
                param.Add("@STEWCODE", cart.Waiter);
                param.Add("@DKSMBILLNO", "0"); // initial bill no
                param.Add("@DAYEND", 'N');
                param.Add("@REASON", Reason);
                param.Add("@KSMBillNoofTime", 0);
                param.Add("@famt", RoundOff);
                param.Add("@fdamt", 0);
                param.Add("@fdper", 0);
                param.Add("@lamt", 0);
                param.Add("@ldamt", 0);
                param.Add("@ldper", 0);
                param.Add("@nop", cart.Pax);
                param.Add("@ksmservicecharge", SerCharge);
                param.Add("@TokenNo", TokenNo);
                param.Add("@Branch_Code", cart.Branch);
                param.Add("@BillingType", BillingType);
                param.Add("@SubBillingType", SubBillingType);
                param.Add("@KSMId", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@KSMBillNo", dbType: DbType.Int32, direction: ParameterDirection.Output);
                param.Add("@IsOnline", 0);
                param.Add("@FinCode", fincode);

                // Execute the stored procedure
                await dbConnection.ExecuteAsync("SaveNCKotSettlement", param, commandType: CommandType.StoredProcedure);

                // Return the KBillModel populated from output parameters
                return new KBillModel
                {
                    ksmid = param.Get<int>("@KSMId"),
                    billno = param.Get<int>("@KSMBillNo").ToString()
                };
            }
        }

        public async Task<string> GetInfo(string tblname, string fieldname, string code, string parameter, int parameter1, string branch)
        {
            string info = string.Empty;
            try
            {
                string str = string.Empty;

                if (string.IsNullOrEmpty(parameter))
                {
                    str = "select " + fieldname + " as info from " + tblname + " where " + code + " = " + parameter1 + " and Branch_Code = '" + branch + "' ";
                }
                else
                    str = "select " + fieldname + " as info from " + tblname + " where " + code + " = '" + parameter + "' and Branch_Code = '" + branch + "' ";

                if (code.ToString().ToUpper() == "DKSMBillNo".ToString().ToUpper() && fieldname.ToUpper() == "KSMBillNo".ToUpper())
                    str = str + " and AccountType = 'F' ";

                if (tblname.ToUpper() == "SpecialInformation".ToUpper())
                    str = "select " + fieldname + " as info from " + tblname + " where " + code + " = '" + parameter + "' and oltCode = '" + branch + "' ";

                using var connection = _factory.CreateConnection(DbNames.POS);

                return await connection.QueryFirstOrDefaultAsync<string>(str);

            }
            catch
            {
                throw;
            }
        }

        public async Task<int> findhotelnextnumber(string table_name, string field_name)
        {
            string str;
            int trno = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.HMS);

                var selectquery = "select max(" + field_name + ") as trno from " + table_name + " ";

                var dr = await connection.QueryFirstOrDefaultAsync<dynamic>(selectquery);

                if (dr != null)
                {
                    if (string.IsNullOrEmpty(dr.trno.ToString()))
                    {
                        trno = trno + 1;
                    }
                    else
                    {
                        trno = Convert.ToInt32(dr.trno.ToString());
                        trno = trno + 1;
                    }
                }
                else
                {
                    trno = trno + 1;
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;

            }
            return trno;
        }

        public async Task<string> GetUserName(int userCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT UserName from UserMaster where UserCode= @UserCode and Branch_code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { UserCode = userCode, BranchCode = branchCode });
        }

        public async Task<bool> InsertTmpBillPrint(int waiter, int outlet, DateTime posEntryDate, string billType, string billNo, string tableNo, string userName, int ksmId, string merchantId, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string insertquery = @"INSERT INTO tmpbillprint
                   (SWT, PAX, ITM, RATE, BILLNO, TABNO, QTY, NETBILL, KSMID, STCC, Branch_Code)
                   VALUES
                   (@Waiter, @Outlet, @ItemDateTime, @ItemDate, @BillNo, @TableNo, @BillType, @UserName, @KsmId, @MerchantId, @BranchCode)";

            var parameters = new { Waiter = waiter, Outlet = outlet, ItemDateTime = posEntryDate.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToShortTimeString(), ItemDate = posEntryDate.ToString("dd/MM/yyyy"), BillType = billType, BillNo = billNo, TableNo = tableNo, UserName = userName, KsmId = ksmId, MerchantId = merchantId, BranchCode = branchCode };

            return await connection.ExecuteAsync(insertquery, parameters) > 0;
        }

        public async Task<string> GetKOTSettlementMaster(int billId, string billNo, int outletcode, string branchcode, string accounttype)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string selectquery = @"select TokenNo from KOTSettlementMaster where KSMId = @BillID and KSMBillNo = @BillNo and oltcode = @Outletcode and Branch_Code = @Brancode and AccountType = @accounttype";

            var parameters = new { BillId = billId, BillNo = billNo, Outletcode = outletcode, Brancode = branchcode, AccountType = accounttype };

            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, parameters);
        }

        public async Task<bool> SavePhonePeOrderBillDetails(string orderId, string billNo, DateTime billDate, int outletCode, string outletName, string billView, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"INSERT INTO Tbl_PhonePe_OrderBillDetails
                   (OrderId, Billno, BillDate, Outletcode, Outletname, BillView, Branch_Code)
                   VALUES
                   (@OrderId, @BillNo, @BillDate, @OutletCode, @OutletName, @BillView, @BranchCode)";

            return await connection.ExecuteAsync(sql, new
            {
                OrderId = orderId,
                BillNo = billNo,
                BillDate = billDate,
                OutletCode = outletCode,
                OutletName = outletName,
                BillView = billView,
                BranchCode = branchCode
            }) > 0;
        }

        public async Task<bool> UpdatePaymentStatus(string orderId, string billNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string updatequery = @"UPDATE paymentstatus SET POSBillno = @BillNo
                   WHERE orderId = @OrderId OR MerchantOrderId = @OrderId";

            return await connection.ExecuteAsync(updatequery, new { OrderId = orderId, BillNo = billNo }) > 0;
        }

        public async Task<bool> IsDirectSettlement(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = "SELECT OltIsParcelService FROM OutletMaster WHERE OltCode = @OutletCode AND branch_code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
        }


        public async Task<bool> SettleBill(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            if (await DOkotsettlement(settlement, posentrydate, Validdate))
            {
                string selectsql = @"SELECT *  FROM kotsettlementmaster WHERE KsmBillNo = @Billno AND oltcode = @Outletcode AND Branch_Code = @branchcode AND KSMBillDate = @BillDate";

                var ds = connection.QueryFirstOrDefault<dynamic>(selectsql, new { Billno = settlement.BillNo, Outletcode = settlement.OltCode, branchcode = settlement.BranchCode, BillDate = posentrydate });

                if (ds != null)
                {
                    string updateGuestLink = @"UPDATE tbl_guestlinking SET BillNo = @Billno, LinkStatus = 'N'
                                       WHERE TableNo = @TableNo AND BillNo IS NULL";

                    connection.Execute(updateGuestLink, new { Billno = settlement.BillNo, TableNo = ds.KSMTblNo });
                }

                double gpoints = 0;

                string pointQuery = @"SELECT SUM(ISNULL(gpoint,0)) AS Points FROM view_guestpoints WHERE Billno = @BillNo AND OltCode = @Outletcode";

                var dsp = connection.QueryFirstOrDefault<dynamic>(pointQuery, new { Billno = settlement.BillNo, Outletcode = settlement.OltCode });

                if (dsp != null && dsp.Points != null)
                    gpoints = dsp.Points;

                string updatePoints = @"UPDATE tbl_guestlinking SET PointsEarned = @Points
                                WHERE TableNo = @TableNo AND Billno = @BillNo";

                connection.Execute(updatePoints, new { Points = gpoints, TableNo = ds?.KSMTblNo, Billno = settlement.BillNo });
            }
            return true;
        }

        public async Task<bool> DOkotsettlement(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int lngKSBId = 0;
            bool saved = false;
            double ntotal = settlement.GrandAmount;
            int companycode = 0;

            string checkroomservice = await GetInfo("OutletMaster", "OltIsRoomService", "OltCode", settlement.OltCode.ToString(), 0, settlement.BranchCode);

            try
            {
                string dttime = DateTime.Now.TimeOfDay.ToString();
                DateTime posDate = posentrydate;

                foreach (var payment in settlement.PaymentDetails)
                {
                    if (payment.Mode == "Transfer to Company")
                    {
                        lngKSBId = await findnextnumber("BillTransferToCompany", "BTId", "Branch_Code", settlement.BranchCode);
                    }
                    else if (payment.Mode == "Transfer to Room")
                    {

                    }
                    else
                    {
                        lngKSBId = await findnextnumber("KOTBillSettlement", "KBSId", "Branch_Code", settlement.BranchCode);
                    }

                    if (payment.Mode == "Transfer to Company")
                    {
                        var selectquery = "select CompanyCode From CompanyMaster Where Branch_code = @branchcode AND CompanyName LIKE @Cmpname ";
                        companycode = await connection.QueryFirstOrDefaultAsync<int>(selectquery, new { branchcode = settlement.BranchCode, Cmpname = payment.SubMode });

                        string sql = @"INSERT INTO BillTransferToCompany
                        (BTId, POSCode, CompanyCode, BTDate, BTTime, BillNo, BillAmt, UserCode, LastModify, BTCSettled, AmtPaid, Remarks, 
                        oltcode, Discount, Branch_Code, Isuploaded, Ismodified)
                        VALUES
                        (@Id, '1', @Company, @Date, @Time, @billNo, @amount, @User, @Modify, 0, 0, @remarks, @Outlet, @discount, @Branch, '0', '0')";

                        saved = connection.Execute(sql, new
                        {
                            Id = lngKSBId,
                            Company = companycode,
                            Date = posDate,
                            Time = Validdate.ToString(@"hh\:mm\:ss"),
                            billNo = settlement.BillNo,
                            amount = payment.Amount,
                            User = settlement.UserCode,
                            Modify = dttime,
                            remarks = payment.Remarks,
                            Outlet = settlement.OltCode,
                            discount = settlement.Discount,
                            Branch = settlement.BranchCode
                        }) > 0;

                    }
                    else if (payment.Mode == "Transfer to Room")
                    {

                    }
                    else
                    {
                        string sql = @"INSERT INTO KOTBillSettlement
                        (KBSId, POSCode, OltCode, KSMId, KSMBillNo, KSMBillAmount,
                        KBSSetteleDate, KBSPaymentMode, KBSRefNo, KBSRefName, KBSValidDate, 
                        KBSRoomNo, KBSDiscount, UserCode, LastModify, Branch_Code, tips, 
                        AccountType, Isuploaded, Ismodified, DAYEND)
                        VALUES
                        (@Id, '1', @Outlet, @KSMId, @billNo, @amount, @Date, 
                        @mainmode, @refNo, @submode, @validDate, '0', @discount, @User, @Time, 
                        @Branch, @Tips, 'F', '0', '0', 'N')";

                        saved = connection.Execute(sql, new
                        {
                            Id = lngKSBId,
                            Outlet = settlement.OltCode,
                            KSMId = settlement.BillId,
                            billNo = settlement.BillNo,
                            amount = payment.Amount,
                            Date = posDate,
                            refNo = payment.Remarks,
                            mainmode = payment.Mode,
                            submode = payment.SubMode,
                            validDate = Validdate,
                            discount = settlement.Discount,
                            User = settlement.UserCode,
                            Time = dttime,
                            Branch = settlement.BranchCode,
                            Tips = settlement.ChangeAmount
                        }) > 0;

                    }

                    //if (settlement.PayMode.ToLower() == "card")
                    //{
                    //    string sql = @"INSERT INTO KOTBillSettlement
                    //    (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                    //    KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                    //    KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                    //    AccountType,Isuploaded,Ismodified,DAYEND)
                    //    VALUES
                    //    (@Id,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
                    //    'Card',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
                    //    @Time,@Branch,@Tips,'F','0','0','N')";

                    //    saved = connection.Execute(sql, new
                    //    {
                    //        Id = lngKSBId,
                    //        Outlet = settlement.OutletCode,
                    //        KSMId = settlement.Bill.BillId,
                    //        BillNo = settlement.Bill.BillNo,
                    //        Amount = settlement.Bill.GrandAmount,
                    //        Date = posDate,
                    //        RefNo = settlement.Bill.RefNo,
                    //        RefName = settlement.Bill.CardName,
                    //        validDate = settlement.Bill.ValidDate,
                    //        Discount = settlement.Bill.Discount,
                    //        User = settlement.UserCode,
                    //        Time = dttime,
                    //        Branch = settlement.Branch,
                    //        Tips = settlement.Bill.ChangeAmount
                    //    }) > 0;
                    //}

                    //if (settlement.PayMode.ToLower() == "upi")
                    //{
                    //    string sql = @"INSERT INTO KOTBillSettlement
                    //    (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                    //    KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                    //    KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                    //    AccountType,Isuploaded,Ismodified,DAYEND)
                    //    VALUES
                    //    (@Id,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
                    //    'UPI',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
                    //    @Time,@Branch,@Tips,'F','0','0','N')";

                    //    saved = connection.Execute(sql, new
                    //    {
                    //        Id = lngKSBId,
                    //        Outlet = settlement.OutletCode,
                    //        KSMId = settlement.Bill.BillId,
                    //        BillNo = settlement.Bill.BillNo,
                    //        Amount = settlement.Bill.GrandAmount,
                    //        Date = posDate,
                    //        RefNo = settlement.Bill.RefNo,
                    //        RefName = settlement.Bill.CardName,
                    //        validDate = settlement.Bill.ValidDate,
                    //        Discount = settlement.Bill.Discount,
                    //        User = settlement.UserCode,
                    //        Time = dttime,
                    //        Branch = settlement.Branch,
                    //        Tips = settlement.Bill.ChangeAmount
                    //    }) > 0;
                    //}

                    //if (settlement.PayMode.ToLower() == "online")
                    //{
                    //    string sql = @"INSERT INTO KOTBillSettlement
                    //    (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                    //    KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                    //    KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                    //    Accounttype,Isuploaded,Ismodified,DAYEND)
                    //    VALUES
                    //    (@Id,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
                    //    'Online',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
                    //    @Time,@Branch,@Tips,'F','0','0','N')";

                    //    saved = connection.Execute(sql, new
                    //    {
                    //        Id = lngKSBId,
                    //        Outlet = settlement.OutletCode,
                    //        KSMId = settlement.Bill.BillId,
                    //        BillNo = settlement.Bill.BillNo,
                    //        Amount = settlement.Bill.GrandAmount,
                    //        Date = posDate,
                    //        RefNo = settlement.Bill.RefNo,
                    //        RefName = settlement.Bill.CardName,
                    //        ValidDate = settlement.Bill.ValidDate,
                    //        Discount = settlement.Bill.Discount,
                    //        User = settlement.UserCode,
                    //        Time = dttime,
                    //        Branch = settlement.Branch,
                    //        Tips = settlement.Bill.Tips
                    //    }) > 0;
                    //}

                    //if (settlement.PayMode.ToLower() == "cheque")
                    //{
                    //    string sql = @"INSERT INTO KOTBillSettlement
                    //    (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                    //    KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                    //    KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                    //    Accounttype,Isuploaded,Ismodified,DAYEND)
                    //    VALUES
                    //    (@Id,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
                    //    'Cheque',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
                    //    @Time,@Branch,@Tips,'F','0','0','N')";

                    //    saved = connection.Execute(sql, new
                    //    {
                    //        Id = lngKSBId,
                    //        Outlet = settlement.OutletCode,
                    //        KSMId = settlement.Bill.BillId,
                    //        BillNo = settlement.Bill.BillNo,
                    //        Amount = settlement.Bill.GrandAmount,
                    //        Date = posDate,
                    //        RefNo = settlement.Bill.RefNo,
                    //        RefName = settlement.Bill.CardName,
                    //        validDate = settlement.Bill.ValidDate,
                    //        Discount = settlement.Bill.Discount,
                    //        User = settlement.UserCode,
                    //        Time = dttime,
                    //        Branch = settlement.Branch,
                    //        Tips = settlement.Bill.ChangeAmount
                    //    }) > 0;
                    //}

                    if (saved)
                    {
                        if (checkroomservice == "True")
                        {
                            connection.Execute(@"UPDATE KOTSettlementMaster
                    SET Ismodified='1',Isuploaded='0',KSMBillSettled=1, KSMBillTransfered=1,KSMIsRoomService=1, KsmSettledAmt=0,tips=@Tips
                    WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
                                new { Tips = settlement.Tips, Id = settlement.BillId, Branch = settlement.BranchCode });

                            connection.Execute(@"UPDATE Tbl_FoodBills
                    SET BillStatus='Y'
                    WHERE RcptNo=@BillNo AND OutlateName=@Outlet",
                                new { BillNo = settlement.BillNo, Outlet = settlement.OltCode });
                        }

                        if (payment.Mode == "Transfer to Company")
                        {
                            connection.Execute(@"UPDATE KOTSettlementMaster
                    SET Ismodified='1',Isuploaded='0', KSMBillSettled=1,KSMBillTransfered=1, KSMTblNo=@Company,tips=@Tips
                    WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
                                new
                                {
                                    Company = companycode,
                                    Tips = settlement.Tips,
                                    Id = settlement.BillId,
                                    Branch = settlement.BranchCode
                                });
                        }
                        else
                        {
                            connection.Execute(@"UPDATE KOTSettlementMaster
                    SET Ismodified='1',Isuploaded='0', KSMBillSettled=1,tips=@Tips
                    WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
                                new
                                {
                                    Tips = settlement.Tips,
                                    Id = settlement.BillId,
                                    Branch = settlement.BranchCode
                                });
                        }
                    }
                }
            }
            catch
            {
                throw;
            }

            return saved;
        }
        
        public async Task<int> findnextnumber(string table_name, string field_name, string field_name1, string branch)
        {
            string str;
            int trno = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                if (table_name == "ITEMMASTER")
                {
                    str = "select ISNULL(MAX(" + field_name + "),0) as trno from " + table_name + " WITH (HOLDLOCK, ROWLOCK) where SubItem = 0 And " + field_name1 + " = '" + branch + "'";
                }
                else
                {
                    str = "select ISNULL(MAX(" + field_name + "),0) as trno from " + table_name + " WITH (HOLDLOCK, ROWLOCK) where " + field_name1 + " = '" + branch + "'";
                }

                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str);

                if (ds.trno != null)
                {
                    if (trno == 0)
                    {
                        if (string.IsNullOrEmpty(ds.trno.ToString()))
                        {
                            trno = trno + 1;
                        }
                        else
                        {
                            trno = Convert.ToInt32(ds.trno);
                            trno = trno + 1;
                        }
                    }
                    else
                    {
                        trno = trno + 1;
                    }
                }
                else
                {
                    trno = trno + 1;
                }
            }
            catch
            {
                throw;
            }
            return trno;
        }

        public async Task<bool> IsFastFoodDirectSettlement(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT OltIsFastFood from OutletMaster where OltCode = @OutletCode AND branch_code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
        }

        public async Task<bool> PhonepeSettleBill(SettlementModel settlement, KOTPhonePeCollectResponseBody payment, DateTime posentrydate, DateTime Validdate)
        {

            if (await PhonepeDOkotsettlement(settlement, payment, posentrydate, Validdate))
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                string selectsql = @"SELECT * FROM kotsettlementmaster WHERE KsmBillNo = @Billno AND oltcode = @Outletcode AND KSMBillDate = @BillDate And Branch_Code = @BranchCode";

                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(selectsql, new { Billno = settlement.Bill.BillNo, Outletcode = settlement.OutletCode, BillDate = posentrydate, BranchCode = settlement.Branch });

                if (ds != null)
                {
                    string updateGuestLink = @"UPDATE tbl_guestlinking SET BillNo = @Billno, LinkStatus = 'N'
                                       WHERE TableNo = @TableNo AND BillNo IS NULL";

                    await connection.ExecuteAsync(updateGuestLink, new { Billno = settlement.Bill.BillNo, TableNo = ds.KSMTblNo });
                }

                double gpoints = 0;

                string pointQuery = @"SELECT SUM(ISNULL(gpoint,0)) AS Points FROM view_guestpoints WHERE Billno = @BillNo AND OltCode = @Outletcode";

                var dsp = await connection.QueryFirstOrDefaultAsync<dynamic>(pointQuery, new { Billno = settlement.Bill.BillNo, Outletcode = settlement.OutletCode });

                if (dsp != null && dsp.Points != null)
                    gpoints = dsp.Points;

                string updatePoints = @"UPDATE tbl_guestlinking SET PointsEarned = @Points
                                WHERE TableNo = @TableNo AND Billno = @BillNo";

                await connection.ExecuteAsync(updatePoints, new { Points = gpoints, TableNo = ds?.KSMTblNo, Billno = settlement.Bill.BillNo });
            }
            return true;
        }

        public async Task<bool> PhonepeDOkotsettlement(SettlementModel settlement, KOTPhonePeCollectResponseBody payment, DateTime posentrydate, DateTime Validdate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int lngKSBId = 0;
            bool saved = false;
            double ntotal = settlement.Bill.GrandAmount;

            string checkroomservice = await GetInfo("OutletMaster", "OltIsRoomService", "OltCode", settlement.OutletCode.ToString(), 0, settlement.Branch);

            try
            {
                lngKSBId = await findnextnumber("KOTBillSettlement", "KBSId", "Branch_Code", settlement.Branch);

                if (settlement.PayMode == "company")
                {
                    lngKSBId = await findnextnumber("BillTransferToCompany", "BTId", "Branch_Code", settlement.Branch);
                }

                string dttime = DateTime.Now.TimeOfDay.ToString();
                DateTime posDate = posentrydate;

                if (settlement.SubBillingType != "S")
                {
                    string deleteSql = @"DELETE FROM KOTBillSettlement WHERE KSMBillNo=@BillNo AND OltCode=@OutletCode AND Branch_Code = @BranchCode";

                    connection.Execute(deleteSql, new { BillNo = settlement.Bill.BillNo, OutletCode = settlement.OutletCode, BranchCode = settlement.Branch });
                }

                if (settlement.PayMode.ToLower() == "cash")
                {
                    string sql = @"INSERT INTO KOTBillSettlement
                          (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                           KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                           KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                           AccountType,Isuploaded,Ismodified,DAYEND)
                           VALUES
                          (@KBSId,'1',@Outlet,@KSMId,@billNo,@Amount,@Date,
                           'CASH','0','Cash',@Date,'0',@discount,@User,@Time,
                           @branch,@Tips,'F','0','0','N')";

                    saved = connection.Execute(sql, new
                    {
                        KBSId = lngKSBId,
                        Outlet = settlement.OutletCode,
                        KSMId = settlement.Bill.BillId,
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        Date = posDate,
                        discount = settlement.Bill.Discount,
                        User = settlement.UserCode,
                        Time = dttime,
                        branch = settlement.Branch,
                        Tips = settlement.Bill.ChangeAmount
                    }) > 0;
                }

                if (settlement.PayMode.ToLower() == "card")
                {
                    string sql = @"INSERT INTO KOTBillSettlement
                          (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                           KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                           KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                           AccountType,Isuploaded,Ismodified,DAYEND)
                           VALUES
                          (@KBSId,'1',@Outlet,@KSMId,@billNo,@Amount,@Date,
                           'CARD',@refNo,@RefName,@ValidDate,'0',@discount,@User,
                           @Time,@branch,@Tips,'F','0','0','N')";

                    saved = connection.Execute(sql, new
                    {
                        KBSId = lngKSBId,
                        Outlet = settlement.OutletCode,
                        KSMId = settlement.Bill.BillId,
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        Date = posDate,
                        refNo = settlement.Bill.RefNo,
                        RefName = settlement.Bill.CardName,
                        ValidDate = Validdate,
                        discount = settlement.Bill.Discount,
                        User = settlement.UserCode,
                        Time = dttime,
                        branch = settlement.Branch,
                        Tips = settlement.Bill.ChangeAmount
                    }) > 0;
                }

                if (settlement.PayMode.ToLower() == "cheque")
                {
                    string sql = @"INSERT INTO KOTBillSettlement
                          (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                           KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                           KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                           Accounttype,Isuploaded,Ismodified,DAYEND)
                           VALUES
                          (@KBSId,'1',@Outlet,@KSMId,@billNo,@Amount,@Date,
                           'CHEQUE',@refNo,@refName,@ValidDate,'0',@discount,@User,
                           @Time,@branch,@Tips,'F','0','0','N')";

                    saved = connection.Execute(sql, new
                    {
                        KBSId = lngKSBId,
                        Outlet = settlement.OutletCode,
                        KSMId = settlement.Bill.BillId,
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        Date = posDate,
                        refNo = settlement.Bill.RefNo,
                        refName = settlement.Bill.CardName,
                        ValidDate = Validdate,
                        discount = settlement.Bill.Discount,
                        User = settlement.UserCode,
                        Time = dttime,
                        branch = settlement.Branch,
                        Tips = settlement.Bill.ChangeAmount
                    }) > 0;
                }

                if (settlement.PayMode.ToLower() == "online")
                {
                    string sql = @"INSERT INTO KOTBillSettlement
                          (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                           KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                           KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                           Accounttype,Isuploaded,Ismodified,DAYEND)
                           VALUES
                          (@KBSId,'1',@Outlet,@KSMId,@billNo,@Amount,@Date,
                           'UPI',@TxnId,@refName,@ValidDate,'0',@discount,@User,
                           @Time,@branch,@tips,'F','0','0','N')";

                    saved = connection.Execute(sql, new
                    {
                        KBSId = lngKSBId,
                        Outlet = settlement.OutletCode,
                        KSMId = settlement.Bill.BillId,
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        Date = posDate,
                        TxnId = payment.data.transactionId,
                        refName = settlement.Bill.CardName,
                        ValidDate = Validdate,
                        discount = settlement.Bill.Discount,
                        User = settlement.UserCode,
                        Time = dttime,
                        branch = settlement.Branch,
                        tips = settlement.Bill.Tips
                    }) > 0;
                }

                if (settlement.PayMode.ToLower() == "pluxee")
                {
                    string sql = @"INSERT INTO KOTBillSettlement
                          (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
                           KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
                           KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
                           Accounttype,Isuploaded,Ismodified,DAYEND)
                           VALUES
                          (@KBSId,'1',@Outlet,@KSMId,@billNo,@Amount,@Date,
                           'PLUXEE',@TxnId,@refName,@ValidDate,'0',@discount,@User,
                           @Time,@branch,@tips,'F','0','0','N')";

                    saved = connection.Execute(sql, new
                    {
                        KBSId = lngKSBId,
                        Outlet = settlement.OutletCode,
                        KSMId = settlement.Bill.BillId,
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        Date = posDate,
                        TxnId = payment.data.transactionId,
                        refName = settlement.Bill.CardName,
                        ValidDate = Validdate,
                        discount = settlement.Bill.Discount,
                        User = settlement.UserCode,
                        Time = dttime,
                        branch = settlement.Branch,
                        tips = settlement.Bill.Tips
                    }) > 0;
                }

                if (settlement.PayMode.ToLower() == "company")
                {
                    string sql = @"INSERT INTO BillTransferToCompany
                          (BTId,POSCode,CompanyCode,BTDate,BTTime,BillNo,BillAmt,
                           UserCode,LastModify,BTCSettled,AmtPaid,Remarks,
                           oltcode,Discount,Branch_Code,Isuploaded,Ismodified)
                           VALUES
                          (@Id,'1',@Company,@Date,@Time,@billNo,@Amount,
                           @User,@Modify,0,0,@remarks,@Outlet,@discount,
                           @branch,'0','0')";

                    saved = connection.Execute(sql, new
                    {
                        Id = lngKSBId,
                        Company = settlement.CompanyCode,
                        Date = posDate,
                        Time = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss"),
                        billNo = settlement.Bill.BillNo,
                        Amount = settlement.Bill.GrandAmount,
                        User = settlement.UserCode,
                        Modify = dttime,
                        remarks = settlement.Remarks,
                        Outlet = settlement.OutletCode,
                        discount = settlement.Bill.Discount,
                        branch = settlement.Branch
                    }) > 0;

                    int rno = await findhotelnextnumber("Tbl_OutStanding_Slave", "Rno");

                    string billno = "POS" + Convert.ToInt64(settlement.Bill.BillNo).ToString("10000000");

                    string sql2 = @"INSERT INTO Tbl_OutStanding_Slave
                           VALUES (@Rno,@BillNo,'0',@Date,@Time,'0',@Guest,0,'0','0',0,
                                   '0','0','0',0,'0','0','0',@companyName,@companyCode,
                                   @Amount,0,@Amount,'S','S','S',@User,'0',@Date,
                                   @Amount,'Tr','0','0',@Outlet)";

                    connection.Execute(sql2, new
                    {
                        Rno = rno,
                        BillNo = billno,
                        Date = posDate,
                        Time = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss"),
                        Guest = settlement.GuestName,
                        companyName = settlement.CompanyName,
                        companyCode = settlement.CompanyCode,
                        Amount = settlement.Bill.GrandAmount,
                        User = settlement.UserCode,
                        Outlet = settlement.OutletCode
                    });
                }

                if (saved)
                {
                    if (checkroomservice == "True")
                    {
                        connection.Execute(@"UPDATE KOTSettlementMaster 
                        SET Ismodified = '1', Isuploaded = '0', KSMBillSettled = 1, KSMBillTransfered = 1, KSMIsRoomService = 1, KsmSettledAmt = 0, tips=@tips
                        WHERE KSMId = @Id AND Branch_Code = @branch AND AccountType = 'F'",
                        new { tips = settlement.Bill.Tips, Id = settlement.Bill.BillId, branch = settlement.Branch });

                        connection.Execute(@"UPDATE Tbl_FoodBills SET BillStatus = 'Y'
                        WHERE RcptNo = @billNo AND OutlateName = @Outlet", 
                        new { billNo = settlement.Bill.BillNo, Outlet = settlement.OutletName });
                    }

                    if (settlement.PayMode == "company")
                    {
                        connection.Execute(@"UPDATE KOTSettlementMaster 
                        SET Ismodified='1', Isuploaded='0', KSMBillSettled=1, KSMBillTransfered=1, KSMTblNo=@Company, tips=@tips
                        WHERE KSMId = @Id AND Branch_Code = @branch AND AccountType = 'F'",
                        new
                        {
                            Company = settlement.CompanyCode,
                            tips = settlement.Bill.Tips,
                            Id = settlement.Bill.BillId,
                            branch = settlement.Branch
                        });

                    }
                    else
                    {
                        connection.Execute(@"UPDATE KOTSettlementMaster 
                        SET Ismodified='1', Isuploaded='0', KSMBillSettled=1, tips=@tips
                        WHERE KSMId = @Id AND Branch_Code = @branch AND AccountType = 'F'",
                         new
                         {
                             tips = settlement.Bill.Tips,
                             Id = settlement.Bill.BillId,
                             branch = settlement.Branch
                         });
                    }
                }
            }
            catch
            {
                throw;
            }

            return saved;
        }

        public async Task<bool> IsDirectBillSettlementOnline(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT IsDirectBill from TBl_OnlineOutlet_Type where OutletCode = @OutletCode AND BranchCode = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
        }
        #endregion

        #region WhatsAppConfiguration

        public async Task<Whatsupconfig> WhatsappConfiguration()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT * FROM Tbl_TryOWBot_Config";
            return await connection.QueryFirstOrDefaultAsync<Whatsupconfig>(sql);
        }
        #endregion

        #region OnlinePaymentType

        public async Task<OnlinePaymentTypeModel> OnlinePaymentType()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = @"SELECT IsQRActive FROM Tbl_OnlinePaymentType";
            return await connection.QueryFirstOrDefaultAsync<OnlinePaymentTypeModel>(sql);
        }
        #endregion
    }
}
