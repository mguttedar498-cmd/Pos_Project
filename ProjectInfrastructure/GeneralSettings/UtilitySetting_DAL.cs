using Dapper;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using System.Data;

namespace HMS_360_PMS.ProjectInfrastructure.GeneralSettings
{
    public class UtilitySetting_DAL : IUtilitySetting_Repository
    {
        private readonly DbConnectionFactory _factory;

        public UtilitySetting_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        #region Happy Hours Setting

        public async Task<HappyHoursModel> GetHappyHoursSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select InOrExOfTax, HappyHours, HHFrom, HHTo, BranchCode From dbo.TGeneralSettings Where BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<HappyHoursModel>(qry, new { Branch = branchcode }));
        }

        //public async Task<bool> SaveHappyHoursSettings(bool InOrExOfTax, bool HappyHours, TimeSpan from, TimeSpan to, string branchcode)
        //public async Task<bool> SaveorUpdateHappyHoursSettings(HappyHoursRequest settings)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    TimeSpan from = TimeSpan.Parse(settings.HHFrom);
        //    TimeSpan to = TimeSpan.Parse(settings.HHTo);

        //    const string query = @"
        //    UPDATE TGeneralSettings SET
        //    InOrExOfTax = COALESCE(@InOrExOfTax, inOrExOfTax),
        //    HappyHours = COALESCE(@HappyHours, happyHours),
        //    HHFrom = COALESCE(@HHFrom, HHFrom),
        //    HHTo = COALESCE(@HHTo, HHTo)
        //    Where BranchCode = @branchCode";

        //    var rows = await connection.ExecuteAsync(query, new { inOrExOfTax = settings.InOrExOfTax, happyHours = settings.HappyHours, HHFrom = from.ToString(@"hh\:mm"), HHTo = to.ToString(@"hh\:mm"), branchCode = settings.BranchCode });
        //    return rows > 0;
        //}

        public async Task<bool> SaveorUpdateHappyHoursSettings(HappyHoursModel settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //TimeSpan from = TimeSpan.Parse(settings.HHFrom);
            //TimeSpan to = TimeSpan.Parse(settings.HHTo);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM TGeneralSettings WHERE BranchCode = @BranchCode
            )
            BEGIN
            UPDATE TGeneralSettings SET 
            InOrExOfTax = COALESCE(@InOrExOfTax, InOrExOfTax),
            HappyHours = COALESCE(@HappyHours, HappyHours),
            HHFrom = COALESCE(@HHFrom, HHFrom),
            HHTo = COALESCE(@HHTo, HHTo)
            WHERE BranchCode = @BranchCode
            END
            ELSE
            BEGIN
            INSERT INTO TGeneralSettings
            (InOrExOfTax, HappyHours, HHFrom, HHTo, BranchCode)
            VALUES
            (@InOrExOfTax, @HappyHours, @HHFrom, @HHTo, @BranchCode)
            END";

            var rows = await connection.ExecuteAsync(query, settings);
            return rows > 0;
        }

        #endregion


        #region KOT Timer Setting

        public async Task<TimerSettingModel> GetKOTTimerSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select TimerRequired, TimerMinute, BranchCode From dbo.Tbl_TimerSetting_Master  Where BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<TimerSettingModel>(qry, new { Branch = branchcode }));
        }

        //public async Task<bool> UpdateKOTTimerSettings(TimerSettingMaster settings)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    const string query = @"
        //    UPDATE Tbl_TimerSetting_Master SET
        //    TimerRequired = COALESCE(@TimerRequired, timerRequired),
        //    TimerMinute = COALESCE(@TimerMinute, timerMinute)
        //    Where TimerId = @timerId And BranchCode = @branchCode";

        //    var rows = await connection.ExecuteAsync(query, new { timerRequired = settings.TimerRequired, timerMinute = settings.TimerMinute, branchCode = settings.BranchCode, timerId = settings.TimerId });
        //    return rows > 0;
        //}

        public async Task<bool> SaveOrUpdateKOTTimerSettings(TimerSettingModel settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM Tbl_TimerSetting_Master WHERE BranchCode = @BranchCode
            )
            BEGIN
            UPDATE Tbl_TimerSetting_Master SET 
            TimerRequired = COALESCE(@TimerRequired, TimerRequired),
            TimerMinute = COALESCE(@TimerMinute, TimerMinute)
            WHERE BranchCode = @BranchCode
            END
            ELSE
            BEGIN
            INSERT INTO Tbl_TimerSetting_Master
            (TimerRequired, TimerMinute, BranchCode)
            VALUES
            (@TimerRequired, @TimerMinute, @BranchCode)
            END";

            var rows = await connection.ExecuteAsync(query, settings);
            return rows > 0;
        }

        #endregion

        #region Financial Year Setting

        public async Task<FinancialYearModel> GetFinancialSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "SELECT FinId, FinFromDate, FinToDate , FincurrentYear , FinEndYear, CurrentStatus, LogUser, IpAddress, FinalClose, FinCode, BranchCode FROM dbo.Tbl_Financial_Master Where BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<FinancialYearModel>(qry, new { Branch = branchcode }));
        }

        public async Task<bool> SaveOrUpdateFinancialSettings(FinancialYearModel settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM Tbl_Financial_Master WHERE BranchCode = @BranchCode
            )
            BEGIN
            UPDATE Tbl_Financial_Master SET 
            FinFromDate = COALESCE(@FinFromDate, FinFromDate),
            FinToDate = COALESCE(@FinToDate, FinToDate),
            FincurrentYear = COALESCE(@FincurrentYear, FincurrentYear),
            FinEndYear = COALESCE(@FinEndYear, FinEndYear),
            CurrentStatus = COALESCE(@CurrentStatus, CurrentStatus),
            LogUser = COALESCE(@LogUser, LogUser),
            IpAddress = COALESCE(@IpAddress, IpAddress),
            FinalClose = COALESCE(@FinalClose, FinalClose),
            FinCode = COALESCE(@FinCode, FinCode)
            WHERE BranchCode = @BranchCode
            END
            ELSE
            BEGIN
            INSERT INTO Tbl_Financial_Master
            (FinFromDate, FinToDate, FincurrentYear, FinEndYear, CurrentStatus, LogUser, IpAddress, FinalClose, FinCode, BranchCode)
            VALUES
            (@FinFromDate, @FinToDate, @FincurrentYear, @FinEndYear, @CurrentStatus, @LogUser, @IpAddress, @FinalClose, @FinCode, @BranchCode)
            END";

            var rows = await connection.ExecuteAsync(query, settings);
            return rows > 0;
        }

        #endregion

        #region TaxMode Setting

        public async Task<IEnumerable<TaxModeSettingModel>> GetTaxModeSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select TaxId, TaxRequired, TaxType, BranchCode" +
                " From dbo.Tbl_TaxSettingMode_Master" +
                " Where BranchCode = @Branch";

            return (await connection.QueryAsync<TaxModeSettingModel>(qry, new { Branch = branchcode }));
        }

        public async Task<bool> UpdateTaxModeSettings(TaxModeSettingModel settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE Tbl_TaxSettingMode_Master SET
            TaxRequired = COALESCE(@taxRequired, TaxRequired)
            Where TaxId = @taxId And BranchCode = @branchCode";

            var rows = await connection.ExecuteAsync(query, new { taxRequired = settings.TaxRequired, branchCode = settings.BranchCode, taxId = settings.TaxId });
            return rows > 0;
        }

        #endregion

        #region DiscountMode Setting

        public async Task<IEnumerable<DiscountModeSetting>> GetDiscountModeSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select DiscId, DiscountRequired, DiscountType, BranchCode" +
                " From Tbl_DiscountSettingMode_Master " +
                " Where BranchCode = @BranchCode ";

            return await connection.QueryAsync<DiscountModeSetting>(selectquery, new { BranchCode = branchcode });
        }

        public async Task<bool> UpdateDiscountModeSettings(DiscountModeSetting settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE Tbl_DiscountSettingMode_Master SET
            DiscountRequired = COALESCE(@discountRequired, DiscountRequired)
            Where DiscId = @discId And BranchCode = @branchCode";

            var rows = await connection.ExecuteAsync(query, new { discountRequired = settings.DiscountRequired, branchCode = settings.BranchCode, discId = settings.DiscId });
            return rows > 0;
        }

        #endregion

        #region Sms Sender Setting

        public async Task<SmsSettingModel> GetSMSSenderSettings(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var qry = "Select SMSId, SMSPwd, SMSSenderId, SMSProvider, MobileNo, BackUpLocation, DBName," +
                " IsKotPrinter, IsHomeDelivery, IsCustomerEntry, EmailID, Password, IsSMS," +
                " IsMail, IsPriceShow, IsDescriptionShow, DayCloseGraceHour, BranchCode" +
                " From dbo.Tbl_SmsSender Where BranchCode = @Branch";

            return (await connection.QueryFirstOrDefaultAsync<SmsSettingModel>(qry, new { Branch = branchcode }));
        }

        public async Task<bool> SaveOrUpdateSMSSenderSettings(SmsSettingModel settings)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM Tbl_SmsSender  WHERE BranchCode = @BranchCode
            )
            BEGIN
            UPDATE Tbl_SmsSender SET 
            SMSId = COALESCE(@SMSId, SMSId),
            SMSPwd = COALESCE(@SMSPwd, SMSPwd),
            SMSSenderId = COALESCE(@SMSSenderId, SMSSenderId),
            SMSProvider = COALESCE(@SMSProvider, SMSProvider),
            MobileNo = COALESCE(@MobileNo, MobileNo),
            BackUpLocation = COALESCE(@BackUpLocation, BackUpLocation),
            DBName = COALESCE(@DBName, DBName),
            IsKotPrinter = COALESCE(@IsKotPrinter, IsKotPrinter),
            IsHomeDelivery = COALESCE(@IsHomeDelivery, IsHomeDelivery),
            IsCustomerEntry = COALESCE(@IsCustomerEntry, IsCustomerEntry),
            EmailID = COALESCE(@EmailID, EmailID),
            Password = COALESCE(@Password, Password),
            IsSMS = COALESCE(@IsSMS, IsSMS),
            IsMail = COALESCE(@IsMail, IsMail),
            IsPriceShow = COALESCE(@IsPriceShow, IsPriceShow),
            IsDescriptionShow = COALESCE(@IsDescriptionShow, IsDescriptionShow),
            DayCloseGraceHour = COALESCE(@DayCloseGraceHour, DayCloseGraceHour)
            WHERE BranchCode = @BranchCode
            END
            ELSE
            BEGIN
            INSERT INTO Tbl_SmsSender
            (SMSId, SMSPwd, SMSSenderId, SMSProvider, MobileNo, BackUpLocation, DBName, IsKotPrinter, IsHomeDelivery, IsCustomerEntry, EmailID, Password, IsSMS, IsMail, IsPriceShow, IsDescriptionShow, DayCloseGraceHour, BranchCode)
            VALUES
            (@SMSId, @SMSPwd, @SMSSenderId, @SMSProvider, @MobileNo, @BackUpLocation, @DBName, @IsKotPrinter, @IsHomeDelivery, @IsCustomerEntry, @EmailID, @Password, @IsSMS, @IsMail, @IsPriceShow, @IsDescriptionShow, @DayCloseGraceHour, @BranchCode)
            END";

            var rows = await connection.ExecuteAsync(query, settings);
            return rows > 0;
        }
        #endregion

        #region Printer Setting

        public async Task<IEnumerable<PrinterModel>> GetPrinterDetails(string branchcode, IDbConnection con)
        {
            var sql = @"SELECT PrinterName, IPAddress, Branch_Code FROM tmp_printer WHERE Branch_Code = @BranchCode";

            return await con.QueryAsync<PrinterModel>(sql, new { BranchCode = branchcode });
        }

        public async Task<IEnumerable<CartGroupModel>> GetCartGroupDetails(string branchcode, int OltCode, IDbConnection con)
        {
            var sql = @"SELECT Rno, CatGrp, Grp, OltCode, Branch_code FROM Tbl_CatGrp_Kot WHERE Branch_Code = @BranchCode AND OltCode = @oltcode ORDER BY CatGrp";

            return await con.QueryAsync<CartGroupModel>(sql, new { BranchCode = branchcode, oltcode = OltCode });
        }

        public async Task<IEnumerable<PrinterConfigModel>> GetPrinterConfigurations(string branchcode, int OltCode, IDbConnection con)
        {
            var sql = " SELECT PrinterName, BillType, Branch_Code, OltCode, PrintType, GrpCode, IPAddress, UserCode, UserName FROM PrinterNew " +
                " WHERE Branch_Code = @BranchCode AND OltCode = @oltcode And IsDeleted = 0";

            return await con.QueryAsync<PrinterConfigModel>(sql, new { BranchCode = branchcode, oltcode = OltCode });
        }

        public async Task<bool> SaveOrUpdateCatGroupSettings(CartGroupModel request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM Tbl_CatGrp_Kot WHERE Branch_Code = @Branch_Code And Grp = @Grp And OltCode = @OltCode
            )
            BEGIN
            UPDATE Tbl_CatGrp_Kot SET 
            CatGrp = COALESCE(@CatGrp, CatGrp ),
            OltCode = COALESCE(@OltCode, OltCode )
            WHERE Branch_Code = @Branch_Code And Grp = @Grp
            END
            ELSE
            BEGIN
            DECLARE @LastRec INT;
            SELECT @LastRec = ISNULL(MAX(Rno), 0) + 1 FROM Tbl_CatGrp_Kot WITH (HOLDLOCK, ROWLOCK) WHERE Branch_Code = @Branch_Code;

            INSERT INTO Tbl_CatGrp_Kot
            (Rno, CatGrp, Grp, Branch_Code, OltCode)
            VALUES
            (@LastRec, @CatGrp, @Grp, @Branch_Code, @OltCode);
            END";

            var rows = await connection.ExecuteAsync(query, request);
            return rows > 0;
        }

        public async Task<bool> SaveOrUpdatePrinterSettings(PrinterConfigModel request)
        {
            DateTime currentDatetime = ConvertUtcToIst();

            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            IF EXISTS (
            SELECT 1 FROM PrinterNew  WHERE Branch_Code = @branch_code And GrpCode = @grpcode And OltCode = @oltcode And IsDeleted = 0
            )
            BEGIN
            UPDATE PrinterNew SET 
            PrinterName = COALESCE(@printername, PrinterName),
            BillType = COALESCE(@billtype, BillType ),
            OltCode = COALESCE(@oltcode, OltCode ),
            PrintType = COALESCE(@printtype, PrintType ),
            IPAddress = COALESCE(@ipaddress, IPAddress ),
            ModifiedOn = COALESCE(@modifiedon, ModifiedOn ),
            ModifiedBy = COALESCE(@modifiedby, ModifiedBy )
            WHERE Branch_Code = @branch_code And GrpCode = @grpcode And IsDeleted = 0
            END
            ELSE
            BEGIN
            INSERT INTO PrinterNew
            (PrinterName, BillType, Branch_Code, OltCode, PrintType, GrpCode, IPAddress, UserCode, UserName)
            VALUES
            (@printername, @billtype, @branch_code, @oltcode, @printtype, @grpcode, @ipaddress, @usercode, @username)
            END";

            var rows = await connection.ExecuteAsync(query, new 
            {
                printername = request.PrinterName,
                billtype = request.BillType,
                branch_code = request.Branch_Code,
                oltcode = request.OltCode,
                printtype = request.PrintType,
                grpcode = request.GrpCode,
                ipaddress = request.IPAddress,
                usercode = request.UserCode,
                username = request.UserName,
                modifiedon = currentDatetime,
                modifiedby = request.UserCode
            });
            return rows > 0;
        }

        public async Task<bool> DeleteCatGroupSettings(int GrpCode, int OltCode, string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "DELETE FROM Tbl_CatGrp_Kot WHERE Grp = @grpcode AND OltCode = @oltcode AND Branch_code = @branchcode";

            var affectedRows = await connection.ExecuteAsync(query, new { grpcode = GrpCode, oltcode = OltCode, branchcode = Branchcode });
            return affectedRows > 0;
        }

        public async Task<bool> DeletePrinterSettings(int GrpCode, int OltCode, int UserCode, string Branchcode)
        {
            DateTime currentDatetime = ConvertUtcToIst();

            using var connection = _factory.CreateConnection(DbNames.POS);

            //var query = "DELETE FROM PrinterNew WHERE GrpCode = @GrpCode AND OltCode = @OltCode AND Branch_Code = @BranchCode";
            var query = "Update PrinterNew Set IsDeleted = 1, ModifiedBy = @usercode, ModifiedOn = @modifiedon WHERE GrpCode = @grpcode AND OltCode = @oltcode AND Branch_Code = @branchcode";

            var affectedRows = await connection.ExecuteAsync(query, new { grpcode = GrpCode, oltcode = OltCode, usercode = UserCode, branchcode = Branchcode, modifiedon = currentDatetime });
            return affectedRows > 0;
        }

        #endregion


        #region Bill Geneartion

        public async Task<bool> BillGeneration(BillGeneration model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            string sql = "";
            bool save = false;
            if (model.BillingType.ToLower() == "c")
            {
                sql = @"DELETE FROM Bill_Config; INSERT INTO Bill_Config(BilltType, Branch_Code, Config, IsReq) VALUES ('C', @BranchCode, 'BillNo', '1')";
            }
            else if (model.BillingType.ToLower() == "o")
            {
                sql = @"DELETE FROM Bill_Config; INSERT INTO Bill_Config (BilltType, Branch_Code, Config, IsReq) VALUES ('O', @BranchCode, 'BillNo', '1')";
            }
            else if (model.BillingType.ToLower() == "d")
            {
                sql = @" DELETE FROM Bill_Config; INSERT INTO Bill_Config (BilltType, Branch_Code, Config, IsReq,SubBillType) VALUES ('D', @BranchCode, 'BillNo', '1',@SubBillingType)";
            }
            int result = await connection.ExecuteAsync(sql, new { model.BranchCode, model.SubBillingType });
            return result > 0;
        }

        public async Task<IEnumerable<BillGeneration>> GetBillGenerationData(string Branch_Code)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = @"select Branch_Code AS BranchCode,BilltType AS BillingType,SubBillType AS SubBillingType from  Bill_Config  WHERE Branch_Code = @Branch_Code AND  BilltType IN ('C', 'O', 'D')";
            return (await connection.QueryAsync<BillGeneration>(sql, new { Branch_Code = Branch_Code }));
        }

        #endregion

        #region KotConfig

        public async Task<bool> KotConfiguration(KotConfig model)
        {

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            string deleteKot = @" DELETE FROM kot_config WHERE OltCode = @OltCode AND Branch_Code = @BranchCode";
            await connection.ExecuteAsync(deleteKot, new { model.OltCode, model.BranchCode }, transaction);

            string insertKot = @"INSERT INTO kot_config (OltCode, KotType, Branch_Code) VALUES (@OltCode, @KotType, @BranchCode)";
            int saved = await connection.ExecuteAsync(insertKot, new { model.OltCode, model.KotType, model.BranchCode }, transaction);

            //string deleteSplit = @"DELETE FROM kot_config_New WHERE OltCode = @OltCode AND Branch_Code = @BranchCode";
            //await connection.ExecuteAsync(deleteSplit, new { model.OltCode, model.BranchCode }, transaction);

            //if (model.SplitKot)
            //{
            //    string insertSplit = @" INSERT INTO kot_config_New (oltcode, KotType, Branch_Code)
            //                                     VALUES (@OltCode, @KotType, @BranchCode)";
            //    await connection.ExecuteAsync(insertSplit, new { model.OltCode, KotType = "SPLIT KOT", model.BranchCode }, transaction);
            //}
            transaction.Commit();
            return saved > 0;
        }

        public async Task<IEnumerable<KotConfig>> GetKotConfiguration(string Branch_Code)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = @"select OltCode as OltCode,KotType as KotType,Branch_Code as BranchCode from kot_config  WHERE Branch_Code = @Branch_Code";
            return (await connection.QueryAsync<KotConfig>(sql, new { Branch_Code = Branch_Code }));
        }

        #endregion

        #region SaveBillConfigModel

        public async Task<bool> BillConfiguration(SaveBillConfigModel model)
        {

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            string deleteQuery = @"DELETE FROM Bill_Config WHERE Config = 'REQBILL' AND Branch_Code = @BranchCode";
            await connection.ExecuteAsync(deleteQuery, new { model.BranchCode }, transaction);

            string insertQuery = @"INSERT INTO Bill_Config (BilltType, isreq, Branch_Code, SubBillType, Config) VALUES ('B', @ReqBill, @BranchCode, NULL, 'REQBILL')";
            int saved = await connection.ExecuteAsync(insertQuery, new { model.ReqBill, model.BranchCode }, transaction);

            transaction.Commit();
            return saved > 0;
        }

        public async Task<IEnumerable<SaveBillConfigModel>> GetBillConfiguration(string Branch_Code)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            var sql = @"select isreq as ReqBill,Branch_Code as BranchCode from  Bill_Config  WHERE Branch_Code = @Branch_Code and Config='REQBILL'";
            return (await connection.QueryAsync<SaveBillConfigModel>(sql, new { Branch_Code = Branch_Code }));
        }

        #endregion

        #region PosAndKotDataDelete
        public async Task<string> PosAndKotDataDelete()
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    string deleteQuery = @"DELETE FROM KOTMaster; DELETE FROM KOTDetails; DELETE FROM KOTSettlementMaster; DELETE FROM KOTSettlementDetails;
                           DELETE FROM KOTBillCancelation; DELETE FROM KOTBillSettlement; DELETE FROM KOTModifyDetails; DELETE FROM salestax;
                           DELETE FROM CompanyBillSettlement; DELETE FROM KOTBillTransfer; DELETE FROM itemdiscount; DELETE FROM splitBill;
                           DELETE FROM splitBill_Details; DELETE FROM BillTransferToCompany; DELETE FROM tabletransfer; DELETE FROM KOTCancelation;
                           DELETE FROM Stock_Updated_Restaurant; DELETE FROM BillModificationDetails; DELETE FROM Tbl_Kot_Display; DELETE FROM tmpKotPrint;
                           DELETE FROM TBL_SHIFTMASTER; DELETE FROM foodconversion; DELETE FROM RECEIPTMASTER; DELETE FROM RECEIPTDETAILS;
                           DELETE FROM NCKOTSettlementMaster; DELETE FROM NCKOTSettlementDetails;";
                    await connection.ExecuteAsync(deleteQuery, null, transaction);
                    transaction.Commit();
                    return "Pos and Kot Data Deleted Successfully";
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion


        #region Common Code

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

        public async Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            string str;
            int trno = 0;

            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                if (table_name == "ITEMMASTER")
                {
                    str = $@"select (MAX({column_name}), 0) as trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where SubItem = 0 And {condition_name} = @Branch";
                }
                else
                {
                    str = $@"select ISNULL(MAX({column_name}), 0) AS trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where {condition_name} = @Branch ";
                }

                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str, new { Branch = branch });

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

        #endregion
    }
}
