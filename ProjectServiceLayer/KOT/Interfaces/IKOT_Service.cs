using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.Services_Layers.KOT.Interface
{
    public interface IKOT_Service
    {
        Task<OpenDayResponse> DayOpenAsync(OpenDayRequest request);

        Task<CloseDayResponse> DayCloseAsync(CloseDayRequest request);

        Task<OpenDayDetailsResponse> GetOpenDayDetais(int userid, string branchCode);

        #region Scanner

        Task<List<CategoryModel>> GetFoodCategories(string Branchcode);

        Task<FoodImageModelMain> GetFoodsinImage(int outlet, int category, string filter, string Branchcode);

        Task<bool> Submitorder(KOTCartModel cart);

        Task<List<StewardModel>> GetStewards(string branchCode);

        Task<List<OutletSelectModel>> GetOutletsforUser(string username);

        Task<CartScannerModel> Getoldcartforscanner(string tableNo, string outlet, string subtable, string branchCode);

        Task<List<BranchViewModal>> GetBranch();

        Task<List<FoodImageModel>> GetMenuListByolt(int outlet, string branchCode);

        Task<OutletSelectModelType> GetOutletTypebyId(string oltCode, string branchCode);

        Task<PGCreatePaymentResponse> CreatePaymentAsync(int amount, string redirectURL);

        Task<PGPaymentOrderStatus> GetPaymentStatusAsync(string merchantOrderId);

        Task<KOTTaxModel> GetBill(KOTCartModel cart);

        Task<bool> PostBill(KOTBillModel bill);

        Task<bool> SubmitOrderFastFoodBill(KOTBillModel bill);

        Task<RoomserviceModel> GetRoomserviceDetails(string roomno);

        Task<CompanyInfoScannerDto> GetCompanyinfoBill();

        Task<BillModelForBIll> Getbillnowithorderid(string orderid, int Oltcode, string Branchcode);

        #endregion

        #region Whatsapp Configuration

        Task<Whatsupconfig> WhatsappConfiguration();

        #endregion

        #region QR Payment Scanner

        #endregion

        #region Whatsapp Configuration

        Task<OnlinePaymentTypeModel> OnlinePaymentType();

        #endregion

        #region POS Device

        Task<bool> submitOrderdirectbillnew(KOTBillModel request);

        #endregion

        #region EmailSender

        Task<KOTServiceResult<bool>> EmailRequestAsync(EmailRequest request);

        #endregion
    }
}
