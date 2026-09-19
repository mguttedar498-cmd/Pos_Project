using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.ProjectEntitiesModels.KOT
{
    public class SmsRights
    {
        public decimal? RNo { get; set; }
        public int? SMSId { get; set; }
        public string Remark { get; set; }
        public bool? IsValid { get; set; }
        public DateTime? TrDate { get; set; }
    }

    //public class SmsSender
    //{
    //    public string SmsId { get; set; }
    //    public string SmsNo { get; set; }
    //    public string Password { get; set; }
    //    public string MobileNo { get; set; }
    //    public string SmsProvider { get; set; }
    //    public bool? IsPower { get; set; }
    //    public bool? IsHouseKeep { get; set; }
    //    public string AutoBackUp { get; set; }
    //    public DateTime? TrDate { get; set; }
    //    public bool? IsMenu { get; set; }
    //    public bool? DirectPrint { get; set; }
    //    public bool? IsPOS { get; set; }
    //    public bool? IsTele { get; set; }
    //    public bool? IsSMS { get; set; }
    //    public string RecHelp { get; set; }
    //    public bool? NoonCheckOut { get; set; }
    //    public bool? IsGroup { get; set; }
    //    public bool? IsGrpIn { get; set; }
    //    public bool? IsSplit { get; set; }
    //    public bool? IsDetailBILL { get; set; }
    //    public bool? ispending { get; set; }
    //    public bool? Planmaster { get; set; }
    //    public bool? Dashfloor { get; set; }
    //    public bool? SMS_SAMENo { get; set; }
    //    public bool? ContinusDiscount { get; set; }
    //    public bool? IsMail { get; set; }
    //    public bool? DoorLock { get; set; }
    //    public bool? IsBulkLink { get; set; }
    //}

    public class SmsSettings
    {
        public string SMSId { get; set; }
        public string SMSPwd { get; set; }
        public string SMSSenderId { get; set; }
        public string SMSProvider { get; set; }
        public string MobileNo { get; set; }

        public string BackUpLocation { get; set; }
        public string DBName { get; set; }

        public bool IsKotPrinter { get; set; }
        public bool IsHomeDelivery { get; set; }
        public bool IsCustomerEntry { get; set; }

        public string EmailID { get; set; }
        public string Password { get; set; }

        public bool IsSMS { get; set; }
        public bool IsMail { get; set; }
        public bool IsPriceShow { get; set; }
        public bool IsDescriptionShow { get; set; }

        public int DayCloseGraceHour { get; set; }
        public string BranchCode { get; set; }
    }

    public class PurcharserDetalis
    {
        public string HotelName { get; set; }
        public string Address { get; set; }
        public string TelePhone { get; set; }
        public string Mobile { get; set; }
        public string EmailId { get; set; }
        public string Fax { get; set; }
        public string WebSite { get; set; }
        public byte[] Logo { get; set; }
        public string States { get; set; }
        public string City { get; set; }
        public string Nation { get; set; }
        public DateTime? TrDate { get; set; }
        public string EPass { get; set; }
        public string PoliceEmail { get; set; }
    }

    public class DashboardModel
    {
        public int Vacant { get; set; }
        public int Occupied { get; set; }
        public int Dirty { get; set; }
        public int Blocked { get; set; }
        public int Un_Settel { get; set; }
        public int Management { get; set; }
        public decimal Occ { get; set; }
        public int TodayCheckin { get; set; }
        public int TodayCheckout { get; set; }
        public List<FloorMaster> Floors { get; set; }
        public List<RoomDetails> Rooms { get; set; }
    }

    public class FloorMaster
    {
        public decimal? RNo { get; set; }
        public DateTime? TrDate { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public string FloorDesc { get; set; }
        public int? MaxRoom { get; set; }
        public int? Alloted { get; set; }
        public int? Unalloted { get; set; }
        public string Status { get; set; }
        public string IpAdd { get; set; }
        public string Loger { get; set; }
    }

    public class RoomDetails
    {
        public decimal? RNo { get; set; }
        public DateTime? TrDate { get; set; }
        public string RoomNo { get; set; }
        public string RoomCode { get; set; }
        public string RoomDesc { get; set; }
        public int? PAX { get; set; }
        public int? EPAX { get; set; }
        public string Prioritys { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public string Status { get; set; }
        public string IpAdd { get; set; }
        public string Loger { get; set; }
        public string CurrentCheckIn { get; set; }
        public string GroupCode { get; set; }
    }


    #region Email Notfication

    public class EmailSenderModel
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseAuthentication { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public string SecurityType { get; set; }
    }

    public class EmailReceiver
    {
        public string EmailId { get; set; }
        public string BillType { get; set; }
        public string Bracnch_Code { get; set; }
    }
    public class PdfReportModel
    {
        public string ReportName { get; set; }
        public List<string> Headers { get; set; } = new();
        public List<List<string>> Rows { get; set; } = new();
        public List<string> TotalRow { get; set; } = new();
    }

    public class PdfChancesheetReportModel
    {
        public string ReportName { get; set; }

        public List<string> Headers { get; set; } = new();

        public List<List<string>> Rows { get; set; } = new();

        public ChanceSheetSummary OverallSummary { get; set; }

        public List<ChanceSheetRemarksSummary> RemarksSummary { get; set; }

        public List<OutletSummary> OutletWiseSummary { get; set; }
    }

    public class EmailNotficationModel
    {
        public string Branchcode { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string CCEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string EmailBody { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public DailySalesModelReport DailySalesData { get; set; }
        public ChanceSheetModelReport ChanceSheetData { get; set; }
        public VoidKotModelReport VoidKotModelData { get; set; }
        public NCKotModelReport NCKotModelData { get; set; }
        public GroupedItemSalesModelReport GroupedItemSalesData { get; set; }
        public KotCancellationModel KotCancellationModelData { get; set; }
        public BillCancellationModel BillCancellationModelData { get; set; }
        public KOTRegisterModel KOTRegisterModelData { get; set; }
        public DailySaleCategorywiseModel DailySaleCategorywiseModelData { get; set; }
        public CreditOutstandingModel CreditOutstandingModelData { get; set; }
    }

    public class DailySalesModelReport
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Outlet { get; set; } = string.Empty;
    }

    public class ChanceSheetModelReport
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Outlet { get; set; } = string.Empty;
    }

    public class VoidKotModelReport
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Outlet { get; set; } = string.Empty;
    }

    public class NCKotModelReport
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Outlet { get; set; } = string.Empty;
    }

    public class GroupedItemSalesModelReport
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Outlet { get; set; } = string.Empty;
    }

    #endregion
}
