

using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.HMS_360_PMS.Infrastructure.POS;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.DTOs;
using HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HMS_360_PMS.HMS_360_PMS.ServiceLayer.POS.Services
{
    public class POSReports_Service : IPOSReports_Service
    {
        private readonly IPOSReports_Repository _reportrepo;
        private readonly POSReports_DAL _posreportdal;
        private readonly DbConnectionFactory _factory;


        public POSReports_Service(IPOSReports_Repository reportrepo, POSReports_DAL posreportdal, DbConnectionFactory factory)
        {
            _reportrepo = reportrepo;
            _posreportdal = posreportdal;
            _factory = factory;
        }

        public async Task<List<DailySalesModel>> GetDailySales(DateTime fromdate, DateTime todate, string outlet)
        {
            var daily = await _reportrepo.GetDailySales(fromdate, todate, outlet);
            return daily;
        }

        public async Task<List<ReportItemGroup>> GetItemGroups()
        {
            var itemgrp = await _reportrepo.GetItemGroups();
            return itemgrp;
        }

        //public async Task<List<ItemSalesModelResponse>> GetItemSales(DateTime fromdate, DateTime todate, string outlet)
        //{
        //    var ItemSales = await _reportrepo.GetItemSales(fromdate, todate, outlet);

        //    var result = ItemSales
        //        .GroupBy(x => x.GroupCode)
        //        .SelectMany(group => group
        //            .GroupBy(x => x.ItemCode)
        //            .Select(item => new ItemSalesModelResponse
        //            {
        //                GroupName = item.First().GroupName,
        //                ItemName = item.First().ItemName,
        //                Rate = item.First().Rate,
        //                Quantity = item.Sum(x => x.Qty),          // ✅ SUM QTY
        //                Total = item.Sum(x => x.Total),      // ✅ SUM TOTAL

        //                OutletName = item.First().OltName
        //            })
        //        )
        //        .OrderBy(x => x.ItemName)
        //        .ToList();

        //    return result;
        //}

        public async Task<List<GroupedItemSalesResponse>> GetItemSales(DateTime fromdate, DateTime todate, string outlet)
        {
            var itemSales = await _reportrepo.GetItemSales(fromdate, todate, outlet);

            var result = itemSales
                .GroupBy(x => x.GroupCode)
                .Select(group => new GroupedItemSalesResponse
                {
                    GroupName = group.First().GroupName,

                    Items = group
                        .GroupBy(x => x.ItemCode)
                        .Select(item => new ItemSalesModelResponse
                        {
                            GroupName = item.First().GroupName,
                            ItemName = item.First().ItemName,
                            Rate = item.First().Rate,
                            Quantity = item.Sum(x => x.Qty),
                            Total = item.Sum(x => x.Total),
                            OutletName = item.First().OltName
                        })
                        .OrderBy(x => x.ItemName)
                        .ToList()
                })
                .OrderBy(x => x.GroupName)
                .ToList();

            return result;
        }

        public async Task<List<ReportOutletMaster>> GetOutlets()
        {
           var oultels = await _reportrepo.GetOutlets();
            return oultels;
        }

        public async Task<List<TableModel>> GetTables(string outlet)
        {
           var tables = await _reportrepo.GetTables(outlet);
            return tables;
        }

        //public async Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode)
        //{
        //    var response = await _reportrepo.GetChancesheet(fromdate, todate, outlet, branchcode);
        //    var data = response.Data;

        //    // Step 1: Remove duplicate bill effect

        //    var groupedBills = data
        //        .GroupBy(x => x.BillNo)
        //        .Select(g => new
        //        {
        //            Tax = g.First().Tax,
        //            CGST = g.First().CGST,
        //            SGST = g.First().SGST,
        //            Dis = g.First().Dis,
        //            Total = g.First().Total,
        //            Grand = g.First().Grand,
        //            RoundOff = g.First().RoundOff,

        //            Cash = g.Sum(x => x.Cash),
        //            Card = g.Sum(x => x.Card),
        //            UPI = g.Sum(x => x.UPI),
        //            Online = g.Sum(x => x.Online),
        //            Cheque = g.Sum(x => x.Cheque),
        //            Credit = g.Sum(x => x.Credit)
        //        })
        //        .ToList();

        //    // Step 2: Outletwise summery

        //    response.OutletWiseSummary = data.GroupBy(x => x.oltcode)
        //        .Select(g => new OutletSummary
        //        {
        //            //OltCode = g.Key,
        //            OutletName = g.First().OltName,
        //            TotalAmount = g.Sum(x => x.Grand)
        //        })
        //        .OrderBy(x => x.OutletName)
        //        .ToList();

        //    // Step 2: Summary
        //    response.Summary = new ChanceSheetSummary
        //    {
        //        Tax = groupedBills.Sum(x => x.Tax),
        //        CGST = groupedBills.Sum(x => x.CGST),
        //        SGST = groupedBills.Sum(x => x.SGST),
        //        Discount = groupedBills.Sum(x => x.Dis),
        //        Total = groupedBills.Sum(x => x.Total),
        //        Grand = groupedBills.Sum(x => x.Grand),
        //        RoundOff = groupedBills.Sum(x => x.RoundOff),

        //        Cash = groupedBills.Sum(x => x.Cash),
        //        Card = groupedBills.Sum(x => x.Card),
        //        UPI = groupedBills.Sum(x => x.UPI),
        //        Online = groupedBills.Sum(x => x.Online),
        //        Cheque = groupedBills.Sum(x => x.Cheque),
        //        Credit = groupedBills.Sum(x => x.Credit)

        //    };
        //    return response;
        //}

        public async Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet, string branchcode)
        {
            var response = await _reportrepo.GetChancesheet(fromdate, todate, outlet, branchcode);
            var data = response.Data;

            // Remove duplicate bill effect for bill totals only
            var groupedBills = data
                .GroupBy(x => x.BillNo)
                .Select(g => new
                {
                    Tax = g.First().Tax,
                    CGST = g.First().CGST,
                    SGST = g.First().SGST,
                    Dis = g.First().Dis,
                    Total = g.First().Total,
                    Grand = g.First().Grand,
                    RoundOff = g.First().RoundOff
                })
                .ToList();

            // Outlet-wise summary
            response.OutletWiseSummary = data
                .GroupBy(x => x.oltcode)
                .Select(g => new OutletSummary
                {
                    OutletName = g.First().OltName,
                    TotalAmount = g.Sum(x => x.Grand)
                })
                .OrderBy(x => x.OutletName)
                .ToList();

            // Convert RemarksSummary into a dictionary for easy lookup
            var paymentSummary = response.RemarksSummary
                .ToDictionary(x => x.Particulars, x => x.Amount, StringComparer.OrdinalIgnoreCase);

            response.Summary = new ChanceSheetSummary
            {
                Tax = groupedBills.Sum(x => x.Tax),
                CGST = groupedBills.Sum(x => x.CGST),
                SGST = groupedBills.Sum(x => x.SGST),
                Discount = groupedBills.Sum(x => x.Dis),
                Total = groupedBills.Sum(x => x.Total),
                Grand = groupedBills.Sum(x => x.Grand),
                RoundOff = groupedBills.Sum(x => x.RoundOff),

                Cash = paymentSummary.TryGetValue("Cash", out var cash) ? (decimal)cash : 0,
                Card = paymentSummary.TryGetValue("Card", out var card) ? (decimal)card : 0,
                UPI = paymentSummary.TryGetValue("UPI", out var upi) ? (decimal)upi : 0,
                Online = paymentSummary.TryGetValue("Online", out var online) ? (decimal)online : 0,
                Cheque = paymentSummary.TryGetValue("Cheque", out var cheque) ? (decimal)cheque : 0,
                Credit = paymentSummary.TryGetValue("Credit", out var credit) ? (decimal)credit : 0,

                // If you've included these in SQL and your model supports them
                NEFT = paymentSummary.TryGetValue("NEFT", out var neft) ? (decimal)neft : 0,
                Pluxee = paymentSummary.TryGetValue("Pluxee", out var pluxee) ? (decimal)pluxee : 0,
                Cancelled = paymentSummary.TryGetValue("Cancelled", out var cancel) ? (decimal)cancel : 0
            };

            return response;
        }

        //public async Task<ChanceSheetResponse> GetChancesheet(DateTime fromdate, DateTime todate, string outlet)
        //{
        //    var data = await _reportrepo.GetChancesheet(fromdate, todate, outlet);

        //    // Step 1: Remove duplicate bill effect
        //    var groupedBills = data
        //        .GroupBy(x => x.BillNo)
        //        .Select(g => new
        //        {
        //            CGST = g.First().CGST,
        //            SGST = g.First().SGST,
        //            Discount = g.First().Discount,
        //            Total = g.First().Total,
        //            Grand = g.First().Grand,
        //            RoundOff = g.First().RoundOff,

        //            Cash = g.Sum(x => x.Cash),
        //            Card = g.Sum(x => x.Card),
        //            UPI = g.Sum(x => x.UPI),
        //            Online = g.Sum(x => x.Online),
        //            Cheque = g.Sum(x => x.Cheque),
        //            Credit = g.Sum(x => x.Credit)
        //        })
        //        .ToList();

        //    // Step 2: Summary
        //    var summary = new ChanceSheetSummary
        //    {
        //        CGST = groupedBills.Sum(x => x.CGST),
        //        SGST = groupedBills.Sum(x => x.SGST),
        //        Discount = groupedBills.Sum(x => x.Discount),
        //        Total = groupedBills.Sum(x => x.Total),
        //        Grand = groupedBills.Sum(x => x.Grand),
        //        RoundOff = groupedBills.Sum(x => x.RoundOff),

        //        Cash = groupedBills.Sum(x => x.Cash),
        //        Card = groupedBills.Sum(x => x.Card),
        //        UPI = groupedBills.Sum(x => x.UPI),
        //        Online = groupedBills.Sum(x => x.Online),
        //        Cheque = groupedBills.Sum(x => x.Cheque),
        //        Credit = groupedBills.Sum(x => x.Credit)
        //    };

        //    // Step 3: Return both
        //    return new ChanceSheetResponse
        //    {
        //        Data = data,        // ✅ original list (unchanged)
        //        Summary = summary   // ✅ separate totals
        //    };
        //}

        public async Task<List<OutletsaleModel>> GetOutletSale(DateTime fromdate, int outletcode)
        {
            var outletsale = await _reportrepo.GetOutletSale(fromdate, outletcode);
            return outletsale;
        }

        public async Task<dynamic> GetSalesChart(DateTime fromdate, DateTime todate)
        {
            var posSales = await _reportrepo.GetSalesChart(fromdate, todate);
            var outletSales = await _reportrepo.GetOutletSalesChart(fromdate, todate);
            var outlets = await _reportrepo.GetOutlets();

            dynamic chart = new System.Dynamic.ExpandoObject();
            List<dynamic> columns = new();

            var date = fromdate;
            List<dynamic> dt = new() { "date" };
            List<dynamic> po = new() { "Sales" };

            while (date <= todate)
            {
                dt.Add(date.ToString("dd/MM/yyyy"));
                var sale = posSales.FirstOrDefault(x => x.Date.Date == date.Date);
                po.Add(sale?.Amount ?? 0);
                date = date.AddDays(1);
            }

            columns.Add(dt);
            columns.Add(po);

            // Per-outlet columns
            List<string> groups = new List<string>();
            foreach (var outlet in outlets)
            {
                var outletName = outlet.OltName;
                var salesForOutlet = outletSales.Where(x => x.Outlet == outletName).ToList();

                if (salesForOutlet.Any())
                {
                    List<dynamic> typeSale = new List<dynamic> { outletName };
                    groups.Add(outletName);

                    date = fromdate;
                    while (date <= todate)
                    {
                        var sale = salesForOutlet.FirstOrDefault(x => x.Date.Date == date.Date);
                        typeSale.Add(sale?.Amount ?? 0);
                        date = date.AddDays(1);
                    }

                    columns.Add(typeSale);
                }
            }

            chart.columns = columns;
            chart.groups = new List<string>();

            return chart;
        }

        public async Task<List<TableItemsModel>> GetItemDetail(string tableNo, string oltCode)
        {
           var itemdetails = await _reportrepo.GetItemDetail(tableNo, oltCode);
            return itemdetails;
        }

        public async Task<List<VoidKotModel>> GetVoidData(DateTime fromdate, DateTime todate, string outlet)
        {
            var voiddata = await _reportrepo.GetVoidData(fromdate, todate, outlet);
            return voiddata;
        }

        public async Task<List<NCKotModel>> GetNCData(DateTime fromdate, DateTime todate, string outlet)
        {
            var ncdata = await _reportrepo.GetNCData(fromdate, todate, outlet);
            return ncdata;
        }

        public async Task<List<KotCancelReportDto>> GetKotCancellation(KotCancellationModel request)
        {
            var kotCancellationData = await _reportrepo.GetKotCancellation(request);
            return kotCancellationData;
        }

        public async Task<List<BillCancelReportDto>> GetBillCancellation(BillCancellationModel request)
        {
            var billCancellationData = await _reportrepo.GetBillCancellation(request);
            return billCancellationData;
        }

        public async Task<List<CreditOutstandingReportDto>> GetCreditOutstanding(CreditOutstandingModel request)
        {
            var creditOutstandingData = await _reportrepo.GetCreditOutstanding(request);
            return creditOutstandingData;
        }

        public async Task<DailySaleCategorywiseResponseDto> GetDailysaleCategorywise(DailySaleCategorywiseModel request)
        {
            using var con = _factory.CreateConnection(DbNames.POS);
            con.Open();

            try
            {
                List<DailysaleCategorywiseReport> normalBills;
                List<DailysaleCategorywiseReport> splitBills;

                if (request.IsSubCategory == true)
                {
                    normalBills = await _reportrepo.GetSubCatNormalBillsAsync(request, con);

                    splitBills = await _reportrepo.GetSubCatSplitBillsAsync(request, con);
                }
                else
                {
                    normalBills = await _reportrepo.GetDepWiseNormalBillsAsync(request, con);
                    splitBills = await _reportrepo.GetDepWiseSplitBillsAsync(request, con);
                }

                var finalResult = normalBills.Concat(splitBills).ToList();

                var taxAmount = await _reportrepo.GetTaxesAsync( finalResult, request.BillingType, request.OutletCode,  con);

                var groupedItems = finalResult
                      .GroupBy(x => new
                      {
                          x.ItemName,
                          x.KOTDRate,
                          x.CatName
                      })
                      .Select(g => new DailySaleCategorywiseItemDto
                      {
                          ItemName = g.Key.ItemName,
                          Qty = g.Sum(x => x.Qty),
                          Rate = g.Key.KOTDRate,
                          Category = g.Key.CatName
                      })
                      .OrderBy(x => x.Category)
                      .ThenBy(x => x.ItemName)
                      .ToList();

                // Tax Calculation

                // Equivalent of summary SQL
                var summary = new DailySaleCategorywiseSummaryDto
                {
                    TotalQuantity = groupedItems.Sum(x => x.Qty),

                    TotalTax = taxAmount,

                    TotalAmount = groupedItems.Sum(x => x.Qty * x.Rate),

                    GrandAmount =
                        groupedItems.Sum(x => x.Qty * x.Rate)
                        + taxAmount
                };

                return new DailySaleCategorywiseResponseDto
                {
                    Items = groupedItems,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                throw; // Rethrow the exception after rollback
            }
        }

        public async Task<List<KOTRegisterResponseDto>> GetKotRegister(KOTRegisterModel request)
        {
            List<KOTRegisterResponseDto> kotRegisterData = new List<KOTRegisterResponseDto>();

            if (request.IsPendingkot == true)
            {
                kotRegisterData = await _reportrepo.GetKotRegister(request, "VIEW_PENDINGKOTREGISTER", 1);
            }
            else
            {
                kotRegisterData = await _reportrepo.GetKotRegister(request, "VIEW_KOTREGISTER", 0);

            }
            return kotRegisterData;
        }

        public async Task<DashboardSummaryDto> GetDashboardData(string Branchcode)
        {
            DateTime TodayDateTime = ConvertUtcToIst();

            var dashboardData = await _reportrepo.GetDashboardData(Branchcode, TodayDateTime);
            return dashboardData;
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
    }
}
