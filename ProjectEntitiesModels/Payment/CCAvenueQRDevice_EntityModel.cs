namespace HMS_360_PMS.ProjectEntitiesModels.Payment
{
    public class CCApiRequest
    {
        public string EncRequest { get; set; } = string.Empty;
        public string AccessCode { get; set; }= "AVXP89NC47AU98PXUA";
        public string Command { get; set; } = "makePayment";
        public string RequestType { get; set; } = "JSON";
        public string ResponseType { get; set; } = "JSON";
        public string Version { get; set; } = "1.2";
    }
    public class TerminalDetailsResponse
    {
        public List<TerminalInfo> Terminals { get; set; } = new List<TerminalInfo>();
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDesc { get; set; } = string.Empty;
    }

    public class TerminalInfo
    {
        public string GtwId { get; set; } = string.Empty;
        public string AccountMid { get; set; } = string.Empty;
        public string AccountMidName { get; set; } = string.Empty;
        public string AccountTid { get; set; } = string.Empty;
    }

    public class MakePaymentRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public string EmailId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string CardBin { get; set; } = string.Empty;
        public string EmiPlanId { get; set; } = string.Empty;
        public string TenureId { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR";
        public string TerminalId { get; set; } = string.Empty;
        public string Device { get; set; } = "POS";
        public string PaymentType { get; set; } = "SALE";
        public string TransactionType { get; set; } = "CARD";
    }

    public class MakePaymentResponse
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string ErrorDesc { get; set; } = string.Empty;
        public string TrackingId { get; set; } = string.Empty;
    }

    public class TransactionStatusRequest
    {
        public string TrackingId { get; set; } = string.Empty;
    }

    public class TransactionStatusResponse
    {
        public string TransactionId { get; set; } = string.Empty;

        public string TransactionTime { get; set; } = string.Empty;

        public string Amount { get; set; } = string.Empty;

        public string TransactionStatus { get; set; } = string.Empty;

        public string BankRefNuber { get; set; } = string.Empty;
    }
}
