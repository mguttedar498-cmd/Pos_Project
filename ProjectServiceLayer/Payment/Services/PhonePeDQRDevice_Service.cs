
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.Payment.Interface;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace HMS_360_PMS.ProjectServiceLayer.Payment.Services
{
    public class PhonePeDQRDevice_Service : IPhonePeDQR_Service
    {
        public readonly IPhonePeDQR_Repository _repository;
        public PhonePeDQRDevice_Service(IPhonePeDQR_Repository repository)
        {
            _repository = repository;
        }

        #region DQR Device Payment
        public async Task<PhonePeTransaction> GetPhonePeTransaction()
        {
            var translist = await _repository.GetPhonePeTransaction();

            return translist;
        }

        public async Task<PhonePeCollectResponseBody> SendPaymentRequestDQRDevice(int amount, string TransNo)
        {
            var translist = await _repository.GetPhonePeTransaction();
            if(translist == null)
                return new PhonePeCollectResponseBody { success = false, code = "NO_CONFIG", message = "PhonePe transaction configuration not found.", data = null };

            // static varible
            string PHONEPE_STAGE_BASE_URL = translist.BaseUrl;

            string merchantKey = translist.MerchantKey;
            string merchantId = translist.MerchantId;
            string storeId = translist.StoreId;
            string terminalId = translist.TerminalId;
            int expiresIn = translist.ExpiresIn;
            string providerId = translist.ProviderId;
            string callbackurl = null;
            // Generate unique transaction ID
            //string transactionId = "TRXID" + DateTime.Now.ToString("yyyyMMddHHmmssfff");

            // Prepare response
            PhonePeCollectResponseBody responseBody = new PhonePeCollectResponseBody();

            // Build request object
            PhonePeCollectRequestDQRDevice phonePeCollectRequest = new PhonePeCollectRequestDQRDevice
            {
                merchantId = merchantId,
                merchantTransactionId = TransNo,
                orderId = TransNo,
                amount = amount,
                expiresIn = expiresIn,
                storeId = storeId,
                terminalId = terminalId,
                solutionType = "DQR_DEVICE"
            };

            // Convert request to JSON
            string jsonStr = JsonConvert.SerializeObject(phonePeCollectRequest);

            // Convert JSON to Base64
            string base64Json = ConvertStringToBase64(jsonStr);

            // Generate checksum

            string apiEndPoint = "/v1/payment/init" + merchantKey;
            string checksum = GenerateSha256ChecksumFromBase64Json(base64Json, apiEndPoint) + "###1";

            // Combine URL safely
            //baseUrl = baseUrl.TrimEnd('/');
            //string txnURL = $"{baseUrl}{apiEndPoint}";
            string txnURL = PHONEPE_STAGE_BASE_URL + "/v1/payment/init";

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";

                // Add headers
                webRequest.Headers.Add("X-VERIFY", checksum);
                webRequest.Headers.Add("X-PROVIDER-ID", providerId);
                //webRequest.Headers.Add("X-MERCHANT-ID", merchantId);
                //webRequest.Headers.Add("X-CALL-MODE", "POST");
                //webRequest.Headers.Add("X-CALLBACK-URL", callbackUrl);

                //webRequest.Headers.Add("X-CALLBACK-URL", string.IsNullOrEmpty(callbackUrl) ? "" : callbackUrl);

                // Wrap request
                PhonePeCollectApiRequestBody apiRequestBody = new PhonePeCollectApiRequestBody
                {
                    request = base64Json
                };

                string jsonBody = JsonConvert.SerializeObject(apiRequestBody);

                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter.Write(jsonBody);
                }

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(responseData))
                    {
                        responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(responseData);
                    }
                }

                return responseBody;
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (var reader = new StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string errorText = reader.ReadToEnd();

                        Console.WriteLine("ERROR RESPONSE:");
                        Console.WriteLine(errorText);

                        try
                        {
                            return JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(errorText);
                        }
                        catch
                        {
                            return new PhonePeCollectResponseBody
                            {
                                success = false,
                                code = "INVALID_RESPONSE",
                                message = errorText,
                                data = null
                            };
                        }
                    }
                }

                return new PhonePeCollectResponseBody
                {
                    success = false,
                    code = "NO_RESPONSE",
                    message = webEx.Message,
                    data = null
                };
            }
        }

        // Convert JSON string to Base64
        private string ConvertStringToBase64(string input)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(plainTextBytes);
        }

        // Generate SHA256 checksum
        //private string GenerateSha256Checksum(string base64Json, string apiEndPoint, string merchantKey)
        //{
        //    string rawString = base64Json + apiEndPoint + merchantKey;

        //    using (var sha256 = System.Security.Cryptography.SHA256.Create())
        //    {
        //        byte[] bytes = Encoding.UTF8.GetBytes(rawString);
        //        byte[] hash = sha256.ComputeHash(bytes);
        //        return BitConverter.ToString(hash).Replace("-", "").ToLower();
        //    }
        //}

        private string GenerateSha256ChecksumFromBase64Json(string base64JsonString, string jsonSuffixString)
        {
            string checksum = "";
            string raw = base64JsonString + jsonSuffixString;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));

                StringBuilder sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        public async Task<PhonePeCollectResponseBodyDQRDevice> SendCheckPaymentStatusRequestDQRDevice(string transno)
        {
            var translist = await _repository.GetPhonePeTransaction();

            if (translist == null)
            {
                return new PhonePeCollectResponseBodyDQRDevice
                {success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
            }

            //string transactionId = "trans12746565";

            string baseUrl = translist.BaseUrl;
            string merchantKey = translist.MerchantKey;
            string merchantId = translist.MerchantId;
            string providerId = translist.ProviderId;
            string keyIndex = "1"; // usually 1 in UAT, confirm with PhonePe

            string apiPath = $"/v1/payment/{merchantId}/{transno}/status" + merchantKey;

            // ✅ Proper X-VERIFY
            string xVerify = GenerateSha256ChecksumFromBase64Json("", apiPath) + "###1";

            Console.WriteLine("X-VERIFY: " + xVerify);

            // Call API
            return await CallPhonePeStatusApiDQRDevice(xVerify, transno);
        }

        private async Task<PhonePeCollectResponseBodyDQRDevice> CallPhonePeStatusApiDQRDevice(string xVerify, string transno)
        {
            var translist = await _repository.GetPhonePeTransaction();
            if (translist == null)
                return new PhonePeCollectResponseBodyDQRDevice { success = false, code = "NO_CONFIG", message = "PhonePe transaction configuration not found.", data = null };

            PhonePeCollectResponseBodyDQRDevice responseBody = new PhonePeCollectResponseBodyDQRDevice();

            string baseUrl = translist.BaseUrl;
            string merchantId = translist.MerchantId;
            string providerId = translist.ProviderId;
            string transactionId = "trans12746565";
            // Correct endpoint (no merchantKey here!)
            string urlSuffix = $"/v1/payment/{merchantId}/{transno}/status";
            string txnURL = baseUrl + urlSuffix;

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";

                // Very important headers
                webRequest.Headers.Add("X-VERIFY", xVerify);   // Must be calculated properly
                webRequest.Headers.Add("X-PROVIDER-ID", providerId);
                webRequest.Headers.Add("X-MERCHANT-ID", merchantId); // Some versions require this

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(responseData))
                    {
                        responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBodyDQRDevice>(responseData);
                    }
                }
                return responseBody;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response?.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string error = reader.ReadToEnd();
                    Console.WriteLine("Error Response: " + error);
                }
                return responseBody;
            }
        } 
        
        #endregion

        #region Own Device Payment
        public async Task<PhonePeCollectResponseBody> SendPaymentRequestOwnDevice(int amount, string TransNo)
        {
            var translist = await _repository.GetPhonePeImageRequest();

            if (translist == null)
            {
                return new PhonePeCollectResponseBody
                { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
            }
            // static varible
            string PHONEPE_STAGE_BASE_URL = translist.BaseUrl;

            string merchantKey = translist.MerchantKey;
            string merchantId = translist.MerchantId;
            string storeId = translist.StoreId;
            string terminalId = translist.TerminalId;
            int expiresIn = translist.ExpiresIn;
            string providerId = translist.ProviderId;
            string callbackurl = translist.CallbackUrl;
            // Generate unique transaction ID
            string transactionId = "TRXID" + DateTime.Now.ToString("ddMMyyHHmmss");

            // Prepare response
            PhonePeCollectResponseBody responseBody = new PhonePeCollectResponseBody();

            // Build request object
            PhonePeCollectRequest phonePeCollectRequest = new PhonePeCollectRequest
            {
                merchantId = merchantId,
                transactionId = TransNo,
                merchantOrderId = TransNo,
                amount = amount,
                expiresIn = expiresIn,
                storeId = storeId,
                terminalId = terminalId
            };

            // Convert request to JSON
            string jsonStr = JsonConvert.SerializeObject(phonePeCollectRequest);

            // Convert JSON to Base64
            string base64Json = ConvertStringToBase64(jsonStr);

            // Generate checksum

            string apiEndPoint = "/v3/qr/init" + merchantKey;
            string checksum = GenerateSha256ChecksumFromBase64Json(base64Json, apiEndPoint) + "###1";

            // Combine URL safely
            //baseUrl = baseUrl.TrimEnd('/');
            //string txnURL = $"{baseUrl}{apiEndPoint}";
            string txnURL = PHONEPE_STAGE_BASE_URL + "/v3/qr/init";

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";

                // Add headers
                webRequest.Headers.Add("X-VERIFY", checksum);
                webRequest.Headers.Add("X-PROVIDER-ID", providerId);
                webRequest.Headers.Add("X-CALL-MODE", "POST");
                webRequest.Headers.Add("X-CALLBACK-URL", callbackurl);

                //webRequest.Headers.Add("X-CALLBACK-URL", string.IsNullOrEmpty(callbackUrl) ? "" : callbackUrl);

                // Wrap request
                PhonePeCollectApiRequestBody apiRequestBody = new PhonePeCollectApiRequestBody
                {
                    request = base64Json
                };

                string jsonBody = JsonConvert.SerializeObject(apiRequestBody);

                using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    requestWriter.Write(jsonBody);
                }

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(responseData))
                    {
                        responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(responseData);
                    }
                }

                return responseBody;
            }
            catch (WebException webEx)
            {
                // Optional: read response from server in case of error
                if (webEx.Response != null)
                {
                    using (var errorResponse = new StreamReader(webEx.Response.GetResponseStream()))
                    {
                        string errorText = errorResponse.ReadToEnd();
                        Console.WriteLine("PhonePe API error: " + errorText);
                    }
                }
                return responseBody;
            }
        }

        // Convert JSON string to Base64
        //private string ConvertStringToBase64(string input)
        //{
        //    var plainTextBytes = Encoding.UTF8.GetBytes(input);
        //    return Convert.ToBase64String(plainTextBytes);
        //}

        // Generate SHA256 checksum
        //private string GenerateSha256Checksum(string base64Json, string apiEndPoint, string merchantKey)
        //{
        //    string rawString = base64Json + apiEndPoint + merchantKey;

        //    using (var sha256 = System.Security.Cryptography.SHA256.Create())
        //    {
        //        byte[] bytes = Encoding.UTF8.GetBytes(rawString);
        //        byte[] hash = sha256.ComputeHash(bytes);
        //        return BitConverter.ToString(hash).Replace("-", "").ToLower();
        //    }
        //}

        //private string GenerateSha256ChecksumFromBase64Json(string base64JsonString, string jsonSuffixString)
        //{
        //    string checksum = null;
        //    SHA256 sha256 = SHA256.Create();
        //    string checksumString = base64JsonString + jsonSuffixString;
        //    byte[] checksumBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(checksumString));
        //    //checksum = BitConverter.ToString(checksumBytes).Replace("-", string.Empty);
        //    foreach (byte b in checksumBytes)
        //    {
        //        checksum += $"{b:x2}";
        //    }
        //    return checksum;
        //}



        public async Task<PhonePeCollectResponseBody> CheckOwnDevicePaymentStatus(string transno)
        {
            var translist = await _repository.GetPhonePeImageRequest();

            if (translist == null)
            {
                return new PhonePeCollectResponseBody
                { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
            }
            string baseUrl = translist.BaseUrl;
            string transactionId = "trans12746565";

            string merchantKey = translist.MerchantKey; // salt key from PhonePe
            string merchantId = translist.MerchantId;
            string providerId = translist.ProviderId;
            string keyIndex = "1"; // usually 1 in UAT, confirm with PhonePe

            string apiPath = $"/v3/transaction/{merchantId}/{transno}/status{merchantKey}";

            // ✅ Proper X-VERIFY
            string xVerify = GenerateSha256ChecksumFromBase64Json("", apiPath) + "###1";
            Console.WriteLine("X-VERIFY: " + xVerify);

            // Call API
            return await CallPhonePeStatusApi(xVerify, transno);
        }

        private async Task<PhonePeCollectResponseBody> CallPhonePeStatusApi(string xVerify, string transno)
        {
            PhonePeCollectResponseBody responseBody = new PhonePeCollectResponseBody();

            var translist = await _repository.GetPhonePeImageRequest();

            if (translist == null)
            {
                return new PhonePeCollectResponseBody
                { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
            }

            string baseUrl = translist.BaseUrl;
            string merchantId = translist.MerchantId;
            string providerId = translist.ProviderId;
            string transactionId = "trans12746565";
            // Correct endpoint (no merchantKey here!)
            string urlSuffix = $"/v3/transaction/{merchantId}/{transno}/status";
            string txnURL = baseUrl + urlSuffix;

            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
                webRequest.Method = "GET";

                // Very important headers
                webRequest.Headers.Add("X-VERIFY", xVerify);   // Must be calculated properly
                webRequest.Headers.Add("X-MERCHANT-ID", merchantId); // Some versions require this
                webRequest.Headers.Add("X-PROVIDER-ID", providerId);
                webRequest.ContentType = "application/json";

                using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
                {
                    string responseData = responseReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(responseData))
                    {
                        responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(responseData);
                    }
                }
                return responseBody;
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response?.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    string error = reader.ReadToEnd();
                    Console.WriteLine("Error Response: " + error);
                }
                return responseBody;
            }
        }

#endregion

    }
}
