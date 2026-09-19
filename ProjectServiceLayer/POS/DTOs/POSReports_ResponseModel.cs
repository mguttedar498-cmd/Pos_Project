using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs
{

    public class GroupedItemSalesResponse
    {
        public string GroupName { get; set; }
        public List<ItemSalesModelResponse> Items { get; set; }
    }

    public class ItemSalesModelResponse
    {
        public string GroupName { get; set; }
        public string ItemName { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Total { get; set; }
        public string OutletName { get; set; }
    }

    public class ChanceSheetResponse
    {
        public List<ChanceSheetModel> Data { get; set; }
        public ChanceSheetSummary Summary { get; set; }
        public List<OutletSummary> OutletWiseSummary { get; set; }
        public List<ChanceSheetRemarksSummary> RemarksSummary { get; set; }
    }

    public class ChanceSheetSummary
    {
        public decimal Tax { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public decimal Grand { get; set; }
        public decimal RoundOff { get; set; }

        public decimal Cash { get; set; }
        public decimal Card { get; set; }
        public decimal UPI { get; set; }
        public decimal Online { get; set; }
        public decimal Cheque { get; set; }
        public decimal Credit { get; set; }
        public decimal NEFT { get; set; }
        public decimal Pluxee { get; set; }
        public decimal Cancelled { get; set; }
    }
    public class ChanceSheetRemarksSummary
    {
        public string Particulars { get; set; }
        public double Amount { get; set; }
    }

    //public class ChanceSheetPaymentSummary
    //{
    //    public decimal Cash { get; set; }
    //    public decimal Card { get; set; }
    //    public decimal UPI { get; set; }
    //    public decimal NEFT { get; set; }
    //    public decimal Pluxee { get; set; }
    //    public decimal Online { get; set; }
    //    public decimal Credit { get; set; }
    //    public decimal Cancelled { get; set; }
    //}

    public class KotCancelReportDto
    {
        public string KotNo { get; set; }
        public DateTime KotDate { get; set; }
        public string KotTime { get; set; }
        public decimal TotalAmount { get; set; }
        public string Outlet { get; set; }
    }

    public class OutletSummary
    {
        //public int OltCode { get; set; }
        public string OutletName { get; set; }
        public decimal TotalAmount { get; set; }
    }
    public class BillCancelReportDto
    {
        public string KSMBillNo { get; set; }
        public DateTime KSMBillDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class CreditOutstandingReportDto
    {
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
    }

    public class DailySaleCategorywiseResponseDto
    {
        public List<DailySaleCategorywiseItemDto> Items { get; set; }

        public DailySaleCategorywiseSummaryDto Summary { get; set; }
    }

    public class DailySaleCategorywiseItemDto
    {
        public string ItemName { get; set; }

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public string Category { get; set; }
    }

    public class DailySaleCategorywiseSummaryDto
    {
        public decimal TotalQuantity { get; set; }

        public decimal TotalTax { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal GrandAmount { get; set; }
    }

    public class KOTRegisterResponseDto
    {
        public string KOTNO { get; set; }
        public string IssueTime { get; set; }
        public string ItemName { get; set; }
        public decimal Qty { get; set; }
        public string UserId { get; set; }
        public string Steward { get; set; }
        public string TableNo { get; set; }
        public decimal TotalAmount { get; set; }
        public string BillNo { get; set; }
    }

    public class DashboardSummaryDto
    {
        public decimal DailySalesTotal { get; set; }

        public decimal MonthlySalesTotal { get; set; }

        public decimal YearlySalesTotal { get; set; }
    }
}
