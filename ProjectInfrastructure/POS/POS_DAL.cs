using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace HMS_360_PMS.DAL_Layers.POS
{
    public class POS_DAL : IPOS_Repository
    {
        private readonly DbConnectionFactory _factory;

        public POS_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<IEnumerable<Branch>> GetBranchesByUsername(string username)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"SELECT b.*FROM Tbl_branch b INNER JOIN UserMaster u ON b.Branch_code = u.Branch_code WHERE LOWER(u.UserName) = @Username ";
            //var selectquery = @"SELECT b.*FROM Tbl_branch b INNER JOIN UserMaster u ON b.Branch_code = u.Branch_code WHERE u.UserName LIKE @Username ";
            //return await connection.QueryAsync<Branch>(selectquery, new { Username = $"%{username}%" });

            return await connection.QueryAsync<Branch>(selectquery, new { Username = username.ToLower() });
        }

        public async Task<CompanyInfo> GetCompanyInfoByBranchCode(string branchcode, int companycode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = $@"SELECT Company_Name, Company_code, StartYear, Address1, Address2, Phone_number, Mob_number, OwnerName, 
                          Owner_Number, Fax_number, Email_id, Tin_no, Licence_number, Branch_code, STDCODE
                          FROM dbo.Tbl_companyinfo
                          WHERE Company_code = @CompanyCode ";

            return await connection.QueryFirstOrDefaultAsync<CompanyInfo>(selectquery, new { CompanyCode = companycode });
        }

        public async Task<UserMaster> GetUserByUsername(string username, string branchid)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = $@"SELECT UserCode, UserName, UserPassword, UserPrivilege, EnteredBy, LastModify, 
                          Branch_code, storeid, TabId, DisPercent, DisAmount, RoleId 
                          FROM dbo.UserMaster
                          WHERE UserName = @Username and Branch_code = @BranchID ";

            return await connection.QueryFirstOrDefaultAsync<UserMaster>(selectquery, new { Username = username, BranchID = branchid });
        }

        public async Task InsertUserLog(int userid, string branchid, DateTime logintime)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var insertquery = @"INSERT INTO User_logs
                  (logintime, logdate, userid, branch_code)
                  VALUES
                  (@logtime, CAST(GETDATE() AS DATE), @UserId, @BranchCode)";

            await connection.ExecuteAsync(insertquery, new { logtime = logintime, UserId = userid, BranchCode = branchid });
        }

        public async Task<IEnumerable<BillConfig>> GetBillConfig(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string selectquery = @"SELECT BilltType, isreq, Branch_Code, SubBillType, Config FROM bill_config WHERE Branch_Code = @BranchCode";

            var result = await connection.QueryAsync<BillConfig>(selectquery, new { BranchCode = branchcode });
            return result;
        }

        public async Task<IEnumerable<PosUserRightsAccess>> GetPosUserAccessRight(int usercode, string username, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string selectquery = "select ura.UserName, ura.UserId, ura.IsNcKotAccess, ura.IsCancelKotAccess, \r\n" +
                " ura.IsVoidKotAccess, ura.IsTodayAccess, ura.IsSplitBillAccess, ura.IsSettlementAccess, ura.Branch_Code, \r\n" +
                " um.storeid, um.DisPercent, um.DisAmount \r\n" +
                " From dbo.Tbl_PosUserRights_access ura \r\n" +
                " Left Join dbo.UserMaster um on um.UserCode = ura.UserId And um.Branch_code = ura.Branch_Code \r\n" +
                " Where ura.UserId = @Usercode and ura.UserName = @UserName and ura.Branch_Code = @BranchCode";

            var result = await connection.QueryAsync<PosUserRightsAccess>(selectquery, new { Usercode = usercode, UserName = username, BranchCode = branchcode });
            return result;
        }

        public async Task<ProductLicenceModel> GetProductLicenceKey(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = " select  SerialKey, ProductKey, TrDate, ValidDate, NoDays, IntimationDate, IndimationDays, ClientName, MachineName, ServerName, Status, BranchCode, Encryptedserialkey, EncryptedToDate, EncryptedIntimationDate " +
                " From Tbl_Config_Man WHERE BranchCode = @BranchCode";

            var result = await connection.QueryFirstOrDefaultAsync<ProductLicenceModel>(sql, new { BranchCode = branchCode });
            return result;
        }

        public async Task<IEnumerable<SystemOutletModel>> GetSystemOutlet()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select RNo, SystemName, OltCode, Temp1, Temp2, Temp3, Temp4, LastModify From Tbl_System_Outlet";

            return await connection.QueryAsync<SystemOutletModel>(selectquery);
        }

        public async Task<IEnumerable<OutletMaster>> GetReprintOutletMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select * from OutletMaster where branch_code = @BranchCode ";

            return await connection.QueryAsync<OutletMaster>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<StewardMaster>> GetStewardList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT StwCode, POSCode, StwName, UserCode, LastModify, Branch_Code, MobNo FROM StewardMaster WHERE Branch_Code = @Branchcode";

            return await connection.QueryAsync<StewardMaster>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<IEnumerable<OutletandTablemaster>> GetCombinedOutletandtablemasterList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var selectquery = @"SELECT o.OltCode, o.POSCode, o.OltName, o.OltIsRoomService, o.OltServiceTaxRequired, o.UserCode, o.OltIsParcelService, 
            //            o.OltIsFastFood, t.TblCode, t.TblNo, t.TblSeatCount, t.LastModify, t.c, t.Branch_Code, t.QR_Code,
            //            CASE WHEN km.KOTTblNo IS NOT NULL THEN 'Occupied' ELSE 'Available' END AS TableStatus, km.KOTChargeable,
            //            CASE WHEN km.KOTChargeable = 0 THEN 'NCKOT' ELSE 'KOT' END AS KOTStatus 
            //            FROM dbo.OutletMaster o
            //            INNER JOIN dbo.TableMaster t  ON o.OltCode = t.OltCode 
            //            OUTER APPLY
            //            ( SELECT TOP 1 KOTTblNo, KOTChargeable FROM KOTMaster km WHERE km.KOTTblNo = t.TblNo
            //            AND km.KOTSettled = 0 AND km.KOTCancelled = 0 AND km.StwCode <> '0' AND km.KOTtotal > 0) km
            //            WHERE o.Branch_Code = @Branchcode";
            var selectquery = " SELECT o.OltCode, o.POSCode, o.OltName, o.OltIsRoomService, \r\n" +
                " o.OltServiceTaxRequired, o.UserCode, o.OltIsParcelService, \r\n" +
                " o.OltIsFastFood, o.IsDirectKOTandBill , o.IsDirectPaxandStw, o.IsDirectBill, t.TblCode, t.TblNo, t.TblSeatCount, t.LastModify, t.c, o.Branch_Code, t.QR_Code, \r\n" +
                " CASE  WHEN o.OltIsRoomService = 1 THEN NULL \r\n" +
                " WHEN ksm.KSMBillSettled = 0 THEN 'Unsettled' \r\n" +
                " WHEN km.KOTTblNo IS NOT NULL THEN 'Occupied' \r\n" +
                " ELSE 'Available' END AS TableStatus, \r\n" +
                "  km.KOTChargeable, \r\n" +
                " CASE WHEN o.OltIsRoomService = 1 THEN NULL \r\n" +
                " WHEN km.KOTChargeable = 0 THEN 'NCKOT' \r\n" +
                " ELSE 'KOT' END AS KOTStatus, COALESCE(ksm.KSMId, 0) AS BillNo, COALESCE(ksm.KSMBillAmount, 0) AS BillAmount \r\n" +
                " FROM dbo.OutletMaster o \r\n" +
                " LEFT JOIN dbo.TableMaster t ON o.OltCode = t.OltCode And o.branch_code = t.Branch_Code \r\n" +
                " OUTER APPLY ( SELECT TOP 1 ksm.KSMId, ksm.KSMBillSettled,ksm.KSMBillAmount FROM dbo.KOTSettlementMaster ksm \r\n" +
                " WHERE ksm.KSMTblNo = t.TblNo AND ksm.OltCode = t.OltCode AND ksm.KSMBillSettled = 0 AND ISNULL(ksm.BillCancelled,0) = 0 And ksm.Branch_Code = o.branch_code ORDER BY ksm.KSMId DESC) ksm \r\n" +
                " OUTER APPLY ( SELECT TOP 1 KOTTblNo, KOTChargeable FROM KOTMaster km \r\n" +
                " WHERE km.KOTTblNo = t.TblNo AND km.OltCode = t.OltCode AND km.KOTSettled = 0 AND km.KOTCancelled = 0 AND km.branch_code = o.branch_code AND km.StwCode <> '0' AND km.KOTtotal > 0 ORDER BY km.KOTId DESC) km \r\n" +
                " WHERE o.Branch_Code = @branchcode \r\n";
            return await connection.QueryAsync<OutletandTablemaster>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<IEnumerable<ItemMaster>> GetItemMasterList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"SELECT ItemCode, ItemName, ItemDisplayName, QPB, CatCode, GrpCode, ItemDiscountAllowed, ItemRate, ItemSaleQtyUnit, LastModify,mostrunningitemsrno, subItem, ItemOpStock, ItemCurStock, ItemOpRate, ItemCurRate, UnitCode, ItemROQ, ItemROL, Dep, Opstock, Ctstock, perqty, DepCode, perrate, SUnit, branch_code, thumb, IsVeg FROM dbo.ItemMaster WHERE Branch_Code = @Branchcode";

            return await connection.QueryAsync<ItemMaster>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<IEnumerable<ItemCategory>> GetItemCategoryList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT CatCode, CatName, LastModify, Branch_Code, SubCat, thumb FROM dbo.ItemCategory WHERE Branch_Code = @Branchcode";

            return await connection.QueryAsync<ItemCategory>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<IEnumerable<ItemGroup>> GetItemGroupList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT GrpCode, GrpName, LastModify, Branch_Code, Dep FROM dbo.ItemGroup WHERE Branch_Code = @Branchcode";

            return await connection.QueryAsync<ItemGroup>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<IEnumerable<CombinedOltItemlist>> GetCombinedOltItemList(int oltcode, int grpcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select old.OltCode as OltCode, old.ItemCode as ItemCode, old.OIDRate as OIDRate, old.OIDAvailable as OIDAvailable,\r\n " +
                " old.Branch_Code as BranchCode, im.ItemName as ItemName, im.ItemDiscountAllowed as ItemDiscountAllowed, im.IsVeg, im.thumb as thumb,\r\n " +
                " im.CatCode as CatCode, ic.CatName as CatName, ic.thumb as catthumb, im.Grpcode as Grpcode, ig.GrpName as GrpName\r\n " +
                " From dbo.ItemMaster im\r\n " +
                " inner join dbo.OltItemDetails old on old.ItemCode = im.ItemCode and old.Branch_Code = @Branchcode\r\n " +
                " inner join dbo.ItemCategory ic on im.CatCode = ic.CatCode and ic.Branch_Code = @Branchcode\r\n " +
                " inner join dbo.ItemGroup ig on ig.Grpcode = im.Grpcode and ig.Branch_Code = @Branchcode \r\n" +
                " where ic.SubCat = 0 and old.oltcode = @OltCode and im.Grpcode = @Grpcode and im.Branch_Code = @Branchcode\r\n " +
                " order by ic.CatCode, im.ItemName ";

            return await connection.QueryAsync<CombinedOltItemlist>(selectquery, new { OltCode = oltcode, Grpcode = grpcode, Branchcode = branchcode });
        }

        public async Task<IEnumerable<CombinedItemMasterCategorylist>> GetCombinedIMandICList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"Select IM.ItemCode, IM.ItemName, IM.ItemRate, IM.ItemDiscountAllowed, IM.thumb, IM.IsVeg, IM.branch_code as Branchcode,
            IM.CatCode, IC.CatName, IC.thumb as Catthumb, 
            IM.GrpCode, IG.GrpName
            From dbo.ItemMaster IM
            LEFT JOIN dbo.ItemGroup IG on IG.GrpCode =  IM.GrpCode
            LEFT JOIN dbo.ItemCategory IC on IC.CatCode =  IM.CatCode
            Where IM.branch_code = @Branchcode
            order by IM.ItemCode asc ";

            return await connection.QueryAsync<CombinedItemMasterCategorylist>(selectquery, new { Branchcode = branchcode });
        }

        #region POS

        public async Task<dynamic> TableReservations(int resid)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT * FROM tablereservations WHERE Id = @ResId";

            return await connection.QueryFirstOrDefaultAsync<dynamic>(selectquery, new { ResId = resid });
        }

        public async Task<dynamic> GetKOTDetails(string table, string subtable, int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT distinct KOTMaster.KOTTblNo,KOTChargeable " +
                " FROM ItemMaster  " +
                " INNER JOIN  (KOTMaster INNER JOIN KOTDetails ON KOTMaster.KOTId = KOTDetails.KOTId And KOTMaster.branch_code = KOTDetails.branch_code) " +
                " ON ItemMaster.ItemCode = KOTDetails.ItemCode And ItemMaster.branch_code = KOTDetails.branch_code where KOTMaster.KOTSettled=0 AND  " +
                " KOTMaster.KOTCancelled=0 and KOTMaster.StwCode<>'0' and KOTtotal>0 and " +
                " oltcode = @Outlet and KOTTblNo = @Table and KOTMaster.SubTable = @SubTable and KOTMaster.Branch_Code = @Branchcode";

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
                WHERE KOTNO = @KOTNo AND KotTblNo = @Table AND SubTable = @SubTable AND KotMobileNo = @KotMobileNo AND branch_code = @branchcode";

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

        public int GetSpecialInfoId(string specialInfo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"select spid from SpecialInformation where spinfo = @SpecialInfo";

            var spid = connection.QueryFirstOrDefault<int>(selectquery, new { SpecialInfo = specialInfo });
            return spid;
        }

        //public IEnumerable<int> GetSpecialInfoIds(string specialInfoCsv)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    // Split the CSV string into array
        //    var specialInfoArray = specialInfoCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);

        //    string str = "Insert into SpecialInformation values('" + option + "','" + branch + "')";

        //    var selectQuery = @"SELECT spid 
        //                FROM SpecialInformation 
        //                WHERE spid IN @SpecialInfoArray";

        //    var spids = connection.Query<int>(selectQuery, new { SpecialInfoArray = specialInfoArray });
        //    return spids;
        //}

        public async Task<IEnumerable<string>> GetSpecialInfoIds(string specialInfoCsv, string branchcode)
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
                    // Text instruction → check if exists
                    //var existingId = connection.QueryFirstOrDefault<int?>(
                    //    "SELECT SPID FROM SpecialInformation WHERE SPINFO = @Text AND OltCode = @Branch",
                    //    new { Text = part, Branch = branch });

                    //var existingId = connection.QueryFirstOrDefault<int?>(
                    //    @"SELECT SPID FROM SpecialInformation  WHERE LOWER(SPINFO) = LOWER(@Text)  AND OltCode = @Branch", 
                    //    new { Text = part, Branch = branch });

                    var existingId = await connection.QueryFirstOrDefaultAsync<int?>(
                        "SELECT SPID FROM SpecialInformation WHERE SPINFO LIKE @Text AND OltCode = @Oltcode",
                        new { Text = "%" + part + "%", Oltcode = branchcode });

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
                            VALUES (@Text, @Oltcode)",
                            new { Text = part, Oltcode = branchcode });

                        allIds.Add(newId);
                    }
                }
            }

            if (!allIds.Any())
                return Enumerable.Empty<string>();
            var spinfo = await connection.QueryAsync<string>
                (@"SELECT SPINFO FROM SpecialInformation WHERE SPID IN @ids AND OltCode = @Oltcode", new { ids = allIds, Oltcode = branchcode });

            return spinfo;
        }

        public async Task<IEnumerable<SpecialInstruction>> GetSpecialInfo()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = @"select SPID, SPINFO from SpecialInformation order by SPID";

            var result = await connection.QueryAsync<SpecialInstruction>(selectquery);
            return result;
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

            var insertquery = @" INSERT INTO dbo.tmpKotPrint (KotnO, StwNo, TblNo, KotDate, ItemName, qty, CatName, grpcode, ManualKotNo, itemcode, Branch_Code) 
                VALUES (@KotNo, @WaiterNo, @TableNo, @KotDate, @ItemName, @Qty, @CategoryName, @GroupCode, @ManualKotNo, @ItemCode, @Branch_Code)";

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

            var sql = " SELECT km.kotid, km.kotno, kd.kid, kd.ItemCode, kd.KOTDQty \r\n" +
                " FROM kotmaster km \r\n" +
                " INNER JOIN kotdetails kd ON km.kotid = kd.kotid And kd.branch_code = km.branch_code \r\n" +
                " WHERE kotsettled = 0 AND RAWORNOT IS NULL AND kotcancelled = 0 AND kottblno = @Table AND km.OltCOde = @Outlet \r\n" +
                " AND subtable = @SubTable AND kd.itemcode = @ItemCode AND km.Branch_Code = @Branch AND KotChargeable = @IsChargeable";

            return await connection.QueryFirstOrDefaultAsync<ActiveKotDetail>(sql, new { Table = table, Outlet = outlet, SubTable = subTable, ItemCode = itemCode, Branch = branch, IsChargeable = isChargeable ? 1 : 0 });
        }

        public async Task UpdateKotMasterForVoidAsync(int kotNo, string branch, string remarks)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @" UPDATE KOTMASTER SET RefKOTNo = @KotNo, remarks = @Remarks, ISUPLOADED = '0' WHERE KOTNO = @KotNo AND Branch_Code = @Branch";

            await connection.ExecuteAsync(sql, new { KotNo = kotNo, Remarks = remarks, Branch = branch });
        }

        //public async Task GenerateCancelKotAsync(int kotId, int itemCode, int qty, int kid, string branch, bool isChargeable, string tableNo, string subTable, int userCode, string remarks)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    connection.Open();

        //    using var transaction = connection.BeginTransaction();

        //    try
        //    {
        //        // 1️⃣ Reduce quantity
        //        var updateQtySql = @" UPDATE KotDetails SET KotdQty = KotdQty - @Qty, Isuploaded = '0' WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid";

        //        await connection.ExecuteAsync(updateQtySql, new { KotId = kotId, ItemCode = itemCode, Qty = qty, Branch = branch, Kid = kid }, transaction);

        //        // 2️⃣ Get updated row
        //        var detail = await connection.QueryFirstOrDefaultAsync<dynamic>(
        //            @"SELECT KOTDQty FROM KotDetails WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid",
        //            new { KotId = kotId, ItemCode = itemCode, Branch = branch, Kid = kid }, transaction);

        //        if (detail != null && detail.KOTDQty == 0)
        //        {
        //            // delete row
        //            await connection.ExecuteAsync(@"DELETE FROM KotDetails WHERE Kotid = @KotId AND ItemCode = @ItemCode AND Branch_Code = @Branch AND KID = @Kid",
        //                new { KotId = kotId, ItemCode = itemCode, Branch = branch, Kid = kid }, transaction);
        //        }

        //        // 3️⃣ If no details left → delete master
        //        var remaining = await connection.QueryFirstOrDefaultAsync<int>(@"SELECT COUNT(1) FROM KotDetails WHERE Kotid = @KotId AND Branch_Code = @Branch",
        //            new { KotId = kotId, Branch = branch }, transaction);

        //        if (remaining == 0)
        //        {
        //            await connection.ExecuteAsync(@"DELETE FROM KotMaster WHERE Kotid = @KotId AND Branch_Code = @Branch",
        //                new { KotId = kotId, Branch = branch }, transaction);
        //        }

        //        // 4️⃣ Auto settle orphan masters
        //        await connection.ExecuteAsync(@"
        //        UPDATE KOTMaster SET KOTSettled = 1, Isuploaded = '0'
        //        WHERE kotid NOT IN ( SELECT kotid FROM KOTDetails WHERE Branch_Code = @Branch )
        //        AND Branch_Code = @Branch AND KOTSettled = 0 AND KotChargeable = @IsChargeable",
        //            new { Branch = branch, IsChargeable = isChargeable ? 1 : 0 }, transaction);

        //        transaction.Commit();
        //    }
        //    catch
        //    {
        //        transaction.Rollback();
        //        throw;
        //    }
        //}

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
                                "SELECT UserName FROM UserMaster WHERE UserCode = @UserCode and Branch_code = @BranchCode",
                                new { UserCode = userCode, BranchCode = branch }, transaction);

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

        public async Task<List<ExtraChargeModel>> GetExtraCharges(string itemcode, string branchcode, int outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var selectquery = "Select * from ExtraCharges E inner join TaxMaster T on E.ChargeCode = T.TaxCode and" +
            //    " E.ItemCode = @ItemCode and T.Branch_Code = @BranchCode and" +
            //    " E.Lastmodify is null and OltCode = @Outlet";

            var selectquery = "Select * from ExtraCharges E " +
                " Inner join BillTaxDescription T on E.ChargeCode = T.TaxCode And T.BranchCode = E.Branch_Code " +
                " Where E.ItemCode = @ItemCode and T.BranchCode = @BranchCode and OltCode = @Outlet";

            return (await connection.QueryAsync<ExtraChargeModel>(selectquery, new { ItemCode = itemcode, Outlet = outlet, BranchCode = branchcode })).ToList();

        }

        public async Task<TaxModel> GetTaxCharges(int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select * from OutletMaster where OltCode = @Outlet and Branch_Code = @BranchCode ";

            return await connection.QueryFirstOrDefaultAsync<TaxModel>(selectquery, new { Outlet = outlet, BranchCode = branchcode });
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

        public async Task<(DateTime billDate, string billTime)> GetBillDateTime (string tabletype, string billNo, string branchCode, int outletCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var tableName = tabletype == "NC" ? "NCKOTSettlementMaster" : "KOTSettlementMaster";

            string selectquery = @$"SELECT KSMBillDate as billDate, KSMBillTime as billTime FROM {tableName} WHERE KSMBillNo = @Bill_No AND Branch_Code = @Branch_Code AND OltCode = @OltCode";

            return await connection.QueryFirstOrDefaultAsync<(DateTime billDate, string billTime)>(selectquery, new { Bill_No = billNo, Branch_Code = branchCode, OltCode = outletCode });
        }

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

        public async Task InsertSalesGroupTaxBulk(int billId, string branchCode, int oltCode, TaxModel taxData, DateTime POSEntryDate)
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
                    DiscountPer, Discount, DiscountIn, DiscountRemarks, RoundOff, billId , oltCode, branchCode,BillDate)
                    VALUES
                    (@TotalAmount, @TotalQty, @CGSTAmt, @SGSTAmt, @ServiceChargePer, @ServiceCharge, @GrandTotal,
                    @DiscountPer, @Discount, @DiscountIn, @DiscountRemarks, @RoundOff, @billId , @oltCode, @branchCode, @BillDate);
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
                        taxData.DiscountIn,
                        taxData.DiscountRemarks,
                        taxData.RoundOff,
                        billId,
                        oltCode,
                        branchCode,
                        BillDate = POSEntryDate
                    },
                    transaction
                );

                // 2️⃣ Insert child table BillTaxDetails
                string insertChild = @"
                    INSERT INTO BillTaxDetails
                    (BillTaxId, GroupCode, GroupName, TaxName, Taxper, TaxableAmount, TaxAmount, Total, CGST, SGST, billId , oltCode, branchCode, BillDate)
                    VALUES
                    (@BillTaxId, @GroupCode, @GroupName, @TaxName, @Taxper, @TaxableAmount, @TaxAmount, @Total, @CGST, @SGST, @billId , @oltCode, @branchCode, @BillDate);";

                foreach (var detail in taxData.TaxList)
                {
                    await connection.ExecuteAsync(insertChild, new
                    {
                        BillTaxId = billTaxId,
                        detail.GroupCode,
                        detail.GroupName,
                        detail.TaxName,
                        detail.Taxper,
                        detail.TaxableAmount,
                        detail.TaxAmount,
                        detail.Total,
                        detail.CGST,
                        detail.SGST,
                        billId,
                        oltCode,
                        branchCode,
                        BillDate = POSEntryDate
                    }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task GetDiscount(string billNo, BillModel bill, DateTime posEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                double dsc = 0.0;

                if (bill.Cart.Discount == 0 || bill.Tax.Discount == 0)
                    dsc = 0.0;
                if (bill.Cart.Discount > 0 || bill.Tax.Discount > 0)
                {
                    dsc = bill.Cart.Discount;

                    var Grpcodes = string.Join(",", bill.Cart?.Food?.Select(f => f.GrpCode) ?? Enumerable.Empty<int>());

                    string insertDiscount = @"INSERT INTO ItemDiscount
                                    (BillNo, BillDate, amount, grpcode, amountperc, discamount, Branch_Code, OltCode, Ref, IsOnline, DiscountIn, DiscountType)
                                    VALUES
                                    (@BillNo, @BillDate, @Amount, @GrpCode, @AmountPerc, @DiscAmount, @Branchcode, @Outletcode, 'F' , 0, @discountIn, @discountType)";

                    bool saved = await connection.ExecuteAsync(insertDiscount, new
                    {
                        BillNo = billNo,
                        BillDate = posEntryDate,
                        Amount = bill.Tax.TotalAmount,
                        GrpCode = Grpcodes,
                        AmountPerc = bill.Tax.DiscountPer,
                        DiscAmount = bill.Tax.Discount,
                        Branchcode = bill.Cart.Branch,
                        Outletcode = bill.Cart.Outlet,
                        discountIn = bill.Tax.DiscountIn,
                        discountType = bill.Cart.DiscountType
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
        public async Task SaveDiscount(string billNo, BillModel bill, DateTime posEntryDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                double dsc = 0.0;

                if (bill.Cart.Discount == 0 || bill.Tax.Discount == 0)
                    dsc = 0.0;
                if (bill.Cart.Discount > 0 || bill.Tax.Discount > 0)
                {
                    dsc = bill.Cart.Discount;

                    var Grpcodes = string.Join(",", bill.Cart?.Food?.Select(f => f.GrpCode) ?? Enumerable.Empty<int>());

                    string insertDiscount = @"INSERT INTO ItemDiscount
                                    (BillNo, BillDate, amount, grpcode, amountperc, discamount, Branch_Code, OltCode, Ref, IsOnline, DiscountIn, DiscountType)
                                    VALUES
                                    (@BillNo, @BillDate, @Amount, @GrpCode, @AmountPerc, @DiscAmount, @Branchcode, @Outletcode, 'F' , 0, @discountIn, @discountType)";

                    bool saved = connection.Execute(insertDiscount, new
                    {
                        BillNo = billNo,
                        BillDate = posEntryDate,
                        Amount = bill.Tax.TotalAmount,
                        GrpCode = Grpcodes,
                        AmountPerc = bill.Tax.DiscountPer,
                        DiscAmount = bill.Tax.Discount,
                        Branchcode = bill.Cart.Branch,
                        Outletcode = bill.Cart.Outlet,
                        discountIn = bill.Tax.DiscountIn,
                        discountType = bill.Cart.DiscountType
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

        public async Task UpdateDiscount(int Billno, BillModel Bill, DateTime POSEntryDate)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string sql = @" Update ItemDiscount SET amount = @Amount, amountperc = @AmountPerc, discamount = @DiscAmount  WHERE BillNo=@BillNo AND OltCode=@Outlet AND Branch_Code=@BranchCode AND BillDate=@BillDate";

                connection.ExecuteAsync(sql, new { Amount = Bill.Tax.TotalAmount, AmountPerc = Bill.Tax.DiscountPer, DiscAmount = Bill.Tax.Discount, BillNo = Billno, Outlet = Bill.Cart.Outlet, BillDate = POSEntryDate, BranchCode = Bill.Cart.Branch });
            }
            catch
            {
                throw;
            }
        }

        public async Task DeleteDiscount(int Billno, int OltCode, DateTime SettledDate, string Branch_Code)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string sql = @" DELETE FROM ItemDiscount WHERE BillNo=@BillNo AND OltCode=@Outlet AND Branch_Code=@BranchCode AND CAST(BillDate AS DATE) = CAST(@BillDate AS DATE)";

                await connection.ExecuteAsync(sql, new { Outlet = OltCode, BillNo = Billno, BillDate = SettledDate, BranchCode = Branch_Code });
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> InsertTmpBillPrint(int waiter, int outlet, DateTime posEntryDate, string billType, string billNo, string tableNo, string userName, int ksmId, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string insertquery = @"INSERT INTO tmpbillprint
                   (SWT, PAX, ITM, RATE, BILLNO, TABNO, QTY, NETBILL, KSMID, Branch_Code)
                   VALUES
                   (@Waiter, @Outlet, @ItemDateTime, @ItemDate, @BillNo, @TableNo, @BillType, @UserName, @KsmId, @BranchCode)";

            var parameters = new { Waiter = waiter, Outlet = outlet, ItemDateTime = posEntryDate.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToShortTimeString(), ItemDate = posEntryDate.ToString("dd/MM/yyyy"), BillType = billType, BillNo = billNo, TableNo = tableNo, UserName = userName, KsmId = ksmId, BranchCode = branchCode };

            return await connection.ExecuteAsync(insertquery, parameters) > 0;
        }

        public async Task<string> GetKOTSettlementMaster(int billId, string billNo, int outletcode, string branchcode, string accounttype)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string selectquery = @"select TokenNo from KOTSettlementMaster where KSMId = @BillID and KSMBillNo = @BillNo and oltcode = @Outletcode and Branch_Code = @Brancode and AccountType = @accounttype";

            var parameters = new { BillId = billId, BillNo = billNo, Outletcode = outletcode, Brancode = branchcode, AccountType = accounttype };

            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, parameters);
        }

        public async Task<string> GetUserName(int userCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT UserName from UserMaster where UserCode = @UserCode AND Branch_code = @BranchCode";

            return connection.QueryFirstOrDefault<string>(selectquery, new { UserCode = userCode, BranchCode = branchCode });
        }

        public async Task<bool> IsDirectSettlement(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string sql = "SELECT OltIsParcelService FROM OutletMaster WHERE OltCode = @OutletCode AND branch_code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
        }

        public async Task<bool> IsDirectBillSettlementOnline(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT IsDirectBill from TBl_OnlineOutlet_Type where OutletCode = @OutletCode AND BranchCode = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
        }

        public async Task<bool> IsFastFoodDirectSettlement(int outlet, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT OltIsFastFood from OutletMaster where OltCode = @OutletCode AND branch_code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<bool>(sql, new { OutletCode = outlet, BranchCode = branchCode });
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

        public async Task<int> SettleBill(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate, string BillType)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int isSettled = await DOkotsettlement(settlement, posentrydate, Validdate, BillType);

            if (isSettled > 0)
            {
                string selectsql = @"SELECT * FROM kotsettlementmaster WHERE KsmBillNo = @Billno AND oltcode = @Outletcode AND KSMBillDate = @BillDate AND Branch_Code = @branch;";

                var ds = connection.QueryFirstOrDefault<dynamic>(selectsql, new { Billno = settlement.BillNo, Outletcode = settlement.OltCode, BillDate = settlement.BillDate.Date, branch = settlement.BranchCode });

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
            return isSettled;
        }

        public async Task<int> DOkotsettlement(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate, string BillType)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            using var HMSconnection = _factory.CreateConnection(DbNames.HMS);
            HMSconnection.Open(); // Ensure connection is open

            int lngKSBId = 0;
            int saved = 0;
            double ntotal = settlement.GrandAmount;
            int companycode = 0;

            string checkroomservice = await GetInfo("OutletMaster", "OltIsRoomService", "OltCode", settlement.OltCode.ToString(), 0, settlement.BranchCode);

            try
            {
                string dttime = DateTime.Now.TimeOfDay.ToString();
                DateTime posDate = posentrydate;

                foreach (var payment in settlement.PaymentDetails)
                {
                    if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertocompany")
                    {
                        lngKSBId = await findnextnumber("BillTransferToCompany", "BTId", "Branch_Code", settlement.BranchCode);
                    }
                    else if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertoroom")
                    {
                        lngKSBId = await findhotelnextnumber("Tbl_FoodBills", "RNo");
                    }
                    else
                    {
                        lngKSBId = await findnextnumber("KOTBillSettlement", "KBSId", "Branch_Code", settlement.BranchCode);
                    }

                    if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertocompany")
                    {
                        //var selectquery = "select CompanyCode From CompanyMaster Where CompanyName LIKE @Cmpname ";
                        //companycode = connection.QueryFirstOrDefault<int>(selectquery, new { Cmpname = payment.SubMode });

                        string sql = @"INSERT INTO BillTransferToCompany
                        (BTId, POSCode, CompanyCode, BTDate, BTTime, BillNo, BillAmt, UserCode, LastModify, BTCSettled, AmtPaid, Remarks, 
                        oltcode, Discount, Branch_Code, Isuploaded, Ismodified)
                        VALUES
                        (@Id, '1', @Company, @Date, @Time, @billNo, @amount, @User, @Modify, 0, 0, @remarks, @Outlet, @discount, @Branch, '0', '0')";

                        saved = await connection.ExecuteAsync(sql, new
                        {
                            Id = lngKSBId,
                            Company = payment.SubMode,
                            Date = BillType == "newbill" ? posDate.Date : settlement.BillDate.Date,
                            Time = Validdate.ToString(@"hh\:mm\:ss"),
                            billNo = settlement.BillNo,
                            amount = payment.Amount,
                            User = settlement.UserCode,
                            Modify = dttime,
                            remarks = payment.Remarks,
                            Outlet = settlement.OltCode,
                            discount = settlement.Discount,
                            Branch = settlement.BranchCode
                        });

                    }
                    else if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertoroom")
                    {
                        var outname = await GetOutletNameAsync(settlement.OltCode, settlement.BranchCode);
                        decimal GstVal = Convert.ToDecimal(settlement.TaxAmount / 2);
                        decimal ExculTax = Convert.ToDecimal(settlement.GrandAmount  - settlement.TaxAmount);

                        string sql = @"
                        INSERT INTO Tbl_FoodBills 
                        (RNo, TrDate, RcptNo, GuestCode, GuestName, CheckInNo, RoomNo, BillAmt, SplitId, BillStatus, OutlateName, Tax1, Tax2, TaxAmt1, Tax3, TaxAmt2)
                        VALUES
                        (@RNo, @TrDate, @RcptNo, @guestcode, @guestname, @checkinNo, @RoomNo, @BillAmt, 0, 'N', @Outlet, @CGST, @SGST, @TaxVal, @ExTax1, @ExTax2);";

                        saved = await HMSconnection.ExecuteAsync(sql, new
                        {
                            RNo = lngKSBId,
                            TrDate = BillType == "newbill" ? posDate.Date : settlement.BillDate.Date,
                            RcptNo = settlement.BillNo,
                            guestcode = settlement.GuestCode,
                            guestname = settlement.GuestName,
                            checkinNo = settlement.CheckInNo,
                            RoomNo = settlement.TableNo,
                            BillAmt = payment.Amount,
                            Outlet = outname.OutletName,
                            CGST = GstVal,
                            SGST = GstVal,
                            TaxVal = settlement.TaxAmount,
                            ExTax1 = ExculTax,
                            ExTax2 = ExculTax
                        });
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

                        saved = await connection.ExecuteAsync(sql, new
                        {
                            Id = lngKSBId,
                            Outlet = settlement.OltCode,
                            KSMId = settlement.BillId,
                            billNo = settlement.BillNo,
                            amount = payment.Amount,
                            Date = BillType == "newbill" ? posDate.Date : settlement.BillDate.Date,
                            refNo = payment.Remarks,
                            mainmode = payment.Mode,
                            submode = payment.SubMode,
                            validDate = Validdate,
                            discount = settlement.Discount,
                            User = settlement.UserCode,
                            Time = dttime,
                            Branch = settlement.BranchCode,
                            Tips = settlement.ChangeAmount
                        });

                    }

                    if (saved > 0)
                    {
                        if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertoroom")
                        {
                            await connection.ExecuteAsync(@"UPDATE KOTSettlementMaster
                                    SET Ismodified ='1', Isuploaded = '0', KSMBillSettled = 1, KSMBillTransfered = 1, KSMIsRoomService = 1, KsmSettledAmt = 0, tips = @tips
                                    WHERE KSMId = @Id AND Branch_Code = @Branch AND AccountType = 'F'",
                                new { tips = settlement.Tips, Id = settlement.BillId, Branch = settlement.BranchCode });

                            await HMSconnection.ExecuteAsync(@"UPDATE Tbl_FoodBills SET BillStatus = 'Y' WHERE RcptNo = @billno AND OutlateName = @Outlet",
                                new { billno = settlement.BillNo, Outlet = settlement.OutletName });

                            await connection.ExecuteAsync(@" Delete From BillTransferToCompany Where BTCSettled = 0 And BillNo = @billno And Oltcode = @oltcode And Branch_Code = @Branch",
                                new { billno = settlement.BillNo, oltcode = settlement.OltCode, Branch = settlement.BranchCode });
                        }
                        else if (payment.Mode?.Replace(" ", "").Trim().ToLowerInvariant() == "transfertocompany")
                        {
                            await connection.ExecuteAsync(@"UPDATE KOTSettlementMaster
                                    SET Ismodified = '1', Isuploaded = '0', KSMBillSettled = 1, KSMBillTransfered = 1, KSMIsRoomService = 0, KsmSettledAmt = 0, tips = @tips
                                    WHERE KSMId = @Id AND Branch_Code = @Branch AND AccountType = 'F'",
                                new
                                {
                                    tips = settlement.Tips,
                                    Id = settlement.BillId,
                                    Branch = settlement.BranchCode
                                });

                            await HMSconnection.ExecuteAsync(@"Delete From Tbl_FoodBills WHERE RcptNo = @billno AND OutlateName = @Outlet",
                                new { billno = settlement.BillNo, Outlet = settlement.OutletName });
                        }
                        else
                        {
                            await connection.ExecuteAsync(@"UPDATE KOTSettlementMaster
                                    SET Ismodified = '1', Isuploaded = '0', KSMBillSettled = 1, tips = @tips, KSMBillTransfered = 0 , KSMIsRoomService = 0
                                    WHERE KSMId = @Id AND Branch_Code = @Branch AND AccountType = 'F'",
                                new
                                {
                                    tips = settlement.Tips,
                                    Id = settlement.BillId,
                                    Branch = settlement.BranchCode
                                });

                            await connection.ExecuteAsync(@" Delete From BillTransferToCompany Where BTCSettled = 0 And BillNo = @billno And Oltcode = @oltcode And Branch_Code = @Branch",
                                new { billno = settlement.BillNo, oltcode = settlement.OltCode, Branch = settlement.BranchCode });

                            if (checkroomservice == "True")
                            {
                                await HMSconnection.ExecuteAsync(@"Delete From Tbl_FoodBills WHERE RcptNo = @billno AND OutlateName = @Outlet",
                                new { billno = settlement.BillNo, Outlet = settlement.OutletName });
                            }
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

        public async Task<bool> PhonepeSettleBill(SettlementModel settlement, PhonePeCollectResponseBody payment, DateTime posentrydate)
        {
            //if (DOkotsettlement(settlement, payment, posentrydate))
            //{
            //    using var connection = _factory.CreateConnection(DbNames.POS);

            //    string str = "Select * from kotsettlementmaster where KsmBillNo = '" + settlement.Bill.BillNo + "' and oltcode = '" + settlement.OutletCode + "'  and KSMBillDate='" + posentrydate + "'";
            //    var ds = connection.QueryFirstOrDefault<dynamic>(str);
            //    if (ds != null)
            //    {
            //        str = "Update tbl_guestlinking set BillNo = '" + settlement.Bill.BillNo + "',LinkStatus = 'N' where TableNo = '" + ds.KSMTblNo + "' and BillNo is null";
            //        connection.Execute(str);
            //    }
            //    double gpoints = 0;
            //    str = "Select sum(isnull(gpoint,0)) as Points from view_guestpoints where BillNo = '" + settlement.Bill.BillNo + "' and OltName = '" + settlement.OutletName + "' ";
            //    var dsp = connection.QueryFirstOrDefault<dynamic>(str);
            //    if (string.IsNullOrEmpty(dsp.Points))
            //        gpoints = 0;
            //    else
            //        gpoints = dsp.Points;
            //    str = "Update tbl_guestlinking set PointsEarned = '" + gpoints + "' where TableNo = '" + ds.KsmTblNo + "' and BillNo = '" + settlement.Bill.BillNo + "' ";
            //    connection.Execute(str);
            //}
            return true;
        }

        //public bool DOkotsettlement(SettlementModel settlement, PhonePeCollectResponseBody payment, DateTime posentrydate)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    int lngKSBId = 0;
        //    bool saved = false;
        //    double ntotal = settlement.Bill.GrandAmount;

        //    string checkroomservice = await GetInfo("OutletMaster", "OltIsRoomService", "OltCode", settlement.OutletCode.ToString(), 0, settlement.Branch);

        //    try
        //    {
        //        lngKSBId = findnextnumber("KOTBillSettlement", "KBSId", "Branch_Code", settlement.Branch);

        //        if (settlement.PayMode == "company")
        //        {
        //            lngKSBId = findnextnumber("BillTransferToCompany", "BTId", "Branch_Code", settlement.Branch);
        //        }

        //        string dttime = DateTime.Now.TimeOfDay.ToString();
        //        DateTime posDate = posentrydate;

        //        if (settlement.SubBillingType != "S")
        //        {
        //            string deleteSql = @"DELETE FROM KOTBillSettlement WHERE KSMBillNo=@BillNo AND OltCode=@OutletCode";

        //            connection.Execute(deleteSql, new { BillNo = settlement.Bill.BillNo, OutletCode = settlement.OutletCode });
        //        }

        //        if (settlement.PayMode.ToLower() == "cash")
        //        {
        //            string sql = @"INSERT INTO KOTBillSettlement
        //                  (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
        //                   KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
        //                   KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
        //                   AccountType,Isuploaded,Ismodified,DAYEND)
        //                   VALUES
        //                  (@KBSId,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
        //                   'CASH','0','Cash',@Date,'0',@Discount,@User,@Time,
        //                   @Branch,@Tips,'F','0','0','N')";

        //            saved = connection.Execute(sql, new
        //            {
        //                KBSId = lngKSBId,
        //                Outlet = settlement.OutletCode,
        //                KSMId = settlement.Bill.BillId,
        //                BillNo = settlement.Bill.BillNo,
        //                Amount = settlement.Bill.GrandAmount,
        //                Date = posDate,
        //                Discount = settlement.Bill.Discount,
        //                User = settlement.UserCode,
        //                Time = dttime,
        //                Branch = settlement.Branch,
        //                Tips = settlement.Bill.ChangeAmount
        //            }) > 0;
        //        }

        //        if (settlement.PayMode.ToLower() == "card")
        //        {
        //            string sql = @"INSERT INTO KOTBillSettlement
        //                  (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
        //                   KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
        //                   KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
        //                   AccountType,Isuploaded,Ismodified,DAYEND)
        //                   VALUES
        //                  (@KBSId,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
        //                   'CARD',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
        //                   @Time,@Branch,@Tips,'F','0','0','N')";

        //            saved = connection.Execute(sql, new
        //            {
        //                KBSId = lngKSBId,
        //                Outlet = settlement.OutletCode,
        //                KSMId = settlement.Bill.BillId,
        //                BillNo = settlement.Bill.BillNo,
        //                Amount = settlement.Bill.GrandAmount,
        //                Date = posDate,
        //                RefNo = settlement.Bill.RefNo,
        //                RefName = settlement.Bill.CardName,
        //                ValidDate = DateTime.Parse(settlement.Bill.ValidDate),
        //                Discount = settlement.Bill.Discount,
        //                User = settlement.UserCode,
        //                Time = dttime,
        //                Branch = settlement.Branch,
        //                Tips = settlement.Bill.ChangeAmount
        //            }) > 0;
        //        }

        //        if (settlement.PayMode.ToLower() == "cheque")
        //        {
        //            string sql = @"INSERT INTO KOTBillSettlement
        //                  (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
        //                   KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
        //                   KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
        //                   Accounttype,Isuploaded,Ismodified,DAYEND)
        //                   VALUES
        //                  (@KBSId,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
        //                   'CHEQUE',@RefNo,@RefName,@ValidDate,'0',@Discount,@User,
        //                   @Time,@Branch,@Tips,'F','0','0','N')";

        //            saved = connection.Execute(sql, new
        //            {
        //                KBSId = lngKSBId,
        //                Outlet = settlement.OutletCode,
        //                KSMId = settlement.Bill.BillId,
        //                BillNo = settlement.Bill.BillNo,
        //                Amount = settlement.Bill.GrandAmount,
        //                Date = posDate,
        //                RefNo = settlement.Bill.RefNo,
        //                RefName = settlement.Bill.CardName,
        //                ValidDate = settlement.Bill.ValidDate,
        //                Discount = settlement.Bill.Discount,
        //                User = settlement.UserCode,
        //                Time = dttime,
        //                Branch = settlement.Branch,
        //                Tips = settlement.Bill.ChangeAmount
        //            }) > 0;
        //        }

        //        if (settlement.PayMode.ToLower() == "online")
        //        {
        //            string sql = @"INSERT INTO KOTBillSettlement
        //                  (KBSId,POSCode,OltCode,KSMId,KSMBillNo,KSMBillAmount,
        //                   KBSSetteleDate,KBSPaymentMode,KBSRefNo,KBSRefName,KBSValidDate,
        //                   KBSRoomNo,KBSDiscount,UserCode,LastModify,Branch_Code,tips,
        //                   Accounttype,Isuploaded,Ismodified,DAYEND)
        //                   VALUES
        //                  (@KBSId,'1',@Outlet,@KSMId,@BillNo,@Amount,@Date,
        //                   'ONLINE',@TxnId,'DQRCode',@ValidDate,'0',@Discount,@User,
        //                   @Time,@Branch,@Tips,'F','0','0','N')";

        //            saved = connection.Execute(sql, new
        //            {
        //                KBSId = lngKSBId,
        //                Outlet = settlement.OutletCode,
        //                KSMId = settlement.Bill.BillId,
        //                BillNo = settlement.Bill.BillNo,
        //                Amount = settlement.Bill.GrandAmount,
        //                Date = posDate,
        //                TxnId = payment.data.transactionId,
        //                ValidDate = settlement.Bill.ValidDate,
        //                Discount = settlement.Bill.Discount,
        //                User = settlement.UserCode,
        //                Time = dttime,
        //                Branch = settlement.Branch,
        //                Tips = settlement.Bill.Tips
        //            }) > 0;
        //        }

        //        if (settlement.PayMode.ToLower() == "company")
        //        {
        //            string sql = @"INSERT INTO BillTransferToCompany
        //                  (BTId,POSCode,CompanyCode,BTDate,BTTime,BillNo,BillAmt,
        //                   UserCode,LastModify,BTCSettled,AmtPaid,Remarks,
        //                   oltcode,Discount,Branch_Code,Isuploaded,Ismodified)
        //                   VALUES
        //                  (@Id,'1',@Company,@Date,@Time,@BillNo,@Amount,
        //                   @User,@Modify,0,0,@Remarks,@Outlet,@Discount,
        //                   @Branch,'0','0')";

        //            saved = connection.Execute(sql, new
        //            {
        //                Id = lngKSBId,
        //                Company = settlement.CompanyCode,
        //                Date = posDate,
        //                Time = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss"),
        //                BillNo = settlement.Bill.BillNo,
        //                Amount = settlement.Bill.GrandAmount,
        //                User = settlement.UserCode,
        //                Modify = dttime,
        //                Remarks = settlement.Remarks,
        //                Outlet = settlement.OutletCode,
        //                Discount = settlement.Bill.Discount,
        //                Branch = settlement.Branch
        //            }) > 0;

        //            int rno = findhotelnextnumber("Tbl_OutStanding_Slave", "Rno");

        //            string billno = "POS" + Convert.ToInt64(settlement.Bill.BillNo).ToString("10000000");

        //            string sql2 = @"INSERT INTO Tbl_OutStanding_Slave
        //                   VALUES (@Rno,@BillNo,'0',@Date,@Time,'0',@Guest,0,'0','0',0,
        //                           '0','0','0',0,'0','0','0',@CompanyName,@CompanyCode,
        //                           @Amount,0,@Amount,'S','S','S',@User,'0',@Date,
        //                           @Amount,'Tr','0','0',@Outlet)";

        //            connection.Execute(sql2, new
        //            {
        //                Rno = rno,
        //                BillNo = billno,
        //                Date = posDate,
        //                Time = DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss"),
        //                Guest = settlement.GuestName,
        //                CompanyName = settlement.CompanyName,
        //                CompanyCode = settlement.CompanyCode,
        //                Amount = settlement.Bill.GrandAmount,
        //                User = settlement.UserCode,
        //                Outlet = settlement.OutletCode
        //            });
        //        }

        //        if (saved)
        //        {
        //            if (checkroomservice == "True")
        //            {
        //                connection.Execute(@"UPDATE KOTSettlementMaster 
        //                SET Ismodified='1',Isuploaded='0',KSMBillSettled=1,
        //                    KSMBillTransfered=1,KSMIsRoomService=1,
        //                    KsmSettledAmt=0,tips=@Tips
        //                WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
        //                        new { Tips = settlement.Bill.Tips, Id = settlement.Bill.BillId, Branch = settlement.Branch });

        //                connection.Execute(@"UPDATE Tbl_FoodBills 
        //                SET BillStatus='Y'
        //                WHERE RcptNo=@BillNo AND OutlateName=@Outlet",
        //                        new { BillNo = settlement.Bill.BillNo, Outlet = settlement.OutletName });
        //            }

        //            if (settlement.PayMode == "company")
        //            {
        //                connection.Execute(@"UPDATE KOTSettlementMaster 
        //                SET Ismodified='1',Isuploaded='0',KSMBillSettled=1,
        //                    KSMBillTransfered=1,KSMTblNo=@Company, tips=@Tips
        //                WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
        //                        new
        //                        {
        //                            Company = settlement.CompanyCode,
        //                            Tips = settlement.Bill.Tips,
        //                            Id = settlement.Bill.BillId,
        //                            Branch = settlement.Branch
        //                        });
        //            }
        //            else
        //            {
        //                connection.Execute(@"UPDATE KOTSettlementMaster 
        //                SET Ismodified='1',Isuploaded='0',KSMBillSettled=1,
        //                    tips=@Tips
        //                WHERE KSMId=@Id AND Branch_Code=@Branch AND AccountType='F'",
        //                        new
        //                        {
        //                            Tips = settlement.Bill.Tips,
        //                            Id = settlement.Bill.BillId,
        //                            Branch = settlement.Branch
        //                        });
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        throw;
        //    }

        //    return saved;
        //}

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

        public async Task<int> GetNextGuestCodeAsync(string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = "SELECT ISNULL(MAX(GuestCode), 0) + 1 FROM GuestMaster Where Branch_code = @branchcode";

            var guestCode = await connection.QueryFirstOrDefaultAsync<int>(sql, new { branchcode = Branchcode });
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

            var updatequery = @" UPDATE GuestMaster SET GuestName = @GuestName, Address = @Address, City = @City, Remarks = @Remarks, Email = @Email WHERE Phone = @Phone where Branch_code = @Branch_code";

            var rows = await connection.ExecuteAsync(updatequery, guest);
            return rows > 0;
        }

        public async Task<KBillModel> SettleKOTPartI(DateTime posentrydate, CartModel cart, double taxamount, double discount, string Reason, double RoundOff,
            int TokenNo, string BillingType, string SubBillingType, string GuestName, string GuestMobileNo, string fincode)
        {
            using (IDbConnection dbConnection = _factory.CreateConnection(DbNames.POS))
            {
                var dbillno = "0";
                var billno = "0";
                var billtime = DateTime.Now.ToString("hh:mm tt");
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

                var user = await dbConnection.ExecuteAsync("SaveKotSettlement_stage1", param, commandType: CommandType.StoredProcedure);

                var ksmid = 0;
                billno = "0";
                ksmid = param.Get<int>("@KSMId");
                billno = param.Get<int>("@KSMBillNo").ToString();

                return new KBillModel()
                {
                    ksmid = param.Get<int>("@KSMId"),
                    billno = param.Get<int>("@KSMBillNo").ToString()
                    //billdate = DateOnly.FromDateTime(posentrydate),
                    //billtime = billtime
                };

            }
        }

        public async Task<KBillModel> SettleNCKOTPartI(DateTime posentrydate, CartModel cart, double taxamount, double discount, double SerTax, string Reason, double RoundOff, double SerCharge, int TokenNo, string BillingType, string SubBillingType, string fincode)
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
                param.Add("@KSMBillTime", DateTime.Now.ToString("HH:mm tt"));
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

        #endregion

        public async Task<List<OldCartFoodModel>> GetOldCartFoodAsync(string tableNo, string outlet, char subtable, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var qry = @"Select kd.ItemCode as Id,
            //            im.ItemName as Food, SUM(convert(numeric(10,2),kd.KOTDQty)) as Qty, SUM(convert(numeric(10,2),kd.KOTDQty)) as OrigQty,
            //            kd.KOTDRate as Price, si.SPINFO as Comment, SUM(kd.KOTDQty * kd.KOTDRate) as Total, Convert(nvarchar,km.kotno) as Code
            //        from kotmaster km
            //        inner join kotdetails kd on km.kotno = kd.kotno
            //        inner join ItemMaster im on im.ItemCode = kd.ItemCode
            //        left outer join SpecialInformation si on si.SPID = kd.SplInst
            //        where km.KOTCancelled = '0' and km.KOTSettled = '0' and km.KOTTblNo = @TableNo and km.oltcode = @Outlet and SubTable = @Subtable
            //        group by kd.ItemCode,im.ItemName,kd.KOTDRate,si.SPINFO,km.kotno
            //        order by kd.ItemCode";
            var qry = "Select  km.KOTId as KOTId, km.KOTTblNo as KOTTblNo, kd.ItemCode as ItemCode, km.OltCode as OltCode, ig.GrpCode as GrpCode, " +
                " ig.GrpName as GrpName, km.branch_code as branchcode, im.ItemName as Food, SUM(convert(numeric(10,2),kd.KOTDQty)) as Qty, \r\n" +
                " SUM(convert(numeric(10,2),kd.KOTDQty)) as OrigQty, im.ItemDiscountAllowed, kd.KOTDRate as Price, kd.SplInst as Comment, \r\n" +
                " SUM(kd.KOTDQty * kd.KOTDRate) as Total, Convert(nvarchar,km.kotno) as Code, km.KOTSeatsServed as KOTSeatsServed  \r\n" +
                " From kotmaster km \r\n" +
                " Inner join kotdetails kd on km.kotno = kd.kotno And kd.branch_code = km.branch_code \r\n" +
                " Inner join ItemMaster im on im.ItemCode = kd.ItemCode And im.Branch_Code = km.branch_code \r\n" +
                " Inner join ItemGroup ig on ig.GrpCode = im.GrpCode And ig.Branch_Code = km.branch_code \r\n" +
                " -- left outer join SpecialInformation si on si.SPID = kd.SplInst \r\n" +
                " Where km.KOTCancelled = '0' and km.KOTSettled = '0' and km.KOTTblNo = @TableNo and km.oltcode = @Outlet and SubTable = @Subtable And km.branch_code = @BranchCode \r\n" +
                " Group by kd.ItemCode,im.ItemName,kd.KOTDRate,kd.SplInst,km.kotno, km.KOTId,km.KOTTblNo, km.OltCode, ig.GrpCode, ig.GrpName, \r\n" +
                " km.StwCode, km.KOTSeatsServed, km.branch_code, im.ItemDiscountAllowed order by kd.ItemCode";

            return (await connection.QueryAsync<OldCartFoodModel>(qry, new { TableNo = tableNo, Outlet = outlet, Subtable = subtable, BranchCode = branchcode })).ToList();
        }

        public async Task<List<WaiterModel>> GetOldCartWaiterAsync(string tableNo, string outlet, char subtable, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select DRefKOTNo as NCCode, NCKOT_Particulars as NCRemarks, sm.StwCode, \r\n" +
                " sm.StwName, KOTSeatsServed as Pax, KotMobileNo, KOTGuestName \r\n" +
                " From kotmaster km \r\n" +
                " Inner join StewardMaster sm on km.StwCode = sm.StwCode And sm.Branch_Code = km.branch_code \r\n" +
                " Where km.KOTCancelled = '0' and km.KOTSettled = '0' and km.KOTTblNo = @TableNo and km.oltcode = @Outlet and SubTable = @Subtable and km.branch_code = @BranchCode \r\n" +
                " Order by km.kotno";

            return (await connection.QueryAsync<WaiterModel>(qry, new { TableNo = tableNo, Outlet = outlet, Subtable = subtable, BranchCode = branchcode })).ToList();
        }

        public async Task<List<SubTableStatusModel>> GetSubTables(string outlet, string tableno, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var qry = "SELECT distinct SubTable FROM ItemMaster INNER JOIN" +
            //    "(KOTMaster INNER JOIN KOTDetails ON KOTMaster.KOTId = KOTDetails.KOTId)" +
            //    " ON ItemMaster.ItemCode = KOTDetails.ItemCode where KOTMaster.KOTSettled = 0 AND" +
            //    " KOTMaster.KOTCancelled = 0 and KOTMaster.StwCode <> '0' and" +
            //    " KOTtotal> 0 and oltcode = @Outlet  and KOTTblNo = @TableNo ";
            //var qry = "SELECT t.KOTTblNo, t.SubTable, " +
            //    " CASE WHEN ksm.KSMBillSettled = 0 THEN 'Unsettled' " +
            //    " WHEN EXISTS( SELECT 1 FROM KOTMaster km2 WHERE km2.KOTTblNo = t.KOTTblNo " +
            //    " AND km2.SubTable = t.SubTable AND km2.KOTSettled = 0 AND km2.KOTCancelled = 0 AND km2.OltCode = @Outlet) THEN 'Occupied' " +
            //    " ELSE 'Available' END AS TableStatus, ksm.KSMId AS BillNo, ksm.KSMBillAmount AS BillAmount " +
            //    " FROM(SELECT DISTINCT KOTTblNo, SubTable FROM KOTMaster WHERE KOTTblNo = @TableNo AND OltCode = @Outlet) t " +
            //    " OUTER APPLY( SELECT TOP 1 * FROM KOTSettlementMaster ksm WHERE ksm.KSMTblNo = t.KOTTblNo AND ksm.KSMSUBTBLNO = t.SubTable AND ksm.KSMBillSettled = 0 AND ksm.OltCode = @Outlet ORDER BY ksm.KSMId DESC) ksm; ";

            //var qry = "WITH TableStatusCTE AS \r\n" +
            //    " ( SELECT t.KOTTblNo, t.SubTable, \r\n" +
            //    " CASE WHEN ksm.KSMBillSettled = 0 THEN 'Unsettled' \r\n" +
            //    " WHEN EXISTS ( SELECT 1 FROM KOTMaster km2 WHERE km2.KOTTblNo = t.KOTTblNo AND km2.SubTable = t.SubTable AND km2.KOTSettled = 0 AND km2.KOTCancelled = 0 AND km2.OltCode = @Outlet AND km2.branch_code = @BranchCode) THEN 'Occupied' \r\n" +
            //    " ELSE 'Available' END AS TableStatus, ksm.KSMBillNo AS BillNo, ksm.KSMBillAmount AS BillAmount \r\n" +
            //    " FROM (SELECT DISTINCT KOTTblNo, SubTable FROM KOTMaster WHERE KOTTblNo = @TableNo AND OltCode = @Outlet AND branch_code = @BranchCode ) t \r\n" +
            //    " OUTER APPLY ( SELECT TOP 1 * FROM KOTSettlementMaster ksm WHERE ksm.KSMTblNo = t.KOTTblNo AND ksm.KSMSUBTBLNO = t.SubTable AND ksm.KSMBillSettled = 0 \r\n" +
            //    " AND ISNULL(ksm.BillCancelled,0) = 0 AND ksm.OltCode = @Outlet AND ksm.Branch_Code = @BranchCode ORDER BY ksm.KSMId DESC) ksm )  \r\n" +
            //    " SELECT * FROM TableStatusCTE WHERE TableStatus <> 'Available'; \r\n";

            var qry = @"SELECT t.KOTTblNo, t.SubTable, t.KOTTime, CASE WHEN ksm.KSMBillSettled = 0 THEN 'Unsettled' WHEN EXISTS ( SELECT 1 FROM KOTMaster km2 WHERE km2.KOTTblNo = t.KOTTblNo AND km2.SubTable = t.SubTable AND km2.KOTSettled = 0 AND km2.KOTCancelled = 0 AND km2.OltCode = @Outlet AND km2.branch_code = @BranchCode ) THEN 'Occupied' ELSE 'Available' END AS TableStatus, ksm.KSMBillNo AS BillNo, ksm.KSMBillAmount AS BillAmount FROM ( SELECT KOTTblNo, SubTable, MAX(KOTTime) AS KOTTime FROM KOTMaster WHERE KOTTblNo = @TableNo AND OltCode = @Outlet AND branch_code = @BranchCode GROUP BY KOTTblNo, SubTable ) t OUTER APPLY ( SELECT TOP 1 * FROM KOTSettlementMaster ksm WHERE ksm.KSMTblNo = t.KOTTblNo AND ksm.KSMSUBTBLNO = t.SubTable AND ksm.KSMBillSettled = 0 AND ISNULL(ksm.BillCancelled, 0) = 0 AND ksm.OltCode = @Outlet AND ksm.Branch_Code = @BranchCode ORDER BY ksm.KSMId DESC ) ksm WHERE ksm.KSMBillSettled = 0 OR EXISTS ( SELECT 1 FROM KOTMaster km2 WHERE km2.KOTTblNo = t.KOTTblNo AND km2.SubTable = t.SubTable AND km2.KOTSettled = 0 AND km2.KOTCancelled = 0 AND km2.OltCode = @Outlet AND km2.branch_code = @BranchCode ); ";

            var result = (await connection.QueryAsync<SubTableStatusModel>(qry, new { Outlet = outlet, TableNo = tableno, BranchCode = branchcode })).ToList();
            return result;
        }

        public async Task<List<NCModel>> GetNCKOT(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select NCDepCode,NCDepName from NCDepartment Where Branch_Code = @BranchCode order by NCDepName";

            return (await connection.QueryAsync<NCModel>(qry, new { BranchCode = branchcode })).ToList();
        }

        public async Task<TaxSettingMaster> GetTaxSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select TaxId, TaxRequired, TaxType, BranchCode From dbo.Tbl_TaxSettingMode_Master Where BranchCode = @Branch And TaxRequired = 1";

            return (await connection.QueryFirstOrDefaultAsync<TaxSettingMaster>(qry, new { Branch = branchcode }));
        }

        public async Task<IEnumerable<DiscountModeMaster>> GetDiscountModeMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select DiscId, DiscountRequired, DiscountType, BranchCode From Tbl_DiscountSettingMode_Master \r\n " +
                " Where BranchCode = @BranchCode And DiscountRequired = 1 ";

            return await connection.QueryAsync<DiscountModeMaster>(selectquery, new { BranchCode = branchcode });
        }

        //public async Task<KOTBillModelDto> GetKotBillAsync(int kotid, int kotcancel, int outlet, string table, string subTable, int KotMinTimer)
        //{
        //    KOTBillModelDto kot = new KOTBillModelDto();
        //    List<KOTBillFoodModel> foods = new List<KOTBillFoodModel>();

        //    using var connection = (SqlConnection)_factory.CreateConnection(DbNames.POS);
        //    connection.Open();

        //    string query = @"SELECT km.UserCode, km.KOTId, km.KOTTblNo, km.SubTable, km.OltCode AS Outlet, om.OltName AS OutletName, 
        //                    km.StwCode AS Waiter, sm.StwName AS WaiterName, km.KOTSeatsServed AS Pax, km.branch_code AS Branchcode, km.DRefKOTNo AS NCCode, km.NCKOT_Particulars AS NCRemarks, km.KOTTime, TSM.TimerMinute,
        //                    kd.ItemCode, im.ItemName AS Food, kd.SplInst AS Comment, ic.CatCode AS CatCode, SUM(CONVERT(NUMERIC(10,2), kd.KOTDQty)) AS OrigQty
        //                    FROM kotmaster km
        //                    INNER JOIN kotdetails kd ON km.kotno = kd.kotno
        //                    INNER JOIN ItemMaster im ON im.ItemCode = kd.ItemCode
        //                    INNER JOIN StewardMaster sm ON km.StwCode = sm.StwCode
        //                    LEFT JOIN OutletMaster om ON om.OltCode = km.OltCode
        //                    LEFT JOIN ItemCategory ic ON ic.CatCode = im.CatCode
        //                    LEFT JOIN Tbl_TimerSetting_Master TSM ON TSM.BranchCode = km.branch_code
        //                    WHERE km.KOTCancelled = @KOTCancel AND km.KOTSettled = '0' AND km.KOTId = @KOTID AND km.OltCode = @Outlet AND km.KOTTblNo = @Table AND km.SubTable = @Subtable
        //                    GROUP BY km.UserCode, km.KOTId, km.KOTTblNo, km.SubTable, km.OltCode, om.OltName, km.StwCode, sm.StwName, km.KOTSeatsServed,
        //                    km.branch_code, km.DRefKOTNo, km.NCKOT_Particulars, km.KOTTime, kd.ItemCode, im.ItemName, kd.SplInst, ic.CatCode, TSM.TimerMinute
        //                    ORDER BY kd.ItemCode";

        //    using SqlCommand cmd = new SqlCommand(query, connection);
        //    cmd.Parameters.AddWithValue("@KOTID", kotid);
        //    cmd.Parameters.AddWithValue("@KOTCancel", kotcancel);
        //    cmd.Parameters.AddWithValue("@Outlet", outlet);
        //    cmd.Parameters.AddWithValue("@Table", table);
        //    cmd.Parameters.AddWithValue("@Subtable", subTable);

        //    using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //    while (await reader.ReadAsync())
        //    {
        //        // Fill header only once
        //        if (kot.KOTId == 0)
        //        {
        //            kot.UserCode = Convert.ToInt32(reader["UserCode"]);
        //            kot.KOTId = Convert.ToInt32(reader["KOTId"]);
        //            kot.KOTTblNo = Convert.ToInt32(reader["KOTTblNo"]);
        //            kot.SubTable = reader["SubTable"]?.ToString();
        //            kot.Outlet = Convert.ToInt32(reader["Outlet"]);
        //            kot.OutletName = reader["OutletName"]?.ToString();
        //            kot.Waiter = Convert.ToInt32(reader["Waiter"]);
        //            kot.WaiterName = reader["WaiterName"]?.ToString();
        //            kot.Pax = Convert.ToInt32(reader["Pax"]);
        //            kot.Branchcode = reader["Branchcode"]?.ToString();
        //            kot.NCCode = reader["NCCode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NCCode"]);
        //            kot.NCRemarks = reader["NCRemarks"]?.ToString();

        //            kot.KOTTime = Convert.ToDateTime(reader["KOTTime"]);
        //            kot.KotMinTimer = KotMinTimer;
        //        }

        //        foods.Add(new KOTBillFoodModel
        //        {
        //            ItemCode = Convert.ToInt32(reader["ItemCode"]),
        //            Food = reader["Food"]?.ToString(),
        //            Comment = reader["Comment"]?.ToString(),
        //            Category = reader["CatCode"]?.ToString(),
        //            OrigQty = Convert.ToInt32(reader["OrigQty"])
        //        });
        //    }

        //    kot.Food = foods;

        //    return kot;
        //}

        public async Task<IEnumerable<KotBillQueryResult>> GetKotBillAsync(int kotsettled, int kotid, int kotcancel, int outlet, string table, string subTable, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT km.UserCode, km.KOTId, km.KOTTblNo, km.SubTable, \r\n" +
                " km.OltCode AS Outlet, om.OltName AS OutletName, km.StwCode AS Waiter, sm.StwName AS WaiterName, \r\n" +
                " km.KOTSeatsServed AS Pax, km.branch_code AS Branchcode, km.DRefKOTNo AS NCCode, km.NCKOT_Particulars AS NCRemarks, \r\n" +
                " km.KOTTime, kd.ItemCode, im.ItemName AS Food, OID.OIDRate as ItemRate, kd.SplInst AS Comment, ic.CatCode, im.GrpCode, \r\n" +
                " SUM(CONVERT(NUMERIC(10,2), kd.KOTDQty)) AS OrigQty \r\n" +
                " FROM kotmaster km \r\n" +
                " INNER JOIN kotdetails kd ON km.kotno = kd.kotno AND km.branch_code = kd.branch_code \r\n" +
                " INNER JOIN ItemMaster im ON im.ItemCode = kd.ItemCode AND km.branch_code = im.branch_code \r\n" +
                " INNER JOIN OltItemDetails OID ON OID.ItemCode = kd.ItemCode AND OID.OltCode = km.OltCode AND km.branch_code = OID.branch_code \r\n" +
                " INNER JOIN StewardMaster sm ON km.StwCode = sm.StwCode AND km.branch_code = sm.Branch_Code \r\n" +
                " LEFT JOIN OutletMaster om ON om.OltCode = km.OltCode AND km.branch_code = om.branch_code \r\n" +
                " LEFT JOIN ItemCategory ic ON ic.CatCode = im.CatCode AND km.branch_code = ic.Branch_Code \r\n" +
                " WHERE km.KOTCancelled = @KOTCancel \r\n" +
                " AND km.KOTSettled = @KOTSettled \r\n" +
                " AND km.KOTId = @KOTID \r\n" +
                " AND km.OltCode = @Outlet \r\n" +
                " AND km.KOTTblNo = @Table \r\n" +
                " AND km.SubTable = @Subtable \r\n" +
                " AND km.branch_code = @BranchCode \r\n" +
                " GROUP BY km.UserCode, km.KOTId, km.KOTTblNo, km.SubTable, km.OltCode, om.OltName, km.StwCode, \r\n" +
                " sm.StwName, km.KOTSeatsServed, km.branch_code, km.DRefKOTNo, km.NCKOT_Particulars, km.KOTTime, \r\n" +
                " im.GrpCode, kd.ItemCode, im.ItemName, OID.OIDRate, kd.SplInst, ic.CatCode \r\n" +
                " ORDER BY kd.ItemCode \r\n";

            return await connection.QueryAsync<KotBillQueryResult>(query, new
            {
                KOTSettled = kotsettled,
                KOTID = kotid,
                KOTCancel = kotcancel,
                Outlet = outlet,
                Table = table,
                Subtable = subTable,
                BranchCode = branch
            });
        }

        public async Task<FinancialMaster> GetFinancialMasters(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "SELECT FinId, FinFromDate, FinToDate , FincurrentYear , FinEndYear, CurrentStatus, " +
                " LogUser, IpAddress, FinalClose, FinCode, BranchCode \r\n" +
                " FROM dbo.Tbl_Financial_Master \r\n" +
                " Where CurrentStatus = 1 AND FinalClose = 'N' AND  BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<FinancialMaster>(qry, new { Branch = branchcode }));
        }

        public async Task<List<PrinterSettingMaster>> GetDefaultPrinter(string branchcode, int oltcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "select PrinterName, BillType, Branch_Code, OltCode, PrintType, GrpCode, IPAddress from tmp_printer \r\n" +
                " where Branch_Code = @Branchcode and OltCode = @Outlet";

            return (await connection.QueryAsync<PrinterSettingMaster>(qry, new { Branchcode = branchcode, Outlet = oltcode })).ToList();

        }

        public async Task<List<PrinterSettingMaster>> GetUtilityPrinter(string branchcode, int oltcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "select PrinterName, BillType, Branch_Code, OltCode, PrintType, GrpCode, IPAddress from PrinterNew \r\n" +
                " where Branch_Code = @Branchcode and OltCode = @Outlet And IsDeleted = 0";

            return (await connection.QueryAsync<PrinterSettingMaster>(qry, new { Branchcode = branchcode, Outlet = oltcode })).ToList();

        }

        public async Task<List<CategoryGroupSetting>> GetDefaultCatGrpDetails(string branchcode, int oltcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "select CatGrp, Grp, Branch_code from Tbl_CatGrp_Kot \r\n" +
                " where Branch_code = @Branchcode and OltCode = @Oltcode";

            return (await connection.QueryAsync<CategoryGroupSetting>(qry, new { Branchcode = branchcode, Oltcode = oltcode })).ToList();
        }

        public async Task<FastFoodDetails> GetFastfoodDetails(int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "SELECT s.StwCode, s.POSCode, s.StwName, s.UserCode, t.TblCode, t.OltCode, t.TblNo, t.TblSeatCount, t.Branch_Code, t.QR_Code \r\n" +
                " FROM TableMaster t \r\n" +
                " CROSS JOIN ( SELECT TOP 1 StwCode, POSCode, StwName, UserCode FROM StewardMaster Where Branch_Code = @Branchcode ORDER BY StwCode) s \r\n" +
                " where OltCode = @Outlet and Branch_Code = @Branchcode";

            return await connection.QueryFirstOrDefaultAsync<FastFoodDetails>(selectquery, new { Outlet = outlet, Branchcode = branchcode });
        }

        public async Task<IEnumerable<CompanyMaster>> GetCompanyMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select CompanyCode, CompanyName, ContactPerson, Address, City, Pincode, Phone, Email, UserCode, Branch_code \r\n" +
                " From CompanyMaster \r\n" +
                " Where Branch_code = @BranchCode ";

            return await connection.QueryAsync<CompanyMaster>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<PaymentModeMaster>> GetPaymentModeMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "select pm.ModeId, pm.ModeType, pm.ModeRequired, pm.BranchCode,	psm.SubModeId, psm.SubModeType, psm.ModeId as MasterModeId \r\n" +
                " From dbo.Tbl_PaymentMode_Master pm \r\n" +
                " Left Join dbo.Tbl_PaymentSubMode_Master psm on psm.ModeId = pm.ModeId And psm.BranchCode = pm.BranchCode \r\n" +
                " Where pm.BranchCode = @BranchCode ";

            return await connection.QueryAsync<PaymentModeMaster>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<UnSettlementBillModel>> GetUnbillDetails(int billno, string tblno, string outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string selectquery = "select KSMId, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, SUM(ISNULL(KSMBillAmount, 0) + ISNULL(KSMBillTaxAmt, 0) + " +
                " ISNULL(KSMServiceTaxAmt, 0) + ISNULL(KSMServiceCharge, 0) + ISNULL(KSM_SBCess, 0) + ISNULL(KSM_KKCess, 0) - ISNULL(KSMBillDiscount, 0)) AS Total, " +
                " KSMBillTaxAmt, KSMBillDiscount, KSMTblNo, UserCode, DiscountPercent, KSMSUBTBLNO, STEWCODE, Branch_Code, tips \r\n" +
                " From KOTSettlementMaster \r\n" +
                " Where KSMBillSettled = 0 AND BILLCANCELLED = 0 and KSMBillAmount> 0 and AccountType = 'F' and KsmIsRoomService = '0' and KSMBillNo = @Billno and KSMTblNo = @Tblno and OltCode = @Outlet and Branch_Code = @BranchCode \r\n " +
                " GROUP BY KSMId, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, KSMBillTaxAmt, KSMBillDiscount, KSMTblNo, UserCode, DiscountPercent, KSMSUBTBLNO, STEWCODE, Branch_Code, KSMIsParcelService, tips";

            //else if (actiontype == "NCKOT")
            //{
            //    selectquery = "select KSMId, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, SUM(ISNULL(KSMBillAmount, 0) + ISNULL(KSMBillTaxAmt, 0) + " +
            //    " ISNULL(KSMServiceTaxAmt, 0) + ISNULL(KSMServiceCharge, 0) - ISNULL(KSMBillDiscount, 0)) AS Total, " +
            //    " KSMBillTaxAmt, KSMBillDiscount, KSMTblNo, UserCode, DiscountPercent, KSMSUBTBLNO, STEWCODE, Branch_Code, tips \r\n" +
            //    " From NCKOTSettlementMaster \r\n" +
            //    " Where KSMBillTransfered = 0 And KSMBillSettled = 0 And KSMBillCancled = 0 AND BILLCANCELLED = 0 and" +
            //    " KSMBillAmount> 0 and AccountType = 'F' and KsmIsRoomService = '0' and KSMBillNo = @Billno and KSMTblNo = @Tblno and OltCode = @Outlet and Branch_Code = @BranchCode \r\n " +
            //    " GROUP BY KSMId, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, KSMBillTaxAmt, KSMBillDiscount, KSMTblNo, UserCode, DiscountPercent, KSMSUBTBLNO, STEWCODE, Branch_Code, KSMIsParcelService, tips";
            //}


            return await connection.QueryAsync<UnSettlementBillModel>(selectquery, new { Billno = billno, Tblno = tblno, Outlet = outlet, BranchCode = branchcode });
        }

        public async Task<IEnumerable<KOTTransferTypeMaster>> GetKotTransferTypeMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select TransferId, TransferType, BranchCode From Tbl_TransferKOT_Master Where BranchCode = @BranchCode ";

            return await connection.QueryAsync<KOTTransferTypeMaster>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<bool> KOTTransferTable(KOTTransferRequest req, string FinCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int insertCount = 0;
                int updateCount = 0;

                DateTime POSEntryDate = await GetPOSEntryDate(req.Branch);

                if (req.TransferType.ToLower() == "tabletransfer")
                {
                    var insertqry = @"INSERT INTO tabletransfer
                              SELECT KotNo, KotTblNo, SubTable, KOTDate, @usercode, @branch
                              FROM KotMaster
                              WHERE KOTTBLNO = @oldtableno AND SUBTABLE = @oldsubtable
                              AND KotSettled = 0 AND KotCancelled = 0
                              AND Branch_Code = @Branch AND OltCode = @oldoutlet";

                    insertCount = await connection.ExecuteAsync(insertqry, new
                    {
                        usercode = req.UserCode,
                        branch = req.Branch,
                        oldtableno = req.OldTableNo,
                        oldsubtable = req.OldSubTable,
                        oldoutlet = req.OldOutlet
                    }, transaction);

                    var updateqry = @"UPDATE KOTMASTER
                              SET KOTTblNo = @newtable, SubTable = @newsubtable, OltCode = @newoutlet
                              WHERE KOTTblNo = @oldtableno AND SubTable = @oldsubtable
                              AND KotSettled = 0 AND KotCancelled = 0
                              AND Branch_Code = @branch AND OltCode = @oldoutlet";

                    updateCount = await connection.ExecuteAsync(updateqry, new
                    {
                        newtable = req.NewTable,
                        newsubtable = req.NewSubTable,
                        newoutlet = req.NewOutlet,
                        oldtableno = req.OldTableNo,
                        oldsubtable = req.OldSubTable,
                        branch = req.Branch,
                        oldoutlet = req.OldOutlet
                    }, transaction);
                }
                else if (req.TransferType.ToLower() == "kotnowise")
                {
                    insertCount = 1;
                    foreach (var kotno in req.KotNo)
                    {
                        var updateqry = @"UPDATE KOTMASTER
                              SET KOTTblNo = @newtable, SubTable = @newsubtable, OltCode = @newoutlet
                              WHERE KOTTblNo = @oldtableno AND SubTable = @oldsubtable
                              AND KotSettled = 0 AND KotCancelled = 0 AND KOTId = @Kotno 
                              AND Branch_Code = @branch AND OltCode = @oldoutlet";

                        updateCount = await connection.ExecuteAsync(updateqry, new
                        {
                            Kotno = kotno,
                            newtable = req.NewTable,
                            newsubtable = req.NewSubTable,
                            newoutlet = req.NewOutlet,
                            branch = req.Branch,
                            oldtableno = req.OldTableNo,
                            oldsubtable = req.OldSubTable,
                            oldoutlet = req.OldOutlet
                        }, transaction);
                    }
                }
                else if (req.TransferType.ToLower() == "itemwise")
                {
                    if (req.ItemCode == null || req.ItemCode.Count == 0)
                        throw new Exception("No items selected for transfer");

                    var str = "Select StwCOde from KotMaster \r\n" +
                        " where KOTTBLNO = @tableno and SubTable = @subtable AND KotSettled = 0 and KotCancelled = 0 and Branch_Code = @BranchCode ";

                    var stwcode = await connection.QueryFirstOrDefaultAsync<string>(str, new
                    {
                        tableno = req.OldTableNo,
                        subtable = req.OldSubTable,
                        BranchCode = req.Branch
                    },
                    transaction);

                    var newKotNo = await connection.ExecuteScalarAsync<long>(
                        @"SELECT ISNULL(MAX(KOTNo),0) + 1 FROM KOTMASTER WHERE Branch_Code = @branch",
                        new { branch = req.Branch }, transaction);

                    double movedTotal = 0;
                    double remainingTotal = 0;

                    var insertqry = @"INSERT INTO tabletransfer (KotNo, KotTblNo, subtable, datetimee, username, Branch_Code)
                              SELECT KotNo, KotTblNo, SubTable, KOTDate, @usercode, @branch
                              FROM KotMaster
                              WHERE KOTTBLNO = @oldtableno AND SUBTABLE = @oldsubtable
                              AND KotSettled = 0 AND KotCancelled = 0
                              AND Branch_Code = @Branch AND OltCode = @oldoutlet";

                    insertCount = await connection.ExecuteAsync(insertqry, new
                    {
                        usercode = req.UserCode,
                        branch = req.Branch,
                        oldtableno = req.OldTableNo,
                        oldsubtable = req.OldSubTable,
                        oldoutlet = req.OldOutlet
                    }, transaction);

                    var items = await connection.QueryAsync<dynamic>(@"
                                SELECT kd.KOTNo, kd.ItemCode, kd.kotdRate, kd.kotdqty
                                FROM kotdetails kd
                                INNER JOIN kotmaster km ON km.KOTNo = kd.KOTNo And km.branch_code = kd.branch_code
                                WHERE km.KOTTBLNO = @oldtable
                                AND km.SubTable = @oldsubtable
                                AND km.Branch_Code = @branch
                                AND km.OltCode = @oldoutlet
                                AND km.KotSettled = 0 AND km.KotCancelled = 0",
                        new
                        {
                            oldtable = req.OldTableNo,
                            oldsubtable = req.OldSubTable,
                            branch = req.Branch,
                            oldoutlet = req.OldOutlet
                        }, transaction);

                    foreach (var item in items)
                    {
                        double lineTotal = Convert.ToDouble(item.kotdRate) * Convert.ToDouble(item.kotdqty);

                        if (req.ItemCode.Contains((int)item.ItemCode))
                        {
                            // 3. Move item to new KOT
                            var updateItem = @"UPDATE kotdetails 
                               SET KOTNo = @newkotno, KOTId = @newkotno
                               WHERE KOTNo = @oldKotNo AND ItemCode = @itemCode";

                            await connection.ExecuteAsync(updateItem, new
                            {
                                newkotno = newKotNo,
                                oldKotNo = item.KOTNo,
                                itemCode = item.ItemCode
                            }, transaction);

                            movedTotal += lineTotal;
                        }
                        else
                        {
                            remainingTotal += lineTotal;
                        }
                    }

                    // 4. Insert new KOTMASTER
                    var insertKot = @" 
                                    INSERT INTO KOTMASTER
                                    (KOTId, KOTNo, POSCode, OltCode, KOTTblNo, StwCode,
                                    KOTSeatsServed, KOTDate, KOTTime, KOTChargeable, KOTTotal, KOTCancelled, KOTSettled, UserCode,
                                    LastModify, SubTable, DKOTNO, DayEnd, Branch_Code, IsUploaded, TransNo, IsOnline, FinCode)
                                    VALUES
                                    (@KOTId, @KOTNo, 1, @OltCode, @TableNo, @Stwcode,
                                    1, @kotdate, @kottime, 1, @Total, 0, 0, @usercode,
                                    @kottime, @SubTable, @KOTNo, 'N', @branch, 0, 0, 0, @fincode)";

                    insertCount = await connection.ExecuteAsync(insertKot, new
                    {
                        KOTId = newKotNo,
                        KOTNo = newKotNo,
                        OltCode = req.NewOutlet,
                        TableNo = req.NewTable,
                        Stwcode = stwcode,
                        kotdate = POSEntryDate,
                        kottime = DateTime.Now,
                        Total = movedTotal,
                        usercode = req.UserCode,
                        SubTable = req.NewSubTable,
                        branch = req.Branch,
                        fincode = FinCode
                    }, transaction);

                    // 5. Update OLD KOT total
                    if (remainingTotal > 0)
                    {
                        var updateOld = @"UPDATE KOTMASTER 
                                          SET KOTTotal = @total
                                          WHERE KOTTBLNO = @oldtable AND SubTable = @oldsubtable AND Branch_Code = @branch 
                                          AND OltCode = @oldoutlet AND KotSettled = 0 AND KotCancelled = 0";

                        updateCount = await connection.ExecuteAsync(updateOld, new
                        {
                            total = remainingTotal,
                            oldtable = req.OldTableNo,
                            oldsubtable = req.OldSubTable,
                            branch = req.Branch,
                            oldoutlet = req.OldOutlet
                        }, transaction);
                    }
                    else
                    {
                        // Cancel old KOT if no items left
                        var cancelOld = @"UPDATE KOTMASTER 
                                          SET KOTTotal = @total, KotCancelled = 1
                                          WHERE KOTTBLNO = @oldtable AND SubTable = @oldsubtable AND Branch_Code = @branch
                                          AND OltCode = @oldoutlet AND KotSettled = 0 AND KotCancelled = 0";

                        updateCount = await connection.ExecuteAsync(cancelOld, new
                        {
                            total = remainingTotal,
                            oldtable = req.OldTableNo,
                            oldsubtable = req.OldSubTable,
                            branch = req.Branch,
                            oldoutlet = req.OldOutlet
                        }, transaction);
                    }

                    //insertCount = 1;
                    //updateCount = 1;
                }

                if (insertCount > 0 && updateCount > 0)
                {
                    transaction.Commit();
                    return true;
                }

                transaction.Rollback();
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> KOT2NCKOT(string tableno, string subTable, string branch, int nccode, string ncremarks)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                var updateqry = @"Update KOTMaster set KOTChargeable = 0, DRefKOTNo = @NcCode, NCKOT_Particulars= @NcRemark 
                                  where KOTTblNo = @TableNo and SubTable = @SubTable and KOTSettled!= 1 and 
                                  KOTChargeable = 1 and KOTCancelled!= 1 and branch_code = @Branch ";

                return await connection.ExecuteAsync(updateqry, new
                {
                    NcCode = nccode,
                    NcRemark = ncremarks,
                    TableNo = tableno,
                    SubTable = subTable,
                    Branch = branch
                }) > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> KOT2NCKOT(KOT2NCKOTRequest request)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                string query = string.Empty;

                if (request.ActionType == "KOT2NC")
                {
                    query = @"UPDATE KOTMaster SET KOTChargeable = 0, DRefKOTNo = @NcCode, NCKOT_Particulars = @NcRemarks
                      WHERE KOTTblNo = @TableNo AND SubTable = @SubTable AND KOTSettled != 1 AND KOTChargeable = 1 
                      AND KOTCancelled != 1 AND branch_code = @Branch AND KOTId IN @KOTIds";
                }
                else if (request.ActionType == "NC2KOT")
                {
                    query = @"UPDATE KOTMaster SET KOTChargeable = 1, DRefKOTNo = 0, NCKOT_Particulars = ''
                      WHERE KOTTblNo = @TableNo AND SubTable = @SubTable AND KOTSettled != 1 AND KOTChargeable = 0 
                      AND KOTCancelled != 1 AND branch_code = @Branch AND KOTId IN @KOTIds";
                }
                else
                {
                    throw new Exception("Invalid ActionType");
                }

                return await connection.ExecuteAsync(query, new
                {
                    request.NcCode,
                    request.NcRemarks,
                    request.TableNo,
                    request.SubTable,
                    request.Branch,
                    KOTIds = request.KOTId
                }) > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<KOTBillSettlementModel>> GetFilteredBillDetails(KOTBillSettlementFilter filter)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var query = " SELECT kb.KBSId, kb.OltCode, kb.KSMId, kb.KSMBillNo, kb.KSMBillAmount, kb.KBSSetteleDate, kb.KBSValidDate, " +
            //    " kb.KBSPaymentMode, kb.KBSDiscount, kb.UserCode, kb.Branch_Code FROM KOTBillSettlement kb " +
            //    " LEFT JOIN KOTSettlementMaster ks on ks.KSMId = kb.KSMId and kb.Branch_Code = ks.Branch_Code \r\n" +
            //    " WHERE (@FromDate IS NULL OR TRY_CAST(KBSSetteleDate AS DATE) >= @FromDate) AND (@ToDate IS NULL OR TRY_CAST(KBSSetteleDate AS DATE) <= @ToDate) AND (@BranchCode IS NULL OR kb.Branch_Code = @BranchCode) AND (@OltCode IS NULL OR kb.OltCode = @OltCode) AND ks.BillCancelled = 0 ORDER BY KBSId DESC";

            var query = @$" SELECT COALESCE(kbs.KBSId, 0) AS KBSId, ksm.OltCode As OltCode, ksm.KSMId As KSMId, ksm.KSMBillNo as KSMBillNo, " +
                " CASE WHEN ksm.KSMBillSettled = 0 THEN ABS(ksm.KSMBillAmount +ksm.KSMBillTaxAmt - ksm.KSMBillDiscount + ksm.famt ) " +
                " WHEN ksm.KSMBillSettled = 1 THEN kbs.BillAmount END AS KSMBillAmount, ksm.KSMBillDate AS KBSSetteleDate, ksm.KSMBillTime AS KBSValidDate, " +
                " CASE WHEN ksm.KSMBillSettled = 0 THEN 'Unsettled' WHEN ksm.KSMBillSettled = 1 THEN kbs.PaymentStatus END AS KBSPaymentMode, " +
                " COALESCE(ksm.KSMBillDiscount, 0) AS KBSDiscount, ksm.UserCode AS UserCode, ksm.Branch_Code AS Branch_Code " +
                " FROM KOTSettlementMaster ksm  " +
                " LEFT JOIN ( SELECT KSMId, Branch_Code, MAX(KBSId) AS KBSId, SUM(ISNULL(KSMBillAmount, 0)) AS BillAmount, " +
                " STUFF((SELECT DISTINCT ', ' + kbs2.KBSPaymentMode FROM KOTBillSettlement kbs2 WHERE kbs2.KSMId = kbs1.KSMId AND " +
                " kbs2.Branch_Code = kbs1.Branch_Code AND kbs2.KBSPaymentMode IS NOT NULL FOR XML PATH('') ), 1, 2, '' ) AS PaymentStatus " +
                " FROM KOTBillSettlement kbs1 GROUP BY KSMId, Branch_Code ) kbs ON ksm.KSMId = kbs.KSMId AND ksm.Branch_Code = kbs.Branch_Code " +
                " WHERE TRY_CAST(ksm.KSMBillDate AS DATE) >= @FromDate AND TRY_CAST(ksm.KSMBillDate AS DATE) <= @ToDate " +
                " AND ksm.Branch_Code = @BranchCode AND ksm.OltCode = @OltCode AND ksm.BillCancelled = 0  ORDER BY ksm.KSMId DESC; ";

        var parameters = new
            {
                FromDate = filter.FromDate?.Date,
                ToDate = filter.ToDate?.Date, // ✅ full day coverage
                BranchCode = filter.Branch_Code,
                OltCode = filter.OltCode
            };

            return await connection.QueryAsync<KOTBillSettlementModel>(query, parameters);
        }

        public async Task<IEnumerable<KOTMaster>> KOTMasters(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select KOTId, KOTNo, OltCode, KOTTblNo, StwCode, KOTSeatsServed, KOTDate, KOTTime, KOTChargeable, KOTTotal, \r\n" +
                " KOTCancelled, KOTSettled, UserCode, NCKOT_Particulars, SubTable, DRefKOTNo, DKOTNO, DayEnd, branch_code, FinCode \r\n" +
                " From KOTMaster \r\n" +
                " Where OltCode = @Oltcode and branch_code = @BranchCode \r\n Order by KOTId";

            return await connection.QueryAsync<KOTMaster>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<IEnumerable<KOTDetails>> KOTDetails(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select KOTId, KOTNO, ItemCode, KOTDRate, KOTDQty, branch_code, ItemDiscount,FinCode \r\n" +
                " From KOTDetails Where branch_code = @BranchCode \r\n Order by KOTId";

            return await connection.QueryAsync<KOTDetails>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<KOTSettlementMaster>> SettlementMasters(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select KSMId, POSCode, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, KSMBillAmount, KSMBillTaxAmt, KSMBillDiscount, \r\n " +
                " KSMBillCancled, KSMBillSettled, KSMTblNo, KSMBillTransfered, UserCode, DCParticulars, KSMSUBTBLNO, STEWCODE, DKSMBillNo, DayEnd, \r\n" +
                " TokenNo, Branch_Code, ismodified, FinCode \r\n" +
                " From KOTSettlementMaster Where OltCode = @Oltcode and Branch_Code = @BranchCode \r\n Order by KSMId";

            return await connection.QueryAsync<KOTSettlementMaster>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<IEnumerable<KOTSettlementDetails>> SettlementDetails(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select KSMId, KOTId, Branch_Code, FinCode\r\n" +
                " From KOTSettlementDetails Where Branch_Code = @BranchCode Order by KSMId";

            return await connection.QueryAsync<KOTSettlementDetails>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<KOTBillSettlement>> KOTBillSettlement(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select KBSId, OltCode, KSMId, KSMBillNo, KSMBillAmount, KBSSetteleDate, KBSValidDate, KBSPaymentMode, KBSDiscount, UserCode, Branch_Code \r\n" +
                " From KOTBillSettlement \r\n " +
                " Where OltCode = @Oltcode and Branch_Code = @BranchCode  Order by KBSId";

            return await connection.QueryAsync<KOTBillSettlement>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<IEnumerable<SalesTax>> SalesTaxList(int oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select Bill_No, ItemCode, TaxCode, TaxAmount, Branch_Code, oltcode, billdate as BillDate, ref, IsOnline, FinCode, TaxType \r\n" +
                " From salestax \r\n" +
                " Where oltcode = @Oltcode and Branch_Code = @BranchCode Order by Bill_No";

            return await connection.QueryAsync<SalesTax>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }
        public async Task<IEnumerable<BillTaxModel>> BillTaxList(int oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select BillTaxId, TotalAmount, TotalQty, CGSTAmt, SGSTAmt, ServiceChargePer, ServiceCharge, GrandTotal, DiscountPer, " +
                " Discount, DiscountIn, DiscountRemarks, RoundOff, BillId, BillDate, OltCode, BranchCode From BillTax \r\n" +
                " Where OltCode = @Oltcode and BranchCode = @BranchCode Order by BillId";

            return await connection.QueryAsync<BillTaxModel>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<IEnumerable<BillTaxDetailModel>> BillTaxDetailsList(int oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " select TaxDetailId, BillTaxId, GroupCode, GroupName, TaxName, Taxper, TaxableAmount, TaxAmount, \r\n" +
                " Total, CGST, SGST, BillId, BillDate, OltCode, BranchCode From BillTaxDetails \r\n" +
                " Where OltCode = @Oltcode and BranchCode = @BranchCode Order by BillId";

            return await connection.QueryAsync<BillTaxDetailModel>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<IEnumerable<ItemDiscount>> ItemDiscount(string oltcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select BillNo, Billdate, amount, grpcode, amountperc, discamount, Branch_Code, oltcode, ref, IsOnline, DiscountIn, DiscountType\r\n" +
                " From ItemDiscount \r\n " +
                " Where oltcode = @Oltcode and Branch_Code = @BranchCode Order by BillNo";

            return await connection.QueryAsync<ItemDiscount>(selectquery, new { Oltcode = oltcode, BranchCode = branchcode });
        }

        public async Task<ReprintBillData> GetReprintBillData(int billNo, string oltCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            using var multi = await connection.QueryMultipleAsync(
                "sp_GetReprintBill",
                new { BillNo = billNo, OltCode = oltCode, BranchCode = branchCode },
                commandType: CommandType.StoredProcedure
            );

            //---------------------------------
            // Read all result sets in order
            //---------------------------------
            var settlementMasters = multi.Read<KOTSettlementMaster>().ToList();
            var settlementDetails = multi.Read<KOTSettlementDetails>().ToList();
            var kotMasters = multi.Read<KOTMaster>().ToList();
            var kotDetails = multi.Read<KOTDetails>().ToList();
            var billSettlements = multi.Read<KOTBillSettlement>().ToList();
            var salesTaxes = multi.Read<SalesTax>().ToList();
            var itemDiscounts = multi.Read<ItemDiscount>().ToList();
            var outletMaster = multi.Read<OutletMaster>().FirstOrDefault();
            var itemGroups = multi.Read<ItemGroup>().ToList();
            var stewardMasters = multi.Read<StewardMaster>().ToList();
            var itemMasters = multi.Read<ItemMaster>().ToList();

            // 12a. BillTax (Parent)
            var billTaxList = multi.Read<BillTaxModel>().ToList();

            // 12b. BillTaxDetails (Child)
            var taxDetailsList = multi.Read<BillTaxDetailModel>().ToList();

            // Map TaxDetails into BillTax
            foreach (var bt in billTaxList)
            {
                bt.TaxDetails = taxDetailsList
                    .Where(td => td.BillTaxId == bt.BillTaxId)
                    .ToList();
            }

            return new ReprintBillData
            {
                SettlementMasters = settlementMasters,
                SettlementDetails = settlementDetails,
                KOTMasters = kotMasters,
                KOTDetails = kotDetails,
                BillSettlements = billSettlements,
                SalesTaxes = salesTaxes,
                ItemDiscounts = itemDiscounts,
                OutletMaster = outletMaster,
                ItemGroups = itemGroups,
                StewardMasters = stewardMasters,
                ItemMasters = itemMasters,
                BillTaxList = billTaxList.FirstOrDefault() // Keep as List in case multiple parents exist
            };
        }

        public async Task InsertGSTData(GSTBillDetailModel gstdetails)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string insertquery = @"Insert Into Tbl_GuestGST (GuestName, GSTNo, Address, StateCode, BillNo, Outlet, BillDate)
                   VALUES(@GuestName, @GSTNo, @Address, @StateCode, @BillNo, @OltCode, @BillDate)";

            await connection.ExecuteAsync(insertquery, gstdetails);
        }

        public async Task<int> GetRunningKotCountAsync(string branchcode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = @"SELECT COUNT(1) FROM KOTMaster WHERE KOTCancelled = 0 AND KOTSettled = 0 AND Branch_Code = @BranchId";

            return await con.ExecuteScalarAsync<int>(query, new { BranchId = branchcode });
        }

        public async Task<int> GetPendingBillsCountAsync(string branchcode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM kotsettlementmaster " +
                " WHERE KSMBillTransfered = 0 AND KSMBillSettled = 0 AND KSMBillCancled = 0 AND BILLCANCELLED = 0 AND KSMBillAmount > 0 AND AccountType = 'F' AND KsmIsRoomService = 0 AND Branch_Code = @BranchId";

            return await con.ExecuteScalarAsync<int>(query, new { BranchId = branchcode });
        }

        public async Task<DateTime?> GetOpenShiftDateAsync( string branchCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var hasBranchCode = await con.ExecuteScalarAsync<int>(@"
          SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tbl_ShiftMaster' AND COLUMN_NAME = 'BranchCode'");

            var branchColumn = hasBranchCode > 0 ? "BranchCode" : "Branch_Code";

            string query = @$" SELECT ShiftDate " +
                @$" FROM Tbl_ShiftMaster WHERE ShiftStatus = 'O' AND {branchColumn} = @BranchCode";

            return await con.ExecuteScalarAsync<DateTime?>(query, new { BranchCode = branchCode });
        }

        public async Task<SmsSenderConfig> GetSmsDetailsAsync(string BranchCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "Select SMSId, SMSPwd, SMSSenderId, SMSProvider, MobileNo, BackUpLocation, DBName, " +
                " IsKotPrinter, IsHomeDelivery,	IsCustomerEntry, EmailID, Password,	IsSMS, IsMail, IsPriceShow," +
                " IsDescriptionShow, DayCloseGraceHour, BranchCode " +
                " FROM Tbl_SmsSender WHERE BranchCode = @branchcode";

            return await con.QueryFirstOrDefaultAsync<SmsSenderConfig>(query, new { branchcode = BranchCode });
        }

        //public async Task<PhonePeImageRequestModel> GetPhonePeImageRequest()
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    string query = "Select Id, BaseUrl, MerchantKey, MerchantId, StoreId, TerminalId, ExpiresIn, ProviderId, CallbackUrl, TransactionId From PhonePeImageRequestMaster";

        //    return await connection.QueryFirstOrDefaultAsync<PhonePeImageRequestModel>(query);
        //}

        public async Task<IEnumerable<BillListResponse>> GetBillDetails(CancelBillListModel request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //string query = " SELECT km.KSMId, kd.KOTId, km.OltCode, km.KSMBillNo, km.KSMBillDate," +
            //    " km.KSMBillTime, CAST(km.KSMBillAmount AS DECIMAL(18,2)) AS KSMBillAmount , CAST(km.KSMBillTaxAmt AS DECIMAL(18,2)) AS KSMBillTaxAmt, CAST(km.KSMBillDiscount AS DECIMAL(18,2)) AS KSMBillDiscount, km.KSMBillSettled," +
            //    " km.KSMBillCancled, km.KSMTblNo, km.DiscountPercent, km.DCParticulars as Reason, km.Branch_Code," +
            //    "  CASE WHEN EXISTS ( SELECT 1 FROM ItemDiscount itd WHERE itd.BillNo = kd.KSMId AND itd.Branch_Code = km.Branch_Code ) " +
            //    " THEN 'True' ELSE 'False' END AS HasItemDiscount " +
            //    " FROM KOTSettlementDetails kd " +
            //    " INNER JOIN KOTSettlementMaster km ON km.KSMId = kd.KSMId And kd.Branch_Code = km.Branch_Code" +
            //    " WHERE km.Branch_Code = @Branchcode AND km.OltCode = @Oltcode AND km.BillCancelled = 0 " +
            //    " AND km.KSMBillDate >= @Fromdate AND km.KSMBillDate <= @Todate " +
            //    " order by km.KSMId";

            string query = " SELECT km.KSMId, kd.KOTId, km.OltCode, km.KSMBillNo, km.KSMBillDate, km.KSMBillTime,  \r\n" +
                " CAST(km.KSMBillAmount AS DECIMAL(18,2)) AS KSMBillAmount, CAST(km.KSMBillTaxAmt AS DECIMAL(18,2)) AS KSMBillTaxAmt,  \r\n" +
                " CAST(km.KSMBillDiscount AS DECIMAL(18,2)) AS KSMBillDiscount, km.KSMBillSettled, km.KSMBillCancled, km.KSMTblNo,  \r\n" +
                " km.DiscountPercent, km.DCParticulars AS Reason, km.Branch_Code,  \r\n" +
                " CASE WHEN EXISTS (SELECT 1 FROM ItemDiscount itd WHERE itd.BillNo = kd.KSMId AND itd.Branch_Code = km.Branch_Code ) THEN 'True' ELSE 'False' END AS HasItemDiscount, \r\n" +
                " CASE  WHEN km.KSMBillSettled = 1 AND km.KSMBillTransfered = 1 AND km.KSMIsRoomService = 1 THEN 'Room Transfer' \r\n" +
                " WHEN bt.BillNo IS NOT NULL THEN 'Company Transfer' ELSE STUFF(( SELECT ', ' + kbs1.KBSPaymentMode FROM KOTBillSettlement kbs1  \r\n" +
                " WHERE kbs1.KSMId = km.KSMId AND kbs1.Branch_Code = km.Branch_Code GROUP BY kbs1.KBSPaymentMode FOR XML PATH(''), TYPE ).value('.', 'NVARCHAR(MAX)'),1,2,'') END AS PaymentStatus, bt.CompanyCode \r\n " +
                " FROM KOTSettlementDetails kd \r\n" +
                " INNER JOIN KOTSettlementMaster km ON km.KSMId = kd.KSMId AND kd.Branch_Code = km.Branch_Code \r\n" +
                " LEFT JOIN BillTransferToCompany bt ON bt.BillNo = km.KSMBillNo AND bt.Branch_Code = km.Branch_Code \r\n " +
                " WHERE km.Branch_Code = @Branchcode AND km.OltCode = @Oltcode AND km.BillCancelled = 0 AND km.KSMBillDate >= @Fromdate AND km.KSMBillDate <= @Todate ORDER BY km.KSMId";

            return await connection.QueryAsync<BillListResponse>(query, new { Oltcode = request.OutletCode, Branchcode = request.BranchCode, Fromdate = request.FromDate.ToString("yyyy-MM-dd HH:mm:ss"), Todate = request.ToDate.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        public async Task<bool> CancelBill(CancelBillModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var kotIds = await connection.QueryAsync<int>(
                    @"SELECT kd.KOTId FROM KOTSettlementDetails kd
                      INNER JOIN KOTSettlementMaster km ON km.KSMId = kd.KSMId And kd.Branch_Code = km.Branch_Code
                      WHERE km.KSMBillNo = @BillNo AND km.Branch_Code = @Branch
                      AND km.OltCode = @Outlet AND km.AccountType = 'F' 
                      AND CAST(km.KSMBillDate AS DATE) = @BillDate",
                    new
                    {
                        model.BillNo,
                        model.Branch,
                        model.Outlet,
                        BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss")
                    }, transaction);

                if (!kotIds.Any())
                {
                    transaction.Rollback();
                    return false;
                }

                // Update Settlement Master
                await connection.ExecuteAsync(
                    @"UPDATE KOTSettlementMaster SET IsModified = 1, IsUploaded = 0, BillCancelled = 1, DCParticulars = @Reason
                      WHERE KSMBillNo = @BillNo AND Branch_Code = @Branch AND OltCode = @Outlet AND AccountType = 'F'
                      AND CAST(KSMBillDate AS DATE) = @BillDate",
                    new
                    {
                        model.Reason,
                        model.BillNo,
                        model.Branch,
                        model.Outlet,
                        BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss")
                    }, transaction);

                // Get KSMId
                var ksmId = await connection.ExecuteScalarAsync<int>(
                    @"SELECT TOP 1 KSMId FROM KOTSettlementMaster
                      WHERE KSMBillNo = @BillNo AND Branch_Code = @Branch AND CAST(KSMBillDate AS DATE) = @BillDate",
                    new
                    {
                        model.BillNo,
                        model.Branch,
                        BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss")
                    }, transaction);

                // Insert Bill Cancellation
                await connection.ExecuteAsync(
                    @"INSERT INTO KOTBillCancelation
                      ( KSMId, KSMBillNo, KBCDate, KBCDesc, UserCode, LastModify, Branch_Code )
                      VALUES
                      ( @KSMId, @BillNo, @BillDate, @Reason, @UserId, @LastModify, @Branch )",
                    new
                    {
                        KSMId = ksmId,
                        model.BillNo,
                        BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        model.Reason,
                        model.UserId,
                        LastModify = DateTime.Now,
                        model.Branch
                    }, transaction);

                foreach (var kotId in kotIds)
                {
                    var kotInfo = await connection.QueryFirstOrDefaultAsync<KotInfo>(
                        @"SELECT KOTNo, SUM(KOTTotal) Amount FROM KOTMaster
                          WHERE KOTId = @KOTId AND Branch_Code = @Branch AND OltCode = @Outlet AND CAST(KOTDate AS DATE) = @BillDate
                          GROUP BY KOTNo",
                        new
                        {
                            KOTId = kotId,
                            model.Branch,
                            model.Outlet,
                            BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss")
                        }, transaction);

                    if (kotInfo == null)
                        continue;

                    // Cancel KOT
                    await connection.ExecuteAsync(
                        @"UPDATE KOTMaster SET IsUploaded = 0, KOTCancelled = 1
                          WHERE KOTNo = @KOTNo AND OltCode = @Outlet AND Branch_Code = @Branch AND CAST(KOTDate AS DATE) = @BillDate",
                        new
                        {
                            kotInfo.KOTNo,
                            model.Outlet,
                            model.Branch,
                            BillDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss")
                        }, transaction);


                    //// Update Food Bills
                    //await connection.ExecuteAsync(
                    //    @"UPDATE Tbl_FoodBills SET BillStatus = 'Y'
                    //      WHERE RcptNo = @BillNo AND BillStatus = 'N' AND OutLateName = @Outlet",
                    //    new
                    //    {
                    //        model.BillNo,
                    //        model.Outlet
                    //    }, transaction);

                    // Insert KOT Cancellation
                    await connection.ExecuteAsync(
                        @"INSERT INTO KOTCancelation
                        ( KOTId, KOTCDate, KOTCDesc, UserCode, LastModify, Branch_Code )
                        VALUES
                        ( @KOTId, @KOTCDate, @Reason, @UserId, @LastModify, @Branch )",
                        new
                        {
                            KOTId = kotId,
                            KOTCDate = model.BillDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            model.Reason,
                            model.UserId,
                            LastModify = DateTime.Now,
                            model.Branch
                        }, transaction);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }


        public async Task<bool> IsRoomServiceAsync(int outletCode, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @" SELECT OltIsRoomService FROM OutletMaster WHERE OltCode = @OutletCode AND Branch_Code = @Branch";

            var result = await connection.QueryFirstOrDefaultAsync<bool?>(sql, new { OutletCode = outletCode, Branch = branch });

            return result ?? false;
        }

        public async Task<bool> UpdateFoodBillStatusAsync(string outletName, string billNo, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            const string sql = @" UPDATE Tbl_FoodBills SET BillStatus='Y' WHERE OutlateName=@OutletName AND RcptNo=@BillNo AND TrDate=@BillDate";

            int rowsAffected = await connection.ExecuteAsync(sql, new { OutletName = outletName, BillNo = billNo, BillDate = billDate });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteFoodBillAsync(string outletName, string billNo, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            const string sql = @" DELETE FROM Tbl_FoodBills WHERE OutlateName=@OutletName AND RcptNo=@BillNo AND TrDate=@BillDate";

            int rowsAffected = await connection.ExecuteAsync(sql, new { OutletName = outletName, BillNo = billNo, BillDate = billDate });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteOutstandingBillAsync(string billNo, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            const string sql = @" DELETE FROM Tbl_OutStanding_Slave WHERE BillNo=@BillNo AND SettledDate=@BillDate";

            int rowsAffected = await connection.ExecuteAsync(sql, new { BillNo = billNo, BillDate = billDate });
            return rowsAffected > 0;
        }

        public async Task UpdateKotSettlementMasterAsync(string billNo, int outletCode, string branch, bool isRoomService, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @"
            UPDATE KOTSettlementMaster
            SET Isuploaded = 0,
                Ismodified = 1,
                ksmbillsettled = 0,
                TIPS = 0,
                GuestCode = '1',
                KSMIsRoomService = @RoomService,
                KSMBillTransfered = @BillTransferred
            WHERE ksmbillno = @BillNo
            AND oltcode = @OutletCode
            AND Branch_Code = @Branch
            AND AccountType = 'F' 
            AND CAST(KSMBillDate AS DATE) = @BillDate ";

            await connection.ExecuteAsync(sql,
                new
                {
                    BillNo = billNo,
                    OutletCode = outletCode,
                    Branch = branch,
                    RoomService = isRoomService ? "1" : "0",
                    BillTransferred = isRoomService ? "1" : "0",
                    BillDate = billDate.ToString("yyyy-MM-dd HH:mm:ss")

                });
        }

        public async Task DeleteKotBillSettlementAsync(string billNo, int outletCode, string branch, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @" DELETE FROM KOTBillSettlement WHERE ksmbillno = @BillNo AND CAST(KBSSetteleDate AS DATE) = @BillDate AND oltcode = @OutletCode AND Branch_Code = @Branch AND AccountType = 'F'";

            await connection.ExecuteAsync(sql, new { BillNo = billNo, BillDate = billDate.Date, OutletCode = outletCode, Branch = branch });
        }

        public async Task DeleteBillTransferToCompanyAsync(string billNo, int outletCode, string branch, DateTime billDate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @" DELETE FROM BillTransferToCompany WHERE billno=@BillNo AND oltcode=@OutletCode AND Branch_Code=@Branch AND CAST(BTDate AS DATE) = @BillDate";

            await connection.ExecuteAsync(sql, new { BillNo = billNo, OutletCode = outletCode, Branch = branch, BillDate = billDate });
        }

        //public async Task<bool> TransferToRoom(SettlementModel settlement, DateTime posentrydate, DateTime istTime, string outletname)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    string str;

        //    str = @"DELETE FROM Tbl_FoodBills WHERE RcptNo = @BillNo AND RoomNo = @RoomNo AND OutlateName = @OutletName";

        //    await connection.ExecuteAsync(str, new
        //    {
        //        BillNo = settlement.Bill.BillNo,
        //        RoomNo = settlement.RoomNo,
        //        OutletName = outletname
        //    });

        //    str = @"INSERT INTO Tbl_FoodBills
        //    ( RNo, TrDate, RcptNo, GuestCode, GuestName, CheckInNo, RoomNo, BillAmt, SplitId, BillStatus, OutlateName )
        //    VALUES
        //    ( @RNo, @TrDate, @RcptNo, @GuestCode, @GuestName, @CheckInNo, @RoomNo, @BillAmt, 0, 'N', @OutletName )";

        //    int saved = await connection.ExecuteAsync(str, new
        //    {
        //        RNo = findhotelnextnumber("Tbl_FoodBills", "RNo"),
        //        TrDate = posentrydate,
        //        RcptNo = settlement.Bill.BillNo,
        //        GuestCode = settlement.GuestCode,
        //        GuestName = settlement.GuestName,
        //        CheckInNo = settlement.CheckInNo,
        //        RoomNo = settlement.RoomNo,
        //        BillAmt = settlement.Bill.GrandAmount,
        //        OutletName = outletname
        //    });

        //    if (saved > 0)
        //    {
        //        str = @"UPDATE KOTSettlementMaster
        //        SET Isuploaded = '0',
        //            KSMIsRoomService = 1,
        //            KSMBillSettled = 1,
        //            KSMBillTransfered = 0,
        //            KSMTblNo = @RoomNo,
        //            oltcode = @OutletCode
        //        WHERE KSMId = @BillId AND Branch_Code = @Branch AND AccountType = 'F'";

        //        saved = await connection.ExecuteAsync(str, new
        //        {
        //            RoomNo = settlement.RoomNo,
        //            OutletCode = settlement.OutletCode,
        //            BillId = settlement.Bill.BillId,
        //            Branch = settlement.Branch
        //        });
        //    }

        //    return saved > 0;
        //}

        public async Task<OutletConfigModel> GetOutletNameAsync(int outletCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"SELECT OltName AS OutletName, IsDirectKOTandBill, IsDirectPaxandStw FROM OutletMaster WHERE OltCode = @oltcode AND Branch_Code = @BranchCode";

            return await connection.QueryFirstOrDefaultAsync<OutletConfigModel>(sql, new { oltcode = outletCode, BranchCode = branchCode });
        }

        public async Task<decimal> GetTotalBillAmountAsync(string companyCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = " SELECT ISNULL(SUM(BillAmt),0) FROM BillTransferToCompany \r\n" +
                " WHERE CompanyCode=@CompanyCode AND BTCSettled=0 AND Branch_Code=@BranchCode";

            return await connection.ExecuteScalarAsync<decimal>(
                sql,
                new
                {
                    CompanyCode = companyCode,
                    BranchCode = branchCode
                });
        }

        public async Task<decimal> GetPaidAmountAsync(int billNo, int companyCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = " SELECT ISNULL(SUM(AmountPaid),0) FROM CompanyBillSettlement \r\n" +
                " WHERE CompanyCode=@CompanyCode AND billNo = @BillNo AND Branch_Code=@BranchCode";

            return await connection.ExecuteScalarAsync<decimal>(
                sql,
                new
                {
                    BillNo = billNo,
                    CompanyCode = companyCode,
                    BranchCode = branchCode
                });
        }

        public async Task<List<CompanyBillModel>> GetCompanyBillsAsync(string companyCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = " SELECT BTId, CompanyCode, BTDate, BTTime, BillNo, BillAmt, UserCode, BTCSettled, SettleDate, \r\n " +
                " PMode, AmtPaid, Oltcode, Discount, Branch_Code, IsUploaded, IsModified \r\n" +
                " From BillTransferToCompany \r\n" +
                " WHERE CompanyCode=@CompanyCode AND BTCSettled=0 AND Branch_Code=@BranchCode \r\n" +
                " ORDER BY BillNo";

            var result = await connection.QueryAsync<CompanyBillModel>(sql, new { CompanyCode = companyCode, BranchCode = branchCode });

            return result.ToList();
        }

        public async Task<bool> SaveCompanyBillSettlement(CompanyBillSettlementRequest request, int BillNo, decimal BillAmount, decimal AmountPaid, decimal CurrentPay, int Set, List<ChargeDetail> Chargeslist, int BTId, IDbConnection con)
        {
            int Nxtid = 0;
            string infoString = string.Empty;
            decimal infoDecimal = 0;
            try
            {
                if (true)
                {
                    infoString = string.Join(",", Chargeslist.Select(x => x.ChargesType));
                    infoDecimal = Chargeslist.Sum(x => x.ChargesAmount);
                }

                Nxtid = await findnextnumber("CompanyBillSettlement", "CBSId", "Branch_Code", request.Branch_Code);

                if (request.PaymentMode.ToLower() == "card")
                {
                    const string insertSql = @"
                        INSERT INTO CompanyBillSettlement
                        ( CBSId, CompanyCode, BillAmount, AmountPaid, SettleDate, UserCode, PMode, BillNo, Branch_Code, CCNO, RefNo, ValidDate, ChargesType, ChargesAmount )
                        VALUES
                        ( @nxtid, @cmpcode, @billamount, @amountpaid, @settledate, @usercode, @paymentmode, @billno, @branchcode, @ccno, @refno, @validdate, @chargestype, @chargesamount )";

                    int rowsAffected = await con.ExecuteAsync(
                            insertSql,
                            new
                            {
                                nxtid = Nxtid,
                                cmpcode = request.CompanyCode,
                                billamount = BillAmount,
                                amountpaid = CurrentPay,
                                settledate = request.SettleDate,
                                usercode = request.UserCode,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                branchcode = request.Branch_Code,
                                ccno = request.CCNO,
                                refno = request.RefNo,
                                validdate = request.ValidDate,
                                chargestype = infoString,
                                chargesamount = infoDecimal
                            });

                    if (rowsAffected > 0)
                    {
                        const string updateSql = @"
                        UPDATE BillTransferToCompany
                        SET Ismodified = '1', BTCSettled = @set, AmtPaid = @amount, SettleDate = @settledate, PMode = @paymentmode
                        WHERE BillNo = @billno AND CompanyCode = @cmpcode AND Branch_Code = @branchcode";

                        await con.ExecuteAsync(
                            updateSql,
                            new
                            {
                                set = Set,
                                amount = (AmountPaid + CurrentPay),
                                settledate = request.SettleDate,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                cmpcode = request.CompanyCode,
                                branchcode = request.Branch_Code
                            }
                        );

                        foreach (var item in Chargeslist)
                        {
                            const string insertChargeSql = @"
                                INSERT INTO Tbl_ChargesDetails
                                ( BillNo, CompanyCode, BTId, ChargesType, ChargesAmount, BranchCode )
                                VALUES
                                ( @billno, @cmpcode, @btid, @type, @amount, @branchcode )";
                            await con.ExecuteAsync(
                                insertChargeSql,
                                new
                                {
                                    billno = BillNo,
                                    cmpcode = request.CompanyCode,
                                    btid = BTId,
                                    type = item.ChargesType,
                                    amount = item.ChargesAmount,
                                    branchcode = request.Branch_Code
                                });
                        }
                    }
                }
                else if (request.PaymentMode.ToLower() == "cheque")
                {
                    const string insertSql = @"
                        INSERT INTO CompanyBillSettlement
                        ( CBSId, CompanyCode, BillAmount, AmountPaid, SettleDate, UserCode, PMode, BillNo, Branch_Code, BankName, BranchName, ChDDNo, ValidDate, ChargesType, ChargesAmount )
                        VALUES
                        ( @nxtid, @cmpcode, @billamount, @amountpaid, @settledate, @usercode, @paymentmode, @billno, @branchcode, @bankname, @branchname, @chddno, @validdate, @chargestype, @chargesamount )";

                    int rowsAffected = await con.ExecuteAsync(
                            insertSql,
                            new
                            {
                                nxtid = Nxtid,
                                cmpcode = request.CompanyCode,
                                billamount = BillAmount,
                                amountpaid = CurrentPay,
                                settledate = request.SettleDate,
                                usercode = request.UserCode,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                branchcode = request.Branch_Code,
                                bankname = request.BankName,
                                branchname = request.BranchName,
                                chddno = request.ChDDNo,
                                validdate = request.ValidDate,
                                chargestype = infoString,
                                chargesamount = infoDecimal
                            });

                    if (rowsAffected > 0)
                    {
                        const string updateSql = @"
                        UPDATE BillTransferToCompany
                        SET Ismodified = '1', BTCSettled = @set, AmtPaid = @amount, SettleDate = @settledate, PMode = @paymentmode
                        WHERE BillNo = @billno AND CompanyCode = @cmpcode AND Branch_Code = @branchcode";

                        await con.ExecuteAsync(
                            updateSql,
                            new
                            {
                                set = Set,
                                amount = (AmountPaid + CurrentPay),
                                settledate = request.SettleDate,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                cmpcode = request.CompanyCode,
                                branchcode = request.Branch_Code
                            }
                        );

                        foreach (var item in Chargeslist)
                        {
                            const string insertChargeSql = @"
                                INSERT INTO Tbl_ChargesDetails
                                ( BillNo, CompanyCode, BTId, ChargesType, ChargesAmount, BranchCode )
                                VALUES
                                ( @billno, @cmpcode, @btid, @type, @amount, @branchcode )";
                            await con.ExecuteAsync(
                                insertChargeSql,
                                new
                                {
                                    billno = BillNo,
                                    cmpcode = request.CompanyCode,
                                    btid = BTId,
                                    type = item.ChargesType,
                                    amount = item.ChargesAmount,
                                    branchcode = request.Branch_Code
                                });
                        }
                    }
                }
                else if (request.PaymentMode.ToLower() == "bank")
                {
                    const string insertSql = @"
                        INSERT INTO CompanyBillSettlement
                        ( CBSId, CompanyCode, BillAmount, AmountPaid, SettleDate, UserCode, PMode, BillNo, Branch_Code, BankName, BranchName, CCNO, RefNo, ValidDate, ChargesType, ChargesAmount )
                        VALUES
                        ( @nxtid, @cmpcode, @billamount, @amountpaid, @settledate, @usercode, @paymentmode, @billno, @branchcode, @bankname, @branchname, @ccno, @refno, @validdate, @chargestype, @chargesamount )";

                    int rowsAffected = await con.ExecuteAsync(
                            insertSql,
                            new
                            {
                                nxtid = Nxtid,
                                cmpcode = request.CompanyCode,
                                billamount = BillAmount,
                                amountpaid = CurrentPay,
                                settledate = request.SettleDate,
                                usercode = request.UserCode,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                branchcode = request.Branch_Code,
                                bankname = request.BankName,
                                branchname = request.BranchName,
                                ccno = request.CCNO,
                                refno = request.RefNo,
                                validdate = request.ValidDate,
                                chargestype = infoString,
                                chargesamount = infoDecimal

                            });

                    if (rowsAffected > 0)
                    {
                        const string updateSql = @"
                        UPDATE BillTransferToCompany
                        SET Ismodified = '1', BTCSettled = @set, AmtPaid = @amount, SettleDate = @settledate, PMode = @paymentmode
                        WHERE BillNo = @billno AND CompanyCode = @cmpcode AND Branch_Code = @branchcode";

                        await con.ExecuteAsync(
                            updateSql,
                            new
                            {
                                set = Set,
                                amount = (AmountPaid + CurrentPay),
                                settledate = request.SettleDate,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                cmpcode = request.CompanyCode,
                                branchcode = request.Branch_Code
                            }
                        );

                        foreach (var item in Chargeslist)
                        {
                            const string insertChargeSql = @"
                                INSERT INTO Tbl_ChargesDetails
                                ( BillNo, CompanyCode, BTId, ChargesType, ChargesAmount, BranchCode )
                                VALUES
                                ( @billno, @cmpcode, @btid, @type, @amount, @branchcode )";
                            await con.ExecuteAsync(
                                insertChargeSql,
                                new
                                {
                                    billno = BillNo,
                                    cmpcode = request.CompanyCode,
                                    btid = BTId,
                                    type = item.ChargesType,
                                    amount = item.ChargesAmount,
                                    branchcode = request.Branch_Code
                                });
                        }
                    }
                }
                else if (request.PaymentMode.ToLower() == "cash" || request.PaymentMode.ToLower() == "online")
                {
                    const string insertSql = @"
                        INSERT INTO CompanyBillSettlement
                        ( CBSId, CompanyCode, BillAmount, AmountPaid, SettleDate, UserCode, PMode, BillNo, Branch_Code, ChargesType, ChargesAmount )
                        VALUES
                        ( @nxtid, @cmpcode, @billamount, @amountpaid, @settledate, @usercode, @paymentmode, @billno, @branchcode, @chargestype, @chargesamount )";

                    int rowsAffected = await con.ExecuteAsync(
                            insertSql,
                            new
                            {
                                nxtid = Nxtid,
                                cmpcode = request.CompanyCode,
                                billamount = BillAmount,
                                amountpaid = CurrentPay,
                                settledate = request.SettleDate,
                                usercode = request.UserCode,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                branchcode = request.Branch_Code,
                                chargestype = infoString,
                                chargesamount = infoDecimal
                            });

                    if (rowsAffected > 0)
                    {
                        const string updateSql = @"
                        UPDATE BillTransferToCompany
                        SET Ismodified = '1', BTCSettled = @set, AmtPaid = @amount, SettleDate = @settledate, PMode = @paymentmode
                        WHERE BillNo = @billno AND CompanyCode = @cmpcode AND Branch_Code = @branchcode";

                        await con.ExecuteAsync(
                            updateSql,
                            new
                            {
                                set = Set,
                                amount = (AmountPaid + CurrentPay),
                                settledate = request.SettleDate,
                                paymentmode = request.PaymentMode,
                                billno = BillNo,
                                cmpcode = request.CompanyCode,
                                branchcode = request.Branch_Code
                            }
                        );

                        foreach (var item in Chargeslist)
                        {
                            const string insertChargeSql = @"
                                INSERT INTO Tbl_ChargesDetails
                                ( BillNo, CompanyCode, BTId, ChargesType, ChargesAmount, BranchCode )
                                VALUES
                                ( @billno, @cmpcode, @btid, @type, @amount, @branchcode )";
                            await con.ExecuteAsync(
                                insertChargeSql,
                                new
                                {
                                    billno = BillNo,
                                    cmpcode = request.CompanyCode,
                                    btid = BTId,
                                    type = item.ChargesType,
                                    amount = item.ChargesAmount,
                                    branchcode = request.Branch_Code
                                });
                        }
                    }
                }

                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IEnumerable<ChargesMasterModel>> GetChargesDetails(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = " SELECT ChargesId, ChargesType, BranchCode \r\n" +
                " From Tbl_Charges_Master \r\n" +
                " WHERE BranchCode = @BranchCode";

            var result = await connection.QueryAsync<ChargesMasterModel>(sql, new { BranchCode = branchCode });

            return result.ToList();
        }

        #region ModifyBill

        public async Task<PosModifyBill> GetModifyBillData(string orderid, int oltcode, int billno, string branchcode, DateTime settledDate)
        {
            try
            {
                using var con = _factory.CreateConnection(DbNames.POS);

                var orderIds = orderid.Split(',').Select(x => Convert.ToInt32(x.Trim())).ToList();

                string sql = "SELECT  km.KOTId, km.KOTTblNo,km.OltCode,km.branch_code AS branchcode, \r\n" +
                    " km.KOTSeatsServed,kd.ItemCode,im.ItemName AS Food, CONVERT(NVARCHAR, km.KOTNo) AS Code, \r\n" +
                    " kd.KOTDRate AS Price,SUM(CONVERT(NUMERIC(10,2), kd.KOTDQty)) AS Qty,ISNULL(kd.SplInst,'') AS Comment, \r\n" +
                    " SUM(CONVERT(NUMERIC(10,2), kd.KOTDQty)) AS OrigQty,im.ItemDiscountAllowed,im.GrpCode,im.CatCode \r\n" +
                    "  FROM KOTMaster km \r\n" +
                    " INNER JOIN KOTDetails kd ON km.KOTNo = kd.KOTNo And kd.branch_code = km.branch_code \r\n" +
                    " INNER JOIN ItemMaster im ON im.ItemCode = kd.ItemCode And im.Branch_Code = km.branch_code \r\n" +
                    " WHERE km.KOTCancelled = 0 AND km.KOTSettled = 1 AND km.KOTId IN @OrderIds  AND km.OltCode = @OltCode And km.branch_code = @BranchCode \r\n" +
                    " And KOTDate >= @SettledDate AND KOTDate < DATEADD(DAY, 1, @SettledDate)" +
                    " GROUP BY km.KOTId, km.KOTTblNo,km.OltCode,km.branch_code,km.KOTSeatsServed,kd.ItemCode,im.ItemName, \r\n" +
                    " km.KOTNo,kd.KOTDRate,kd.SplInst, im.ItemDiscountAllowed,im.GrpCode,im.CatCode ORDER BY kd.ItemCode \r\n";

                var rows = (await con.QueryAsync(sql, new { OrderIds = orderIds, OltCode = oltcode, BranchCode = branchcode, SettledDate = settledDate })).ToList();

                if (!rows.Any())
                    return new PosModifyBill();

                var result = rows
                    .GroupBy(x => new
                    {
                        KOTTblNo = Convert.ToString(x.KOTTblNo),
                        OltCode = Convert.ToInt32(x.OltCode),
                        branchcode = Convert.ToString(x.branchcode),
                        KOTSeatsServed = Convert.ToInt32(x.KOTSeatsServed)
                    })
                    .Select(g => new PosModifyBill
                    {
                        KOTTblNo = g.Key.KOTTblNo,
                        OltCode = g.Key.OltCode,
                        branchcode = g.Key.branchcode,
                        KOTSeatsServed = g.Key.KOTSeatsServed,

                        Food = g.Select(f => new FoodModel
                        {
                            KOTId = Convert.ToInt32(f.KOTId),
                            Id = Convert.ToInt32(f.ItemCode),
                            Food = Convert.ToString(f.Food),
                            code = Convert.ToString(f.Code),
                            Price = Convert.ToDouble(f.Price),
                            Qty = Convert.ToInt32(f.Qty),
                            Comment = Convert.ToString(f.Comment),
                            Category = Convert.ToInt32(f.CatCode),
                            GrpCode = Convert.ToInt32(f.GrpCode),
                            OrigQty = Convert.ToInt32(f.OrigQty),
                            itemDiscountAllowed = Convert.ToBoolean(f.ItemDiscountAllowed)
                        }).ToList()
                    })
                    .FirstOrDefault();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<BillDiscountModel> GetBillDiscountData(int oltcode, int billno, string branchcode, DateTime settledDate)
        {
            try
            {
                using var con = _factory.CreateConnection(DbNames.POS);


                string sql = "select BillNo, Billdate, amount, grpcode, amountperc, discamount, Branch_Code, oltcode, ref as reference, IsOnline, DiscountIn, DiscountType \r\n" +
                    " From ItemDiscount \r\n" +
                    " Where BillNo = @BillNo AND oltcode = @OltCode AND Branch_Code = @Branch_Code AND CAST(Billdate AS DATETIME) >= @SettledDate AND CAST(Billdate AS DATETIME) < DATEADD(DAY,1,@SettledDate)";

                var result = (await con.QueryFirstOrDefaultAsync<BillDiscountModel>(sql, new { BillNo = billno, OltCode = oltcode, Branch_Code = branchcode, SettledDate = settledDate }));

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UnsettledBillDelete(int KSMId, int OltCode, int UserCode, string Branch_Code)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var deleteRows = await connection.ExecuteAsync(
                    @"DELETE FROM KOTBillSettlement WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @Branch_Code",
                    new { KSMId, OltCode, Branch_Code }, transaction);
                if (deleteRows == 0)
                {
                    transaction.Rollback();
                    return false;
                }
                //var updateRows = await connection.ExecuteAsync(
                //    @"UPDATE KOTSettlementMaster SET KSMBillSettled = 0, IsModified = 1 WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @Branch_Code",
                //    new { KSMId, OltCode, Branch_Code }, transaction);

                //if (updateRows == 0)
                //{
                //    transaction.Rollback();
                //    return false;
                //}
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<bool> UnsettledKotBillDelete(int KOTId, int itemcode, string Branch_Code)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            if (connection.State != ConnectionState.Open)
                connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var deleteRows = await connection.ExecuteAsync(
                    @"DELETE FROM KOTDetails WHERE KOTId = @KOTId AND ItemCode = @itemcode AND branch_code = @Branch_Code",
                    new { KOTId, itemcode, Branch_Code });
                transaction.Commit();
                return true;
            }
            catch
            {
                throw;
            }
        }
        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }
        public async Task<PosModifyBillSettlementResponse> ModifyBillCreateUpdate(PosModifyBillSettlement model, List<SalesTax> salesTax, BillTaxModel BillTax, BillTaxDetailModel BillTaxDetails, string FinCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            using var transaction = connection.BeginTransaction();

            try
            {
                decimal CurrentBillAmount = Convert.ToDecimal(model.Taxdetails.TotalAmount);
                decimal CurrentBillTaxAmt = Convert.ToDecimal(model.Taxdetails.CGSTAmt + model.Taxdetails.SGSTAmt);
                decimal CurrentBillDiscount = Convert.ToDecimal(model.Taxdetails.Discount);
                decimal roundOff = Convert.ToDecimal(model.Taxdetails.RoundOff);
                decimal GrandTotal = Convert.ToDecimal(model.Taxdetails.GrandTotal);

                //DateTime POSEntryDate = GetPOSEntryDate();
                //DateTime istTime = ConvertUtcToIst();

                //// Step 1 - Settlement
                //if (model.settlement != null)
                //{
                //    foreach (var item in model.settlement)
                //    {
                //        var result = await SettleBill(item, POSEntryDate, istTime);
                //        if (!result)
                //        {
                //            throw new Exception("SettleBill failed");
                //        }
                //    }
                //}

                //// Step 1
                //var adjustableMode = await connection.QueryFirstOrDefaultAsync<string>
                //    (@"SELECT TOP 1 KBSPaymentMode FROM KOTBillSettlement WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @BranchCode AND KBSPaymentMode IN ('Cash','UPI') ORDER BY CASE WHEN KBSPaymentMode='Cash' THEN 1 WHEN KBSPaymentMode='UPI' THEN 2 END", new { model.KSMId, model.OltCode, model.Branch_Code }, transaction);


                //var otherPayments = await connection.QuerySingleAsync<decimal>
                //    ( @"SELECT ISNULL(SUM(KSMBillAmount),0) FROM KOTBillSettlement WHERE KSMId = @KSMId AND OltCode = @OltCode AND UserCode = @UserCode AND Branch_Code = @BranchCode AND KBSPaymentMode <> @PaymentMode", new { model.KSMId, model.OltCode, model.UserCode, BranchCode = model.Branch_Code, PaymentMode = adjustableMode }, transaction);

                //var adjustedAmount = model.GrandTotal - otherPayments;

                //await connection.ExecuteAsync( @"UPDATE KOTBillSettlement SET KSMBillAmount = @Amount WHERE KSMId = @KSMId AND OltCode = @OltCode AND UserCode = @UserCode AND Branch_Code = @BranchCode AND KBSPaymentMode = @PaymentMode", new { Amount = adjustedAmount, model.KSMId, model.OltCode, model.UserCode, BranchCode = model.Branch_Code, PaymentMode = adjustableMode }, transaction);

                //const string updatetqry = "UPDATE KOTBillSettlement SET KSMBillAmount = @ksmamount " +
                //    " WHERE KSMId = @ksmid AND OltCode = @oltcode AND usercode = @usercode AND Branch_Code = @branch_code";
                //await connection.ExecuteAsync(updatetqry, new { ksmid = model.KSMId, ksmamount = model.GrandTotal, oltcode = model.OltCode, usercode = model.UserCode, branch_code = model.Branch_Code }, transaction);

                decimal previousbill = Math.Abs(model.PreviousBillAmount + model.PreviousBillTaxAmt - model.PreviousBillDiscount);
                decimal currentbill = Math.Abs(CurrentBillAmount + CurrentBillTaxAmt - CurrentBillDiscount + roundOff);

                // Step 1
                var adjustableMode = await connection.QueryFirstOrDefaultAsync<string>(@"SELECT TOP 1 KBSPaymentMode FROM KOTBillSettlement WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @BranchCode AND CAST(KBSSetteleDate AS DATE) = @settledate ORDER BY CASE WHEN KBSPaymentMode = 'Cash' THEN 1 WHEN KBSPaymentMode = 'UPI' THEN 2 ELSE 3 END", new { model.KSMId, model.OltCode, BranchCode = model.Branch_Code, settledate = model.SettledDate }, transaction);

                var CompanybillMode = await connection.QueryFirstOrDefaultAsync<string>(@"SELECT TOP 1 BTId FROM BillTransferToCompany WHERE BillNo = @KSMBillNo AND Oltcode = @OltCode AND CompanyCode = @CompanyCode AND Branch_Code = @BranchCode AND CAST(BTDate AS DATE) = @settledate And BTCSEttled = 0", new { model.KSMBillNo, model.OltCode, model.CompanyCode, BranchCode = model.Branch_Code, settledate = model.SettledDate }, transaction);

                if (!string.IsNullOrWhiteSpace(CompanybillMode) && Convert.ToInt32(CompanybillMode) > 0 && model.PaymentStatus == "Company Transfer")
                {
                    var cmprowsAffected = await connection.ExecuteAsync(@"UPDATE BillTransferToCompany SET BillAmt = @Amount WHERE BillNo = @KSMBillNo AND Oltcode = @OltCode AND CompanyCode = @CompanyCode AND Branch_Code = @BranchCode AND CAST(BTDate AS DATE) = @settledate And BTCSEttled = 0", new { Amount = GrandTotal, model.KSMBillNo, model.OltCode, model.CompanyCode, BranchCode = model.Branch_Code, settledate = model.SettledDate }, transaction);

                    if (cmprowsAffected == 0)
                    {
                        return new PosModifyBillSettlementResponse
                        {
                            Success = false,
                            DifferenceAmount = 0,
                            Message = $"Failed to update Company Payment Bill '{GrandTotal}'.",
                            Data = model
                        };
                    }
                }

                if (string.IsNullOrWhiteSpace(adjustableMode) && string.IsNullOrWhiteSpace(CompanybillMode) && model.PaymentStatus == "Company Transfer")
                {
                    return new PosModifyBillSettlementResponse
                    {
                        Success = false,
                        DifferenceAmount = 0,
                        Message = $"No payment records found for this bill.",
                        Data = model
                    };
                }

                if (!string.IsNullOrWhiteSpace(adjustableMode) && Convert.ToInt32(CompanybillMode) == 0 && model.PaymentStatus != "Company Transfer")
                {

                    // Sum all other payment modes
                    var otherPayments = await connection.QuerySingleAsync<decimal>(@"SELECT ISNULL(SUM(KSMBillAmount), 0) FROM KOTBillSettlement WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @BranchCode AND CAST(KBSSetteleDate AS DATE) = @settledate AND KBSPaymentMode <> @PaymentMode", new { model.KSMId, model.OltCode, BranchCode = model.Branch_Code, settledate = model.SettledDate, PaymentMode = adjustableMode }, transaction);

                    var adjustedAmount = Math.Abs(GrandTotal - otherPayments);

                    // Prevent negative payment amount
                    if (adjustedAmount < 0)
                    {
                        return new PosModifyBillSettlementResponse
                        {
                            Success = false,
                            DifferenceAmount = 0,
                            Message = (
                            $"Cannot reduce bill total to {GrandTotal}. " +
                            $"Other payment modes already total {Math.Abs(otherPayments)}. " +
                            $"Please adjust the payment distribution first."),
                            Data = model
                        };
                        //throw new InvalidOperationException(
                        //    $"Cannot reduce bill total to {GrandTotal}. " +
                        //    $"Other payment modes already total {Math.Abs(otherPayments)}. " +
                        //    $"Please adjust the payment distribution first.");
                    }

                    // Update the selected payment mode
                    var rowsAffected = await connection.ExecuteAsync(@"UPDATE KOTBillSettlement SET KSMBillAmount = @Amount, KBSDiscount = @Discount WHERE KSMId = @KSMId AND OltCode = @OltCode AND Branch_Code = @BranchCode AND CAST(KBSSetteleDate AS DATE) = @settledate AND KBSPaymentMode = @PaymentMode", new { Amount = adjustedAmount, Discount = CurrentBillDiscount, model.KSMId, model.OltCode, BranchCode = model.Branch_Code, settledate = model.SettledDate, PaymentMode = adjustableMode }, transaction);

                    if (rowsAffected == 0)
                    {
                        return new PosModifyBillSettlementResponse
                        {
                            Success = false,
                            DifferenceAmount = 0,
                            Message = $"Failed to update payment mode '{adjustableMode}'.",
                            Data = model
                        };
                    }
                }

                // Step 2
                const string updateqry2 = "UPDATE KOTSettlementMaster  " +
                    " SET KSMBillAmount = @ksmbillamount, KSMBillTaxAmt = @ksmbilltaxamt, KSMBillDiscount = @ksmbilldiscount, famt = @roundOff " +
                    " WHERE KSMId = @ksmid AND OltCode = @oltcode AND Branch_Code = @branch_code AND CAST(KSMBillDate AS DATE) = @settledate";
                await connection.ExecuteAsync(updateqry2, new { ksmid = model.KSMId, oltcode = model.OltCode, branch_code = model.Branch_Code, ksmbillamount = CurrentBillAmount, ksmbilltaxamt = CurrentBillTaxAmt, ksmbilldiscount = CurrentBillDiscount, roundoff = roundOff, settledate = model.SettledDate, }, transaction);

                // Step 3
                if (model.foods != null)
                {
                    //string taxText = string.Join(" + ", model.Taxdetails.TaxList.Select(x => x.TaxName));

                    //decimal cgst = 0;
                    //decimal sgst = 0;

                    //if (!string.Equals(taxText, "No Tax", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var taxes = taxText.Split('+', StringSplitOptions.TrimEntries);

                    //    foreach (var tax in taxes)
                    //    {
                    //        decimal value = decimal.TryParse(
                    //            System.Text.RegularExpressions.Regex.Match(tax, @"\d+(\.\d+)?").Value,
                    //            out var result)
                    //            ? result
                    //            : 0;

                    //        if (tax.Contains("CGST", StringComparison.OrdinalIgnoreCase))
                    //            cgst = value;
                    //        else if (tax.Contains("SGST", StringComparison.OrdinalIgnoreCase))
                    //            sgst = value;
                    //    }
                    //}

                    //decimal itemAmount = food.Qty * food.Price;

                    //decimal taxAmount = (itemAmount * cgst / 100);

                    var deleteRows = await connection.ExecuteAsync
                        (@"DELETE FROM KOTDetails WHERE KOTId = @kotid AND ItemCode = @itemcode AND Branch_Code = @branch_code",
                        model.foods.Select(x => new { kotid = x.KotId, itemcode = x.ItemCode, branch_code = model.Branch_Code }), transaction);

                    foreach (var food in model.foods)
                    {
                        //var taxModelList = new List<IndiTaxModel>();

                        var taxmodel = await GetExtraCharges(food.ItemCode.ToString(), model.Branch_Code, model.OltCode);
                        if (taxmodel == null)
                        {
                            var taxModel = new IndiTaxModel();
                            taxModel.TaxAmount = 0;
                            taxModel.TaxCode = "0";

                            //taxModelList.Add(taxModel);
                        }
                        else
                        {
                            foreach (var item in taxmodel)
                            {
                                var taxModel = new IndiTaxModel();
                                taxModel.TaxAmount = ((food.Qty * Convert.ToDouble(food.Price)) * item.TaxPercentage) / 100;
                                taxModel.TaxCode = item.TaxCode;
                                taxModel.ItemCode = food.ItemCode;

                                const string updatesales = @"UPDATE SalesTax SET TaxAmount = @Amount WHERE Bill_No = @billno AND oltcode = @oltcode AND ItemCode = @itemcode AND Branch_Code = @BranchCode AND billdate = @settledate";
                                var rowsAffected1 = await connection.ExecuteAsync(updatesales, new { Amount = taxModel.TaxAmount, billno = model.KSMBillNo, oltcode = model.OltCode, itemcode = food.ItemCode, BranchCode = model.Branch_Code, settledate = model.SettledDate }, transaction);
                                //taxModelList.Add(taxModel);
                            }
                        }

                        const string insertqry = @"INSERT INTO KOTDetails (KOTId, KOTNO, ItemCode, KOTDRate, KOTDQty, Branch_Code, FinCode )
                        VALUES (@kotid, @kotno, @itemcode, @rate, @oty, @Branch, @fincode)";
                        await connection.ExecuteAsync(insertqry, new { kotid = food.KotId, kotno = food.KotId, itemcode = food.ItemCode, rate = food.Price, oty = food.Qty, Branch = model.Branch_Code, fincode = FinCode }, transaction);

                    }
                }

                // Step 4

                var PreviousKotTotal = await connection.QuerySingleAsync<decimal>(@"SELECT KOTTotal FROM KOTMaster WHERE KOTId = @kotid AND OltCode = @oltcode AND Branch_Code = @branch AND CAST(KOTDate AS DATE) = @settledate", new { kotid = model.KOTId, oltcode = model.OltCode, branch = model.Branch_Code, settledate = model.SettledDate }, transaction);

                decimal newKotTotal = model.foods.Where(x => x.KotId == model.KOTId).Sum(x => x.Price * x.Qty);

                const string updatetqry3 = "UPDATE KOTMaster SET KOTTotal = @totalamount WHERE KOTId = @kotid AND OltCode = @oltcode AND Branch_Code = @branch AND CAST(KOTDate AS DATE) = @settledate";
                await connection.ExecuteAsync(updatetqry3, new { kotid = model.KOTId, totalamount = Math.Abs(newKotTotal), oltcode = model.OltCode, branch = model.Branch_Code, settledate = model.SettledDate }, transaction);
                transaction.Commit();

                // Step 5

                const string updatetax = @"UPDATE BillTax SET TotalAmount = @Amount, TotalQty = @totalQty, CGSTAmt = @cgstAmt, SGSTAmt = @sgstAmt, GrandTotal = @grandtotal, DiscountPer = @discountper, Discount = @discount, DiscountIn = @discountIn, RoundOff = @roundOff WHERE BillId = @billid AND OltCode = @oltcode AND BranchCode = @branch AND BillDate = @settledate";
                var rowsAffected2 = await connection.ExecuteAsync(updatetax,
                    new
                    {
                        Amount = model.Taxdetails.TotalAmount,
                        totalQty = model.Taxdetails.TotalQty,
                        cgstAmt = model.Taxdetails.CGSTAmt,
                        sgstAmt = model.Taxdetails.SGSTAmt,
                        grandtotal = model.Taxdetails.GrandTotal,
                        discountper = model.Taxdetails.DiscountPer,
                        discount = model.Taxdetails.Discount,
                        discountIn = model.Taxdetails.DiscountIn,
                        roundOff = model.Taxdetails.RoundOff,
                        billid = model.KSMBillNo,
                        oltcode = model.OltCode,
                        branch = model.Branch_Code,
                        settledate = model.SettledDate
                    }, transaction);

                var updatetaxdetails = @"UPDATE BillTaxDetails SET TaxableAmount = @Amount, TaxAmount = @taxamount, Total = @total, CGST = @cgst, SGST = @sgst WHERE BillId = @billid AND OltCode = @oltcode AND BranchCode = @branch AND BillDate = @settledate";
                var rowsAffected3 = await connection.ExecuteAsync(updatetaxdetails,
                    new
                    {
                        Amount = model.Taxdetails.TaxList.Sum(x => x.TaxableAmount),
                        taxamount = model.Taxdetails.TaxList.Sum(x => x.TaxAmount),
                        total = model.Taxdetails.TaxList.Sum(x => x.Total),
                        cgst = model.Taxdetails.TaxList.Sum(x => x.CGST),
                        sgst = model.Taxdetails.TaxList.Sum(x => x.SGST),
                        billid = model.KSMBillNo,
                        oltcode = model.OltCode,
                        branch = model.Branch_Code,
                        settledate = model.SettledDate
                    }, transaction);

                // Step 6

                decimal difference = Math.Abs(previousbill - currentbill);

                string message;

                if (currentbill > previousbill)
                {
                    message = $"Bill amount increased by {Math.Abs(difference)}. Please Collect From Customer";
                }
                else if (currentbill < previousbill)
                {
                    message = $"Bill amount reduced by {Math.Abs(difference)}. Please Refund To Customer";
                }
                else
                {
                    message = "No change in bill amount";
                }

                return new PosModifyBillSettlementResponse
                {
                    Success = true,
                    DifferenceAmount = difference,
                    Message = message,
                    Data = model
                };
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new PosModifyBillSettlementResponse
                {
                    Success = false,
                    DifferenceAmount = 0,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        #endregion

        #region Bill Adjustment

        public async Task<List<BillAdjustmentModel>> GetAdjustmentLoadData(BillAdjustmentRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder();

            sql.Append(@"
            SELECT
                KSM.KSMBillNo AS BillNo,
                KSM.KSMBillDate AS FromDate,
                KSM.KSMBillDate AS ToDate,
                KSM.KSMBillAmount AS BillAmount,
                KBS.KBSPaymentMode AS PaymentMode,
                KSM.DKSMBillNo AS OriginalBillNo
            FROM KotSettlementmaster KSM
            INNER JOIN KOTBillSettlement KBS
                ON KSM.KSMBillNo = KBS.KSMBillNo
                AND KSM.OltCode = KBS.OltCode
                AND KSM.KSMBillDate = KBS.KBSSetteleDate
            WHERE
                KSM.AccountType='F'
                AND KBS.AccountType='F'
                AND KSM.Branch_Code=@branchcode
                AND KSM.OltCode=@oltcode
                AND KBS.Branch_Code = KSM.Branch_Code
                AND KSM.KSMBillDate >= @fromdate AND KSM.KSMBillDate <= @todate 
            ");

            if (request.PaymentMode.ToLower() == "cash")
            {
                sql.Append(" AND KBS.KBSPaymentMode = 'Cash' ");
            }

            if (request.ExcludedBills.Any())
            {
                sql.Append(" AND KSM.KSMBillNo NOT IN @excludedbills ");
            }

            sql.Append(" ORDER BY CONVERT(INT,KSM.KSMBillNo)");

            var result = await connection.QueryAsync<BillAdjustmentModel>(
                sql.ToString(),
                new
                {
                    branchcode = request.BranchCode,
                    oltcode = request.OltCode,
                    fromdate = request.FromDate.Date,
                    todate = request.ToDate.Date,
                    excludedbills = request.ExcludedBills
                });

            return result.ToList();
        }

        public async Task<List<BillAdjustmentSettlementModel>> GetBillSettlementAsync(BillAdjustmentRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                string partitionColumn = request.RankByType.ToLower() == "ksmid" ? "KSM.KSMId" : "KM.KOTNO";

                //-----------------------------------------
                // Delete Temporary Table
                //-----------------------------------------

                await connection.ExecuteAsync("DELETE FROM Tmp_BillSettlement Where Branchcode = @branchcode", new { branchcode = request.BranchCode }, transaction: transaction);

                //-----------------------------------------
                // Main Query
                //-----------------------------------------
                var sql = new StringBuilder();

                sql.Append ( $@"
                    SELECT KSM.KSMBillNo as BillNo, KD.KId, KD.KOTId, KD.KOTDRate as KotRate, KD.ItemCode, KOTDQty as Qty, 0 TaxPercentage, KSM.KSMId, 
                    KD.KOTNo, 0 TaxCode, IM.ItemName, IM.GrpCode, IG.GrpName, (KD.KOTDRate*KOTDQty) as Amount, KOTDate as BillDate, KM.branch_code as BranchCode,
                     ROW_NUMBER() OVER ( PARTITION BY {partitionColumn} ORDER BY KD.KOTDRate ASC ) AS RankId
                    FROM KOTMaster KM
                    INNER JOIN KOTDetails KD ON KM.KOTNo = KD.KOTNo And KD.branch_code = km.branch_code
                    INNER JOIN KOTSettlementDetails KSD ON KSD.KOTId = KD.KOTId And KSD.Branch_Code = KM.branch_code
                    INNER JOIN KOTSettlementMaster KSM ON KSM.KSMId = KSD.KSMId And KSM.Branch_Code = km.branch_code
                    INNER JOIN KOTBillSettlement KBS ON KBS.KSMId = KSM.KSMId AND KBS.OltCode = KSM.OltCode And KBS.Branch_Code = KM.branch_code
                    INNER JOIN ItemMaster IM ON IM.ItemCode = KD.ItemCode And IM.branch_code = KM.branch_code
                    INNER JOIN ItemGroup IG ON IG.GrpCode = IM.GrpCode And IG.Branch_Code = km.branch_code ");

                sql.Append( $@" WHERE KSM.KSMBillDate >= @fromdate AND KSM.KSMBillDate <= @todate  
                    AND BillCancelled = 0
                    AND KM.OltCode = @OltCode
                    AND KSM.Branch_Code = @BranchCode
                    AND KSM.KSMBillNo NOT IN @ExcludedBills ");

                //if (request.PaymentMode.ToLower() == "cash")
                //{
                //    sql.Append(" AND KBS.KBSPaymentMode = 'Cash' ");
                //}

                //sql.Append(@" AND KSM.KSMBillNo NOT IN ( SELECT KSMBillNo FROM KOTBillSettlement WHERE KBSSetteleDate >= @fromdate AND KBSSetteleDate <= @todate AND KBSPaymentMode <> 'Cash' ) ");

                if (request.PaymentMode.Equals("cash", StringComparison.OrdinalIgnoreCase))
                {
                    sql.Append(@" AND KBS.KBSPaymentMode = 'Cash' AND KSM.KSMBillNo NOT IN ( SELECT KSMBillNo FROM KOTBillSettlement WHERE Branch_Code = @BranchCode AND CAST(KBSSetteleDate AS DATE) >= @fromdate AND CAST(KBSSetteleDate AS DATE) <= @todate AND KBSPaymentMode <> 'Cash' ) ");
                }
                
                sql.Append ($@" ORDER BY KSM.KSMId, CONVERT(INT,KSM.KSMBillNo) ");

                var itemdetails = (await connection.QueryAsync<BillAdjustmentSettlementModel>(
                    sql.ToString(),
                    new
                    {
                        branchcode = request.BranchCode,
                        oltcode = request.OltCode,
                        fromdate = request.FromDate.Date,
                        todate = request.ToDate.Date,
                        paymentmode = request.PaymentMode,
                        excludedbills = request.ExcludedBills.Any() ? request.ExcludedBills : new List<string> { "0" }
                    },
                    transaction)).ToList();

                //-----------------------------------------
                // Insert Temp Table
                //-----------------------------------------

                var insertSql = @"

                    INSERT INTO Tmp_BillSettlement ( BillNo, KSMId, Kid, KotId, KotRate, Code, Qty, Amount, TaxPerc, Chk, KOTNO, TaxCode, ItemName, GrpCode, RankId, Branchcode, BillDate, GrpName )

                    VALUES  ( @billno, @ksmid, @kid, @kotid, @kotrate, @itemcode, @qty, @amount, @taxper, 'N', @kotno, @taxcode, @itemname, @grpcode, @rankid, @branchcode, @billdate, @grpname )";

                foreach (var item in itemdetails)
                {
                    if (item.GrpCode >= 2)
                        item.RankId = 1;

                    await connection.ExecuteAsync(
                        insertSql,
                        new
                        {
                            billno = item.BillNo,
                            ksmid = item.KSMId,
                            kid = item.KId,
                            kotid = item.KOTId,
                            kotrate = item.KotRate,
                            itemcode = item.ItemCode,
                            qty = item.Qty,
                            amount = item.Amount,
                            taxper = item.TaxPercentage,
                            kotno = item.KOTNo,
                            taxcode = item.TaxCode,
                            itemname = item.ItemName,
                            grpcode = item.GrpCode,
                            rankid = item.RankId,
                            branchcode = request.BranchCode,
                            billdate = item.BillDate.Date,
                            grpname = item.GrpName
                        },
                        transaction);
                }

                var selectSql = (@"
                Select BillNo, KSMId, Kid, KotId, KotRate, Code as ItemCode, Qty, Amount, TaxPerc, Chk, KOTNO, TaxCode, ItemName, GrpCode, GrpName, RankId, Branchcode, BillDate From Tmp_BillSettlement Where Branchcode = @branchcode ");

                var Tempdetails = (await connection.QueryAsync<BillAdjustmentSettlementModel>(selectSql.ToString(), new { branchcode = request.BranchCode }, transaction: transaction)).ToList();

                transaction.Commit();

                return Tempdetails;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }


        public async Task<decimal> GetEstimatedAmount(int rankId, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @" SELECT ISNULL(SUM(CONVERT(decimal(18,2), Amount)),0) FROM Tmp_BillSettlement WHERE RankId >= @RankId AND Branchcode = @BranchCode";

            var amount = await connection.QueryFirstOrDefaultAsync<decimal>( sql, new { RankId = rankId, BranchCode = branchCode });

            return amount;
        }

        public async Task<List<BillAdjustmentSettlementModel>> GetBiddingItemsAsync(int rankId, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Update Reseted ranks

                var resetupdateSql = @" UPDATE Tmp_BillSettlement SET Chk='N' WHERE Branchcode = @BranchCode";

                await connection.ExecuteAsync( resetupdateSql, new { BranchCode = branchCode }, transaction);

                // Update selected ranks

                var updateSql = @" UPDATE Tmp_BillSettlement SET Chk='Y' WHERE RankId >= @RankId AND Branchcode = @BranchCode";

                await connection.ExecuteAsync( updateSql, new { RankId = rankId, BranchCode = branchCode }, transaction);

                // Get items
                var selectSql = (@"
                Select BillNo, KSMId, Kid, KotId, KotRate, Code as ItemCode, Qty, Amount, TaxPerc, Chk, KOTNO, TaxCode, ItemName, GrpCode, GrpName, RankId, Branchcode, BillDate From Tmp_BillSettlement Where Branchcode = @BranchCode");

                var items = (await connection.QueryAsync<BillAdjustmentSettlementModel>( selectSql, new { BranchCode = branchCode }, transaction: transaction )).ToList();

                transaction.Commit();

                return items;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> ResetKOTMasterValuesAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {

            var updatesql = @" UPDATE KOTMASTER SET KOTTotal = 0 WHERE KOTId IN ( SELECT KOTId FROM KOTSettlementDetails WHERE KSMID IN ( SELECT DISTINCT KSMId FROM Tmp_BillSettlement Where Branchcode = @branchcode ) ) AND OltCode = @oltcode And branch_code = @branchcode ";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);
            return affectedrow > 0;
        }

        public async Task<bool> ResetKOTSettlementMasterValuesAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {
            var updatesql = @" UPDATE KOTSettlementMaster SET KSMBillAmount = 0, KSMBillTaxAmt = 0, KSMServiceTaxAmt = 0, KSMServiceCharge = 0 
                WHERE KSMId IN ( SELECT DISTINCT KSMId FROM Tmp_BillSettlement Where Branchcode = @branchcode ) AND OltCode = @oltcode And Branch_Code = @branchcode";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);
            return affectedrow > 0;
        }   

        public async Task<bool> ResetKOTBillSettlementAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {
            var updatesql = @" UPDATE KOTBillSettlement SET KSMBillAmount = 0 WHERE KSMId IN ( SELECT DISTINCT KSMId FROM Tmp_BillSettlement Where Branchcode = @branchcode ) 
                AND OltCode = @oltcode And Branch_Code = @branchcode";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);
            return affectedrow > 0;
        }

        public async Task<bool> DeleteSalesTaxAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {
            var Deletesql1 = @" DELETE FROM SalesTax WHERE Bill_No IN ( SELECT DISTINCT BillNo FROM Tmp_BillSettlement Where Branchcode = @branchcode )
                AND OltCode = @oltcode And Branch_Code = @branchcode";

            var affectedrow1 = await connection.ExecuteAsync(Deletesql1, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);

            //var Deletesql2 = @" DELETE FROM BillTax WHERE BillId IN ( SELECT DISTINCT BillNo FROM Tmp_BillSettlement Where Branchcode = @branchcode )
            //    AND OltCode = @oltcode And BranchCode = @branchcode";

            //var affectedrow2 = await connection.ExecuteAsync(Deletesql2, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);

            //var Deletesql3 = @" DELETE FROM BillTaxDetails WHERE BillId IN ( SELECT DISTINCT BillNo FROM Tmp_BillSettlement Where Branchcode = @branchcode )
            //    AND OltCode = @oltcode And BranchCode = @branchcode";

            //var affectedrow3 = await connection.ExecuteAsync(Deletesql3, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);
            
            return affectedrow1 > 0 ;
        }

        public async Task<OutletSettingsModel> GetOutletSettingsAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {

            var selectsql = @" SELECT TaxCode, ServiceCharge FROM OutletMaster WHERE OltCode = @oltcode And branch_code = @branchcode";

            var result = await connection.QueryFirstOrDefaultAsync<OutletSettingsModel>(selectsql, new { oltcode = model.OltCode, branchcode = model.BranchCode }, transaction);

            return result;
        }

        public async Task<IEnumerable<BillAdjustmentSettlementModel>> GetTmpBillSettlementAsync(SaveBidRequest model, IDbConnection connection, IDbTransaction transaction)
        {
            var selectsql = @" Select BillNo, KSMId, Kid, KotId, KotRate, Code as ItemCode, Qty, Amount, TaxPerc, Chk, KOTNO, TaxCode, ItemName, GrpCode, GrpName, RankId, Branchcode, BillDate From Tmp_BillSettlement Where Branchcode = @branchcode";

            var result = await connection.QueryAsync<BillAdjustmentSettlementModel>(selectsql, new { branchcode = model.BranchCode }, transaction);

            return result;
        }

        public async Task<bool> UpdateAdjustmentKotMasterAsync(string kotId, decimal amount, DateTime billDate, string outletCode, string branchCode, IDbConnection connection, IDbTransaction transaction)
        {

            var updatesql = @" UPDATE KOTMASTER SET KOTTotal = @Amount WHERE KOTId = @KOTId AND CAST(KOTDATE AS DATE) = @BillDate AND OltCode = @OltCode AND branch_code = @branchcode";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = outletCode, branchcode = branchCode, Amount = amount, KOTId = kotId, BillDate = billDate.Date }, transaction);

            return affectedrow > 0;
        }

        public async Task<bool> UpdateAdjustmentKotSettlementMasterAsync(KotSettlementMasterUpdateModel model, IDbConnection connection, IDbTransaction transaction)
        {
            var updatesql = @" UPDATE KOTSettlementMaster SET KSMBillAmount = @amount, KSMBillTaxAmt = @taxamount,  KSMServiceTaxAmt = 0, KSMServiceCharge = @servicecharge, KSMBillDiscount = 0 WHERE KSMBillNo = @billno AND KSMId = @ksmid AND OltCode = @oltcode AND Branch_Code = @branchcode AND CAST(KSMBillDate AS DATE) = @BillDate";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = model.OltCode, branchcode = model.BranchCode, amount = model.BillAmount, taxamount = model.TaxAmount, servicecharge = model.ServiceCharge , BillDate = model.BillDate.Date, billno = model.BillNo, ksmid = model.KSMId }, transaction);

            return affectedrow > 0;
        }

        public async Task<bool> UpdateAdjustmentKotBillSettlementAsync(string BillNo, string KSMId, decimal Amount, string OltCode, DateTime Billdate, string Branchcode, IDbConnection connection, IDbTransaction transaction)
        {
            var updatesql = @" update KOTBillSettlement set KSMBillAmount = @amount where KSMBillNo = @billno and KSMId = @ksmid and oltcode = @oltcode and CAST(KBSSetteleDate AS DATE) = @billdate and Branch_Code = @branchcode";

            var affectedrow = await connection.ExecuteAsync(updatesql, new { oltcode = OltCode, branchcode = Branchcode, amount = Amount, billdate = Billdate.Date, billno = BillNo, ksmid = KSMId }, transaction);

            return affectedrow > 0;
        }

        public async Task<List<ExtraChargeModel>> GetItemTaxesAsync(string itemCode, string outletCode, string branchCode, IDbConnection connection, IDbTransaction transaction)
        {

            var selectsql = @"SELECT T.TaxCode, T.TaxPercentage as TaxPercentage, T.TaxDescription FROM ExtraCharges E 
                INNER JOIN BillTaxDescription T ON E.ChargeCode = T.TaxCode And T.BranchCode = E.Branch_Code 
                WHERE E.ItemCode = @ItemCode AND T.BranchCode = @BranchCode AND OltCode=@oltcode";

            var result = (await connection.QueryAsync<ExtraChargeModel>(selectsql, new { ItemCode = itemCode, oltcode = outletCode, branchcode = branchCode }, transaction)).ToList();

            return result;
        }

        public async Task<bool> UpdateAdjustmentBillTaxAsync(AdjustmentSalesTaxModel model, IDbConnection connection, IDbTransaction transaction)
        {
            const string updatetax = @"UPDATE BillTax SET TotalAmount = @Amount, TotalQty = @totalQty, CGSTAmt = @cgstAmt, SGSTAmt = @sgstAmt, GrandTotal = @grandtotal, RoundOff = @roundOff WHERE BillId = @billid AND OltCode = @oltcode AND BranchCode = @branch AND CAST(BillDate AS DATE) = @settledate";

            var rowsAffected2 = await connection.ExecuteAsync(updatetax,
                new
                {
                    Amount = model.Total,
                    totalQty = model.TotalQty,
                    cgstAmt = model.CGST,
                    sgstAmt = model.SGST,
                    grandtotal = model.Total,
                    roundOff = model.RoundOff,
                    billid = model.KSMBillNo,
                    oltcode = model.OltCode,
                    branch = model.BranchCode,
                    settledate = model.SettledDate
                }, transaction);

            var updatetaxdetails = @"UPDATE BillTaxDetails SET TaxableAmount = @Amount, TaxAmount = @taxamount, Total = @total, CGST = @cgst, SGST = @sgst WHERE BillId = @billid AND OltCode = @oltcode AND BranchCode = @branch AND CAST(BillDate AS DATE) = @settledate";

            var rowsAffected3 = await connection.ExecuteAsync(updatetaxdetails,
                new
                {
                    Amount = model.TaxableAmount,
                    taxamount = model.TaxAmount,
                    total = model.Total,
                    cgst = model.CGST,
                    sgst = model.SGST,
                    billid = model.KSMBillNo,
                    oltcode = model.OltCode,
                    branch = model.BranchCode,
                    settledate = model.SettledDate
                }, transaction);

            return rowsAffected2 > 0 || rowsAffected3 > 0;
        }

        public async Task<bool> InsertSalesTaxAsync(SalesTax model, IDbConnection connection, IDbTransaction transaction)
        {
            string insertquery = @"INSERT INTO SalesTax
                   (Bill_No, ItemCode, TaxCode, TaxAmount, Branch_Code, OltCode, BillDate, Ref, IsOnline, FinCode, TaxType)
                   VALUES(@Bill_No, @ItemCode, @TaxCode, @TaxAmount, @Branch_Code, @OltCode, @BillDate, @Ref, @IsOnline, @FinCode, @TaxType)";

            var rowaffected = await connection.ExecuteAsync(insertquery, model, transaction);

            return rowaffected > 0;
        }

        public async Task<bool> DeleteSaleTaxAsync(string BillNo, string ItemCode, string OltCode, string Branchcode, IDbConnection connection, IDbTransaction transaction)
        {
            var Deletesql1 = @" DELETE FROM SalesTax WHERE Bill_No = @billno And ItemCode = @itemcode AND OltCode = @oltcode And Branch_Code = @branchcode";

            var rowsAffected = await connection.ExecuteAsync(Deletesql1, new { billno = BillNo, itemcode = ItemCode, oltcode = OltCode, branchcode = Branchcode }, transaction);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteKotDetailsAsync(string KOTNo, string ItemCode, string Kid, string Branchcode, IDbConnection connection, IDbTransaction transaction)
        {
            string Deletesql = @" DELETE FROM KOTDetails WHERE KOTNo = @kotno AND ItemCode = @itemcode AND Kid = @kid AND branch_code = @branchcode";

            var rowaffected = await connection.ExecuteAsync(Deletesql, new {kotno = KOTNo, itemcode = ItemCode, kid = Kid, branchcode = Branchcode}, transaction);

            return rowaffected > 0;
        }

        public async Task<bool> DeleteKotSettlementDetailsAsync(string KotId, string KSMId, string Branchcode, IDbConnection connection, IDbTransaction transaction)
        {

            string Deletesql = @"DELETE FROM KOTSettlementDetails WHERE KOTId = @kotId AND KSMId = @ksmId AND Branch_Code = @branchcode";

            var rowaffected = await connection.ExecuteAsync(Deletesql, new { kotId = KotId, ksmId = KSMId, branchcode = Branchcode }, transaction);

            return rowaffected > 0;
        }

        public async Task<bool> UpdateFinalBillAmountAsync(DateTime FromDate, DateTime ToDate, string Branchcode, IDbConnection connection, IDbTransaction transaction)
        {
            var updatesql1 = @" UPDATE KOTSettlementMaster SET famt = ROUND( ROUND( KSMBillAmount + KSMBillTaxAmt + KSMBillDiscount + KSMServiceCharge,0 ) - ROUND( KSMBillAmount + KSMBillTaxAmt + KSMBillDiscount + KSMServiceCharge,2 ), 2)  
              WHERE CAST(KSMBillDate AS DATE) BETWEEN @FromDate AND @ToDate AND Branch_Code = @branchcode AND KSMBillNo IN ( SELECT DISTINCT BillNo FROM Tmp_BillSettlement Where Branchcode = @branchcode ) ";

            var affectedrow1 = await connection.ExecuteAsync(updatesql1, new { fromdate = FromDate.Date, todate = ToDate.Date, branchcode = Branchcode }, transaction);

            var updatesql2 = @" UPDATE KOTBillSettlement SET KSMBillAmount=A.BillAmt  FROM ( SELECT ROUND( KSMBillAmount + KSMBillTaxAmt - KSMBillDiscount + KSMServiceCharge + famt,0) BillAmt,  KSMBillNo, KSMId  FROM KOTSettlementMaster  
              WHERE KSMBillNo IN ( SELECT DISTINCT BillNo FROM Tmp_BillSettlement Where Branchcode = @branchcode ) ) A  WHERE A.KSMBillNo = KOTBillSettlement.KSMBillNo  AND A.KSMId = KOTBillSettlement.KSMId AND Branch_Code = @branchcode ";

            var affectedrow2 = await connection.ExecuteAsync(updatesql2, new { fromdate = FromDate.Date, todate = ToDate.Date, branchcode = Branchcode }, transaction);

            return affectedrow1 > 0 && affectedrow2 > 0;
        }


        #endregion

        #region POS Room Service

        public async Task<IEnumerable<RoomTableStatusModel>> GetTableListForRoomService()
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            var selectquery = " select RD.RoomNo, RD.RoomCode, RD.PAX as Pax, CM.GPlane as PlanId, GM.GuestName, GM.GuestCode, CM.CheckInNo \r\n" +
                " From Tbl_Room_Details RD \r\n" +
                " INNER join Tbl_CheckIn_Master CM on RD.CurrentCheckIn = CM.CheckInNo \r\n" +
                " INNER join Tbl_Guest_Master GM on CM.CustomerCode = GM.GuestCode \r\n" +
                " where Prioritys = 'P' AND(Status = 'O' or status = 'Q' or Status = 'F') Order by RoomNo\r\n";

            return await connection.QueryAsync<RoomTableStatusModel>(selectquery);
        }

        public async Task<IEnumerable<string>> GetKOTMasterListForRoomService(int oltcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = " Select Distinct KOTTblNo from KOTMaster Where KOTCancelled <> 1 and KOTSettled <> 1 and OltCode = @OltCode ";

            return await connection.QueryAsync<string>(selectquery, new { OltCode = oltcode });
        }

        public async Task<List<TableStatusModel>> GetTables(int outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);
            connection.Open();

            var status = new List<TableStatusModel>();

            var occupiedQuery = @"SELECT DISTINCT km.KOTTblNo, km.KOTChargeable FROM ItemMaster im
                INNER JOIN KOTDetails kd ON im.ItemCode = kd.ItemCode
                INNER JOIN KOTMaster km ON km.KOTId = kd.KOTId
                WHERE km.KOTSettled = 0 AND km.KOTCancelled = 0 AND km.StwCode <> '0' AND km.KOTtotal > 0 AND km.OltCode = @Outlet";

            var allTablesQuery = @" SELECT TblNo FROM TableMaster WHERE OltCode = @Outlet";

            var occupiedTables = await connection.QueryAsync<dynamic>(occupiedQuery, new { Outlet = outlet });

            var allTables = await connection.QueryAsync<string>(allTablesQuery, new { Outlet = outlet });

            foreach (var table in allTables)
            {
                var occ = occupiedTables.FirstOrDefault(x => x.KOTTblNo == table);

                status.Add(new TableStatusModel
                {
                    TableNo = table,
                    Status = occ != null ? "Occupied" : "Free",
                    NC = occ != null && occ.KOTChargeable == 1
                });
            }

            return status;
        }

        public async Task<(int OltCode, bool IsRoomService)> POSRoomServiceAsync(string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @" SELECT OltCode, OltIsRoomService FROM OutletMaster WHERE Branch_Code = @Branch";

            var result = await connection.QueryFirstOrDefaultAsync<(int OltCode, bool IsRoomService)>(sql, new { Branch = branch });

            return result;
        }

        public async Task<bool> IsRoomCheckinareNot(string RoomNo, int BillNo)
        {
            using var connection = _factory.CreateConnection(DbNames.HMS);

            //const string sql = @" select RD.RoomNo, RD.RoomCode, RD.PAX as Pax, CM.GPlane as PlanId, CM.CheckInNo
            //     From Tbl_Room_Details RD
            //     INNER join Tbl_CheckIn_Master CM on RD.CurrentCheckIn = CM.CheckInNo
            //     INNER join Tbl_FoodBills FB on FB.CheckInNo = RD.CurrentCheckIn 
            //     Where RD.Status = 'O' And RD.RoomNo = @roomno And RcptNo = @billno ";

            const string sql = @" SELECT CAST( CASE WHEN EXISTS ( SELECT 1 FROM Tbl_Room_Details RD 
                    INNER JOIN Tbl_CheckIn_Master CM ON CM.CheckInNo = RD.CurrentCheckIn
                    INNER JOIN Tbl_FoodBills FB ON FB.CheckInNo = CM.CheckInNo 
                    WHERE RD.Status = 'O' AND RD.RoomNo = @roomNo AND FB.RcptNo = @billNo ) 
                    THEN 1 ELSE 0 END AS BIT)";

            var result = await connection.ExecuteScalarAsync<bool>(sql, new { roomno = RoomNo, billno = BillNo });

            return result;
        }

        #endregion


        #region Unsettled KOT and Bill Details
        public async Task<IEnumerable<UnsettledKOTModel>> GetUnsettledKOTDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @"Select KOTId, KOTNo, OltCode, KOTTblNo, StwCode, KOTDate, KOTTime, KOTChargeable, KOTTotal, CheckinNo, KOTGuestName, 
                    KOTCancelled, KOTSettled, SubTable, GuestCode, branch_code
                    FROM KOTMaster
                    Where Convert(DATE,KOTDate) >= @FromDate And Convert(DATE,KOTDate) <= @ToDate
                    And KOTCancelled = 0 And KOTSettled = 0 And branch_code = @Branch ";

            var result = await connection.QueryAsync<UnsettledKOTModel>(sql, new { FromDate = fromdate.Date, ToDate = todate.Date, Branch = Branchcode });

            return result;
        }

        public async Task<IEnumerable<UnsettledBillModel>> GetUnsettledBillDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = @"Select KSMId, OltCode, KSMBillNo, KSMBillDate, KSMBillTime, KSMBillAmount, KSMBillTaxAmt, KSMBillDiscount, 
                    KSMBillCancled, KSMBillSettled, KSMTblNo, KSMBillTransfered, KSMIsRoomService, BillCancelled, KSMSettledAmt, KSMServiceTaxAmt,
                    KSMBillNoofTime, KSMSUBTBLNO, STEWCODE , GuestCode, Branch_Code, ABS(KSMBillAmount + KSMBillTaxAmt) as GrandTotal
                    From KOTSettlementMaster
                    Where Convert(DATE,KSMBillDate) >= @FromDate And Convert(DATE,KSMBillDate) <= @ToDate
                    And KSMBillSettled = 0 And BillCancelled = 0 And Branch_Code = @Branch ";

            var result = await connection.QueryAsync<UnsettledBillModel>(sql, new { FromDate = fromdate.Date, ToDate = todate.Date, Branch = Branchcode });

            return result;
        }

        public async Task<int> UpdateUnsettledKOT(List<UpdateUnsettledKOTRequest> request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            int rowsAffected = 0;

            const string sql = @"Update KOTMaster Set KOTCancelled = 1 Where Convert(DATE,KOTDate) >= @kotdate And KOTSettled = 0 
                                And KOTId = @kotid And OltCode = @OltCode And branch_code = @branchCode ";

            foreach (var detail in request)
            {
                rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    kotdate = detail.KOTDate.Date,
                    kotid = detail.KOTId,
                    oltCode = detail.OltCode,
                    branchCode = detail.Branchcode
                });

                if (rowsAffected > 0)
                {
                    rowsAffected++;
                }
            }

            if (rowsAffected == 0)
            {
                return 0; // If any update fails, return false
            }
            else
            {
                return rowsAffected;
            }
        }

        #endregion
    }
}
