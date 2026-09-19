using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;

namespace HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra
{
    public interface IInventoryPurchase_Repository
    {
        Task<List<PurchaseOrderListResponse>> GetPurchaseOrderList(string branchCode);
        Task<int> CreatePurchaseOrder(InventoryPurchase purchase);
        Task<bool> CreatePurchaseOrderApproval(InventoryPurchase request);
        Task<bool> DeletePurchaseOrder(int poNo, string branchCode,string reasonDelete);
        Task<PurchaseOrderPrintResponse?> GetPurchaseOrderForPrint(int poNo, string branchCode);
        Task<List<PurchaseExtraChargeModel>> GetExtraCharges(string ItemCode, string BranchCode, int Storeid);
        Task<IEnumerable<BillTaxDescription>> GetMiscChargeTax(int chargeCode,string taxCode,string branchCode);
        Task<IEnumerable<InventoryMasterItemModel>> ItemStoreGetListByStoreId(string branchcode,string Storeid);
        Task<List<PurchaseOrderListResponse>> GetPurchaseOrderApprovalList(string branchCode);
        Task<PurchaseOrderPrintResponse?> GetPurchaseOrderApprovalPrint(int poNo, string branchCode);
        Task<List<PurchaseOrderListResponse>> GetPurchaseOrderGRNList(int poNo, string branchCode);
        Task<string> CreatePurchaseOrderGRN(InventoryPurchaseGRN grnrequest);
        Task<List<PurchaseOrderPonoRequest>> GetPurchaseOrderNumber(string branchCode);
        Task<GoodsReceivedNotePrintResponse?> GetGoodsReceivedNotePrint(int poNo, string GrnNo, string branchCode);
        Task<List<GoodsReceivedNoteListResponse>> GetGoodsReceivedList(string branchCode);
        Task<List<GoodsReceivedNoteListResponse>> GetPurchaseGoodsReceivedList(string branchCode, string GrnNo);
        Task<int> CreatePurchase(PurchaseSaveRequest request);
        Task<string> DeletePurchaseOrderGRN(string grnNo, string branchCode);
        Task<bool> DeleteDirectPurchase(int pNo, string branchCode);
        Task<List<PurchaseOrderList>> GetPurchasePrintList(string branchCode, int pNo);
        Task<List<PurchaseOrderList>> GetPurchaseList(string branchCode);
        Task<int> SavePurchaseReturnOrder(PurchaseReturnSaveRequest request);
        Task<List<PurchaseOrderReturnList>> GetItemPurchaseOrderList(string branchCode, int pNo);
        Task<List<PurchaseOrderGRNNUmberRequest>> GetPurchaseOrderGRNNumber(string branchCode);
        Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderList(string branchCode);
        Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderPrintList(string branchCode, int PRNo);
        Task<List<PurchaseDetailResponse>> LoadPurchaseDetailReturnData(string branchCode, int ItemCode);
        Task<List<PurchaseOrderRequestNumber>> GetPurchaseOrderNumberReturn(string branchCode);
        Task<int> SaveItemDamageAsync(ItemDamageSaveRequest request);
        Task<List<ItemDamageListResponselist>> GetPurchaseOrderItemDamageList(string branchCode, int damageNo);
        Task<List<ItemDetailsResponse>> GetItemDetailsIndentOrder(GetItemDetailsRequest request);
        Task<List<IndentOrderListResponse>> GetIndentOrderListPrint(int ioNo, string branchCode);
        Task<int> SaveIndentOrderAsync(IndentOrderSaveRequest request);
        Task<List<IndentOrderListResponse>> GetIndentOrderList(string branchCode);
        Task<int> IndentOrderApprovalSave(IndentOrderApprovalSaveRequest request);
        Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalPrintList(string branchCode, int ioNo);
        Task<List<IndentOrderSearchResponse>> SearchIndentOrderAsync(string branchCode);
        Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalData(string branchCode, int ioNo);
        Task<int> ItemIssueSave(ItemIssueSaveRequest request);
        Task<List<ItemIssueListDto>> GetItemIssuePrintData(string branchCode, int iNo);
        Task<List<ItemIssueListDto>> GetItemIssueData(string branchCode);
    }
}
