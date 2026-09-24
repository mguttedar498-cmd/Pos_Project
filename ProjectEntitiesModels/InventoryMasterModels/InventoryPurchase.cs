namespace HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels
{
    public class InventoryPurchase
    {
        public PurchaseOrderMasterModel Master { get; set; } = new();
        public List<PurchaseOrderDetailModel> Details { get; set; } = new();
        public List<PurchaseOrderTaxModel> Taxes { get; set; } = new();
        public List<PurchaseOrderMiscModel> Miscellaneous { get; set; } = new();
    }
    public class PurchaseOrderMasterModel
    {
        public int PONo { get; set; } = 0;
        public DateTime PODate { get; set; } = DateTime.Now;
        public int SupCode { get; set; } = 0;
        public string Billed { get; set; } = "N";
        public string Branch_Code { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime EffectiveTo { get; set; } = DateTime.Now;
        public string Instruction { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public int StoreId { get; set; } = 0;
        public string status { get; set; } = string.Empty;
        public DateTime POValidDate { get; set; }= DateTime.Now;
        public DateTime Deliverydate { get; set; } = DateTime.Now;
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ApprovedDate { get; set; } = DateTime.Now;
        public bool IsApproved { get; set; } = false;

    }
    public class PurchaseOrderDetailModel
    {
        public int PONo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public decimal POItemQty { get; set; } = 0;
        public decimal POOrderQty { get; set; } = 0;
        public decimal POItemRate { get; set; }= 0;
        public string Branch_Code { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public decimal POItemSuplyQty { get; set; } = 0;
        public decimal CPOItemQty { get; set; } = 0;
        public string? ApprovedBy { get; set; }= string.Empty;
        public DateTime? ApprovedDate { get; set; }= DateTime.Now;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public decimal ReceivedQty { get; set; } = 0;
        public decimal BalanceQty { get; set; } = 0;
        public decimal FinalApprovedQty { get; set; } = 0;
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
    }
    public class PurchaseOrderTaxModel
    {
        public int Pno { get; set; } = 0;
        public int PRNo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public int TaxCode { get; set; } = 0;
        public decimal TaxPer { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0; 
        public string Branch_Code { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public string TaxPercentage { get; set; } = string.Empty;
    }
    public class PurchaseOrderMiscModel
    {
        public int ChargeId { get; set; } = 0;
        public decimal ChargeAmt { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public int Pno { get; set; } = 0;
        public int PRNo { get; set; } = 0;
        public int TaxCode { get; set; } = 0;
        public string TaxDescription { get; set; } = string.Empty;
        public string TaxPercentage { get; set; } = string.Empty;
        public string ChargeName { get; set; } = string.Empty;
    }
    public class PurchaseOrderListResponse
    {
        public PurchaseOrderMasterModel Master { get; set; } = new();
        public List<PurchaseOrderDetailModel> Details { get; set; } = new();
        public List<PurchaseOrderTaxModel> Taxes { get; set; } = new();
        public List<PurchaseOrderMiscModel> Miscellaneous { get; set; } = new();
    }
    public class PurchaseOrderPrintResponse
    {
        public PurchaseOrderPrintMaster Master { get; set; } = new();
        public List<PurchaseOrderPrintDetail> Details { get; set; } = new();
        public List<PurchaseOrderTaxDetail> TaxDetails { get; set; } = new();
        public List<TermsAndConditionsMaster> TermsMaster { get; set; } = new();
    }
    public class PurchaseOrderPrintMaster
    {
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string TinNo { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int PONo { get; set; } = 0;
        public DateTime PODate { get; set; } = DateTime.Now;
        public string OrderBy { get; set; } = string.Empty;
        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public DateTime EffectiveTo { get; set; } = DateTime.Now;
        public string Instruction { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = 0;
        public decimal Tax { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
    }
    public class PurchaseOrderPrintDetail
    {
        public int Rno { get; set; } = 0;
        public int PONo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal ItemRate { get; set; } = 0;
        public decimal ItemQty { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public decimal ApprovedTotal { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public decimal POOrderQty { get; set; } = 0;
        public string? ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; } = DateTime.MinValue;
        public string? MainUnit { get; set; } = string.Empty;
        public string? MainUnitConverstion { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;

    }
    public class PurchaseOrderTaxDetail
    {
        public int TaxDescId { get; set; } = 0;
        public int Pno { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public int TaxCode { get; set; } = 0;
        public decimal TaxPer { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; } = 0;
    }

    #region Purchase Order Calcuations
    public class PurchaseOrderSubmitModel
    {
        public int PONo { get; set; } = 0;
        public int StoreId { get; set; } = 0;
        public string Branch { get; set; } = string.Empty;
        public double Discount { get; set; } = 0;
        public string DiscountIn { get; set; } = string.Empty;
        public List<PODetailsModel> PODetail { get; set; }= new List<PODetailsModel>();
        public List<POMiscModel> POMiscDetail { get; set; }= new List<POMiscModel>();
    }
    public class POMiscModel
    {
        public double MiscCharge { get; set; } = 0;
        public int MiscChargeCode { get; set; } = 0;
        public string MiscTaxCode { get; set; } = string.Empty;
    }
    public class POMiscTax
    {
        public int ChargeId { get; set; } = 0;
        public double ChargeAmt { get; set; } = 0;
        public int MiscChargeCode { get; set; } = 0;
        public string MiscTaxCode { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public double Taxper { get; set; } = 0;
        public double MiscTotalAmount { get; set; } = 0;
        public double TaxAmount { get; set; } = 0;
        public double Total { get; set; } = 0;
        public double CGST { get; set; } = 0;
        public double SGST { get; set; } = 0;
    }
    public class PODetailsModel
    {
        public int ItemCode { get; set; } = 0;
        public decimal POItemQty { get; set; } = 0;
        public decimal POItemRate { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public decimal POItemSuplyQty { get; set; } = 0;
        public decimal CPOItemQty { get; set; } = 0;
    }
    public class PurchaseOrderSubmitTax
    {
        public double TotalAmount { get; set; } = 0;
        public decimal TotalQty { get; set; } = 0;
        public double CGSTPer { get; set; } = 0;
        public double CGSTAmt { get; set; } = 0;
        public double SGSTPer { get; set; } = 0;
        public double SGSTAmt { get; set; } = 0;
        public double ServiceChargePer { get; set; } = 0;
        public double ServiceCharge { get; set; } = 0;
        public double TaxAmount { get; set; } = 0;
        public double GrandTotal { get; set; } = 0;
        public double DiscountPer { get; set; } = 0;
        public double Discount { get; set; } = 0;
        public string DiscountIn { get; set; } = string.Empty;
        public string DiscountRemarks { get; set; } = string.Empty;
        public double RoundOff { get; set; } = 0;
        public double MiscCharge { get; set; } = 0;
        public int MiscChargeCode { get; set; } = 0;
        public string MiscTaxCode { get; set; } = string.Empty;
        public double MiscCGSTPer { get; set; } = 0;
        public double MiscCGSTAmt { get; set; } = 0;
        public double MiscSGSTPer { get; set; } = 0;
        public double MiscSGSTAmt { get; set; } = 0;
        public double MiscTaxAmount { get; set; } = 0;
        public double MiscTotalAmount { get; set; } = 0;
        public List<POTax> TaxList { get; set; } = new List<POTax>();
        public List<POMiscTax> MiscTaxList { get; set; } = new List<POMiscTax>();
    }
    public class POTax
    {
        public int ItemCode { get; set; } = 0;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxName { get; set; } = string.Empty;
        public double Taxper { get; set; } = 0;
        public double POTotalAmount{ get; set; } = 0;
        public double TaxAmount { get; set; } = 0;
        public double Total { get; set; } = 0;
        public double CGST { get; set; } = 0;
        public double SGST { get; set; } = 0;
    }
    public class PurchaseExtraChargeModel
    {
        public int ChargeCode { get; set; } = 0;
        public string ChargeName { get; set; } = string.Empty;
        public int ItemCode { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public DateTime? LastModify { get; set; }
        public int StoreId { get; set; } = 0;
        public int TaxDescId { get; set; } = 0;
        public string TaxCode { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public double TaxPercentage { get; set; } = 0.0;
        public int IsActive { get; set; } = 0;
        public int UserCode { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
        public DateTime? CreatedOn { get; set; } = DateTime.Now;
    }
    #endregion

    #region GRN
    public class InventoryPurchaseGRN
    {
        public int PONo { get; set; }
        public DateTime PODate { get; set; }
        public int SupCode { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public string GRNNo { get; set; } = string.Empty;
        public DateTime GRNDate { get; set; }
        public string GRNTime { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string BillNo { get; set; } = string.Empty;
        public string InspectedBy { get; set; } = string.Empty;
        public int StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal RoundOff { get; set; }
        public decimal NetAmount { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal MissChargeAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public string status { get; set; } = string.Empty;
        public DateTime POValidDate { get; set; } = DateTime.Now;
        public List<GRNDetailRequest> Details { get; set; } = new();
        public List<GRNTaxRequest> Taxes { get; set; } = new();
        public List<GRNMiscRequest> Miscellaneous { get; set; } = new();
    }

    public class GRNDetailRequest
    {
        public int PONo { get; set; }
        public int ItemCode { get; set; }
        public decimal POItemQty { get; set; }
        public decimal POOrderQty { get; set; }
        public decimal POItemRate { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public decimal POItemSuplyQty { get; set; }
        public decimal CPOItemQty { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal BalanceQty { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime? ApprovedDate { get; set; }
        public decimal FinalQty { get; set; } = 0;
        public string? MainUnitConverstion { get; set; } = string.Empty;
        public string? MainUnit { get; set; } = string.Empty;
    }

    public class GRNTaxRequest
    {
        public int Pno { get; set; }
        public int ItemCode { get; set; }
        public int TaxCode { get; set; }
        public decimal TaxPer { get; set; }
        public decimal TaxAmount { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
    }

    public class GRNMiscRequest
    {
        public int ChargeId { get; set; }
        public decimal ChargeAmt { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public int Pno { get; set; }
        public int TaxCode { get; set; }
    }

    public class PurchaseOrderPonoRequest
    {
        public int PONo { get; set; } = 0;
        public string status { get; set; } = string.Empty;
    }
    public class GRNMasterModel
    {
        public string VendorName { get; set; }=string.Empty;
        public string VendorAddress { get; set; }=string.Empty;
        public string PhoneNo { get; set; }=string.Empty;
        public string MobileNo { get; set; }=string.Empty;
        public string GSTNo { get; set; }=string.Empty;
        public string TinNo { get; set; }=string.Empty;
        public string StateCode { get; set; }=string.Empty;
        public int PONo { get; set; } = 0;
        public DateTime? PODate { get; set; }= DateTime.Now;
        public string SupCode { get; set; }=string.Empty;
        public DateTime? POValidDate { get; set; } = DateTime.Now;
        public string Billed { get; set; }=string.Empty;
        public string Branch_Code { get; set; }=string.Empty;
        public string OrderBy { get; set; }=string.Empty;
        public string status { get; set; }=string.Empty;
        public string? GRNNo { get; set; } = string.Empty;
        public DateTime? GRNDate { get; set; }
        public string GRNtime { get; set; }=string.Empty;
        public string Receivedby { get; set; }=string.Empty; 
        public string Reason { get; set; } = string.Empty;
        public string InspectedBy { get; set; } = string.Empty;
        public int? StoreID { get; set; } = 0;
        public string StoreName { get; set; } = string.Empty;
        public string userId { get; set; } = string.Empty;
        public string ipAdd { get; set; } = string.Empty;
        public decimal? TotalAmount { get; set; } = 0;
        public decimal? TotTax { get; set; } = 0;
        public decimal? OtherCharges { get; set; } = 0;
        public decimal? Roundoff { get; set; } = 0;
        public decimal? NetAmount { get; set; } = 0;
        public decimal? MissChargeAmount { get; set; } = 0;
        public decimal? CgstAmount { get; set; } = 0;
        public decimal? SgstAmount { get; set; } = 0;
    }
    public class GRNDetailsModel
    {
        public int Rno { get; set; }
        public int? PONo { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public decimal? POItemQty { get; set; }
        public decimal? POItemRate { get; set; }
        public decimal? POItemSuplyQty { get; set; }
        public decimal? CPOItemQty { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string GRNNo { get; set; } = string.Empty;
        public int? UnitCode { get; set; }
        public decimal? BalanceQty { get; set; }
        public decimal? ReceivedQty { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal ReceivedQtyTotal { get; set; }
        public decimal BalanceQtyTotal { get; set; }
        public decimal FinalQty { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
    }
    public class GRNTaxDescriptionModel
    {
        public int TaxDescId { get; set; }
        public int Pno { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public int? TaxCode { get; set; }
        public decimal? TaxPer { get; set; }
        public decimal? TaxAmount { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public string TaxPercentage { get; set; } = string.Empty;
        public string GRNNo { get; set; } = string.Empty;
    }
    public class GRNMiscResponse
    {
        public int ChargeId { get; set; }
        public decimal ChargeAmt { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public int Pno { get; set; }
        public int TaxCode { get; set; }
        public string GRNNo { get; set; }
        public string TaxDescription { get; set; } = string.Empty;
        public decimal TaxPercentage { get; set; }
        public string ChargeName { get; set; } = string.Empty;
    }
    public class GoodsReceivedNotePrintResponse
    {
        public GRNMasterModel Master { get; set; } = new();
        public List<GRNDetailsModel> Details { get; set; } = new();
        public List<GRNTaxDescriptionModel> TaxDetails { get; set; } = new();
        public List<TermsAndConditionsMaster> TermsMaster { get; set; } = new();
    }
    public class GoodsReceivedNoteListResponse
    {
        public GRNMasterModel Master { get; set; } = new();
        public List<GRNDetailsModel> Details { get; set; } = new();
        public List<GRNTaxDescriptionModel> TaxDetails { get; set; } = new();
        public List<GRNMiscResponse> MiscDetails { get; set; } = new();
    }
    #endregion

    #region Purchase Order
    public class PurchaseSaveRequest
    {
        public int PNo { get; set; }
        public int PONo { get; set; }
        public DateTime PDate { get; set; }
        public int SupCode { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal PTotalAmount { get; set; }
        public string BillNo { get; set; } = string.Empty;
        public decimal TaxAmount { get; set; }
        public decimal RoundOff { get; set; }
        public decimal Misc { get; set; }
        public decimal Discount { get; set; }
        public int DepCode { get; set; }
        public string PType { get; set; } = string.Empty;
        public bool DirectIssue { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string StoredId { get; set; } = string.Empty;
        public decimal MissChargeAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public string GrnNo { get; set; } = string.Empty;
        public List<PurchaseDetailRequest> Details { get; set; } = new();
        public List<PurchaseTaxRequest> Taxes { get; set; } = new();
        public List<PurchaseMiscRequest> MiscDetails { get; set; } = new();
    }
    public class PurchaseDetailRequest
    {
        public int ItemCode { get; set; }
        public decimal PItemQty { get; set; }
        public decimal PItemRate { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal QtyPer { get; set; }
        public decimal NoOfQty { get; set; }
        public decimal TotalQty { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoredId { get; set; } = string.Empty;
        public int NoOfDays { get; set; }
        public int UnitCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal TrowQty { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
    }
    public class PurchaseTaxRequest
    {
        public int ItemCode { get; set; }
        public int TaxCode { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TaxPer { get; set; }
    }
    public class PurchaseMiscRequest
    {
        public int TaxCode { get; set; }
        public int ChargeId { get; set; }
        public string ChargeCode { get; set; } = string.Empty;
        public string ChargeAmount { get; set; } = string.Empty;
    }

    public class PurchaseSaveListRequest
    {
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string TinNo { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int PNo { get; set; } = 0;
        public DateTime PDate { get; set; }=DateTime.Now;
        public int SupCode { get; set; } = 0;
        public decimal? PTotalAmount { get; set; } = 0;
        public string? BillNo { get; set; } = string.Empty;
        public decimal? TaxAmount { get; set; } = 0;
        public int? DIssue { get; set; } = 0;
        public int? INo { get; set; } = 0;
        public string? DepCode { get; set; } = string.Empty;
        public double? Netamount { get; set; }
        public double? Discount { get; set; }
        public string? PType { get; set; }
        public string? BranchCode { get; set; }
        public decimal? RoundOff { get; set; }
        public decimal? Misc { get; set; }
        public string? UserCode { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public decimal? MissChargeAmount { get; set; }
        public decimal? SgstAmount { get; set; }
        public decimal? CgstAmount { get; set; }
        public string? StoreId { get; set; } = string.Empty;
        public string? StoreName { get; set; } = string.Empty;
    }
    public class PurchaseOrderDetailListResponse
    {
        public int Rno { get; set; } = 0;
        public int? PNo { get; set; }
        public int? PONo { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public float? PItemQty { get; set; }
        public double? PItemRate { get; set; }
        public float? PItemReturnQty { get; set; }
        public string? Unit { get; set; }
        public double? QtyPer { get; set; }
        public double? NoOfQty { get; set; }
        public int? SrNo { get; set; }
        public double? TotalQty { get; set; }
        public double? Qty { get; set; }
        public decimal? PerRate { get; set; }
        public double? PIRNoQty { get; set; }
        public float? PIQty { get; set; }
        public decimal? PRate { get; set; }
        public double? Tax { get; set; }
        public double? TotAmt { get; set; }
        public double? Pegs { get; set; }
        public string? BranchCode { get; set; }
        public int? NoOfDays { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? TrowQty { get; set; }
        public DateTime? TrowDate { get; set; }
        public string? StoredId { get; set; }
        public decimal Total { get; set; }
        public string? ItemName { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public decimal PAQ { get; set; } = 0;
        public string? StoreId { get; set; } = string.Empty;
        public string? StoreName { get; set; } = string.Empty;

    }
    public class PurchaseTaxModel
    {
        public int Pno { get; set; } = 0;
        public int PRNo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public int TaxCode { get; set; } = 0;
        public decimal TaxPer { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string TaxDescription { get; set; } = string.Empty;
        public string TaxPercentage { get; set; } = string.Empty;
    }
    public class PurchaseMiscModel
    {
        public int ChargeId { get; set; } = 0;
        public decimal ChargeAmt { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public int Pno { get; set; } = 0;
        public int PRNo { get; set; } = 0;
        public int TaxCode { get; set; } = 0;
        public string TaxDescription { get; set; } = string.Empty;
        public string TaxPercentage { get; set; } = string.Empty;
        public string ChargeName { get; set; } = string.Empty;
    }
    public class PurchaseOrderList
    {
        public PurchaseSaveListRequest Master { get; set; } = new();
        public List<PurchaseOrderDetailListResponse> Details { get; set; } = new();
        public List<PurchaseTaxModel> TaxDetails { get; set; } = new();
        public List<PurchaseMiscModel> MiscDetails { get; set; } = new();
    }
    public class PurchaseOrderGRNNUmberRequest
    {
        public string GRNNumber { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }
    #endregion

    #region Purchase Return
    public class PurchaseOrderRequestNumber
    {
        public string PNo { get; set; } = string.Empty;
        public string PurchaseType { get; set; } = string.Empty;
    }
    public class PurchaseReturnSaveRequest
    {
        public int TransactionNo { get; set; }
        public int PRNo { get; set; }
        public DateTime PRDate { get; set; }
        public int SupCode { get; set; }
        public int PNo { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public decimal? TotalAmount { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? GrossAmount { get; set; }
        public decimal? MissChargeAmount { get; set; }
        public decimal? CgstAmount { get; set; }
        public decimal? SgstAmount { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public List<PurchaseReturnDetailRequest> Details { get; set; } = new();
        public List<PurchaseOrderTaxModel> Taxes { get; set; } = new();
        public List<PurchaseOrderMiscModel> Miscellaneous { get; set; } = new();
    }

    public class PurchaseReturnDetailRequest
    {
        public int ItemCode { get; set; }
        public decimal PRItemRate { get; set; }
        public decimal PRItemQty { get; set; }
        public decimal PRNIQty { get; set; }
        public decimal PReturnQty { get; set; }
        public decimal PRAQty { get; set; } 
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
    }
    public class PurchaseOrderReturnList
    {
        public PurchaseSaveListRequest Master { get; set; } = new();
        public List<PurchaseOrderReturnDetailList> Details { get; set; } = new();
        public List<PurchaseTaxModel> TaxDetails { get; set; } = new();
        public List<PurchaseMiscModel> MiscDetails { get; set; } = new();
    }
    public class PurchaseOrderReturnDetailList
    {
        public int Rno { get; set; } = 0;
        public int? PNo { get; set; }
        public int? PONo { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public float? PItemQty { get; set; }
        public double? PItemRate { get; set; }
        public float? PItemReturnQty { get; set; }
        public string? Unit { get; set; }
        public int UnitCode { get; set; } = 0;
        public double? QtyPer { get; set; }
        public double? NoOfQty { get; set; }
        public int? SrNo { get; set; }
        public double? TotalQty { get; set; }
        public double? Qty { get; set; }
        public decimal? PerRate { get; set; }
        public double? PIRNoQty { get; set; }
        public float? PIQty { get; set; }
        public decimal? PRate { get; set; }
        public double? Tax { get; set; }
        public double? TotAmt { get; set; }
        public double? Pegs { get; set; }
        public string? BranchCode { get; set; }
        public int? NoOfDays { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? TrowQty { get; set; }
        public DateTime? TrowDate { get; set; }
        public string? StoredId { get; set; }
        public decimal Total { get; set; }
        public string? ItemName { get; set; }
        public decimal ReamingQty { get; set; }
        public decimal DamageQty { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
    }
    public class PurchaseOrderReturnListResponse
    {
        public PurchaseReturnMasterModel Master { get; set; } = new();
        public List<PurchaseReturnDetailModel> Details { get; set; } = new();
        public List<PurchaseTaxModel> TaxDetails { get; set; } = new();
        public List<PurchaseMiscModel> MiscDetails { get; set; } = new();
    }
    public class PurchaseReturnMasterModel
    {
        public string VendorName { get; set; } = string.Empty;
        public string VendorAddress { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string TinNo { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int PRNo { get; set; }
        public DateTime PRDate { get; set; }
        public int SupCode { get; set; }
        public decimal PRTotalAmount { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal MissChargeAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
        public string TransactionNo { get; set; } = string.Empty;
        public int PNo { get; set; }
    }
    public class PurchaseReturnDetailModel
    {
        public int Rno { get; set; }
        public int PRNo { get; set; }
        public int PNo { get; set; }
        public int ItemCode { get; set; }
        public decimal PRItemQty { get; set; }
        public decimal PRItemRate { get; set; }
        public decimal PRNIQty { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public decimal PReturnQty { get; set; }
        public decimal PRAQty { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public decimal RemainingQty { get; set; }
        public decimal DamageQty { get; set; }
        public int TaxCode { get; set; } = 0;
        public string TaxName { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
    }
    #endregion

    #region  Item Damage Entry
    public class PurchaseDetailResponse
    {
        public int PNo { get; set; }
        public int ItemCode { get; set; }
        public string Supplier { get; set; } = string.Empty;
        public int SupplierCode { get; set; } = 0;
        public decimal PurQty { get; set; }
        public decimal Rate { get; set; }
        public decimal AvailQty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public bool IsAlreadyIssued { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal RetQty { get; set; }
        public decimal DamageQty { get; set; }
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string MainUnit { get; set; } = string.Empty;
    }

    public class ItemDamageSaveRequest
    {
        public int DNo { get; set; }
        public DateTime DDate { get; set; }
        public decimal DTotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public string BranchCode { get; set; }=string.Empty;
        public List<ItemDamageDetailRequest> Details { get; set; } = new List<ItemDamageDetailRequest>();
    }
    public class PurchaseQuantityModel
    {
        public decimal PItemQty { get; set; }
        public decimal PItemReturnQty { get; set; }
        public decimal PAQ { get; set; }
    }

    public class ItemDamageDetailRequest
    {
        public int DNo { get; set; }
        public int ItemCode { get; set; }
        public decimal DItemRate { get; set; }
        public decimal DItemQty { get; set; }
        public decimal ItemQty { get; set; }
        public decimal ItemBalQty { get; set; }
        public int PNo { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string MainUnit { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
    }
    public class ItemDamageListResponse
    {
        public int DNo { get; set; }
        public DateTime DDate { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public decimal DTotalAmount { get; set; }
        public int ItemCode { get; set; }
        public string ItemName { get; set; }=string.Empty;
        public int PNo { get; set; }
        public decimal ItemQty { get; set; }
        public decimal DItemQty { get; set; }
        public decimal DItemRate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal MissChargeAmount { get; set; }
        public decimal CgstAmount { get; set; }
        public decimal SgstAmount { get; set; }
    }
    public class ItemDamageListResponselist
    {
        public ItemDamageListResponse Master { get; set; } = new();
        public List<ItemDamageDetailRequest> Details { get; set; } = new();
    }
    #endregion

    #region Indent Order
    public class GetItemDetailsRequest
    {
        public int ItemCode { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
    }
    public class ItemDetailsResponse
    {
        public int PNo { get; set; } = 0;
        public int PONo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public decimal ItemRate { get; set; }
        public int UnitCode { get; set; } = 0;
        public string UnitName { get; set; } = string.Empty;
        public decimal AvailableQty { get; set; }
        public decimal PItemQty { get; set; }
        public decimal PItemReturnQty { get; set; }
        public decimal DamageQty { get; set; }
        public string StoredId { get; set; } = string.Empty;
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string Branch_Code { get; set; } = string.Empty;
        public string StockSource { get; set; } = string.Empty;
        public int StockReferenceNo { get; set; } = 0;
    }

    public class IndentOrderSaveRequest
    {
        public int IONo { get; set; }
        public string Billed { get; set; } = string.Empty;
        public DateTime IODate { get; set; }
        public DateTime POValidDate { get; set; }=DateTime.Now;
        public string StoreCode { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public string DepCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public string StoreId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<IndentOrderSaveItem> Items { get; set; } = new();
    }

    public class IndentOrderSaveItem
    {
        public int PNo { get; set; }
        public int IONo { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public string ItemName { get; set; } = string.Empty;
        public decimal IOItemQty { get; set; } = 0;
        public decimal IOItemRate { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string MainUnit { get; set; } = string.Empty;
        public decimal IOAvailableQty { get; set; } = 0;
        public decimal IOOrginalQty { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public decimal ReamingQty { get; set; } = 0;
        public decimal ApprovedQty { get; set; } = 0;
        public int StockReferenceNo { get; set; } = 0;
        public string StockSource { get; set; } = string.Empty;
    }
    public class IndentOrderMasterList
    {
        public string PurchaseNo { get; set; } = string.Empty;
        public int IONo { get; set; }
        public string Billed { get; set; } = string.Empty;
        public DateTime IODate { get; set; }
        public DateTime POValidDate { get; set; } = DateTime.Now;
        public string StoreCode { get; set; } = string.Empty;
        public string OrderBy { get; set; } = string.Empty;
        public string DepCode { get; set; } = string.Empty;
        public string DepName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public string StoreId { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    public class IndentOrderListResponse
    {
        public IndentOrderMasterList Master { get; set; } = new();
        public List<IndentOrderSaveItem> Details { get; set; } = new();
    }
    #endregion

    #region Indent Order Approval
    public class IndentOrderApprovalSaveRequest
    {
        public int IONo { get; set; }
        public DateTime IODate { get; set; }
        public DateTime POValidDate { get; set; } = DateTime.Now;
        public DateTime ApprovedDate { get; set; } = DateTime.Now;
        public int? SupCode { get; set; }
        public string? Billed { get; set; }
        public string? Branch_Code { get; set; }
        public string? OrderBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? DepCode { get; set; }
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public string StoreId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<IndentOrderApprovalItemRequest> Items { get; set; } = new();
    }

    public class IndentOrderApprovalItemRequest
    {
        public int PNo { get; set; }
        public int IONo { get; set; }
        public int ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public int? UnitCode { get; set; }
        public decimal IOItemQty { get; set; }
        public decimal IOItemRate { get; set; }
        public decimal ApprovedQty { get; set; }
        public string? BranchCode { get; set; }
        public string? MainUnitConverstion { get; set; }
        public string? MainUnit { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal OrginalQty { get; set; }
        public decimal IndentQty { get; set; }
        public string StockSource { get; set; } = string.Empty;
        public int StockReferenceNo { get; set; } = 0;
        public int ReamingQty { get; set; } = 0;

    }
    public class IndentOrderApprovalModel
    {
        public string? PurchaseNo { get; set; } = string.Empty;
        public int IONo { get; set; }
        public DateTime IODate { get; set; }
        public DateTime POValidDate { get; set; } = DateTime.Now;
        public DateTime ApprovedDate { get; set; } = DateTime.Now;
        public int? SupCode { get; set; }
        public string? Billed { get; set; }
        public string? Branch_Code { get; set; }
        public string? OrderBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? DepCode { get; set; }
        public decimal CgstAmount { get; set; } = 0;
        public decimal SgstAmount { get; set; } = 0;
        public decimal MissChargeAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; } = 0;
        public decimal TaxAmount { get; set; } = 0;
        public decimal GrossAmount { get; set; } = 0;
        public string StoreId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    public class IndentOrderApprovalListDto
    {
        public IndentOrderApprovalModel Master { get; set; } = new();
        public List<IndentOrderApprovalItemRequest> Details { get; set; } = new();
    }
    #endregion

    #region Item Issue
    public class IndentOrderSearchResponse
    {
        public int IONo { get; set; }
    }
    public class ItemIssueSaveRequest
    {
        public string TrasnsactionNo { get; set; } = string.Empty;
        public int INo { get; set; }
        public DateTime IssueDate { get; set; }
        public int DepCode { get; set; }
        public decimal TotalAmount { get; set; }
        public int BillNo { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public int UserCode { get; set; }
        public int PNo { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public int IndentNo { get; set; }
        public bool IsMinibar { get; set; }
        public string StoreId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<ItemIssueDetailRequest> Items { get; set; } = new();
    }

    public class ItemIssueDetailRequest
    {
        public int INo { get; set; }
        public int ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;    
        public decimal IssueQty { get; set; }
        public decimal ItemRate { get; set; }
        public string Unit { get; set; } = string.Empty;
        public int UnitCode { get; set; } = 0;
        public int PNo { get; set; }
        public decimal QtyPer { get; set; }
        public decimal NoOfQty { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public decimal OrginalQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal ReturnQty { get; set; }
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public int DepCode { get; set; } = 0;
        public string StockSource { get; set; } = string.Empty;
        public int StockReferenceNo { get; set; } = 0;


    }
    public class ItemIssueList
    {
        public int INo { get; set; }
        public DateTime IssueDate { get; set; }
        public int DepCode { get; set; }
        public decimal TotalAmount { get; set; }
        public int BillNo { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public int UserCode { get; set; }
        public int PNo { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public int IndentNo { get; set; }
        public bool IsMinibar { get; set; }
        public string StoreId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string TrasnsactionNo { get; set; } = string.Empty;
    }
    public class ItemIssueListDto
    {
        public ItemIssueList Master { get; set; } = new();
        public List<ItemIssueDetailRequest> Items { get; set; } = new();
    }
    #endregion

    #region Item Issue Return
    public class ItemIssueReturnSaveRequest
    {
        public int IRNo { get; set; }
        public DateTime IRDate { get; set; }
        public string BranchCode { get; set; } = string.Empty;
        public decimal IRTotalAmount { get; set; } =0;
        public string StoredId { get; set; } = string.Empty;
        public string IType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DeptCode { get; set; } = string.Empty;
        public List<ItemIssueReturnDetailRequest> Items { get; set; } = new();
    }

    public class ItemIssueReturnDetailRequest
    {
        public int INo { get; set; }
        public int ItemCode { get; set; }
        public decimal IRItemRate { get; set; }
        public decimal IRItemQty { get; set; }
        public decimal IRNoofQty { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal AvailableQty { get; set; }
        public int PNo { get; set; } = 0;
        public int IndentNo { get; set; }
        public int UnitCode { get; set; } = 0;
        public string Unit { get; set; } = string.Empty;
        public string MainUnit { get; set; } = string.Empty;
        public string MainUnitConverstion { get; set; } = string.Empty;
        public string StockSource { get; set; } = string.Empty;
        public int StockReferenceNo { get; set; } = 0;
    }
    #endregion

    #region Item Opening Stock
    public class ItemOpeningStock
    {
        public int OpeningStockId { get; set; }
        public int ItemCode { get; set; }
        public int StoreId { get; set; }
        public int? DeptCode { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public DateTime StockDate { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal OpeningRate { get; set; }
        public int? UnitCode { get; set; }
        public string? UnitName { get; set; }
        public decimal BaseOpeningQty { get; set; }
        public int? BaseUnitCode { get; set; }
        public string? BaseUnitName { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class ItemPurchaseStock
    {
        public int PurchaseStockId { get; set; }
        public int PurchaseNo { get; set; }
        public int? PONo { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int ItemCode { get; set; }
        public int StoreId { get; set; }
        public string Branch_Code { get; set; } = string.Empty;
        public decimal PurchaseQty { get; set; }
        public decimal PurchaseApprovalQty { get; set; }
        public decimal ReceivedQty { get; set; }
        public decimal ReturnQty { get; set; }
        public decimal DamageQty { get; set; }
        public decimal IssueQty { get; set; }
        public decimal IssueReturnQty { get; set; }
        public decimal IndentQty { get; set; }
        public decimal IndentApprovalQty { get; set; }
        public decimal AvailableQty { get; set; }
        public decimal PurchaseRate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    #endregion
}
