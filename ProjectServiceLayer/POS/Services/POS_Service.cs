using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HMS_360_PMS.DAL_Layers.KOT;
using HMS_360_PMS.DAL_Layers.POS;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.Helper.Security;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectEntitiesModels.Payment;
using HMS_360_PMS.ProjectServiceLayer.GeneralSettings.Services;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.Services_Layers.POS.Interfaces;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using static Dapper.SqlMapper;

namespace HMS_360_PMS.Services_Layers.POS.Services
{
    public class POS_Service : IPOS_Services
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IPOS_Repository _repository;
        private readonly JwtTokenService _jwtService;
        private readonly POS_DAL _posdal;
        private readonly KotBillSettlement_DAL _kotbillsettlementdal;
        private readonly NCKotBillSettlement_DAL _nckotbillsettlementdal;
        private readonly AppSettings _appSettings;
        private readonly PhonePeSettings _phonePe;
        private readonly DbConnectionFactory _factory;
        private readonly ProductLicence_Service _productLicenceService;

        public POS_Service(IHttpClientFactory httpClientFactory, IPOS_Repository repository, JwtTokenService jwtService, POS_DAL posdal, KotBillSettlement_DAL kotbillsettlementdal, NCKotBillSettlement_DAL nckotbillsettlementdal, IOptions<AppSettings> appSettings, IOptions<PhonePeSettings> phonePe, DbConnectionFactory factory, ProductLicence_Service productLicenceService)
        {
            _httpClientFactory = httpClientFactory;
            _repository = repository;
            _jwtService = jwtService;
            _posdal = posdal;
            _kotbillsettlementdal = kotbillsettlementdal;
            _nckotbillsettlementdal = nckotbillsettlementdal;
            _appSettings = appSettings.Value;
            _phonePe = phonePe.Value;
            _factory = factory;
            _productLicenceService = productLicenceService;
        }

        public async Task<IEnumerable<BranchDto>> GetBranchesByUsername(string username)
        {
            var branches = await _repository.GetBranchesByUsername(username);

            return branches.Select(x => new BranchDto
            {
                Branch_code = x.Branch_code,
                Branch_name = x.Branch_name,
                BrId = x.BrId,
                Company_code = x.Company_code
            });
        }

        public async Task<CompanyInfoDto> GetCompanyInfoByBranchCode(string branchcode, int companycode)
        {
            var Componyinfo = await _repository.GetCompanyInfoByBranchCode(branchcode, companycode);

            if (Componyinfo == null)
                throw new Exception("Null Exception");

            return new CompanyInfoDto
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

        public async Task<UserMasterDto> Login(LoginRequestDto request)
        {
            int uid = 0;
            DateTime istTime = ConvertUtcToIst();

            var user = await _repository.GetUserByUsername(request.Username, request.Branch_code);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid Username");

            if (user.UserPassword != request.Password)
                throw new UnauthorizedAccessException("Invalid Password");

            if (user.UserName.ToUpper() == "ADMIN")
                uid = 100;
            else
                uid = user.UserCode;

            await _repository.InsertUserLog(uid, request.Branch_code, istTime);
            var token = _jwtService.GenerateToken(user);

            return new UserMasterDto
            {
                UserCode = user.UserCode,
                UserName = user.UserName,
                Branch_code = user.Branch_code,
                storeid = user.storeid,
                DisPercent = user.DisPercent,
                DisAmount = user.DisAmount,
                Token = token,
                RoleId = user.RoleId
            };
        }

        //public async Task<IEnumerable<BillConfig>> GetBillConfig(int usercode, string username, string branchcode)
        //{
        //    var billConfig = await _repository.GetBillConfig(branchcode);
        //    var PosUserAccessRight = await _repository.GetPosUserAccessRight(usercode, username, branchcode);

        //    if (billConfig == null || !billConfig.Any())
        //        return new List<BillConfig>(); // frontend can check empty

        //    var reqBillExists = billConfig.Where(x => x.Config == "REQBILL");
        //    var billNoExists = billConfig.Where(x => x.Config == "BillNo" && x.isreq == '1');
        //    var kotNoExists = billConfig.Where(x => x.Config == "KotNo" && x.isreq == '1');
        //    var ReportExists = billConfig.Where(x => x.Config == "Report" && x.isreq == '1');

        //    return billConfig;
        //}

        public async Task<POSServiceResult<POSLoginResponseDto>> LoginWithConfig(LoginRequestDto request)
        {
            try
            {
                DateTime currentdate = ConvertUtcToIst();
                var returnMessage = "";
                bool productValid = false;
                bool SerialKeyExpired = false;
                string SerialKey = string.Empty;
                string decryptSerialkey = string.Empty;
                //string decryptValidDate = string.Empty;
                //string decryptIntimationDate = string.Empty;

                var companyInfo = await GetCompanyInfoByBranchCode(request.Branch_code, request.Company_code);

                var userDto = await Login(request);

                var billConfig = await _repository.GetBillConfig(request.Branch_code);

                //var userRights = await _repository.GetPosUserAccessRight(userDto.UserCode, userDto.UserName, request.Branch_code);

                var ProductLicence = await _repository.GetProductLicenceKey(request.Branch_code);

                var reqBillConfigs = billConfig.Where(x => x.Config == "REQBILL").ToList();

                var billNoConfigs = billConfig.Where(x => x.Config == "BillNo" && x.isreq == "1").ToList();

                var kotNoConfigs = billConfig.Where(x => x.Config == "KotNo" && x.isreq == "1").ToList();

                var reportConfigs = billConfig.Where(x => x.Config == "Report" && x.isreq == "1").ToList();

                var loginResponse = new POSLoginResponseDto
                {
                    User = userDto,
                    CompanyInfo = companyInfo,
                    BillConfigs = billConfig,
                    //UserRights = userRights,
                    ReqBillConfigs = reqBillConfigs,
                    BillNoConfigs = billNoConfigs,
                    KotNoConfigs = kotNoConfigs,
                    ReportConfigs = reportConfigs,
                    SerialKey = SerialKey,
                    SerialKeyExpired = SerialKeyExpired
                };

                if (ProductLicence == null)
                {
                    SerialKey = DateTime.Now.ToString("yyyy-MMdd-HHmm-ssff");

                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = true,
                        Message = "Product licence not found",
                        Data = new POSLoginResponseDto
                        {
                            User = userDto,
                            SerialKey = SerialKey,
                            SerialKeyExpired = true
                        }
                    };
                }

                int daysRemaining = (ProductLicence.ValidDate.Date - currentdate.Date).Days;
                decryptSerialkey = _productLicenceService.Decrypt(ProductLicence.Encryptedserialkey);
                //decryptValidDate = _productLicenceService.Decrypt(ProductLicence.EncryptedToDate);
                //decryptIntimationDate = _productLicenceService.Decrypt(ProductLicence.EncryptedIntimationDate);

                DateTime decryptValidDate = DateTime.Parse(_productLicenceService.Decrypt(ProductLicence.EncryptedToDate));
                DateTime decryptIntimationDate = DateTime.Parse(_productLicenceService.Decrypt(ProductLicence.EncryptedIntimationDate));

                if (decryptSerialkey != ProductLicence.SerialKey)
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid serial key",
                        Data = null
                    };
                }
                else if (decryptValidDate.Date != ProductLicence.ValidDate.Date)
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid valid date",
                        Data = null
                    };
                }
                else if (decryptIntimationDate.Date != ProductLicence.IntimationDate.Date)
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = false,
                        Message = "Invalid intimation date",
                        Data = null
                    };
                }

                if (currentdate.Date > decryptValidDate.Date)
                {
                    productValid = true;
                    SerialKey = DateTime.Now.ToString("yyyy-MMdd-HHmm-ssff");
                    SerialKeyExpired = true;
                    returnMessage = "Your Period Has Been Expiried, Please Contact Software Vender";
                }
                else if (currentdate.Date > decryptIntimationDate.Date)
                {
                    if (daysRemaining <= ProductLicence.IndimationDays)
                    {
                        productValid = true;
                        returnMessage = $"Your license will expire in {daysRemaining} day(s). Please contact the software vendor.";
                    }
                }

                if (!productValid && daysRemaining > 0)
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = true,
                        Message = "Login successfully",
                        Data = loginResponse
                    };
                }
                else if (productValid && daysRemaining >= 0)
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = true,
                        Message = returnMessage,
                        Data = loginResponse
                    };
                }
                //else if (productValid && daysRemaining == 0)
                //{
                //    loginResponse.SerialKey = DateTime.Now.ToString("yyyy-MMdd-HHmm-ssff");
                //    return new POSServiceResult<POSLoginResponseDto>
                //    {
                //        Success = true,
                //        Message = returnMessage,
                //        Data = loginResponse
                //    };
                //}
                else
                {
                    return new POSServiceResult<POSLoginResponseDto>
                    {
                        Success = true,
                        Message = returnMessage,
                        Data = new POSLoginResponseDto
                        {
                            User = userDto,
                            SerialKey = SerialKey,
                            SerialKeyExpired = true
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                return new POSServiceResult<POSLoginResponseDto>
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<IEnumerable<SystemOutletModel>> GetSystemOutlet()
        {
            var SystemOutletList = await _repository.GetSystemOutlet();

            return SystemOutletList;
        }

        public async Task<IEnumerable<StewardMasterResponseDto>> GetStewardList(string branchcode)
        {
            var StewardList = await _repository.GetStewardList(branchcode);

            return StewardList.Select(x => new StewardMasterResponseDto
            {
                StwCode = x.StwCode,
                POSCode = x.POSCode,
                StwName = x.StwName,
                UserCode = x.UserCode,
                LastModify = x.LastModify,
                Branch_Code = x.Branch_Code,
                MobNo = x.MobNo
            });
        }

        public async Task<IEnumerable<OutletDto>> GetCombinedOutletandtablemasterList(int usercode, string branchcode)
        {
            var SystemOutletList = await _repository.GetSystemOutlet();
            var SystemOutletDetail = SystemOutletList.Where(s => Convert.ToInt32(s.SystemName) == usercode).FirstOrDefault();

            var CombinedList = await _repository.GetCombinedOutletandtablemasterList(branchcode);

            var allowedOltCodes = SystemOutletDetail.OltCode
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToHashSet();

            var list = CombinedList
                .Where(w => allowedOltCodes.Contains(w.OltCode))
                .ToList();

            if (list == null || !list.Any())
                return null;

            var result = list
                .GroupBy(x => new
                {
                    x.OltCode,
                    x.POSCode,
                    x.OltName,
                    x.OltIsRoomService,
                    x.OltServiceTaxRequired,
                    x.OltIsParcelService,
                    x.OltIsFastFood,
                    x.IsDirectKOTandBill,
                    x.IsDirectBill,
                    x.IsDirectPaxandStw
                })
                .Select(group => new OutletDto
                {
                    OltCode = group.Key.OltCode,
                    POSCode = group.Key.POSCode,
                    OltName = group.Key.OltName,
                    OltIsRoomService = group.Key.OltIsRoomService,
                    OltServiceTaxRequired = group.Key.OltServiceTaxRequired,
                    OltIsParcelService = group.Key.OltIsParcelService,
                    OltIsFastFood = group.Key.OltIsFastFood,
                    IsDirectKOTandBill = group.Key.IsDirectKOTandBill,
                    IsDirectBill = group.Key.IsDirectBill,
                    IsDirectPaxandStw = group.Key.IsDirectPaxandStw,

                    Tables = group.Select(x => new TableDto
                    {
                        TblCode = x.TblCode,
                        TblNo = x.TblNo,
                        TblSeatCount = x.TblSeatCount,
                        LastModify = x.LastModify,
                        c = x.c,
                        Branch_Code = x.Branch_Code,
                        QR_Code = x.QR_Code,
                        TableStatus = x.TableStatus,
                        KOTChargeable = x.KOTChargeable,
                        KOTStatus = x.KOTStatus,
                        //BillNo = x.BillNo,
                        //BillAmount = x.BillAmount,
                    }).ToList()
                })
                .ToList();

            return result;
        }

        public async Task<IEnumerable<ItemMasterResponseDto>> GetItemMasterList(string branchcode)
        {
            var ItemMasterList = await _repository.GetItemMasterList(branchcode);

            return ItemMasterList.Select(x => new ItemMasterResponseDto
            {
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                CatCode = x.CatCode,
                GrpCode = x.GrpCode,
                ItemDiscountAllowed = x.ItemDiscountAllowed,
                BranchCode = x.branch_code,
                Thumbnail = x.thumb,
                IsVeg = x.IsVeg
            });
        }

        public async Task<IEnumerable<ItemCategoryResponseDto>> GetItemCategoryList(string branchcode)
        {
            var ItemCategoryList = await _repository.GetItemCategoryList(branchcode);

            return ItemCategoryList.Select(x => new ItemCategoryResponseDto
            {
                CatCode = x.CatCode,
                CatName = x.CatName,
                BranchCode = x.Branch_Code,
                SubCategory = x.SubCat,
                Thumbnail = x.thumb
            });
        }

        public async Task<IEnumerable<SpecialInstruction>> GetSpecialInfo()
        {
            var SpecialInfo = await _repository.GetSpecialInfo();

            return SpecialInfo;
        }

        public async Task<IEnumerable<ItemGroupResponseDto>> GetItemGroupList(string branchcode)
        {
            var ItemGroupList = await _repository.GetItemGroupList(branchcode);

            return ItemGroupList.Select(x => new ItemGroupResponseDto
            {
                GrpCode = x.GrpCode,
                GrpName = x.GrpName,
                BranchCode = x.Branch_Code,
                Dep = x.Dep
            });
        }

        public async Task<IEnumerable<CategoryListDto>> GetCombinedOltItemList(int oltcode, int grpcode, string branchcode)
        {
            var CombinedList = await _repository.GetCombinedOltItemList(oltcode, grpcode, branchcode);

            if (CombinedList == null || !CombinedList.Any())
            {
                return new List<CategoryListDto>
                {
                    new CategoryListDto
                    {
                        OltCode = oltcode,
                        Branchcode = branchcode,
                        CatCode = 0,
                        CatName = "",
                        catthumb = null,
                        GrpCode = grpcode.ToString(),
                        GrpName = "",

                        Items = new List<ItemListDto>
                        {
                            new ItemListDto
                            {
                                ItemCode = 0,
                                ItemName = "",
                                OIDRate = 0,
                                OIDAvailable = false,
                                ItemDiscountAllowed = false,
                                thumb = null,
                                IsVeg = false
                            }
                        }
                    }
                };
            }
            var result = CombinedList
                 .GroupBy(x => new
                 {
                     x.OltCode,
                     x.Branchcode,
                     x.CatCode,
                     x.CatName,
                     x.catthumb,
                     x.GrpCode,
                     x.GrpName
                 })

                  .Select(group => new CategoryListDto
                  {
                      OltCode = group.Key.OltCode,
                      Branchcode = group.Key.Branchcode,
                      CatCode = group.Key.CatCode,
                      CatName = group.Key.CatName,
                      catthumb = group.Key.catthumb,
                      GrpCode = group.Key.GrpCode,
                      GrpName = group.Key.GrpName,

                      Items = group.Select(x => new ItemListDto
                      {
                          ItemCode = x.ItemCode,
                          ItemName = x.ItemName,
                          OIDRate = x.OIDRate,
                          OIDAvailable = x.OIDAvailable,
                          ItemDiscountAllowed = x.ItemDiscountAllowed,
                          thumb = x.thumb,
                          IsVeg = x.IsVeg
                      }).ToList()
                  })
                  .ToList();
            return result;
        }

        public async Task<IEnumerable<CategoryListDto>> GetCombinedIMandICList(string branchcode)
        {
            var CombinedList = await _repository.GetCombinedIMandICList(branchcode);

            var result = CombinedList
                .GroupBy(x => new
                {
                    x.Branchcode,
                    x.CatCode,
                    x.CatName,
                    x.catthumb,
                    x.GrpCode,
                    x.GrpName
                })
                .Select(group => new CategoryListDto
                {
                    Branchcode = group.Key.Branchcode,
                    CatCode = group.Key.CatCode,
                    CatName = group.Key.CatName,
                    catthumb = group.Key.catthumb,
                    GrpCode = group.Key.GrpCode,
                    GrpName = group.Key.GrpName,

                    Items = group.Select(x => new ItemListDto
                    {
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        //ItemRate = x.ItemRate,
                        ItemDiscountAllowed = x.ItemDiscountAllowed,
                        thumb = x.thumb,
                        IsVeg = x.IsVeg
                    }).ToList()
                })
                .ToList();
            return result;
        }


        #region

        public async Task<KOTBillModelDto> SubmitPOSOrder(CartModel cartdetails)
        {

            int resId = 0;
            int mkotno = 0;
            var otnames = "0";

            BillResponse fastfoodbill = new BillResponse();
            BillModel BillModel = new BillModel();

            if (cartdetails.Mode == "VOID")
            {

            }
            else
            {
                resId = Convert.ToInt32(cartdetails.VRemarks);
            }

            var tableRSql = await _repository.TableReservations(resId);

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
                var financialList = await _posdal.GetFinancialMasters(cartdetails.Branch);

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

                var updateRequest = new UpdateKOTMasterRequest
                {
                    KOTNo = kot.KOTNo,
                    GuestCode = cartdetails.GuestCode,
                    NCCode = cartdetails.NCCode,
                    Table = cartdetails.Table,
                    SubTable = cartdetails.SubTable,
                    KotMobileNo = cartdetails.KotMobileNo,
                    branchcode = cartdetails.Branch
                };

                bool stats = await _repository.UpdateKOTMasterAsync(updateRequest);

                bool tablestats = await _repository.UpdateTableReservationAsync(kot.KOTNo, resId);

                string outlet = cartdetails.OutletName?.Replace(" ", "").Trim().ToLowerInvariant();

                var outletmaster = await _repository.GetOutletNameAsync(cartdetails.Outlet, cartdetails.Branch);

                var Billrequired = await _posdal.IsDirectBillSettlementOnline(cartdetails.Outlet, cartdetails.Branch);

                var ncList = await _repository.GetNCKOT(cartdetails.Branch);

                var ncKotName = ncList .Where(x => x.NCDepCode == cartdetails.NCCode) .Select(x => x.NCDepName) .FirstOrDefault();


                foreach (var item in cartdetails.Food)
                {
                    //var spinfo = _repository.GetSpecialInfoId(item.Comment);

                    var spinfo = await _repository.GetSpecialInfoIds(item.Comment, cartdetails.Branch);

                    var spinfoString = string.Join(",", spinfo);

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

                    //otnames = outletname.ToString();
                    if (outlet.ToLower() != "fast food" || outlet.ToLower() != "fastfood" || outlet.ToLower() != "parcel" || outlet.ToLower() != "home delivery")
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
                                ItemCode = item.Id,
                                Branch_Code = cartdetails.Branch
                            };

                            var pDetails = await _repository.InsertTmpKotPrintAsync(kotprintrequest);
                        }
                        else if (type1.ToLower() == "bill" && outletmaster.IsDirectKOTandBill == true && Billrequired == false)
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
                                ItemCode = item.Id,
                                Branch_Code = cartdetails.Branch
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
                    BillModel.Cart = cartdetails;
                    BillModel.Tax = await GetBill(cartdetails);
                    BillModel.BillingType = BillConfig.BillType;
                    BillModel.SubBillingType = BillConfig.SubBillType;
                    fastfoodbill = await PostBill(BillModel);
                }

                if (otnames == "Home Delivery")
                {
                    var guest = new HomeDelivery
                    {
                        GuestName = cartdetails.HomeDelivary.GuestName,
                        Address = cartdetails.HomeDelivary.Address,
                        City = cartdetails.HomeDelivary.City,
                        Phone = cartdetails.HomeDelivary.Phone,
                        Remarks = cartdetails.HomeDelivary.Remarks,
                        Branch_code = cartdetails.Branch,
                        Email = kot.KOTId.ToString() // storing KOTId
                    };

                    if (cartdetails.HomeDelivary.isUpdate == 0)
                    {
                        // New guest
                        guest.GuestCode = await _repository.GetNextGuestCodeAsync(cartdetails.Branch);
                        await _repository.InsertGuestAsync(guest);
                    }
                    else
                    {
                        // Existing guest
                        await _repository.UpdateGuestAsync(guest);
                    }
                }

                if (cartdetails.Mode == "ADD" && outlet.ToLower() != "fastfood" && outletmaster.IsDirectKOTandBill == false )
                {
                    //var bill = await _repository.GetKotBillAsync(mkotno, 0, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.KotMinTimer);

                    //                // 1️⃣ Get KOT Bill
                    //                var bill = await _repository.GetKotBillAsync(
                    //                    mkotno,
                    //                    0,
                    //                    cartdetails.Outlet,
                    //                    cartdetails.Table,
                    //                    cartdetails.SubTable
                    //                );

                    //                var first = bill.FirstOrDefault();
                    //                if (first == null) return null;

                    //                // 2️⃣ Build KOTBillModelDto
                    //                var kotbill = new KOTBillModelDto
                    //                {
                    //                    UserCode = first.UserCode,
                    //                    KOTId = first.KOTId,
                    //                    KOTTblNo = first.KOTTblNo,
                    //                    SubTable = first.SubTable,
                    //                    Outlet = first.Outlet,
                    //                    OutletName = first.OutletName,
                    //                    Waiter = first.Waiter,
                    //                    WaiterName = first.WaiterName,
                    //                    Pax = first.Pax,
                    //                    Branchcode = first.Branchcode,
                    //                    NCCode = first.NCCode,
                    //                    NCRemarks = first.NCRemarks,
                    //                    KOTTime = first.KOTTime,
                    //                    KotMinTimer = cartdetails.KotMinTimer,
                    //                    Food = bill.Select(x => new KOTBillFoodModel
                    //                    {
                    //                        ItemCode = x.ItemCode,
                    //                        Food = x.Food,
                    //                        Comment = x.Comment,
                    //                        Category = x.CatCode,
                    //                        OrigQty = Convert.ToInt32(x.OrigQty)
                    //                    }).ToList()
                    //                };

                    //                // 3️⃣ Get category group and printer details
                    //                var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch);
                    //                var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);

                    //                if (CatGrpDetails.Count == 0 || DefaultPrinter.Count == 0)
                    //                    return kotbill; // return KOT without printers if none

                    //                // 4️⃣ Build CatCode → GrpCode dictionary
                    //                // 4️⃣ Build CatCode → GrpCode dictionary safely
                    //                var catToGroup = CatGrpDetails
                    //                    .SelectMany(g => g.CatGrp.Split(',')
                    //                                              .Select(c => new { Cat = Convert.ToInt32(c.Trim()), Grp = g.Grp }))
                    //                    .GroupBy(x => x.Cat)         // group by Cat to remove duplicates
                    //                    .ToDictionary(g => g.Key, g => g.First().Grp); // take first Grp if duplicates

                    //                // 5️⃣ Merge printers using LINQ grouping
                    //                kotbill.Printers = kotbill.Food
                    //.Select(f => new
                    //{
                    //    GrpCode = catToGroup.TryGetValue(Convert.ToInt32(f.Category), out var g) ? g : (int?)null
                    //})
                    //.Where(x => x.GrpCode.HasValue)
                    //.Select(x => x.GrpCode.Value)
                    //.Distinct() // one printer per group
                    //.Select(grpCode =>
                    //{
                    //    var printer = DefaultPrinter.FirstOrDefault(p => p.GrpCode == grpCode);
                    //    if (printer == null) return null;

                    //    return new Printerdetaildto
                    //    {
                    //        PrinterName = printer.PrinterName,
                    //        BillType = printer.BillType,
                    //        PrintType = printer.PrintType,
                    //        GrpCode = printer.GrpCode
                    //        // ✅ No CategoryIds
                    //    };
                    //})
                    //.Where(p => p != null)
                    //.ToList();

                    //                return kotbill;

                    var bill = await _repository.GetKotBillAsync(0, kot.KOTId, 0, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.Branch);
                    var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch
                    var UtilityPrinter = await _repository.GetUtilityPrinter(cartdetails.Branch, cartdetails.Outlet);

                    var first = bill.FirstOrDefault();
                    if (first == null) return null;

                    var kotbill = new KOTBillModelDto
                    {
                        Billtype = cartdetails.Type == "K" ? "KOT" : "NCKOT",
                        UserCode = first.UserCode,
                        KOTId = first.KOTId,
                        KOTTblNo = first.KOTTblNo,
                        SubTable = first.SubTable,
                        Outlet = first.Outlet,
                        OutletName = first.OutletName,
                        Waiter = first.Waiter,
                        WaiterName = first.WaiterName,
                        Pax = first.Pax,
                        Branchcode = first.Branchcode,
                        NCCode = first.NCCode,
                        NCRemarks = first.NCRemarks,
                        KOTTime = first.KOTTime,
                        KotMinTimer = cartdetails.KotMinTimer,
                        TaxType = cartdetails.TaxType,
                        NCDepName = ncKotName
                    };

                    // Food Items
                    kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    {
                        ItemCode = x.ItemCode,
                        Food = x.Food,
                        ItemRate = x.ItemRate,
                        Comment = x.Comment,
                        Category = x.CatCode,
                        GrpCode = x.GrpCode,
                        OrigQty = Convert.ToInt32(x.OrigQty)
                    }).ToList();

                    // --------------------
                    // Step 1: Category → Group Mapping (ONLY for current branch)
                    // --------------------
                    //var catToGroup = new Dictionary<int, int>();

                    //foreach (var grp in CatGrpDetails)
                    //{
                    //    //var cats = grp.CatGrp.Split(',')
                    //    //                     .Select(x => Convert.ToInt32(x.Trim()));
                    //    var cats = grp.CatGrp.Split(',')
                    //   .Select(x => Convert.ToInt32(x.Trim()))
                    //   .Distinct();

                    //    foreach (var cat in cats)
                    //    {
                    //        catToGroup[cat] = grp.Grp;
                    //    }
                    //}
                    var catToGroups = new Dictionary<int, HashSet<int>>();

                    foreach (var grp in CatGrpDetails)
                    {
                        var cats = grp.CatGrp
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => Convert.ToInt32(x.Trim()))
                                      .Distinct();

                        foreach (var cat in cats)
                        {
                            if (!catToGroups.ContainsKey(cat))
                                catToGroups[cat] = new HashSet<int>();

                            catToGroups[cat].Add(grp.Grp);
                        }
                    }
                    // --------------------
                    // Step 2: Group Categories from Food
                    // --------------------
                    //var groupCategories = new Dictionary<int, HashSet<int>>();

                    //foreach (var food in kotbill.Food)
                    //{
                    //    int catCode = Convert.ToInt32(food.Category);

                    //    if (catToGroup.TryGetValue(catCode, out int grpCode))
                    //    {
                    //        if (!groupCategories.ContainsKey(grpCode))
                    //            groupCategories[grpCode] = new HashSet<int>();

                    //        groupCategories[grpCode].Add(catCode);
                    //    }
                    //}
                    var groupCategories = new Dictionary<int, HashSet<int>>();

                    foreach (var food in kotbill.Food)
                    {
                        int catCode = Convert.ToInt32(food.Category);

                        if (catToGroups.TryGetValue(catCode, out var grpCodes))
                        {
                            foreach (var grpCode in grpCodes)
                            {
                                if (!groupCategories.ContainsKey(grpCode))
                                    groupCategories[grpCode] = new HashSet<int>();

                                groupCategories[grpCode].Add(catCode);
                            }
                        }
                    }

                    // --------------------
                    // Step 3: Build Printer List (with Branch + Fallback)
                    // --------------------
                    var printerList = new List<Printerdetaildto>();

                    foreach (var grp in groupCategories)
                    {
                        int grpCode = grp.Key;
                        var categories = grp.Value.ToList();

                        var printers = UtilityPrinter?
                            .Where(p => p.GrpCode == grpCode &&
                                        p.Branch_Code == cartdetails.Branch) // ✅ FIX HERE
                            .ToList();

                        // ✅ Case 1: Printer exists for this branch + group
                        if (printers != null && printers.Any())
                        {
                            foreach (var printer in printers)
                            {
                                printerList.Add(new Printerdetaildto
                                {
                                    PrinterName = printer.PrinterName,
                                    BillType = printer.BillType,
                                    Branch_Code = printer.Branch_Code,
                                    OltCode = printer.OltCode,
                                    PrintType = printer.PrintType,
                                    GrpCode = printer.GrpCode,
                                    CategoryIds = categories,
                                    IPAddress = printer?.IPAddress ?? string.Empty

                                });
                            }
                        }
                        else
                        {
                            // ✅ Case 2: No printer for this branch → fallback
                            printerList.Add(new Printerdetaildto
                            {
                                PrinterName = string.Empty,
                                BillType = string.Empty,
                                Branch_Code = string.Empty,
                                OltCode = string.Empty,
                                PrintType = string.Empty,
                                GrpCode = grpCode,
                                CategoryIds = categories,
                                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty

                            });
                        }
                    }

                    if (groupCategories.Count() == 0)
                    {
                        printerList.Add(new Printerdetaildto
                        {
                            PrinterName = string.Empty,
                            BillType = string.Empty,
                            Branch_Code = string.Empty,
                            OltCode = string.Empty,
                            PrintType = string.Empty,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                        });
                    }

                    // --------------------
                    // Final Assignment
                    // --------------------
                    kotbill.Printers = printerList;

                    return kotbill;


                    // Map Food → Printer
                    //var printerList = new List<Printerdetaildto>();

                    //foreach (var food in kotbill.Food)
                    //{
                    //    int catCode = Convert.ToInt32(food.Category);

                    //    if (catToGroup.TryGetValue(catCode, out int grpCode))
                    //    {
                    //        var printers = DefaultPrinter
                    //            .Where(p => p.GrpCode == grpCode);

                    //        foreach (var printer in printers)
                    //        {
                    //            printerList.Add(new Printerdetaildto
                    //            {
                    //                PrinterName = printer.PrinterName,
                    //                BillType = printer.BillType,
                    //                PrintType = printer.PrintType,
                    //                GrpCode = printer.GrpCode,
                    //                CategoryIds = new List<int> { catCode }
                    //            });
                    //        }
                    //    }
                    //}

                    //kotbill.Printers = printerList;
                    //var printerDict = new Dictionary<string, Printerdetaildto>();

                    //foreach (var food in kotbill.Food)
                    //{
                    //    int catCode = Convert.ToInt32(food.Category);

                    //    if (catToGroup.TryGetValue(catCode, out int grpCode))
                    //    {
                    //        var printers = DefaultPrinter
                    //            .Where(p => p.GrpCode == grpCode);

                    //        foreach (var printer in printers)
                    //        {
                    //            // Unique key per printer
                    //            string key = $"{printer.PrinterName}_{printer.GrpCode}_{printer.PrintType}";

                    //            if (!printerDict.TryGetValue(key, out var existing))
                    //            {
                    //                existing = new Printerdetaildto
                    //                {
                    //                    PrinterName = printer.PrinterName,
                    //                    BillType = printer.BillType,
                    //                    Branch_Code = printer.Branch_Code,
                    //                    OltCode = printer.OltCode,
                    //                    PrintType = printer.PrintType,
                    //                    GrpCode = printer.GrpCode,
                    //                    CategoryIds = new List<int>()
                    //                };

                    //                printerDict[key] = existing;
                    //            }

                    //            // Merge category ids (avoid duplicates)
                    //            if (!existing.CategoryIds.Contains(catCode))
                    //            {
                    //                existing.CategoryIds.Add(catCode);
                    //            }
                    //        }
                    //    }
                    //}

                    //// Final assignment
                    //kotbill.Printers = printerDict.Values.ToList();


                    //var first = bill.FirstOrDefault();
                    //if (first == null) return null;

                    //var kotbill = new KOTBillModelDto
                    //{
                    //    UserCode = first.UserCode,
                    //    KOTId = first.KOTId,
                    //    KOTTblNo = first.KOTTblNo,
                    //    SubTable = first.SubTable,
                    //    Outlet = first.Outlet,
                    //    OutletName = first.OutletName,
                    //    Waiter = first.Waiter,
                    //    WaiterName = first.WaiterName,
                    //    Pax = first.Pax,
                    //    Branchcode = first.Branchcode,
                    //    NCCode = first.NCCode,
                    //    NCRemarks = first.NCRemarks,
                    //    KOTTime = first.KOTTime,
                    //    KotMinTimer = cartdetails.KotMinTimer
                    //};

                    //kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    //{
                    //    ItemCode = x.ItemCode,
                    //    Food = x.Food,
                    //    Comment = x.Comment,
                    //    Category = x.CatCode,
                    //    OrigQty = Convert.ToInt32(x.OrigQty)
                    //}).ToList();

                    //return kotbill;


                }
                else if (cartdetails.Mode == "VOID" && outletmaster.IsDirectKOTandBill == false)
                {
                    var bill = await _repository.GetKotBillAsync(0, kot.KOTId, 1, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.Branch);
                    var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch
                    var UtilityPrinter = await _repository.GetUtilityPrinter(cartdetails.Branch, cartdetails.Outlet);

                    var first = bill.FirstOrDefault();
                    if (first == null) return null;

                    var kotbill = new KOTBillModelDto
                    {
                        Billtype = cartdetails.Type == "K" ? "KOTVOID" : "NCKOTVOID",
                        UserCode = first.UserCode,
                        KOTId = first.KOTId,
                        KOTTblNo = first.KOTTblNo,
                        SubTable = first.SubTable,
                        Outlet = first.Outlet,
                        OutletName = first.OutletName,
                        Waiter = first.Waiter,
                        WaiterName = first.WaiterName,
                        Pax = first.Pax,
                        Branchcode = first.Branchcode,
                        NCCode = first.NCCode,
                        NCRemarks = first.NCRemarks,
                        KOTTime = first.KOTTime,
                        KotMinTimer = cartdetails.KotMinTimer,
                        TaxType = cartdetails.TaxType,
                        NCDepName = ncKotName
                    };

                    // Food Items
                    kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    {
                        ItemCode = x.ItemCode,
                        Food = x.Food,
                        ItemRate = x.ItemRate,
                        Comment = x.Comment,
                        Category = x.CatCode,
                        GrpCode = x.GrpCode,
                        OrigQty = Convert.ToInt32(x.OrigQty)
                    }).ToList();

                    // --------------------
                    // Step 1: Category → Group Mapping (ONLY for current branch)
                    // --------------------
                    var catToGroups = new Dictionary<int, HashSet<int>>();

                    foreach (var grp in CatGrpDetails)
                    {
                        var cats = grp.CatGrp
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => Convert.ToInt32(x.Trim()))
                                      .Distinct();

                        foreach (var cat in cats)
                        {
                            if (!catToGroups.ContainsKey(cat))
                                catToGroups[cat] = new HashSet<int>();

                            catToGroups[cat].Add(grp.Grp);
                        }
                    }

                    // --------------------
                    // Step 2: Group Categories from Food
                    // --------------------
                    var groupCategories = new Dictionary<int, HashSet<int>>();

                    foreach (var food in kotbill.Food)
                    {
                        int catCode = Convert.ToInt32(food.Category);

                        if (catToGroups.TryGetValue(catCode, out var grpCodes))
                        {
                            foreach (var grpCode in grpCodes)
                            {
                                if (!groupCategories.ContainsKey(grpCode))
                                    groupCategories[grpCode] = new HashSet<int>();

                                groupCategories[grpCode].Add(catCode);
                            }
                        }
                    }

                    // --------------------
                    // Step 3: Build Printer List (with Branch + Fallback)
                    // --------------------
                    var printerList = new List<Printerdetaildto>();

                    foreach (var grp in groupCategories)
                    {
                        int grpCode = grp.Key;
                        var categories = grp.Value.ToList();

                        var printers = UtilityPrinter?
                            .Where(p => p.GrpCode == grpCode &&
                                        p.Branch_Code == cartdetails.Branch) // ✅ FIX HERE
                            .ToList();

                        // ✅ Case 1: Printer exists for this branch + group
                        if (printers != null && printers.Any())
                        {
                            foreach (var printer in printers)
                            {
                                printerList.Add(new Printerdetaildto
                                {
                                    PrinterName = printer.PrinterName,
                                    BillType = printer.BillType,
                                    Branch_Code = printer.Branch_Code,
                                    OltCode = printer.OltCode,
                                    PrintType = printer.PrintType,
                                    GrpCode = printer.GrpCode,
                                    CategoryIds = categories,
                                    IPAddress = printer?.IPAddress ?? string.Empty
                                });
                            }
                        }
                        else
                        {
                            // ✅ Case 2: No printer for this branch → fallback
                            printerList.Add(new Printerdetaildto
                            {
                                PrinterName = string.Empty,
                                BillType = string.Empty,
                                Branch_Code = string.Empty,
                                OltCode = string.Empty,
                                PrintType = string.Empty,
                                GrpCode = grpCode,
                                CategoryIds = categories,
                                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                            });
                        }
                    }

                    if (groupCategories.Count() == 0)
                    {
                        printerList.Add(new Printerdetaildto
                        {
                            PrinterName = string.Empty,
                            BillType = string.Empty,
                            Branch_Code = string.Empty,
                            OltCode = string.Empty,
                            PrintType = string.Empty,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                        });
                    }

                    // --------------------
                    // Final Assignment
                    // --------------------
                    kotbill.Printers = printerList;

                    return kotbill;
                }
                else if (cartdetails.Mode == "ADD" && outlet.ToLower() == "fastfood" && outletmaster.IsDirectKOTandBill == false)
                {
                    var bill = await _repository.GetKotBillAsync(1, kot.KOTId, 0, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.Branch);
                    var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch
                    var UtilityPrinter = await _repository.GetUtilityPrinter(cartdetails.Branch, cartdetails.Outlet);

                    var first = bill.FirstOrDefault();
                    if (first == null) return null;

                    var kotbill = new KOTBillModelDto
                    {
                        Billtype = cartdetails.Type == "K" ? "KOT" : "NCKOT",
                        UserCode = first.UserCode,
                        KOTId = first.KOTId,
                        KOTTblNo = first.KOTTblNo,
                        SubTable = first.SubTable,
                        Outlet = first.Outlet,
                        OutletName = first.OutletName,
                        Waiter = first.Waiter,
                        WaiterName = first.WaiterName,
                        Pax = first.Pax,
                        Branchcode = first.Branchcode,
                        NCCode = first.NCCode,
                        NCRemarks = first.NCRemarks,
                        KOTTime = first.KOTTime,
                        KotMinTimer = cartdetails.KotMinTimer,
                        TaxType = cartdetails.TaxType,
                        NCDepName = ncKotName,
                        FNBillResponse = fastfoodbill
                    };

                    // Food Items
                    kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    {
                        ItemCode = x.ItemCode,
                        Food = x.Food,
                        ItemRate = x.ItemRate,
                        Comment = x.Comment,
                        Category = x.CatCode,
                        GrpCode = x.GrpCode,
                        OrigQty = Convert.ToInt32(x.OrigQty)
                    }).ToList();

                    // --------------------
                    // Step 1: Category → Group Mapping (ONLY for current branch)
                    // --------------------
                    var catToGroups = new Dictionary<int, HashSet<int>>();

                    foreach (var grp in CatGrpDetails)
                    {
                        var cats = grp.CatGrp
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => Convert.ToInt32(x.Trim()))
                                      .Distinct();

                        foreach (var cat in cats)
                        {
                            if (!catToGroups.ContainsKey(cat))
                                catToGroups[cat] = new HashSet<int>();

                            catToGroups[cat].Add(grp.Grp);
                        }
                    }

                    // --------------------
                    // Step 2: Group Categories from Food
                    // --------------------
                    var groupCategories = new Dictionary<int, HashSet<int>>();

                    foreach (var food in kotbill.Food)
                    {
                        int catCode = Convert.ToInt32(food.Category);

                        if (catToGroups.TryGetValue(catCode, out var grpCodes))
                        {
                            foreach (var grpCode in grpCodes)
                            {
                                if (!groupCategories.ContainsKey(grpCode))
                                    groupCategories[grpCode] = new HashSet<int>();

                                groupCategories[grpCode].Add(catCode);
                            }
                        }
                    }

                    // --------------------
                    // Step 3: Build Printer List (with Branch + Fallback)
                    // --------------------
                    var printerList = new List<Printerdetaildto>();

                    foreach (var grp in groupCategories)
                    {
                        int grpCode = grp.Key;
                        var categories = grp.Value.ToList();

                        var printers = UtilityPrinter?
                            .Where(p => p.GrpCode == grpCode &&
                                        p.Branch_Code == cartdetails.Branch) // ✅ FIX HERE
                            .ToList();

                        // ✅ Case 1: Printer exists for this branch + group
                        if (printers != null && printers.Any())
                        {
                            foreach (var printer in printers)
                            {
                                printerList.Add(new Printerdetaildto
                                {
                                    PrinterName = printer.PrinterName,
                                    BillType = printer.BillType,
                                    Branch_Code = printer.Branch_Code,
                                    OltCode = printer.OltCode,
                                    PrintType = printer.PrintType,
                                    GrpCode = printer.GrpCode,
                                    CategoryIds = categories,
                                    IPAddress = printer?.IPAddress ?? string.Empty
                                });
                            }
                        }
                        else
                        {
                            // ✅ Case 2: No printer for this branch → fallback
                            printerList.Add(new Printerdetaildto
                            {
                                PrinterName = string.Empty,
                                BillType = string.Empty,
                                Branch_Code = string.Empty,
                                OltCode = string.Empty,
                                PrintType = string.Empty,
                                GrpCode = grpCode,
                                CategoryIds = categories,
                                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                            });
                        }
                    }

                    if (groupCategories.Count() == 0)
                    {
                        printerList.Add(new Printerdetaildto
                        {
                            PrinterName = string.Empty,
                            BillType = string.Empty,
                            Branch_Code = string.Empty,
                            OltCode = string.Empty,
                            PrintType = string.Empty,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                        });
                    }

                    // --------------------
                    // Final Assignment
                    // --------------------
                    kotbill.Printers = printerList;

                    return kotbill;
                }
                else if(cartdetails.Mode == "ADD" && cartdetails.Type == "K" && outlet.ToLower() != "fastfood" && outletmaster.IsDirectKOTandBill == true)
                {
                    var bill = await _repository.GetKotBillAsync(1, kot.KOTId, 0, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.Branch);
                    var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch
                    var UtilityPrinter = await _repository.GetUtilityPrinter(cartdetails.Branch, cartdetails.Outlet);

                    var first = bill.FirstOrDefault();
                    if (first == null) return null;

                    var kotbill = new KOTBillModelDto
                    {
                        Billtype = cartdetails.Type == "K" ? "KOT" : "NCKOT",
                        UserCode = first.UserCode,
                        KOTId = first.KOTId,
                        KOTTblNo = first.KOTTblNo,
                        SubTable = first.SubTable,
                        Outlet = first.Outlet,
                        OutletName = first.OutletName,
                        Waiter = first.Waiter,
                        WaiterName = first.WaiterName,
                        Pax = first.Pax,
                        Branchcode = first.Branchcode,
                        NCCode = first.NCCode,
                        NCRemarks = first.NCRemarks,
                        KOTTime = first.KOTTime,
                        KotMinTimer = cartdetails.KotMinTimer,
                        TaxType = cartdetails.TaxType,
                        Tax = BillModel.Tax,
                        NCDepName = ncKotName,
                        FNBillResponse = fastfoodbill,
                        isDirectKOTandBill = outletmaster.IsDirectKOTandBill,
                        Message = "KOT and Bill generation is Successful Inserted in KOT and Bill tables for Direct Option"
                    };

                    // Food Items
                    kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    {
                        ItemCode = x.ItemCode,
                        Food = x.Food,
                        ItemRate = x.ItemRate,
                        Comment = x.Comment,
                        Category = x.CatCode,
                        GrpCode = x.GrpCode,
                        OrigQty = Convert.ToInt32(x.OrigQty)
                    }).ToList();

                    // --------------------
                    // Step 1: Category → Group Mapping (ONLY for current branch)
                    // --------------------
                    var catToGroups = new Dictionary<int, HashSet<int>>();

                    foreach (var grp in CatGrpDetails)
                    {
                        var cats = grp.CatGrp
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => Convert.ToInt32(x.Trim()))
                                      .Distinct();

                        foreach (var cat in cats)
                        {
                            if (!catToGroups.ContainsKey(cat))
                                catToGroups[cat] = new HashSet<int>();

                            catToGroups[cat].Add(grp.Grp);
                        }
                    }

                    // --------------------
                    // Step 2: Group Categories from Food
                    // --------------------
                    var groupCategories = new Dictionary<int, HashSet<int>>();

                    foreach (var food in kotbill.Food)
                    {
                        int catCode = Convert.ToInt32(food.Category);

                        if (catToGroups.TryGetValue(catCode, out var grpCodes))
                        {
                            foreach (var grpCode in grpCodes)
                            {
                                if (!groupCategories.ContainsKey(grpCode))
                                    groupCategories[grpCode] = new HashSet<int>();

                                groupCategories[grpCode].Add(catCode);
                            }
                        }
                    }

                    // --------------------
                    // Step 3: Build Printer List (with Branch + Fallback)
                    // --------------------
                    var printerList = new List<Printerdetaildto>();

                    foreach (var grp in groupCategories)
                    {
                        int grpCode = grp.Key;
                        var categories = grp.Value.ToList();

                        var printers = UtilityPrinter?
                            .Where(p => p.GrpCode == grpCode &&
                                        p.Branch_Code == cartdetails.Branch) // ✅ FIX HERE
                            .ToList();

                        // ✅ Case 1: Printer exists for this branch + group
                        if (printers != null && printers.Any())
                        {
                            foreach (var printer in printers)
                            {
                                printerList.Add(new Printerdetaildto
                                {
                                    PrinterName = printer.PrinterName,
                                    BillType = printer.BillType,
                                    Branch_Code = printer.Branch_Code,
                                    OltCode = printer.OltCode,
                                    PrintType = printer.PrintType,
                                    GrpCode = printer.GrpCode,
                                    CategoryIds = categories,
                                    IPAddress = printer?.IPAddress ?? string.Empty
                                });
                            }
                        }
                        else
                        {
                            // ✅ Case 2: No printer for this branch → fallback
                            printerList.Add(new Printerdetaildto
                            {
                                PrinterName = string.Empty,
                                BillType = string.Empty,
                                Branch_Code = string.Empty,
                                OltCode = string.Empty,
                                PrintType = string.Empty,
                                GrpCode = grpCode,
                                CategoryIds = categories,
                                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                            });
                        }
                    }

                    if(groupCategories.Count() == 0)
                    {
                        printerList.Add(new Printerdetaildto
                        {
                            PrinterName = string.Empty,
                            BillType = string.Empty,
                            Branch_Code = string.Empty,
                            OltCode = string.Empty,
                            PrintType = string.Empty,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                        });
                    }

                    // --------------------
                    // Final Assignment
                    // --------------------
                    kotbill.Printers = printerList;

                    return kotbill;
                }
                else if (cartdetails.Mode == "ADD" && cartdetails.Type == "N" && outlet.ToLower() != "fastfood" && outletmaster.IsDirectKOTandBill == true)
                {
                    var bill = await _repository.GetKotBillAsync(1, kot.KOTId, 0, cartdetails.Outlet, cartdetails.Table, cartdetails.SubTable, cartdetails.Branch);
                    var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinter = await _repository.GetDefaultPrinter(cartdetails.Branch, cartdetails.Outlet);
                    var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch
                    var UtilityPrinter = await _repository.GetUtilityPrinter(cartdetails.Branch, cartdetails.Outlet);

                    var first = bill.FirstOrDefault();
                    if (first == null) return null;

                    var kotbill = new KOTBillModelDto
                    {
                        Billtype = cartdetails.Type == "K" ? "KOT" : "NCKOT",
                        UserCode = first.UserCode,
                        KOTId = first.KOTId,
                        KOTTblNo = first.KOTTblNo,
                        SubTable = first.SubTable,
                        Outlet = first.Outlet,
                        OutletName = first.OutletName,
                        Waiter = first.Waiter,
                        WaiterName = first.WaiterName,
                        Pax = first.Pax,
                        Branchcode = first.Branchcode,
                        NCCode = first.NCCode,
                        NCRemarks = first.NCRemarks,
                        KOTTime = first.KOTTime,
                        KotMinTimer = cartdetails.KotMinTimer,
                        TaxType = cartdetails.TaxType,
                        NCDepName = ncKotName,
                        //Tax = BillModel.Tax,
                        FNBillResponse = fastfoodbill,
                        isDirectKOTandBill = outletmaster.IsDirectKOTandBill,
                        Message = "KOT and Bill generation is Successful Inserted in KOT and Bill tables for Direct Option"
                    };

                    // Food Items
                    kotbill.Food = bill.Select(x => new KOTBillFoodModel
                    {
                        ItemCode = x.ItemCode,
                        Food = x.Food,
                        ItemRate = 0,
                        Comment = x.Comment,
                        Category = x.CatCode,
                        GrpCode = x.GrpCode,
                        OrigQty = Convert.ToInt32(x.OrigQty)
                    }).ToList();

                    // --------------------
                    // Step 1: Category → Group Mapping (ONLY for current branch)
                    // --------------------
                    var catToGroups = new Dictionary<int, HashSet<int>>();

                    foreach (var grp in CatGrpDetails)
                    {
                        var cats = grp.CatGrp
                                      .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                      .Select(x => Convert.ToInt32(x.Trim()))
                                      .Distinct();

                        foreach (var cat in cats)
                        {
                            if (!catToGroups.ContainsKey(cat))
                                catToGroups[cat] = new HashSet<int>();

                            catToGroups[cat].Add(grp.Grp);
                        }
                    }

                    // --------------------
                    // Step 2: Group Categories from Food
                    // --------------------
                    var groupCategories = new Dictionary<int, HashSet<int>>();

                    foreach (var food in kotbill.Food)
                    {
                        int catCode = Convert.ToInt32(food.Category);

                        if (catToGroups.TryGetValue(catCode, out var grpCodes))
                        {
                            foreach (var grpCode in grpCodes)
                            {
                                if (!groupCategories.ContainsKey(grpCode))
                                    groupCategories[grpCode] = new HashSet<int>();

                                groupCategories[grpCode].Add(catCode);
                            }
                        }
                    }

                    // --------------------
                    // Step 3: Build Printer List (with Branch + Fallback)
                    // --------------------
                    var printerList = new List<Printerdetaildto>();

                    foreach (var grp in groupCategories)
                    {
                        int grpCode = grp.Key;
                        var categories = grp.Value.ToList();

                        var printers = UtilityPrinter?
                            .Where(p => p.GrpCode == grpCode &&
                                        p.Branch_Code == cartdetails.Branch) // ✅ FIX HERE
                            .ToList();

                        // ✅ Case 1: Printer exists for this branch + group
                        if (printers != null && printers.Any())
                        {
                            foreach (var printer in printers)
                            {
                                printerList.Add(new Printerdetaildto
                                {
                                    PrinterName = printer.PrinterName,
                                    BillType = printer.BillType,
                                    Branch_Code = printer.Branch_Code,
                                    OltCode = printer.OltCode,
                                    PrintType = printer.PrintType,
                                    GrpCode = printer.GrpCode,
                                    CategoryIds = categories,
                                    IPAddress = printer?.IPAddress ?? string.Empty
                                });
                            }
                        }
                        else
                        {
                            // ✅ Case 2: No printer for this branch → fallback
                            printerList.Add(new Printerdetaildto
                            {
                                PrinterName = string.Empty,
                                BillType = string.Empty,
                                Branch_Code = string.Empty,
                                OltCode = string.Empty,
                                PrintType = string.Empty,
                                GrpCode = grpCode,
                                CategoryIds = categories,
                                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty

                            });
                        }
                    }

                    if (groupCategories.Count() == 0)
                    {
                        printerList.Add(new Printerdetaildto
                        {
                            PrinterName = string.Empty,
                            BillType = string.Empty,
                            Branch_Code = string.Empty,
                            OltCode = string.Empty,
                            PrintType = string.Empty,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
                        });
                    }

                    // --------------------
                    // Final Assignment
                    // --------------------
                    kotbill.Printers = printerList;

                    return kotbill;
                }

                else
                {
                    return new KOTBillModelDto
                    {
                        Message = "No data found for the given KOT"
                    };
                }
            }
            catch (Exception ex)
            {
                return new KOTBillModelDto
                {
                    Message = ex.StackTrace.ToString()
                };
                //var type = await _repository.GetKotType(cartdetails.Outlet, cartdetails.Branch);

                //throw;
            }
            //});
        }

        private bool CheckHappyHours(TimeSpan StartTime, TimeSpan EndTime)
        {
            var someTime = DateTime.Now.TimeOfDay;
            return someTime >= StartTime && someTime <= EndTime;
        }

        public async Task<TaxModel> GetBill(CartModel cart)
        {
            double totalbillamt = 0.0;
            int totalqty = 0;
            var posdate = await _repository.GetPOSEntryDate(cart.Branch);

            double discountamt = 0.0; // ✅ FIXED
            string discountin = cart.DiscountIn;

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

                // ===============================
                // ✅ ON BILL DISCOUNT (BEFORE TAX)
                // ===============================
                double discountedBillAmt = totalbillamt;

                if (cart.Discount > 0 && cart.DiscountType.ToLower() == "onbill")
                {
                    if (cart.DiscountIn == "per")
                        discountamt = totalbillamt * cart.Discount / 100;
                    else
                        discountamt = cart.Discount;

                    discountedBillAmt = totalbillamt - discountamt;
                }

                List<Taxdetails> finalTaxList = new List<Taxdetails>();

                // ===============================
                // ✅ GROUPED TAX
                // ===============================
                if (cart.TaxType.ToLower() == "groupedtax")
                {
                    var itemGroups = await _repository.GetItemGroupList(cart.Branch);
                    var expandedItems = new List<TempTaxItem>();

                    foreach (var item in cart.Food)
                    {
                        var taxmodel = await _repository.GetExtraCharges(item.Id.ToString(), cart.Branch, cart.Outlet);

                        double cgstPer = 0;
                        double sgstPer = 0;

                        foreach (var t in taxmodel)
                        {
                            if (t.TaxDescription.Contains("CGST"))
                                cgstPer = t.TaxPercentage;
                            else if (t.TaxDescription.Contains("SGST"))
                                sgstPer = t.TaxPercentage;
                        }

                        double itemTotal = item.Qty * item.Price;

                        // ✅ APPLY ONBILL DISCOUNT
                        if (cart.DiscountType.ToLower() == "onbill" && totalbillamt > 0 && item.itemDiscountAllowed != false)
                        {
                            double ratio = discountedBillAmt / totalbillamt;
                            itemTotal = itemTotal * ratio;
                        }

                        expandedItems.Add(new TempTaxItem
                        {
                            GrpCode = item.GrpCode,
                            ItemTotal = Math.Round(itemTotal, 2), // ✅ FIXED
                            CGSTPer = cgstPer,
                            SGSTPer = sgstPer
                        });
                    }

                    var groupedItems = expandedItems
                        .GroupBy(x => new { x.GrpCode, x.CGSTPer, x.SGSTPer });

                    // ===============================
                    // SELECTED GROUPS FOR GROUPWISE DISCOUNT
                    // ===============================
                    var selectedGroups = groupedItems
                        .Where(g => cart.DiscountGroups != null && cart.DiscountGroups.Contains(
                            itemGroups.FirstOrDefault(x => x.GrpCode == g.Key.GrpCode)?.GrpName,
                            StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    double totalSelectedAmount = selectedGroups.Sum(g => g.Sum(x => x.ItemTotal));

                    foreach (var group in groupedItems)
                    {
                        double groupTotal = group.Sum(x => x.ItemTotal);
                        double discountedGroupTotal = groupTotal;

                        var groupInfo = itemGroups.FirstOrDefault(g => g.GrpCode == group.Key.GrpCode);

                        // ✅ GROUPWISE DISCOUNT
                        if (cart.Discount > 0 && cart.DiscountType.ToLower() == "groupwise")
                        {
                            //if (cart.DiscountGroups != null &&
                            //cart.DiscountGroups.Contains(groupInfo?.GrpName, StringComparer.OrdinalIgnoreCase))
                            //if (selectedGroups.Contains(group))
                            bool isSelected = selectedGroups.Any(sg => sg.Key.GrpCode == group.Key.GrpCode);

                            if (isSelected)
                            {
                                double grpDiscount = 0;

                                if (cart.DiscountIn == "per")
                                {
                                    grpDiscount = groupTotal * cart.Discount / 100;
                                    discountamt += grpDiscount;
                                }
                                else
                                {
                                    grpDiscount = (groupTotal / totalSelectedAmount) * cart.Discount;

                                    //grpDiscount = cart.Discount;
                                    //discountamt = cart.Discount; // apply once
                                }
                                discountamt += grpDiscount;
                                discountedGroupTotal = groupTotal - grpDiscount;
                                //discountedGroupTotal = groupTotal - grpDiscount;
                            }
                        }

                        double cgst = Math.Round(discountedGroupTotal * group.Key.CGSTPer / 100, 2);
                        double sgst = Math.Round(discountedGroupTotal * group.Key.SGSTPer / 100, 2);

                        if (groupInfo?.GrpName == "LIQUOR")
                        {
                            cgst = 0;
                            sgst = 0;
                        }

                        double totalTax = Math.Round(cgst + sgst, 2);

                        string taxName = totalTax > 0
                            ? $"CGST {group.Key.CGSTPer}% + SGST {group.Key.SGSTPer}%"
                            : "NO TAX";

                        finalTaxList.Add(new Taxdetails
                        {
                            GroupCode = group.Key.GrpCode,
                            //GroupName = groupInfo?.GrpName,
                            GroupName = groupInfo?.GrpName ?? string.Empty,
                            TaxName = taxName,
                            Taxper = group.Key.CGSTPer + group.Key.SGSTPer,
                            TaxableAmount = Math.Round(discountedGroupTotal, 2),
                            CGST = cgst,
                            SGST = sgst,
                            TaxAmount = Math.Round(totalTax, 2),
                            Total = Math.Round(discountedGroupTotal + totalTax, 2)
                        });

                        cgsttaxamt += cgst;
                        sgsttaxamt += sgst;
                    }

                    finalTaxList = finalTaxList
                        .GroupBy(x => new { x.GroupCode, x.TaxName })
                        .Select(g => new Taxdetails
                        {
                            GroupCode = g.Key.GroupCode,
                            GroupName = g.First().GroupName,
                            TaxName = g.Key.TaxName,
                            Taxper = g.First().Taxper,
                            TaxableAmount = Math.Round(g.Sum(x => x.TaxableAmount), 2),
                            CGST = Math.Round(g.Sum(x => x.CGST), 2),
                            SGST = Math.Round(g.Sum(x => x.SGST), 2),
                            TaxAmount = Math.Round(g.Sum(x => x.TaxAmount), 2),
                            Total = Math.Round(g.Sum(x => x.Total), 2)
                        })
                        .OrderBy(x => x.GroupName)
                        .ThenBy(x => x.Taxper)
                        .ToList();
                }

                // ===============================
                // ✅ NORMAL TAX (FIXED)
                // ===============================
                else
                {
                    List<Taxdetails> taxList = new List<Taxdetails>();

                    foreach (var x in cart.Food)
                    {
                        var taxmodel = await _repository.GetExtraCharges(x.Id.ToString(), cart.Branch, cart.Outlet);

                        foreach (var item in taxmodel)
                        {
                            double txamt = 0;
                            double taxableamt = 0;

                            double itemTotal = x.Qty * x.Price;

                            // ✅ APPLY ONBILL DISCOUNT
                            if (cart.DiscountType.ToLower() == "onbill" && totalbillamt > 0 && x.itemDiscountAllowed != false)
                            {
                                double ratio = discountedBillAmt / totalbillamt;
                                itemTotal = itemTotal * ratio;
                            }

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

                            taxList.Add(new Taxdetails
                            {
                                TaxName = item.TaxDescription,
                                Taxper = item.TaxPercentage,
                                TaxAmount = txamt,
                                TaxableAmount = taxableamt
                            });
                        }
                    }

                    finalTaxList = taxList
                        .GroupBy(t => new { t.TaxName, t.Taxper })
                        .Select(g => new Taxdetails
                        {
                            GroupCode = 0,
                            GroupName = "ON Bill Tax",
                            TaxName = g.Key.TaxName,
                            Taxper = g.Key.Taxper,
                            TaxAmount = Math.Round(g.Sum(x => x.TaxAmount), 2),
                            TaxableAmount = Math.Round(g.Sum(x => x.TaxableAmount), 2)
                        })
                        .ToList();
                }

                // ===============================
                // ✅ SERVICE CHARGE
                // ===============================
                var tax = await _repository.GetTaxCharges(cart.Outlet, cart.Branch);

                if (tax.ServiceCharge > 0)
                {
                    serchargeper = tax.ServiceCharge;

                    var scamt = await _repository.LoadRule("Service Charge",
                        totalbillamt, cgsttaxamt, sgsttaxamt, posdate, cart.Branch);

                    sercharge = scamt * serchargeper / 100;
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
                return new TaxModel()
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
                    DiscountPer = cart.DiscountIn == "per" ? cart.Discount : 0,
                    DiscountIn = discountin,
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

        //public async Task<TaxModel> GetBill(CartModel cart)
        //{
        //    double totalbillamt = 0.0;
        //    int totalqty = 0;
        //    var posdate = _repository.GetPOSEntryDate();
        //    double discountamt = 0.0;
        //    string discountin = cart.DiscountIn;

        //    double cgsttaxper = 0.0;
        //    double sgsttaxper = 0.0;
        //    double cgsttaxamt = 0.0;
        //    double sgsttaxamt = 0.0;
        //    double serchargeper = 0.0;
        //    double sercharge = 0.0;

        //    try
        //    {
        //        // ===============================
        //        // ✅ TOTAL BILL + QTY
        //        // ===============================
        //        cart.Food.ForEach(x =>
        //        {
        //            totalbillamt += (x.Qty * x.Price);
        //            totalqty += x.Qty;
        //        });

        //        // ===============================
        //        // ✅ ON BILL DISCOUNT (BEFORE TAX)
        //        // ===============================
        //        double discountedBillAmt = totalbillamt;

        //        if (cart.Discount > 0 && cart.DiscountType.ToLower() == "onbill")
        //        {
        //            if (cart.DiscountIn == "per")
        //                discountamt = totalbillamt * cart.Discount / 100;
        //            else
        //                discountamt = cart.Discount;

        //            discountedBillAmt = totalbillamt - discountamt;
        //        }

        //        List<Taxdetails> finalTaxList = new List<Taxdetails>();

        //        // ===============================
        //        // ✅ GROUPED TAX CONDITION
        //        // ===============================

        //        if (cart.TaxType == "groupedtax")
        //        {
        //            var itemGroups = await _repository.GetItemGroupList(cart.Branch);

        //            var expandedItems = new List<TempTaxItem>();

        //            // ===============================
        //            // ✅ STEP 1: Expand items with tax %
        //            // ===============================
        //            foreach (var item in cart.Food)
        //            {
        //                var taxmodel = _repository.GetExtraCharges(item.Id.ToString(), cart.Branch, cart.Outlet);

        //                double cgstPer = 0;
        //                double sgstPer = 0;

        //                foreach (var t in taxmodel)
        //                {
        //                    if (t.ChargeName.Contains("CGST"))
        //                        cgstPer = t.TaxPercentage;

        //                    else if (t.ChargeName.Contains("SGST"))
        //                        sgstPer = t.TaxPercentage;
        //                }

        //                double itemTotal = item.Qty * item.Price;

        //                // ✅ APPLY ONBILL DISCOUNT PROPORTIONALLY
        //                if (cart.DiscountType.ToLower() == "onbill" && totalbillamt > 0)
        //                {
        //                    double ratio = discountedBillAmt / totalbillamt;
        //                    itemTotal = itemTotal * ratio;
        //                }

        //                expandedItems.Add(new TempTaxItem
        //                {
        //                    GrpCode = item.GrpCode,
        //                    ItemTotal = Math.Round(item.Qty * item.Price, 2),
        //                    CGSTPer = cgstPer,
        //                    SGSTPer = sgstPer
        //                });
        //            }

        //            // ===============================
        //            // ✅ STEP 2: Group by GrpCode + Tax %
        //            // ===============================
        //            var groupedItems = expandedItems
        //                .GroupBy(x => new { x.GrpCode, x.CGSTPer, x.SGSTPer });

        //            // ===============================
        //            // ✅ STEP 3: Build Tax List
        //            // ===============================
        //            foreach (var group in groupedItems)
        //            {
        //                double groupTotal = group.Sum(x => x.ItemTotal);

        //                double discountedGroupTotal = groupTotal;

        //                var groupInfo = itemGroups.FirstOrDefault(g => g.GrpCode == group.Key.GrpCode);

        //                // ===============================
        //                // ✅ GROUPWISE DISCOUNT (ONLY FOOD)
        //                // ===============================
        //                if (cart.Discount > 0 && cart.DiscountType.ToLower() == "groupwise")
        //                {
        //                    if (groupInfo?.GrpName?.ToLower() == "food")
        //                    {
        //                        double grpDiscount = 0;

        //                        if (cart.DiscountIn == "per")
        //                            grpDiscount = groupTotal * cart.Discount / 100;
        //                        else
        //                            grpDiscount = cart.Discount;

        //                        discountamt += grpDiscount;
        //                        discountedGroupTotal = groupTotal - grpDiscount;
        //                    }
        //                }

        //                double cgst = Math.Round(discountedGroupTotal * group.Key.CGSTPer / 100, 2);
        //                double sgst = Math.Round(discountedGroupTotal * group.Key.SGSTPer / 100, 2);

        //                //double cgst = Math.Round(groupTotal * group.Key.CGSTPer / 100, 2);
        //                //double sgst = Math.Round(groupTotal * group.Key.SGSTPer / 100, 2);

        //                // ✅ LIQUOR → NO TAX
        //                if (groupInfo?.GrpName == "LIQUOR")
        //                {
        //                    cgst = 0;
        //                    sgst = 0;
        //                }

        //                double totalTax = Math.Round(cgst + sgst, 2);

        //                string taxName = totalTax > 0
        //                    ? $"CGST {group.Key.CGSTPer}% + SGST {group.Key.SGSTPer}%"
        //                    : "NO TAX";

        //                finalTaxList.Add(new Taxdetails
        //                {
        //                    GroupCode = group.Key.GrpCode,
        //                    GroupName = groupInfo?.GrpName,
        //                    TaxName = taxName,
        //                    Taxper = group.Key.CGSTPer + group.Key.SGSTPer,
        //                    //TaxableAmount = Math.Round(groupTotal, 2),
        //                    TaxableAmount = Math.Round(discountedGroupTotal, 2),
        //                    CGST = cgst,
        //                    SGST = sgst,
        //                    TaxAmount = totalTax,
        //                    Total = Math.Round(discountedGroupTotal + totalTax, 2)
        //                    //Total = Math.Round(groupTotal + totalTax, 2)
        //                });

        //                // ✅ accumulate totals
        //                cgsttaxamt += cgst;
        //                sgsttaxamt += sgst;
        //            }

        //            // ===============================
        //            // ✅ IMPORTANT: DO NOT MERGE WRONG
        //            // ===============================
        //            finalTaxList = finalTaxList
        //                .GroupBy(x => new { x.GroupCode, x.TaxName }) // ✅ KEEP TAX SPLIT
        //                .Select(g => new Taxdetails
        //                {
        //                    GroupCode = g.Key.GroupCode,
        //                    GroupName = g.First().GroupName,
        //                    TaxName = g.Key.TaxName,
        //                    Taxper = g.First().Taxper,
        //                    TaxableAmount = g.Sum(x => x.TaxableAmount),
        //                    CGST = g.Sum(x => x.CGST),
        //                    SGST = g.Sum(x => x.SGST),
        //                    TaxAmount = g.Sum(x => x.TaxAmount),
        //                    Total = g.Sum(x => x.Total)
        //                })
        //                .OrderBy(x => x.GroupName)
        //                .ThenBy(x => x.Taxper)
        //                .ToList();
        //        }
        //        // ===============================
        //        // ✅ NORMAL TAX (YOUR ORIGINAL)
        //        // ===============================
        //        else
        //        {
        //            List<Taxdetails> taxList = new List<Taxdetails>();

        //            foreach (var x in cart.Food)
        //            {
        //                var taxmodel = _repository.GetExtraCharges(x.Id.ToString(), cart.Branch, cart.Outlet);

        //                foreach (var item in taxmodel)
        //                {
        //                    double txamt = 0;
        //                    double taxableamt = 0;

        //                    if (item.ChargeName.Contains("CGST"))
        //                    {
        //                        //var perc = taxmodel != null ? item.TaxPercentage : 0;
        //                        var perc = item.TaxPercentage;

        //                        taxableamt = Math.Round((x.Qty * x.Price), 2);

        //                        txamt = Math.Round(((x.Qty * x.Price) * perc) / 100, 2);

        //                        cgsttaxamt += txamt;

        //                        cgsttaxper = perc;
        //                    }
        //                    else if (item.ChargeName.Contains("SGST"))
        //                    {
        //                        //var perc = taxmodel != null ? item.TaxPercentage : 0;
        //                        var perc = item.TaxPercentage;

        //                        txamt = Math.Round(((x.Qty * x.Price) * perc) / 100, 2);

        //                        taxableamt = Math.Round((x.Qty * x.Price), 2);

        //                        sgsttaxamt += txamt;

        //                        sgsttaxper = perc;
        //                    }

        //                    taxList.Add(new Taxdetails
        //                    {
        //                        TaxName = item.ChargeName,
        //                        Taxper = item.TaxPercentage,
        //                        TaxAmount = txamt,
        //                        TaxableAmount = taxableamt
        //                    });
        //                }
        //            }

        //            finalTaxList = taxList
        //                .GroupBy(t => new { t.TaxName, t.Taxper })
        //                .Select(g => new Taxdetails
        //                {
        //                    TaxName = g.Key.TaxName,
        //                    Taxper = g.Key.Taxper,
        //                    TaxAmount = g.Sum(x => x.TaxAmount),
        //                    TaxableAmount = g.Sum(x => x.TaxableAmount)
        //                })
        //                .ToList();
        //        }

        //        // ===============================
        //        // ✅ SERVICE CHARGE
        //        // ===============================
        //        var tax = _repository.GetTaxCharges(cart.Outlet, cart.Branch);

        //        if (tax.ServiceCharge > 0)
        //        {
        //            serchargeper = tax.ServiceCharge;

        //            var scamt = _repository.LoadRule("Service Charge",
        //                totalbillamt, cgsttaxamt, sgsttaxamt, posdate);

        //            sercharge = scamt * serchargeper / 100;
        //        }

        //        // ===============================
        //        // ✅ DISCOUNT
        //        // ===============================
        //        //if (cart.Discount > 0)
        //        //{
        //        //    var totamt = _repository.LoadRule("Discount",
        //        //        totalbillamt, cgsttaxamt, sgsttaxamt, posdate);

        //        //    if (cart.DiscountType.ToLower() == "onbill")
        //        //    {
        //        //        //discountamt = totamt * cart.Discount / 100;
        //        //        discountamt = cart.DiscountIn == "per" ? totamt * cart.Discount / 100 : totamt - cart.Discount;
        //        //    }
        //        //    else if (cart.DiscountType.ToLower() == "groupwise")
        //        //    {
        //        //        discountamt = cart.Discount;
        //        //    }
        //        //}
        //        //else if (cart.DiscountType == "A")
        //        //{
        //        //    discountamt = cart.Discount;
        //        //}

        //        // ===============================
        //        // ✅ FINAL CALCULATION
        //        // ===============================
        //        var calc = dobillcalcwithServiceCharge( cart.Outlet, cart.Branch, totalbillamt, discountamt, cgsttaxamt, sgsttaxamt, sercharge);

        //        // ===============================
        //        // ✅ RETURN MODEL
        //        // ===============================
        //        var rr = new TaxModel()
        //        {
        //            CGSTPer = cgsttaxper,
        //            CGSTAmt = Math.Round(cgsttaxamt, 2),
        //            SGSTAmt = Math.Round(sgsttaxamt, 2),
        //            SGSTPer = sgsttaxper,
        //            ServiceChargePer = serchargeper,
        //            ServiceCharge = Math.Round(sercharge, 2),
        //            TotalAmount = totalbillamt,
        //            TotalQty = totalqty,
        //            Discount = Math.Round(discountamt, 2),
        //            DiscountPer = 0,
        //            DiscountIn = discountin,
        //            DiscountRemarks = cart.DiscountRemarks == "" ? string.Empty : cart.DiscountRemarks,
        //            GrandTotal = calc.Total,
        //            RoundOff = Math.Round(calc.RoundOff, 2),
        //            TaxList = finalTaxList
        //        };

        //        return rr;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<TaxModel> GetBill(CartModel cart)
        //{
        //    double totalbillamt = 0;
        //    int totalqty = 0;
        //    double totalCGST = 0;
        //    double totalSGST = 0;

        //    List<Taxdetails> taxList = new List<Taxdetails>();

        //    try
        //    {
        //        // Calculate total bill and quantity
        //        foreach (var item in cart.Food)
        //        {
        //            totalbillamt += item.Qty * item.Price;
        //            totalqty += item.Qty;
        //        }

        //        if (cart.TaxType == "groupedtax")
        //        {
        //            var itemGroups = await _repository.GetItemGroupList(cart.Branch);
        //            var groupedItems = cart.Food.GroupBy(x => x.GrpCode);

        //            foreach (var group in groupedItems)
        //            {
        //                double subTotal = group.Sum(x => x.Qty * x.Price);
        //                var groupInfo = itemGroups.FirstOrDefault(g => g.GrpCode == group.Key);
        //                string groupName = groupInfo?.GrpName ?? "Unknown";

        //                double cgst = 0;
        //                double sgst = 0;

        //                var taxmodel = _repository.GetExtraCharges(group.Key.id.ToString(), cart.Branch, cart.Outlet);

        //                foreach (var tax in taxmodel)
        //                {
        //                    double taxAmt = Math.Round((subTotal * tax.TaxPercentage) / 100, 2);
        //                    if (tax.ChargeName.Contains("CGST"))
        //                        cgst += taxAmt;
        //                    else if (tax.ChargeName.Contains("SGST"))
        //                        sgst += taxAmt;
        //                }

        //                totalCGST += cgst;
        //                totalSGST += sgst;

        //                taxList.Add(new Taxdetails
        //                {
        //                    GroupCode = group.Key,
        //                    GroupName = groupName,
        //                    TaxName = $"CGST & SGST {taxmodel.FirstOrDefault()?.TaxPercentage}%",
        //                    TaxableAmount = subTotal,
        //                    CGST = cgst,
        //                    SGST = sgst,
        //                    TaxAmount = cgst + sgst,
        //                    Total = subTotal + cgst + sgst
        //                });
        //            }
        //        }
        //        else // NORMAL TAX
        //        {
        //            foreach (var item in cart.Food)
        //            {
        //                double itemTotal = item.Qty * item.Price;
        //                var taxmodel = _repository.GetExtraCharges(item.Id.ToString(), cart.Branch, cart.Outlet);

        //                double cgst = 0;
        //                double sgst = 0;

        //                foreach (var tax in taxmodel)
        //                {
        //                    double taxAmt = Math.Round((itemTotal * tax.TaxPercentage) / 100, 2);
        //                    if (tax.ChargeName.Contains("CGST"))
        //                        cgst += taxAmt;
        //                    else if (tax.ChargeName.Contains("SGST"))
        //                        sgst += taxAmt;
        //                }

        //                totalCGST += cgst;
        //                totalSGST += sgst;

        //                taxList.Add(new Taxdetails
        //                {
        //                    GroupCode = item.GrpCode,
        //                    GroupName = item.Food,
        //                    TaxName = $"CGST & SGST {taxmodel.FirstOrDefault()?.TaxPercentage}%",
        //                    TaxableAmount = itemTotal,
        //                    CGST = cgst,
        //                    SGST = sgst,
        //                    TaxAmount = cgst + sgst,
        //                    Total = itemTotal + cgst + sgst
        //                });
        //            }
        //        }

        //        return new TaxModel
        //        {
        //            TotalAmount = totalbillamt,
        //            TotalQty = totalqty,
        //            CGSTAmt = Math.Round(totalCGST, 2),
        //            SGSTAmt = Math.Round(totalSGST, 2),
        //            GrandTotal = Math.Round(totalbillamt + totalCGST + totalSGST, 2),
        //            Discount = cart.Discount,
        //            DiscountPer = cart.DiscountType == "P" ? cart.Discount : 0,
        //            DiscountRemarks = cart.DiscountRemarks,
        //            TaxList = taxList
        //        };
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

        //public async Task<TaxModel> GetBill(CartModel cart)
        //{
        //    double totalbillamt = 0;
        //    int totalqty = 0;

        //    double cgsttaxamt = 0;
        //    double sgsttaxamt = 0;

        //    List<Taxdetails> taxList = new List<Taxdetails>();

        //    try
        //    {
        //        // 1️⃣ TOTAL BILL & QTY
        //        foreach (var item in cart.Food)
        //        {
        //            totalbillamt += item.Qty * item.Price;
        //            totalqty += item.Qty;
        //        }

        //        if (cart.TaxType == "groupedtax")
        //        {
        //            var itemGroups = await _repository.GetItemGroupList(cart.Branch);

        //            var groupedItems = cart.Food.GroupBy(x => x.GrpCode);

        //            foreach (var group in groupedItems)
        //            {
        //                double subTotal = group.Sum(x => x.Qty * x.Price);

        //                var groupInfo = itemGroups.FirstOrDefault(g => g.GrpCode == group.Key);
        //                string groupName = groupInfo?.GrpName ?? "Unknown";

        //                double cgst = 0;
        //                double sgst = 0;

        //                // TAX FOR THE GROUP
        //                var taxmodel = _repository.GetExtraCharges(group.Key.ToString(), cart.Branch, cart.Outlet);

        //                foreach (var tax in taxmodel)
        //                {
        //                    double taxAmt = Math.Round((subTotal * tax.TaxPercentage) / 100, 2);

        //                    if (tax.ChargeName.Contains("CGST"))
        //                    {
        //                        cgst += taxAmt;
        //                        cgsttaxamt += taxAmt;
        //                    }
        //                    else if (tax.ChargeName.Contains("SGST"))
        //                    {
        //                        sgst += taxAmt;
        //                        sgsttaxamt += taxAmt;
        //                    }
        //                }

        //                double totalTax = cgst + sgst;
        //                double groupTotal = subTotal + totalTax;

        //                taxList.Add(new Taxdetails
        //                {
        //                    GroupCode = group.Key,
        //                    GroupName = groupName,
        //                    TaxableAmount = subTotal,
        //                    CGST = cgst,
        //                    SGST = sgst,
        //                    TaxAmount = totalTax,
        //                    Total = groupTotal
        //                });
        //            }
        //        }
        //        else // NORMAL TAX
        //        {
        //            foreach (var item in cart.Food)
        //            {
        //                double itemTotal = item.Qty * item.Price;

        //                var taxmodel = _repository.GetExtraCharges(item.Id.ToString(), cart.Branch, cart.Outlet);

        //                double cgst = 0;
        //                double sgst = 0;

        //                foreach (var tax in taxmodel)
        //                {
        //                    double taxAmt = Math.Round((itemTotal * tax.TaxPercentage) / 100, 2);

        //                    if (tax.ChargeName.Contains("CGST"))
        //                    {
        //                        cgst += taxAmt;
        //                        cgsttaxamt += taxAmt;
        //                    }
        //                    else if (tax.ChargeName.Contains("SGST"))
        //                    {
        //                        sgst += taxAmt;
        //                        sgsttaxamt += taxAmt;
        //                    }
        //                }

        //                double totalTax = cgst + sgst;
        //                double itemGrandTotal = itemTotal + totalTax;

        //                taxList.Add(new Taxdetails
        //                {
        //                    GroupCode = item.GrpCode,
        //                    GroupName = item.Food,  // normal mode: item name
        //                    TaxableAmount = itemTotal,
        //                    CGST = cgst,
        //                    SGST = sgst,
        //                    TaxAmount = totalTax,
        //                    Total = itemGrandTotal
        //                });
        //            }
        //        }

        //        // 3️⃣ RETURN MODEL
        //        return new TaxModel
        //        {
        //            TotalAmount = totalbillamt,
        //            TotalQty = totalqty,
        //            CGSTAmt = Math.Round(cgsttaxamt, 2),
        //            SGSTAmt = Math.Round(sgsttaxamt, 2),
        //            GrandTotal = Math.Round(totalbillamt + cgsttaxamt + sgsttaxamt, 2),
        //            Discount = cart.Discount,
        //            DiscountPer = cart.DiscountType == "P" ? cart.Discount : 0,
        //            DiscountRemarks = cart.DiscountRemarks,
        //            TaxList = taxList
        //        };
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}


        //    public async Task<TaxModel> GetBill(CartModel cart)
        //    {
        //        double totalbillamt = 0.0;
        //        int totalqty = 0;
        //        var posdate = _repository.GetPOSEntryDate();
        //        double discountamt = cart.Discount;

        //        double cgsttaxper = 0.0;
        //        double sgsttaxper = 0.0;
        //        double cgsttaxamt = 0.0;
        //        double sgsttaxamt = 0.0;
        //        double serchargeper = 0.0;
        //        double sercharge = 0.0;
        //        try
        //        {
        //            cart.Food.ForEach(x =>
        //            {
        //                totalbillamt += (x.Qty * x.Price);
        //                totalqty += x.Qty;
        //            });

        //            List<Taxdetails> taxList = new List<Taxdetails>();

        //            cart.Food.ForEach(x =>
        //            {
        //                var taxmodel = _repository.GetExtraCharges(x.Id.ToString(), cart.Branch, cart.Outlet);

        //                foreach (var item in taxmodel)
        //                {
        //                    double txamt = 0;
        //                    double taxableamt = 0;
        //                    if (item.ChargeName.Contains("CGST"))
        //                    {
        //                        var perc = taxmodel != null ? item.TaxPercentage : 0;

        //                        taxableamt = Math.Round((x.Qty * x.Price), 2);

        //                        txamt = Math.Round(((x.Qty * x.Price) * perc) / 100, 2);

        //                        cgsttaxamt += Math.Round(((x.Qty * x.Price) * perc) / 100, 2);

        //                        cgsttaxper = perc;
        //                        var ddd = ((x.Qty * x.Price) * perc) / 100;
        //                        decimal ff = Convert.ToDecimal(Math.Round(ddd, 2));
        //                    }
        //                    else if (item.ChargeName.Contains("SGST"))
        //                    {
        //                        var perc = taxmodel != null ? item.TaxPercentage : 0;
        //                        sgsttaxamt += Math.Round(((x.Qty * x.Price) * perc) / 100, 2);
        //                        txamt = Math.Round(((x.Qty * x.Price) * perc) / 100, 2);
        //                        taxableamt = Math.Round((x.Qty * x.Price), 2);
        //                        sgsttaxper = perc;
        //                    }

        //                    Taxdetails aa = new Taxdetails();
        //                    aa.TaxName = item.ChargeName;
        //                    aa.Taxper = item.TaxPercentage;
        //                    aa.TaxAmount = txamt;
        //                    aa.TaxableAmount = taxableamt;
        //                    taxList.Add(aa);

        //                }
        //            });


        //            var mergedTaxList = taxList
        //.GroupBy(t => new { t.TaxName, t.Taxper })
        //.Select(g => new Taxdetails
        //{
        //    TaxName = g.Key.TaxName,
        //    Taxper = g.Key.Taxper,
        //    TaxAmount = g.Sum(x => x.TaxAmount),
        //    TaxableAmount = g.Sum(x => x.TaxableAmount)
        //})
        //.ToList();

        //            var tax = _repository.GetTaxCharges(cart.Outlet, cart.Branch);

        //            if (tax.ServiceCharge > 0)
        //            {
        //                serchargeper = tax.ServiceCharge;
        //                var scamt = _repository.LoadRule("Service Charge", totalbillamt, cgsttaxamt, sgsttaxamt, posdate);
        //                sercharge = scamt * serchargeper / 100;
        //            }

        //            if (cart.Discount > 0)
        //            {
        //                var totamt = _repository.LoadRule("Discount", totalbillamt, cgsttaxamt, sgsttaxamt, posdate);

        //                if (cart.DiscountType == "P")
        //                    discountamt = totamt * cart.Discount / 100;
        //                else if (cart.DiscountType == "A")
        //                    discountamt = cart.Discount;
        //            }
        //            else if (cart.DiscountType == "A")
        //            {
        //                discountamt = cart.Discount;
        //            }

        //            var calc = dobillcalcwithServiceCharge(cart.Outlet, cart.Branch, totalbillamt, discountamt, cgsttaxamt, sgsttaxamt, sercharge);
        //            var rr = new TaxModel()
        //            {
        //                CGSTPer = cgsttaxper,
        //                CGSTAmt = Math.Round(cgsttaxamt, 2),
        //                SGSTAmt = Math.Round(sgsttaxamt, 2),
        //                SGSTPer = sgsttaxper,
        //                //KKCessPer = kkcessper,
        //                //KKCess = Math.Round(kkcess, 2),
        //                //SBCessPer = sbcessper,
        //                //SBCess = Math.Round(sbcess, 2),
        //                ServiceChargePer = serchargeper,
        //                ServiceCharge = Math.Round(sercharge, 2),
        //                //TaxCodePer = sertaxper,
        //                //TaxCode = Math.Round(sertax, 2),
        //                TotalAmount = totalbillamt,
        //                TotalQty = totalqty,
        //                //TaxPer = taxper,
        //                //Tax = Math.Round(taxamount, 2),
        //                Discount = Math.Round(discountamt, 2),
        //                DiscountRemarks = cart.DiscountRemarks == "" ? string.Empty : cart.DiscountRemarks,
        //                GrandTotal = calc.Total,
        //                RoundOff = Math.Round(calc.RoundOff, 2),
        //                TaxList = mergedTaxList
        //            };
        //            return rr;

        //        }
        //        catch
        //        {
        //            throw;
        //        }
        //    }

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

        public async Task<BillResponse> PostBill(BillModel bill)
        {
            try
            {
                string billno = string.Empty;
                (DateTime billDate, string billTime) billdatetime = (DateTime.MinValue, string.Empty);

                DateTime POSEntryDate = await _repository.GetPOSEntryDate(bill.Cart.Branch);

                var financialList = await _posdal.GetFinancialMasters(bill.Cart.Branch);

                var DefaultPrinter = await _repository.GetDefaultPrinter(bill.Cart.Branch, bill.Cart.Outlet);
                var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch

                BillModelForBill billdetails = new BillModelForBill();

                billdetails.Cart = bill.Cart;
                billdetails.Tax = bill.Tax;
                billdetails.SubBillingType = bill.SubBillingType;
                billdetails.BillingType = bill.BillingType;
                //billdetails.paymentresponse = bill.paymentresponse;

                BillHeaderdetilsforBill billheaddetail = new BillHeaderdetilsforBill();

                var Billrequired = await _posdal.IsDirectBillSettlementOnline(bill.Cart.Outlet, bill.Cart.Branch);

                if (bill.Cart.NCCode != 0)
                {
                    var nckotbsmodel = await NCKotBillSettlement(bill, POSEntryDate, financialList.FinCode);
                    billno = nckotbsmodel.BillNo;
                    if (nckotbsmodel.Status)
                    {
                        string username = await _posdal.GetUserName(bill.Cart.UserCode, bill.Cart.Branch);

                        var taxes = (await GetIndiTax(bill.Cart)).Select(x => new NCSalesTax { Bill_No = billno, ItemCode = x.ItemCode, TaxCode = x.TaxCode, TaxAmount = Math.Round(x.TaxAmount, 2), Branch_Code = bill.Cart.Branch, OltCode = bill.Cart.Outlet, BillDate = POSEntryDate, Ref = "F", FinCode = financialList.FinCode }).ToList();

                        await _repository.InsertNCSalesTaxBulk(taxes);

                        billdatetime = await _repository.GetBillDateTime("NC", nckotbsmodel.BillNo, bill.Cart.Branch, bill.Cart.Outlet);

                        bool savetempdata = await _posdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, "NC", billno, bill.Cart.Table, username, nckotbsmodel.BillId, bill.Cart.Branch);
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
                                TaxType = bill.Cart.TaxType
                            };
                        }).ToList();

                        await _repository.InsertSalesTaxBulk(taxes);

                        await _repository.InsertSalesGroupTaxBulk(Convert.ToInt32(billno), bill.Cart.Branch, bill.Cart.Outlet, bill.Tax, POSEntryDate);

                        string username = await _posdal.GetUserName(bill.Cart.UserCode, bill.Cart.Branch);

                        billdatetime = await _repository.GetBillDateTime("K", kotbsmodel.BillNo, bill.Cart.Branch, bill.Cart.Outlet);

                        //bool savetempdata = _posdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, billno, bill.Cart.Table, username, kotbsmodel.BillId, bill.paymentresponse.data.merchantId);
                        bool savetempdata = await _posdal.InsertTmpBillPrint(bill.Cart.Waiter, bill.Cart.Outlet, POSEntryDate, "N", billno, bill.Cart.Table, username, kotbsmodel.BillId, bill.Cart.Branch);

                        string checkFastFoodService = await _repository.GetInfo("OutletMaster", "OltIsFastFood", "OltCode", Convert.ToString(bill.Cart.Outlet), 0, bill.Cart.Branch);

                        string FastFoodTokenNo = "0";

                        if (checkFastFoodService == "True")
                        {
                            var kotFastfoodTToken = await _posdal.GetKOTSettlementMaster(kotbsmodel.BillId, kotbsmodel.BillNo, bill.Cart.Outlet, bill.Cart.Branch, "F");

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
                    }
                    else
                    {
                        return new BillResponse
                        {
                            IsSuccess = true,
                            BillNo = billno,
                            IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty,
                            BillDate = DateOnly.FromDateTime(billdatetime.billDate),
                            BillTime = billdatetime.billTime
                        };
                    }
                }
                return new BillResponse
                {
                    IsSuccess = true,
                    BillNo = billno,
                    IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty,
                    BillDate = DateOnly.FromDateTime(billdatetime.billDate),
                    BillTime = billdatetime.billTime
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<KOTSettlementStatusModel> KotBillSettlement(BillModel bill, DateTime POSEntryDate, string fincode)
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
            //var UnsettledKOT = await _kotbillsettlementdal.GetUnsettledKOTs(bill.Cart.Table, bill.Cart.SubTable);
            var ECart = new CartModel();
            var EFoodList = new List<FoodModel>();
            try
            {
                cart.Food.ForEach(x =>
                {
                    var EFood = new FoodModel();
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
                string checkFastFoodService = await _repository.GetInfo("OutletMaster", "OltIsFastFood", "OltCode", oltcode, 0, cart.Branch);

                if (checkroomservice == "True")
                {
                    var ds = await _kotbillsettlementdal.GetFoodBill(POSEntryDate, ksm.billno, cart.CheckInNo, cart.Outlet);

                    if (ds.Count == 0)
                    {
                        string outname = await _kotbillsettlementdal.GetOutletName(ECart.Outlet, cart.Branch);
                        decimal Taxv = Convert.ToDecimal(taxmodel.CGSTPer + taxmodel.SGSTPer);
                        decimal ExculTax = Convert.ToDecimal(taxmodel.TotalAmount);

                        var foodBill = new FoodBill
                        {
                            RNo = await _posdal.findhotelnextnumber("Tbl_FoodBills", "RNo"),
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

        public async Task<KOTSettlementStatusModel> NCKotBillSettlement(BillModel bill, DateTime POSEntryDate, string fincode)
        {
            var kotnos = "";
            var taxmodel = bill.Tax;
            var cart = bill.Cart;
            var billingtype = bill.BillingType;
            var subbillingtype = bill.SubBillingType;
            var reason = cart.NCRemarks;
            var ECart = new CartModel();
            var EFoodList = new List<FoodModel>();
            try
            {
                cart.Food.ForEach(x =>
                {
                    var EFood = new FoodModel();
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
                        await _nckotbillsettlementdal.DeleteFoodBill(ksm.billno, POSEntryDate);
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

        //public async Task<List<IndiTaxModel>> GetIndiTax(CartModel cart)
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

        public async Task<List<IndiTaxModel>> GetIndiTax(CartModel cart)
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


        //public async Task<CartModel> GetOldCart(string tableNo, string outlet, string subtable)
        //{
        //    var food = await _repository.GetOldCartFoodAsync(tableNo, outlet, subtable);
        //    var waiter = await _repository.GetOldCartWaiterAsync(tableNo, outlet, subtable);

        //    if (food.Count == 0 && waiter == null)
        //        return null;

        //    return new CartModel
        //    {
        //        Food = food,
        //        Pax = waiter?.Pax ?? 0,
        //        Waiter = waiter.StwCode,
        //        WaiterName = waiter?.StwName,
        //        NCCode = waiter.NCCode,
        //        NCRemarks = waiter?.NCRemarks,
        //        KotMobileNo = waiter?.KotMobileNo,
        //        GuestName = waiter?.KOTGuestName
        //    };
        //}

        //public async Task<CartModel> GetOldCart(string tableNo, string outlet, string subtable)
        //{
        //    var food = await _repository.GetOldCartFoodAsync(tableNo, outlet, subtable);
        //    var waiter = await _repository.GetOldCartWaiterAsync(tableNo, outlet, subtable);

        //    if (food.Count == 0 && waiter == null)
        //        return null;

        //    var groupedFood = food
        //        .GroupBy(x => x.KOTId)
        //        .ToDictionary(g => g.Key, g => g.ToList());

        //    return new CartModel
        //    {
        //        Food = food,
        //        FoodByKot = groupedFood,
        //        Pax = waiter?.Pax ?? 0,
        //        Waiter = waiter?.StwCode ?? 0,
        //        WaiterName = waiter?.StwName,
        //        NCCode = waiter?.NCCode ?? 0,
        //        NCRemarks = waiter?.NCRemarks,
        //        KotMobileNo = waiter?.KotMobileNo,
        //        GuestName = waiter?.KOTGuestName
        //    };
        //}


        //public async Task<CartModel> GetOldCart(string tableNo, string outlet, string subtable)
        //{
        //    var food = await _repository.GetOldCartFoodAsync(tableNo, outlet, subtable);
        //    var waiter = await _repository.GetOldCartWaiterAsync(tableNo, outlet, subtable);

        //    if (food.Count == 0 && waiter == null)
        //        return null;

        //    var groupedFood = food
        //        .GroupBy(x => x.KOTId)
        //        .Select(g => new KotGroupModel
        //        {
        //            KotId = g.Key,
        //            Items = g.ToList()
        //        })
        //        .ToList();

        //    return new CartModel
        //    {
        //        Food = groupedFood,
        //        Pax = waiter?.Pax ?? 0,
        //        Waiter = waiter?.StwCode ?? 0,
        //        WaiterName = waiter?.StwName,
        //        NCCode = waiter?.NCCode ?? 0,
        //        NCRemarks = waiter?.NCRemarks,
        //        KotMobileNo = waiter?.KotMobileNo,
        //        GuestName = waiter?.KOTGuestName
        //    };
        //}

        public async Task<List<CartResponse>> GetOldCart(string tableNo, string outlet, char subtable, string branchcode)
        {
            List<CartResponse> result = new List<CartResponse>();

            var food = await _repository.GetOldCartFoodAsync(tableNo, outlet, subtable, branchcode);
            var waiters = await _repository.GetOldCartWaiterAsync(tableNo, outlet, subtable, branchcode);

            if (food.Count == 0 && waiters == null)
                return null;

            result = food.GroupBy(f => f.KOTId)
                .Select(g =>
                {
                    var waiter = waiters.FirstOrDefault(w => w.Pax == g.First().KOTSeatsServed);
                    return new CartResponse
                    {
                        KotId = g.Key,
                        Waiter = waiter?.StwCode ?? 0,
                        WaiterName = waiter?.StwName,
                        Pax = waiter?.Pax ?? 0,
                        NCCode = waiter?.NCCode ?? 0,
                        NCRemarks = waiter?.NCRemarks,
                        Food = g.ToList()
                    };
                }).ToList();
            return result;
        }

        public async Task<List<SubTableStatusModel>> GetSubTables(string outlet, string tableno, string branchcode)
        {
            var Subtables = await _repository.GetSubTables(outlet, tableno, branchcode);
            return Subtables;
        }

        public async Task<TaxSettingMaster> GetTaxSettings(string branchcode)
        {
            var timerList = await _repository.GetTaxSettings(branchcode);
            return timerList;
        }

        public async Task<IEnumerable<DiscountModeMaster>> GetDiscountModeMaster(string branchcode)
        {
            var data = await _repository.GetDiscountModeMaster(branchcode);
            return data;
        }

        public async Task<List<NCModel>> GetNCKOT(string branchcode)
        {
            var Subtables = await _repository.GetNCKOT(branchcode);
            return Subtables;
        }

        public async Task<List<Printerdetaildto>> GetPrinterDetails(int oltcode, string branchcode)
        {
            var CatGrpDetails = await _repository.GetDefaultCatGrpDetails(branchcode, oltcode);
            var UtilityPrinter = await _repository.GetUtilityPrinter(branchcode, oltcode);

            if (CatGrpDetails.Count == 0 || UtilityPrinter.Count == 0)
                return new List<Printerdetaildto>();

            var result = new List<Printerdetaildto>();

            foreach (var grp in CatGrpDetails)
            {
                var catIds = grp.CatGrp
                                .Split(',')
                                .Select(x => Convert.ToInt32(x.Trim()))
                                .ToList();

                var printers = UtilityPrinter
                                .Where(p => p.GrpCode == grp.Grp)
                                .ToList();

                foreach (var printer in printers)
                {
                    result.Add(new Printerdetaildto
                    {
                        Branch_Code = printer.Branch_Code,
                        OltCode = printer.OltCode,
                        PrinterName = printer.PrinterName,
                        BillType = printer.BillType,
                        PrintType = printer.PrintType,
                        GrpCode = printer.GrpCode,
                        CategoryIds = catIds,
                        IPAddress = printer?.IPAddress ?? string.Empty,
                    });
                }
            }

            return result;
        }

        public async Task<FastFoodDetails> GetFastfoodDetails(int outlet, string branchcode)
        {
            var FastfoodDetails = await _repository.GetFastfoodDetails(outlet, branchcode);

            return FastfoodDetails;
        }

        public async Task<POSServiceResult<int>> SettleBill(SettlementBillModel settlement, string BillType)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            DateTime POSEntryDate = await _repository.GetPOSEntryDate(settlement.BranchCode);
            DateTime istTime = ConvertUtcToIst();

            string checksql = @"select CBSId from CompanyBillSettlement Where BillNo = @Billno AND Branch_Code = @branch";

            int cbsId = await con.QueryFirstOrDefaultAsync<int>(checksql, new { Billno = settlement.BillNo, branch = settlement.BranchCode });

            if (cbsId > 0)
            {
                return new POSServiceResult<int>
                {
                    Success = false,
                    Message = "Already Partial Payment Paid Made in This particular Bill. Can not be Modify settlement.",
                    Data = 0
                };
            }

            //string samebillduplicatesql = @"select KSMId from KOTBillSettlement Where KSMBillNo = @Billno And OltCode = @oltcode And CAST(KBSSetteleDate AS DATE) = @billdate AND Branch_Code = @branch";

            //int SameduplicateId = await con.QueryFirstOrDefaultAsync<int>(samebillduplicatesql, new { Billno = settlement.BillNo, oltcode = settlement.OltCode , billdate = POSEntryDate.Date, branch = settlement.BranchCode });

            //if (SameduplicateId > 0)
            //{
            //    return new POSServiceResult<int>
            //    {
            //        Success = false,
            //        Message = "Already Bill Payment Settled.",
            //        Data = 0
            //    };
            //}

            string billduplicatesql = @"select KSMId from KOTBillSettlement Where KSMBillNo = @Billno And CAST(KBSSetteleDate AS DATE) = @billdate AND Branch_Code = @branch";

            int duplicateId = await con.QueryFirstOrDefaultAsync<int>(billduplicatesql, new { Billno = settlement.BillNo, billdate = POSEntryDate.Date, branch = settlement.BranchCode });

            if (duplicateId > 0)
            {
                return new POSServiceResult<int>
                {
                    Success = false,
                    Message = "Already Bill Payment Exists. Please Contact Admin.",
                    Data = 0
                };
            }

            await _repository.DeleteKotBillSettlementAsync(Convert.ToString(settlement.BillNo), settlement.OltCode, settlement.BranchCode, settlement.BillDate);

            var result = await _repository.SettleBill(settlement, POSEntryDate, istTime, BillType);
            if (result > 0)
            {
                return new POSServiceResult<int>
                {
                    Success = true,
                    Message = "Bill settled successfully.",
                    Data = 1
                };
            }
            else
            {
                return new POSServiceResult<int>
                {
                    Success = false,
                    Message = "Failed while settling the bill.",
                    Data = -1
                };
            }
        }

        public async Task<IEnumerable<CompanyMaster>> GetCompanyMaster(string branchcode)
        {
            var Companymaster = await _repository.GetCompanyMaster(branchcode);
            return Companymaster;
        }

        public async Task<IEnumerable<PaymentModeGrouped>> GetPaymentModeMaster(string branchcode)
        {
            var data = await _repository.GetPaymentModeMaster(branchcode);

            var grouped = data
                .GroupBy(x => new { x.ModeId, x.ModeType, x.ModeRequired, x.BranchCode })
                .Select(g => new PaymentModeGrouped
                {
                    ModeId = g.Key.ModeId,
                    ModeType = g.Key.ModeType,
                    ModeRequired = g.Key.ModeRequired,
                    BranchCode = g.Key.BranchCode,

                    SubModes = g
                        .Where(x => x.SubModeId != null && x.SubModeType != null)
                        .Select(x => new PaymentSubMode
                        {
                            SubModeId = x.SubModeId.Value,
                            SubModeType = x.SubModeType
                        })
                        .ToList()
                })
                .ToList();

            return grouped;
        }

        public async Task<IEnumerable<UnSettlementBillModel>> GetUnbillDetails(int billno, string tblno, string outlet, string branchcode)
        {
            var data = await _repository.GetUnbillDetails(billno, tblno, outlet, branchcode);
            return data;
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

        public async Task<IEnumerable<PosUserRightsAccess>> GetPosUserAccessRight(int usercode, string userName, string branchcode)
        {
            var userRights = await _repository.GetPosUserAccessRight(usercode, userName, branchcode);
            return userRights;
        }

        public async Task<IEnumerable<KOTTransferTypeMaster>> GetKotTransferTypeMaster(string branchcode)
        {
            var data = await _repository.GetKotTransferTypeMaster(branchcode);
            return data;
        }

        public async Task<bool> KOTTransferTable(KOTTransferRequest request)
        {
            var financialList = await _posdal.GetFinancialMasters(request.Branch);

            var transfertable = await _repository.KOTTransferTable(request, financialList.FinCode);
            return transfertable;
        }

        public async Task<bool> KOT2NCKOT(KOT2NCKOTRequest request)
        {
            var kottonckotdetails = await _repository.KOT2NCKOT(request);
            return kottonckotdetails;
        }

        public async Task<IEnumerable<KOTBillSettlementModel>> GetFilteredBillDetails(KOTBillSettlementFilter filter)
        {
            var billdetails = await _repository.GetFilteredBillDetails(filter);
            return billdetails;
        }


        //public async Task<IEnumerable<BillReprintResponse>> GetReprintBill(int billno, string oltcode, string branchcode)
        //{
        //    var kotmasterlist = await _repository.KOTMasters(oltcode, branchcode);
        //    var kotdetailslist = await _repository.KOTDetails(oltcode, branchcode);
        //    var settlementmasterlist = await _repository.SettlementMasters(oltcode, branchcode);
        //    var settlementdetailslist = await _repository.SettlementDetails(oltcode, branchcode);
        //    var billsettlementlist = await _repository.KOTBillSettlement(oltcode, branchcode);
        //    var salesTaxlist = await _repository.SalesTax(oltcode, branchcode);
        //    var itemdiscountlist = await _repository.ItemDiscount(oltcode, branchcode);
        //    var itemGroups = await _repository.GetItemGroupList(branchcode);
        //    var ItemMasterList = await _repository.GetItemMasterList(branchcode);

        //    // Step 1: Filter settlement master
        //    var settlementMasters = settlementmasterlist
        //        .Where(w => w.KSMBillNo == billno) // assuming string
        //        .ToList();

        //    var billsettlement = billsettlementlist
        //        .Where(w => w.KSMBillNo == billno) // assuming string
        //        .ToList();

        //    // Step 2: Get related settlement details
        //    var billIds = billsettlement.Select(s => s.KBSId).ToList();

        //    var settlementIds = settlementMasters.Select(s => s.KSMId).ToList();

        //    var relatedSettlementDetails = settlementdetailslist
        //        .Where(sd => settlementIds.Contains(sd.KSMId))
        //        .ToList();

        //    // Step 3: Get KOTIds
        //    var kotIds = relatedSettlementDetails.Select(sd => sd.KOTId).Distinct().ToList();

        //    // Step 4: Filter KOT Master & Details
        //    var kotMasters = kotmasterlist
        //        .Where(k => kotIds.Contains(k.KOTId))
        //        .ToList();

        //    var kotDetails = kotdetailslist
        //        .Where(k => kotIds.Contains(k.KOTId))
        //        .ToList();

        //    var Itemcodes = kotDetails.Select(s => s.ItemCode).ToList();

        //    var TaxDetails = salesTaxlist
        //       .Where(sd => settlementIds.Contains(Convert.ToInt32(sd.Bill_No)) &&  Itemcodes.Contains(sd.ItemCode))
        //       .ToList();

        //    var DiscountDetails = itemdiscountlist
        //       .Where(sd => billIds.Contains(Convert.ToInt32(sd.BillNo)))
        //       .ToList();


        //    // TODO: Map into KOTBillSettlementModel
        //    return new List<BillReprintResponse>();

        //}

        //public async Task<IEnumerable<BillReprintResponse>> GetReprintBill(int billno, string oltcode, string branchcode)
        //{
        //    // 🔹 Load Data
        //    var kotmasterlist = await _repository.KOTMasters(oltcode, branchcode);
        //    var kotdetailslist = await _repository.KOTDetails(oltcode, branchcode);
        //    var settlementmasterlist = await _repository.SettlementMasters(oltcode, branchcode);
        //    var settlementdetailslist = await _repository.SettlementDetails(oltcode, branchcode);
        //    var billsettlementlist = await _repository.KOTBillSettlement(oltcode, branchcode);
        //    var salesTaxlist = await _repository.SalesTax(oltcode, branchcode);
        //    var itemdiscountlist = await _repository.ItemDiscount(oltcode, branchcode);
        //    var itemGroups = await _repository.GetItemGroupList(branchcode);
        //    var itemMasterList = await _repository.GetItemMasterList(branchcode);

        //    // 🔹 Step 1: Filter Settlement Master
        //    var settlementMasters = settlementmasterlist
        //        .Where(w => w.KSMBillNo == billno.ToString())
        //        .ToList();

        //    var settlementIds = settlementMasters.Select(s => s.KSMId).ToList();

        //    // 🔹 Step 2: Settlement Details
        //    var relatedSettlementDetails = settlementdetailslist
        //        .Where(sd => settlementIds.Contains(sd.KSMId))
        //        .ToList();

        //    var kotIds = relatedSettlementDetails
        //        .Select(sd => sd.KOTId)
        //        .Distinct()
        //        .ToList();

        //    // 🔹 Step 3: KOT Master & Details
        //    var kotMasters = kotmasterlist
        //        .Where(k => kotIds.Contains(k.KOTId))
        //        .ToList();

        //    var kotDetails = kotdetailslist
        //        .Where(k => kotIds.Contains(k.KOTId))
        //        .ToList();

        //    // 🔹 Step 4: Bill Settlement
        //    var billSettlement = billsettlementlist
        //        .Where(w => w.KSMBillNo == billno.ToString())
        //        .ToList();

        //    var billIds = billSettlement.Select(b => b.KBSId).ToList();

        //    // 🔹 Step 5: Tax
        //    var itemCodes = kotDetails.Select(k => k.ItemCode).ToList();

        //    var taxDetails = salesTaxlist
        //        .Where(t =>
        //        {
        //            int billParsed;
        //            return int.TryParse(t.Bill_No, out billParsed)
        //                   && settlementIds.Contains(billParsed)
        //                   && itemCodes.Contains(t.ItemCode);
        //        })
        //        .ToList();

        //    // 🔹 Step 6: Discount
        //    var discountDetails = itemdiscountlist
        //        .Where(d =>
        //        {
        //            int billParsed;
        //            return int.TryParse(d.BillNo, out billParsed)
        //                   && billParsed == billno;
        //        })
        //        .ToList();

        //    // 🔹 Step 7: FOOD LIST
        //    var foodList = (from kd in kotDetails
        //                    join im in itemMasterList on kd.ItemCode equals im.ItemCode
        //                    select new FoodModel
        //                    {
        //                        id = 0,
        //                        food = im.ItemName,
        //                        code = kd.ItemCode,
        //                        price = kd.KOTDRate,
        //                        qty = kd.KOTDQty,
        //                        comment = "",
        //                        category = im.CategoryId,
        //                        grpCode = im.GrpCode,
        //                        origQty = kd.KOTDQty,
        //                        itemDiscountAllowed = true
        //                    }).ToList();

        //    // 🔹 Step 8: CART
        //    var master = kotMasters.FirstOrDefault();

        //    var cart = new CartModel
        //    {
        //        userCode = int.TryParse(master?.UserCode, out var u) ? u : 0,
        //        table = master?.KOTTblNo,
        //        subTable = master?.SubTable,
        //        outlet = int.TryParse(master?.OltCode, out var o) ? o : 0,
        //        outletName = "",
        //        waiter = int.TryParse(master?.StwCode, out var w) ? w : 0,
        //        waiterName = "",
        //        pax = master?.KOTSeatsServed ?? 0,
        //        food = foodList,
        //        total = foodList.Sum(x => x.price * x.qty),
        //        totQty = foodList.Sum(x => x.qty),
        //        branch = master?.branch_code,
        //        type = "",
        //        ncCode = 0,
        //        ncRemarks = "",
        //        discount = discountDetails.Sum(x => x.DiscAmount),
        //        discountIn = discountDetails.FirstOrDefault()?.DiscountIn,
        //        discountType = discountDetails.FirstOrDefault()?.DiscountType,
        //        discountRemarks = "",
        //        discountGroups = discountDetails
        //            .Select(x => x.GrpCode.ToString())
        //            .Distinct()
        //            .ToList(),
        //        vRemarks = "",
        //        mode = "",
        //        subBillType = "",
        //        plan = "",
        //        guestName = "",
        //        guestCode = "",
        //        checkInNo = "",
        //        kotMobileNo = "",
        //        kotMinTimer = 0,
        //        taxType = "",
        //        homeDelivary = null
        //    };

        //    // 🔹 Step 9: TAX LIST
        //    var taxList = taxDetails
        //        .GroupBy(t => new { t.GroupCode, t.TaxName })
        //        .Select(g => new TaxDetailModel
        //        {
        //            groupCode = g.Key.GroupCode,
        //            groupName = "",
        //            taxName = g.Key.TaxName,
        //            taxper = g.First().TaxPer,
        //            taxableAmount = g.Sum(x => x.TaxableAmount),
        //            taxAmount = g.Sum(x => x.TaxAmount),
        //            total = g.Sum(x => x.Total),
        //            cgst = g.Sum(x => x.CGST),
        //            sgst = g.Sum(x => x.SGST)
        //        }).ToList();

        //    // 🔹 Step 10: TAX SUMMARY
        //    var tax = new TaxModel
        //    {
        //        totalAmount = cart.total,
        //        totalQty = cart.totQty,
        //        cgstPer = 0,
        //        cgstAmt = taxList.Sum(x => x.cgst),
        //        sgstPer = 0,
        //        sgstAmt = taxList.Sum(x => x.sgst),
        //        serviceChargePer = 0,
        //        serviceCharge = 0,
        //        grandTotal = cart.total + taxList.Sum(x => x.taxAmount),
        //        discountPer = 0,
        //        discount = cart.discount,
        //        discountIn = cart.discountIn,
        //        discountRemarks = cart.discountRemarks,
        //        roundOff = 0,
        //        taxList = taxList
        //    };

        //    // 🔹 Final Response
        //    var response = new BillReprintResponse
        //    {
        //        cart = cart,
        //        tax = tax,
        //        billingType = "DINEIN",
        //        subBillingType = ""
        //    };

        //    return new List<BillReprintResponse> { response };
        //}

        public async Task<IEnumerable<OutletMaster>> GetReprintOutletMaster(int usercode, string branchcode)
        {
            var SystemOutletList = await _repository.GetSystemOutlet();
            var SystemOutletDetail = SystemOutletList.Where(s => Convert.ToInt32(s.SystemName) == usercode).FirstOrDefault();

            var outletdetails = await _repository.GetReprintOutletMaster(branchcode);


            var allowedOltCodes = SystemOutletDetail.OltCode
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToHashSet();

            var oltlist = outletdetails
                .Where(w => allowedOltCodes.Contains(w.OltCode))
                .ToList();

            return oltlist;
        }

        public async Task<BillReprintResponse> GetReprintBill(GSTBillDetailModel gstdetails)
        {
            var data = await _repository.GetReprintBillData(gstdetails.BillNo, gstdetails.OltCode, gstdetails.Branchcode);

            var DefaultPrinter = await _repository.GetDefaultPrinter(gstdetails.Branchcode, Convert.ToInt32(gstdetails.OltCode));
            var DefaultPrinterResponse = DefaultPrinter.OrderBy(p => p.PrinterName).FirstOrDefault(); // ✅ Filter by branch

            if ((gstdetails.GSTNo != null && gstdetails.GSTNo != "") && (gstdetails.GuestName != null && gstdetails.GuestName != ""))
            {
                await _repository.InsertGSTData(gstdetails);
            }

            //var itemMasterList = await _repository.GetItemMasterList(branchCode);

            //-----------------------------------
            // FOOD LIST
            //-----------------------------------
            var foodList = (from kd in data.KOTDetails
                            join im in data.ItemMasters on kd.ItemCode equals im.ItemCode
                            select new FoodModel
                            {
                                Id = kd.ItemCode,
                                Food = im.ItemName,
                                code = Convert.ToString(kd.ItemCode),
                                Price = kd.KOTDRate,
                                Qty = kd.KOTDQty,
                                Comment = "",
                                Category = Convert.ToInt32(im.CatCode),
                                GrpCode = Convert.ToInt32(im.GrpCode),
                                OrigQty = kd.KOTDQty,
                                itemDiscountAllowed = im.ItemDiscountAllowed
                            }).ToList();

            //-----------------------------------
            // CART
            //-----------------------------------
            var master = data.KOTMasters.FirstOrDefault();

            //var cart = new CartModel
            //{
            //    UserCode = int.TryParse(master?.UserCode, out var u) ? u : 0,
            //    Table = master?.KOTTblNo,
            //    SubTable = master?.SubTable,
            //    Outlet = int.TryParse(master?.OltCode, out var o) ? o : 0,
            //    OutletName = data.OutletMaster.OltName,
            //    Waiter = master.StwCode,
            //    WaiterName = data.StewardMasters.Where(d => d.StwCode == master.StwCode).Select(s=> s.StwName).FirstOrDefault(),
            //    Pax = master?.KOTSeatsServed ?? 0,
            //    Food = foodList,
            //    Total = foodList.Sum(x => x.Price * x.Qty),
            //    TotQty = foodList.Sum(x => x.Qty),
            //    Branch = master?.branch_code,
            //    Discount = data.ItemDiscounts.FirstOrDefault().DiscAmount,
            //    DiscountIn = data.ItemDiscounts.FirstOrDefault()?.DiscountIn,
            //    DiscountType = data.ItemDiscounts.FirstOrDefault()?.DiscountType,
            //    DiscountGroups = data.ItemDiscounts
            //        .Select(x => x.GrpCode.ToString())
            //        .Distinct()
            //        .ToList(),
            //    TaxType = data.SalesTaxes.FirstOrDefault()?.TaxType
            //};


            var cart = new CartModel
            {
                UserCode = int.TryParse(master?.UserCode, out var u) ? u : 0,
                Table = master?.KOTTblNo ?? "",
                SubTable = master?.SubTable ?? "",
                Outlet = int.TryParse(master?.OltCode, out var o) ? o : 0,
                OutletName = data.OutletMaster?.OltName ?? "",
                Waiter = master?.StwCode ?? 0,
                WaiterName = data.StewardMasters?
                                .FirstOrDefault(d => d.StwCode == master?.StwCode)?
                                .StwName ?? "",
                Pax = master?.KOTSeatsServed ?? 0,
                Food = foodList ?? new List<FoodModel>(),
                Total = (foodList?.Sum(x => x.Price * x.Qty)) ?? 0,
                TotQty = (foodList?.Sum(x => x.Qty)) ?? 0,
                Branch = master?.branch_code ?? "",
                Discount = data.ItemDiscounts?.FirstOrDefault()?.DiscAmount ?? 0,
                DiscountIn = data.ItemDiscounts?.FirstOrDefault()?.DiscountIn ?? "",
                DiscountType = data.ItemDiscounts?.FirstOrDefault()?.DiscountType ?? "",
                DiscountGroups = data.ItemDiscounts?
                                    .Select(x => x.GrpCode.ToString())
                                    .Distinct()
                                    .ToList() ?? new List<string>(),
                TaxType = data.SalesTaxes?.FirstOrDefault()?.TaxType ?? ""
            };

            //-----------------------------------
            // 4️⃣ TAX MAPPING (using BillTax + BillTaxDetails)
            //-----------------------------------
            var billTax = data.BillTaxList; // Parent
            var taxDetails = data.BillTaxList?.TaxDetails ?? new List<BillTaxDetailModel>(); // Child

            var tax = new TaxModel
            {
                TotalAmount = billTax?.TotalAmount ?? 0,
                TotalQty = billTax?.TotalQty ?? 0,
                CGSTAmt = billTax?.CGSTAmt ?? 0,
                SGSTAmt = billTax?.SGSTAmt ?? 0,
                GrandTotal = billTax?.GrandTotal ?? 0,
                Discount = billTax?.Discount ?? 0,
                DiscountIn = billTax?.DiscountIn ?? "",
                DiscountRemarks = billTax?.DiscountRemarks ?? "",
                RoundOff = billTax?.RoundOff ?? 0,
                ServiceChargePer = 0,
                ServiceCharge = 0,

                TaxList = (taxDetails ?? new List<BillTaxDetailModel>())
                    .Select(td => new Taxdetails
                    {
                        GroupCode = td.GroupCode,
                        GroupName = td.GroupName ?? "",
                        TaxName = td.TaxName ?? "",
                        Taxper = td.Taxper,
                        TaxableAmount = td.TaxableAmount,
                        TaxAmount = td.TaxAmount,
                        Total = td.Total,
                        CGST = td.CGST,
                        SGST = td.SGST
                    }).ToList()
            };

            // Map to your TaxModel
            //var tax = new TaxModel
            //{
            //    TotalAmount = billTax.TotalAmount,
            //    TotalQty = billTax.TotalQty,
            //    CGSTAmt = billTax.CGSTAmt,
            //    SGSTAmt = billTax.SGSTAmt,
            //    GrandTotal = billTax.GrandTotal,
            //    Discount = billTax.Discount,
            //    DiscountIn = billTax.DiscountIn,
            //    DiscountRemarks = billTax.DiscountRemarks, // Add if available
            //    RoundOff = billTax.RoundOff,        // Can calculate rounding if needed
            //    ServiceChargePer = 0, // Populate if applicable
            //    ServiceCharge = 0,    // Populate if applicable
            //    TaxList = taxDetails.Select(td => new Taxdetails
            //    {
            //        GroupCode = td.GroupCode,
            //        GroupName = td.GroupName,
            //        TaxName = td.TaxName,
            //        Taxper = td.Taxper,
            //        TaxableAmount = td.TaxableAmount,
            //        TaxAmount = td.TaxAmount,
            //        Total = td.Total,
            //        CGST = td.CGST,
            //        SGST = td.SGST
            //    }).ToList()
            //};

            //var taxList = data.SalesTaxes
            //    .GroupBy(t => new { t.TaxCode })
            //    .Select(g => new Taxdetails
            //    {
            //        GroupCode = 0,
            //        GroupName = "",
            //        TaxName = g.Key.TaxCode,
            //        TaxAmount = g.Sum(x => x.TaxAmount),
            //        TaxableAmount = g.Sum(x => x.TaxAmount),
            //        Total = g.Sum(x => x.TaxAmount)
            //    }).ToList();

            //var tax = new TaxModel
            //{
            //    TotalAmount = cart.Total,
            //    TotalQty = cart.TotQty,
            //    CGSTAmt = taxList.Sum(x => x.TaxAmount) / 2,
            //    SGSTAmt = taxList.Sum(x => x.TaxAmount) / 2,
            //    GrandTotal = cart.Total + taxList.Sum(x => x.TaxAmount),
            //    Discount = cart.Discount,
            //    DiscountIn = cart.DiscountIn,
            //    RoundOff = 0,
            //    TaxList = taxList
            //};

            //-----------------------------------
            // FINAL RESPONSE
            //-----------------------------------
            return new BillReprintResponse
            {
                Cart = cart,
                Tax = tax,
                BillingType = "C",
                SubBillingType = "C",
                IPAddress = DefaultPrinterResponse?.IPAddress ?? string.Empty
            };
        }

        //public async Task<PhonePeCollectResponseBody> SendPaymentRequest(int amount, string TransNo)
        //{
        //    var translist = await _repository.GetPhonePeImageRequest();

        //    if (translist == null)
        //    {
        //        return new PhonePeCollectResponseBody
        //        { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
        //    }
        //    // static varible
        //    string PHONEPE_STAGE_BASE_URL = translist.BaseUrl;

        //    string merchantKey = translist.MerchantKey;
        //    string merchantId = translist.MerchantId;
        //    string storeId = translist.StoreId;
        //    string terminalId = translist.TerminalId;
        //    int expiresIn = translist.ExpiresIn;
        //    string providerId = translist.ProviderId;
        //    string callbackurl = translist.CallbackUrl;
        //    // Generate unique transaction ID
        //    string transactionId = "TRXID" + DateTime.Now.ToString("ddMMyyHHmmss");

        //    // Prepare response
        //    PhonePeCollectResponseBody responseBody = new PhonePeCollectResponseBody();

        //    // Build request object
        //    PhonePeCollectRequest phonePeCollectRequest = new PhonePeCollectRequest
        //    {
        //        merchantId = merchantId,
        //        transactionId = TransNo,
        //        merchantOrderId = TransNo,
        //        amount = amount,
        //        expiresIn = expiresIn,
        //        storeId = storeId,
        //        terminalId = terminalId
        //    };

        //    // Convert request to JSON
        //    string jsonStr = JsonConvert.SerializeObject(phonePeCollectRequest);

        //    // Convert JSON to Base64
        //    string base64Json = ConvertStringToBase64(jsonStr);

        //    // Generate checksum

        //    string apiEndPoint = "/v3/qr/init" + merchantKey;
        //    string checksum = GenerateSha256ChecksumFromBase64Json(base64Json, apiEndPoint) + "###1";

        //    // Combine URL safely
        //    //baseUrl = baseUrl.TrimEnd('/');
        //    //string txnURL = $"{baseUrl}{apiEndPoint}";
        //    string txnURL = PHONEPE_STAGE_BASE_URL + "/v3/qr/init";

        //    try
        //    {
        //        ServicePointManager.Expect100Continue = true;
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        //        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
        //        webRequest.Method = "POST";
        //        webRequest.ContentType = "application/json";

        //        // Add headers
        //        webRequest.Headers.Add("X-VERIFY", checksum);
        //        webRequest.Headers.Add("X-PROVIDER-ID", providerId);
        //        webRequest.Headers.Add("X-CALL-MODE", "POST");
        //        webRequest.Headers.Add("X-CALLBACK-URL", callbackurl);

        //        //webRequest.Headers.Add("X-CALLBACK-URL", string.IsNullOrEmpty(callbackUrl) ? "" : callbackUrl);

        //        // Wrap request
        //        PhonePeCollectApiRequestBody apiRequestBody = new PhonePeCollectApiRequestBody
        //        {
        //            request = base64Json
        //        };

        //        string jsonBody = JsonConvert.SerializeObject(apiRequestBody);

        //        using (StreamWriter requestWriter = new StreamWriter(webRequest.GetRequestStream()))
        //        {
        //            requestWriter.Write(jsonBody);
        //        }

        //        using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
        //        {
        //            string responseData = responseReader.ReadToEnd();
        //            if (!string.IsNullOrEmpty(responseData))
        //            {
        //                responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(responseData);
        //            }
        //        }

        //        return responseBody;
        //    }
        //    catch (WebException webEx)
        //    {
        //        // Optional: read response from server in case of error
        //        if (webEx.Response != null)
        //        {
        //            using (var errorResponse = new StreamReader(webEx.Response.GetResponseStream()))
        //            {
        //                string errorText = errorResponse.ReadToEnd();
        //                Console.WriteLine("PhonePe API error: " + errorText);
        //            }
        //        }
        //        return responseBody;
        //    }
        //}

        //// Convert JSON string to Base64
        //private string ConvertStringToBase64(string input)
        //{
        //    var plainTextBytes = Encoding.UTF8.GetBytes(input);
        //    return Convert.ToBase64String(plainTextBytes);
        //}

        //// Generate SHA256 checksum
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

        //public async Task<PhonePeCollectResponseBody> SendCheckPaymentStatusRequest(string transno)
        //{
        //    var translist = await _repository.GetPhonePeImageRequest();

        //    if (translist == null)
        //    {
        //        return new PhonePeCollectResponseBody
        //        { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
        //    }
        //    string baseUrl = translist.BaseUrl;
        //    string transactionId = "trans12746565";

        //    string merchantKey = translist.MerchantKey; // salt key from PhonePe
        //    string merchantId = translist.MerchantId;
        //    string providerId = translist.ProviderId;
        //    string keyIndex = "1"; // usually 1 in UAT, confirm with PhonePe

        //    string apiPath = $"/v3/transaction/{merchantId}/{transno}/status{merchantKey}";

        //    // ✅ Proper X-VERIFY
        //    string xVerify = GenerateSha256ChecksumFromBase64Json("", apiPath) + "###1";
        //    Console.WriteLine("X-VERIFY: " + xVerify);

        //    // Call API
        //    return await CallPhonePeStatusApi(xVerify, transno);
        //}

        //private async Task<PhonePeCollectResponseBody> CallPhonePeStatusApi(string xVerify, string transno)
        //{
        //    PhonePeCollectResponseBody responseBody = new PhonePeCollectResponseBody();

        //    var translist = await _repository.GetPhonePeImageRequest();

        //    if (translist == null)
        //    {
        //        return new PhonePeCollectResponseBody
        //        { success = false, code = "NO_CONFIG", message = "PhonePe configuration not found", data = null };
        //    }

        //    string baseUrl = translist.BaseUrl;
        //    string merchantId = translist.MerchantId;
        //    string providerId = translist.ProviderId;
        //    string transactionId = "trans12746565";
        //    // Correct endpoint (no merchantKey here!)
        //    string urlSuffix = $"/v3/transaction/{merchantId}/{transno}/status";
        //    string txnURL = baseUrl + urlSuffix;

        //    try
        //    {
        //        ServicePointManager.Expect100Continue = true;
        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        //        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(txnURL);
        //        webRequest.Method = "GET";

        //        // Very important headers
        //        webRequest.Headers.Add("X-VERIFY", xVerify);   // Must be calculated properly
        //        webRequest.Headers.Add("X-MERCHANT-ID", merchantId); // Some versions require this
        //        webRequest.Headers.Add("X-PROVIDER-ID", providerId);
        //        webRequest.ContentType = "application/json";

        //        using (StreamReader responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
        //        {
        //            string responseData = responseReader.ReadToEnd();
        //            if (!string.IsNullOrEmpty(responseData))
        //            {
        //                responseBody = JsonConvert.DeserializeObject<PhonePeCollectResponseBody>(responseData);
        //            }
        //        }
        //        return responseBody;
        //    }
        //    catch (WebException ex)
        //    {
        //        using (var stream = ex.Response?.GetResponseStream())
        //        using (var reader = new StreamReader(stream))
        //        {
        //            string error = reader.ReadToEnd();
        //            Console.WriteLine("Error Response: " + error);
        //        }
        //        return responseBody;
        //    }
        //}


        public async Task<POSServiceResult<bool>> CancelBill(CancelBillModel model)
        {
            try
            {
                var result = await _repository.CancelBill(model);

                return new POSServiceResult<bool>
                {
                    Success = true,
                    Message = "Bill cancelled successfully.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new POSServiceResult<bool>
                {
                    Success = false,
                    Message = "Error cancelling bill: " + ex.Message,
                    Data = false
                };
            }
        }

        //public async Task<IEnumerable<BillListResponse>> GetBillDetails(CancelBillListModel request)
        //{
        //    var data = await _repository.GetBillDetails(request);

        //    var result = data
        //        .GroupBy(x => x.KSMId)
        //        .Select(g => new BillListResponse
        //        {
        //            KSMId = g.Key,
        //            OltCode = g.FirstOrDefault().OltCode,
        //            KSMBillNo = g.FirstOrDefault().KSMBillNo,
        //            KSMBillDate = g.FirstOrDefault().KSMBillDate,
        //            KSMBillTime = g.FirstOrDefault().KSMBillTime,
        //            KSMBillAmount = Math.Abs(g.FirstOrDefault().KSMBillAmount),
        //            KSMBillTaxAmt = Math.Abs(g.FirstOrDefault().KSMBillTaxAmt),
        //            KSMBillDiscount = Math.Abs(g.FirstOrDefault().KSMBillDiscount),
        //            GrandTotal = (Math.Abs(g.FirstOrDefault().KSMBillAmount) + Math.Abs(g.First().KSMBillTaxAmt)),
        //            KSMBillSettled = g.FirstOrDefault().KSMBillSettled,
        //            KSMBillCancled = g.FirstOrDefault().KSMBillCancled,
        //            KSMTblNo = g.FirstOrDefault().KSMTblNo,
        //            DiscountPercent = g.FirstOrDefault().DiscountPercent,
        //            Reason = g.FirstOrDefault().Reason,
        //            Branch_Code = g.FirstOrDefault().Branch_Code,
        //            HasItemDiscount = g.FirstOrDefault().HasItemDiscount,
        //            KOTIds = g.Select(x => x.KOTId).ToList()
        //        })
        //        .ToList();

        //    return result;
        //}

        public async Task<IEnumerable<BillListResponse>> GetBillDetails(CancelBillListModel request)
        {
            var data = await _repository.GetBillDetails(request);

            var result = data
                .GroupBy(x => x.KSMId)
                .Select(g =>
                {
                    var first = g.First();

                    return new BillListResponse
                    {
                        KSMId = first.KSMId,
                        OltCode = first.OltCode,
                        KSMBillNo = first.KSMBillNo,
                        KSMBillDate = first.KSMBillDate,
                        KSMBillTime = first.KSMBillTime,
                        KSMBillAmount = Math.Abs(first.KSMBillAmount),
                        KSMBillTaxAmt = Math.Abs(first.KSMBillTaxAmt),
                        KSMBillDiscount = Math.Abs(first.KSMBillDiscount),
                        // Grand Total = Bill Amount + Tax Amount
                        GrandTotal = Math.Abs(first.KSMBillAmount) + Math.Abs(first.KSMBillTaxAmt),
                        KSMBillSettled = first.KSMBillSettled,
                        KSMBillCancled = first.KSMBillCancled,
                        KSMTblNo = first.KSMTblNo,
                        DiscountPercent = first.DiscountPercent,
                        Reason = first.Reason,
                        Branch_Code = first.Branch_Code,
                        HasItemDiscount = first.HasItemDiscount,
                        PaymentStatus = first.PaymentStatus,
                        CompanyCode = first.CompanyCode,
                        KOTIds = g.Select(x => x.KOTId).ToList()
                    };
                })
                .ToList();

            return result;
        }

        public async Task<POSServiceResult<bool>> ModifySettleBill(SettlementBillModel settlement)
        {
            //var result = false;
            try
            {
                string billno = Convert.ToString(settlement.BillNo);
                DateTime billdate = settlement.BillDate;

                string outletname = settlement.OutletName;
                //string outletname = await _repository.GetOutletNameAsync(settlement.OutletCode);

                bool isRoomService = await _repository.IsRoomServiceAsync(settlement.OltCode, settlement.BranchCode);

                if (isRoomService)
                {
                    bool updatefoodbill = await _repository.UpdateFoodBillStatusAsync(outletname, billno, billdate);
                }
                else
                {
                    bool Deletefoodbill = await _repository.DeleteFoodBillAsync(outletname, billno, billdate);

                    string outstandingBillNo = $"POS{Convert.ToInt64(billno):00000000}";

                    bool DeleteOutstanding = await _repository.DeleteOutstandingBillAsync(outstandingBillNo, billdate);
                }

                //await _repository.UpdateKotSettlementMasterAsync(billno, settlement.OltCode, settlement.BranchCode, isRoomService, billdate);

                //await _repository.DeleteBillTransferToCompanyAsync(billno, settlement.OltCode, settlement.BranchCode, billdate);

                await _repository.DeleteKotBillSettlementAsync(billno, settlement.OltCode, settlement.BranchCode, billdate);

                var settleResult = await SettleBill(settlement, "modifybill");

                if (!settleResult.Success)
                {
                    if (settleResult.Data == 0)
                    {
                        return new POSServiceResult<bool>
                        {
                            Success = false,
                            Message = "Already Partial Payment Paid Made in This particular Bill. Can not be Modify settlement.",
                            Data = settleResult.Data > 0
                        };
                    }
                    else
                    {
                        return new POSServiceResult<bool>
                        {
                            Success = false,
                            Message = "Failed while settling the bill.",
                            Data = settleResult.Data > 0
                        };
                    }
                }
                else
                {
                    return new POSServiceResult<bool>
                    {
                        Success = true,
                        Message = "settled Bill Modified successfully.",
                        Data = settleResult.Data > 0
                    };
                }
            }
            catch (Exception ex)
            {
                return new POSServiceResult<bool>
                {
                    Success = false,
                    Message = "Error Modifying Settlement Bill: " + ex.Message,
                    Data = false
                };
            }
        }

        //public async Task<POSServiceResult<bool>> ModifySettleBill(SettlementModel settlement)
        //{
        //    var result = false;
        //    try 
        //    {
        //        string billno = Convert.ToString(settlement.Bill.BillNo);
        //        DateTime billdate = settlement.Bill.BillDate;

        //        string outletname = settlement.OutletName;
        //        //string outletname = await _repository.GetOutletNameAsync(settlement.OutletCode);

        //        bool isRoomService = await _repository.IsRoomServiceAsync(settlement.OutletCode, settlement.Branch);

        //        if (isRoomService)
        //        {
        //            bool updatefoodbill = await _repository.UpdateFoodBillStatusAsync(outletname, billno, billdate);
        //        }
        //        else
        //        {
        //            bool Deletefoodbill = await _repository.DeleteFoodBillAsync(outletname, billno, billdate);

        //            string outstandingBillNo = $"POS{Convert.ToInt64(billno):00000000}";

        //            bool DeleteOutstanding = await _repository.DeleteOutstandingBillAsync(outstandingBillNo, billdate);
        //        }

        //        await _repository.UpdateKotSettlementMasterAsync(billno, settlement.OutletCode, settlement.Branch, isRoomService, billdate);

        //        await _repository.DeleteKotBillSettlementAsync(billno, settlement.OutletCode, settlement.Branch, billdate);

        //        await _repository.DeleteBillTransferToCompanyAsync(billno, settlement.OutletCode, settlement.Branch, billdate);

        //        if (settlement.PayMode?.ToLower() == "room")
        //        {
        //            //await TransferToRoom(settlement);
        //        }
        //        else
        //        {
        //            result = await SettleBill(settlement.Bill, "modifybill");
        //        }

        //        return new POSServiceResult<bool>
        //        {
        //            Success = true,
        //            Message = "settled Bill Modified successfully.",
        //            Data = result
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new POSServiceResult<bool>
        //        {
        //            Success = false,
        //            Message = "Error Modifying Settlement Bill: " + ex.Message,
        //            Data = false
        //        };
        //    }
        //}

        //private async Task TransferToRoom(SettlementModel settlement)
        //{
        //    DateTime POSEntryDate = await _repository.GetPOSEntryDate(settlement.Branch);
        //    DateTime istTime = ConvertUtcToIst();
        //    var outletmaster = await _repository.GetOutletNameAsync(settlement.OutletCode, settlement.Branch);

        //    await _repository.TransferToRoom(settlement, POSEntryDate, istTime, outletmaster.OutletName);
        //}

        public async Task<POSServiceResult<CompanyBillsResponse>> GetCompanyBillsAsync( string companyCode, string branchCode)
        {
            try
            {
                //var ssu = await _repository.GetTotalBillAmountAsync(companyCode, branchCode);

                var CompanybillDetails = await _repository.GetCompanyBillsAsync(companyCode, branchCode);

                var totalAmount = CompanybillDetails.Sum(b => b.BillAmt);

                var paidAmount = CompanybillDetails.Sum(b => b.AmtPaid);

                return new POSServiceResult<CompanyBillsResponse>
                {
                    Success = true,
                    Message = "Company bills fetched successfully.",
                    Data = new CompanyBillsResponse
                    {
                        TotalAmount = totalAmount,
                        PaidAmount = paidAmount,
                        BalanceAmount = totalAmount - paidAmount,
                        Bills = CompanybillDetails
                    }
                };
            }
            catch (Exception ex)
            {
                return new POSServiceResult<CompanyBillsResponse>
                {
                    Success = false,
                    Message = "Error fetching company bills: " + ex.Message,
                    Data = null
                };
            }
        }

        public async Task<POSServiceResult<bool>> SaveCompanyBillSettlement( CompanyBillSettlementRequest request)
        {
            try
            {
                using var con = _factory.CreateConnection(DbNames.POS);

                int Set = 0;
                //int billno = 0;
                //decimal billamount = 0;
                //decimal amountpaid = 0;
                decimal currentpay = 0;
                decimal remainingamount = request.PayingAmount;

                var result = false;

                foreach (var bill in request.Bills)
                {
                    //billno = bill.BillNo;
                    //billamount = bill.BillAmount;
                    //amountpaid = bill.AmountPaid;
                    if (request.IsFullSettlement)
                    {
                        decimal amt = bill.Partialpay + bill.AmountPaid;
                        if (remainingamount >= bill.BillAmount)
                        {
                            currentpay = bill.BillAmount - bill.AmountPaid;
                            remainingamount = remainingamount - currentpay;
                            Set = 1;

                            result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, request.FullChargesDetails, bill.BTId, con);

                        }
                        else if (amt == bill.BillAmount && remainingamount > 0)
                        {
                            currentpay = bill.Partialpay;
                            remainingamount = remainingamount - currentpay;
                            Set = 1;

                            result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, request.FullChargesDetails, bill.BTId, con);

                        }
                        else if (remainingamount < bill.BillAmount && remainingamount > 0)
                        {
                            decimal total = remainingamount + request.FullChargesDetails.Sum(x => x.ChargesAmount);
                            if (request.isChargesApplied && total == bill.BillAmount)
                            {
                                currentpay = bill.BillAmount;
                                remainingamount = remainingamount - currentpay;
                                Set = 1;

                                result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, request.FullChargesDetails, bill.BTId, con);
                            }
                            else
                            {
                                return new POSServiceResult<bool>
                                {
                                    Success = false,
                                    Message = $"Bill No {bill.BillNo} is not fully paid. Full settlement requires the bill to be fully paid.",
                                    Data = false
                                };
                            }
                        }
                    }
                    else
                    {
                        if (bill.Partialpay > 0)
                        {
                            decimal total = bill.Partialpay + bill.IndividualCharges.Sum(x => x.ChargesAmount);

                            if (bill.Partialpay == bill.BillAmount)
                            {
                                currentpay = bill.BillAmount;
                                Set = 1;
                                result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, bill.IndividualCharges, bill.BTId, con);

                            }
                            else if (bill.Partialpay < bill.BillAmount && bill.Partialpay > 0 && !bill.IndividualChargesApplied)
                            {
                                currentpay = bill.Partialpay;
                                Set = 0;
                                result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, bill.IndividualCharges, bill.BTId, con);
                            }
                            else if (bill.IndividualChargesApplied && total == bill.BillAmount)
                            {
                                currentpay = bill.BillAmount;
                                Set = 1;

                                result = await _repository.SaveCompanyBillSettlement(request, bill.BillNo, bill.BillAmount, bill.AmountPaid, currentpay, Set, bill.IndividualCharges, bill.BTId, con);
                            }
                        }
                        else
                        {
                            return new POSServiceResult<bool>
                            {
                                Success = false,
                                Message = $"Bill No {bill.BillNo} Enter valid partial payment amount.",
                                Data = false
                            };
                        }
                    }

                    //if (request.IsFullSettlement)
                    //{
                    //     CompanybillDetails
                    //    billno = bill.BillNo;
                    //    billamount = bill.BillAmount; 
                    //    amountpaid = bill.AmountPaid;

                    //    result = await _repository.SaveSettlementAsync(request, billno, billamount, amountpaid, con);

                    //}
                }

                return new POSServiceResult<bool>
                {
                    Success = true,
                    Message = "Settlement saved successfully.",
                    Data = result
                };
            }
            catch (Exception ex)
            {
                return new POSServiceResult<bool>
                {
                    Success = false,
                    Message = "Error saving settlement: " + ex.Message,
                    Data = false
                };
            }
        }

        public async Task<IEnumerable<ChargesMasterModel>> GetChargesDetails(string branchCode)
        {
            var ChargesDetails = await _repository.GetChargesDetails(branchCode);
            return ChargesDetails;
        }

        #region ModifyBill

        public async Task<ModifyBillDetailsresponse> GetModifyBillData(string orderid, int oltcode, int billno, string branchcode, DateTime settledDate)
        {
            var KotDetailsdata = await _repository.GetModifyBillData(orderid, oltcode, billno, branchcode, settledDate);

            var Discountdata = await _repository.GetBillDiscountData(oltcode, billno, branchcode, settledDate);

            return new ModifyBillDetailsresponse
            {
                KotDetails = KotDetailsdata,
                DiscountDetails = Discountdata
            };
        }

        public async Task<POSServiceResult<bool>> UnsettledBillDelete(int KSMId, int oltcode, int usercode, string Branch)
        {
            var success = await _repository.UnsettledBillDelete(KSMId, oltcode, usercode, Branch);
            return new POSServiceResult<bool>
            {
                Success = success,
                Message = success ? "Bill settlement deleted successfully." : "Settlement record not found.",
                Data = success
            };
        }

        public async Task<POSServiceResult<bool>> UnsettledKotBillDelete(int KOTId, int itemcode, string Branch)
        {
            var success = await _repository.UnsettledKotBillDelete(KOTId, itemcode, Branch);
            return new POSServiceResult<bool>
            {
                Success = success,
                Message = success ? "Bill settlement deleted successfully." : "Settlement record not found.",
                Data = success
            };
        }

        public async Task<POSServiceResult<TaxModel>> ModifyBillCalculation(CartModel cartdetails)
        {
            try
            {
                var response =  await GetBill(cartdetails);
                return new POSServiceResult<TaxModel>
                {
                    Success = true,
                    Message = "Tax Bill Calculated successfully.",
                    Data = response
                };
            }
            catch
            {
                return new POSServiceResult<TaxModel>
                {
                    Success = false,
                    Message = "Error occurred while modifying bill.",
                    Data = null
                }; ;
            }

        }

        public async Task<POSServiceResult<PosModifyBillSettlementResponse>> ModifyBillCreateUpdate(PosModifyBillSettlement modifydetails)
        {
            try
            {
                //DateTime POSEntryDate = _repository.GetPOSEntryDate(modifydetails.Branch_Code);

                var financialList = await _posdal.GetFinancialMasters(modifydetails.Branch_Code);

                if (modifydetails.IsDiscountAdded)
                {
                    BillModel billdetails = new BillModel()
                    {
                        Cart = new CartModel
                        {
                            UserCode = modifydetails.UserCode,
                            Table = modifydetails.KOTTblNo,
                            Outlet = modifydetails.OltCode,
                            Branch = modifydetails.Branch_Code,
                            Discount = modifydetails.Taxdetails.Discount,
                            DiscountType = modifydetails.DiscountType,

                            Food = modifydetails.GrpCode.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => new FoodModel { GrpCode = Convert.ToInt32(x.Trim()) }) .ToList()
                        },

                        Tax = modifydetails.Taxdetails
                    };

                    var DiscountDetailsList = await _repository.ItemDiscount(Convert.ToString(modifydetails.OltCode), modifydetails.Branch_Code);
                    var DiscountList = DiscountDetailsList.FirstOrDefault(x => x.Billdate == modifydetails.SettledDate && Convert.ToInt32(x.BillNo) == modifydetails.KSMBillNo);

                    if (DiscountList == null)
                    {
                        await _repository.SaveDiscount(Convert.ToString(modifydetails.KSMBillNo), billdetails, modifydetails.SettledDate);
                    }
                    else
                    {
                        await _repository.UpdateDiscount(modifydetails.KSMBillNo, billdetails, modifydetails.SettledDate);
                    }
                }
                else if (!modifydetails.IsDiscountAdded)
                {
                   await _repository.DeleteDiscount(modifydetails.KSMBillNo, modifydetails.OltCode, modifydetails.SettledDate, modifydetails.Branch_Code);
                }

                var SalesTaxList = await _repository.SalesTaxList(modifydetails.OltCode, modifydetails.Branch_Code);
                var salesTax = SalesTaxList.Where(x => x.BillDate == modifydetails.SettledDate && Convert.ToInt32(x.Bill_No) == modifydetails.KSMBillNo).ToList();

                var BillTaxList = await _repository.BillTaxList(modifydetails.OltCode, modifydetails.Branch_Code);
                var BillTax = BillTaxList.Where(x => x.BillDate == modifydetails.SettledDate && Convert.ToInt32(x.BillId) == modifydetails.KSMBillNo).FirstOrDefault();

                var BillTaxDetailsList = await _repository.BillTaxDetailsList(modifydetails.OltCode, modifydetails.Branch_Code);
                var BillTaxDetails = BillTaxDetailsList.Where(x => x.BillDate == modifydetails.SettledDate && Convert.ToInt32(x.BillId) == modifydetails.KSMBillNo).FirstOrDefault();

                var data = await _repository.ModifyBillCreateUpdate(modifydetails, salesTax, BillTax, BillTaxDetails, financialList.FinCode);

                if (data.Success)
                {
                    return new POSServiceResult<PosModifyBillSettlementResponse>
                    {
                        Success = true,
                        Message = "Bill modified successfully.",
                        Data = data
                    };
                }
                else
                {
                    return new POSServiceResult<PosModifyBillSettlementResponse>
                    {
                        Success = false,
                        Message = "Incompleted Bill modified.",
                        Data = data
                    };
                }
                
            }
            catch
            {
                return new POSServiceResult<PosModifyBillSettlementResponse>
                {
                    Success = false,
                    Message = "Error occurred while modifying bill.",
                    Data = null
                };
            }
        }

        #endregion

        #region Bill Adjustment

        public async Task<POSServiceResult<BillAdjustmentResponse>> GetAdjustmentLoadData(BillAdjustmentRequest request)
        {
            var Billresponsedata = await _repository.GetAdjustmentLoadData(request);

            var Itemresponsedata = await _repository.GetBillSettlementAsync(request);


            return new POSServiceResult<BillAdjustmentResponse>
            {
                Success = true,
                Message = "Data loaded successfully.",
                Data = new BillAdjustmentResponse
                {
                    BillDetails = Billresponsedata,
                    ItemDetails = Itemresponsedata,
                    TotalBillsFound = Itemresponsedata.Select(x => x.BillNo).Distinct().Count(),
                    TotalAmount = Billresponsedata.Sum(x => x.BillAmount),

                    GroupDetails = Itemresponsedata.GroupBy(x => x.GrpCode).Select(g => new GroupTotalModel
                    {
                        Groupwise = g.First().GrpName,
                        Amount = g.Sum(x => x.Amount)
                    }).ToList()
                }
            };
        }

        public async Task<POSServiceResult<RankAmountResponse>> GetCalculateRankAmount(int RankId, decimal TotalAmount, string BranchCode)
        {
            if (RankId <= 1)
            {
                return new POSServiceResult<RankAmountResponse>
                {
                    Success = false,
                    Message = "Rank Id Should be Greater than 1",
                    Data = null
                };
            }

            var estimatedAmount = await _repository.GetEstimatedAmount(RankId, BranchCode);

            return new POSServiceResult<RankAmountResponse>
            {
                Success = true,
                Message = "Data loaded successfully.",
                Data = new RankAmountResponse
                {
                    EstimatedAmount = estimatedAmount,
                    BidAmount = TotalAmount - estimatedAmount
                }
            };
        }

        public async Task<POSServiceResult<BillAdjustmentResponse>> NewbiddingCheck(int RankId, decimal BidAmount, string BranchCode)
        {
            if (BidAmount <= 0)
            {
                return new POSServiceResult<BillAdjustmentResponse>
                {
                    Success = false,
                    Message = "Enter Bid Amount.",
                    Data = null
                };
            }

            var Itemresponsedata = await _repository.GetBiddingItemsAsync(RankId, BranchCode);

            return new POSServiceResult<BillAdjustmentResponse>
            {
                Success = true,
                Message = "Data loaded successfully.",
                Data = new BillAdjustmentResponse
                {
                    ItemDetails = Itemresponsedata,
                    TotalAmount = Itemresponsedata.Where(x => x.Chk == "").Sum(x => x.Amount)
                }
            };  
        }

        public async Task<POSServiceResult<SaveBidResponse>> SaveBidChanges( SaveBidRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            if (request.FinalSaleAmount <= 0)
            {
                return new POSServiceResult<SaveBidResponse>
                {
                    Success = false,
                    Message = "Enter Amount",
                    Data = null
                };
            }

            try
            {
                var financialList = await _posdal.GetFinancialMasters(request.BranchCode);

                bool updaterow1 = await _repository.ResetKOTMasterValuesAsync(request, connection, transaction);

                bool updaterow2 = await _repository.ResetKOTSettlementMasterValuesAsync(request, connection, transaction);

                bool updaterow3 = await _repository.ResetKOTBillSettlementAsync(request, connection, transaction);

                //bool deleterow1 = await _repository.DeleteSalesTaxAsync(request, connection, transaction);

                var outletSettings = await _repository.GetOutletSettingsAsync(request, connection, transaction);

                var tempBills = await _repository.GetTmpBillSettlementAsync(request, connection, transaction);

                var billGroups = tempBills.GroupBy(x => x.KSMId);

                foreach (var billGroup in billGroups)
                {
                    decimal totalAmount = 0;
                    int totalQty = 0;
                    decimal totalTax = 0;
                    decimal cgst = 0;
                    decimal sgst = 0;

                    foreach (var item in billGroup)
                    {

                        if (item.Chk == "N")
                        {
                            decimal amount = item.KotRate * item.Qty;
                            totalAmount += amount;
                            totalQty += item.Qty;


                            bool updaterow4 = await _repository.UpdateAdjustmentKotMasterAsync(item.KOTId, amount, item.BillDate, request.OltCode, request.BranchCode, connection, transaction);

                            var taxes = await _repository.GetItemTaxesAsync(item.ItemCode, request.OltCode, request.BranchCode, connection, transaction);

                            foreach (var tax in taxes)
                            {
                                decimal taxValue = amount * Convert.ToDecimal(tax.TaxPercentage) / 100;

                                totalTax += taxValue;

                                if (tax.TaxDescription.Contains("CGST"))
                                    cgst += taxValue;

                                if (tax.TaxDescription.Contains("SGST"))
                                    sgst += taxValue;

                                //bool insertrow = await _repository.InsertSalesTaxAsync(
                                //    new SalesTax
                                //    {
                                //        Bill_No = item.BillNo,
                                //        ItemCode = Convert.ToInt32(item.ItemCode),
                                //        TaxCode = tax.TaxCode,
                                //        TaxAmount = Convert.ToDouble(taxValue),
                                //        Branch_Code = request.BranchCode,
                                //        OltCode = Convert.ToInt32(request.OltCode),
                                //        BillDate = item.BillDate,
                                //        Ref = "F",
                                //        IsOnline = 0,
                                //        FinCode = financialList.FinCode,
                                //        TaxType = "OnbillTax"

                                //    }, connection, transaction);
                            }
                        }
                        else
                        {
                            if (request.RankByType.ToLower() == "kotno")
                            {
                                bool deleterow2 = await _repository.DeleteKotDetailsAsync(item.KOTId.ToString(), item.ItemCode.ToString(), item.KId.ToString(), request.BranchCode, connection, transaction);

                                bool deleterow3 = await _repository.DeleteSaleTaxAsync(item.BillNo, item.ItemCode, request.OltCode, request.BranchCode, connection, transaction);

                            }
                            else
                            {
                                bool deleterow4 = await _repository.DeleteKotSettlementDetailsAsync(item.KOTId.ToString(), item.KSMId.ToString(), request.BranchCode, connection, transaction);

                                bool deleterow5 = await _repository.DeleteKotDetailsAsync(item.KOTId.ToString(), item.ItemCode.ToString(), item.KId.ToString(), request.BranchCode, connection, transaction);

                                bool deleterow6 = await _repository.DeleteSaleTaxAsync(item.BillNo, item.ItemCode, request.OltCode, request.BranchCode, connection, transaction);
                            }
                        }
                    }

                    var first = billGroup.First();

                    AdjustmentSalesTaxModel taxdetails = new AdjustmentSalesTaxModel
                    {
                        KSMBillNo = Convert.ToInt32(first.BillNo),
                        KSMId = Convert.ToInt32(first.KSMId),
                        SettledDate = first.BillDate,
                        OltCode = Convert.ToInt32(request.OltCode),
                        BranchCode = request.BranchCode,
                        TaxableAmount = totalAmount,
                        TaxAmount = totalTax,
                        Total = totalAmount + totalTax,
                        CGST = cgst,
                        SGST = sgst,
                        TotalQty = totalQty,
                        RoundOff = 0

                    };

                    bool status = await _repository.UpdateAdjustmentBillTaxAsync(taxdetails, connection, transaction);


                    decimal serviceCharge = 0;

                    if (outletSettings.ServiceCharge > 0)
                    {
                        serviceCharge = (totalAmount * outletSettings.ServiceCharge) / 100;
                    }


                    bool updaterow5 = await _repository.UpdateAdjustmentKotSettlementMasterAsync(
                        new KotSettlementMasterUpdateModel
                        {
                            BillNo = first.BillNo,
                            BillDate = first.BillDate,
                            KSMId = first.KSMId.ToString(),
                            BillAmount = totalAmount,
                            TaxAmount = totalTax,
                            ServiceTaxAmount = 0,
                            ServiceCharge = serviceCharge,
                            OltCode = request.OltCode,
                            BranchCode = request.BranchCode
                        }, connection, transaction);

                    bool updaterow6 = await _repository.UpdateAdjustmentKotBillSettlementAsync(
                        first.BillNo,
                        first.KSMId.ToString(),
                        totalAmount + totalTax,
                        request.OltCode,
                        first.BillDate,
                        request.BranchCode,
                        connection,
                        transaction);
                }

                bool updaterow7 = await _repository.UpdateFinalBillAmountAsync(request.FromDate, request.ToDate, request.BranchCode, connection, transaction);

                transaction.Commit();

                return new POSServiceResult<SaveBidResponse>
                {
                    Success = true,
                    Message = "Saved Bid Changes Successfully",
                    Data = null
                };

            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return new POSServiceResult<SaveBidResponse>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                };
            }
        }

        #endregion

        #region POS Room Service

        public async Task<POSServiceResult<IEnumerable<RoomTableStatusModel>>> GetTableListForRoomService(int oltcode, string branchcode)
        {
            //var SystemOutletList = await _repository.GetSystemOutlet();
            //var SystemOutletDetail = SystemOutletList.FirstOrDefault(s => Convert.ToInt32(s.SystemName) == usercode);

            //if (SystemOutletDetail == null)
            //{
            //    return new POSServiceResult<IEnumerable<RoomTableStatusModel>>
            //    {
            //        Success = false,
            //        Message = "System outlet configuration not found.",
            //        Data = null
            //    };
            //}

            //var OutletDetails = await _repository.POSRoomServiceAsync(branchcode);

            //var allowedOltCodes = SystemOutletDetail.OltCode
            //    .Split(',', StringSplitOptions.RemoveEmptyEntries)
            //    .Select(x => Convert.ToInt32(x.Trim()))
            //    .ToHashSet();

            //if (!allowedOltCodes.Contains(OutletDetails.OltCode))
            //{
            //    return new POSServiceResult<IEnumerable<RoomTableStatusModel>>
            //    {
            //        Success = false,
            //        Message = "There is no allowed outlet for the specified branch.",
            //        Data = null
            //    };
            //}

            var IsRoomService = await _repository.IsRoomServiceAsync(oltcode, branchcode);


            // Check whether room service is enabled
            if (!IsRoomService)
            {
                return new POSServiceResult<IEnumerable<RoomTableStatusModel>>
                {
                    Success = false,
                    Message = "Room service is not enabled for the specified outlet.",
                    Data = null
                };
            }

            var CombinedList = await _repository.GetTableListForRoomService();

            var KOTMasterList = await _repository.GetKOTMasterListForRoomService(oltcode);

            var occupiedRooms = KOTMasterList.ToHashSet();

            foreach (var table in CombinedList)
            {
                table.TableStatus = occupiedRooms.Contains(table.RoomNo)
                    ? "Occupied"
                    : "Available";
            }

            return new POSServiceResult<IEnumerable<RoomTableStatusModel>>
            {
                Success = true,
                Message = "Fetched Room Table Status Successfully",
                Data = CombinedList
            };
        }

        public async Task<POSServiceResult<bool>> GetRoomInActive(string RoomNo, int BillNo)
        {
            bool IsRoomCheckinareNot = await _repository.IsRoomCheckinareNot(RoomNo, BillNo);

            if (!IsRoomCheckinareNot)
            {
                return new POSServiceResult<bool>
                {
                    Success = true,
                    Message = "This Table is Not Checkin Room Service. Can not be Modify settlement.",
                    Data = true
                };
            }
            else
            {
                return new POSServiceResult<bool>
                {
                    Success = true,
                    Message = "Fetched Room Table Status Successfully",
                    Data = false
                };
            }
        }

        #endregion

        #region Unsettled KOT and Bill Details

        public async Task<POSServiceResult<IEnumerable<UnsettledKOTModel>>> GetUnsettledKOTDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            var KOTMasterList = await _repository.GetUnsettledKOTDetails(fromdate, todate, Branchcode);

            return new POSServiceResult<IEnumerable<UnsettledKOTModel>>
            {
                Success = true,
                Message = "Fetched Unsettled KOT Details Successfully",
                Data = KOTMasterList
            };
        }

        public async Task<POSServiceResult<IEnumerable<UnsettledBillModel>>> GetUnsettledBillDetails(DateTime fromdate, DateTime todate, string Branchcode)
        {
            var BillMasterList = await _repository.GetUnsettledBillDetails(fromdate, todate, Branchcode);

            return new POSServiceResult<IEnumerable<UnsettledBillModel>>
            {
                Success = true,
                Message = "Fetched Unsettled Bill Details Successfully",
                Data = BillMasterList
            };
        }

        public async Task<POSServiceResult<bool>> UpdateUnsettledKOT(List<UpdateUnsettledKOTRequest> request)
        {
            var success = await _repository.UpdateUnsettledKOT(request);

            return new POSServiceResult<bool>
            {
                Success = success > 0,
                Message = success > 0 ? "KOT Cancelled successfully." : "KOT record not found.",
                Data = success > 0
            };
        }

    
        #endregion
    }
}
