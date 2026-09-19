using DocumentFormat.OpenXml.EMMA;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Emit;

namespace HMS_360_PMS.Services_Layers.POS.Interfaces
{
    public interface IPOS_Services
    {
        Task<IEnumerable<BranchDto>> GetBranchesByUsername(string username);

        Task<CompanyInfoDto> GetCompanyInfoByBranchCode(string branchcode, int companycode);

        //Task<UserMasterDto> Login(LoginRequestDto request);

        //Task<IEnumerable<BillConfig>> GetBillConfig(int usercode, string username, string branchcode);

        Task<POSServiceResult<POSLoginResponseDto>> LoginWithConfig(LoginRequestDto request);

        Task<IEnumerable<StewardMasterResponseDto>> GetStewardList(string branchcode);

        Task<IEnumerable<OutletDto>> GetCombinedOutletandtablemasterList(int usercode, string branchcode);

        Task<IEnumerable<SpecialInstruction>> GetSpecialInfo();

        Task<IEnumerable<ItemMasterResponseDto>> GetItemMasterList(string branchcode);

        Task<IEnumerable<ItemCategoryResponseDto>> GetItemCategoryList(string branchcode);

        Task<IEnumerable<ItemGroupResponseDto>> GetItemGroupList(string branchcode);

        Task<IEnumerable<CategoryListDto>> GetCombinedOltItemList(int oltcode, int grpcode, string branchcode);

        Task<IEnumerable<CategoryListDto>> GetCombinedIMandICList(string branchcode);

        Task<KOTBillModelDto> SubmitPOSOrder(CartModel cart);

        Task<List<CartResponse>> GetOldCart(string tableno, string outlet, char subtable, string branchcode);

        Task<List<SubTableStatusModel>> GetSubTables(string outlet, string tableno, string branchcode);

        Task<List<NCModel>> GetNCKOT(string branchcode);

        Task<TaxModel> GetBill(CartModel cart);

        Task<BillResponse> PostBill(BillModel bill);

        Task<List<Printerdetaildto>> GetPrinterDetails(int oltcode, string branchcode);

        Task<FastFoodDetails> GetFastfoodDetails(int outlet, string branchcode);

        Task<POSServiceResult<int>> SettleBill(SettlementBillModel settlement, string BillType);

        Task<IEnumerable<CompanyMaster>> GetCompanyMaster(string branchcode);

        Task<IEnumerable<PaymentModeGrouped>> GetPaymentModeMaster(string branchcode);

        Task<IEnumerable<UnSettlementBillModel>> GetUnbillDetails(int billno, string tblno, string outlet, string branchcode);

        Task<IEnumerable<PosUserRightsAccess>> GetPosUserAccessRight(int usercode, string userName, string branchcode);

        Task<IEnumerable<KOTTransferTypeMaster>> GetKotTransferTypeMaster(string branchcode);

        Task<bool> KOTTransferTable(KOTTransferRequest request);

        Task<bool> KOT2NCKOT(KOT2NCKOTRequest request);

        Task<IEnumerable<KOTBillSettlementModel>> GetFilteredBillDetails(KOTBillSettlementFilter filter);

        //Task<IEnumerable<BillReprintResponse>> GetReprintBill(int billno, string oltcode, string branchcode);

        //Task<BillReprintResponse> GetReprintBill(int billNo, string oltCode, string branchCode);

        Task<IEnumerable<OutletMaster>> GetReprintOutletMaster(int usercode, string branchcode);

        Task<IEnumerable<SystemOutletModel>> GetSystemOutlet();

        Task<BillReprintResponse> GetReprintBill(GSTBillDetailModel gstdetails);

        //Task<PhonePeCollectResponseBody> SendPaymentRequest(int amount, string TransNo);

        //Task<PhonePeCollectResponseBody> SendCheckPaymentStatusRequest(string transno);

        Task<TaxSettingMaster> GetTaxSettings(string branchcode);

        Task<IEnumerable<DiscountModeMaster>> GetDiscountModeMaster(string branchcode);

        Task<POSServiceResult<bool>> CancelBill(CancelBillModel model);

        Task<IEnumerable<BillListResponse>> GetBillDetails(CancelBillListModel request);

        Task<POSServiceResult<bool>> ModifySettleBill(SettlementBillModel settlement);
        //Task<POSServiceResult<bool>> ModifySettleBill(SettlementModel settlement);

        Task<POSServiceResult<CompanyBillsResponse>> GetCompanyBillsAsync( string companyCode, string branchCode);

        Task<POSServiceResult<bool>> SaveCompanyBillSettlement(CompanyBillSettlementRequest request);

        Task<IEnumerable<ChargesMasterModel>> GetChargesDetails(string branchCode);

        #region ModifyBill

        Task<ModifyBillDetailsresponse> GetModifyBillData(string orderid, int oltcode, int billno, string branchcode, DateTime settledDate);

        Task<POSServiceResult<bool>> UnsettledBillDelete(int KSMId, int oltcode, int usercode, string Branch);

        Task<POSServiceResult<bool>> UnsettledKotBillDelete(int KOTId, int itemcode, string Branch);

        Task<POSServiceResult<TaxModel>> ModifyBillCalculation(CartModel model);

        Task<POSServiceResult<PosModifyBillSettlementResponse>> ModifyBillCreateUpdate(PosModifyBillSettlement model);

        #endregion

        #region Bill Adjustment

        Task<POSServiceResult<BillAdjustmentResponse>> GetAdjustmentLoadData(BillAdjustmentRequest request);

        Task<POSServiceResult<RankAmountResponse>> GetCalculateRankAmount(int RankId, decimal TotalAmount, string BranchCode);

        Task<POSServiceResult<BillAdjustmentResponse>> NewbiddingCheck(int RankId, decimal BidAmount, string BranchCode);

        Task<POSServiceResult<SaveBidResponse>> SaveBidChanges(SaveBidRequest request);


        #endregion

        #region POS Room Service

        Task<POSServiceResult<IEnumerable<RoomTableStatusModel>>> GetTableListForRoomService(int oltcode, string branchcode);

        Task<POSServiceResult<bool>> GetRoomInActive(string RoomNo, int BillNo);

        #endregion


        #region Unsettled KOT and Bill Details

        Task<POSServiceResult<IEnumerable<UnsettledKOTModel>>> GetUnsettledKOTDetails(DateTime fromdate, DateTime todate, string Branchcode);

        Task<POSServiceResult<IEnumerable<UnsettledBillModel>>> GetUnsettledBillDetails(DateTime fromdate, DateTime todate, string Branchcode);

        Task<POSServiceResult<bool>> UpdateUnsettledKOT(List<UpdateUnsettledKOTRequest> request);

        #endregion
    }
}
