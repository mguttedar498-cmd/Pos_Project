using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.Payment.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HMS_360_PMS.ProjectServiceLayer.Payment.Services
{
    public class CCAvenueQRDevice_Service : ICCAvenueQRDevice_Service
    {
        public readonly ICCAvenueQRDevice_Repository _repository;

        private readonly HttpClient _httpClient;

        private const string MerchantId = "4429672";
        private const string AccessCode = "AVXP89NC47AU98PXUA";
        private const string WorkingKey = "B78ADA291271E37FFD5E973AAD13666B";

        private const string ApiUrl = "https://apitest.ccavenue.com/apis/servlet/DoWebTrans";

        public CCAvenueQRDevice_Service(ICCAvenueQRDevice_Repository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        public async Task<MakePaymentResponse> MakePaymentAsync( MakePaymentRequest request)
        {
            //var terminals = await FetchTerminalDetailsAsync();

            //var terminalId = terminals.Terminals.FirstOrDefault()?.AccountTid;

            //request.TerminalId = terminalId;

            var requestJson = JsonConvert.SerializeObject(request);

            var encryptedRequest = Encrypt(requestJson, WorkingKey);

            var formData = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                { "enc_request", encryptedRequest },
                { "access_code", AccessCode },
                { "command", "makePayment" },
                { "request_type", "JSON" },
                { "response_type", "JSON" },
                { "version", "1.2" }
                });

            var response = await _httpClient.PostAsync(ApiUrl, formData);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var parsed = System.Web.HttpUtility.ParseQueryString(responseString);

            string status = parsed["status"];
            string encResponse = parsed["enc_response"];

            if (status == "1")
            {
                throw new Exception(encResponse);
            }

            var decryptedJson = Decrypt(encResponse, WorkingKey);

            return JsonConvert.DeserializeObject<MakePaymentResponse>(decryptedJson);
        }

        private static string Encrypt(string plainText, string workingKey)
        {
            byte[] keyBytes =
                MD5.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(workingKey));

            byte[] iv = Enumerable.Range(0, 16)
                                  .Select(i => (byte)i)
                                  .ToArray();

            using var aes = Aes.Create();

            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var encryptor = aes.CreateEncryptor();

            byte[] inputBytes =
                Encoding.UTF8.GetBytes(plainText);

            byte[] encryptedBytes =
                encryptor.TransformFinalBlock(
                    inputBytes,
                    0,
                    inputBytes.Length);

            return Convert.ToHexString(encryptedBytes)
                .ToLower();
        }

        private static string Decrypt(string cipherText, string workingKey)
        {
            byte[] keyBytes =
                MD5.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(workingKey));

            byte[] iv = Enumerable.Range(0, 16)
                                  .Select(i => (byte)i)
                                  .ToArray();
            byte[] cipherBytes =
                Convert.FromHexString(cipherText);

            using var aes = Aes.Create();

            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decryptor = aes.CreateDecryptor();

            var decryptedBytes =
                decryptor.TransformFinalBlock(
                    cipherBytes,
                    0,
                    cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public async Task<TerminalDetailsResponse> FetchTerminalDetailsAsync()
        {
            var requestJson = "{}";

            var encrypted = Encrypt(requestJson, WorkingKey);

            var formData =
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                { "enc_request", encrypted },
                { "access_code", AccessCode },
                { "command", "fetchTerminalDetails" },
                { "request_type", "JSON" },
                { "response_type", "JSON" },
                { "version", "1.2" }
                    });

            var response = await _httpClient.PostAsync(ApiUrl, formData);

            var content = await response.Content.ReadAsStringAsync();

            return DecryptResponse<TerminalDetailsResponse>(content);
        }

        public async Task<TransactionStatusResponse> CheckTransactionStatus(string trackingId)
        {
            var request =
                new TransactionStatusRequest
                {
                    TrackingId = trackingId
                };

            var requestJson = JsonConvert.SerializeObject(request);

            var encrypted = Encrypt(requestJson, WorkingKey);

            var formData =
                new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                { "enc_request", encrypted },
                { "access_code", AccessCode },
                { "command", "checkTransactionStatus" },
                { "request_type", "JSON" },
                { "response_type", "JSON" },
                { "version", "1.2" }
                    });

            var response = await _httpClient.PostAsync(ApiUrl, formData);

            var content = await response.Content.ReadAsStringAsync();

            return DecryptResponse<TransactionStatusResponse>(content);
        }

        private T DecryptResponse<T>(string response)
        {
            var parsed =
                System.Web.HttpUtility
                .ParseQueryString(response);

            string status = parsed["status"];

            string encResponse = parsed["enc_response"];

            if (status == "1")
            {
                throw new Exception(encResponse);
            }

            string decrypted = Decrypt(encResponse, WorkingKey);

            return JsonConvert.DeserializeObject<T>(decrypted);
        }
    }
}
