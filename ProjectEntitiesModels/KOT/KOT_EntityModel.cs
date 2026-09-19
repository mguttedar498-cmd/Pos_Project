//using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.EntitiesModels.POS;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace HMS_360_PMS.EntitiesModels.KOT
{
    public class OpenDayRequest
    {
        public int UserId { get; set; } = 0;
        public DateTime SystemTime { get; set; } = DateTime.Now;
        public DateTime SystemDate { get; set; } = DateTime.Now;
        public string BranchCode { get; set; } = string.Empty;

        //public string IpAddress { get; set; } = string.Empty;
        //public DateTime POSEntryDate { get; set; } = DateTime.Now;
    }

    public class CloseDayRequest
    {
        public int UserId { get; set; } = 0;
        public DateTime SystemTime { get; set; } = DateTime.Now;
        public DateTime POSEntryDate { get; set; } = DateTime.Now;
        public string BranchCode { get; set; } = string.Empty;
        public string Outlet { get; set; } = string.Empty;
    }

    public class OpenDayDetails
    {
        public int ShiftOpenedUserId { get; set; }
        public DateTime ShiftOpenTime { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftStatus { get; set; }
        public string Ipaddress { get; set; }
        public string BranchCode { get; set; }
    }


    #region Scanner Models

    public class CategoryModel
    {
        [JsonPropertyName("CategoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }

        [JsonPropertyName("thumb")]
        public string thumb { get; set; }

    }

    public class FoodImageModel
    {
        [JsonPropertyName("ItemCode")]
        public int ItemCode { get; set; }

        [JsonPropertyName("ItemName")]
        public string ItemName { get; set; }

        [JsonPropertyName("ItemRate")]
        public double ItemRate { get; set; }

        [JsonPropertyName("CatCode")]
        public int CatCode { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("thumb")]
        public string thumb { get; set; }

        [JsonPropertyName("Avaliable")]
        public bool Avaliable { get; set; }

        [JsonPropertyName("description")]
        public string description { get; set; }

        [JsonPropertyName("CurrentPrize")]
        public double CurrentPrize { get; set; }

        [JsonPropertyName("VATPER")]
        public double VATPER { get; set; }

        [JsonPropertyName("Rating")]
        public int Rating { get; set; }

        [JsonPropertyName("IsVeg")]
        public bool IsVeg { get; set; }

        [JsonPropertyName("Category")]
        public string Category { get; set; }
    }

    public class FoodImageModelMain
    {
        public FoodImageModel foodmodellists { get; set; }
        public List<FoodImageModel> foodmodellist { get; set; }
        public IsOpenResturent isopen { get; set; }
    }

    public class IsOpenResturent
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsOpen { get; set; }
        public string Message { get; set; }
    }

    public class TimingModel
    {
        public int DayId { get; set; }
        public string Day { get; set; }
        public string Branch_Code { get; set; }
    }

    public class Slots
    {
        public int DayId { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public bool Active { get; set; }
    }

    public class StewardModel
    {
        [JsonPropertyName("StewardCode")]
        public int StewardCode { get; set; }

        [JsonPropertyName("StewardName")]
        public string StewardName { get; set; }

        [JsonPropertyName("MobNo")]
        public string MobNo { get; set; }
    }

    public class OutletSelectModel
    {
        [JsonPropertyName("OltCode")]
        public int OltCode { get; set; }

        [JsonPropertyName("OltName")]
        public string OltName { get; set; }
    }

    public class CartScannerModel
    {
        [JsonPropertyName("Food")]
        public List<FoodScannerModel> Food { get; set; }

        [JsonPropertyName("Pax")]
        public int Pax { get; set; }

        [JsonPropertyName("Waiter")]
        public int Waiter { get; set; }

        [JsonPropertyName("WaiterName")]
        public string WaiterName { get; set; }

        [JsonPropertyName("NCCode")]
        public int NCCode { get; set; }

        [JsonPropertyName("NCRemarks")]
        public string NCRemarks { get; set; }

        [JsonPropertyName("GuestName")]
        public string GuestName { get; set; }

        [JsonPropertyName("KotMobileNo")]
        public string KotMobileNo { get; set; }
    }

    public class FoodScannerModel
    {
        [JsonPropertyName("Id")]
        public int Id { get; set; }

        [JsonPropertyName("Food")]
        public string Food { get; set; }

        [JsonPropertyName("code")]
        public string code { get; set; }

        [JsonPropertyName("Price")]
        public double Price { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Comment")]
        public string Comment { get; set; }

        [JsonPropertyName("Category")]
        public int Category { get; set; }

        [JsonPropertyName("GrpCode")]
        public int GrpCode { get; set; }

        [JsonPropertyName("OrigQty")]
        public int OrigQty { get; set; }

    }

    public class WaiterScannerModel
    {
        [JsonPropertyName("StwCode")]
        public int StwCode { get; set; }

        [JsonPropertyName("StwName")]
        public string StwName { get; set; }

        [JsonPropertyName("Pax")]
        public int Pax { get; set; }

        [JsonPropertyName("NCCode")]
        public int NCCode { get; set; }

        [JsonPropertyName("NCRemarks")]
        public string NCRemarks { get; set; }

        [JsonPropertyName("KotMobileNo")]
        public string KotMobileNo { get; set; }

        [JsonPropertyName("KOTGuestName")]
        public string KOTGuestName { get; set; }
    }

    public class BranchViewModal
    {
        [JsonPropertyName("Branch_Code")]
        public string Branch_Code { get; set; }

        [JsonPropertyName("Branch_name")]
        public string Branch_name { get; set; }

    }

    public class PGCreatePaymentResponse
    {
        public string orderId { get; set; }
        public string state { get; set; }
        public long expireAt { get; set; }
        public string redirectUrl { get; set; }
        public string merchantOrderId { get; set; }
        public bool success { get; set; }
    }

    public class PGTokenResponseModel
    {
        public string access_token { get; set; }
        public string encrypted_access_token { get; set; }
        public int expires_in { get; set; }
        //public int issued_at { get; set; }
        //public int expires_at { get; set; }
        public long issued_at { get; set; }
        public long expires_at { get; set; }
        public int session_expires_at { get; set; }
        public string token_type { get; set; }
    }

    public class PGPaymentOrderStatus
    {
        public string orderId { get; set; }
        public string state { get; set; }
        public int amount { get; set; }
        public long expireAt { get; set; }
        public MetaInfo metaInfo { get; set; }
        public List<PaymentDetail> paymentDetails { get; set; }
    }
    public class PaymentDetail
    {
        public string paymentMode { get; set; }
        public string transactionId { get; set; }
        public long timestamp { get; set; }
        public int amount { get; set; }
        public string state { get; set; }
    }

    public class RoomserviceModel
    {
        public string CheckInNo { get; set; }
        public string GuestCode { get; set; }
        public string GuestName { get; set; }
        public string Mobile { get; set; }
    }

    #endregion

    #region SubmitOrder Model

    public class KOTCartModel
    {
        [JsonPropertyName("UserCode")]
        public int UserCode { get; set; }

        [JsonPropertyName("Table")]
        public string Table { get; set; }

        [JsonPropertyName("SubTable")]
        public string SubTable { get; set; }

        [JsonPropertyName("Outlet")]
        public int Outlet { get; set; }

        [JsonPropertyName("OutletName")]
        public string OutletName { get; set; }

        [JsonPropertyName("Waiter")]
        public int Waiter { get; set; }

        [JsonPropertyName("WaiterName")]
        public string WaiterName { get; set; }

        [JsonPropertyName("Pax")]
        public int Pax { get; set; }

        [JsonPropertyName("Food")]
        public List<KOTFoodModel> Food { get; set; }

        [JsonPropertyName("Total")]
        public double Total { get; set; }

        [JsonPropertyName("TotQty")]
        public int TotQty { get; set; }

        [JsonPropertyName("Branch")]
        public string Branch { get; set; }

        [JsonPropertyName("Type")]
        public string Type { get; set; }

        [JsonPropertyName("NCCode")]
        public int NCCode { get; set; }

        [JsonPropertyName("NCRemarks")]
        public string NCRemarks { get; set; }

        [JsonPropertyName("Discount")]
        public double Discount { get; set; }

        [JsonPropertyName("DiscountType")]
        public string DiscountType { get; set; }

        [JsonPropertyName("DiscountRemarks")]
        public string DiscountRemarks { get; set; }

        [JsonPropertyName("VRemarks")]
        public string VRemarks { get; set; }

        [JsonPropertyName("Mode")]
        public string Mode { get; set; }

        [JsonPropertyName("SubBillType")]
        public string SubBillType { get; set; }

        [JsonPropertyName("Plan")]
        public string Plan { get; set; }

        [JsonPropertyName("GuestName")]
        public string GuestName { get; set; }

        [JsonPropertyName("GuestCode")]
        public string GuestCode { get; set; }

        [JsonPropertyName("CheckInNo")]
        public string CheckInNo { get; set; }

        [JsonPropertyName("KotMobileNo")]
        public string KotMobileNo { get; set; }

        //[JsonPropertyName("HomeDelivary")]
        //public KOTHomeDelivery HomeDelivary { get; set; }

        public class KOTFoodModel
        {
            [JsonPropertyName("Id")]
            public int Id { get; set; }

            //[JsonPropertyName("Itemid")]
            //public int Itemid { get; set; }

            [JsonPropertyName("Food")]
            public string Food { get; set; }

            [JsonPropertyName("code")]
            public string code { get; set; }

            [JsonPropertyName("Price")]
            public double Price { get; set; }

            [JsonPropertyName("Qty")]
            public int Qty { get; set; }

            [JsonPropertyName("Comment")]
            public string Comment { get; set; }

            [JsonPropertyName("Category")]
            public int Category { get; set; }

            [JsonPropertyName("OrigQty")]
            public int OrigQty { get; set; }
        }

        public class KOTHomeDelivery
        {
            [JsonPropertyName("GuestCode")]
            public decimal GuestCode { get; set; } = 0;

            [JsonPropertyName("TitleGn1")]
            public decimal TitleGn1 { get; set; } = 0;

            [JsonPropertyName("GuestName")]
            public string GuestName { get; set; } = string.Empty;

            [JsonPropertyName("DOB")]
            public DateTime DOB { get; set; } = DateTime.Now;   

            [JsonPropertyName("Address")]
            public string Address { get; set; } = string.Empty;

            [JsonPropertyName("City")]
            public string City { get; set; } = string.Empty;

            [JsonPropertyName("Phone")]
            public string Phone { get; set; } = string.Empty;   

            [JsonPropertyName("Email")]
            public string Email { get; set; } = string.Empty;

            [JsonPropertyName("Remarks")]
            public string Remarks { get; set; } = string.Empty;

            [JsonPropertyName("LastModify")]
            public DateTime LastModify { get; set; } = DateTime.Now;

            [JsonPropertyName("Discount")]
            public decimal Discount { get; set; } = 0;

            [JsonPropertyName("Branch_code")]
            public string Branch_code { get; set; } = string.Empty;

            [JsonPropertyName("isUpdate")]
            public int? isUpdate { get; set; } = 0;
        }
    }

    //public class SaveKOTDetailRequest
    //{
    //    public int KOTId { get; set; }
    //    public int KOTNo { get; set; }
    //    public int ItemCode { get; set; }
    //    public double KOTDRate { get; set; }
    //    public int KOTDQty { get; set; }
    //    public string SpecialInstId { get; set; } // getSpecialInfoId(item.Comment)
    //    public string BranchCode { get; set; }
    //    public string IsFree { get; set; } = "False";
    //    public double ItemDiscount { get; set; } = 0;
    //    public int IsOnline { get; set; } = 0;
    //    public int KNQty { get; set; } = 0;
    //    public string FinCode { get; set; }
    //}

    #endregion

    #region GetBill Model

    public class KOTTaxModel
    {
        [JsonPropertyName("TotalAmount")]
        public double TotalAmount { get; set; }

        [JsonPropertyName("TotalQty")]
        public int TotalQty { get; set; }

        [JsonPropertyName("CGSTPer")]
        public double CGSTPer { get; set; }

        [JsonPropertyName("CGSTAmt")]
        public double CGSTAmt { get; set; }

        [JsonPropertyName("SGSTPer")]
        public double SGSTPer { get; set; }

        [JsonPropertyName("SGSTAmt")]
        public double SGSTAmt { get; set; }

        [JsonPropertyName("ServiceChargePer")]
        public double ServiceChargePer { get; set; }

        [JsonPropertyName("ServiceCharge")]
        public double ServiceCharge { get; set; }

        [JsonPropertyName("GrandTotal")]
        public double GrandTotal { get; set; }

        [JsonPropertyName("DiscountPer")]
        public double DiscountPer { get; set; }

        [JsonPropertyName("Discount")]
        public double Discount { get; set; }

        [JsonPropertyName("DiscountRemarks")]
        public string DiscountRemarks { get; set; }

        [JsonPropertyName("RoundOff")]
        public double RoundOff { get; set; }

        [JsonPropertyName("TaxList")]
        public List<KOTTaxdetails> TaxList { get; set; }
    }

    public class KOTTaxdetails
    {
        [JsonPropertyName("TaxName")]
        public string TaxName { get; set; }

        [JsonPropertyName("Taxper")]
        public double Taxper { get; set; }

        [JsonPropertyName("TaxableAmount")]
        public double TaxableAmount { get; set; }

        [JsonPropertyName("TaxAmount")]
        public double TaxAmount { get; set; }

    }

    #endregion

    #region PostBill Model

    public class KOTBillModel
    {
        public KOTCartModel Cart { get; set; }
        public KOTTaxModel Tax { get; set; }
        public string BillingType { get; set; }
        public string SubBillingType { get; set; }
        public KOTPhonePeCollectResponseBody paymentresponse { get; set; }
    }

    public class BillModelForBIll
    {
        public KOTCartModel Cart { get; set; }
        public KOTTaxModel Tax { get; set; }
        public string BillingType { get; set; }
        public string SubBillingType { get; set; }
        public KOTPhonePeCollectResponseBody paymentresponse { get; set; }
        public KOTBillHeaderdetilsforBill billdetails { get; set; }

    }

    public class KOTPhonePeCollectResponseBody
    {
        public bool success { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public KOTPhonePeData data { get; set; }
    }
    public class KOTPhonePeData
    {
        public string transactionId { get; set; }
        public int amount { get; set; }
        public string merchantId { get; set; }
        public string providerReferenceId { get; set; }
        public string qrString { get; set; }
    }

    public class KOTBillHeaderdetilsforBill
    {
        public string Billno { get; set; }
        public string BillDate { get; set; }
        public string BillTime { get; set; }
        public string OutletName { get; set; }
        public string TokenNo { get; set; }
        public string OrderId { get; set; }
    }

    //public class FoodModel
    //{
    //    public int Id { get; set; }
    //    public string Food { get; set; }
    //    public double Price { get; set; }
    //    public int Qty { get; set; }
    //    public string Comment { get; set; }
    //    public int Category { get; set; }
    //}
    #endregion

    public class Whatsupconfig
    {
        public string Token { get; set; }
        public string APIEnd { get; set; }
        public string template_name { get; set; }
        public string template_language { get; set; }
        public string ContactNo { get; set; }
        public string WhatsupimageURL { get; set; }
        public string appid { get; set; }

    }

    #region SubmitFastFoodBill Model

    #endregion

    public class OnlinePaymentTypeModel
    {
        public bool IsQRActive { get; set; } = false;
    }

    public class EmailRequest
    {
        public int UserId { get; set; } = 0;
        public DateTime FromDate { get; set; } = DateTime.Now;
        public DateTime ToDate { get; set; } = DateTime.Now;
        public string BranchCode { get; set; } = string.Empty;
    }
}
