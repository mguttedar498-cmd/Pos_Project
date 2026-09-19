using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace HMS_360_PMS.EntitiesModels.POS
{
    public class Branch
    {
        public string Branch_code { get; set; }
        public string Branch_name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Phone_number { get; set; }
        public string Mob_number { get; set; }
        public string Fax_number { get; set; }
        public string Email_id { get; set; }
        public string Tin_no { get; set; }
        public string Licence_number { get; set; }
        public int BrId { get; set; }
        public int Company_code { get; set; }
    }

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Branch is required. Please select branch.")]
        public string Branch_code { get; set; }
        
        [Required(ErrorMessage = "Company is required. Please select company.")]
        public int Company_code { get; set; }
    }

    public class CompanyInfo
    {
        public string Company_Name { get; set; }
        public int Company_code { get; set; }
        public DateTime StartYear { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Phone_number { get; set; }
        public string Mob_number { get; set; }
        public string OwnerName { get; set; }
        public string Owner_Number { get; set; }
        public string Fax_number { get; set; }
        public string Email_id { get; set; }
        public string Tin_no { get; set; }
        public string Licence_number { get; set; }
        public string Branch_code { get; set; }
        public string STDCODE { get; set; }
        public string Logo { get; set; } = string.Empty;

    }

    public class UserMaster
    {
        public int UserCode { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public int UserPrivilege { get; set; }
        public string EnteredBy { get; set; }
        public string LastModify { get; set; }
        public string Branch_code { get; set; }
        public int storeid { get; set; }
        public string TabId { get; set; }
        public string DisPercent { get; set; }
        public string DisAmount { get; set; }
        public int RoleId { get; set; } = 0;

    }
    public class BillConfig
    {
        public string BilltType { get; set; }
        public string? isreq { get; set; }
        public string Branch_code { get; set; }
        public string SubBillType { get; set; }
        public string Config { get; set; }
    }

    public class PosUserRightsAccess
    {
        public string? UserName { get; set; }
        public string? UserId { get; set; }
        public bool? IsNcKotAccess { get; set; }
        public bool? IsCancelKotAccess { get; set; }
        public bool? IsVoidKotAccess { get; set; }
        public bool? IsTodayAccess { get; set; }
        public bool? IsSplitBillAccess { get; set; }
        public bool? IsSettlementAccess { get; set; }
        public string? Branch_Code { get; set; }
        public int? storeid { get; set; }
        public double? DisPercent { get; set; }
        public double? DisAmount { get; set; }
    }

    public class SystemOutletModel
    {
        public string SystemName { get; set; }
        public string OltCode { get; set; } = string.Empty;
        public string Temp1 { get; set; } = string.Empty;
        public string Temp2 { get; set; } = string.Empty;
        public string Temp3 { get; set; } = string.Empty;
        public string Temp4 { get; set; } = string.Empty;
        public DateTime? LastModify { get; set; } = DateTime.MinValue;
    }

    public class StewardMaster
    {
        public int StwCode { get; set; }
        public string POSCode { get; set; } = null!;
        public string StwName { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string? LastModify { get; set; }
        public string? Branch_Code { get; set; }
        public string? MobNo { get; set; }
    }

    public class OutletMaster
    {
        public string? OltCode { get; set; }
        public string POSCode { get; set; }
        public string OltName { get; set; }
        public bool OltIsRoomService { get; set; }
        public bool OltServiceTaxRequired { get; set; }
        public string OltAddress1 { get; set; }
        public string OltAddress2 { get; set; }
        public string TaxCode { get; set; }
        public string UserCode { get; set; }
        public string LastModify { get; set; }
        public double? ServiceCharge { get; set; }
        public string branch_code { get; set; }
        public bool? OltIsParcelService { get; set; }
        public string isuploaded { get; set; }
        public string ismodified { get; set; }
        public string TinNo { get; set; }
        public decimal? SBCess { get; set; }
        public double? KKCess { get; set; }
        public bool? InExTax { get; set; }
    }

    public class TableMaster
    {
        public decimal? TblCode { get; set; }
        public string OltCode { get; set; }
        public string TblNo { get; set; }
        public byte? TblSeatCount { get; set; }
        public string UserCode { get; set; }
        public string LastModify { get; set; }
        public string POSCODE { get; set; }
        public int c { get; set; }
        public string Branch_Code { get; set; }
    }

    public class TableStatusModel
    {
        public string TableNo { get; set; }
        public string Status { get; set; }
        public bool NC { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
    }

    public class OutletandTablemaster
    {
        // OutletMaster
        public string OltCode { get; set; }
        public string POSCode { get; set; }
        public string OltName { get; set; }
        public bool OltIsRoomService { get; set; }
        public bool OltServiceTaxRequired { get; set; }
        public string UserCode { get; set; }
        public bool? OltIsParcelService { get; set; }
        public bool? OltIsFastFood { get; set; }
        public bool IsDirectKOTandBill { get; set; }
        public bool IsDirectBill { get; set; }
        public bool IsDirectPaxandStw { get; set; }

        // TableMaster
        public decimal? TblCode { get; set; }
        public string TblNo { get; set; }
        public byte? TblSeatCount { get; set; }
        public string LastModify { get; set; }
        public int c { get; set; }
        public string Branch_Code { get; set; }
        public byte[] QR_Code { get; set; }
        public string TableStatus { get; set; }
        public bool KOTChargeable { get; set; }
        public string KOTStatus { get; set; }
        public int BillNo { get; set; }
        public decimal BillAmount { get; set; }
    }

    public class OutletConfigModel
    {
        public string OutletName { get; set; } = string.Empty;
        public bool IsDirectKOTandBill { get; set; }
        public bool IsDirectPaxandStw { get; set; }

    }


    public class SpecialInstruction
    {
        public int SPID { get; set; }
        public string SPINFO { get; set; }
    }
    public class ItemGroup
    {
        public int GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string? LastModify { get; set; }
        public string? Branch_Code { get; set; }
        public string? Dep { get; set; }
    }

    public class ItemCategory
    {
        public decimal? CatCode { get; set; }      // numeric(10,0)
        public string CatName { get; set; } = string.Empty;
        public string LastModify { get; set; } = string.Empty;
        public string? Branch_Code { get; set; }
        public string? SubCat { get; set; }
        public string? thumb { get; set; }         // nvarchar(max)
    }

    public class ItemMaster
    {
        public decimal? ItemCode { get; set; }              // numeric(10,0)
        public string ItemName { get; set; } = string.Empty;
        public string? ItemDisplayName { get; set; }
        public double? QPB { get; set; }                    // float
        public decimal? CatCode { get; set; }               // numeric(10,0)
        public string GrpCode { get; set; } = string.Empty;
        public bool ItemDiscountAllowed { get; set; }       // bit
        public double? ItemRate { get; set; }               // float
        public double? ItemSaleQtyUnit { get; set; }        // float
        public string? LastModify { get; set; }             // nvarchar(max)
        public string? mostrunningitemsrno { get; set; }
        public int? subItem { get; set; }
        public decimal? ItemOpStock { get; set; }           // numeric(18,2)
        public decimal? ItemCurStock { get; set; }
        public decimal? ItemOpRate { get; set; }            // money
        public decimal? ItemCurRate { get; set; }
        public decimal? UnitCode { get; set; }
        public float? ItemROQ { get; set; }                 // real
        public float? ItemROL { get; set; }                 // real
        public string? Dep { get; set; }                    // char(1)
        public decimal? Opstock { get; set; }
        public decimal? Ctstock { get; set; }
        public decimal? perqty { get; set; }
        public string? DepCode { get; set; }
        public decimal? perrate { get; set; }
        public string? SUnit { get; set; }
        public string? branch_code { get; set; }
        public string? thumb { get; set; }
        public bool? IsVeg { get; set; }                    // bit
    }

    public class CombinedOltItemlist
    {
        public int? OltCode { get; set; }
        public int? ItemCode { get; set; }
        public double? OIDRate { get; set; }
        public bool OIDAvailable { get; set; }        // bit
        public string? Branchcode { get; set; }

        public string ItemName { get; set; } = string.Empty;
        public bool ItemDiscountAllowed { get; set; }       // bit
        public string? thumb { get; set; }
        public bool? IsVeg { get; set; }

        public decimal? CatCode { get; set; }      // numeric(10,0)
        public string CatName { get; set; } = string.Empty;
        public string? catthumb { get; set; }         // nvarchar(max)

        public string? GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;
    }

    public class CombinedItemMasterCategorylist
    {
        public int? ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public double? ItemRate { get; set; }
        public bool ItemDiscountAllowed { get; set; }       // bit
        public string? thumb { get; set; }
        public bool? IsVeg { get; set; }
        public string? Branchcode { get; set; }

        public decimal? CatCode { get; set; }      // numeric(10,0)
        public string CatName { get; set; } = string.Empty;
        public string? catthumb { get; set; }         // nvarchar(max)

        public string? GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;
    }

    #region Btnsubmit for POS
    public class CartModel
    {
        public int UserCode { get; set; }
        public string Table { get; set; }
        public string SubTable { get; set; } = string.Empty;
        public int Outlet { get; set; }
        public string OutletName { get; set; }
        public int Waiter { get; set; }
        public string WaiterName { get; set; }
        public int Pax { get; set; }
        public List<FoodModel> Food { get; set; }
        public double Total { get; set; }
        public int TotQty { get; set; }
        public string Branch { get; set; }
        public string Type { get; set; }
        public int NCCode { get; set; }
        public string NCRemarks { get; set; }
        public double Discount { get; set; }
        public string DiscountIn { get; set; } = string.Empty;
        public string DiscountType { get; set; }
        public string DiscountRemarks { get; set; }
        public List<string> DiscountGroups { get; set; }
        public string VRemarks { get; set; }
        public string Mode { get; set; }
        public string SubBillType { get; set; }
        public string Plan { get; set; }
        public string GuestName { get; set; }
        public string GuestCode { get; set; }
        public string CheckInNo { get; set; }
        public string KotMobileNo { get; set; }
        public int KotMinTimer { get; set; }
        public string TaxType { get; set; } = string.Empty;
        public HomeDelivery HomeDelivary { get; set; }
        //public Dictionary<int, List<FoodModel>> FoodByKot { get; set; }
    }

    public class FoodModel
    {
        public int KOTId { get; set; } = 0;
        public int Id { get; set; }
        public string Food { get; set; }
        public string code { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public string Comment { get; set; }
        public int Category { get; set; }
        public int GrpCode { get; set; }
        public int OrigQty { get; set; }
        public bool itemDiscountAllowed { get; set; }

    }

    public class OldCartFoodModel
    {
        public int KOTId { get; set; }
        public string KOTTblNo { get; set; }
        public int ItemCode { get; set; }
        public int OltCode { get; set; }
        public int GrpCode { get; set; }
        public string GrpName { get; set; }
        public string branchcode { get; set; }
        public int KOTSeatsServed { get; set; }
        public string Food { get; set; }
        public string code { get; set; }
        public double Price { get; set; }
        public int Qty { get; set; }
        public string Comment { get; set; }
        public int Category { get; set; }
        public int OrigQty { get; set; }
        public bool itemDiscountAllowed { get; set; }

    }

    public class WaiterModel
    {
        public int StwCode { get; set; }
        public string StwName { get; set; }
        public int Pax { get; set; }
        public int NCCode { get; set; }
        public string NCRemarks { get; set; }
        public string KotMobileNo { get; set; }
        public string KOTGuestName { get; set; }
    }

    public class HomeDelivery
    {
        public decimal GuestCode { get; set; }
        public decimal TitleGn1 { get; set; }
        public string GuestName { get; set; }
        public DateTime DOB { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        public DateTime LastModify { get; set; }
        public decimal Discount { get; set; }
        public string Branch_code { get; set; }
        public int? isUpdate { get; set; }
    }

    public class BillConfigModel
    {
        public string BillType { get; set; }
        public string SubBillType { get; set; }
    }

    public class KOTModel
    {
        public int KOTId { get; set; }
        public int KOTNo { get; set; }
    }

    public class UpdateKOTMasterRequest
    {
        public int KOTNo { get; set; }
        public string GuestCode { get; set; }
        public int NCCode { get; set; }
        public string Table { get; set; }
        public string SubTable { get; set; }
        public string KotMobileNo { get; set; }
        public string branchcode { get; set; }
    }

    public class SaveKOTDetailRequest
    {
        public int KOTId { get; set; }
        public int KOTNo { get; set; }
        public int ItemCode { get; set; }
        public double KOTDRate { get; set; }
        public int KOTDQty { get; set; }
        public string SpecialInstId { get; set; } // getSpecialInfoId(item.Comment)
        public string BranchCode { get; set; }
        public string IsFree { get; set; } = "False";
        public double ItemDiscount { get; set; } = 0;
        public int IsOnline { get; set; } = 0;
        public int KNQty { get; set; } = 0;
        public string FinCode { get; set; }
    }

    public class GlobalSettingsModel
    {
        public bool InOrExOfTax { get; set; }
        public bool HappyHours { get; set; }
        public TimeSpan HHFrom { get; set; }
        public TimeSpan HHTo { get; set; }
    }

    public class FreeItemDetail
    {
        public int FreeItemCode { get; set; }
        public int FreeItemQty { get; set; }
        public bool IsFree { get; set; }
    }

    public class TmpKotPrintRequest
    {
        public int KotNo { get; set; }
        public int WaiterNo { get; set; }
        public string TableNo { get; set; }
        public DateTime KotDate { get; set; }
        public string ItemName { get; set; }
        public int Qty { get; set; }
        public string CategoryName { get; set; }
        public string GroupCode { get; set; } = "P";
        public int ManualKotNo { get; set; }
        public int ItemCode { get; set; }
        public string Branch_Code { get; set; }
    }

    public class ActiveKotDetail
    {
        public int KotId { get; set; }
        public int KotNo { get; set; }
        public int Kid { get; set; }
        public int ItemCode { get; set; }
        public int KOTDQty { get; set; }
    }

    public class BillModel
    {
        public CartModel Cart { get; set; }
        public TaxModel Tax { get; set; }
        public string BillingType { get; set; }
        public string SubBillingType { get; set; }
        //public PhonePeCollectResponseBody paymentresponse { get; set; }
    }

    public class KBillModel
    {
        public int ksmid { get; set; }
        public string billno { get; set; }
        //public DateOnly billdate { get; set; }
        //public string billtime { get; set; }
    }

    public class TaxModel
    {
        public double TotalAmount { get; set; }
        public int TotalQty { get; set; }
        public double CGSTPer { get; set; }
        public double CGSTAmt { get; set; }
        public double SGSTPer { get; set; }
        public double SGSTAmt { get; set; }
        public double ServiceChargePer { get; set; }
        public double ServiceCharge { get; set; }
        public double GrandTotal { get; set; }
        public double DiscountPer { get; set; }
        public double Discount { get; set; }
        public string DiscountIn { get; set; }
        public string DiscountRemarks { get; set; }
        public double RoundOff { get; set; }
        public List<Taxdetails> TaxList { get; set; }

        // ✅ Only for grouped tax
        //public List<GroupTaxModel> Groups { get; set; }
    }

    public class Taxdetails
    {
        public int GroupCode { get; set; } = 0;
        public string GroupName { get; set; } = string.Empty;
        public string TaxName { get; set; }
        public double Taxper { get; set; }
        public double TaxableAmount { get; set; }
        public double TaxAmount { get; set; } // CGST + SGST
        public double Total { get; set; }     // SubTotal + Tax
        public double CGST { get; set; }
        public double SGST { get; set; }
    }

    public class GroupTaxModel
    {
        public int GroupCode { get; set; }
        public string GroupName { get; set; }
        public double SubTotal { get; set; }
        public double CGST { get; set; }
        public double SGST { get; set; }
        public double Total { get; set; } // SubTotal + CGST + SGST
    }

    public class PhonePeCollectResponseBody
    {
        public bool success { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }
    public class Data
    {
        public string transactionId { get; set; }
        public int amount { get; set; }
        public string merchantId { get; set; }
        public string providerReferenceId { get; set; }
        public string qrString { get; set; }
    }

    public class PhonePeCollectRequest
    {
        public string merchantId { get; set; }
        public string transactionId { get; set; }
        public string merchantOrderId { get; set; }
        public int amount { get; set; }
        public int expiresIn { get; set; }
        public int intentExpiryInSeconds { get; set; } = 0;
        public string storeId { get; set; }
        public string terminalId { get; set; }
        public string solutionType { get; set; }
    }

    public class ExtraChargeModel
    {
        public double TaxPercentage { get; set; }
        public string TaxCode { get; set; }
        public string ChargeName { get; set; }
        public string TaxDescription { get; set; }
    }

    public class TempTaxItem
    {
        public int GrpCode { get; set; }
        public double ItemTotal { get; set; }
        public double CGSTPer { get; set; }
        public double SGSTPer { get; set; }
    }

    public class RuleModel
    {
        public int RNo { get; set; }
        public string RuleType { get; set; }
        public string Type1 { get; set; }
        public string Operation1 { get; set; }
        public string Type2 { get; set; }
        public string Operation2 { get; set; }
        public string Type3 { get; set; }
        public string BranchCode { get; set; }
    }

    public class CalcModel
    {
        public double RoundOff { get; set; }
        public double Total { get; set; }
    }

    public class BillModelForBill
    {
        public CartModel Cart { get; set; }
        public TaxModel Tax { get; set; }
        public string BillingType { get; set; }
        public string SubBillingType { get; set; }
        public PhonePeCollectResponseBody paymentresponse { get; set; }
        public BillHeaderdetilsforBill billdetails { get; set; }

    }

    public class BillHeaderdetilsforBill
    {
        public string Billno { get; set; }
        public string BillDate { get; set; }
        public string BillTime { get; set; }
        public string OutletName { get; set; }
        public string TokenNo { get; set; }
        public string OrderId { get; set; }
    }

    public class IndiTaxModel
    {
        public int ItemCode { get; set; }
        public string TaxCode { get; set; }
        public double TaxAmount { get; set; }
    }

    public class NCSalesTax
    {
        public string Bill_No { get; set; }
        public int ItemCode { get; set; }
        public string TaxCode { get; set; }
        public double TaxAmount { get; set; }
        public string Branch_Code { get; set; }
        public int OltCode { get; set; }
        public DateTime BillDate { get; set; }
        public string Ref { get; set; }
        public string FinCode { get; set; }

    }

    public class SalesTax
    {
        public string Bill_No { get; set; }
        public int ItemCode { get; set; }
        public string TaxCode { get; set; }
        public double TaxAmount { get; set; }
        public string Branch_Code { get; set; }
        public int OltCode { get; set; }
        public DateTime BillDate { get; set; }
        public string Ref { get; set; }
        public int IsOnline { get; set; }
        public string FinCode { get; set; }
        public string TaxType { get; set; }
    }

    public class KOTSettlementStatusModel
    {
        public bool Status { get; set; }
        public string BillNo { get; set; }
        public int BillId { get; set; }
    }

    public class SettlementModel
    {
        public string Branch { get; set; }
        public int UserCode { get; set; }
        public int CompanyCode { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public int GuestCode { get; set; } = 0;
        public string GuestName { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public int OutletCode { get; set; }
        public string OutletName { get; set; } = string.Empty;
        public string RoomNo { get; set; }
        public string SubBillingType { get; set; } = string.Empty;
        public string PayMode { get; set; }
        public SettlementBillModel Bill { get; set; }
    }

    public class SettlementBillModel
    {
        public int OltCode { get; set; }
        public string OutletName { get; set; } = string.Empty;
        public int UserCode { get; set; }
        public int BillId { get; set; }
        public int BillNo { get; set; }
        public string TableNo { get; set; }
        public string SubTableNo { get; set; }
        public double Discount { get; set; }
        public double TaxAmount { get; set; }
        public double Tips { get; set; }
        public double ChangeAmount { get; set; }
        public double GrandAmount { get; set; }
        public string RefNo { get; set; } = string.Empty;
        public string CardName { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string BranchCode { get; set; }
        public string GuestCode { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;

        public List<paymentDetailsModel> PaymentDetails { get; set; }
    }

    public class paymentDetailsModel
    {
        public string Mode { get; set; }
        public string SubMode { get; set; }
        public double Amount { get; set; }
        public string Remarks { get; set; }
    }

    public class FoodBill
    {
        public int RNo { get; set; }
        public DateTime TrDate { get; set; }
        public string RcptNo { get; set; }
        public string GuestCode { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string RoomNo { get; set; } = string.Empty;
        public double BillAmt { get; set; }
        public string Outlet { get; set; } = string.Empty;
        public double CGST { get; set; }
        public double SGST { get; set; }
        public decimal TaxVal { get; set; }
        public decimal ExTax1 { get; set; }
        public decimal ExTax2 { get; set; }
    }

    public class NCModel
    {
        public int NCDepCode { get; set; }
        public string NCDepName { get; set; }
    }
    #endregion


    public class FinancialMaster
    {
        public int FinId { get; set; }
        public DateTime? FinFromDate { get; set; }
        public DateTime? FinToDate { get; set; }
        public int? FincurrentYear { get; set; }
        public int? FinEndYear { get; set; }
        public int? CurrentStatus { get; set; }
        public string LogUser { get; set; }
        public string IpAddress { get; set; }
        public char? FinalClose { get; set; }
        public string FinCode { get; set; }
        public string BranchCode { get; set; }
    }

    public class DiscountModeMaster
    {
        public int DiscId { get; set; }
        public bool DiscountRequired { get; set; }
        public string DiscountType { get; set; }
        public string BranchCode { get; set; }
    }

    public class TaxSettingMaster
    {
        public int TaxId { get; set; }
        public bool TaxRequired { get; set; }
        public string? TaxType { get; set; }
        public string BranchCode { get; set; }
    }

    public class PrinterSettingMaster
    {
        public string PrinterName { get; set; }
        public string BillType { get; set; }
        public string Branch_Code { get; set; }
        public string OltCode { get; set; }
        public string PrintType { get; set; }
        public int GrpCode { get; set; }
        public string IPAddress { get; set; }
    }

    public class CategoryGroupSetting
    {
        public string CatGrp { get; set; }
        public int Grp { get; set; }
        public string Branch_code { get; set; }
    }

    public class FastFoodDetails
    {
        public decimal? StwCode { get; set; }
        public string POSCode { get; set; } = null!;
        public string StwName { get; set; } = null!;
        public string UserCode { get; set; } = null!;

        public decimal? TblCode { get; set; }
        public int? OltCode { get; set; }
        public string TblNo { get; set; }
        public byte? TblSeatCount { get; set; }
        public string Branch_Code { get; set; }
        public byte[] QR_Code { get; set; }
    }

    public class CompanyMaster
    {
        public int CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int Pincode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string UserCode { get; set; }
        public string Branch_code { get; set; }
    }

    public class PaymentModeMaster
    {
        public int ModeId { get; set; }
        public string ModeType { get; set; }
        public bool ModeRequired { get; set; }
        public string BranchCode { get; set; }
        public int? SubModeId { get; set; }
        public string SubModeType { get; set; }
        public int? MasterModeId { get; set; }
    }

    public class UnSettlementBillModel
    {
        public int KSMId { get; set; }
        public string OltCode { get; set; }
        public string KSMBillNo { get; set; }

        public DateTime KSMBillDate { get; set; }
        public string KSMBillTime { get; set; }

        public double Total { get; set; }              // ✅ Calculated SUM column

        public double? KSMBillTaxAmt { get; set; }
        public double? KSMBillDiscount { get; set; }

        public string KSMTblNo { get; set; }
        public string UserCode { get; set; }

        public double? DiscountPercent { get; set; }

        public string KSMSUBTBLNO { get; set; }
        public string STEWCODE { get; set; }

        public string Branch_Code { get; set; }

        public string? Tips { get; set; } = string.Empty;
    }

    public class KOTTransferTypeMaster
    {
        public int TransferId { get; set; }
        public string TransferType { get; set; }
        public string BranchCode { get; set; }
    }

    public class KOTTransferRequest
    {
        public string OldOutlet { get; set; }
        public string OldTableNo { get; set; }
        public string OldSubTable { get; set; }

        public string NewOutlet { get; set; }
        public string NewTable { get; set; }
        public string NewSubTable { get; set; }

        public string UserCode { get; set; }
        public string Branch { get; set; }
        public string TransferType { get; set; } // FULL / KOT / ITEM
        public List<string?> KotNo { get; set; }       // for KOT & ITEM
        public List<int?> ItemCode { get; set; }         // for ITEM
    }

    public class SubTableStatusModel
    {
        public string KOTTblNo { get; set; }
        public string SubTable { get; set; }
        public DateTime KOTTime { get; set; }
        public string TableStatus { get; set; }
        public int? BillNo { get; set; }
        public decimal? BillAmount { get; set; }
    }

    public class KotTransferRequest
    {
        public string SourceTableNo { get; set; }
        public string SourceSubTable { get; set; }
        public string TargetTableNo { get; set; }
        public string TargetSubTable { get; set; }
        public string KotNo { get; set; }

        public int OutletCode { get; set; }
        public int TargetOutletCode { get; set; }

        public bool IsKotNo { get; set; }
        public bool IsItem { get; set; }

        public string BranchCode { get; set; }
        public string UserId { get; set; }
    }

    public class KOT2NCKOTRequest
    {
        public List<int> KOTId { get; set; }
        public string TableNo { get; set; }
        public string SubTable { get; set; }
        public string Branch { get; set; }
        public int NcCode { get; set; }
        public string NcRemarks { get; set; }
        public string ActionType { get; set; } // "KOT2NC" or "NC2KOT"

    }

    public class KOTBillSettlementFilter
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Branch_Code { get; set; }
        public string? OltCode { get; set; }
    }

    public class KOTBillSettlementModel
    {
        public int KBSId { get; set; }
        public string? OltCode { get; set; }
        public int KSMId { get; set; }
        public string? KSMBillNo { get; set; }
        public double KSMBillAmount { get; set; }
        public DateTime KBSSetteleDate { get; set; }
        public DateTime KBSValidDate { get; set; }
        public string? KBSPaymentMode { get; set; }
        public double KBSDiscount { get; set; }
        public string? UserCode { get; set; }
        public string? Branch_Code { get; set; }
    }

    public class KOTMaster
    {
        public int KOTId { get; set; }
        public string KOTNo { get; set; }
        public string OltCode { get; set; }
        public string KOTTblNo { get; set; }
        public int StwCode { get; set; }
        public int KOTSeatsServed { get; set; }
        public DateTime KOTDate { get; set; }
        public DateTime KOTTime { get; set; }
        public double KOTChargeable { get; set; }
        public double KOTTotal { get; set; }
        public bool KOTCancelled { get; set; }
        public bool KOTSettled { get; set; }
        public string UserCode { get; set; }
        public string NCKOT_Particulars { get; set; }
        public string SubTable { get; set; }
        public string DRefKOTNo { get; set; }
        public string DKOTNO { get; set; }
        public string DayEnd { get; set; }
        public string branch_code { get; set; }
        public string FinCode { get; set; }
    }

    public class KOTDetails
    {
        public int KOTId { get; set; }
        public string KOTNO { get; set; }
        public int ItemCode { get; set; }
        public double KOTDRate { get; set; }
        public int KOTDQty { get; set; }
        public string branch_code { get; set; }
        public double ItemDiscount { get; set; }
        public string FinCode { get; set; }
    }

    public class KOTSettlementMaster
    {
        public int KSMId { get; set; }
        public string POSCode { get; set; }
        public string OltCode { get; set; }
        public int KSMBillNo { get; set; }
        public DateTime KSMBillDate { get; set; }
        public DateTime KSMBillTime { get; set; }
        public double KSMBillAmount { get; set; }
        public double KSMBillTaxAmt { get; set; }
        public double KSMBillDiscount { get; set; }
        public bool KSMBillCancled { get; set; }
        public bool KSMBillSettled { get; set; }
        public string KSMTblNo { get; set; }
        public bool KSMBillTransfered { get; set; }
        public string UserCode { get; set; }
        public string DCParticulars { get; set; }
        public string KSMSUBTBLNO { get; set; }
        public string STEWCODE { get; set; }
        public string DKSMBillNo { get; set; }
        public string DayEnd { get; set; }
        public string TokenNo { get; set; }
        public string Branch_Code { get; set; }
        public string ismodified { get; set; }
        public string FinCode { get; set; }
    }

    public class KOTSettlementDetails
    {
        public int KSMId { get; set; }
        public int KOTId { get; set; }
        public string Branch_Code { get; set; }
        public string FinCode { get; set; }
    }

    public class ItemDiscount
    {
        public int BillNo { get; set; }
        public DateTime Billdate { get; set; }
        public double Amount { get; set; }
        public string GrpCode { get; set; }
        public double AmountPerc { get; set; }
        public double DiscAmount { get; set; }
        public string Branch_Code { get; set; }
        public string OltCode { get; set; }
        public string Ref { get; set; }
        public bool IsOnline { get; set; }
        public string DiscountIn { get; set; }
        public string DiscountType { get; set; }
    }

    public class KOTBillSettlement
    {
        public int KBSId { get; set; }
        public string OltCode { get; set; }
        public int KSMId { get; set; }
        public int KSMBillNo { get; set; }
        public double KSMBillAmount { get; set; }
        public DateTime KBSSetteleDate { get; set; }
        public DateTime KBSValidDate { get; set; }
        public string KBSPaymentMode { get; set; }
        public double KBSDiscount { get; set; }
        public string UserCode { get; set; }
        public string Branch_Code { get; set; }
    }

    public class BillTaxModel
    {
        public int BillTaxId { get; set; }       // Primary key
        public double TotalAmount { get; set; }
        public int TotalQty { get; set; }
        public double CGSTAmt { get; set; }
        public double SGSTAmt { get; set; }
        public double ServiceChargePer { get; set; }
        public double ServiceCharge { get; set; }
        public double GrandTotal { get; set; }
        public double DiscountPer { get; set; }
        public double Discount { get; set; }
        public string DiscountIn { get; set; }
        public string DiscountRemarks { get; set; }
        public double RoundOff { get; set; }

        public int BillId { get; set; }         // Reference to original bill
        public DateTime BillDate { get; set; }
        public string OltCode { get; set; }
        public string BranchCode { get; set; }

        public List<BillTaxDetailModel> TaxDetails { get; set; } = new List<BillTaxDetailModel>();
    }

    public class BillTaxDetailModel
    {
        public int TaxDetailId { get; set; }    // Primary key
        public int BillTaxId { get; set; }      // Foreign key to BillTax
        public int GroupCode { get; set; }
        public string GroupName { get; set; }
        public string TaxName { get; set; }
        public double Taxper { get; set; }
        public double TaxableAmount { get; set; }
        public double TaxAmount { get; set; }   // CGST + SGST
        public double Total { get; set; }       // SubTotal + Tax
        public double CGST { get; set; }
        public double SGST { get; set; }
        public int BillId { get; set; }   // Reference to original bill
        public DateTime BillDate { get; set; }
        public string OltCode { get; set; }
        public string BranchCode { get; set; }
    }

    public class ReprintBillData
    {
        public List<KOTSettlementMaster> SettlementMasters { get; set; }
        public List<KOTSettlementDetails> SettlementDetails { get; set; }
        public List<KOTMaster> KOTMasters { get; set; }
        public List<KOTDetails> KOTDetails { get; set; }
        public List<KOTBillSettlement> BillSettlements { get; set; }
        public List<SalesTax> SalesTaxes { get; set; }
        public List<ItemDiscount> ItemDiscounts { get; set; }
        public OutletMaster OutletMaster { get; set; }
        public List<ItemGroup> ItemGroups { get; set; }
        public List<StewardMaster> StewardMasters { get; set; }
        public List<ItemMaster> ItemMasters { get; set; }
        public BillTaxModel BillTaxList { get; set; }
    }

    public class GSTBillDetailModel
    {
        public string GuestName { get; set; } = string.Empty;
        public string GSTNo { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int StateCode { get; set; } = 0;
        public int BillNo { get; set; }
        public string OltCode { get; set; }
        public DateTime BillDate { get; set; } = DateTime.Now;
        public string Branchcode { get; set; }
    }

    public class DayValidationRequest
    {
        public DateTime POSEntryDate { get; set; }
        public string Branchcode { get; set; }
        //public int DayCloseGraceHour { get; set; }
    }

    public class SmsSenderConfig
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

    public class PGCreatePayment
    {
        public string merchantOrderId { get; set; }
        public int amount { get; set; }
        public int expireAfter { get; set; }
        public MetaInfo metaInfo { get; set; }
        public PaymentFlow paymentFlow { get; set; }
    }


    public class MetaInfo
    {
        public string udf1 { get; set; }
        public string udf2 { get; set; }
        public string udf3 { get; set; }
        public string udf4 { get; set; }
        public string udf5 { get; set; }
    }

    public class PaymentFlow
    {
        public string type { get; set; }
        public string message { get; set; }
        public MerchantUrls merchantUrls { get; set; }
    }

    public class MerchantUrls
    {
        public string redirectUrl { get; set; }
    }

    public class PGTokenModel
    {
        public string client_id { get; set; }
        public int client_version { get; set; }
        public string client_secret { get; set; }
        public string grant_type { get; set; }
    }


    public class OutletSelectModelType
    {
        [JsonPropertyName("OutletCode")]
        public int OutletCode { get; set; }

        [JsonPropertyName("OutletName")]
        public string OutletName { get; set; }

        [JsonPropertyName("OutletType")]
        public string OutletType { get; set; }
    }

    public class PhonePeImageRequestModel
    {
        public int Id { get; set; }
        public string BaseUrl { get; set; }
        public string MerchantKey { get; set; }
        public string MerchantId { get; set; }
        public string StoreId { get; set; }
        public string TerminalId { get; set; }
        public int ExpiresIn { get; set; }
        public string TransactionId { get; set; }
        public string ProviderId { get; set; }
        public string CallbackUrl { get; set; }
    }

    public class CancelBillModel
    {
        public int Outlet { get; set; } = 0;
        public int BillNo { get; set; } = 0;
        public string Branch { get; set; } = string.Empty;
        public DateTime BillDate { get; set; } = DateTime.Now;
        public int UserId { get; set; } = 0;
        public string Reason { get; set; } = string.Empty;
    }

    public class KotInfo
    {
        public int KOTNo { get; set; }
        public decimal Amount { get; set; }
    }

    public class CancelBillListModel
    {
        public string OutletCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
    }

    public class CompanyBillModel
    {
        public int BTId { get; set; }
        public string CompanyCode { get; set; }
        public DateTime BTDate { get; set; }
        public DateTime BTTime { get; set; }
        public int BillNo { get; set; }
        public decimal BillAmt { get; set; }
        public decimal AmtPaid { get; set; }
        public bool BTCSettled { get; set; }
        public int UserCode { get; set; }
        public DateTime SettleDate { get; set; }
        public string PMode { get; set; }
        public int Oltcode { get; set; }
        public bool Discount { get; set; }
        public string Branch_Code { get; set; }
        public string IsUploaded { get; set; }
        public string IsModified { get; set; }
    }

    public class CompanyBillDetailRequest
    {
        public int BTId { get; set; } = 0;
        public int BillNo { get; set; } = 0;
        public decimal BillAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Partialpay { get; set; }
        public bool IndividualChargesApplied { get; set; } = false;
        public List<ChargeDetail> IndividualCharges { get; set; } = new List<ChargeDetail>();
    }

    public class ChargeDetail
    {
        public string ChargesType { get; set; }
        public decimal ChargesAmount { get; set; }
    }

    public class CompanyBillSettlementRequest
    {
        //public int BillNo { get; set; } = 0;
        //public decimal BillAmount { get; set; } = 0;
        //public decimal AmountPaid { get; set; } = 0;
        public int CompanyCode { get; set; } = 0;
        public decimal PayingAmount { get; set; } = 0;
        public DateTime SettleDate { get; set; } = DateTime.Now;
        public string BankName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string ChDDNo { get; set; } = string.Empty;
        public string UserCode { get; set; } = string.Empty;
        public string PaymentMode { get; set; } = "CASH";
        public string CCNO { get; set; } = string.Empty;
        public string RefNo { get; set; } = string.Empty;
        public DateTime ValidDate { get; set; } = DateTime.Now;
        public string Branch_Code { get; set; } = string.Empty;
        public bool IsFullSettlement { get; set; } = false;
        public bool isChargesApplied { get; set; } = false;
        public List<ChargeDetail> FullChargesDetails { get; set; } = new List<ChargeDetail>();

        public List<CompanyBillDetailRequest> Bills { get; set; } = new List<CompanyBillDetailRequest>();
    }

    public class ChargesMasterModel
    {
        public int ChargesId { get; set; } = 0;
        public string ChargesType { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    #region ModifyBill

    public class BillDiscountModel
    {
        public int  BillNo { get; set; } = 0;
        public decimal amount { get; set; } = 0;
        public string grpcode { get; set; } = string.Empty;
        public decimal amountperc { get; set; } = 0;
        public decimal discamount { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public int oltcode { get; set; } = 0;
        public string reference  { get; set; } = string.Empty;
        public int IsOnline { get; set; } = 0;
        public string DiscountIn { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
    }

    public class PosModifyBill
    {
        public string KOTTblNo { get; set; } = string.Empty;
        public int OltCode { get; set; } = 0;
        public string branchcode { get; set; } = string.Empty;
        public int KOTSeatsServed { get; set; } = 0;
        public List<FoodModel> Food { get; set; }

    }
    public class PosModifyBillSettlement
    {
        public int KSMId { get; set; } = 0;
        public int KSMBillNo { get; set; } = 0;
        public int KOTId { get; set; } = 0;
        public DateTime SettledDate { get; set; } = DateTime.Now;
        public string KOTTblNo { get; set; } = string.Empty;
        public int OltCode { get; set; } = 0;
        public string Branch_Code { get; set; } = string.Empty;
        public int UserCode { get; set; } = 0;
        public decimal PreviousBillAmount { get; set; } = 0;
        public decimal PreviousBillTaxAmt { get; set; } = 0;
        public decimal PreviousBillDiscount { get; set; } = 0;
        //public decimal CurrentBillAmount { get; set; } = 0;
        //public decimal CurrentBillTaxAmt { get; set; } = 0;
        //public decimal CurrentBillDiscount { get; set; } = 0;
        //public decimal GrandTotal { get; set; } = 0;
        //public decimal roundOff { get; set; } = 0;
        public bool IsDiscountAdded { get; set; } = false;
        public string DiscountType { get; set; } = string.Empty;

        public string GrpCode { get; set; } = string.Empty;

        public string PaymentStatus { get; set; } = string.Empty;
        public int CompanyCode { get; set; } = 0;

        //public List<SettlementBillModel> settlement { get; set; }
        public List<KOTDetailsFood> foods { get; set; }
        public TaxModel Taxdetails { get; set; }
    }

    public class KOTDetailsFood
    {
        public int KotId { get; set; } = 0;
        public int ItemCode { get; set; } = 0;
        public string Food { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0;
        public int Qty { get; set; } = 0;
    }

    #endregion

    #region Bill Adjustment

    public class BillAdjustmentRequest
    {
        public string BranchCode { get; set; }
        public string OltCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string PaymentMode { get; set; }
        public List<string> ExcludedBills { get; set; } = new List<string>();
        public string RankByType { get; set; } = "KSMID";
    }

    public class BillAdjustmentModel
    {
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public decimal BillAmount { get; set; }
        public string PaymentMode { get; set; }
        public string OriginalBillNo { get; set; }
    }

    public class BillAdjustmentSettlementModel
    {
        public string BillNo { get; set; }
        public string KSMId { get; set; }
        public string KId { get; set; }
        public string KOTId { get; set; }
        public string KOTNo { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public int Qty { get; set; }
        public decimal KotRate { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxPercentage { get; set; }
        public int TaxCode { get; set; }
        public int GrpCode { get; set; }
        public string GrpName { get; set; }
        public string Chk { get; set; }
        public int RankId { get; set; }
        public DateTime BillDate { get; set; }
        public string BranchCode { get; set; }

    }

    public class GroupTotalModel
    {
        public string Groupwise { get; set; }

        public decimal Amount { get; set; }
    }

    public class SaveBidRequest
    {
        public string BranchCode { get; set; }
        public string OltCode { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal FinalSaleAmount { get; set; }
        public string RankByType { get; set; } = "KSMID";
    }

    public class OutletSettingsModel
    {
        public decimal TaxCode { get; set; } = 0;

        public decimal ServiceCharge { get; set; } = 0;
    }

    public class ItemTaxModel
    {
        public int TaxCode { get; set; }

        public decimal TaxPercentage { get; set; }
    }

    public class KotSettlementMasterUpdateModel
    {
        public string BillNo { get; set; } = string.Empty;
        public DateTime BillDate { get; set; }
        public string KSMId { get; set; } = string.Empty;
        public decimal BillAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ServiceTaxAmount { get; set; }
        public decimal ServiceCharge { get; set; }
        public string OltCode { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;
    }

    public class AdjustmentSalesTaxModel
    {
        public int KSMId { get; set; } = 0;
        public int KSMBillNo { get; set; } = 0;
        //public int KOTId { get; set; } = 0;
        public DateTime SettledDate { get; set; } = DateTime.Now;
        public int OltCode { get; set; } = 0;
        public string BranchCode { get; set; } = string.Empty;
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public int TotalQty { get; set; }
        public double RoundOff { get; set; }
    }

    #endregion

    #region POS Room Service

    public class RoomTableStatusModel
    {
        public string RoomNo { get; set; }
        public string RoomCode { get; set; }
        public int PAX { get; set; }
        public string PlanId { get; set; }
        public string GuestCode { get; set; }
        public string GuestName { get; set; }
        public string CheckinNo { get; set; }
        public string TableStatus { get; set; }
    }

    public class RoomStatus
    {
        public DateTime TrDate { get; set; }
        public string RoomNo { get; set; }
        public string RoomCode { get; set; }
        public string RoomDesc { get; set; }
        public int PAX { get; set; }
        public int EPAX { get; set; }
        public string Prioritys { get; set; }
        public string FloorCode { get; set; }
        public string FloorName { get; set; }
        public string Status { get; set; }
        public string IpAdd { get; set; }
        public string Loger { get; set; }
        public string CurrentCheckIn { get; set; }
        public string GroupCode { get; set; }
    }

    #endregion


    #region Unsettled KOT and Bill Details

    public class UnsettledKOTModel
    {
        public int KOTId { get; set; }
        public int KOTNo { get; set; }
        public int OltCode { get; set; }
        public string KOTTblNo { get; set; }
        public int StwCode { get; set; }
        public DateTime KOTDate { get; set; }
        public DateTime KOTTime { get; set; }
        public bool KOTChargeable { get; set; }
        public decimal KOTTotal { get; set; }
        public string CheckinNo { get; set; }
        public string KOTGuestName { get; set; }
        public bool KOTCancelled { get; set; }
        public bool KOTSettled { get; set; }
        public string SubTable { get; set; }
        public string GuestCode { get; set; }
        public string branch_code { get; set; }
    }

    public class UpdateUnsettledKOTRequest
    {
        public string KOTId { get; set; }
        public string OltCode { get; set; }
        public DateTime KOTDate { get; set; }
        public string Branchcode { get; set; }
    }

    public class UnsettledBillModel
    {
        public int KSMId { get; set; }
        public int OltCode { get; set; }
        public string KSMBillNo { get; set; }
        public DateTime KSMBillDate { get; set; }
        public DateTime KSMBillTime { get; set; }
        public decimal KSMBillAmount { get; set; }
        public decimal KSMBillTaxAmt { get; set; }
        public decimal KSMBillDiscount { get; set; }
        public bool KSMBillCancled { get; set; }
        public bool KSMBillSettled { get; set; }
        public string KSMTblNo { get; set; }
        public bool KSMBillTransfered { get; set; }
        public bool KSMIsRoomService { get; set; }
        public bool BillCancelled { get; set; }
        public decimal KSMSettledAmt { get; set; }
        public decimal KSMServiceTaxAmt { get; set; }
        public bool KSMBillNoofTime { get; set; }
        public string KSMSUBTBLNO { get; set; }
        public int STEWCODE { get; set; }
        public string GuestCode { get; set; }
        public string Branch_Code { get; set; }
        public decimal GrandTotal { get; set; }
    }

    #endregion
}
