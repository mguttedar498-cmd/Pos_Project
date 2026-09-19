using Azure.Core;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.ProjectEntitiesModels.GeneralSettings;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Globalization;
using System.Reflection.Emit;
using System.Transactions;
using static System.Reflection.Metadata.BlobBuilder;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces
{
    public interface IPOS_Repository
    {
        Task<IEnumerable<Branch>> GetBranchesByUsername(string username);

        Task<CompanyInfo> GetCompanyInfoByBranchCode(string branchcode, int companycode);

        Task<UserMaster> GetUserByUsername(string username, string branchid);

        Task InsertUserLog(int userid, string branchid, DateTime logintime);

        Task<IEnumerable<BillConfig>> GetBillConfig(string branchcode);

        Task<IEnumerable<PosUserRightsAccess>> GetPosUserAccessRight(int usercode, string username, string branchcode);

        Task<ProductLicenceModel> GetProductLicenceKey(string branchCode);

        Task<IEnumerable<SystemOutletModel>> GetSystemOutlet();

        Task<IEnumerable<OutletMaster>> GetReprintOutletMaster(string branchcode);

        Task<IEnumerable<StewardMaster>> GetStewardList(string branchcode);

        Task<IEnumerable<OutletandTablemaster>> GetCombinedOutletandtablemasterList(string branchcode);

        Task<List<TableStatusModel>> GetTables(int outlet);

        Task<IEnumerable<ItemMaster>> GetItemMasterList(string branchcode);

        Task<IEnumerable<ItemCategory>> GetItemCategoryList(string branchcode);

        Task<IEnumerable<ItemGroup>> GetItemGroupList(string branchcode);

        Task<IEnumerable<CombinedOltItemlist>> GetCombinedOltItemList(int oltcode, int grpcode, string branchcode);

        Task<IEnumerable<CombinedItemMasterCategorylist>> GetCombinedIMandICList(string branchcode);

        Task<dynamic> TableReservations(int ResId);

        Task<dynamic> GetKOTDetails(string table, string subtable, int outlet, string branchcode);

        Task<BillConfigModel> GetKOTConfig(string branchcode);

        Task<int> GetKOTNo(string SubKOTType, bool isNCKOT, string branchcode);

        Task<int> GetNextDKOT(string branchcode);

        Task<DateTime> GetPOSEntryDate(string branchCode);

        Task<KOTModel> SaveKOT(int Outlet, string Table, int Waiter, int Pax, DateTime POSEntryDate, double Total, int UserCode, bool Settled, bool Canceled, 
            string SubTable, string BranchCode, string Type, string Remarks, int DKOT, string CheckInNo, string GuestName, string GuestCode, string GuestMobileNo, string FinCode);

        Task<bool> UpdateKOTMasterAsync(UpdateKOTMasterRequest request);

        Task<bool> UpdateTableReservationAsync(int kotNo, int reservationId);

        int GetSpecialInfoId(string specialInfo);

        Task <IEnumerable<string>> GetSpecialInfoIds(string specialInfoCsv, string branchcode);

        Task<IEnumerable<SpecialInstruction>> GetSpecialInfo();

        Task<dynamic> SaveKOTDetailAsync(SaveKOTDetailRequest request);

        Task<TaxSettingMaster> GetTaxSettings(string branchcode);

        Task<IEnumerable<DiscountModeMaster>> GetDiscountModeMaster(string branchcode);

        Task<GlobalSettingsModel> GetGlobalSettings(string branchCode);

        Task<IEnumerable<FreeItemDetail>> GetFreeItemsAsync(int outletCode, int itemCode, string branchCode);

        Task<dynamic> SaveFreeItemKOTDetailAsync(SaveKOTDetailRequest request);

        Task<string> GetKotType(int outletCode, string branchCode);

        Task<bool> InsertTmpKotPrintAsync(TmpKotPrintRequest request);

        Task<bool> UpdateKotDetails(int KOTId, string branchcode);

        Task<ActiveKotDetail?> GetActiveKotAsync( string table, int outlet, string subTable, int itemCode, string branch, bool isChargeable);

        Task UpdateKotMasterForVoidAsync( int kotNo, string branch, string remarks);

        Task GenerateCancelKotAsync( int kotId, int itemCode, int qty, int kid, string branch, bool isChargeable, string tableNo, string subTable, int userCode, string remarks);

        Task InsertKotModifyDetailsAsync( int kotNo, int itemCode, int origQty, int userCode, DateTime lastModify, int outlet, int preQty, string branch);

        Task<BillConfigModel> GetPOSBillConfig(string branchcode);

        Task<List<ExtraChargeModel>> GetExtraCharges(string ItemCode, string BranchCode, int Outlet);

        Task<TaxModel> GetTaxCharges(int Outlet, string BranchCode);

        Task<double> LoadRule(string ruletype, double BillAmt, double CGST, double SGST, DateTime fromdate, string branchcode);

        Task<string> GetInfo(string tblname, string fieldname, string code, string parameter, int parameter1, string branch);

        //bool InsertNCSalesTax(NCSalesTax tax);

        Task InsertNCSalesTaxBulk(List<NCSalesTax> taxes);

        Task SaveDiscount(string BillNo, BillModel Bill, DateTime POSEntryDate);

        Task UpdateDiscount(int Billno, BillModel Bill, DateTime POSEntryDate);

        Task DeleteDiscount (int Billno, int OltCode, DateTime SettledDate, string Branch_Code);

        Task<KBillModel> SettleKOTPartI(DateTime posentrydate, CartModel cart, double taxamount,
            double discount, string Reason, double RoundOff,
            int TokenNo, string BillingType, string SubBillingType, string GuestName, string GuestMobileNo, string fincode);

        Task<KBillModel> SettleNCKOTPartI(DateTime posentrydate, CartModel cart, double taxamount,
           double discount, double SerTax, string Reason, double RoundOff,
            double SerCharge, int TokenNo, string BillingType, string SubBillingType, string fincode);

        Task<(DateTime billDate, string billTime)> GetBillDateTime(string tabletype, string billNo, string branchCode, int outletCode);

        Task InsertSalesTaxBulk(List<SalesTax> taxes);

        Task InsertSalesGroupTaxBulk(int billId, string branchCode, int oltCode, TaxModel Tax, DateTime POSEntryDate);

        Task<bool> SavePhonePeOrderBillDetails(string orderId, string billNo, DateTime billDate, int outletCode, string outletName, string billView, string branchCode);

        Task<bool> UpdatePaymentStatus(string orderId, string billNo);

        Task<int> SettleBill(SettlementBillModel settlement, DateTime posentrydate, DateTime Validdate, string BillType);

        Task<bool> PhonepeSettleBill(SettlementModel settlement, PhonePeCollectResponseBody payment, DateTime posentrydate);

        Task<int> GetNextGuestCodeAsync(string Branchcode);

        Task<bool> InsertGuestAsync(HomeDelivery guest);

        Task<bool> UpdateGuestAsync(HomeDelivery guest);

        Task<List<OldCartFoodModel>> GetOldCartFoodAsync(string tableNo, string outlet, char subtable, string branchcode);

        Task<List<WaiterModel>> GetOldCartWaiterAsync(string tableNo, string outlet, char subtable, string branchcode);

        Task<List<SubTableStatusModel>> GetSubTables(string outlet, string tableno, string branchcode);

        Task<List<NCModel>> GetNCKOT(string branchcode);

        //Task<KotBillQueryResult> GetKotBillAsync(int tableNo, int kotcancel, int outlet, string table, string subTable, int KotMinTimer);

        Task<IEnumerable<KotBillQueryResult>> GetKotBillAsync(int kotsettled, int kotid, int kotcancel, int outlet, string table, string subTable, string branch);

        Task<List<CategoryGroupSetting>> GetDefaultCatGrpDetails(string branchcode, int oltcode);

        Task<List<PrinterSettingMaster>> GetDefaultPrinter(string branchcode, int oltcode);

        Task<List<PrinterSettingMaster>> GetUtilityPrinter(string branchcode, int oltcode);

        Task<FastFoodDetails> GetFastfoodDetails(int outlet, string branchcode);

        Task<IEnumerable<CompanyMaster>> GetCompanyMaster(string branchcode);

        Task<IEnumerable<PaymentModeMaster>> GetPaymentModeMaster(string branchcode);

        Task<IEnumerable<UnSettlementBillModel>> GetUnbillDetails(int billno, string tblno, string outlet, string branchcode);

        Task<IEnumerable<KOTTransferTypeMaster>> GetKotTransferTypeMaster(string branchcode);

        Task<bool> KOTTransferTable(KOTTransferRequest request, string FinCode);

        Task<bool> KOT2NCKOT(KOT2NCKOTRequest request);

        Task<IEnumerable<KOTBillSettlementModel>> GetFilteredBillDetails(KOTBillSettlementFilter filter);

        Task<IEnumerable<KOTMaster>> KOTMasters(string oltcode, string branchcode);

        Task<IEnumerable<KOTDetails>> KOTDetails(string oltcode, string branchcode);

        Task<IEnumerable<KOTSettlementMaster>> SettlementMasters(string oltcode, string branchcode);

        Task<IEnumerable<KOTSettlementDetails>> SettlementDetails(string oltcode, string branchcode);

        Task<IEnumerable<KOTBillSettlement>> KOTBillSettlement(string oltcode, string branchcode);

        Task<IEnumerable<SalesTax>> SalesTaxList(int oltcode, string branchcode);

        Task<IEnumerable<BillTaxModel>> BillTaxList(int oltcode, string branchcode);

        Task<IEnumerable<BillTaxDetailModel>> BillTaxDetailsList(int oltcode, string branchcode);

        Task<IEnumerable<ItemDiscount>> ItemDiscount(string oltcode, string branchcode);

        Task<ReprintBillData> GetReprintBillData(int billNo, string oltCode, string branchCode);

        Task InsertGSTData(GSTBillDetailModel gstdetails);

        Task<SmsSenderConfig> GetSmsDetailsAsync(string Branchcode);

        Task<int> GetRunningKotCountAsync(string branchcode);

        Task<int> GetPendingBillsCountAsync(string branchcode);

        Task<DateTime?> GetOpenShiftDateAsync(string branchCode);

        //Task<PhonePeImageRequestModel> GetPhonePeImageRequest();

        Task<bool> CancelBill(CancelBillModel model);

        Task<IEnumerable<BillListResponse>> GetBillDetails(CancelBillListModel request);

        Task<bool> IsRoomCheckinareNot(string RoomNo, int BillNo);

        Task<bool> IsRoomServiceAsync(int outletCode, string branch);

        Task<bool> UpdateFoodBillStatusAsync(string outletName, string billNo, DateTime billDate);

        Task<bool> DeleteFoodBillAsync(string outletName, string billNo, DateTime billDate);

        Task<bool> DeleteOutstandingBillAsync(string billNo, DateTime billDate);

        Task UpdateKotSettlementMasterAsync(string billNo, int outletCode, string branch, bool isRoomService, DateTime billDate);

        Task DeleteKotBillSettlementAsync(string billNo, int outletCode, string branch, DateTime billDate);

        Task DeleteBillTransferToCompanyAsync(string billNo, int outletCode, string branch, DateTime billDate);

        //Task<bool> TransferToRoom(SettlementModel settlement, DateTime posentrydate, DateTime istTime, string outletname);

        Task<OutletConfigModel> GetOutletNameAsync(int outletCode, string branchCode);

        Task<decimal> GetTotalBillAmountAsync( string companyCode, string branchCode);

        Task<decimal> GetPaidAmountAsync(int billNo, int companyCode, string branchCode);

        Task<List<CompanyBillModel>> GetCompanyBillsAsync( string companyCode, string branchCode);

        Task<bool> SaveCompanyBillSettlement(CompanyBillSettlementRequest request, int BillNo, decimal BillAmount, decimal AmountPaid, decimal CurrentPay, int Set, List<ChargeDetail> Chargeslist, int BTId, IDbConnection con);

        Task<IEnumerable<ChargesMasterModel>> GetChargesDetails(string branchCode);

        #region ModifyBill

        Task<PosModifyBill> GetModifyBillData(string orderid, int oltcode, int billno, string branchcode, DateTime settledDate);

        Task<BillDiscountModel> GetBillDiscountData(int oltcode, int billno, string branchcode, DateTime settledDate);

        Task<bool> UnsettledBillDelete(int KSMId, int oltcode, int usercode, string Branch);

        Task<bool> UnsettledKotBillDelete(int KOTId, int itemcode, string Branch);

        Task<PosModifyBillSettlementResponse> ModifyBillCreateUpdate(PosModifyBillSettlement model, List<SalesTax> salesTax, BillTaxModel BillTax, BillTaxDetailModel BillTaxDetails, string FinCode);

        #endregion

        #region Bill Adjustment

        Task<List<BillAdjustmentModel>> GetAdjustmentLoadData(BillAdjustmentRequest request);

        Task<List<BillAdjustmentSettlementModel>> GetBillSettlementAsync(BillAdjustmentRequest request);

        Task<decimal> GetEstimatedAmount(int rankId, string branchCode);

        Task<List<BillAdjustmentSettlementModel>> GetBiddingItemsAsync(int rankId, string branchCode);

        Task<bool> ResetKOTMasterValuesAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<bool> ResetKOTSettlementMasterValuesAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<bool> ResetKOTBillSettlementAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<bool> DeleteSalesTaxAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<OutletSettingsModel> GetOutletSettingsAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<IEnumerable<BillAdjustmentSettlementModel>> GetTmpBillSettlementAsync(SaveBidRequest request, IDbConnection connection, IDbTransaction transaction);

        Task<List<ExtraChargeModel>> GetItemTaxesAsync( string itemCode, string outletCode, string branchCode, IDbConnection connection, IDbTransaction transaction);

        Task<bool> InsertSalesTaxAsync(SalesTax model, IDbConnection connection, IDbTransaction transaction);

        Task<bool> UpdateAdjustmentBillTaxAsync(AdjustmentSalesTaxModel model, IDbConnection connection, IDbTransaction transaction);

        Task<bool> UpdateAdjustmentKotMasterAsync( string kotId, decimal amount, DateTime billDate, string outletCode, string branchCode, IDbConnection connection, IDbTransaction transaction);

        Task<bool> UpdateAdjustmentKotSettlementMasterAsync(KotSettlementMasterUpdateModel model, IDbConnection connection, IDbTransaction transaction);

        Task<bool> UpdateAdjustmentKotBillSettlementAsync( string billNo, string ksmId, decimal amount, string outletCode, DateTime billdate, string branchcode, IDbConnection connection, IDbTransaction transaction);

        Task<bool> DeleteSaleTaxAsync(string BillNo, string ItemCode, string OltCode, string Branchcode, IDbConnection connection, IDbTransaction transaction);

        Task<bool> DeleteKotDetailsAsync( string kotNo, string itemCode, string kid, string branchcode , IDbConnection connection, IDbTransaction transaction);

        Task<bool> DeleteKotSettlementDetailsAsync( string kotId, string ksmId, string branchcode, IDbConnection connection, IDbTransaction transaction);

        Task<bool> UpdateFinalBillAmountAsync(DateTime FromDate, DateTime ToDate, string Branchcode,  IDbConnection connection, IDbTransaction transaction);

        #endregion

        #region POS Room Service

        Task<(int OltCode, bool IsRoomService)> POSRoomServiceAsync(string branch);

        Task<IEnumerable<RoomTableStatusModel>> GetTableListForRoomService();

        Task<IEnumerable<string>> GetKOTMasterListForRoomService(int oltcode);

        #endregion


        #region Unsettled KOT and Bill Details

        Task<IEnumerable<UnsettledKOTModel>> GetUnsettledKOTDetails(DateTime fromdate, DateTime todate, string Branchcode);

        Task<IEnumerable<UnsettledBillModel>> GetUnsettledBillDetails(DateTime fromdate, DateTime todate, string Branchcode);

        Task<int> UpdateUnsettledKOT(List<UpdateUnsettledKOTRequest> request);

        #endregion
    }
}
