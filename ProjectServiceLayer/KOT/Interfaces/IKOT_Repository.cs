using HMS_360_PMS.EntitiesModels.KOT;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.DTOs;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.KOT.Interfaces
{
    public interface IKOT_Repository
    {
        Task<int> InsertShiftAsync(OpenDayRequest request, string LocalIPAddress);

        Task<IEnumerable<PurchaseExpiryDto>> GetExpiryItemsAsync(DateTime posEntryDate);

        Task<int> GetRunningKotsCount(string branchCode);

        Task<int> GetPendingBillsCount(string branchCode);

        Task<int> CloseShiftAsync(int userId, DateTime systemTime, DateTime date, string branchCode);

        Task<int> InsertKotDisplayMaster();
        Task<int> DeleteKotDisplay();

        Task<int> InsertTmpKotPrint();
        Task<int> DeleteTmpKotPrint();

        Task<int> InsertTmpKotPrintNew();
        Task<int> DeleteTmpKotPrintNew();

        Task<int> DeleteLoggers();

        Task<int> UpdateSettlement(DateTime date, string branchCode);
        Task<int> UpdateKotMaster(DateTime date, string branchCode);

        Task<OutletdetailsResponse> GetOutletDetails(string branchCode);

        Task<int> DeleteFromToDate();
        Task<int> InsertFromToDate(DateTime date);

        Task<List<double>> GetSMSCollection();

        Task<List<string>> GetEmails();

        Task<OpenDayDetails> GetOpenDayDetails(int userid, string branchCode);

        Task<string> GetBrancheName(string branchcode);


        #region Scanner

        Task<List<CategoryModel>> GetFoodCategories(string Branchcode);

        Task<List<FoodImageModel>> GetFoodsinImage(int outlet, int category, string filter, string Branchcode);

        Task<int> GetCategoryCodeByNameAsync(string name, string Branchcode);

        Task<TimingModel> GetDayAsync(string day);

        Task<List<Slots>> GetTimeSlotsAsync(int dayId);

        Task<List<StewardModel>> GetStewards(string branchCode);

        Task<List<OutletSelectModel>> GetOutletsforUser(string username);

        Task<List<FoodScannerModel>> GetFood(string tableNo, string outlet, string subtable, string branchCode);

        Task<WaiterScannerModel> GetWaiter(string tableNo, string outlet, string subtable, string branchCode);

        Task<List<BranchViewModal>> GetBranch();

        Task<List<FoodImageModel>> GetMenuListByolt(int outlet, string branchCode);

        Task<OutletSelectModelType> GetOutletTypebyId(string oltcode, string branchcode);

        Task<string> PGGettokenexpiryTime();

        Task InsertTokenAsync(PGTokenResponseModel data, DateTime issuedAt, DateTime expiresAt);

        Task InsertPaymentOrderAsync(PGCreatePaymentResponse data);

        Task InsertPaymentStatusAsync(PGPaymentOrderStatus data, string merchantOrderId, string rawJson);

        Task<RoomserviceModel> GetRoomserviceDetails(string roomno);

        Task<CompanyInfo> GetCompanyinfoBill();

        Task<BillModelForBIll> Getbillnowithorderid(string orderid, int Oltcode, string Branchcode);

        #endregion


        #region Scanner in POS

        Task<dynamic> TableReservations(int ResId);

        Task<dynamic> GetKOTDetails(string table, string subtable, int outlet, string branchcode);

        Task<BillConfigModel> GetKOTConfig(string branchcode);

        Task<int> GetKOTNo(string SubKOTType, bool isNCKOT, string branchcode);

        Task<DateTime> GetPOSEntryDate(string branchCode);

        Task<FinancialMaster> GetFinancialMasters(string Branch);

        Task<int> GetNextDKOT(string branchcode);

        Task<KOTModel> SaveKOT(int Outlet, string Table, int Waiter, int Pax, DateTime POSEntryDate, double Total, int UserCode, bool Settled, bool Canceled,
    string SubTable, string BranchCode, string Type, string Remarks, int DKOT, string CheckInNo, string GuestName, string GuestCode, string GuestMobileNo, string FinCode);

        Task<bool> UpdateKOTMasterAsync(UpdateKOTMasterRequest request);

        Task<bool> UpdateTableReservationAsync(int kotNo, int reservationId);

        Task<int> GetSpecialInfoId(string specialInfo, string branchCode);

        Task<List<int>> GetSpecialInfoIds(string specialInfoCsv, string branchcode);

        Task<dynamic> SaveKOTDetailAsync(SaveKOTDetailRequest request);

        Task<string> GetOutletName(int outlet, string branchCode);

        Task<GlobalSettingsModel> GetGlobalSettings(string branchCode);

        Task<IEnumerable<FreeItemDetail>> GetFreeItemsAsync(int outletCode, int itemCode, string branchCode);

        Task<dynamic> SaveFreeItemKOTDetailAsync(SaveKOTDetailRequest request);

        Task<string> GetKotType(int outletCode, string branchCode);

        Task<bool> InsertTmpKotPrintAsync(TmpKotPrintRequest request);

        Task<bool> UpdateKotDetails(int KOTId, string branchcode);

        Task<ActiveKotDetail?> GetActiveKotAsync(string table, int outlet, string subTable, int itemCode, string branch, bool isChargeable);

        Task UpdateKotMasterForVoidAsync(int kotNo, string branch, string remarks);

        Task GenerateCancelKotAsync(int kotId, int itemCode, int qty, int kid, string branch, bool isChargeable, string tableNo, string subTable, int userCode, string remarks);

        Task InsertKotModifyDetailsAsync(int kotNo, int itemCode, int origQty, int userCode, DateTime lastModify, int outlet, int preQty, string branch);

        Task<BillConfigModel> GetPOSBillConfig(string branchcode);

        Task<OutletSelectModelType> GetOutletType(int oltcode, string branchcode);

        Task<int> GetNextGuestCodeAsync();

        Task<bool> InsertGuestAsync(HomeDelivery guest);

        Task<bool> UpdateGuestAsync(HomeDelivery guest);

        #endregion

        #region Scanner in POS PostBill
        Task InsertNCSalesTaxBulk(List<NCSalesTax> taxes);

        Task SaveDiscount(string BillNo, KOTBillModel Bill, DateTime POSEntryDate);

        Task<List<ExtraChargeModel>> GetExtraCharges(string ItemCode, string BranchCode, int Outlet);

        Task<KBillModel> SettleKOTPartI(DateTime posentrydate, KOTCartModel cart, double taxamount,
           double discount, string Reason, double RoundOff,
           int TokenNo, string BillingType, string SubBillingType, string GuestName, string GuestMobileNo, string fincode);

        Task<KBillModel> SettleNCKOTPartI(DateTime posentrydate, KOTCartModel cart, double taxamount,
           double discount, double SerTax, string Reason, double RoundOff,
            double SerCharge, int TokenNo, string BillingType, string SubBillingType, string fincode);

        Task<string> GetInfo(string tblname, string fieldname, string code, string parameter, int parameter1, string branch);

        Task InsertSalesTaxBulk(List<SalesTax> taxes);

        Task InsertSalesGroupTaxBulk(int billId, string branchCode, int oltCode, KOTTaxModel Tax, DateTime POSEntryDate);

        //Task<int> GetItemMasterGroupList(int itemcode, int catcode, string branchcode);

        Task<IEnumerable<ItemGroup>> GetItemGroupList(string branchcode);

        Task<bool> SavePhonePeOrderBillDetails(string orderId, string billNo, DateTime billDate, int outletCode, string outletName, string billView, string branchCode);

        Task<bool> UpdatePaymentStatus(string orderId, string billNo);

        Task<bool> SettleBill(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate);

        Task<bool> PhonepeSettleBill(SettlementModel settlement, KOTPhonePeCollectResponseBody payment, DateTime posentrydate, DateTime istTime);

        #endregion

        #region Scanner in POS GetBill
        Task<KOTTaxModel> GetTaxCharges(int Outlet, string BranchCode);

        Task<double> LoadRule(string ruletype, double BillAmt, double CGST, double SGST, DateTime fromdate, string branchcode);


        #endregion

        #region  whatsapp config

        Task<Whatsupconfig> WhatsappConfiguration();

        #endregion

        #region QR payment Scanner

        //void SMSBuilder(int MesgId, string ContactId, dynamic Messaged);

        #endregion

        #region  whatsapp config

        Task<OnlinePaymentTypeModel> OnlinePaymentType();

        #endregion
    }
}
