namespace HMS_360_PMS.ProjectEntitiesModels.GeneralSettings
{
    public class ProductLicenceServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    public class ProductLicenceModel
    {
        public string SerialKey { get; set; } = string.Empty;
        public string ProductKey { get; set; } = string.Empty;
        public DateTime TrDate { get; set; } = DateTime.Now;
        public DateTime ValidDate { get; set; } = DateTime.Now;
        public string NoDays { get; set; } = string.Empty;
        public DateTime IntimationDate { get; set; } = DateTime.Now;
        public int IndimationDays { get; set; } = 0;
        public string ClientName { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public bool Status { get; set; } = false;
        public string BranchCode { get; set; } = string.Empty;
        public string Encryptedserialkey { get; set; } = string.Empty;
        public string EncryptedToDate { get; set; } = string.Empty;
        public string EncryptedIntimationDate { get; set; } = string.Empty;

    }

    public class InsertProductLicenceModel
    {
        public string SerialKey { get; set; } = string.Empty;
        public string ProductKey { get; set; } = string.Empty;
        public DateTime TrDate { get; set; } = DateTime.Now;
        public DateTime ValidDate { get; set; } = DateTime.Now;
        public string ClientName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

    }
}
