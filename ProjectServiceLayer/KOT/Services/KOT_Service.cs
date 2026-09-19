using Dapper;
using HMS_360_PMS.DAL_Layers.KOT;
using HMS_360_PMS.DAL_Layers.POS;
using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.Helper.Security;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using HMS_360_PMS.Services_Layers.KOT.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using static Dapper.SqlMapper;


namespace HMS_360_PMS.Services_Layers.KOT.Service
{
    public class KOT_Service : IKOT_Service
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKOT_Repository _repository;
        private readonly KOT_DAL _kotdal;
        private readonly KotBillSettlement_DAL _kotbillsettlementdal;
        private readonly NCKotBillSettlement_DAL _nckotbillsettlementdal;
        private readonly AppSettings _appSettings;
        private readonly PhonePeSettings _phonePe;
        private readonly EmailSender_Service _emailsender;
        public KOT_Service(IHttpClientFactory httpClientFactory, IKOT_Repository repository, KOT_DAL kotdal, KotBillSettlement_DAL kotbillsettlementdal, NCKotBillSettlement_DAL nckotbillsettlementdal, IOptions<AppSettings> appSettings, IOptions<PhonePeSettings> phonePe, EmailSender_Service emailsender)
        {
            _httpClientFactory = httpClientFactory;
            _repository = repository;
            _kotdal = kotdal;
            _kotbillsettlementdal = kotbillsettlementdal;
            _nckotbillsettlementdal = nckotbillsettlementdal;
            _appSettings = appSettings.Value;
            _phonePe = phonePe.Value;
            _emailsender = emailsender;

        }

        public static string GetActiveIPAddress()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    var properties = ni.GetIPProperties();

                    foreach (var address in properties.UnicastAddresses)
                    {
                        if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return address.Address.ToString();
                        }
                    }
                }
            }

            return "No active IP found";
        }

        public async Task<OpenDayResponse> DayOpenAsync(OpenDayRequest request)
        {
            var response = new OpenDayResponse();

            try
            {
                string LocalIPAddress = GetActiveIPAddress();
                //if (LocalIPAddress != null && LocalIPAddress != "")
                //{
                //    request.IpAddress = LocalIPAddress;
                //}

                var rows = await _repository.InsertShiftAsync(request, LocalIPAddress);

                if (rows == 1)
                {
                    response.Success = true;
                    response.Message = "Day opened successfully";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Cannot Open The Day!, Already Day is opened";
                }

                //var expiryItems = await _repository.GetExpiryItemsAsync(request.POSEntryDate);
                //response.ExpiryItems = expiryItems.ToList();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<CloseDayResponse> DayCloseAsync(CloseDayRequest request)
        {
            try
            {
                // 1. Running KOT check
                if (await _repository.GetRunningKotsCount(request.BranchCode) > 0)
                    return new CloseDayResponse
                    {
                        Success = false,
                        Message = "Clear All Running Kot's in Running Order Window",
                        PopupStatus = "/showKOTcancelscreen"
                    };

                // 2. Pending bills check
                if (await _repository.GetPendingBillsCount(request.BranchCode) > 0)
                    return new CloseDayResponse
                    {
                        Success = false,
                        Message = "Clear All Pending Bills From Settlement Window",
                        PopupStatus = "/showBillsettlescreen"
                    };

                // 3. Close Shift
                var closed = await _repository.CloseShiftAsync(request.UserId, request.SystemTime, request.POSEntryDate, request.BranchCode);

                if (closed == 1)
                {
                    var branchName = await _repository.GetBrancheName(request.BranchCode);
                    // 4. Data movement
                    var insertedRows = await _repository.InsertKotDisplayMaster();

                    if(insertedRows > 0)
                    {
                        await _repository.DeleteKotDisplay();
                    }

                    var insertedRows1 = await _repository.InsertTmpKotPrint();
                    if (insertedRows1 > 0)
                    {
                        await _repository.DeleteTmpKotPrint();
                    }

                    var insertedRows2 = await _repository.InsertTmpKotPrintNew();
                    if (insertedRows2 > 0)
                    {
                        await _repository.DeleteTmpKotPrintNew();
                    }

                    await _repository.DeleteLoggers();

                    // 5. Updates
                    await _repository.UpdateSettlement(request.POSEntryDate, request.BranchCode);
                    await _repository.UpdateKotMaster(request.POSEntryDate, request.BranchCode);

                    // 6. Outlet
                    var OutletDetails = await _repository.GetOutletDetails(request.BranchCode);

                    // 7. From-To Date
                    await _repository.DeleteFromToDate();
                    await _repository.InsertFromToDate(request.POSEntryDate);

                    // 8. SMS Collection logic (FULLY PRESERVED)
                    var sms = await _repository.GetSMSCollection();

                    double CollAmt = 0, CashAmt = 0, CardAmt = 0,
                           OnlineAmt = 0, CreditAmt = 0, PendingAmt = 0;

                    if (sms.Count >= 6)
                    {
                        CollAmt = sms[0];
                        CashAmt = sms[1];
                        CardAmt = sms[2];
                        OnlineAmt = sms[3];
                        CreditAmt = sms[4];
                        PendingAmt = sms[5];
                    }

                    // 👉 Here you plug:
                    // SMS / WhatsApp / Email services (same as WinForms)
                    #region Email Notfication

                    await _emailsender.SendEmailDailyClose(request, branchName);

                    #endregion

                    return new CloseDayResponse
                    {
                        Success = true,
                        Message = "Day Is Closed Successfully"
                    };
                }
                else
                {
                    return new CloseDayResponse
                    {
                        Success = false,
                        Message = "Shift closing failed"
                    };
                }
            }
            catch (Exception ex)
            {
                return new CloseDayResponse
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<OpenDayDetailsResponse> GetOpenDayDetais(int userid, string branchCode)
        {
            var details = await _repository.GetOpenDayDetails(userid, branchCode);

            var data = details;

            if (data == null)
            {
                return new OpenDayDetailsResponse
                {
                    OpenDayResponse = new OpenDayResponse
                    {
                        Success = false,
                        Message = "Please open the day first to continue further!"
                    }
                };
            }

            return new OpenDayDetailsResponse
            {
                ShiftOpenedUserId = data.ShiftOpenedUserId,
                ShiftOpenTime = data.ShiftOpenTime,
                ShiftDate = data.ShiftDate,
                ShiftStatus = data.ShiftStatus,
                Ipaddress = data.Ipaddress,

                OpenDayResponse = new OpenDayResponse
                {
                    Success = true,
                    Message = "Success"
                }
            };
        }


        #region Scanner Services
        public async Task<List<CategoryModel>> GetFoodCategories(string Branchcode)
        {
            var footcategory = await _repository.GetFoodCategories(Branchcode);
            return footcategory;
        }

        public async Task<FoodImageModelMain> GetFoodsinImage(int outlet, int category, string filter, string branchCode )
        {
            try
            {
                // 🔹 Step 1: Resolve category
                if (category == 1000)
                {
                    category = await GetCategoryCodeViaNameAsync(branchCode);
                }

                // 🔹 Step 2: Get foods
                var foods = await _repository.GetFoodsinImage(outlet, category, filter, branchCode);

                // 🔹 Step 3: Check restaurant status
                var isOpen = await IsOpenRestaurantsAsync();

                // 🔹 Step 4: Apply availability logic
                if (!isOpen.IsOpen && foods != null)
                {
                    foreach (var item in foods)
                    {
                        item.Avaliable = false;
                    }
                }

                // 🔹 Step 5: Return result
                return new FoodImageModelMain
                {
                    foodmodellist = foods,
                    isopen = isOpen
                };
            }
            catch (Exception ex)
            {
                // TODO: log properly (ILogger)
                throw;
            }
        }

        public async Task<IsOpenResturent> IsOpenRestaurantsAsync()
        {
            var result = new IsOpenResturent();

            var now = DateTime.Now;
            var currentDay = now.ToString("dddd");
            var currentTime = now.TimeOfDay;

            var day = await _repository.GetDayAsync(currentDay);

            if (day == null)
            {
                return new IsOpenResturent
                {
                    IsOpen = false,
                    Message = "No schedule found"
                };
            }

            var slots = await _repository.GetTimeSlotsAsync(day.DayId);

            if (slots == null || slots.Count == 0)
            {
                return new IsOpenResturent
                {
                    IsOpen = false,
                    Message = "No time slots configured"
                };
            }

            string message = "";

            for (int i = 0; i < slots.Count; i++)
            {
                var start = TimeSpan.Parse(slots[i].start_time);
                var end = TimeSpan.Parse(slots[i].end_time);

                if (currentTime >= start && currentTime < end)
                {
                    result.IsOpen = true;
                    result.Message = "Open now";
                    return result;
                }

                if (currentTime < start)
                {
                    message = $"Hotel will open at {start}";
                    break;
                }

                if (i == slots.Count - 1)
                {
                    var nextStart = TimeSpan.Parse(slots[0].start_time);
                    message = $"Hotel will open tomorrow at {nextStart}";
                }
            }

            result.IsOpen = false;
            result.Message = string.IsNullOrEmpty(message)
                ? "Temporary restaurant closed"
                : message;

            return result;
        }

        public async Task<int> GetCategoryCodeViaNameAsync(string branchCode)
        {
            return await _repository.GetCategoryCodeByNameAsync("OTHERS", branchCode);
        }

        public async Task<bool> Submitorder(KOTCartModel cartdetails)
        {

            int resId = 0;
            int mkotno = 0;
            var otnames = "0";

            bool fastfoodbill = false;
            bool isSuccess = true;

            if (cartdetails.Mode == "VOID")
            {

            }
            else
            {
                resId = Convert.ToInt32(cartdetails.VRemarks);
            }

            var tableRSql = await _repository.TableReservations(resId);

            //return await Task.Run(async () =>
            //{
            var mode = false;
            if (cartdetails.Mode == "VOID")
            {
                mode = true;
            }
            else if (cartdetails.Mode == "ADD")
            {
                mode = false;
            }
            try
            {
                int dkot = 0;
                if (cartdetails.Type == null)
                {
                    var result = await _repository.GetKOTDetails(cartdetails.Table, cartdetails.SubTable, cartdetails.Outlet, cartdetails.Branch);

                    if (result != null)
                    {
                        if (result.KOTChargeable == true)
                        {
                            cartdetails.Type = "K";
                        }
                        else if (result.KOTChargeable == false)
                        {
                            cartdetails.Type = "N";
                        }
                    }
                    else
                        cartdetails.Type = "K";
                }
                var kotconfig = await _repository.GetKOTConfig(cartdetails.Branch);

                mkotno = await _repository.GetKOTNo(cartdetails.SubBillType, (cartdetails.Type == "N") ? true : false, cartdetails.Branch);

                var kot = new KOTModel();

                DateTime POSEntryDate = await _repository.GetPOSEntryDate(cartdetails.Branch);

                // Fetch Financial settings
                var financialList = await _repository.GetFinancialMasters(cartdetails.Branch);

                if (kotconfig != null && kotconfig.BillType == "D" && kotconfig.SubBillType == "S")
                {
                    dkot = await _repository.GetNextDKOT(cartdetails.Branch);

                    kot = await _repository.SaveKOT(cartdetails.Outlet, cartdetails.Table, cartdetails.Waiter, cartdetails.Pax, POSEntryDate, cartdetails.Total,
                                cartdetails.UserCode, false, mode, cartdetails.SubTable, cartdetails.Branch, cartdetails.Type, cartdetails.NCRemarks, Convert.ToInt32(dkot), cartdetails.CheckInNo, cartdetails.GuestName, cartdetails.GuestCode, cartdetails.KotMobileNo, financialList.FinCode);
                }
                else
                {
                    kot = await _repository.SaveKOT(cartdetails.Outlet, cartdetails.Table, cartdetails.Waiter, cartdetails.Pax, POSEntryDate, cartdetails.Total,
                                    cartdetails.UserCode, false, mode, cartdetails.SubTable, cartdetails.Branch, cartdetails.Type, cartdetails.NCRemarks, mkotno,
                                    cartdetails.CheckInNo, cartdetails.GuestName, cartdetails.GuestCode, cartdetails.KotMobileNo, financialList.FinCode);
                }
                isSuccess = isSuccess && kot != null;

                var updateRequest = new UpdateKOTMasterRequest
                {
                    KOTNo = kot.KOTNo,
                    GuestCode = cartdetails.GuestCode,
                    NCCode = cartdetails.NCCode,
                    Table = cartdetails.Table,
                    SubTable = cartdetails.SubTable,
                    KotMobileNo = cartdetails.KotMobileNo
                };

                bool stats = await _repository.UpdateKOTMasterAsync(updateRequest);

                bool tablestats = await _repository.UpdateTableReservationAsync(kot.KOTNo, resId);

                string outlet = cartdetails.OutletName?.Replace(" ", "").Trim().ToLowerInvariant();

                foreach (var item in cartdetails.Food)
                {
                    var spinfo = await _repository.GetSpecialInfoId(item.Comment, cartdetails.Branch);

                    //var spinfoIds = await _repository.GetSpecialInfoIds(item.Comment, cartdetails.Branch);

                    var spinfoString = Convert.ToString(spinfo);

                    var request = new SaveKOTDetailRequest
                    {
                        KOTId = kot.KOTId,
                        KOTNo = kot.KOTNo,
                        ItemCode = item.Id,
                        KOTDRate = item.Price,
                        KOTDQty = item.Qty,
                        SpecialInstId = spinfoString,
                        BranchCode = cartdetails.Branch,
                        IsFree = "False",
                        ItemDiscount = 0,
                        IsOnline = 0,
                        KNQty = 0,
                        FinCode = financialList.FinCode
                    };

                    var spResult = await _repository.SaveKOTDetailAsync(request);
                    isSuccess = isSuccess;

                    var outletname = await _repository.GetOutletName(cartdetails.Outlet, cartdetails.Branch);

                    //otnames = outletname.ToString();
                    if (outlet != "fast food" || outlet != "fastfood" || outlet != "parcel" || outlet != "home delivery")
                    {
                        var settings = await _repository.GetGlobalSettings(cartdetails.Branch);
                        if (settings != null && settings.HappyHours)
                        {
                            if (CheckHappyHours(settings.HHFrom, settings.HHTo))
                            {
                                var freeItems = await _repository.GetFreeItemsAsync(cartdetails.Outlet, item.Id, cartdetails.Branch);

                                foreach (var x in freeItems)
                                {
                                    var saverequest = new SaveKOTDetailRequest
                                    {
                                        KOTId = kot.KOTId,
                                        KOTNo = kot.KOTNo,
                                        ItemCode = x.FreeItemCode,
                                        KOTDRate = 0,
                                        KOTDQty = x.FreeItemQty,
                                        SpecialInstId = spinfoString,
                                        BranchCode = cartdetails.Branch,
                                        IsFree = "True",
                                        ItemDiscount = 0,
                                        IsOnline = 0,
                                        KNQty = 0
                                    };

                                    await _repository.SaveFreeItemKOTDetailAsync(saverequest);
                                }
                            }
                        }

                        string CatName = "";
                        if (cartdetails.Type == "K")
                            CatName = "K";
                        else
                            CatName = "N";

                        if (cartdetails.Mode == "VOID")
                            CatName = "C";

                        if (cartdetails.Mode == "VOID" && cartdetails.Type == "N")
                            CatName = "NC";

                        string ItemNamess = item.Comment == null ? item.Food : item.Food + "(" + item.Comment + ")";

                        // insert to print table 
                        var type1 = await _repository.GetKotType(cartdetails.Outlet, cartdetails.Branch);
                        if (type1.ToLower() != "bill")
                        {
                            var kotprintrequest = new TmpKotPrintRequest
                            {
                                KotNo = kot.KOTNo,
                                WaiterNo = cartdetails.Waiter,
                                TableNo = cartdetails.Table,
                                KotDate = POSEntryDate,
                                ItemName = ItemNamess,
                                Qty = item.Qty,
                                CategoryName = CatName,
                                ManualKotNo = Convert.ToInt32(dkot),
                                ItemCode = item.Id
                            };

                            var pDetails = await _repository.InsertTmpKotPrintAsync(kotprintrequest);
                        }

                        //var sp_temp = _repository.GetSP<dynamic>("insertintotmpKotPrint", new DynamicParameters(new
                        //{
                        //    KotNO = kot.KOTNo,
                        //    StwNo = cartdetails.Waiter,
                        //    TblNo = cartdetails.Table,
                        //    KOtDate = getPOSEntryDate().ToString("MM/dd/yyyy"),
                        //    ItemName = item.Comment == null ? item.Food : item.Food + "(" + item.Comment + ")",
                        //    Qty = item.Qty,
                        //    CatName = CatName,
                        //    grpcode = "P",
                        //    ManualKotNo = Convert.ToInt32(dkot),
                        //    itemcode = item.Id.ToString()
                        //}));
                    }

                    bool updateKOTDetailsstats = await _repository.UpdateKotDetails(kot.KOTId, cartdetails.Branch);

                    var NCorder = false;
                    if (cartdetails.Type == "K")
                    {
                        NCorder = false;
                    }
                    else if (cartdetails.Type == "N")
                    {
                        NCorder = true;
                    }

                    if (cartdetails.Mode == "VOID")
                    {
                        for (int i = 1; i <= item.Qty; i++)
                        {
                            var data = await _repository.GetActiveKotAsync(cartdetails.Table, cartdetails.Outlet, cartdetails.SubTable, item.Id, cartdetails.Branch, !NCorder);

                            if (data != null)
                            {
                                var kotid = data.KotId;
                                var kotno = data.KotNo;
                                var kid = data.Kid;
                                if (cartdetails.Mode == "VOID")
                                {
                                    await _repository.UpdateKotMasterForVoidAsync(kot.KOTNo, cartdetails.Branch, cartdetails.VRemarks);
                                }
                                await _repository.GenerateCancelKotAsync(data.KotId, item.Id, 1, data.Kid, cartdetails.Branch, NCorder, cartdetails.Table, cartdetails.SubTable, cartdetails.UserCode, cartdetails.VRemarks);
                            }
                        }

                        int preQty = item.OrigQty - item.Qty;

                        await _repository.InsertKotModifyDetailsAsync(kot.KOTNo, item.Id, item.OrigQty, cartdetails.UserCode, POSEntryDate, cartdetails.Outlet, preQty, cartdetails.Branch);
                    }
                }

                var type = await _repository.GetKotType(cartdetails.Outlet, cartdetails.Branch);
                if (type.ToLower() == "bill")
                {
                    var BillConfig = await _repository.GetPOSBillConfig(cartdetails.Branch);
                    var BillModel = new KOTBillModel();
                    BillModel.Cart = cartdetails;
                    BillModel.Tax = await GetBill(cartdetails);
                    BillModel.BillingType = BillConfig.BillType;
                    BillModel.SubBillingType = BillConfig.SubBillType;
                    fastfoodbill = await PostBill(BillModel);
                }

                //if (otnames == "Home Delivery")
                //{
                //    var guest = new HomeDelivery
                //    {
                //        GuestName = cartdetails.HomeDelivary.GuestName,
                //        Address = cartdetails.HomeDelivary.Address,
                //        City = cartdetails.HomeDelivary.City,
                //        Phone = cartdetails.HomeDelivary.Phone,
                //        Remarks = cartdetails.HomeDelivary.Remarks,
                //        Branch_code = cartdetails.Branch,
                //        Email = kot.KOTId.ToString() // storing KOTId
                //    };

                //    if (cartdetails.HomeDelivary.isUpdate == 0)
                //    {
                //        // New guest
                //        guest.GuestCode = await _repository.GetNextGuestCodeAsync();
                //        await _repository.InsertGuestAsync(guest);
                //    }
                //    else
                //    {
                //        // Existing guest
                //        await _repository.UpdateGuestAsync(guest);
                //    }
                //}
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<List<StewardModel>> GetStewards(string branchCode)
        {
            var stewardlist = await _repository.GetStewards(branchCode);
            return stewardlist;
        }

        public async Task<List<OutletSelectModel>> GetOutletsforUser(string username)
        {
            var outletlist = await _repository.GetOutletsforUser(username);
            return outletlist;
        }

        public async Task<CartScannerModel> Getoldcartforscanner(string tableNo, string outlet, string subtable, string branchCode)
        {
            var food = await _repository.GetFood(tableNo, outlet, subtable, branchCode);
            var waiter = await _repository.GetWaiter(tableNo, outlet, subtable, branchCode);

            if ((food == null || food.Count == 0) && waiter == null)
            {
                return null;
            }

            return new CartScannerModel()
            {
                Food = food,
                Pax = waiter?.Pax ?? 0,
                Waiter = waiter.StwCode,
                WaiterName = waiter?.StwName,
                NCCode = waiter.NCCode,
                NCRemarks = waiter?.NCRemarks,
                KotMobileNo = waiter?.KotMobileNo,
                GuestName = waiter?.KOTGuestName
            };
        }

        public async Task<List<BranchViewModal>> GetBranch()
        {
            var branchlist = await _repository.GetBranch();
            return branchlist;
        }

        public async Task<List<FoodImageModel>> GetMenuListByolt(int outlet, string branchCode)
        {
            var menuList = await _repository.GetMenuListByolt(outlet, branchCode);

            return menuList;
        }

        public async Task<OutletSelectModelType> GetOutletTypebyId(string oltcode, string branchcode)
        {
            var data = await _repository.GetOutletTypebyId(oltcode, branchcode);

            return data;
        }

        public async Task<PGCreatePaymentResponse> CreatePaymentAsync(int amount, string redirectURL)
        {
            var token = "";

            string url = "https://api-preprod.phonepe.com/apis/pg-sandbox/checkout/v2/pay";

            string UATMID = _phonePe.PhonePeMID;

            string Lasttoken = await _repository.PGGettokenexpiryTime();

            if (Lasttoken == "")
            {
                var accesstoken = await PGGetTokenAsync();
                token = accesstoken.access_token.ToString();
            }
            else
            {
                token = Lasttoken;
            }

            var orderId = "ORID" + DateTime.Now.ToString("ddMMyyyyHHmmss");

            var payload = new PGCreatePayment
            {
                amount = amount,
                expireAfter = 1200,
                merchantOrderId = orderId,
                metaInfo = new MetaInfo { udf1 = "Test" },
                paymentFlow = new PaymentFlow
                {
                    type = "PG_CHECKOUT",
                    message = "Payment message used for collect requests",
                    merchantUrls = new MerchantUrls
                    {
                        redirectUrl = redirectURL
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"O-Bearer {token}");
            client.DefaultRequestHeaders.Add("X-MERCHANT-ID", _phonePe.PhonePeMID);
            client.DefaultRequestHeaders.Add("X-SOURCE", "API");
            client.DefaultRequestHeaders.Add("X-SOURCE-CHANNEL", "web");
            client.DefaultRequestHeaders.Add("X-MERCHANT-DOMAIN", "https://www.cogwave.in");
            client.DefaultRequestHeaders.Add("X-MERCHANT-IP", "11.123.123.212");
            client.DefaultRequestHeaders.Add("X-SOURCE-REDIRECTION-TYPE", "MERCHANT_REDIRECTION");

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(url, content);
            var resultString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<PGCreatePaymentResponse>(resultString);

            result.merchantOrderId = orderId;

            await _repository.InsertPaymentOrderAsync(result);

            return result;
        }

        public async Task<PGTokenResponseModel> PGGetTokenAsync()
        {
            //var phonePeConfig = _phonePe.PhonePeSettings("PhonePe");

            string clientId = _phonePe.ClientId;
            string clientSecret = _phonePe.ClientSecret;
            string clientVersion = _phonePe.ClientVersion;
            string tokenUrl = _phonePe.TokenUrl;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var client = _httpClientFactory.CreateClient();

            var dict = new Dictionary<string, string>
        {
            { "client_id", clientId },
            { "client_version", clientVersion },
            { "client_secret", clientSecret },
            { "grant_type", "client_credentials" }
        };

            var content = new FormUrlEncodedContent(dict);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsync(tokenUrl, content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadAsStringAsync();

            var data = JsonConvert.DeserializeObject<PGTokenResponseModel>(result);

            // Convert timestamps
            DateTime issuedAt = UnixTimeStampToDateTime(data.issued_at);
            DateTime expiresAt = UnixTimeStampToDateTime(data.expires_at);

            // Save to DB via repository
            await _repository.InsertTokenAsync(data, issuedAt, expiresAt);

            return data;
        }

        //private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        //{
        //    return DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime;
        //}

        public static DateTime UnixTimeStampToDateTime(long seconds, bool toLocal = true)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(seconds);
            return toLocal ? dto.LocalDateTime : dto.UtcDateTime;
        }

        public static DateTime UnixMillisecondsToDateTime(long milliseconds, bool toLocal = true)
        {
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
            return toLocal ? dto.LocalDateTime : dto.UtcDateTime;
        }

        public async Task<PGPaymentOrderStatus> GetPaymentStatusAsync(string merchantOrderId)
        {
            string token = "";

            string merchantId = _phonePe.PhonePeMID;
            string url = $"https://api-preprod.phonepe.com/apis/pg-sandbox/checkout/v2/order/{merchantOrderId}/status";


            string Lasttoken = await _repository.PGGettokenexpiryTime();

            if (Lasttoken == "")
            {
                var accesstoken = await PGGetTokenAsync();
                token = accesstoken.access_token.ToString();
            }
            else
            {
                token = Lasttoken;
            }

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Add("Authorization", $"O-Bearer {token}");
            client.DefaultRequestHeaders.Add("X-MERCHANT-ID", merchantId);
            client.DefaultRequestHeaders.Add("X-SOURCE", "API");
            client.DefaultRequestHeaders.Add("X-SOURCE-CHANNEL", "web");
            client.DefaultRequestHeaders.Add("X-MERCHANT-DOMAIN", "https://www.cogwave.in");
            client.DefaultRequestHeaders.Add("X-MERCHANT-IP", "11.123.123.212");
            client.DefaultRequestHeaders.Add("X-SOURCE-REDIRECTION-TYPE", "MERCHANT_REDIRECTION");

            var response = await client.GetAsync(url);
            var resultString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<PGPaymentOrderStatus>(resultString);

            await _repository.InsertPaymentStatusAsync(result, merchantOrderId, resultString);

            return result;
        }

        private async Task<string> GetTokenAsync()
        {
            // Replace with actual token logic
            await Task.Delay(10);
            return "your_token_here";
        }

        public async Task<bool> PostBill(KOTBillModel bill)
        {
            try
            {
                string billno = string.Empty;

                DateTime POSEntryDate = await _repository.GetPOSEntryDate(bill.Cart.Branch);

                var financialList = await _repository.GetFinancialMasters(bill.Cart.Branch);

                DateTime istTime = ConvertUtcToIst();

                BillModelForBIll billdetails = new BillModelForBIll();

                billdetails.Cart = bill.Cart;
                billdetails.Tax = bill.Tax;
                billdetails.SubBillingType = bill.SubBillingType;
                billdetails.BillingType = bill.BillingType;
                billdetails.paymentresponse = bill.paymentresponse;

                KOTBillHeaderdetilsforBill billheaddetail = new KOTBillHeaderdetilsforBill();

                var Billrequired = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);

                if (bill.Cart.NCCode != 0)
                {
                    string username = await _kotdal.GetUserName(bill.Cart.UserCode, bill.Cart.Branch);

                    var nckotbsmodel = await NCKotBillSettlement(bill, POSEntryDate, financialList.FinCode);
                    billno = nckotbsmodel.BillNo;
                    if (nckotbsmodel.Status)
                    {
                        var taxes = (await GetIndiTax(bill.Cart)).Select(x => new NCSalesTax { Bill_No = billno, ItemCode = x.ItemCode, TaxCode = x.TaxCode, TaxAmount = Math.Round(x.TaxAmount, 2), Branch_Code = bill.Cart.Branch, OltCode = bill.Cart.Outlet, BillDate = POSEntryDate, Ref = "F", FinCode = financialList.FinCode }).ToList();

                        await _repository.InsertNCSalesTaxBulk(taxes);
                    }

                    if (!Billrequired)
                    {
                        bool savetempdata = await _kotdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, "NC", billno, bill.Cart.Table, username, nckotbsmodel.BillId, bill.paymentresponse.data.merchantId, bill.Cart.Branch);
                    }
                }
                else
                {
                    var kotbsmodel = await KotBillSettlement(bill, POSEntryDate, financialList.FinCode);
                    if (kotbsmodel.Status)
                    {
                        billno = kotbsmodel.BillNo;
                        await _repository.SaveDiscount(billno, bill, POSEntryDate);

                        double tottax = 0;

                        var taxes = (await GetIndiTax(bill.Cart)).Select(x =>
                        {
                            var taxAmount = Math.Round(x.TaxAmount, 2);
                            tottax += taxAmount;

                            return new SalesTax
                            {
                                Bill_No = billno,
                                ItemCode = x.ItemCode,
                                TaxCode = x.TaxCode,
                                TaxAmount = taxAmount,
                                Branch_Code = bill.Cart.Branch,
                                OltCode = bill.Cart.Outlet,
                                BillDate = POSEntryDate,
                                Ref = "F",
                                IsOnline = 0,
                                FinCode = financialList.FinCode,
                                TaxType = "OnbillTax"
                            };
                        }).ToList();

                        await _repository.InsertSalesTaxBulk(taxes);

                        //var itemGroups = await _repository.GetItemGroupList(bill.Cart.Branch);

                        //int itemgroupcode = await _repository.GetItemMasterGroupList(bill.Cart.ItemCode, bill.Cart.CatCode, bill.Cart.Branch);

                        await _repository.InsertSalesGroupTaxBulk(Convert.ToInt32(billno), bill.Cart.Branch, bill.Cart.Outlet, bill.Tax, POSEntryDate);

                        string username = await _kotdal.GetUserName(bill.Cart.UserCode, bill.Cart.Branch);

                        if (!Billrequired)
                        {
                            bool savetempdata = await _kotdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, "N", billno, bill.Cart.Table, username, kotbsmodel.BillId, bill.paymentresponse.data.merchantId, bill.Cart.Branch);
                            //bool savetempdata = await _kotdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, billno, bill.Cart.Table, username, kotbsmodel.BillId);
                        }

                        string checkFastFoodService = await _repository.GetInfo("OutletMaster", "OltIsFastFood", "OltCode", Convert.ToString(bill.Cart.Outlet), 0, bill.Cart.Branch);

                        string FastFoodTokenNo = "0";

                        if (checkFastFoodService == "True")
                        {
                            var kotFastfoodTToken = await _kotdal.GetKOTSettlementMaster(kotbsmodel.BillId, kotbsmodel.BillNo, bill.Cart.Outlet, bill.Cart.Branch, "F");

                            if (kotFastfoodTToken != null)
                            {
                                //FastFoodTokenNo = kotFastfoodTToken.ToString();
                                FastFoodTokenNo = kotFastfoodTToken;
                            }
                        }
                        else
                        {
                            FastFoodTokenNo = "0";
                        }

                        string outname = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);

                        billheaddetail.Billno = billno.ToString();
                        billheaddetail.BillDate = POSEntryDate.ToString("dd/MM/yyyy");
                        billheaddetail.BillTime = DateTime.Now.ToString("HH:mm");
                        billheaddetail.OutletName = outname;
                        billheaddetail.OrderId = bill.paymentresponse.data.merchantId;
                        billheaddetail.TokenNo = FastFoodTokenNo;

                        billdetails.billdetails = billheaddetail;

                        string jsondata = JsonConvert.SerializeObject(billdetails);

                        bool saved = await _repository.SavePhonePeOrderBillDetails(bill.paymentresponse.data.merchantId, billno, POSEntryDate, bill.Cart.Outlet, outname, jsondata, bill.Cart.Branch);

                        if (saved)
                        {
                            saved = await _repository.UpdatePaymentStatus(bill.paymentresponse.data.merchantId, billno);
                        }


                        var dsc = await _kotdal.IsDirectSettlement(bill.Cart.Outlet, bill.Cart.Branch);
                        if (dsc)
                        {
                            var settlement = new SettlementModel();
                            var settlementBill = new SettlementBillModel();
                            settlementBill.BillId = kotbsmodel.BillId;
                            settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                            settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                            settlementBill.Discount = 0;
                            settlementBill.ChangeAmount = 0;
                            settlementBill.Tips = 0;


                            settlement.UserCode = bill.Cart.UserCode;
                            settlement.Branch = bill.Cart.Branch;
                            settlement.OutletCode = bill.Cart.Outlet;
                            settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                            settlement.PayMode = "cash";
                            settlement.SubBillingType = bill.SubBillingType;

                            settlement.Bill = settlementBill;

                            var result = await SettleBill(settlementBill);
                        }

                        var dsc1 = await _kotdal.IsFastFoodDirectSettlement(bill.Cart.Outlet, bill.Cart.Branch);
                        if (dsc1)
                        {
                            var settlement = new SettlementModel();
                            var settlementBill = new SettlementBillModel();
                            settlementBill.BillId = kotbsmodel.BillId;
                            settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                            settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                            settlementBill.Discount = 0;
                            settlementBill.ChangeAmount = 0;
                            settlementBill.Tips = 0;

                            settlement.UserCode = bill.Cart.UserCode;
                            settlement.Branch = bill.Cart.Branch;
                            settlement.OutletCode = bill.Cart.Outlet;
                            settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);

                            if (bill.paymentresponse.data.providerReferenceId.ToLower() == "pos")
                            {
                                settlement.PayMode = bill.paymentresponse.code;
                                settlementBill.CardName = bill.paymentresponse.data.transactionId;
                                // settlement.Bill.RefNo = bill.paymentresponse.data.transactionId;
                            }
                            else
                            {
                                settlement.PayMode = "Online";
                            }


                            settlement.SubBillingType = bill.SubBillingType;
                            settlement.Bill = settlementBill;
                            var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                        }
                        else
                        {
                            if (bill.paymentresponse.code.ToUpper().ToString() == "COMPLETED")
                            {
                                var dsc11 = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);
                                if (dsc11)
                                {
                                    var settlement = new SettlementModel();
                                    var settlementBill = new SettlementBillModel();
                                    settlementBill.BillId = kotbsmodel.BillId;
                                    settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                                    settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                                    settlementBill.Discount = 0;
                                    settlementBill.ChangeAmount = 0;
                                    settlementBill.Tips = 0;
                                    settlementBill.CardName = "QRScan";

                                    settlement.UserCode = bill.Cart.UserCode;
                                    settlement.Branch = bill.Cart.Branch;
                                    settlement.OutletCode = bill.Cart.Outlet;
                                    settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                                    settlement.PayMode = "online";
                                    settlement.SubBillingType = bill.SubBillingType;
                                    settlement.Bill = settlementBill;
                                    var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                                }
                            }
                            else if (bill.paymentresponse.code.ToUpper().ToString() == "ONLINE")
                            {
                                var dsc11 = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);
                                if (dsc11)
                                {
                                    var settlement = new SettlementModel();
                                    var settlementBill = new SettlementBillModel();
                                    settlementBill.BillId = kotbsmodel.BillId;
                                    settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                                    settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                                    settlementBill.Discount = 0;
                                    settlementBill.ChangeAmount = 0;
                                    settlementBill.Tips = 0;
                                    settlementBill.CardName = bill.paymentresponse.data.qrString;

                                    settlement.UserCode = bill.Cart.UserCode;
                                    settlement.Branch = bill.Cart.Branch;
                                    settlement.OutletCode = bill.Cart.Outlet;
                                    settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                                    settlement.PayMode = "online";
                                    settlement.SubBillingType = bill.SubBillingType;
                                    settlement.Bill = settlementBill;
                                    var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                                }
                            }
                            else if (bill.paymentresponse.code.ToUpper().ToString() == "CASH")
                            {
                                var dsc11 = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);
                                if (dsc11)
                                {
                                    var settlement = new SettlementModel();
                                    var settlementBill = new SettlementBillModel();
                                    settlementBill.BillId = kotbsmodel.BillId;
                                    settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                                    settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                                    settlementBill.Discount = 0;
                                    settlementBill.ChangeAmount = 0;
                                    settlementBill.Tips = 0;
                                    settlementBill.CardName = bill.paymentresponse.data.qrString;

                                    settlement.UserCode = bill.Cart.UserCode;
                                    settlement.Branch = bill.Cart.Branch;
                                    settlement.OutletCode = bill.Cart.Outlet;
                                    settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                                    settlement.PayMode = "Cash";
                                    settlement.SubBillingType = bill.SubBillingType;
                                    settlement.Bill = settlementBill;
                                    var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                                }
                            }
                            else if (bill.paymentresponse.code.ToUpper().ToString() == "CARD")
                            {
                                var dsc11 = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);
                                if (dsc11)
                                {
                                    var settlement = new SettlementModel();
                                    var settlementBill = new SettlementBillModel();
                                    settlementBill.BillId = kotbsmodel.BillId;
                                    settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                                    settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                                    settlementBill.Discount = 0;
                                    settlementBill.ChangeAmount = 0;
                                    settlementBill.Tips = 0;
                                    settlementBill.CardName = bill.paymentresponse.data.qrString;

                                    settlement.UserCode = bill.Cart.UserCode;
                                    settlement.Branch = bill.Cart.Branch;
                                    settlement.OutletCode = bill.Cart.Outlet;
                                    settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                                    settlement.PayMode = "Card";
                                    settlement.SubBillingType = bill.SubBillingType;
                                    settlement.Bill = settlementBill;
                                    var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                                }
                            }
                            else if (bill.paymentresponse.code.ToUpper().ToString() == "PLUXEE")
                            {
                                var dsc11 = await _kotdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);
                                if (dsc11)
                                {
                                    var settlement = new SettlementModel();
                                    var settlementBill = new SettlementBillModel();
                                    settlementBill.BillId = kotbsmodel.BillId;
                                    settlementBill.BillNo = int.Parse(kotbsmodel.BillNo);
                                    settlementBill.GrandAmount = Math.Round(bill.Cart.Total + tottax);
                                    settlementBill.Discount = 0;
                                    settlementBill.ChangeAmount = 0;
                                    settlementBill.Tips = 0;
                                    settlementBill.CardName = bill.paymentresponse.data.qrString;

                                    settlement.UserCode = bill.Cart.UserCode;
                                    settlement.Branch = bill.Cart.Branch;
                                    settlement.OutletCode = bill.Cart.Outlet;
                                    settlement.OutletName = await _repository.GetOutletName(bill.Cart.Outlet, bill.Cart.Branch);
                                    settlement.PayMode = "Pluxee";
                                    settlement.SubBillingType = bill.SubBillingType;
                                    settlement.Bill = settlementBill;
                                    var result = await _repository.PhonepeSettleBill(settlement, bill.paymentresponse, POSEntryDate, istTime);
                                }
                            }
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }


        public async Task<bool> SubmitOrderFastFoodBill(KOTBillModel bill)
        {
            KOTCartModel cart = new KOTCartModel();

            cart = bill.Cart;

            int resId = 0;
            int mkotno = 0;
            var otnames = "0";
            bool fastfoodbill = false;
            bool isSuccess = true;

            if (cart.Mode == "VOID")
            {

            }
            else
            {
                resId = 0;
            }

            var tableRSql = await _repository.TableReservations(resId);

            var mode = false;
            if (cart.Mode == "VOID")
            {
                mode = true;
            }
            else if (cart.Mode == "ADD")
            {
                mode = false;
            }
            try
            {
                int dkot = 0;
                if (cart.Type == null)
                {
                    var result = await _repository.GetKOTDetails(cart.Table, cart.SubTable, cart.Outlet, cart.Branch);

                    if (result != null)
                    {
                        if (result.KOTChargeable == true)
                        {
                            cart.Type = "K";
                        }
                        else if (result.KOTChargeable == false)
                        {
                            cart.Type = "N";
                        }
                    }
                    else
                        cart.Type = "K";
                }
                var kotconfig = await _repository.GetKOTConfig(cart.Branch);

                mkotno = await _repository.GetKOTNo(cart.SubBillType, (cart.Type == "N") ? true : false, cart.Branch);

                var kot = new KOTModel();

                DateTime POSEntryDate = await _repository.GetPOSEntryDate(cart.Branch);

                // Fetch Financial settings
                var financialList = await _repository.GetFinancialMasters(cart.Branch);

                if (kotconfig != null && kotconfig.BillType == "D" && kotconfig.SubBillType == "S")
                {
                    dkot = await _repository.GetNextDKOT(cart.Branch);

                    kot = await _repository.SaveKOT(cart.Outlet, cart.Table, cart.Waiter, cart.Pax, POSEntryDate, cart.Total, cart.UserCode, false, mode, cart.SubTable, cart.Branch, cart.Type, cart.NCRemarks, Convert.ToInt32(dkot), cart.CheckInNo, cart.GuestName, cart.GuestCode, cart.KotMobileNo, financialList.FinCode);
                }
                else
                {
                    kot = await _repository.SaveKOT(cart.Outlet, cart.Table, cart.Waiter, cart.Pax, POSEntryDate, cart.Total, cart.UserCode, false, mode, cart.SubTable, cart.Branch, cart.Type, cart.NCRemarks, mkotno, cart.CheckInNo, cart.GuestName, cart.GuestCode, cart.KotMobileNo, financialList.FinCode);
                }
                isSuccess = isSuccess && kot != null;


                var updateRequest = new UpdateKOTMasterRequest
                {
                    KOTNo = kot.KOTNo,
                    GuestCode = cart.GuestCode,
                    NCCode = cart.NCCode,
                    Table = cart.Table,
                    SubTable = cart.SubTable,
                    KotMobileNo = cart.KotMobileNo
                };

                bool stats = await _repository.UpdateKOTMasterAsync(updateRequest);

                bool tablestats = await _repository.UpdateTableReservationAsync(kot.KOTNo, resId);

                string outlet = cart.OutletName?.Replace(" ", "").Trim().ToLowerInvariant();

                foreach (var item in cart.Food)
                {
                    var spinfo = await _repository.GetSpecialInfoId(item.Comment, cart.Branch);

                    //var spinfoIds = await _repository.GetSpecialInfoIds(item.Comment, cart.Branch);

                    var spinfoString = Convert.ToString(spinfo);

                    var request = new SaveKOTDetailRequest
                    {
                        KOTId = kot.KOTId,
                        KOTNo = kot.KOTNo,
                        ItemCode = item.Id,
                        KOTDRate = item.Price,
                        KOTDQty = item.Qty,
                        SpecialInstId = spinfoString,
                        BranchCode = cart.Branch,
                        IsFree = "False",
                        ItemDiscount = 0,
                        IsOnline = 0,
                        KNQty = 0,
                        FinCode = financialList.FinCode
                    };

                    var spResult = await _repository.SaveKOTDetailAsync(request);

                    var outletname = await _repository.GetOutletName(cart.Outlet, cart.Branch);

                    if (outlet != "fast food" || outlet != "fastfood" || outlet != "parcel" || outlet != "home delivery")
                    {

                        var settings = await _repository.GetGlobalSettings(cart.Branch);
                        if (settings != null && settings.HappyHours)
                        {
                            if (CheckHappyHours(settings.HHFrom, settings.HHTo))
                            {
                                var freeItems = await _repository.GetFreeItemsAsync(cart.Outlet, item.Id, cart.Branch);

                                foreach (var x in freeItems)
                                {
                                    var saverequest = new SaveKOTDetailRequest
                                    {
                                        KOTId = kot.KOTId,
                                        KOTNo = kot.KOTNo,
                                        ItemCode = x.FreeItemCode,
                                        KOTDRate = 0,
                                        KOTDQty = x.FreeItemQty,
                                        SpecialInstId = spinfoString,
                                        BranchCode = cart.Branch,
                                        IsFree = "True",
                                        ItemDiscount = 0,
                                        IsOnline = 0,
                                        KNQty = 0
                                    };

                                    await _repository.SaveFreeItemKOTDetailAsync(saverequest);
                                }
                            }
                        }

                        string CatName = "";
                        if (cart.Type == "K")
                            CatName = "K";
                        else
                            CatName = "N";

                        if (cart.Mode == "VOID")
                            CatName = "C";

                        if (cart.Mode == "VOID" && cart.Type == "N")
                            CatName = "NC";

                        string ItemNamess = item.Comment == null ? item.Food : item.Food + "(" + item.Comment + ")";

                        // insert to print table 

                        var type1 = await _repository.GetKotType(cart.Outlet, cart.Branch);

                        if (type1.ToLower() != "bill")
                        {
                            var kotprintrequest = new TmpKotPrintRequest
                            {
                                KotNo = kot.KOTNo,
                                WaiterNo = cart.Waiter,
                                TableNo = cart.Table,
                                KotDate = POSEntryDate,
                                ItemName = ItemNamess,
                                Qty = item.Qty,
                                CategoryName = CatName,
                                ManualKotNo = Convert.ToInt32(dkot),
                                ItemCode = item.Id
                            };

                            var pDetails = await _repository.InsertTmpKotPrintAsync(kotprintrequest);
                        }
                    }

                    bool updateKOTDetailsstats = await _repository.UpdateKotDetails(kot.KOTId, cart.Branch);

                    var NCorder = false;
                    if (cart.Type == "K")
                    {
                        NCorder = false;
                    }
                    else if (cart.Type == "N")
                    {
                        NCorder = true;
                    }

                    if (cart.Mode == "VOID")
                    {
                        for (int i = 1; i <= item.Qty; i++)
                        {
                            var data = await _repository.GetActiveKotAsync(cart.Table, cart.Outlet, cart.SubTable, item.Id, cart.Branch, !NCorder);

                            if (data != null)
                            {
                                var kotid = data.KotId;
                                var kotno = data.KotNo;
                                var kid = data.Kid;
                                if (cart.Mode == "VOID")
                                {
                                    await _repository.UpdateKotMasterForVoidAsync(kot.KOTNo, cart.Branch, cart.VRemarks);
                                }
                                await _repository.GenerateCancelKotAsync(data.KotId, item.Id, 1, data.Kid, cart.Branch, NCorder, cart.Table, cart.SubTable, cart.UserCode, cart.VRemarks);
                            }
                        }

                        var orig = item.OrigQty;
                        int preqty = orig - item.Qty;
                        if (preqty > 0)
                        {
                            await _repository.InsertKotModifyDetailsAsync(kot.KOTNo, item.Id, item.OrigQty, cart.UserCode, POSEntryDate, cart.Outlet, preqty, cart.Branch);
                        }
                    }
                }

                var type = await _repository.GetKotType(cart.Outlet, cart.Branch);

                if (type.ToLower() == "bill")
                {
                    var BillConfig = await _repository.GetPOSBillConfig(cart.Branch);
                    var BillModel = new KOTBillModel();
                    BillModel.Cart = cart;
                    BillModel.Tax = await GetBill(cart);
                    BillModel.BillingType = BillConfig.BillType;
                    BillModel.SubBillingType = BillConfig.SubBillType;
                    BillModel.paymentresponse = bill.paymentresponse;
                    fastfoodbill = await PostBill(BillModel);
                }

                var type2 = await _repository.GetOutletType(cart.Outlet, cart.Branch);
                if (type2.OutletType.ToLower() == "restdirect")
                {
                    var BillConfig = await _repository.GetPOSBillConfig(cart.Branch);
                    var BillModel = new KOTBillModel();
                    BillModel.Cart = cart;
                    BillModel.Tax = await GetBill(cart);
                    BillModel.BillingType = BillConfig.BillType;
                    BillModel.SubBillingType = BillConfig.SubBillType;
                    BillModel.paymentresponse = bill.paymentresponse;
                    fastfoodbill = await PostBill(BillModel);
                }

                //if (otnames == "Home Delivery")
                //{
                //    var guest = new HomeDelivery
                //    {
                //        GuestName = cart.HomeDelivary.GuestName,
                //        Address = cart.HomeDelivary.Address,
                //        City = cart.HomeDelivary.City,
                //        Phone = cart.HomeDelivary.Phone,
                //        Remarks = cart.HomeDelivary.Remarks,
                //        Branch_code = cart.Branch,
                //        Email = kot.KOTId.ToString() // storing KOTId
                //    };

                //    if (cart.HomeDelivary.isUpdate == 0)
                //    {
                //        // New guest
                //        guest.GuestCode = await _repository.GetNextGuestCodeAsync();
                //        await _repository.InsertGuestAsync(guest);
                //    }
                //    else
                //    {
                //        // Existing guest
                //        await _repository.UpdateGuestAsync(guest);
                //    }
                //}
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        public async Task<bool> SettleBill(SettlementBillModel settlement)
        {
            DateTime POSEntryDate = await _repository.GetPOSEntryDate(settlement.BranchCode);
            DateTime istTime = ConvertUtcToIst();

            var result = await _repository.SettleBill(settlement, POSEntryDate, istTime);
            return result;
        }

        private bool CheckHappyHours(TimeSpan StartTime, TimeSpan EndTime)
        {
            var someTime = DateTime.Now.TimeOfDay;
            return someTime >= StartTime && someTime <= EndTime;
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


        public async Task<RoomserviceModel> GetRoomserviceDetails(string roomno)
        {
            var data = await _repository.GetRoomserviceDetails(roomno);

            return data;
        }

        public async Task<CompanyInfoScannerDto> GetCompanyinfoBill()
        {
            var Componyinfo = await _repository.GetCompanyinfoBill();

            if (Componyinfo == null)
                throw new Exception("Null Exception");

            return new CompanyInfoScannerDto
            {
                Company_Name = Componyinfo.Company_Name,
                Company_code = Componyinfo.Company_code,
                StartYear = Componyinfo.StartYear,
                Address1 = Componyinfo.Address1,
                Address2 = Componyinfo.Address2,
                Phone_number = Componyinfo.Phone_number,
                Mob_number = Componyinfo.Mob_number,
                OwnerName = Componyinfo.OwnerName,
                Owner_Number = Componyinfo.Owner_Number,
                Fax_number = Componyinfo.Fax_number,
                Email_id = Componyinfo.Email_id,
                Tin_no = Componyinfo.Tin_no,
                Licence_number = Componyinfo.Licence_number,
                Branch_code = Componyinfo.Branch_code,
                STDCODE = Componyinfo.STDCODE,
                Logo = _appSettings.ClientLogo
            };
        }

        public async Task<BillModelForBIll> Getbillnowithorderid(string orderid, int Oltcode, string Branchcode)
        {
            var data = await _repository.Getbillnowithorderid(orderid, Oltcode, Branchcode);

            return data;
        }

        #endregion

        #region Scanner in POS GetBill
        public async Task<KOTTaxModel> GetBill(KOTCartModel cart)
        {
            double totalbillamt = 0.0;
            int totalqty = 0;
            var posdate = await _repository.GetPOSEntryDate(cart.Branch);

            double discountamt = 0.0; // ✅ FIXED

            double cgsttaxper = 0.0;
            double sgsttaxper = 0.0;
            double cgsttaxamt = 0.0;
            double sgsttaxamt = 0.0;
            double serchargeper = 0.0;
            double sercharge = 0.0;

            try
            {
                // ===============================
                // ✅ TOTAL BILL + QTY
                // ===============================
                cart.Food.ForEach(x =>
                {
                    totalbillamt += (x.Qty * x.Price);
                    totalqty += x.Qty;
                });

                List<KOTTaxdetails> taxList = new List<KOTTaxdetails>();

                foreach (var x in cart.Food)
                {
                    var taxmodel = await _repository.GetExtraCharges(x.Id.ToString(), cart.Branch, cart.Outlet);

                    foreach (var item in taxmodel)
                    {
                        double txamt = 0;
                        double taxableamt = 0;

                        double itemTotal = x.Qty * x.Price;

                        if (item.TaxDescription.Contains("CGST"))
                        {
                            var perc = item.TaxPercentage;

                            taxableamt = Math.Round(itemTotal, 2);
                            txamt = Math.Round((itemTotal * perc) / 100, 2);

                            cgsttaxamt += txamt;
                            cgsttaxper = perc;
                        }
                        else if (item.TaxDescription.Contains("SGST"))
                        {
                            var perc = item.TaxPercentage;

                            taxableamt = Math.Round(itemTotal, 2);
                            txamt = Math.Round((itemTotal * perc) / 100, 2);

                            sgsttaxamt += txamt;
                            sgsttaxper = perc;
                        }

                        taxList.Add(new KOTTaxdetails
                        {
                            TaxName = item.TaxDescription,
                            Taxper = item.TaxPercentage,
                            TaxAmount = txamt,
                            TaxableAmount = taxableamt
                        });
                    }
                }

                var finalTaxList = taxList
                    .GroupBy(t => new { t.TaxName, t.Taxper })
                    .Select(g => new KOTTaxdetails
                    {
                        TaxName = g.Key.TaxName,
                        Taxper = g.Key.Taxper,
                        TaxAmount = Math.Round(g.Sum(x => x.TaxAmount), 2),
                        TaxableAmount = Math.Round(g.Sum(x => x.TaxableAmount), 2)
                    })
                    .ToList();

                // ===============================
                // ✅ SERVICE CHARGE
                // GetOutletTypebyId
                var tax = await _repository.GetTaxCharges(cart.Outlet, cart.Branch);

                if (tax.ServiceCharge > 0)
                {
                    serchargeper = tax.ServiceCharge;

                    var scamt = await _repository.LoadRule("Service Charge",
                        totalbillamt, cgsttaxamt, sgsttaxamt, posdate, cart.Branch);

                    sercharge = scamt * serchargeper / 100;
                }

                if (cart.Discount > 0)
                {
                    var totamt = await _repository.LoadRule("Discount", totalbillamt, cgsttaxamt, sgsttaxamt, posdate, cart.Branch);

                    if (cart.DiscountType == "P")
                        discountamt = totamt * cart.Discount / 100;
                    else if (cart.DiscountType == "A")
                        discountamt = cart.Discount;
                }
                else if (cart.DiscountType == "A")
                {
                    discountamt = cart.Discount;
                }

                // ===============================
                // ✅ FINAL CALCULATION
                // ===============================
                var calc = await dobillcalcwithServiceCharge(
                    cart.Outlet, cart.Branch,
                    totalbillamt, discountamt,
                    cgsttaxamt, sgsttaxamt, sercharge);

                // ===============================
                // ✅ RETURN
                // ===============================
                return new KOTTaxModel()
                {
                    CGSTPer = cgsttaxper,
                    CGSTAmt = Math.Round(cgsttaxamt, 2),
                    SGSTAmt = Math.Round(sgsttaxamt, 2),
                    SGSTPer = sgsttaxper,
                    ServiceChargePer = serchargeper,
                    ServiceCharge = Math.Round(sercharge, 2),
                    TotalAmount = totalbillamt,
                    TotalQty = totalqty,
                    Discount = Math.Round(discountamt, 2),
                    DiscountRemarks = cart.DiscountRemarks ?? string.Empty,
                    GrandTotal = calc.Total,
                    RoundOff = Math.Round(calc.RoundOff, 2),
                    TaxList = finalTaxList
                };
            }
            catch
            {
                throw;
            }
        }

        private async Task<CalcModel> dobillcalcwithServiceCharge(int Outlet, string branch, double TotalBillAmt, double DiscountAmt, double CGST, double SGST, double ServiceCharge)
        {
            var InExTaxO = false;
            double billamount = 0;
            double cgst = 0;
            double sgst = 0;
            double disc = 0;
            double SC = 0;
            double roundoff = 0;
            string InExTax = await _repository.GetInfo("OutletMaster", "InExTax", "OltCode", Outlet.ToString(), 0, branch);
            if (InExTax.ToString() == "True")
            {
                InExTaxO = true;
            }
            else
            {
                InExTaxO = false;
            }
            if (InExTaxO == false)
            {
                billamount = TotalBillAmt;
                cgst = Math.Round(CGST, 2);
                sgst = Math.Round(SGST, 2);
                disc = DiscountAmt;
                SC = ServiceCharge;
                double sum1 = Math.Round((billamount + cgst + sgst + ServiceCharge - disc), 0);
                double sum2 = Math.Round((billamount + cgst + sgst + ServiceCharge - disc), 2);

                roundoff = sum2 - sum1;
                double chk = Convert.ToDouble(0.5);
                if (roundoff < chk)
                    roundoff = roundoff * (-1);

                return new CalcModel()
                {
                    RoundOff = roundoff,
                    Total = Math.Round((billamount + cgst + sgst + ServiceCharge - disc + roundoff), 2)
                };
            }
            else
            {
                billamount = TotalBillAmt;
                cgst = CGST;
                sgst = SGST;
                disc = DiscountAmt;
                SC = ServiceCharge;

                double sum1 = Math.Round((billamount + cgst + sgst + ServiceCharge - disc), 0);
                double sum2 = Math.Round((billamount + cgst + sgst + ServiceCharge - disc), 2);

                roundoff = sum2 - sum1;
                double chk = Convert.ToDouble(0.5);
                if (roundoff < chk)
                    roundoff = roundoff * (-1);

                return new CalcModel()
                {
                    RoundOff = roundoff,
                    Total = Math.Round((billamount + cgst + sgst + ServiceCharge - disc + roundoff), 2)
                };
            }
        }

        #endregion

        #region Scanner in POS PostBill

        public async Task<KOTSettlementStatusModel> KotBillSettlement(KOTBillModel bill, DateTime POSEntryDate, string fincode)
        {
            var taxmodel = bill.Tax;
            var cart = bill.Cart;
            var billingtype = bill.BillingType;
            var subbillingtype = bill.SubBillingType;
            var reason = cart.DiscountRemarks;

            string kotnos = "";
            bool saved = false;
            string str = "";
            string StwCode = cart.Waiter.ToString();
            string oltcode = cart.Outlet.ToString();
            var UnsettledKOT = await _kotbillsettlementdal.GetUnsettledKOTs(bill.Cart.Table, bill.Cart.SubTable);
            var ECart = new KOTCartModel();
            var EFoodList = new List<KOTCartModel.KOTFoodModel>();
            try
            {
                cart.Food.ForEach(x =>
                {
                    var EFood = new KOTCartModel.KOTFoodModel();
                    EFood.Category = x.Category;
                    EFood.Comment = x.Comment;
                    EFood.Food = x.Food;
                    EFood.Id = x.Id;
                    EFood.Price = x.Price;
                    EFood.Qty = x.Qty;

                    EFoodList.Add(EFood);
                });
                ECart.Branch = cart.Branch;
                ECart.Discount = cart.Discount;
                ECart.DiscountType = cart.DiscountType;
                ECart.Food = EFoodList;
                ECart.NCCode = cart.NCCode;
                ECart.NCRemarks = cart.NCRemarks;
                ECart.Outlet = cart.Outlet;
                ECart.Pax = cart.Pax;
                ECart.SubTable = cart.SubTable;
                ECart.Table = cart.Table;
                ECart.Total = cart.Total;
                ECart.TotQty = cart.TotQty;
                ECart.Type = cart.Type;
                ECart.UserCode = cart.UserCode;
                ECart.Waiter = cart.Waiter;
                ECart.WaiterName = cart.WaiterName;


                var ksm = await _repository.SettleKOTPartI(POSEntryDate, ECart, taxmodel.CGSTAmt + taxmodel.SGSTAmt, taxmodel.Discount, reason, taxmodel.RoundOff,
                    0, billingtype, subbillingtype, cart.GuestName, cart.KotMobileNo, fincode);

                await _kotbillsettlementdal.UpdateKOTSettlementMaster(ksm.ksmid, cart.Branch, taxmodel.ServiceCharge);

                string checkroomservice = await _repository.GetInfo("OutletMaster", "OltIsRoomService", "OltCode", oltcode, 0, cart.Branch);
                string checkparcelservice = await _repository.GetInfo("OutletMaster", "OltIsParcelService", "OltCode", oltcode, 0, cart.Branch);

                if (checkroomservice == "True")
                {
                    var ds = await _kotbillsettlementdal.GetFoodBill(POSEntryDate, ksm.billno, cart.CheckInNo, cart.Outlet);

                    if (ds.Count == 0)
                    {
                        string outname = await _kotbillsettlementdal.GetOutletName(ECart.Outlet, ECart.Branch);
                        decimal Taxv = Convert.ToDecimal(taxmodel.CGSTPer + taxmodel.SGSTPer);
                        decimal ExculTax = Convert.ToDecimal(taxmodel.TotalAmount);

                        var foodBill = new FoodBill
                        {
                            RNo = await _kotdal.findhotelnextnumber("Tbl_FoodBills", "RNo"),
                            TrDate = POSEntryDate,
                            RcptNo = ksm.billno,
                            GuestCode = cart.GuestCode,
                            GuestName = cart.GuestName,
                            CheckInNo = cart.CheckInNo,
                            RoomNo = cart.Table,
                            BillAmt = taxmodel.GrandTotal,
                            Outlet = outname,
                            CGST = taxmodel.CGSTAmt,
                            SGST = taxmodel.SGSTAmt,
                            TaxVal = Taxv,
                            ExTax1 = ExculTax,
                            ExTax2 = ExculTax
                        };

                        int rowsInserted = await _kotbillsettlementdal.InsertFoodBill(foodBill);
                    }

                    await _kotbillsettlementdal.UpdateKOTSettlementRoomService(ksm.billno, cart.Outlet, cart.Branch);
                }
                if (checkparcelservice == "True")
                {
                    await _kotbillsettlementdal.UpdateKOTSettlementParcelService(ksm.billno, oltcode, cart.Branch);
                }
                string checkFastFoodService = await _repository.GetInfo("OutletMaster", "OltIsFastFood", "OltCode", oltcode, 0, cart.Branch);

                string FastFoodTokenNo = "0";

                if (checkFastFoodService == "True")
                {
                    var kotFastfoodTToken = await _kotbillsettlementdal.GetFastFoodToken(ksm.ksmid, ksm.billno, oltcode, cart.Branch);

                    if (kotFastfoodTToken != null)
                    {
                        //FastFoodTokenNo = kotFastfoodTToken.ToString();
                        FastFoodTokenNo = kotFastfoodTToken;
                    }
                }
                else
                {
                    FastFoodTokenNo = "0";
                }

                var kot = await _kotbillsettlementdal.GetKOTDetails(cart.Table, cart.SubTable, cart.Outlet, cart.Branch);

                if (kot.Count() > 0)
                {
                    foreach (var x in kot)
                    {
                        kotnos = string.IsNullOrEmpty(kotnos) ? x.KOTNo.ToString() : kotnos + "," + x.KOTNo.ToString();

                        var existingKOTSettlement = await _kotbillsettlementdal.GetKOTSettlementDetails(x.KOTId, ksm.ksmid, cart.Branch);

                        if (existingKOTSettlement.Count == 0)
                        {
                            var inserted = await _kotbillsettlementdal.InsertKOTSettlementDetails(x.KOTId, ksm.ksmid, cart.Branch, fincode);

                            if (inserted > 0)
                            {
                                await _kotbillsettlementdal.UpdateKOTMasterAfterSettlement(x.KOTNo, cart.Outlet, cart.Branch, FastFoodTokenNo);
                                saved = true;
                            }
                        }

                        // 6️⃣ Update KOT display table
                        var displayid = await _kotbillsettlementdal.GetKOTDisplay(x.KOTId);
                        if (displayid.Count > 0)
                        {
                            var displayExists = await _kotbillsettlementdal.UpdateKOTDisplay(x.KOTNo);
                        }
                    }
                }
                else
                {
                    saved = false;
                }
                if (saved)
                {
                    bool settlement;

                    //if (Globalvar.EnumRestaurent == "CC" || Globalvar.EnumRestaurent == "RIDERS" || Globalvar.EnumRestaurent == "MALLIGI")
                    if (true)
                    {
                        settlement = true;
                    }
                    else
                    {
                        settlement = false;
                    }
                    if (settlement)
                    {
                        //if (checkroomservice == "True" && CheckRevenue(checkinnumber, "FOOD", "-1") == false)
                        //{
                        //    settlingbillwhilebillprint(billno, lblamt.Text, oltcode);
                        //}

                        //NOT YET COMPLETE THIS PART
                    }
                }
                else if (!saved)
                {
                    await _kotbillsettlementdal.DeleteKOTSettlementMaster(ksm.ksmid, cart.Branch);

                    await _kotbillsettlementdal.DeleteKOTSettlementDetails(ksm.ksmid, cart.Branch);

                    if (checkroomservice == "True")
                    {
                        await _kotbillsettlementdal.DeleteFoodBill(ksm.billno, POSEntryDate);
                    }
                }
                return new KOTSettlementStatusModel()
                {
                    Status = saved,
                    BillNo = ksm.billno,
                    BillId = ksm.ksmid
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<KOTSettlementStatusModel> NCKotBillSettlement(KOTBillModel bill, DateTime POSEntryDate, string fincode)
        {
            var kotnos = "";
            var taxmodel = bill.Tax;
            var cart = bill.Cart;
            var billingtype = bill.BillingType;
            var subbillingtype = bill.SubBillingType;
            var reason = cart.NCRemarks;
            var ECart = new KOTCartModel();
            var EFoodList = new List<KOTCartModel.KOTFoodModel>();
            try
            {
                cart.Food.ForEach(x =>
                {
                    var EFood = new KOTCartModel.KOTFoodModel();
                    EFood.Category = x.Category;
                    EFood.Comment = x.Comment;
                    EFood.Food = x.Food;
                    EFood.Id = x.Id;
                    EFood.Price = x.Price;
                    EFood.Qty = x.Qty;

                    EFoodList.Add(EFood);
                });
                ECart.Branch = cart.Branch;
                ECart.Discount = cart.Discount;
                ECart.DiscountType = cart.DiscountType;
                ECart.Food = EFoodList;
                ECart.NCCode = cart.NCCode;
                ECart.NCRemarks = cart.NCRemarks;
                ECart.Outlet = cart.Outlet;
                ECart.Pax = cart.Pax;
                ECart.SubTable = cart.SubTable;
                ECart.Table = cart.Table;
                ECart.Total = cart.Total;
                ECart.TotQty = cart.TotQty;
                ECart.Type = cart.Type;
                ECart.UserCode = cart.UserCode;
                ECart.Waiter = cart.Waiter;
                ECart.WaiterName = cart.WaiterName;

                var ksm = await _repository.SettleNCKOTPartI(POSEntryDate, ECart, taxmodel.CGSTAmt + taxmodel.SGSTAmt, taxmodel.Discount, 0, reason, taxmodel.RoundOff,
                    0, 0, billingtype, subbillingtype, fincode);

                var saved = await _nckotbillsettlementdal.UpdateNCKOTSettlementMaster(ksm.ksmid, cart.Branch) > 0;

                string checkroomservice = await _repository.GetInfo("OutletMaster", "OltIsRoomService", "OltCode", cart.Outlet.ToString(), 0, cart.Branch);
                string checkparcelservice = await _repository.GetInfo("OutletMaster", "OltIsParcelService", "OltCode", cart.Outlet.ToString(), 0, cart.Branch);

                if (checkroomservice == "True")
                {
                    //NOT YET COMPLETED THIS PART
                }
                if (checkparcelservice == "True")
                {
                    //NOT YET COMPLETED THIS PART
                }

                IEnumerable<dynamic> dataset = await _nckotbillsettlementdal.GetNCKOTDetails(cart.Table, cart.SubTable, cart.Outlet, cart.Branch);

                if (dataset.Any())
                {
                    foreach (var x in dataset)
                    {
                        kotnos = string.IsNullOrEmpty(kotnos) ? x.KOTNo.ToString() : kotnos + "," + x.KOTNo;

                        await _nckotbillsettlementdal.UpdateKOTMasterNC(x.KOTNo, cart.Outlet, cart.Branch);

                        IEnumerable<dynamic> ds = await _nckotbillsettlementdal.GetNCKOTSettlementDetails(x.KOTId, ksm.ksmid, cart.Branch);

                        if (!ds.Any())
                        {
                            await _nckotbillsettlementdal.InsertNCKOTSettlementDetails(x.KOTId, ksm.ksmid, cart.Branch, fincode);
                            saved = true;
                        }
                    }
                }
                else
                {
                    saved = false;
                }
                if (saved)
                {
                    bool settlement;
                    if (true)
                        settlement = true;
                    else
                        settlement = false;
                }
                else
                {
                    await _nckotbillsettlementdal.DeleteNCKOTSettlementMaster(ksm.ksmid, cart.Branch);

                    await _nckotbillsettlementdal.DeleteNCKOTSettlementDetails(ksm.ksmid, cart.Branch);

                    if (checkroomservice == "True")
                    {
                        await _kotbillsettlementdal.DeleteFoodBill(ksm.billno, POSEntryDate);
                    }
                }

                return new KOTSettlementStatusModel()
                {
                    Status = saved,
                    BillNo = ksm.billno,
                    BillId = ksm.ksmid

                };
            }
            catch
            {
                throw;
            }
        }

        //public async Task<List<IndiTaxModel>> GetIndiTax(KOTCartModel cart)
        //{
        //    var taxModelList = new List<IndiTaxModel>();
        //    cart.Food.ForEach(x =>
        //    {
        //        try
        //        {
        //            var taxmodel = await _repository.GetExtraCharges(x.Id.ToString(), cart.Branch, cart.Outlet);
        //            if (taxmodel == null)
        //            {
        //                var taxModel = new IndiTaxModel();
        //                taxModel.TaxAmount = 0;
        //                taxModel.TaxCode = "0";
        //            }
        //            else
        //            {
        //                foreach (var item in taxmodel)
        //                {
        //                    var taxModel = new IndiTaxModel();
        //                    taxModel.TaxAmount = ((x.Qty * x.Price) * item.TaxPercentage) / 100;
        //                    taxModel.TaxCode = item.TaxCode;
        //                    taxModel.ItemCode = x.Id;
        //                    taxModelList.Add(taxModel);
        //                }
        //            }
        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    });
        //    return taxModelList;
        //}

        public async Task<List<IndiTaxModel>> GetIndiTax(KOTCartModel cart)
        {
            var taxModelList = new List<IndiTaxModel>();

            foreach (var x in cart.Food)
            {
                var taxmodel = await _repository.GetExtraCharges(
                    x.Id.ToString(),
                    cart.Branch,
                    cart.Outlet);

                if (taxmodel == null)
                {
                    taxModelList.Add(new IndiTaxModel
                    {
                        TaxAmount = 0,
                        TaxCode = "0",
                        ItemCode = x.Id
                    });
                }
                else
                {
                    foreach (var item in taxmodel)
                    {
                        taxModelList.Add(new IndiTaxModel
                        {
                            TaxAmount = ((x.Qty * x.Price) * item.TaxPercentage) / 100,
                            TaxCode = item.TaxCode,
                            ItemCode = x.Id
                        });
                    }
                }
            }

            return taxModelList;
        }


        #endregion

        #region WhatsappConfig

        public async Task<Whatsupconfig> WhatsappConfiguration()
        {
            var data = await _repository.WhatsappConfiguration();
            return data;
        }

        #endregion

        #region OnlinePaymentType

        public async Task<OnlinePaymentTypeModel> OnlinePaymentType()
        {
            var data = await _repository.OnlinePaymentType();
            return data;
        }

        #endregion

        #region POS Device

        public async Task<bool> submitOrderdirectbillnew(KOTBillModel requestdetails)
        {
            KOTCartModel cartdetails = new KOTCartModel();

            cartdetails = requestdetails.Cart;

            int resId = 0;
            int mkotno = 0;
            var otnames = "0";

            bool fastfoodbill = false;
            bool isSuccess = true;

            if (cartdetails.Mode == "VOID")
            {

            }
            else
            {
                resId = Convert.ToInt32(cartdetails.VRemarks);
            }

            var tableRSql = await _repository.TableReservations(resId);

            //return await Task.Run(async () =>
            //{
            var mode = false;
            if (cartdetails.Mode == "VOID")
            {
                mode = true;
            }
            else if (cartdetails.Mode == "ADD")
            {
                mode = false;
            }
            try
            {
                int dkot = 0;
                if (cartdetails.Type == null)
                {
                    var result = await _repository.GetKOTDetails(cartdetails.Table, cartdetails.SubTable, cartdetails.Outlet, cartdetails.Branch);

                    if (result != null)
                    {
                        if (result.KOTChargeable == true)
                        {
                            cartdetails.Type = "K";
                        }
                        else if (result.KOTChargeable == false)
                        {
                            cartdetails.Type = "N";
                        }
                    }
                    else
                        cartdetails.Type = "K";
                }
                var kotconfig = await _repository.GetKOTConfig(cartdetails.Branch);

                mkotno = await _repository.GetKOTNo(cartdetails.SubBillType, (cartdetails.Type == "N") ? true : false, cartdetails.Branch);

                var kot = new KOTModel();

                DateTime POSEntryDate = await _repository.GetPOSEntryDate(cartdetails.Branch);

                // Fetch Financial settings
                var financialList = await _repository.GetFinancialMasters(cartdetails.Branch);

                if (kotconfig != null && kotconfig.BillType == "D" && kotconfig.SubBillType == "S")
                {
                    dkot = await _repository.GetNextDKOT(cartdetails.Branch);

                    kot = await _repository.SaveKOT(cartdetails.Outlet, cartdetails.Table, cartdetails.Waiter, cartdetails.Pax, POSEntryDate, cartdetails.Total,
                                cartdetails.UserCode, false, mode, cartdetails.SubTable, cartdetails.Branch, cartdetails.Type, cartdetails.NCRemarks, Convert.ToInt32(dkot), cartdetails.CheckInNo, cartdetails.GuestName, cartdetails.GuestCode, cartdetails.KotMobileNo, financialList.FinCode);
                }
                else
                {
                    kot = await _repository.SaveKOT(cartdetails.Outlet, cartdetails.Table, cartdetails.Waiter, cartdetails.Pax, POSEntryDate, cartdetails.Total,
                                    cartdetails.UserCode, false, mode, cartdetails.SubTable, cartdetails.Branch, cartdetails.Type, cartdetails.NCRemarks, mkotno,
                                    cartdetails.CheckInNo, cartdetails.GuestName, cartdetails.GuestCode, cartdetails.KotMobileNo, financialList.FinCode);
                }
                isSuccess = isSuccess && kot != null;

                var updateRequest = new UpdateKOTMasterRequest
                {
                    KOTNo = kot.KOTNo,
                    GuestCode = cartdetails.GuestCode,
                    NCCode = cartdetails.NCCode,
                    Table = cartdetails.Table,
                    SubTable = cartdetails.SubTable,
                    KotMobileNo = cartdetails.KotMobileNo
                };

                bool stats = await _repository.UpdateKOTMasterAsync(updateRequest);

                bool tablestats = await _repository.UpdateTableReservationAsync(kot.KOTNo, resId);

                string outlet = cartdetails.OutletName?.Replace(" ", "").Trim().ToLowerInvariant();

                foreach (var item in cartdetails.Food)
                {
                    var spinfo = await _repository.GetSpecialInfoId(item.Comment, cartdetails.Branch);

                    //var spinfoIds = _repository.GetSpecialInfoIds(item.Comment, cartdetails.Branch);

                    var spinfoString = Convert.ToString(spinfo);

                    var request = new SaveKOTDetailRequest
                    {
                        KOTId = kot.KOTId,
                        KOTNo = kot.KOTNo,
                        ItemCode = item.Id,
                        KOTDRate = item.Price,
                        KOTDQty = item.Qty,
                        SpecialInstId = spinfoString,
                        BranchCode = cartdetails.Branch,
                        IsFree = "False",
                        ItemDiscount = 0,
                        IsOnline = 0,
                        KNQty = 0,
                        FinCode = financialList.FinCode
                    };

                    var spResult = await _repository.SaveKOTDetailAsync(request);
                    isSuccess = isSuccess;

                    var outletname = await _repository.GetOutletName(cartdetails.Outlet, cartdetails.Branch);

                    //otnames = outletname.ToString();
                    if (outlet != "fast food" || outlet != "fastfood" || outlet != "parcel" || outlet != "home delivery")
                    {
                        var settings = await _repository.GetGlobalSettings(cartdetails.Branch);
                        if (settings != null && settings.HappyHours)
                        {
                            if (CheckHappyHours(settings.HHFrom, settings.HHTo))
                            {
                                var freeItems = await _repository.GetFreeItemsAsync(cartdetails.Outlet, item.Id, cartdetails.Branch);

                                foreach (var x in freeItems)
                                {
                                    var saverequest = new SaveKOTDetailRequest
                                    {
                                        KOTId = kot.KOTId,
                                        KOTNo = kot.KOTNo,
                                        ItemCode = x.FreeItemCode,
                                        KOTDRate = 0,
                                        KOTDQty = x.FreeItemQty,
                                        SpecialInstId = spinfoString,
                                        BranchCode = cartdetails.Branch,
                                        IsFree = "True",
                                        ItemDiscount = 0,
                                        IsOnline = 0,
                                        KNQty = 0
                                    };

                                    await _repository.SaveFreeItemKOTDetailAsync(saverequest);
                                }
                            }
                        }

                        string CatName = "";
                        if (cartdetails.Type == "K")
                            CatName = "K";
                        else
                            CatName = "N";

                        if (cartdetails.Mode == "VOID")
                            CatName = "C";

                        if (cartdetails.Mode == "VOID" && cartdetails.Type == "N")
                            CatName = "NC";

                        string ItemNamess = item.Comment == null ? item.Food : item.Food + "(" + item.Comment + ")";

                        // insert to print table 
                        var type1 = await _repository.GetKotType(cartdetails.Outlet, cartdetails.Branch);
                        if (type1.ToLower() != "bill")
                        {
                            var kotprintrequest = new TmpKotPrintRequest
                            {
                                KotNo = kot.KOTNo,
                                WaiterNo = cartdetails.Waiter,
                                TableNo = cartdetails.Table,
                                KotDate = POSEntryDate,
                                ItemName = ItemNamess,
                                Qty = item.Qty,
                                CategoryName = CatName,
                                ManualKotNo = Convert.ToInt32(dkot),
                                ItemCode = item.Id
                            };

                            var pDetails = await _repository.InsertTmpKotPrintAsync(kotprintrequest);
                        }
                    }

                    bool updateKOTDetailsstats = await _repository.UpdateKotDetails(kot.KOTId, cartdetails.Branch);

                    var NCorder = false;
                    if (cartdetails.Type == "K")
                    {
                        NCorder = false;
                    }
                    else if (cartdetails.Type == "N")
                    {
                        NCorder = true;
                    }

                    if (cartdetails.Mode == "VOID")
                    {
                        for (int i = 1; i <= item.Qty; i++)
                        {
                            var data = await _repository.GetActiveKotAsync(cartdetails.Table, cartdetails.Outlet, cartdetails.SubTable, item.Id, cartdetails.Branch, !NCorder);

                            if (data != null)
                            {
                                var kotid = data.KotId;
                                var kotno = data.KotNo;
                                var kid = data.Kid;
                                if (cartdetails.Mode == "VOID")
                                {
                                    await _repository.UpdateKotMasterForVoidAsync(kot.KOTNo, cartdetails.Branch, cartdetails.VRemarks);
                                }
                                await _repository.GenerateCancelKotAsync(data.KotId, item.Id, 1, data.Kid, cartdetails.Branch, NCorder, cartdetails.Table, cartdetails.SubTable, cartdetails.UserCode, cartdetails.VRemarks);
                            }
                        }

                        int preQty = item.OrigQty - item.Qty;

                        await _repository.InsertKotModifyDetailsAsync(kot.KOTNo, item.Id, item.OrigQty, cartdetails.UserCode, POSEntryDate, cartdetails.Outlet, preQty, cartdetails.Branch);
                    }
                }

                var type = await _repository.GetKotType(cartdetails.Outlet, cartdetails.Branch);
                if (type.ToLower() == "bill")
                {
                    var BillConfig = await _repository.GetPOSBillConfig(cartdetails.Branch);
                    var kotbillmodel = new KOTBillModel();
                    kotbillmodel.Cart = cartdetails;
                    kotbillmodel.Tax = await GetBill(cartdetails);
                    kotbillmodel.BillingType = BillConfig.BillType;
                    kotbillmodel.SubBillingType = BillConfig.SubBillType;
                    kotbillmodel.paymentresponse = requestdetails.paymentresponse;
                    fastfoodbill = await PostBill(kotbillmodel);
                }

                //if (otnames == "Home Delivery")
                //{
                //    var guest = new HomeDelivery
                //    {
                //        GuestName = cartdetails.HomeDelivary.GuestName,
                //        Address = cartdetails.HomeDelivary.Address,
                //        City = cartdetails.HomeDelivary.City,
                //        Phone = cartdetails.HomeDelivary.Phone,
                //        Remarks = cartdetails.HomeDelivary.Remarks,
                //        Branch_code = cartdetails.Branch,
                //        Email = kot.KOTId.ToString() // storing KOTId
                //    };

                //    if (cartdetails.HomeDelivary.isUpdate == 0)
                //    {
                //        // New guest
                //        guest.GuestCode = await _repository.GetNextGuestCodeAsync();
                //        await _repository.InsertGuestAsync(guest);
                //    }
                //    else
                //    {
                //        // Existing guest
                //        await _repository.UpdateGuestAsync(guest);
                //    }
                //}
                return isSuccess;
            }
            catch (Exception ex)
            {
                return isSuccess;
            }
        }

        #endregion MJ

        #region EmailSender

        public async Task<KOTServiceResult<bool>> EmailRequestAsync(EmailRequest request)
        {
            try
            {
                var branchName = await _repository.GetBrancheName(request.BranchCode);

                var closedaymodel = new CloseDayRequest
                {
                    UserId = request.UserId,
                    POSEntryDate = request.FromDate,
                    SystemTime = request.ToDate,
                    BranchCode = request.BranchCode
                };

                bool result = await _emailsender.SendEmailDailyClose(closedaymodel, branchName);

                if (!result)
                {
                    return new KOTServiceResult<bool>
                    {
                        Success = false,
                        Message = "Failed To Send Mail",
                        Data = result
                    };
                }
                else
                {
                    return new KOTServiceResult<bool>
                    {
                        Success = true,
                        Message = "Mail Send Successfully",
                        Data = result
                    };
                }
            }
            catch (Exception ex)
            {
                return new KOTServiceResult<bool>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        #endregion MJ

    }
}
