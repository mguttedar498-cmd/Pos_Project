namespace HMS_360_PMS.ProjectEntitiesModels.GeneralSettings
{
    #region Happy Hours Setting
    public class HappyHoursModel
    {
        //public int HappyId { get; set; } = 0;
        public bool InOrExOfTax { get; set; } = false;
        public bool HappyHours { get; set; } = false;
        public TimeSpan HHFrom { get; set; } = TimeSpan.Zero;
        public TimeSpan HHTo { get; set; } = TimeSpan.Zero;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region KOT Timer Setting
    public class TimerSettingModel
    {
        //public int TimerId { get; set; }
        public bool TimerRequired { get; set; } = false;
        public int? TimerMinute { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion


    #region Financial Year Setting

    public class FinancialYearModel
    {
        public int FinId { get; set; } = 0;
        public DateTime? FinFromDate { get; set; } = DateTime.MinValue;
        public DateTime? FinToDate { get; set; } = DateTime.MinValue;
        public int? FincurrentYear { get; set; } = 0;
        public int? FinEndYear { get; set; } = 0;
        public int? CurrentStatus { get; set; } = 0;
        public string LogUser { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public char? FinalClose { get; set; } = char.MinValue;
        public string FinCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region TaxMode Setting

    public class TaxModeSettingModel
    {
        public int TaxId { get; set; } = 0;
        public bool TaxRequired { get; set; } = false;
        public string TaxType { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    } 


    #endregion

    #region DiscountMode Setting

    public class DiscountModeSetting
    {
        public int DiscId { get; set; } = 0;
        public bool DiscountRequired { get; set; } = false;
        public string DiscountType { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region SMS Sender Setting

    public class SmsSettingModel
    {
        public string SMSId { get; set; } = string.Empty;
        public string SMSPwd { get; set; } = string.Empty;
        public string SMSSenderId { get; set; } = string.Empty;
        public string SMSProvider { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string BackUpLocation { get; set; } = string.Empty;
        public string DBName { get; set; } = string.Empty;
        public bool IsKotPrinter { get; set; } = false;
        public bool IsHomeDelivery { get; set; } = false;
        public bool IsCustomerEntry { get; set; } = false;
        public string EmailID { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsSMS { get; set; } = false;
        public bool IsMail { get; set; } = false;
        public bool IsPriceShow { get; set; } = false;
        public bool IsDescriptionShow { get; set; } = false;
        public int DayCloseGraceHour { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
    }

    #endregion

    #region Printer Setting

    public class PrintConfigResponse
    {
        //public List<string> Printers { get; set; }
        public List<PrinterModel> Printers { get; set; }
        public List<CartGroupModel> CartGroup { get; set; }
        public List<PrinterConfigModel> PrinterConfigurations { get; set; }

    }

    public class PrinterModel
    {
        public string PrinterName { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }

    public class CartGroupModel
    {
        public int Rno { get; set; } = 0;   
        public string CatGrp { get; set; } = string.Empty;
        public int Grp { get; set; } = 0;
        public string Branch_code { get; set; } = string.Empty;
        public int OltCode { get; set; } = 0;
    }


    public class PrinterConfigModel
    {
        public string PrinterName { get; set; } = string.Empty;
        public string BillType { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string OltCode { get; set; } = string.Empty;
        public string PrintType { get; set; } = string.Empty;
        public string GrpCode { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public string UserName { get; set; } = string.Empty;
    }

    //public class PrinterRequestModel
    //{
    //    public int Rno { get; set; }
    //    public string CatGrp { get; set; }
    //    public int Grp { get; set; }
    //    public string Branch_code { get; set; }
    //}

    #endregion

    #region Bill Geneartion 
    public class BillGeneration
    {
        public string BranchCode { get; set; } = string.Empty;
        public string BillingType { get; set; } = string.Empty; // C / O / D
        public string SubBillingType { get; set; } = string.Empty; // C / O / S (only for DayWise)
    }
    #endregion

    #region KotConfig
    public class KotConfig
    {
        public int OltCode { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
        public string KotType { get; set; } = string.Empty;// KOT or BILL
        public bool SplitKot => KotType?.Equals("SPLIT KOT", StringComparison.OrdinalIgnoreCase) == true;
    }
    #endregion

    #region SaveBillConfigModel
    public class SaveBillConfigModel
    {
        public int ReqBill { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
    }
    #endregion


    #region
    #endregion
    #region
    #endregion
}
