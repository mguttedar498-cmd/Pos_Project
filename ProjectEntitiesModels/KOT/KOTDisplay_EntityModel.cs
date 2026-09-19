using System.Text.Json.Serialization;

namespace HMS_360_PMS.ProjectEntitiesModels.KOT
{
    public class DepartmentModel
    {
        [JsonPropertyName("DepCode")]
        public int DepCode { get; set; }

        [JsonPropertyName("DepName")]
        public string DepName { get; set; }
    }

    public class TblKOTDisplayModel
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; }

        [JsonPropertyName("Res")]
        public string Res { get; set; }

        [JsonPropertyName("Tbl")]
        public string Tbl { get; set; }

        [JsonPropertyName("Item")]
        public string Item { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Cmnt")]
        public string Cmnt { get; set; } = string.Empty;

        [JsonPropertyName("KotTime")]
        public string KotTime { get; set; }

        [JsonPropertyName("KotNo")]
        public string KotNo { get; set; }

        [JsonPropertyName("Barked")]
        public string Barked { get; set; }

        [JsonPropertyName("BarkedTime")]
        public string BarkedTime { get; set; }

        [JsonPropertyName("Ready")]
        public string Ready { get; set; }

        [JsonPropertyName("ReadyTime")]
        public string ReadyTime { get; set; }

        [JsonPropertyName("Picked")]
        public string Picked { get; set; }

        [JsonPropertyName("PickedTime")]
        public string PickedTime { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("ItemCode")]
        public int ItemCode { get; set; }

        [JsonPropertyName("depcode")]
        public int depcode { get; set; }
    }

    public class KOTDisplayModel
    {
        [JsonPropertyName("Res")]
        public string Res { get; set; }

        [JsonPropertyName("Tbl")]
        public string Tbl { get; set; }

        [JsonPropertyName("Item")]
        public string Item { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Cmnts")]
        public string Cmnts { get; set; }

        [JsonPropertyName("Time")]
        public string Time { get; set; }

        [JsonPropertyName("Kot")]
        public string Kot { get; set; }

        [JsonPropertyName("Itemcode")]
        public string Itemcode { get; set; }

        [JsonPropertyName("Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("depcode")]
        public int depcode { get; set; }
    }

    public class ViewKOTDisplayModel
    {
        [JsonPropertyName("Res")]
        public string Res { get; set; }

        [JsonPropertyName("Tbl")]
        public string Tbl { get; set; }

        [JsonPropertyName("Item")]
        public string Item { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Cmnts")]
        public string Cmnts { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("Time")]
        public string Time { get; set; }

        [JsonPropertyName("Kot")]
        public string Kot { get; set; }

        [JsonPropertyName("Priority")]
        public int Priority { get; set; }

        [JsonPropertyName("DumpStatus")]
        public string DumpStatus { get; set; }

        [JsonPropertyName("ItemCode")]
        public string ItemCode { get; set; }

        [JsonPropertyName("Depcode")]
        public string Depcode { get; set; }

        [JsonPropertyName("CheckinNo")]
        public string CheckinNo { get; set; }

        [JsonPropertyName("KDQty")]
        public string KDQty { get; set; }

    }

    public class KOTnQtyModel
    {
        [JsonPropertyName("KOTNo")]
        public string KOTNo { get; set; }

        [JsonPropertyName("KOTDQty")]
        public int KOTDQty { get; set; }
    }

    public class KOTModifyDetailsModel
    {
        [JsonPropertyName("KOTId")]
        public string KOTId { get; set; }

        [JsonPropertyName("ItemCode")]
        public string ItemCode { get; set; }

        [JsonPropertyName("ItemQty")] 
        public string ItemQty { get; set; }

        [JsonPropertyName("UserCode")]
        public string UserCode { get; set; }

        [JsonPropertyName("LastModify")]
        public string LastModify { get; set; }

        [JsonPropertyName("OutCode")]
        public string outCode { get; set; }

        [JsonPropertyName("Pres_ItemQty")]
        public string Pres_ItemQty { get; set; }

        [JsonPropertyName("Branch_Code")]
        public string Branch_Code { get; set; }
    }

}
