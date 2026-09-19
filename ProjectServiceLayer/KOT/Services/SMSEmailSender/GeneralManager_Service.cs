using HMS_360_PMS.ProjectInfrastructure.KOT;
using HMS_360_PMS.ProjectServiceLayer.KOT.Interfaces;
using System.Net;

namespace HMS_360_PMS.ProjectServiceLayer.KOT.Services.SMSEmailSender
{
    public class GeneralManager_Service: IGeneralManager_Service
    {
        public readonly IBasicSettingsManager _basicsettingManager;
        public readonly IDashboardManager _idashboardManager;
        //public readonly GeneralManager_DAL _gmdal;

        public GeneralManager_Service(IBasicSettingsManager basicsettingManager, IDashboardManager idashboardManager)
        {
            _basicsettingManager = basicsettingManager;
            _idashboardManager = idashboardManager;
            //_gmdal = gmdal;
        }

        public async Task<string> GenerateingOtp(string MobileNo)
        {
            //string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = MobileNo;
            string characters = numbers;
            //characters += alphabets + small_alphabets + numbers;
            int length = 4;
            string otp = string.Empty;
            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    Random rnd = new Random();

                    int index = rnd.Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }

            var managed = new
            {
                otps = otp
            };
            SMSBuilder(18, MobileNo, managed);
            // _genManager.SendSMS(MobileNo, otp);
            return otp.ToString();
        }

        public async Task SMSBuilder(int MesgId, string ContactId, dynamic Messaged)
        {
            var HMSDE = await _basicsettingManager.GetBaseSettings();
            if (HMSDE.IsSMS == true)
            {
                if (MesgId != 18 && MesgId != 19)
                {
                    var SmsRights = await _basicsettingManager.GetSmsRights();

                    if (SmsRights.Where(s => s.SMSId == MesgId).FirstOrDefault().IsValid == false)
                    {
                        return;
                    }
                }


                var RecHelp = await _basicsettingManager.GetBaseSettings();

                var ComanyName = await _basicsettingManager.GetPurcharserDetalis();
                string ContDetails = "please contact " + ComanyName.HotelName + " PH: " + ComanyName.Mobile + ".";
                string strmsg = "";
                switch (MesgId)
                {
                    case 1:
                        strmsg = "Thank you, Your Reservation No : " + Messaged.OriginalResvNo +
                            " We are expecting you on " + Messaged.TmpChkDt + " And We Received Rs." + Messaged.AdvAmt +
                            ".  Further assistance " + ContDetails;
                        break;
                    case 2:
                        strmsg = " We refunded Rs." + Messaged.ClBal + " to You. Thank you and visit again. For any assistance " + ContDetails;
                        break;
                    //case 3:
                    //    strmsg = "Welcome to " + ComanyName.HotelName + " ,We Received Rs." + Messaged.AdvanceAmt + ", Wish you the pleasant stay, for Reception and RoomService contact " + RecHelp.RecHelp + ".";
                    //    break;
                    case 3:
                        strmsg = "Welcome to " + ComanyName.HotelName + " ,We Received Rs." + Messaged.AdvanceAmt + ", Wish you the pleasant stay, for Reception and RoomService contact " + "9" + ".";
                        break;
                    case 4:
                        var Status = await _idashboardManager.GetDashboardData();
                        strmsg = Messaged.TmproomNo + " is checked in at " + DateTime.Now + "  " + DateTime.Now.TimeOfDay + ". Occupied-" + Status.Occupied + ",Vacant-" + Status.Vacant + ",Dirty -" + Status.Dirty + ",Blocked -" + Status.Blocked + ".Today Arri-" + Status.TodayCheckin + ",Today Checkout-" + Status.TodayCheckout + ".";
                        break;
                    case 5:
                        var Statuss = await _idashboardManager.GetDashboardData();
                        strmsg = Messaged.TmproomNo + " is checked out at " + DateTime.Now + "  " + DateTime.Now.TimeOfDay + ". Occupied-" + Statuss.Occupied + ",Vacant-" + Statuss.Vacant + ",Dirty -" + Statuss.Dirty + ",Blocked -" + Statuss.Blocked + ".Today Arri-" + Statuss.TodayCheckin + ",Today Checkout-" + Statuss.TodayCheckout + ".";
                        break;
                    case 6:
                        strmsg = "Received further advance Rs." + Messaged.ClBal + " Thank you Your Room No." + Messaged.TmproomNo + ".";
                        break;
                    case 7:
                        strmsg = "Hi Mr / Mrs " + Messaged.TmpGuestName + ", Your stay in our " + ComanyName.HotelName + " is progressing towards next day,your Outstanding amount will exceeds, Thank You.";
                        break;
                    case 8:
                        break;
                    case 9:
                        var Statu1 = await _idashboardManager.GetDashboardData();
                        strmsg = "Reservation no " + Messaged.OriginalResvNo + " is checked in " + Messaged.TmproomNo + " at " + DateTime.Now + DateTime.Now.TimeOfDay + " .Occupied: " & Statu1.Occupied & ", Vacant: " & Statu1.Vacant & ", Blocked: " & Statu1.Blocked & ", Dirty: " & Statu1.Dirty & ". Today Arri-" & Statu1.TodayCheckin & ", Today Checkout-" & Statu1.TodayCheckout & ".";
                        break;
                    case 10:
                        strmsg = "We thank you for choosing  " + ComanyName.HotelName + ". We wish you a safe onward journey. We trust that your stay was pleasant. ";
                        break;
                    case 11:
                        strmsg = "Hi Mr / Mrs " + Messaged.TmpGuestName + ".RoomNo " + Messaged.RO1 + " is Shiffted to RoomNo " + Messaged.RO2 + ". Due to " + Messaged.REM1 + " By " + Messaged.UserName + " ";
                        break;
                    case 12:
                        strmsg = "RoomNo " + Messaged.RO1 + ",Blocked for " + Messaged.RO2 + ". Reason For Blocking " + Messaged.RE1 + ". Blocked By " + Messaged.UserName + ".";
                        break;
                    case 13:
                        strmsg = "Discount Posted in RoomNo " + Messaged.RO1 + " And GuestName " + Messaged.TmpGuestName + " AND NetAmount " + Messaged.RO2 + " .Discount Posted " + Messaged.RE1 + " Reason " + Messaged.REM1 + " Posted By " + Messaged.UserName + "  ";
                        break;
                    case 15:
                        strmsg = " RoomNo " + Messaged.RO1 + " is Cancelled . GuestName " + Messaged.TmpGuestName + ". Reason " + Messaged.REM1 + " Cancelled By " + Messaged.UserName + "  ";
                        break;
                    case 16:
                        strmsg = "Checkout Bill " + Messaged.REM1 + " Canncelled.RoomNo " + Messaged.RO1 + " GuestName " + Messaged.TmpGuestName + ". Reason Wrongly Checkout Done. Cancelled By " + Messaged.UserName + "  ";

                        break;
                    case 17:
                        break;
                    case 18:
                        //strmsg = "Hi your one time otp for food ordering opt is " + Messaged.otps + "  please do not share your otp";
                        strmsg = "Hi your one time otp for food ordering opt is " + Messaged.otps + " please do not share your otp";
                        break;
                    case 19:
                        strmsg = "Dear Guest Thank you, Your Payment amount Rs. " + Messaged.Amount + "  Done Sucessfully Your Payment Order ID: " + Messaged.TrackingId + " Bank RefNo: " + Messaged.Bankrefno + " ";
                        break;
                }
                if (HMSDE.IsSMS == true)
                {
                    if (MesgId == 18)
                    {
                        strmsg = strmsg + ". COGWAVE";
                    }
                    else
                    {
                        strmsg = strmsg + ".CW";
                    }
                    await SendSMS(ContactId, strmsg);
                }
            }

            else
            {
                return;
            }
        }

        public async Task<bool> SendSMS(string MobileNo, string Message)
        {
            var sender = await _basicsettingManager.GetBaseSettings();
            if (sender.IsSMS == true)
            {
                string _createURL = "";

                //if (sender.IsBulkLink == false)
                if (true == false)
                {
                    //string _URL = sender.SmsProvider;
                    //string _username = sender.SmsNo;
                    //string _password = sender.Password;
                    //string _to = MobileNo;
                    //string _sender = sender.SmsId;
                    //_createURL = _URL + "?" +
                    //   "username=" + _username +
                    //   "&password=" + _password +
                    //   "&to=" + _to +
                    //   "&sender=" + _sender +
                    //   "&message=" + WebUtility.UrlEncode(Message);

                    string _URL = sender.SMSProvider;
                    string _username = sender.SMSId;
                    string _password = sender.SMSPwd;
                    string _to = MobileNo;
                    string _sender = sender.SMSSenderId;
                    _createURL = _URL.TrimEnd('?') + "?" +
                        "username=" + _username +
                       "&password=" + _password +
                       "&to=" + _to +
                       "&sender=" + _sender +
                       "&message=" + WebUtility.UrlEncode(Message);
                }
                else
                {
                    // string _URL = sender.SmsProvider;
                    // string _username = sender.SmsNo;
                    // string _password = sender.Password;
                    // string _to = MobileNo;
                    // string _sender = sender.SmsId;
                    // _createURL = _URL +
                    //"username=" + _username +
                    //"&password=" + _password +
                    //"&to=" + _to +
                    //"&from=" + _sender +
                    //"&message=" + WebUtility.UrlEncode(Message);

                    string _URL = sender.SMSProvider;
                    string _username = sender.SMSId;
                    string _password = sender.SMSPwd;
                    string _to = MobileNo;
                    string _sender = sender.SMSSenderId;
                    _createURL = _URL.TrimEnd('?') + "?" +
                   "username=" + _username +
                   "&password=" + _password +
                   "&to=" + _to +
                   "&from=" + _sender +
                   "&message=" + WebUtility.UrlEncode(Message);
                }

                try
                {
                    HttpWebRequest _createRequest = (HttpWebRequest)WebRequest.Create(_createURL);
                    HttpWebResponse myResp = (HttpWebResponse)_createRequest.GetResponse();

                    using var reader = new StreamReader(myResp.GetResponseStream());
                    var responseString = reader.ReadToEnd();

                    Console.WriteLine("SMS API RESPONSE: " + responseString);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SMS ERROR: " + ex.Message);
                }

                //HttpWebRequest _createRequest = (HttpWebRequest)WebRequest.Create(_createURL);
                //HttpWebResponse myResp = (HttpWebResponse)_createRequest.GetResponse();
                //System.IO.StreamReader _responseStreamReader = new System.IO.StreamReader(myResp.GetResponseStream());
                //string responseString = _responseStreamReader.ReadToEnd();
                //Console.WriteLine(responseString);
                //_responseStreamReader.Close();
                //myResp.Close();
            }
            return true;
        }

    }
}
