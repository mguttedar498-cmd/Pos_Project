using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using System.Data;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces
{
    public interface IPOSReports_Repository
    {
        Task<List<ReportOutletMaster>> GetOutlets();

        Task<List<TableModel>> GetTables(string outlet);

        Task<List<DailySalesModel>> GetDailySales(DateTime fromdate, DateTime todate, string outlet);

        Task<List<ReportItemGroup>> GetItemGroups();

        Task<List<ItemSalesModel>> GetItemSales(DateTime fromdate, DateTime todate, string outlet);

        Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode);

        Task<List<SalesChartData>> GetSalesChart(DateTime fromdate, DateTime todate);

        Task<List<OutletSaleData>> GetOutletSalesChart(DateTime fromdate, DateTime todate);

        Task<List<OutletsaleModel>> GetOutletSale(DateTime fromdate, int oltcode);

        Task<List<TableItemsModel>> GetItemDetail(string tableNo, string oltCode);

        Task<List<VoidKotModel>> GetVoidData(DateTime fromdate, DateTime todate, string outlet);

        Task<List<NCKotModel>> GetNCData(DateTime fromdate, DateTime todate, string outlet);

        Task<List<KotCancelReportDto>> GetKotCancellation(KotCancellationModel request);

        Task<List<BillCancelReportDto>> GetBillCancellation(BillCancellationModel request);

        Task<List<CreditOutstandingReportDto>> GetCreditOutstanding(CreditOutstandingModel request);


        Task<List<DailysaleCategorywiseReport>> GetSubCatNormalBillsAsync(DailySaleCategorywiseModel request, IDbConnection con);

        Task<List<DailysaleCategorywiseReport>> GetSubCatSplitBillsAsync(DailySaleCategorywiseModel request, IDbConnection con);

        Task<List<DailysaleCategorywiseReport>> GetDepWiseNormalBillsAsync(DailySaleCategorywiseModel request, IDbConnection con);

        Task<List<DailysaleCategorywiseReport>> GetDepWiseSplitBillsAsync(DailySaleCategorywiseModel request, IDbConnection con);

        Task<decimal> GetTaxesAsync(List<DailysaleCategorywiseReport> bills, string BillType, string OutletCode, IDbConnection con);

        Task<List<KOTRegisterResponseDto>> GetKotRegister(KOTRegisterModel request, string viewName, int status);

        Task<DashboardSummaryDto> GetDashboardData(string Branchcode, DateTime TodayDateTime);


    }
}
