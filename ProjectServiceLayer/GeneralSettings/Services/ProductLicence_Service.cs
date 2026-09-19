using DocumentFormat.OpenXml.Bibliography;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Interfaces;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services
{
    public class ProductLicence_Service : IProductLicence_Service
    {
        private readonly IProductLicence_Repository _repository;

        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");

        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

        public ProductLicence_Service(IProductLicence_Repository repository)
        {
            _repository = repository;
        }

        public async Task<ProductLicenceModel> GetProductLicenceKey(string branchcode)
        {
            var productLicenceKey = await _repository.GetProductLicenceKey(branchcode);
            return productLicenceKey;
        }

        public async Task<ProductLicenceServiceResult<int>> SaveProductLicenceKey(InsertProductLicenceModel model)
        {
            DateTime currentdate = ConvertUtcToIst();

            string serial = model.SerialKey;
            DateTime fromdate = model.TrDate;
            DateTime todate = model.ValidDate;
            int totaldays = (todate.Date - fromdate.Date).Days;
            DateTime intimationdate = todate.AddDays(-10);
            int indimationdays = 10;

            //if (totaldays < indimationdays)
            //{
            //    return new ProductLicenceServiceResult<int>
            //    {
            //        Success = false,
            //        Message = "Please Enter ValidDate, Validate Should be More then 10 Days",
            //        Data = 0
            //    };
            //}

            //string ser = "1234-5678-9012-3456";
            string ser1 = serial.Replace("-", "");

            bool validproductkey = await DualCrackCode(decimal.Parse(model.ProductKey), decimal.Parse(ser1));

            if (!validproductkey)
            {
                return new ProductLicenceServiceResult<int>
                {
                    Success = false,
                    Message = "Invalid product or serial key.",
                    Data = 0
                };
            }

            string encryptedserialkey= Encrypt(model.SerialKey);
            string encryptedToDate = Encrypt(todate.ToString("yyyy-MM-dd"));
            string encryptedIntimationDate = Encrypt(intimationdate.ToString("yyyy-MM-dd"));

            var result =  await _repository.SaveProductLicenceKey(model, totaldays, intimationdate, indimationdays, encryptedserialkey, encryptedToDate, encryptedIntimationDate);

            if (result > 0)
            {
                return new ProductLicenceServiceResult<int>
                {
                    Success = true,
                    Message = "Successfully saved product or serial key.",
                    Data = result
                };
            }
            else
            {
                return new ProductLicenceServiceResult<int>
                {
                    Success = false,
                    Message = "Failed to save product or serial key.",
                    Data = result
                };
            }
        }

        public DateTime ConvertUtcToIst()
        {
            DateTime utcNow = DateTime.UtcNow;

            // Ensure input is treated as UTC
            if (utcNow.Kind != DateTimeKind.Utc)
            {
                utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
            }

            TimeZoneInfo istZone;

            // Handle Windows & Linux
            try
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch
            {
                istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, istZone);
        }

        public async Task<bool> DualCrackCode(decimal productKey, decimal serialKey)
        {
            try
            {
                decimal keyValue = decimal.Round(productKey, 0);
                keyValue = decimal.Round(keyValue / 120, 0);
                keyValue = decimal.Round(keyValue - 99, 0);
                keyValue = decimal.Round(keyValue / 15, 0);

                return decimal.Round(keyValue, 0) == decimal.Round(serialKey, 0);
            }
            catch
            {
                return false;
            }
        }

        public string Encrypt(string plainText)
        {
            //var plainText = DateTime.Now.ToString("yyyyMMddHHmmss");

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor();

            byte[] input = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(input, 0, input.Length);

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string cipher)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor();

            byte[] input = Convert.FromBase64String(cipher);
            byte[] decrypted = decryptor.TransformFinalBlock(input, 0, input.Length);

            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
