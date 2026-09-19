using System.Text.Json.Serialization;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs
{

    public class KOTServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }


    public class OpenDayResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        //public List<PurchaseExpiryDto> ExpiryItems { get; set; } = new();
    }

    public class PurchaseExpiryDto
    {
        public int PNo { get; set; }
        public string ItemName { get; set; }
        public decimal PurchasedQty { get; set; }
        public decimal UsedQty { get; set; }
        public decimal UnUsedQty { get; set; }
    }

    public class CloseDayResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PopupStatus { get; set; } = string.Empty;
    }

    public class OutletdetailsResponse
    {
        public int OltCode { get; set; }
        public string OltName { get; set; }
    }


    public class OpenDayDetailsResponse
    {
        public OpenDayResponse OpenDayResponse { get; set; }
        public int ShiftOpenedUserId { get; set; }
        public DateTime ShiftOpenTime { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftStatus { get; set; }
        public string Ipaddress { get; set; }
    }

    public class CompanyInfoScannerDto
    {
        [JsonPropertyName("Company_Name")]
        public string Company_Name { get; set; }

        [JsonPropertyName("Company_code")]
        public int Company_code { get; set; }

        [JsonPropertyName("StartYear")]
        public DateTime StartYear { get; set; }

        [JsonPropertyName("Address1")]
        public string Address1 { get; set; }

        [JsonPropertyName("Address2")]
        public string Address2 { get; set; }

        [JsonPropertyName("Phone_number")]
        public string Phone_number { get; set; }

        [JsonPropertyName("Mob_number")]
        public string Mob_number { get; set; }

        [JsonPropertyName("OwnerName")]
        public string OwnerName { get; set; }

        [JsonPropertyName("Owner_Number")]
        public string Owner_Number { get; set; }

        [JsonPropertyName("Fax_number")]
        public string Fax_number { get; set; }

        [JsonPropertyName("Email_id")]
        public string Email_id { get; set; }

        [JsonPropertyName("Tin_no")]
        public string Tin_no { get; set; }

        [JsonPropertyName("Licence_number")]
        public string Licence_number { get; set; }

        [JsonPropertyName("Branch_code")]
        public string Branch_code { get; set; }

        [JsonPropertyName("STDCODE")]
        public string STDCODE { get; set; }

        [JsonPropertyName("Logo")]
        public string Logo { get; set; } = string.Empty;
    }

    #region SubmitOrder Model

    #endregion

    #region GetBill Model

    #endregion

    #region PostBill Model

    #endregion

    #region SubmitFastFoodBill Model

    #endregion

}
