using System.Text.Json.Serialization;

namespace HMS_360_PMS.ProjectEntitiesModels.KOT
{
    public class PFoodModel
    {
        [JsonPropertyName("Kot")]
        public int Kot { get; set; }

        [JsonPropertyName("Code")]
        public int Code { get; set; }

        [JsonPropertyName("MyProperty")]
        public string MyProperty { get; set; }

        [JsonPropertyName("Qty")]
        public int Qty { get; set; }

        [JsonPropertyName("Time")]
        public string Time { get; set; }

        [JsonPropertyName("Barked")]
        public bool Barked { get; set; }

        [JsonPropertyName("Ready")]
        public bool Ready { get; set; }

        [JsonPropertyName("Picked")]
        public bool Picked { get; set; }

        [JsonPropertyName("Cmnts")]
        public string Cmnts { get; set; }
    }
}
