namespace HMS_360_PMS.ProjectEntitiesModels.Payment
{
    public class PhonePeCollectRequestDQRDevice
    {
        public string storeId { get; set; }
        public string merchantId { get; set; }
        public string terminalId { get; set; }
        public string merchantTransactionId { get; set; }
        public string orderId { get; set; }
        public int amount { get; set; }
        public string solutionType { get; set; }
        public int expiresIn { get; set; }
    }

    public class Dataqr
    {
        public string merchantId { get; set; }
        public string storeId { get; set; }
        public string terminalId { get; set; }
        public string orderId { get; set; }
        public string transactionId { get; set; }
        public string referenceNumber { get; set; }
        public int amount { get; set; }
        public string status { get; set; }
        public string responseCode { get; set; }
        public List<PaymentInstrument> paymentInstruments { get; set; }
        public string phonepeTransactionId { get; set; }
        public long timestamp { get; set; }
    }

    public class PaymentInstrument
    {
        public string type { get; set; }
        public int amount { get; set; }
        public string utr { get; set; }
        public string upiTransactionId { get; set; }
    }

    public class PhonePeCollectResponseBodyDQRDevice
    {
        public bool success { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public Dataqr data { get; set; }
    }

    public class PhonePeTransaction
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

    public class PhonePeCollectApiRequestBody
    {
        public string request { get; set; }
    }
}
