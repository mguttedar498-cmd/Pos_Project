namespace HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS
{
    public class ReportOutletMaster
    {
        public int OltCode { get; set; } = 0;
        public string POSCode { get; set; } = string.Empty;
        public string OltName { get; set; } = string.Empty;
        public bool OltIsRoomService { get; set; } = true;
        public bool OltServiceTaxRequired { get; set; } = true;
        public string OltAddress1 { get; set; } = string.Empty;
        public string OltAddress2 { get; set; } = string.Empty;
        public string TaxCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public float? ServiceCharge { get; set; } = 0;
        public string branch_code { get; set; } = string.Empty;
        public bool? OltIsParcelService { get; set; } = false;
        public string isuploaded { get; set; } = string.Empty;
        public string ismodified { get; set; } = string.Empty;
        public string TinNo { get; set; } = string.Empty;
        public decimal? SBCess { get; set; } = 0;
        public float? KKCess { get; set; } = 0;
        public bool? InExTax { get; set; } = true;
        public bool ShowInUI { get; set; } = true;
    }

    public class TableModel
    {
        public int TableCode { get; set; } = 0;
        public string TableNo { get; set; } = string.Empty;
        public bool Occupy { get; set; } = false;
        public decimal Amount { get; set; } = 0;
        public int Pax { get; set; } = 0;

    }

    public class DailySalesModel
    {
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Now;
        public TimeSpan? BillTime { get; set; } = TimeSpan.Zero;
        public string TableNo { get; set; } = string.Empty;
        public decimal BillAmount { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal RoundOff { get; set; } = 0;
        public decimal CGST { get; set; } = 0;
        public decimal SGST { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string OltName { get; set; } = string.Empty;
    }

    public class SalesChartData
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Amount { get; set; } = 0;
    }

    public class OutletSaleData
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string Outlet { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
    }

    public class ReportItemGroup
    {
        public string GrpCode { get; set; } = string.Empty;
        public string GrpName { get; set; } = string.Empty;
        public string Dep { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string Isuploaded { get; set; } = string.Empty;
    }

    public class ItemSalesModel
    {
        public int GroupCode { get; set; } = 0;
        public string GroupName { get; set; } = string.Empty;
        //public DateTime BillDatetime { get; set; }
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public decimal Rate { get; set; } = 0;
        public decimal Qty { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public string OltName { get; set; } = string.Empty;
    }

    public class ChanceSheetModel
    {
        public string Date { get; set; }
        public TimeSpan BillTime { get; set; }
        public string BillNo { get; set; }
        public decimal ItemSale { get; set; }
        public decimal Tax { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal Dis { get; set; }
        public decimal Total { get; set; }
        public decimal Grand { get; set; }
        public decimal RoundOff { get; set; }
        public decimal Cash { get; set; }
        public decimal Card { get; set; }
        public decimal NEFT { get; set; }
        public decimal Pluxee { get; set; }
        public decimal Cheque { get; set; }
        public decimal UPI { get; set; }
        public decimal Online { get; set; }
        public decimal Credit { get; set; }
        public decimal Room { get; set; }
        public string KBSRefName { get; set; }
        public int oltcode { get; set; }
        public string OltName { get; set; }
        public string BranchCode { get; set; }
    }


    //public class ChanceSheetModel
    //{
    //    public string BillNo { get; set; } = string.Empty;
    //    public DateTime BillDate { get; set; } = DateTime.Now;
    //    public TimeSpan? BillTime { get; set; } = TimeSpan.Zero;
    //    public decimal Discount { get; set; } = 0;
    //    public decimal ItemSale { get; set; } = 0;
    //    public decimal Tax { get; set; } = 0;
    //    public decimal RoundOff { get; set; } = 0;
    //    public decimal CGST { get; set; } = 0;
    //    public decimal SGST { get; set; } = 0;
    //    public decimal Total { get; set; } = 0;
    //    public decimal Grand { get; set; } = 0;
    //    public decimal Cash { get; set; } = 0;
    //    public decimal Card { get; set; } = 0;
    //    public decimal UPI { get; set; } = 0;
    //    public decimal Online { get; set; } = 0;
    //    public decimal Cheque { get; set; } = 0;
    //    public decimal Credit { get; set; } = 0;
    //    public decimal RoomNo { get; set; } = 0;
    //    public string KBSRefName { get; set; } = string.Empty;
    //    public string OltName { get; set; } = string.Empty;
    //}

    public class OutletsaleModel
    {
        public string Sale { get; set; } = string.Empty;
        public decimal SaleAmt { get; set; } = 0;
        public string KSMBillDate { get; set; } = string.Empty;
        public int? OltCode { get; set; } = 0;
    }

    public class TableItemsModel
    {
        public int kotid { get; set; } = 0;
        public string itemname { get; set; } = string.Empty;
        public int qty { get; set; } = 0;
        public decimal rate { get; set; } = 0;
        public decimal total { get; set; } = 0;
    }

    public class VoidKotModel
    {
        public int KOTNO { get; set; } = 0;
        public DateTime KOTDate { get; set; } = DateTime.Now;
        public string KOTTime { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string StwName { get; set; } = string.Empty;
        public int ItemQty { get; set; } = 0;
        public int CancelQty { get; set; } = 0;
        public decimal CancelRate { get; set; } = 0;
        public string RefKotNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int OltCode { get; set; } = 0;
        public string OltName { get; set; } = string.Empty;

    }

    public class NCKotModel
    {
        public int KOTNO { get; set; } = 0;
        public DateTime KOTDate { get; set; } = DateTime.Now;
        public DateTime KOTTime { get; set; } = DateTime.Now;
        public string ItemName { get; set; } = string.Empty;
        public int KOTDQty { get; set; } = 0;
        public string NCKOT_Particulars { get; set; } = string.Empty;
        public decimal KOTDRate { get; set; } = 0;
        public int NCDepCode { get; set; } = 0;
        public string NCDepName { get; set; } = string.Empty;
        public int OltCode { get; set; } = 0;
        public string OltName { get; set; } = string.Empty;
        public decimal KOTTotal { get; set; } = 0;

    }


    public class KotCancellationModel
    {
        public string BranchCode { get; set; } = string.Empty;
        public bool IsAsOnDate { get; set; } = false;
        public bool IsBetweenDates { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BillingType { get; set; } = string.Empty;
        public string OutletCode { get; set; } = string.Empty;
        //public string SystemId { get; set; }
    }

    public class BillCancellationModel
    {
        public string BranchCode { get; set; } = string.Empty;
        public bool IsAsOnDate { get; set; } = false;
        public bool IsBetweenDates { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BillingType { get; set; } = string.Empty;
        public string OutletCode { get; set; } = string.Empty;
        //public string SystemId { get; set; }
        //public string ReportType { get; set; } = string.Empty;

    }
    public class CreditOutstandingModel
    {
        public string BranchCode { get; set; } = string.Empty;
        public bool IsAsOnDate { get; set; } = false;
        public bool IsBetweenDates { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BillingType { get; set; } = string.Empty;
        public string OutletCode { get; set; } = string.Empty;
        public string CompanyCode { get; set; } = string.Empty;

        //public string SystemId { get; set; }
        //public string ReportType { get; set; } = string.Empty;
    }
    public class DailySaleCategorywiseModel
    {
        public string BranchCode { get; set; } = string.Empty;
        public bool IsAsOnDate { get; set; } = false;
        public bool IsBetweenDates { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BillingType { get; set; } = string.Empty;
        public string OutletCode { get; set; } = string.Empty;
        public string CatCode { get; set; } = string.Empty;
        public string SubCatCode { get; set; } = string.Empty;
        public bool IsSubCategory { get; set; } = false;

        //public string SystemId { get; set; }
        //public string ReportType { get; set; } = string.Empty;
    }

    public class DailysaleCategorywiseReport
    {
        public string KSMBillNo { get; set; }
        public string OltCode { get; set; }
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public decimal KOTDRate { get; set; }
        public DateTime KSMBillDate { get; set; }
        public string GrpCode { get; set; }
        public string QPB { get; set; }
        public decimal Qty { get; set; }
        public string ItemType { get; set; }
        public string Branch_Code { get; set; }
        public string CatName { get; set; }
        public string KSMBillTime { get; set; }
    }

    public class KOTRegisterModel
    {
        public string BranchCode { get; set; } = string.Empty;
        public bool IsAsOnDate { get; set; } = false;
        public bool IsBetweenDates { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BillingType { get; set; } = string.Empty;
        public string OutletCode { get; set; } = string.Empty;
        public string TableNo { get; set; } = string.Empty;
        public bool IsPendingkot { get; set; } = false;
    }
}
