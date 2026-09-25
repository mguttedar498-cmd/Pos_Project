using Azure.Core;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;


namespace HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices
{
    public class InventoryPurchaseService : IInventoryPurchaseService
    {
        private readonly IInventoryPurchase_Repository _repository;
        private readonly DbConnectionFactory _factory;

        public InventoryPurchaseService(IInventoryPurchase_Repository repository, DbConnectionFactory factory)
        {
            _repository = repository;
            _factory = factory;
        }

        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderList(string branchCode)
        {
            return await _repository.GetPurchaseOrderList(branchCode);
        }
        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderApprovalList(string branchCode)
        {
            return await _repository.GetPurchaseOrderApprovalList(branchCode);
        }

        public async Task<int> CreatePurchaseOrder(InventoryPurchase purchase)
        {
            return await _repository.CreatePurchaseOrder(purchase);
        }

        public async Task<bool> CreatePurchaseOrderApproval(InventoryPurchase request)
        {
            return await _repository.CreatePurchaseOrderApproval(request);
        }

        public async Task<bool> DeletePurchaseOrder(int poNo, string branchCode, string reasonDelete)
        {
            return await _repository.DeletePurchaseOrder(poNo, branchCode, reasonDelete);
        }

        public async Task<PurchaseOrderPrintResponse?> GetPurchaseOrderForPrint(int poNo, string branchCode)
        {
            return await _repository.GetPurchaseOrderForPrint(poNo, branchCode);
        }

        public async Task<PurchaseOrderSubmitTax> PurchaseOrderCalculation(PurchaseOrderSubmitModel cart)
        {
            double totalbillamt = 0.0;
            decimal totalqty = 0;
            double discountamt = 0.0;
            string discountin = cart.DiscountIn;
            double cgsttaxper = 0.0;
            double sgsttaxper = 0.0;
            double cgsttaxamt = 0.0;
            double sgsttaxamt = 0.0;
            double serchargeper = 0.0;
            double sercharge = 0.0;
            double taxAmount = 0.0;
            double miscCharge = 0.0;
            double miscCgstAmt = 0.0;
            double miscSgstAmt = 0.0;
            double miscTaxAmount = 0.0;
            double miscTotalAmount = 0.0;

            try
            {
                foreach (var x in cart.PODetail)
                {
                    double itemQty = Convert.ToDouble(x.POItemQty);
                    double itemRate = Convert.ToDouble(x.POItemRate);

                    double itemTotal = Math.Round(itemQty * itemRate, 2);
                    totalbillamt += itemTotal;
                    totalqty += x.POItemQty;
                }

                totalbillamt = Math.Round(totalbillamt, 2);

                List<POTax> taxList = new List<POTax>();
                List<POMiscTax> miscTaxList = new List<POMiscTax>();

                foreach (var x in cart.PODetail)
                {
                    double itemQty = Convert.ToDouble(x.POItemQty);
                    double itemRate = Convert.ToDouble(x.POItemRate);

                    double itemTotal = Math.Round(itemQty * itemRate, 2);

                    var itemTaxList = await _repository.GetExtraCharges(x.ItemCode.ToString(), cart.Branch, cart.StoreId);
                    if (itemTaxList == null || !itemTaxList.Any())
                    {
                        continue;
                    }

                    foreach (var item in itemTaxList)
                    {
                        double taxPercentage = Convert.ToDouble(item.TaxPercentage);
                        double taxAmt = 0.0;
                        double cgstValue = 0.0;
                        double sgstValue = 0.0;

                        if (!string.IsNullOrEmpty(item.TaxDescription) && item.TaxDescription.Contains(
                            "CGST", StringComparison.OrdinalIgnoreCase))
                        {
                            taxAmt = Math.Round(itemTotal * taxPercentage / 100, 2);
                            cgstValue = taxAmt;
                            cgsttaxper = taxPercentage;
                        }

                        else if (!string.IsNullOrEmpty(item.TaxDescription) && item.TaxDescription.Contains(
                          "SGST", StringComparison.OrdinalIgnoreCase))
                        {
                            taxAmt = Math.Round(itemTotal * taxPercentage / 100, 2);
                            sgstValue = taxAmt;
                            sgsttaxper = taxPercentage;
                        }
                        else
                        {
                            taxAmt = Math.Round(itemTotal * taxPercentage / 100, 2);
                        }
                        taxList.Add(new POTax
                        {
                            ItemCode = x.ItemCode,
                            TaxCode = item.TaxCode,
                            TaxName = item.TaxDescription ?? "",
                            Taxper = taxPercentage,
                            POTotalAmount = itemTotal,
                            TaxAmount = taxAmt,
                            Total = taxAmt,
                            CGST = cgstValue,
                            SGST = sgstValue
                        });
                    }
                }

                if (cart.POMiscDetail != null && cart.POMiscDetail.Any())
                {
                    foreach (var misc in cart.POMiscDetail)
                    {
                        double currentMiscCharge = Math.Round(misc.MiscCharge, 2);
                        if (currentMiscCharge <= 0)
                        {
                            continue;
                        }

                        miscCharge += currentMiscCharge;

                        if (misc.MiscChargeCode <= 0 || string.IsNullOrWhiteSpace(misc.MiscTaxCode))
                        {
                            continue;
                        }

                        var miscTaxDetails = await _repository.GetMiscChargeTax(misc.MiscChargeCode, misc.MiscTaxCode, cart.Branch);
                        if (miscTaxDetails == null || !miscTaxDetails.Any())
                        {
                            continue;
                        }

                        foreach (var item in miscTaxDetails)
                        {
                            double taxPercentage = Convert.ToDouble(item.TaxPercentage);
                            double taxAmt = 0.0;
                            double cgstValue = 0.0;
                            double sgstValue = 0.0;

                            if (!string.IsNullOrEmpty(item.TaxDescription) && item.TaxDescription.Contains(
                              "CGST", StringComparison.OrdinalIgnoreCase))
                            {
                                taxAmt = Math.Round(currentMiscCharge * taxPercentage / 100, 2);
                                cgstValue = taxAmt;
                                miscCgstAmt += taxAmt;
                            }

                            else if (!string.IsNullOrEmpty(item.TaxDescription) && item.TaxDescription.Contains(
                              "SGST", StringComparison.OrdinalIgnoreCase))
                            {
                                taxAmt = Math.Round(currentMiscCharge * taxPercentage / 100, 2);
                                sgstValue = taxAmt;
                                miscSgstAmt += taxAmt;
                            }
                            else
                            {
                                taxAmt = Math.Round(currentMiscCharge * taxPercentage / 100, 2);
                            }
                            miscTaxList.Add(new POMiscTax
                            {
                                ChargeId = misc.MiscChargeCode,
                                ChargeAmt = currentMiscCharge,
                                MiscChargeCode = misc.MiscChargeCode,
                                MiscTaxCode = misc.MiscTaxCode,
                                TaxCode = item.TaxCode,
                                TaxName = item.TaxDescription ?? "",
                                Taxper = taxPercentage,
                                MiscTotalAmount = currentMiscCharge,
                                TaxAmount = taxAmt,
                                Total = currentMiscCharge + taxAmt,
                                CGST = cgstValue,
                                SGST = sgstValue
                            });
                        }
                    }
                }

                cgsttaxamt = Math.Round(
                    taxList
                        .Where(x => !string.IsNullOrEmpty(x.TaxName) &&
                                    x.TaxName.Contains("CGST",
                                        StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.CGST), 2);

                sgsttaxamt = Math.Round(
                    taxList
                        .Where(x => !string.IsNullOrEmpty(x.TaxName) &&
                                    x.TaxName.Contains("SGST",
                                        StringComparison.OrdinalIgnoreCase))
                        .Sum(x => x.SGST), 2);

                //taxAmount = Math.Round(cgsttaxamt + sgsttaxamt, 2);
                taxAmount = Math.Round(taxList.Sum(x => x.TaxAmount), 2);
                miscCharge = Math.Round(miscCharge, 2);
                miscCgstAmt = Math.Round(miscCgstAmt, 2);
                miscSgstAmt = Math.Round(miscSgstAmt, 2);
                miscTaxAmount = Math.Round(miscCgstAmt + miscSgstAmt, 2);
                miscTotalAmount = Math.Round(miscCharge + miscTaxAmount, 2);

                double finalCgst = Math.Round(cgsttaxamt, 2);
                double finalSgst = Math.Round(sgsttaxamt, 2);
                double finalTaxAmount = taxAmount;
                var calc = await dobillcalcwithServiceCharge(cart.Branch, totalbillamt, discountamt, finalCgst,
                                 finalSgst, sercharge, miscTotalAmount);

                var finalTaxList = taxList.OrderBy(x => x.ItemCode).ThenBy(x => x.TaxName).ToList();

                var finalMiscTaxList = miscTaxList.OrderBy(x => x.MiscChargeCode)
                                      .ThenBy(x => x.TaxName).ToList();

                return new PurchaseOrderSubmitTax
                {
                    TotalAmount = Math.Round(totalbillamt, 2),
                    TotalQty = totalqty,
                    CGSTPer = cgsttaxper,
                    CGSTAmt = finalCgst,
                    SGSTPer = sgsttaxper,
                    SGSTAmt = finalSgst,
                    ServiceChargePer = serchargeper,
                    ServiceCharge = Math.Round(sercharge, 2),
                    TaxAmount = finalTaxAmount,
                    Discount = Math.Round(discountamt, 2),
                    DiscountPer = cart.DiscountIn == "per" ? cart.Discount : 0,
                    DiscountIn = discountin,
                    MiscCharge = miscCharge,
                    MiscCGSTAmt = miscCgstAmt,
                    MiscSGSTAmt = miscSgstAmt,
                    MiscTaxAmount = miscTaxAmount,
                    MiscTotalAmount = miscTotalAmount,
                    GrandTotal = calc.Total,
                    RoundOff = Math.Round(calc.RoundOff, 2),
                    TaxList = finalTaxList,
                    MiscTaxList = finalMiscTaxList
                };
            }
            catch
            {
                throw;
            }
        }

        private async Task<CalcModel> dobillcalcwithServiceCharge(string branch, double TotalBillAmt, double DiscountAmt,
            double CGST, double SGST, double ServiceCharge, double MiscTotalAmount)
        {
            double amount = TotalBillAmt + CGST + SGST + ServiceCharge + MiscTotalAmount - DiscountAmt;
            double sum1 = Math.Round(amount, 0);
            double sum2 = Math.Round(amount, 2);
            double roundoff = sum2 - sum1;
            if (roundoff < 0.5)
            {
                roundoff = roundoff * -1;
            }
            return new CalcModel
            {
                RoundOff = roundoff,
                Total = Math.Round(amount + roundoff, 2)
            };
        }

        public async Task<IEnumerable<InventoryMasterItemModel>> ItemStoreGetListByStoreId(string branchcode, string Storeid)
        {
            var itemList = await _repository.ItemStoreGetListByStoreId(branchcode, Storeid);
            return itemList;
        }

        public async Task<PurchaseOrderPrintResponse?> GetPurchaseOrderApprovalPrint(int poNo, string branchCode)
        {
            return await _repository.GetPurchaseOrderApprovalPrint(poNo, branchCode);
        }

        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderGRNList(int poNo, string branchCode)
        {
            return await _repository.GetPurchaseOrderGRNList(poNo, branchCode);
        }
        public async Task<string> CreatePurchaseOrderGRN(InventoryPurchaseGRN request)
        {
            return await _repository.CreatePurchaseOrderGRN(request);
        }
        public async Task<List<PurchaseOrderPonoRequest>> GetPurchaseOrderNumber(string branchCode)
        {
            return await _repository.GetPurchaseOrderNumber(branchCode);
        }
        public async Task<GoodsReceivedNotePrintResponse?> GetGoodsReceivedNotePrint(int poNo, string GrnNo, string branchCode)
        {
            return await _repository.GetGoodsReceivedNotePrint(poNo, GrnNo, branchCode);
        }
        public async Task<List<GoodsReceivedNoteListResponse>> GetGoodsReceivedList(string branchCode)
        {
            var grnList = await _repository.GetGoodsReceivedList(branchCode);
            return grnList;
        }
        public async Task<List<GoodsReceivedNoteListResponse>> GetPurchaseGoodsReceivedList(string branchCode, string GrnNo)
        {
            var grnList = await _repository.GetPurchaseGoodsReceivedList(branchCode, GrnNo);
            return grnList;
        }
        public async Task<int> CreatePurchase(PurchaseSaveRequest request)
        {
            return await _repository.CreatePurchase(request);
        }
        public async Task<string> DeletePurchaseOrderGRN(string grnNo, string branchCode)
        {
            return await _repository.DeletePurchaseOrderGRN(grnNo, branchCode);
        }
        public async Task<bool> DeleteDirectPurchase(int pNo, string branchCode)
        {
            return await _repository.DeleteDirectPurchase(pNo, branchCode);
        }
        public async Task<List<PurchaseOrderList>> GetPurchasePrintList(string branchCode, int pNo)
        {
            var purchaseList = await _repository.GetPurchasePrintList(branchCode, pNo);
            return purchaseList;
        }
        public async Task<List<PurchaseOrderList>> GetPurchaseList(string branchCode)
        {
            var purchaseList = await _repository.GetPurchaseList(branchCode);
            return purchaseList;
        }
        public async Task<int> SavePurchaseReturnOrder(PurchaseReturnSaveRequest request)
        {
            return await _repository.SavePurchaseReturnOrder(request);
        }
        public async Task<List<PurchaseOrderReturnList>> GetItemPurchaseOrderList(string branchCode, int pNo)
        {
            var purchaseList = await _repository.GetItemPurchaseOrderList(branchCode, pNo);
            return purchaseList;
        }
        public async Task<List<PurchaseOrderGRNNUmberRequest>> GetPurchaseOrderGRNNumber(string branchCode)
        {
            return await _repository.GetPurchaseOrderGRNNumber(branchCode);
        }
        public async Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderList(string branchCode)
        {
            return await _repository.GetPurchaseReturnOrderList(branchCode);
        }
        public async Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderPrintList(string branchCode, int PRNo)
        {
            return await _repository.GetPurchaseReturnOrderPrintList(branchCode, PRNo);
        }
        public async Task<List<PurchaseDetailResponse>> LoadPurchaseDetailReturnData(string branchCode, int ItemCode)
        {
            return await _repository.LoadPurchaseDetailReturnData(branchCode, ItemCode);
        }
        public async Task<List<PurchaseOrderRequestNumber>> GetPurchaseOrderNumberReturn(string branchCode)
        {
            return await _repository.GetPurchaseOrderNumberReturn(branchCode);
        }
        public async Task<int> SaveItemDamageAsync(ItemDamageSaveRequest request)
        {
            return await _repository.SaveItemDamageAsync(request);
        }
        public async Task<List<ItemDamageListResponselist>> GetPurchaseOrderItemDamageList(string branchCode, int damageNo)
        {
            return await _repository.GetPurchaseOrderItemDamageList(branchCode, damageNo);
        }
        public async Task<List<ItemDetailsResponse>> GetItemDetailsIndentOrder(GetItemDetailsRequest request)
        {
            return await _repository.GetItemDetailsIndentOrder(request);
        }
        public async Task<List<IndentOrderListResponse>> GetIndentOrderListPrint(int ioNo, string branchCode)
        {
            return await _repository.GetIndentOrderListPrint(ioNo, branchCode);
        }
        public async Task<List<IndentOrderListResponse>> GetIndentOrderList(string branchCode)
        {
            return await _repository.GetIndentOrderList(branchCode);
        }
        public async Task<int> SaveIndentOrderAsync(IndentOrderSaveRequest request)
        {
            return await _repository.SaveIndentOrderAsync(request);
        }
        public async Task<int> IndentOrderApprovalSave(IndentOrderApprovalSaveRequest request)
        {
            return await _repository.IndentOrderApprovalSave(request);
        }
        public async Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalPrintList(string branchCode, int ioNo)
        {
            return await _repository.GetIndentOrderApprovalPrintList(branchCode, ioNo);
        }
        public async Task<List<IndentOrderSearchResponse>> SearchIndentOrderAsync(string branchCode)
        {
            return await _repository.SearchIndentOrderAsync(branchCode);
        }
        public async Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalData(string branchCode, int ioNo)
        {
            return await _repository.GetIndentOrderApprovalData(branchCode, ioNo);
        }
        public async Task<int> ItemIssueSave(ItemIssueSaveRequest request)
        {
            return await _repository.ItemIssueSave(request);
        }
        public async Task<List<ItemIssueListDto>> GetItemIssuePrintData(string branchCode, int iNo)
        {
            return await _repository.GetItemIssuePrintData(branchCode, iNo);
        }
        public async Task<List<ItemIssueReturnNumber>> GetItemIssueNumber(string branchCode)
        {
            return await _repository.GetItemIssueNumber(branchCode);
        }
        public async Task<List<ItemIssueListDto>> GetItemIssueData(string branchCode,int ItemNo)
        {
            return await _repository.GetItemIssueData(branchCode, ItemNo);
        }
        public async Task<int> ItemIssueReturnSave(ItemIssueReturnSaveRequest request)
        {
            return await _repository.ItemIssueReturnSave(request);
        }
        public async Task<List<ItemIssueReturnListDto>> GetItemIssueReturnPrintData(string branchCode, int IRNo)
        {
            return await _repository.GetItemIssueReturnPrintData(branchCode, IRNo);
        }
        public async Task<List<ItemOpeningStock>> GetOpeningStockListAsync(string branchCode, int storeId)
        {
            return await _repository.GetOpeningStockListAsync(branchCode, storeId);
        }
        public async Task<int> SaveOpeningStockAsync(ItemOpeningStock request)
        {
            return await _repository.SaveOpeningStockAsync(request);
        }
        public async Task<bool> UpdateOpeningStockAsync(ItemOpeningStock request)
        {
            return await _repository.UpdateOpeningStockAsync(request);
        }
        public async Task<bool> DeleteOpeningStockAsync(int openingStockId, int modifiedBy)
        {
            return await _repository.DeleteOpeningStockAsync(openingStockId, modifiedBy);
        }
        public async Task<List<StockReportResponse>> GetStockReportAsync( StockReportRequest request)
        {
            return await _repository.GetStockReportAsync(request);
        }
    }
}
