using HMS_360_PMS.EntitiesModels.POS;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json.Serialization;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs
{
    public class POSServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class BranchDto
    {
        public string Branch_code { get; set; }
        public string Branch_name { get; set; }
        public int BrId { get; set; }
        public int Company_code { get; set; }
    }

    public class CompanyInfoDto
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

    public class UserMasterDto
    {
        public int UserCode { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
        public string Token { get; set; }
        public string Branch_code { get; set; }
        public int storeid { get; set; }
        public string DisPercent { get; set; }
        public string DisAmount { get; set; }
        public int RoleId { get; set; } = 0;

    }

    public class PosUserRightsAccessDto
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
    }

    public class POSLoginResponseDto
    {
        public UserMasterDto User { get; set; }

        public CompanyInfoDto CompanyInfo { get; set; }

        public IEnumerable<BillConfig> BillConfigs { get; set; }

        public IEnumerable<PosUserRightsAccess> UserRights { get; set; }

        public IEnumerable<BillConfig> ReqBillConfigs { get; set; }

        public IEnumerable<BillConfig> BillNoConfigs { get; set; }

        public IEnumerable<BillConfig> KotNoConfigs { get; set; }

        public IEnumerable<BillConfig> ReportConfigs { get; set; }
        public string SerialKey { get; set; }
        public bool SerialKeyExpired { get; set; }
    }

    public class StewardMasterResponseDto
    {
        public int StwCode { get; set; }
        public string POSCode { get; set; } = null!;
        public string StwName { get; set; } = null!;
        public string UserCode { get; set; } = null!;
        public string? LastModify { get; set; }
        public string? Branch_Code { get; set; }
        public string? MobNo { get; set; }
    }

    public class OutletDto
    {
        public string OltCode { get; set; }
        public string POSCode { get; set; }
        public string OltName { get; set; }
        public bool OltIsRoomService { get; set; }
        public bool OltServiceTaxRequired { get; set; }
        public bool? OltIsParcelService { get; set; }
        public bool? OltIsFastFood { get; set; }
        public bool IsDirectKOTandBill { get; set; }
        public bool IsDirectBill { get; set; }
        public bool IsDirectPaxandStw { get; set; }

        public List<TableDto> Tables { get; set; }
    }

    public class TableDto
    {
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
        //public int BillNo { get; set; }
        //public decimal BillAmount { get; set; }
    }

    public class CategoryListDto
    {
        public int? OltCode { get; set; }
        public string? Branchcode { get; set; }

        public decimal? CatCode { get; set; }      // numeric(10,0)
        public string CatName { get; set; } = string.Empty;
        public string? catthumb { get; set; }         // nvarchar(max)

        public string? GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;

        public List<ItemListDto> Items { get; set; }
    }

    public class ItemListDto
    {
        public int? ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public double? OIDRate { get; set; }
        //public double? ItemRate { get; set; }
        public bool OIDAvailable { get; set; }       // bit
        public bool ItemDiscountAllowed { get; set; }       // bit
        public string? thumb { get; set; }
        public bool? IsVeg { get; set; }
    }

    public class ItemGroupResponseDto
    {
        public int GrpCode { get; set; }
        public string GrpName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? Dep { get; set; }
    }

    public class ItemCategoryResponseDto
    {
        public decimal? CatCode { get; set; }
        public string CatName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? SubCategory { get; set; }
        public string? Thumbnail { get; set; }
    }

    public class ItemMasterResponseDto
    {
        public decimal? ItemCode { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal? CatCode { get; set; }
        public string GrpCode { get; set; } = string.Empty;
        public bool ItemDiscountAllowed { get; set; }
        public string? BranchCode { get; set; }
        public string? Thumbnail { get; set; }
        public bool? IsVeg { get; set; }
    }

    public class FoodBillDto
    {
        public int RNo { get; set; }
        public DateTime TrDate { get; set; }
        public int RcptNo { get; set; }
        public string GuestCode { get; set; } = string.Empty;
        public string GuestName { get; set; } = string.Empty;
        public string CheckInNo { get; set; } = string.Empty;
        public string RoomNo { get; set; } = string.Empty;
        public decimal BillAmt { get; set; }
        public string OutlateName { get; set; } = string.Empty;
    }

    public class KotFoodGroup
    {
        public int KotId { get; set; }
        public List<OldCartFoodModel> Items { get; set; }
    }

    public class CartResponse
    {
        public int Waiter { get; set; }
        public string WaiterName { get; set; }
        public int Pax { get; set; }
        public int KotId { get; set; }
        public int NCCode { get; set; }
        public string NCRemarks { get; set; }
        public List<OldCartFoodModel> Food { get; set; }
    }

    public class KOTBillModelDto
    {
        public string Billtype { get; set; }
        public int UserCode { get; set; }
        public int Outlet { get; set; }
        public string? OutletName { get; set; }
        public int KOTId { get; set; }
        public string KOTTblNo { get; set; }
        public string SubTable { get; set; } = string.Empty;
        public int Waiter { get; set; }
        public string WaiterName { get; set; }
        public int Pax { get; set; }
        public string Branchcode { get; set; }
        public int? NCCode { get; set; }
        public string? NCRemarks { get; set; }
        public DateTime KOTTime { get; set; }
        public int KotMinTimer { get; set; }
        public string TaxType { get; set; } = string.Empty;
        public string NCDepName { get; set; } = string.Empty;
        public TaxModel Tax { get; set; } = new();
        public List<KOTBillFoodModel> Food { get; set; } = new();
        public List<Printerdetaildto> Printers { get; set; } = new();
        public BillResponse FNBillResponse { get; set; } = new();
        public string Message { get; set; }
        public bool isDirectKOTandBill { get; set; }

    }

    public class KOTBillFoodModel
    {
        public int ItemCode { get; set; }
        public string Food { get; set; }
        public double ItemRate { get; set; }
        public string? Comment { get; set; }
        public int? Category { get; set; }
        public int GrpCode { get; set; }
        public int OrigQty { get; set; }
    }

    public class Printerdetaildto
    {
        public string PrinterName { get; set; }
        public string BillType { get; set; }
        public string Branch_Code { get; set; }
        public string OltCode { get; set; }
        public string PrintType { get; set; }
        public int? GrpCode { get; set; }
        public List<int> CategoryIds { get; set; }
        public string IPAddress { get; set; }
    }

    public class KotBillQueryResult
    {
        public int UserCode { get; set; }
        public int KOTId { get; set; }
        public string KOTTblNo { get; set; }
        public string SubTable { get; set; }
        public int Outlet { get; set; }
        public string? OutletName { get; set; }
        public int Waiter { get; set; }
        public string WaiterName { get; set; }
        public int Pax { get; set; }
        public string Branchcode { get; set; }
        public int? NCCode { get; set; }
        public string? NCRemarks { get; set; }
        public DateTime KOTTime { get; set; }

        public int ItemCode { get; set; }
        public string Food { get; set; }
        public double ItemRate { get; set; }
        public string? Comment { get; set; }
        public int? CatCode { get; set; }
        public int GrpCode { get; set; }
        public decimal OrigQty { get; set; }
    }

    public class BillResponse
    {
        public bool IsSuccess { get; set; }
        public string BillNo { get; set; }
        public string IPAddress { get; set; }
        public DateOnly BillDate { get; set; }
        public string BillTime { get; set; }

    }

    public class PaymentModeGrouped
    {
        public int ModeId { get; set; }
        public string ModeType { get; set; }
        public bool ModeRequired { get; set; }
        public string BranchCode { get; set; }

        public List<PaymentSubMode> SubModes { get; set; }
    }

    public class PaymentSubMode
    {
        public int SubModeId { get; set; }
        public string SubModeType { get; set; }
    }

    public class BillReprintResponse
    {
        public CartModel Cart { get; set; }
        public TaxModel Tax { get; set; }
        public string BillingType { get; set; } = string.Empty;
        public string SubBillingType { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class BillListResponse
    {
        public int KSMId { get; set; }
        public int KOTId { get; set; }
        public string OltCode { get; set; }
        public int KSMBillNo { get; set; }
        public DateTime KSMBillDate { get; set; }
        public DateTime KSMBillTime { get; set; }
        public decimal KSMBillAmount { get; set; }
        public decimal KSMBillTaxAmt { get; set; }
        public decimal KSMBillDiscount { get; set; }
        public decimal GrandTotal { get; set; }
        public bool KSMBillSettled { get; set; }
        public bool KSMBillCancled { get; set; }
        public string KSMTblNo { get; set; }
        public string DiscountPercent { get; set; }
        public string Reason { get; set; }
        public string Branch_Code { get; set; }
        public bool HasItemDiscount { get; set; } = false;
        public string PaymentStatus { get; set; } = string.Empty;
        public int CompanyCode { get; set; } = 0;
        public List<int> KOTIds { get; set; } = new();

    }

    public class CompanyBillsResponse
    {
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public List<CompanyBillModel> Bills { get; set; } = new();
    }

    public class ModifyBillDetailsresponse
    {
        public PosModifyBill KotDetails { get; set; }
        public BillDiscountModel DiscountDetails { get; set; }

    }

    public class PosModifyBillSettlementResponse
    {
        public bool Success { get; set; }
        public decimal DifferenceAmount { get; set; }
        public string Message { get; set; }
        public PosModifyBillSettlement Data { get; set; }
    }


    #region Bill Adjustment
    public class BillAdjustmentResponse
    {
        public List<BillAdjustmentModel> BillDetails { get; set; }
        public List<BillAdjustmentSettlementModel> ItemDetails { get; set; }
        public List<GroupTotalModel> GroupDetails { get; set; }
        
        public decimal TotalAmount { get; set; }
        public int TotalBillsFound { get; set; }
    }

    public class RankAmountResponse
    {
        public decimal EstimatedAmount { get; set; }
        public decimal BidAmount { get; set; }
    }

    public class SaveBidResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
    #endregion

    #region POS Room Service

    #endregion

}
