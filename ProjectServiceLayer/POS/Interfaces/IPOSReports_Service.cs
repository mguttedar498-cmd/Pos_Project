using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces
{
    public interface IPOSReports_Service
    {

        Task<List<ReportOutletMaster>> GetOutlets();

        Task<List<TableModel>> GetTables(string outlet);

        Task<List<DailySalesModel>> GetDailySales(DateTime fromdate, DateTime todate, string outlet);

        Task<List<ReportItemGroup>> GetItemGroups();

        Task<List<GroupedItemSalesResponse>> GetItemSales(DateTime fromdate, DateTime todate, string outlet);

        Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode);

        Task<dynamic> GetSalesChart(DateTime fromdate, DateTime todate);

        Task<List<OutletsaleModel>> GetOutletSale(DateTime fromdate, int oltcode);

        Task<List<TableItemsModel>> GetItemDetail(string tableNo, string oltCode);

        Task<List<VoidKotModel>> GetVoidData(DateTime fromdate, DateTime todate, string outlet);

        Task<List<NCKotModel>> GetNCData(DateTime fromdate, DateTime todate, string outlet);

        Task<List<KotCancelReportDto>> GetKotCancellation(KotCancellationModel request);

        Task<List<BillCancelReportDto>> GetBillCancellation(BillCancellationModel request);

        Task<List<CreditOutstandingReportDto>> GetCreditOutstanding(CreditOutstandingModel request);

        Task<DailySaleCategorywiseResponseDto> GetDailysaleCategorywise(DailySaleCategorywiseModel request);

        Task<List<KOTRegisterResponseDto>> GetKotRegister(KOTRegisterModel request);

        Task<DashboardSummaryDto> GetDashboardData(string Branchcode);

    }
}
