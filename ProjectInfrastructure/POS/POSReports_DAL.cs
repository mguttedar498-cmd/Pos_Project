using Azure.Core;
using ClosedXML.Graphics;
using Dapper;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.Emit;
using System.Text;
using System.Transactions;

namespace HMS_360_PMS.HMS_360_PMS.Infrastructure.POS
{
    public class POSReports_DAL : IPOSReports_Repository
    {
        private readonly DbConnectionFactory _factory;

        public POSReports_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<DailySalesModel>> GetDailySales(DateTime fromdate, DateTime todate, string outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet;
            var daily =
                " select st.KSMBillNo AS BillNo, st.KSMBillDate AS BillDate, TRY_CONVERT(TIME, st.KSMBillTime) AS BillTime, KSMTblNo AS TableNo,KSMBillAmount AS BillAmount," +
                " KSMBillDiscount AS Discount, KSMBillTaxAmt AS Tax, KSMBillTaxAmt / 2 AS CGST, KSMBillTaxAmt / 2 AS SGST, famt AS RoundOff," +
                " (KSMBillAmount + KSMBillTaxAmt + famt - KSMBillDiscount) AS Total, ot.OltName as OltName" +
                " from KOTSettlementMaster st " +
                " join OutletMaster ot on st.OltCode = ot.OltCode" +
                " where st.AccountType ='F' and st.BillCancelled = 0 and st.BillCancelled = 0 and st.KSMBillDate between @Fromdate and @Todate and (@Outlet IS NULL OR st.OltCode = @Outlet) " +
                " order by KSMBillDate,Convert(int,KSMBillNo)";

            return (await connection.QueryAsync<DailySalesModel>(daily, new { Fromdate = fromdate, Todate = todate, Outlet = olt })).ToList();
        }

        public async Task<List<ReportItemGroup>> GetItemGroups()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = "SELECT GrpCode, GrpName, UserCode , LastModify , Branch_Code, Isuploaded, Dep FROM ItemGroup";
            return (await connection.QueryAsync<ReportItemGroup>(sql)).ToList();
        }

        public async Task<List<ItemSalesModel>> GetItemSales(DateTime fromdate, DateTime todate, string outlet)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                //var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet;
                //var sql = @"
                //        SELECT GrpName AS GroupName, ItemName, itemrate AS Rate, 
                //        SUM(quantity) AS Qty, SUM(Amount) AS Total
                //        FROM View_ItemWiseSales IW
                //        JOIN ItemGroup ig ON iw.GrpCode = ig.GrpCode
                //        WHERE KOTDT >= @Fromdate 
                //        AND KOTDT < DATEADD(day, 1, @Todate)
                //        AND (@Outlet IS NULL OR IW.OltCode = @Outlet)
                //        GROUP BY GrpName, ItemName, itemrate";

                var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet.Trim();
                var sql = "SELECT iw.GrpCode as GroupCode, GrpName AS GroupName, ItemCode, ItemName, itemrate AS Rate," +
                    " SUM(quantity) AS Qty, SUM(Amount) AS Total,IW.OltName as OltName" +
                    " FROM View_ItemWiseSales IW " +
                    " JOIN ItemGroup ig ON iw.GrpCode = ig.GrpCode " +
                    " WHERE KOTDT >= @Fromdate " +
                    " AND KOTDT < DATEADD(day, 1, @Todate) ";
                if (olt != null && olt.Any())
                {
                    sql += " AND LTRIM(RTRIM(IW.OltCode)) IN ( SELECT value FROM STRING_SPLIT(@Outlet, ',') ) ";
                    //" AND (@Outlet IS NULL OR LTRIM(RTRIM(IW.OltCode)) = LTRIM(RTRIM(@Outlet))) " +
                }
                sql += " GROUP BY GrpName, ItemCode, ItemName, itemrate, OltName, iw.GrpCode";
                return (await connection.QueryAsync<ItemSalesModel>(sql, new { Fromdate = fromdate.Date, Todate = todate.Date, Outlet = olt })).ToList();
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ReportOutletMaster>> GetOutlets()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = "SELECT OltCode, POSCode, OltName, OltIsRoomService, OltServiceTaxRequired, OltAddress1, OltAddress2, TaxCode, UserCode, LastModify, ServiceCharge, branch_code , OltIsParcelService, isuploaded, ismodified, TinNo, SBCess, KKCess, InExTax, OltIsFastFood FROM OutletMaster";
            return (await connection.QueryAsync<ReportOutletMaster>(sql)).ToList();
        }

        public async Task<List<TableModel>> GetTables(string outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
               SELECT DISTINCT 
                tm.TblCode AS TableCode, tm.TblNo AS TableNo,
                CASE WHEN km.KOTSettled IS NULL THEN 0 ELSE 1 END AS Occupy,
                ISNULL(SUM(KOTTotal), 0) AS Amount, TblSeatCount AS Pax
               FROM TableMaster tm
               LEFT JOIN KOTMaster km 
                ON km.KOTTblNo = tm.TblNo 
                AND km.KOTSettled = 0 
                AND km.KOTtotal > 0 
                AND km.KOTCancelled = 0 
                AND km.KOTChargeable = 1
               WHERE tm.OltCode LIKE @Outlet
               GROUP BY tm.TblCode, tm.TblNo, TblSeatCount, km.KOTSettled

               UNION ALL

               SELECT DISTINCT 
                km.KOTTblNo AS TableCode, km.KOTTblNo AS TableNo,
                CASE WHEN km.KOTSettled IS NULL THEN 0 ELSE 1 END AS Occupy,
                SUM(KOTTotal) AS Amount, KOTSeatsServed AS Pax
               FROM KOTMaster km
               WHERE km.KOTSettled = 0 
                AND km.KOTtotal > 0  AND km.KOTCancelled = 0 
                AND km.KOTChargeable = 1 AND km.OltCode LIKE @Outlet
                AND km.KOTTblNo NOT IN (SELECT TblNo FROM TableMaster)
               GROUP BY km.KOTTblNo, KOTSeatsServed, km.KOTSettled";

            return (await connection.QueryAsync<TableModel>(sql, new { Outlet = outlet })).ToList();
        }

        public async Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder();

            string olt = string.Empty; 
            olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? "0" : outlet;

            var parameters = new DynamicParameters();
            parameters.Add("@fromdate", fromdate);
            parameters.Add("@todate", todate);
            parameters.Add("@oltcode", olt);
            parameters.Add("@branch", branchcode);

            await connection.ExecuteAsync( "SP_chancesheet_Application", parameters, commandType: CommandType.StoredProcedure);

            sql.Append(@" select Convert(NVARCHAR, BILLDATE, 103) as Date, BillTime, BillNo as BillNo, Kitchen as ItemSale, Tax, Tax/2 as CGST, Tax/2 as SGST, Disc as Dis, Total, BillTotal as Grand, ROUNDOFF as Roundoff, Cash, Card, NEFT, Pluxee, 0 as Cheque, UPI, PKG as Online, Credit, Charged as Room, Remarks as KBSRefName, oltcode, oltname as OltName, BranchCode From ChanceSheet Where BranchCode = @branch ");

            sql.Append(@" SELECT Particulars, SUM(Amount) AS Amount FROM ( SELECT ISNULL(S.SubModeType, M.ModeType) AS Particulars, K.KSMBillAmount AS Amount FROM KOTBillSettlement K 
                INNER JOIN KOTSettlementMaster KS  ON KS.KSMId = K.KSMId AND KS.Branch_Code = K.Branch_Code
                INNER JOIN Tbl_PaymentMode_Master M ON K.KBSPaymentMode = M.ModeType AND K.Branch_Code = M.BranchCode 
                LEFT JOIN Tbl_PaymentSubMode_Master S ON K.KBSRefName = S.SubModeType AND K.Branch_Code = S.BranchCode ");

            sql.Append(@"WHERE K.Branch_Code = @branch  AND KS.BillCancelled = 0 AND Convert(DATE, K.KBSSetteleDate) >= @FromDate and Convert(DATE, K.KBSSetteleDate) <= @ToDate ");

            if (olt != "0") 
            {
                sql.Append(@" And k.OltCode = @OltCode ");
            }

            sql.Append(@"UNION ALL SELECT 'Cancelled' AS Particulars, Round((KSMBillAmount + KSMBillTaxAmt - KSMBillDiscount + famt), 2) AS Amount FROM KOTSettlementMaster WHERE BillCancelled = 1 AND Branch_Code = @branch AND Convert(DATE, KSMBillDate) >= @FromDate and Convert(DATE, KSMBillDate) <= @ToDate ");

            if (olt != "0")
            {
                sql.Append(@" And OltCode = @OltCode ");
            }

            sql.Append(@" UNION ALL SELECT 'Company Transfered' AS Particulars, Round((ksm.KSMBillAmount + ksm.KSMBillTaxAmt - ksm.KSMBillDiscount + ksm.famt), 2) AS Amount FROM KOTSettlementMaster ksm
                    WHERE EXISTS ( SELECT 1 FROM BillTransferToCompany btc WHERE btc.Branch_Code = ksm.Branch_Code AND CONVERT(date,btc.BTDate) >= @FromDate AND CONVERT(date,btc.BTDate) <= @ToDate AND btc.BillNo = ksm.KSMBillNo) And ksm.BillCancelled = 0 And ksm.KSMBillTransfered = 1 AND ksm.Branch_Code = @branch AND Convert(DATE, ksm.KSMBillDate) >= @FromDate and Convert(DATE, ksm.KSMBillDate) <= @ToDate ");

            if (olt != "0")
            {
                sql.Append(@" And ksm.OltCode = @OltCode ");
            }

            sql.Append(@" UNION ALL SELECT 'Room Transfered' AS Particulars, Round((ksm.KSMBillAmount + ksm.KSMBillTaxAmt - ksm.KSMBillDiscount + ksm.famt), 2) AS Amount FROM KOTSettlementMaster ksm
                    WHERE NOT EXISTS ( SELECT 1 FROM KOTBillSettlement kbs WHERE kbs.Branch_Code = ksm.Branch_Code AND CONVERT(date,kbs.KBSSetteleDate) >= @FromDate AND CONVERT(date,kbs.KBSSetteleDate) <= @ToDate AND kbs.KSMBillNo = ksm.KSMBillNo) And ksm.BillCancelled = 0 And ksm.KSMBillTransfered = 1 And ksm.KSMIsRoomService = 1 AND ksm.Branch_Code = @branch AND Convert(DATE, ksm.KSMBillDate) >= @FromDate and Convert(DATE, ksm.KSMBillDate) <= @ToDate ");

            if (olt != "0")
            {
                sql.Append(@" And ksm.OltCode = @OltCode ");
            }

            sql.Append(@"UNION ALL SELECT 'Bill Pending' AS Particulars, Round((KSMBillAmount + KSMBillTaxAmt - KSMBillDiscount + famt), 2) AS Amount FROM KOTSettlementMaster WHERE KSMBillSettled = 0 AND BillCancelled = 0 AND Branch_Code = @branch AND Convert(DATE, KSMBillDate) >= @FromDate and Convert(DATE, KSMBillDate) <= @ToDate ");

            if (olt != "0")
            {
                sql.Append(@" And OltCode = @OltCode ");
            }

            sql.Append(@" ) BillData GROUP BY Particulars; ");

           

            //var sql = " select Convert(NVARCHAR, BILLDATE, 103) as Date, BillTime, BillNo as BillNo, Kitchen as ItemSale, Tax, Tax/2 as CGST, Tax/2 as SGST," +
            //    " Disc as Dis, Total, BillTotal as Grand, ROUNDOFF as Roundoff, Cash, Card, NEFT, Pluxee, 0 as Cheque, UPI, PKG as Online, Company as Credit," +
            //    " UNLNOWN as RoomNo, Remarks as KBSRefName, oltcode, oltname as OltName, BranchCode" +
            //    " From ChanceSheet Where BranchCode = @branch " +
            //    " SELECT Particulars, SUM(Amount) AS Amount FROM ( SELECT ISNULL(S.SubModeType, M.ModeType) AS Particulars, K.KSMBillAmount AS Amount " +
            //    " FROM KOTBillSettlement K INNER JOIN Tbl_PaymentMode_Master M ON K.KBSPaymentMode = M.ModeType AND K.Branch_Code = M.BranchCode LEFT JOIN Tbl_PaymentSubMode_Master S ON K.KBSRefName = S.SubModeType AND K.Branch_Code = S.BranchCode " +
            //    " WHERE K.Branch_Code = @branch AND Convert(DATE, K.KBSSetteleDate) Between @FromDate and @ToDate " +
            //    " UNION ALL SELECT 'Cancelled' AS Particulars, KSMBillAmount AS Amount FROM KOTSettlementMaster WHERE BillCancelled = 1 AND Branch_Code = @branch AND Convert(DATE, KSMBillDate) Between @FromDate and @ToDate " +
            //    " UNION ALL SELECT 'Bill Pending' AS Particulars, KSMBillAmount + KSMBillTaxAmt AS Amount FROM KOTSettlementMaster WHERE KSMBillSettled = 0 AND BillCancelled = 0\r\n    AND Branch_Code = @branch AND Convert(DATE, KSMBillDate) Between @FromDate and @ToDate ) BillData GROUP BY Particulars; ";

            //var sql = " select Convert(NVARCHAR, BILLDATE, 103) as Date, BillTime, BillNo as BillNo, Kitchen as ItemSale, Tax, Tax/2 as CGST, Tax/2 as SGST," +
            //    " Disc as Dis, Total, BillTotal as Grand, ROUNDOFF as Roundoff, Cash, Card, NEFT, Pluxee, 0 as Cheque, UPI, PKG as Online, Company as Credit," +
            //    " UNLNOWN as RoomNo, Remarks as KBSRefName, oltcode, oltname as OltName, BranchCode" +
            //    " From ChanceSheet Where BranchCode = @branch " +
            //    " SELECT KBSPaymentMode AS Particulars, SUM(KSMBillAmount) AS Amount FROM KOTBillSettlement WHERE Branch_Code = @branch AND KBSPaymentMode IN ('Cash', 'Card', 'UPI', 'NEFT', 'Pluxee', 'Online') AND Convert(Date, KBSSetteleDate) Between @FromDate and @ToDate GROUP BY KBSPaymentMode\r\n " +
            //    " UNION ALL SELECT 'Cancelled' AS Particulars, SUM(Cancel) AS Amount FROM ChanceSheet WHERE Remarks = 'Cancelled' " +
            //    " UNION ALL SELECT 'Credit' AS Particulars, SUM(Company) AS Amount FROM ChanceSheet WHERE Remarks <> 'Bill Pending';";

            //var sql = " select Convert(NVARCHAR, BILLDATE, 103) as Date, BillTime, BillNo as BillNo, Kitchen as ItemSale, Tax, Tax/2 as CGST, Tax/2 as SGST," +
            //    " Disc as Dis, Total, BillTotal as Grand, ROUNDOFF as Roundoff, Cash, Card, NEFT, Pluxee, 0 as Cheque, UPI, PKG as Online, Company as Credit," +
            //    " UNLNOWN as RoomNo, Remarks as KBSRefName, oltcode, oltname as OltName, BranchCode" +
            //    " From ChanceSheet Where BranchCode = @branch " +
            //    " SELECT Particulars, SUM(Amount) AS Amount FROM ( SELECT 'Cash' AS Particulars, Cash AS Amount FROM ChanceSheet WHERE Remarks <> 'Bill Pending' \r\n" +
            //    " UNION ALL SELECT 'Card', Card FROM ChanceSheet WHERE Remarks <> 'Bill Pending'\r\n " +
            //    " UNION ALL SELECT 'UPI', UPI as Online FROM ChanceSheet WHERE Remarks <> 'Bill Pending' \r\n" +
            //    " UNION ALL SELECT 'NEFT', NEFT FROM ChanceSheet WHERE Remarks <> 'Bill Pending' \r\n " +
            //    " UNION ALL SELECT 'Pluxee', Pluxee  FROM ChanceSheet WHERE Remarks <> 'Bill Pending' \r\n " +
            //    " UNION ALL SELECT 'Credit', Company FROM ChanceSheet WHERE Remarks <> 'Bill Pending' \r\n " +
            //    " UNION ALL SELECT 'Cancelled', Cancel FROM ChanceSheet WHERE Remarks = 'Cancelled' ) AS PaymentSummary \r\n" +
            //    " GROUP BY Particulars ORDER BY Particulars;";

            //var sql = " select Convert(NVARCHAR, BILLDATE, 103) as Date, BillTime, BillNo as BillNo, Kitchen as ItemSale, Tax, Tax/2 as CGST, Tax/2 as SGST," +
            //    " Disc as Dis, Total, BillTotal as Grand, ROUNDOFF as Roundoff, Cash, Card, NEFT, Pluxee, 0 as Cheque, UPI, PKG as Online , Company as Credit," +
            //    " UNLNOWN as RoomNo, Remarks as KBSRefName, oltcode, oltname as OltName, BranchCode" +
            //    " From ChanceSheet Where BranchCode = @branch " +
            //    " SELECT Particulars, SUM(Amount) AS Amount FROM ( SELECT Remarks AS Particulars, CASE WHEN Remarks = 'Cancelled'  AND Cancel > 0 THEN Cancel ELSE BillTotal END AS Amount FROM ChanceSheet WHERE Remarks <> 'BILL ON HOLD' ) AS BillData GROUP BY Particulars;";

            //" SELECT Remarks AS Particulars, CASE WHEN Remarks = 'Cancelled' AND SUM(CASE WHEN Cancel > 0 THEN 1 ELSE 0 END) > 0 THEN SUM(Cancel) ELSE SUM(BillTotal)  END AS Amount FROM ChanceSheet WHERE Remarks <> 'BILL ON HOLD' GROUP BY Remarks;";

            //return (await connection.QueryAsync<ChanceSheetModel>(sql, new { branch = branchcode })).ToList();
            using var multi = await connection.QueryMultipleAsync(sql.ToString(), new { branch = branchcode, FromDate = fromdate, ToDate = todate, OltCode = olt });
            var response = new ChanceSheetResponse
            {
                Data = (await multi.ReadAsync<ChanceSheetModel>()).ToList(),
                RemarksSummary = (await multi.ReadAsync<ChanceSheetRemarksSummary>()).ToList()
            };

            return response;
        }

        //public async Task<List<ChanceSheetModel>> GetChancesheet(DateTime fromdate, DateTime todate, string outlet)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet;

        //    var sql = @"
        //    SELECT 
        //        Date as BillDate,  BillTime,
        //        BillNo, ItemSale, Vat AS Tax,
        //        Vat/2 AS CGST,
        //        Vat/2 AS SGST,
        //        Dis as Discount, Total, Grand, RoundOff,
        //        Cash, Card,  UPI, Online, Cheque, Credit,
        //        RoomNo, KBSRefName, OltName
        //    FROM Chancefinal
        //    WHERE Date BETWEEN @Fromdate AND @Todate
        //      AND (@Outlet IS NULL OR OltCode = @Outlet)
        //    ORDER BY Date, BillNo";

        //    return (await connection.QueryAsync<ChanceSheetModel>(sql, new
        //    {
        //        Fromdate = fromdate.Date,
        //        Todate = todate.Date,
        //        Outlet = olt
        //    })).ToList();
        //}

        public async Task<List<OutletsaleModel>> GetOutletSale(DateTime fromdate, int outletcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
            SELECT 
                CONVERT(varchar, KSMBillDate, 103) AS Date,
                Sale,
                SaleAmt,
                oltcode
            FROM TableDetails
            WHERE CAST(KSMBillDate AS DATE) = @Fromdate
              AND oltcode = @OutletCode";

            return (await connection.QueryAsync<OutletsaleModel>(sql, new
            {
                Fromdate = fromdate.Date,
                OutletCode = outletcode
            })).ToList();
        }

        public async Task<List<SalesChartData>> GetSalesChart(DateTime fromdate, DateTime todate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
            SELECT 
                CAST(KSMBillDate AS DATE) AS Date,
                SUM(KSMBillAmount) AS Amount
            FROM KOTSettlementMaster
            WHERE KSMBillDate BETWEEN @Fromdate AND @Todate
            GROUP BY CAST(KSMBillDate AS DATE)
            ORDER BY Date";

            return (await connection.QueryAsync<SalesChartData>(sql, new
            {
                Fromdate = fromdate,
                Todate = todate
            })).ToList();
        }

        public async Task<List<OutletSaleData>> GetOutletSalesChart(DateTime fromdate, DateTime todate)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = @"
            SELECT CAST(ksm.KSMBillDate AS DATE) AS Date,
                   om.OltName AS Outlet,
                   SUM(ksm.KSMBillAmount) AS Amount
            FROM KOTSettlementMaster ksm
            JOIN OutletMaster om ON ksm.OltCode = om.OltCode
            WHERE ksm.KSMBillDate BETWEEN @Fromdate AND @Todate
            GROUP BY CAST(ksm.KSMBillDate AS DATE), om.OltName
            ORDER BY Date";

            return (await connection.QueryAsync<OutletSaleData>(sql, new { Fromdate = fromdate, Todate = todate })).ToList();
        }

        public async Task<List<TableItemsModel>> GetItemDetail(string tableNo, string oltCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
            SELECT 
                kd.KOTId,
                itemdisplayname AS ItemName,
                kotdrate AS Rate,
                SUM(kotdqty) AS Qty,
                (kotdrate * SUM(kotdqty)) AS Total
            FROM kotmaster km
            JOIN kotdetails kd ON km.kotid = kd.kotid
            JOIN itemmaster im ON kd.itemcode = im.itemcode
            WHERE kottblno = @TableNo
              AND (@OltCode = '%' OR km.OltCode = @OltCode)
              AND kotsettled = 0
              AND kotcancelled = 0
            GROUP BY kd.KOTId, itemdisplayname, kotdrate";

            return (await connection.QueryAsync<TableItemsModel>(sql, new
            {
                TableNo = tableNo,
                OltCode = oltCode
            })).ToList();
        }

        public async Task<List<VoidKotModel>> GetVoidData(DateTime fromdate, DateTime todate, string outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet;

            var sql = @"
            SELECT OltCode, OltName, KOTDate, KOTNO, ItemCode, ItemName, ItemQty, CancelQty, CancelRate, KOTTime, StwName, RefKotNo, Remarks
            FROM VIEW_VOIDKOT
            WHERE KOTDate BETWEEN @Fromdate AND @Todate
              AND (@Outlet IS NULL OR OltCode = @Outlet)
            ORDER BY KOTNO";

            return (await connection.QueryAsync<VoidKotModel>(sql, new
            {
                Fromdate = fromdate,
                Todate = todate,
                Outlet = olt
            })).ToList();
        }

        public async Task<List<NCKotModel>> GetNCData(DateTime fromdate, DateTime todate, string outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var olt = (string.IsNullOrEmpty(outlet) || outlet == "All") ? null : outlet;

            var sql = @"
            SELECT OltCode, OltName, ItemName, KOTDQty, KOTDate, KOTTime, NCKOT_Particulars, KOTDRate, NCDepCode, NCDepName, KOTNO, (KOTDQty * KOTDRate) as KOTTotal
            FROM NCKOT
            WHERE KOTDate BETWEEN @Fromdate AND @Todate
              AND (@Outlet IS NULL OR OltCode = @Outlet)
            ORDER BY KOTNO";

            return (await connection.QueryAsync<NCKotModel>(sql, new
            {
                Fromdate = fromdate,
                Todate = todate,
                Outlet = olt
            })).ToList();
        }

        //public async Task<List<KotCancelReportDto>> GetKotCancellation(KotCancellationModel request)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    string query = @"
        //        SELECT  km.KOTNo, km.KOTDate, km.KOTTime, (kd.KOTDQty * kd.KOTDRate) AS KOTTotal, olt.OltName 
        //        FROM KOTMaster km
        //        INNER JOIN KOTDetails kd ON km.KOTNo = kd.KOTNo
        //        INNER JOIN OutletMaster olt ON km.OltCode = olt.OltCode
        //        WHERE km.KOTCancelled = 1 AND km.Branch_Code = @BranchCode";

        //    List<SqlParameter> parameters = new()
        //    {
        //        new SqlParameter("@BranchCode", request.BranchCode)
        //    };

        //    // As On Date
        //    if (request.IsAsOnDate)
        //    {
        //        query += " AND km.KOTDate = @KotDate";

        //        parameters.Add(new SqlParameter("@KotDate", request.Date.Date));
        //    }

        //    // Between Dates
        //    if (request.IsBetweenDates)
        //    {
        //        if (request.BillingType == "D")
        //        {
        //            DateTime toDate = request.ToDate.AddDays(1);

        //            query += @" AND CAST(km.KOTDate AS DATETIME) +  CAST(RIGHT(km.KOTTime,8) AS DATETIME) >= @FromDateTime
        //                AND CAST(km.KOTDate AS DATETIME) +   CAST(RIGHT(km.KOTTime,8) AS DATETIME) <= @ToDateTime";

        //            parameters.Add(new SqlParameter("@FromDateTime", request.FromDate.Date.AddHours(2)));

        //            parameters.Add(new SqlParameter("@ToDateTime", toDate.Date.AddHours(2)));
        //        }
        //        else
        //        {
        //            query += @" AND km.KOTDate >= @FromDate AND km.KOTDate <= @ToDate";

        //            parameters.Add(new SqlParameter("@FromDate", request.FromDate.Date));
        //            parameters.Add(new SqlParameter("@ToDate", request.ToDate.Date));
        //        }
        //    }

        //    // Outlet Filter
        //    if (!string.IsNullOrWhiteSpace(request.OutletCode))
        //    {
        //        query += " AND km.OltCode = @OutletCode";

        //        parameters.Add(new SqlParameter("@OutletCode", request.OutletCode));
        //    }

        //    List<KotCancelReportDto> reportList = new();

        //    using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)connection))
        //    {
        //        cmd.Parameters.AddRange(parameters.ToArray());

        //        using SqlDataReader reader = await cmd.ExecuteReaderAsync();

        //        while (await reader.ReadAsync())
        //        {
        //            reportList.Add(new KotCancelReportDto
        //            {
        //                KotNo = reader["KOTNo"].ToString(),
        //                KotDate = Convert.ToDateTime(reader["KOTDate"]),
        //                KotTime = reader["KOTTime"].ToString(),
        //                TotalAmount = Math.Round(
        //                    Convert.ToDecimal(reader["KOTTotal"]), 2),
        //                Outlet = reader["OltName"].ToString()
        //            });
        //        }

        //        return reportList;
        //    }

        //    //// Insert into temp table
        //    //foreach (var item in reportList)
        //    //{
        //    //    string insertQuery = @"
        //    //        INSERT INTO TMP_KOTFORALL
        //    //        ( Slno, KotDate, KotNo, KotTime, TotalAmount, OUTLET, IPADDRESS )
        //    //        VALUES
        //    //        ( @Slno, @KotDate, @KotNo, @KotTime, @TotalAmount, @Outlet, @IpAddress )";

        //    //    using SqlCommand insertCmd = new SqlCommand(insertQuery, (SqlConnection)connection);

        //    //    insertCmd.Parameters.AddWithValue("@Slno", 1);
        //    //    insertCmd.Parameters.AddWithValue("@KotDate", item.KotDate);
        //    //    insertCmd.Parameters.AddWithValue("@KotNo", item.KotNo);
        //    //    insertCmd.Parameters.AddWithValue("@KotTime", item.KotTime);
        //    //    insertCmd.Parameters.AddWithValue("@TotalAmount", item.TotalAmount);
        //    //    insertCmd.Parameters.AddWithValue("@Outlet", item.Outlet);
        //    //    insertCmd.Parameters.AddWithValue("@IpAddress", request.SystemId);

        //    //    insertCmd.ExecuteNonQuery();
        //    //}
        //}

        public async Task<List<KotCancelReportDto>> GetKotCancellation(KotCancellationModel request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder(@"
                        SELECT km.KOTNo, km.KOTDate, km.KOTTime, (kd.KOTDQty * kd.KOTDRate) AS TotalAmount, olt.OltName AS Outlet
                        FROM KOTMaster km
                        INNER JOIN KOTDetails kd ON km.KOTNo = kd.KOTNo
                        INNER JOIN OutletMaster olt ON km.OltCode = olt.OltCode
                        WHERE km.KOTCancelled = 1 AND km.Branch_Code = @BranchCode");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND km.KOTDate = @KotDate");

                parameters.Add("@KotDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND km.KOTDateTime >= @FromDateTime AND km.KOTDateTime <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND km.KOTDate >= @FromDate AND km.KOTDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter

            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND km.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR km.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            var result = await connection.QueryAsync<KotCancelReportDto>( sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<List<BillCancelReportDto>> GetBillCancellation(BillCancellationModel request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder(@"
                        SELECT KSMBillNo, KSMBillDate, (KSMBillAmount - KSMBillDiscount + KSMBillTaxAmt + KSMServiceCharge + KSMServiceTaxAmt) as TotalAmount, DCParticulars as Reason FROM KOTSettlementMaster
                        WHERE Billcancelled = 1 And AccountType = 'F' AND Branch_Code = @BranchCode");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KSMBillDate = @BillDate");

                parameters.Add("@BillDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KsmBillDate AS DATETIME) + CAST(KSMBillTime AS DATETIME) >= @FromDateTime AND CAST(KsmBillDate AS DATETIME) + CAST(KSMBillTime AS DATETIME) <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KSMBillDate >= @FromDate AND KSMBillDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            var result = await connection.QueryAsync<BillCancelReportDto>(sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<List<CreditOutstandingReportDto>> GetCreditOutstanding(CreditOutstandingModel request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder(@"
                         SELECT cm.CompanyName AS CompanyName, btc.BTDate, btc.BillNo, btc.BillAmt, btc.BTCSettled, cm.CompanyCode, olt.OltName
                         FROM CompanyMaster cm
                         INNER JOIN BillTransferToCompany btc ON cm.CompanyCode = btc.CompanyCode
                         INNER JOIN Outletmaster olt ON olt.oltcode = btc.oltcode
                         WHERE btc.BTCSettled = 0 AND btc.Branch_Code = @BranchCode");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND btc.BTDate = @BillDate");

                parameters.Add("@BillDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(btc.BTDate AS DATETIME)
                                + CAST(RIGHT(btc.Lastmodify, 8) AS DATETIME) >= @FromDateTime AND CAST(btc.BTDate AS DATETIME)
                                + CAST(RIGHT(btc.Lastmodify, 8) AS DATETIME) <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND btc.BTDate >= @FromDate AND btc.BTDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND btc.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR btc.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            // Company Filter
            if (!string.IsNullOrWhiteSpace(request.CompanyCode))
            {
                sql.Append(" AND btc.CompanyCode = @CompanyCode");
                parameters.Add("@CompanyCode", request.CompanyCode);
            }

            sql.Append(" ORDER BY CONVERT(INT, btc.BillNo)");

            var result = await connection.QueryAsync<CreditOutstandingReportDto>(sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<List<DailysaleCategorywiseReport>> GetSubCatNormalBillsAsync(DailySaleCategorywiseModel request, IDbConnection con)
        {

            var sql = new StringBuilder(@"SELECT KSM.KSMBILLNO, KSM.OLTCode, KD.ItemCode, IM.ItemName, KD.KOTDRate, KSM.KSMBillDate, IM.GrpCode, IM.QPB,
                SUM(KD.KOTDQty) AS Qty, IM.ItemType, KSM.Branch_Code, Dep.SubCatName AS CatName, KSM.KSMBillTime
                From KOTSettlementMaster AS KSM
                INNER JOIN KOTSettlementDetails AS KSD ON KSM.KSMId = KSD.KSMId
                INNER JOIN KOTDetails AS KD ON KSD.KOTId = KD.KOTId
                INNER JOIN ItemMaster AS IM ON KD.ItemCode = IM.ItemCode
                INNER JOIN KOTMaster AS KM ON KSD.KOTId = KM.KOTId
                INNER JOIN ItemSubCategory Dep ON IM.QPB = Dep.SubCatCode
                WHERE SplitRef_No IS NULL AND KSM.Branch_Code = @BranchCode AND KSM.BillCancelled = '0' AND AccountType = 'F'");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KSM.KSMBillDate = @AsOnDate");

                parameters.Add("@AsOnDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KSM.KSMBillDate AS DATETIME)
                                + CAST(RIGHT(KSM.KSMBillTime, 8) AS DATETIME) >= @FromDateTime AND CAST(KSM.KSMBillDate AS DATETIME)
                                + CAST(RIGHT(KSM.KSMBillTime, 8) AS DATETIME) <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KSM.KSMBillDate >= @FromDate AND KSM.KSMBillDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND KSM.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR KSM.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            if (!string.IsNullOrEmpty(request.SubCatCode))
            {
                sql.Append($" AND Dep.SubCatCode IN (@subcatcode) ");

                parameters.Add("@subcatcode", request.SubCatCode);
            }

            sql.Append(@" GROUP BY KD.ItemCode, IM.ItemName, KD.KOTDRate, IM.GrpCode, IM.QPB, IM.ItemType, KSM.KSMBillDate, 
                KSM.Branch_Code, Dep.SubCatName, KSM.KSMBillNO, KSM.OLTCode, KSM.KSMBillTime
                ORDER BY KSM.KSMBillNO ");

            var result = await con.QueryAsync<DailysaleCategorywiseReport>(sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<List<DailysaleCategorywiseReport>> GetSubCatSplitBillsAsync(DailySaleCategorywiseModel request, IDbConnection con)
        {

            var sql = new StringBuilder(@" SELECT  KSM.OltCode, sb.Bill_No AS KSMBillNo, sbd.ItemCode, sbd.ItemName, sbd.Rate AS KOTDRate, IM.GrpCode, 
                Dep.SubCatCode, SUM(Qty) AS Qty, KSM.KSMBillDate, sb.Branch_Code, Dep.SubCatName AS CatName, KSM.KSMBillTime 
                FROM SplitBill sb
                INNER JOIN SplitBill_Details sbd ON sb.Bill_No = sbd.Bill_No
                INNER JOIN KOTSettlementMaster KSM ON KSM.KSMBillNo = sb.Bill_No
                INNER JOIN ItemMaster IM ON sbd.ItemCode = IM.ItemCode
                INNER JOIN ItemSubCategory Dep ON IM.QPB = Dep.SubCatCode
                WHERE sb.Branch_Code = @BranchCode AND KSM.BillCancelled = '0'");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KsmBillDate = @BillDate");

                parameters.Add("@BillDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) >= @FromDateTime AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KsmBillDate >= @FromDate AND KsmBillDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND sb.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR sb.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            // Company Filter
            if (!string.IsNullOrWhiteSpace(request.SubCatCode))
            {
                sql.Append($" AND Dep.SubCatCode IN (@subcode) ");
                parameters.Add("@subcode", request.SubCatCode);
            }

            sql.Append(" GROUP BY ksm.OltCode,sbd.ItemCode, sbd.ItemName, sbd.Rate, im.GrpCode, im.QPB, im.ItemType, ksm.KSMBillDate, sb.Branch_Code,dep.SubCatCode,dep.SubCatName,sb.Bill_No,KSM.KSMBillTime order by sb.Bill_No ");

            var result = await con.QueryAsync<DailysaleCategorywiseReport>(sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<List<DailysaleCategorywiseReport>> GetDepWiseNormalBillsAsync(DailySaleCategorywiseModel request, IDbConnection con)
        {

            var sql = new StringBuilder(@"SELECT KSM.KSMBILLNO,ksm.oltcode,kd.ItemCode, im.ItemName, kd.KOTDRate,ksm.KsmBillDate,im.GrpCode, 
                im.CatCode, SUM(kd.KOTDQty) AS Qty, im.ItemType, ksm.KSMBillDate, ksm.Branch_Code,Dep.CatName,KSM.KSMBillTime 
                FROM KOTSettlementMaster AS ksm 
                INNER JOIN KOTSettlementDetails AS ksd ON ksm.KSMId = ksd.KSMId 
                INNER JOIN KOTDetails AS kd ON ksd.KOTId = kd.KOTId 
                INNER JOIN ItemMaster AS im ON kd.ItemCode = im.ItemCode 
                INNER JOIN KOTMaster AS km ON ksd.KOTId = km.KOTId 
                INNER JOIN ItemCategory Dep on Im.CatCode = Dep.CatCode
                Where splitref_no is null and ksm.Branch_Code = @BranchCode and (ksm.BillCancelled = '0') and AccountType = 'F'");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KsmBillDate = @AsOnDate");

                parameters.Add("@AsOnDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) >= @FromDateTime AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) <= @ToDateTime ");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KsmBillDate >= @FromDate AND KsmBillDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND ksm.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR ksm.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            if (!string.IsNullOrEmpty(request.CatCode) && request.CatCode != "All")
            {
                sql.Append($" AND IM.CatCode = @catcode ");

                parameters.Add("@catcode", request.CatCode);
            }
            else
            {
                var CatCode = (string.IsNullOrEmpty(request.CatCode) || request.CatCode == "All") ? null : request.CatCode;

                sql.Append(" AND(@catcode IS NULL OR IM.CatCode = @catcode)");

                parameters.Add("@catcode", CatCode);
            }

            sql.Append(@" GROUP BY kd.ItemCode, im.ItemName, kd.KOTDRate, im.GrpCode, im.CatCode, im.ItemType, ksm.KSMBillDate, ksm.Branch_Code,dep.CatName,KsmBillNo,ksm.oltcode,KSM.KSMBillTime Order by ksmbillno
            ");

            var result = await con.QueryAsync<DailysaleCategorywiseReport>(sql.ToString(), parameters);

            return result.ToList();
        }
        

        public async Task<List<DailysaleCategorywiseReport>> GetDepWiseSplitBillsAsync(DailySaleCategorywiseModel request, IDbConnection con)
        {

            var sql = new StringBuilder(@" SELECT KSM.OltCode, sb.Bill_No AS KSMBillNo, sbd.ItemCode, sbd.ItemName, sbd.Rate AS KOTDRate,
                IM.GrpCode, Dep.CatCode, SUM(Qty) AS Qty, KSM.KSMBillDate, sb.Branch_Code, CatName, KSM.KSMBillTime
                FROM SplitBill sb
                INNER JOIN SplitBill_Details sbd ON sb.Bill_No = sbd.Bill_No
                INNER JOIN KOTSettlementMaster KSM ON KSM.KSMBillNo = sb.Bill_No
                INNER JOIN ItemMaster IM ON sbd.ItemCode = IM.ItemCode
                INNER JOIN ItemCategory Dep on Im.CatCode = Dep.CatCode
                INNER JOIN OutletMaster OLT ON SB.OltCode = OLT.OltCode
                WHERE sb.Branch_Code = @BranchCode AND KSM.BillCancelled = '0'");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KsmBillDate = @BillDate");

                parameters.Add("@BillDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType?.Trim().Equals("D", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var fromDate = request.FromDate.Date.AddHours(2);
                    var toDate = request.ToDate.Date.AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) >= @FromDateTime AND CAST(KsmBillDate AS DATETIME)
                                + CAST(RIGHT(KSMBillTime, 8) AS DATETIME) <= @ToDateTime");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KsmBillDate >= @FromDate AND KsmBillDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet Filter
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND sb.OltCode = @OutletCode");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR sb.OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            // Company Filter
            if (!string.IsNullOrWhiteSpace(request.CatCode) && request.CatCode != "All")
            {
                sql.Append($" AND IM.itemcode = @catcode ");
                parameters.Add("@catcode", request.CatCode);
            }
            else
            {
                var CatCode = (string.IsNullOrEmpty(request.CatCode) || request.CatCode == "All") ? null : request.CatCode;

                sql.Append(" AND(@catcode IS NULL OR IM.CatCode = @catcode)");

                parameters.Add("@catcode", CatCode);
            }

            sql.Append("GROUP BY ksm.OltCode,sbd.ItemCode, sbd.ItemName, sbd.Rate, im.GrpCode, im.CatCode, im.ItemType, ksm.KSMBillDate, sb.Branch_Code,dep.CatCode,dep.CatName,sb.Bill_No,KSM.KSMBillTime order by sb.Bill_No");

            var result = await con.QueryAsync<DailysaleCategorywiseReport>(sql.ToString(), parameters);

            return result.ToList();
        }

        //public async Task<decimal> GetTaxesAsync(List<DailysaleCategorywiseReport> bills, string BillType, string OutletCode, IDbConnection con)
        //{
        //    decimal taxes = 0;

        //    var distinctBills = bills .GroupBy(x => new { x.KSMBillNo, x.KSMBillDate, x.KSMBillTime }) .Select(g => g.First()) .ToList();

        //    foreach (var bill in distinctBills)
        //    {
        //        if (string.IsNullOrEmpty(bill.KSMBillNo?.ToString()))
        //            continue;

        //        var sql = new StringBuilder();

        //        sql.Append(@" SELECT ( KSM.KSMBillTaxAmt - KSM.KSMBillDiscount + KSM.KSMServiceTaxAmt + KSM.KSMServiceCharge) AS Taxes
        //            FROM KOTSettlementMaster KSM
        //            WHERE KSM.KSMBillNo = @BillNo AND KSM.AccountType = 'F' ");

        //        // Billing Type Filter
        //        if (BillType == "D")
        //        {
        //            sql.Append(@" AND CAST( KSM.KSMBillDate AS DATETIME ) + CAST( RIGHT(KSM.KSMBillTime, 8) AS DATETIME ) =
        //            CAST( @BillDate AS DATETIME ) + CAST( RIGHT(@BillTime, 8) AS DATETIME ) AND KSM.DayEnd = 'Y' ");
        //        }
        //        else
        //        {
        //            sql.Append(@" AND KSM.KSMBillDate = @BillDate ");
        //        }

        //        // Outlet Filter
        //        if (!string.IsNullOrEmpty(OutletCode) && OutletCode != "All")
        //        {
        //            sql.Append(" AND KSM.OltCode = @OutletCode ");
        //        }
        //        else
        //        {
        //            OutletCode = (string.IsNullOrEmpty(OutletCode) || OutletCode == "All") ? null : OutletCode;

        //            sql.Append(" AND(@OutletCode IS NULL OR KSM.OltCode = @OutletCode)");
        //        }

        //        var billTaxes = await con.QueryAsync<decimal>(
        //            sql.ToString(),
        //            new
        //            {
        //                BillNo = bill.KSMBillNo,
        //                BillDate = bill.KSMBillDate,
        //                BillTime = bill.KSMBillTime,
        //                OutletCode
        //            });

        //        taxes += billTaxes.Sum();
        //    }

        //    return Math.Round(taxes, 0);
        //}

        public async Task<decimal> GetTaxesAsync(List<DailysaleCategorywiseReport> bills, string BillType, string OutletCode, IDbConnection con)
        {
            if (bills == null || bills.Count == 0)
                return 0;

            var sqlCon = (SqlConnection)con;

            var distinctBills = bills.Where(x => !string.IsNullOrWhiteSpace(x.KSMBillNo))
                                .GroupBy(x => new { x.KSMBillNo, x.KSMBillDate, x.KSMBillTime })
                                .Select(g => g.First()).ToList();

            if (!distinctBills.Any())
                return 0;

            var dt = new DataTable();
            dt.Columns.Add("BillNo", typeof(string));
            dt.Columns.Add("BillDate", typeof(DateTime));
            dt.Columns.Add("BillTime", typeof(string));

            foreach (var bill in distinctBills)
            {
                dt.Rows.Add(bill.KSMBillNo ?? "", bill.KSMBillDate, bill.KSMBillTime ?? "");
            }

            using var tran = sqlCon.BeginTransaction();

            try
            {
                await sqlCon.ExecuteAsync(@"CREATE TABLE #BillTemp(BillNo VARCHAR(50) COLLATE Latin1_General_CI_AI,BillDate DATETIME,
                                            BillTime NVARCHAR(20) COLLATE Latin1_General_CI_AI)", transaction: tran);

                using (var bulk = new SqlBulkCopy(sqlCon, SqlBulkCopyOptions.Default, tran))
                {
                    bulk.DestinationTableName = "#BillTemp";
                    await bulk.WriteToServerAsync(dt);
                }

                string sql = @"SELECT ISNULL(SUM(KSM.KSMBillTaxAmt - KSM.KSMBillDiscount + KSM.KSMServiceTaxAmt + KSM.KSMServiceCharge), 0) 
                       FROM KOTSettlementMaster KSM
                       INNER JOIN #BillTemp B ON KSM.KSMBillNo COLLATE Latin1_General_CI_AI = B.BillNo WHERE KSM.AccountType = 'F'";
                if (BillType == "D")
                {
                    sql += @"AND KSM.BillDateOnly = CONVERT(date, B.BillDate) AND KSM.KSMBillTime = B.BillTime AND KSM.DayEnd = 'Y'";
                }
                OutletCode = (string.IsNullOrEmpty(OutletCode) || OutletCode == "All") ? null : OutletCode;
                sql += @" AND (@OutletCode IS NULL OR KSM.OltCode = @OutletCode) ";

                var tax = await sqlCon.ExecuteScalarAsync<decimal>(sql, new { OutletCode }, transaction: tran);

                await sqlCon.ExecuteAsync("DROP TABLE #BillTemp", transaction: tran);
                tran.Commit();

                return Math.Round(tax, 0);
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
        public async Task<List<KOTRegisterResponseDto>> GetKotRegister(KOTRegisterModel request, string viewName, int status)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = new StringBuilder($@" SELECT KOTNO, IssueTime, ITEM as ItemName, CONVERT(DECIMAL, QTY) AS Qty, U_ID as UserId, Steward, TableNo, ITEMAMT as TotalAmount");

            if (status == 0)
            {
                sql.Append(", BillNo ");
            }

            sql.Append($@" FROM {viewName}  WHERE branch_code = @BranchCode ");

            var parameters = new DynamicParameters();

            parameters.Add("@BranchCode", request.BranchCode);

            // As On Date
            if (request.IsAsOnDate)
            {
                sql.Append(" AND KotDate = @KotDate ");

                parameters.Add("@KotDate", request.Date.Date);
            }

            // Between Dates
            if (request.IsBetweenDates)
            {
                if (request.BillingType == "D")
                {
                    var fromDate = request.FromDate.Date.AddHours(2);

                    var toDate = request.ToDate.Date .AddDays(1).AddHours(2);

                    sql.Append(@" AND CAST(KotDate AS DATETIME) + CAST(RIGHT(ISSUETIME,8) AS DATETIME) >= @FromDateTime 
                                  AND CAST(KotDate AS DATETIME) + CAST(RIGHT(ISSUETIME,8) AS DATETIME) <= @ToDateTime ");

                    parameters.Add("@FromDateTime", fromDate);
                    parameters.Add("@ToDateTime", toDate);
                }
                else
                {
                    sql.Append(@" AND KotDate >= @FromDate AND KotDate <= @ToDate");

                    parameters.Add("@FromDate", request.FromDate.Date);
                    parameters.Add("@ToDate", request.ToDate.Date);
                }
            }

            // Outlet
            if (!string.IsNullOrWhiteSpace(request.OutletCode) && request.OutletCode != "All")
            {
                sql.Append(" AND OltCode = @OutletCode ");

                parameters.Add("@OutletCode", request.OutletCode);
            }
            else
            {
                var olt = (string.IsNullOrEmpty(request.OutletCode) || request.OutletCode == "All") ? null : request.OutletCode;

                sql.Append(" AND(@Outlet IS NULL OR OltCode = @Outlet)");

                parameters.Add("@Outlet", olt);
            }

            // Pending
            if (request.IsPendingkot)
            {
                sql.Append(" AND KotSettled = 0 ");
            }

            // Table No
           if (!string.IsNullOrWhiteSpace(request.TableNo) && request.TableNo != "All")
            {
                sql.Append(" AND TABLENO = @TableNo ");

                parameters.Add("@TableNo", request.TableNo);
            }
            else 
            {
                sql.Append(" AND ISNULL(TABLENO,'') <> '' ");
            }

            var result = await connection.QueryAsync<KOTRegisterResponseDto>( sql.ToString(), parameters);

            return result.ToList();
        }

        public async Task<DashboardSummaryDto> GetDashboardData(string Branchcode, DateTime TodayDateTime)
        {
            // Daily
            var dailyDate = TodayDateTime.Date;

            // Monthly
            var monthStart = new DateTime(TodayDateTime.Year, TodayDateTime.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Financial Year (April 1 - March 31)
            DateTime fyStart;
            DateTime fyEnd;

            if (TodayDateTime.Month >= 4) // April to December
            {
                fyStart = new DateTime(TodayDateTime.Year, 4, 1);
                fyEnd = new DateTime(TodayDateTime.Year + 1, 3, 31);
            }
            else // January to March
            {
                fyStart = new DateTime(TodayDateTime.Year - 1, 4, 1);
                fyEnd = new DateTime(TodayDateTime.Year, 3, 31);
            }
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = ($@"
                SELECT CAST(ISNULL(( SELECT SUM(KsmbillAmount + KsmbillTaxAmt - KsmbillDiscount) FROM KotSettlementMaster WHERE KsmbillSettled = '1' AND BillCancelled = '0' AND KsmbillDate = @DailyDate AND Branch_Code = @branch ),0) AS DECIMAL(10,2)) AS DailySalesTotal,

                CAST(ISNULL(( SELECT SUM(KsmbillAmount + KsmbillTaxAmt - KsmbillDiscount) FROM KotSettlementMaster WHERE KsmbillSettled = '1' AND BillCancelled = '0' AND KsmbillDate BETWEEN @MonthStart AND @MonthEnd AND Branch_Code = @branch ),0) AS DECIMAL(10,2)) AS MonthlySalesTotal,

                CAST(ISNULL(( SELECT SUM(KsmbillAmount + KsmbillTaxAmt - KsmbillDiscount) FROM KotSettlementMaster WHERE KsmbillSettled = '1' AND BillCancelled = '0' AND KsmbillDate BETWEEN @FYStart AND @FYEnd AND Branch_Code = @branch ),0) AS DECIMAL(10,2)) AS YearlySalesTotal;
                ");

            return await connection.QueryFirstOrDefaultAsync<DashboardSummaryDto>(sql, new
            {
                branch = Branchcode,
                DailyDate = dailyDate,
                MonthStart = monthStart,
                MonthEnd = monthEnd,
                FYStart = fyStart,
                FYEnd = fyEnd
            });
        }
    }
}