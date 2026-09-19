using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office.PowerPoint.Y2022.M08.Main;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using Microsoft.Data.SqlClient;
using QuestPDF.Infrastructure;
using System.Data;
using System.Net.NetworkInformation;
using System.Transactions;

namespace HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra
{
    public class InventoryPurchaseDAL : IInventoryPurchase_Repository
    {
        private readonly DbConnectionFactory _factory;

        public InventoryPurchaseDAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }
        #region Common 
        public async Task<List<PurchaseExtraChargeModel>> GetExtraCharges(string itemCode, string branchCode, int storeId)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = "SELECT E.ChargeCode,E.ChargeName,E.ItemCode,E.Branch_Code,E.LastModify,E.StoreId," +
            "T.TaxDescId,T.TaxCode,T.TaxDescription,T.TaxPercentage,T.IsActive,T.UserCode,T.BranchCode,T.CreatedOn " +
            "FROM InventoryExtraCharges E INNER JOIN BillTaxDescription T ON E.ChargeCode = T.TaxCode " +
            "AND E.Branch_Code = T.BranchCode " +
            "WHERE E.ItemCode = @ItemCode AND E.Branch_Code = @BranchCode AND E.StoreId = @StoreId " +
            "AND T.BranchCode = @BranchCode AND T.IsActive = 1 " +
            "ORDER BY T.TaxDescId;";

            var result = await connection.QueryAsync<PurchaseExtraChargeModel>(sql, new
            { ItemCode = itemCode, BranchCode = branchCode, StoreId = storeId });
            return result.ToList();
        }
        public async Task<IEnumerable<BillTaxDescription>> GetMiscChargeTax(int chargeCode, string taxCode, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string sql = "SELECT T.TaxDescId,T.TaxCode,T.TaxDescription,T.TaxPercentage,T.IsActive," +
            "T.UserCode,T.BranchCode,T.CreatedOn FROM MiscCharges M " +
            "INNER JOIN BillTaxDescription T ON M.TaxCode = T.TaxCode AND M.Branch_Code = T.BranchCode " +
            "WHERE M.ChargeId = @ChargeId AND M.TaxCode = @TaxCode AND M.Branch_Code = @BranchCode " +
            "AND T.IsActive = 1";

            return await connection.QueryAsync<BillTaxDescription>(sql,
            new { ChargeId = chargeCode, TaxCode = taxCode, BranchCode = branchCode });
        }
        public async Task<IEnumerable<InventoryMasterItemModel>> ItemStoreGetListByStoreId(string branchcode, string Storeid)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string selectQuery = "SELECT im.UnitName,im.PurchaseRate,im.BarCode,im.Picture,im.UserCode," +
            " im.ItemCode,im.ItemName,im.CatCode,im.SubCatCode,im.Storeid,im.GrpCode,im.UnitCode," +
            " im.NoofUnits,im.ItemRate,im.ItemOpStock,im.ItemOpRate,im.ItemROQ,im.ItemROL," +
            " im.LastModify,im.mostrunningitemsrno AS MostRunningItemSrNo,im.Branch_Code," +
            " ec.ChargeCode AS TaxCode,ec.ChargeName AS TaxName," +
            " iud.UUNIT AS FirstUnit,iud.UDESC AS FirstUnitDesc,iud.LUNIT AS FinalUnit,iud.LDESC AS FinalUnitDesc" +
            " FROM dbo.InventoryItemMaster im" +
            " OUTER APPLY (SELECT TOP 1 ChargeCode,ChargeName FROM InventoryExtraCharges ec " +
            " WHERE ec.ItemCode = im.ItemCode AND ec.Branch_Code = im.Branch_Code) ec" +
            " LEFT JOIN dbo.ITEMUNITSDESC iud ON iud.ITCODE = im.ItemCode AND iud.BRANCH_CODE = im.Branch_Code" +
            " WHERE im.Branch_Code = @Branchcode AND EXISTS(SELECT 1" +
            " FROM STRING_SPLIT(CAST(im.Storeid AS varchar(max)), ',') s" +
            " INNER JOIN STRING_SPLIT(@Storeid, ',') requested" +
            " ON LTRIM(RTRIM(s.value)) = LTRIM(RTRIM(requested.value))) ORDER BY im.ItemCode;";
            return await connection.QueryAsync<InventoryMasterItemModel>(selectQuery,
                new
                {
                    Branchcode = branchcode,
                    Storeid = Storeid
                }
            );
        }
        #endregion

        #region Purchase Order
        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
             SELECT p.PONo,p.PODate,p.SupCode,p.Billed,p.Branch_Code,p.OrderBy,p.EffectiveFrom,
             p.EffectiveTo,p.Instruction,p.Remarks,p.Totalamount AS TotalAmount,p.TaxAmount,
             p.MissChargeAmount,p.GrossAmount,p.StoreId,p.Deliverydate,p.status,
             p.POValidDate,p.CgstAmount,p.SgstAmount,CAST(0 AS BIT) AS IsApproved,NULL AS ApprovedBy,
             NULL AS ApprovedDate FROM PurchaseOrderMaster p
             WHERE p.Branch_Code = @BranchCode AND p.IsDeleted = 0
             AND NOT EXISTS(SELECT 1 FROM ApprovalPurchaseOrderMaster ap
             WHERE ap.PONo = p.PONo AND ap.Branch_Code = p.Branch_Code)
             UNION ALL
             SELECT ap.PONo,ap.PODate,ap.SupCode,ap.Billed,ap.Branch_Code,ap.OrderBy,ap.EffectiveFrom,
             ap.EffectiveTo,ap.Instruction,ap.Remarks,ap.Totalamount AS TotalAmount,
             ap.TaxAmount,ap.MissChargeAmount,ap.GrossAmount,ap.StoreId,
             ap.Deliverydate,ap.status,ap.POValidDate,ap.CgstAmount,
             ap.SgstAmount,CAST(1 AS BIT) AS IsApproved,
             ap.ApprovedBy,ap.ApprovedDate
             FROM ApprovalPurchaseOrderMaster ap 
             WHERE ap.Branch_Code = @BranchCode ORDER BY PONo DESC;

             SELECT pd.PONo,im.ItemCode, pd.POItemQty,pd.POItemRate, pd.Branch_Code, 
             pd.UnitCode,pd.unit AS Unit, pd.POItemSuplyQty, pd.CPOItemQty, im.TaxCode, 
             im.TaxName,pd.MainUnit,pd.MainUnitConverstion FROM PurchaseOrderDetail pd
             LEFT JOIN InventoryItemMaster im ON pd.ItemCode = im.ItemCode
             WHERE pd.Branch_Code = @BranchCode
             AND NOT EXISTS(
             SELECT 1 FROM ApprovalPurchaseOrderMaster ap WHERE ap.PONo = pd.PONo
             AND ap.Branch_Code = pd.Branch_Code)
             UNION ALL
             SELECT pd.PONo,im.ItemCode,pd.POItemQty,pd.POItemRate,pd.Branch_Code,pd.UnitCode,pd.unit AS Unit,
             pd.POItemSuplyQty,pd.CPOItemQty,im.TaxCode,im.TaxName,pd.MainUnit,pd.MainUnitConverstion
             FROM ApprovalPurchaseOrderDetail pd
             LEFT JOIN InventoryItemMaster im ON pd.ItemCode = im.ItemCode
             WHERE pd.Branch_Code = @BranchCode;

             SELECT DISTINCT tax.Pno,tax.ItemCode,tax.TaxCode,tax.TaxPer,tax.TaxAmount,tax.Branch_Code,
             im.ItemName,bill.TaxName AS TaxDescription,bill.TaxPercentage
             FROM PurchaseOrderTax tax
             INNER JOIN InventoryItemMaster im ON tax.ItemCode = im.ItemCode
             INNER JOIN BillTaxMaster bill ON bill.TaxCode = tax.TaxCode
             WHERE tax.Branch_Code = @BranchCode
             AND NOT EXISTS(SELECT 1 FROM ApprovalPurchaseOrderMaster ap
             WHERE ap.PONo = tax.Pno AND ap.Branch_Code = tax.Branch_Code)
             UNION ALL
             SELECT DISTINCT tax.Pno,tax.ItemCode,tax.TaxCode,tax.TaxPer,tax.TaxAmount,tax.Branch_Code,
             im.ItemName,bill.TaxName AS TaxDescription,bill.TaxPercentage
             FROM ApprovalPurchaseOrderTax tax
             INNER JOIN InventoryItemMaster im ON tax.ItemCode = im.ItemCode
             INNER JOIN BillTaxMaster bill ON bill.TaxCode = tax.TaxCode
             WHERE tax.Branch_Code = @BranchCode;

             SELECT DISTINCT pmc.ChargeId,pmc.ChargeAmt,pmc.Branch_Code,pmc.Pno,tax.TaxCode,
             tax.TaxName AS TaxDescription,tax.TaxPercentage,mc.ChargeName
             FROM Tbl_POMISC pmc
             LEFT JOIN BillTaxMaster tax ON pmc.TaxCode = tax.TaxCode
             LEFT JOIN MiscCharges mc ON pmc.ChargeId = mc.ChargeId
             WHERE pmc.Branch_Code = @BranchCode
             AND NOT EXISTS (SELECT 1 FROM ApprovalPurchaseOrderMaster ap 
             WHERE ap.PONo = pmc.Pno AND ap.Branch_Code = pmc.Branch_Code)
             UNION ALL
             SELECT DISTINCT pmc.ChargeId,pmc.ChargeAmt,pmc.Branch_Code,pmc.Pno,tax.TaxCode,
             tax.TaxName AS TaxDescription,tax.TaxPercentage,mc.ChargeName
             FROM ApprovalTbl_POMISC pmc
             LEFT JOIN BillTaxMaster tax ON pmc.TaxCode = tax.TaxCode
             LEFT JOIN MiscCharges mc ON pmc.ChargeId = mc.ChargeId
             WHERE pmc.Branch_Code = @BranchCode;";

            using var multi = await connection.QueryMultipleAsync(query,
                new
                {
                    BranchCode = branchCode
                });
            var masters =(await multi.ReadAsync<PurchaseOrderMasterModel>()).ToList();
            var details =(await multi.ReadAsync<PurchaseOrderDetailModel>()).ToList();
            var taxes =(await multi.ReadAsync<PurchaseOrderTaxModel>()).ToList();
            var miscellaneous =(await multi.ReadAsync<PurchaseOrderMiscModel>()).ToList();
            var result = masters.Select(master => new PurchaseOrderListResponse
            {
                Master = master,
                Details = details.Where(x =>x.PONo == master.PONo &&x.Branch_Code == master.Branch_Code).ToList(),
                Taxes = taxes.Where(x =>x.Pno == master.PONo &&x.Branch_Code == master.Branch_Code).ToList(),
                Miscellaneous = miscellaneous.Where(x =>x.Pno == master.PONo &&x.Branch_Code == master.Branch_Code).ToList()

            }).ToList();

            return result;
        }
        public async Task<int> CreatePurchaseOrder(InventoryPurchase request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                string branchCode = request.Master.Branch_Code;
                int poNo = request.Master.PONo;

                const string checkPoQuery = @"SELECT COUNT(1) FROM PurchaseOrderMaster
                WHERE PONo = @PONo AND Branch_Code = @BranchCode;";

                int poExists = await connection.ExecuteScalarAsync<int>(checkPoQuery,
                    new
                    {
                        PONo = poNo,
                        BranchCode = branchCode
                    },transaction);

                if (poExists > 0)
                {
                    const string nextPoQuery = @"SELECT ISNULL(MAX(PONo), 0) + 1 FROM PurchaseOrderMaster
                    WHERE Branch_Code = @BranchCode;";

                    poNo = await connection.ExecuteScalarAsync<int>(nextPoQuery,
                           new{BranchCode = branchCode},transaction);
                    request.Master.PONo = poNo;
                }

                const string masterQuery = @"
                INSERT INTO PurchaseOrderMaster
                (
                    PONo,
                    PODate,
                    SupCode,
                    Billed,
                    Branch_Code,
                    OrderBy,
                    EffectiveFrom,
                    EffectiveTo,
                    Instruction,
                    Remarks,
                    Totalamount,
                    MissChargeAmount,
                    TaxAmount,
                    GrossAmount,
                    StoreId,
                    Deliverydate,
                    POValidDate,
                    status,
                    CgstAmount,
                    SgstAmount

                )
                VALUES
                (
                    @PONo,
                    @PODate,
                    @SupCode,
                    @Billed,
                    @Branch_Code,
                    @OrderBy,
                    @EffectiveFrom,
                    @EffectiveTo,
                    @Instruction,
                    @Remarks,
                    @TotalAmount,
                    @MissChargeAmount,
                    @TaxAmount,
                    @GrossAmount,
                    @StoreId,
                    @Deliverydate,
                    @POValidDate,
                    @status,
                    @CgstAmount,
                    @SgstAmount

                );";

                await connection.ExecuteAsync(masterQuery, request.Master,transaction);

                const string detailQuery = @"
                INSERT INTO PurchaseOrderDetail
                (
                    PONo,
                    ItemCode,
                    POItemQty,
                    POItemRate,
                    Branch_Code,
                    unit,
                    POItemSuplyQty,
                    CPOItemQty,
                    UnitCode,
                    MainUnitConverstion,
                    MainUnit
                )
                VALUES
                (
                    @PONo,
                    @ItemCode,
                    @POItemQty,
                    @POItemRate,
                    @Branch_Code,
                    @Unit,
                    @POItemSuplyQty,
                    @CPOItemQty,
                    @UnitCode,
                    @MainUnitConverstion,
                    @MainUnit
                );";

                foreach (var detail in request.Details)
                {
                    detail.PONo = poNo;
                    detail.Branch_Code = branchCode;
                    await connection.ExecuteAsync( detailQuery,detail,transaction);
                }

                if (request.Taxes != null && request.Taxes.Count > 0)
                {
                    const string taxQuery = @"
                    INSERT INTO PurchaseOrderTax
                    (
                        Pno,
                        ItemCode,
                        TaxCode,
                        TaxPer,
                        TaxAmount,
                        Branch_Code
                    )
                    VALUES
                    (
                        @Pno,
                        @ItemCode,
                        @TaxCode,
                        @TaxPer,
                        @TaxAmount,
                        @Branch_Code
                    );";

                    foreach (var tax in request.Taxes)
                    {
                        tax.Pno = poNo;
                        tax.Branch_Code = branchCode;
                        tax.TaxPer = Math.Round(tax.TaxPer, 2);
                        tax.TaxAmount = Math.Round(tax.TaxAmount, 2);

                        await connection.ExecuteAsync(taxQuery,tax,transaction);
                    }
                }

                if (request.Miscellaneous != null && request.Miscellaneous.Count > 0)
                {
                    const string miscQuery = @"
                    INSERT INTO Tbl_POMISC
                    (
                        ChargeId,
                        ChargeAmt,
                        Branch_Code,
                        Pno,
                        TaxCode
                    )
                    VALUES
                    (
                        @ChargeId,
                        @ChargeAmt,
                        @Branch_Code,
                        @Pno,
                        @TaxCode
                    );";

                    foreach (var misc in request.Miscellaneous)
                    {
                        misc.Pno = poNo;
                        misc.Branch_Code = branchCode;
                        misc.TaxCode = misc.TaxCode;
                        await connection.ExecuteAsync(miscQuery,misc,transaction);
                    }
                }

                transaction.Commit();
                return poNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<bool> DeletePurchaseOrder(int poNo, string branchCode, string reasonDelete)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string deleteMasterQuery = @"UPDATE PurchaseOrderMaster
                SET  IsDeleted = 1, ReasonDelete = @ReasonDelete, status='R'
                WHERE PONo = @PONo AND Branch_Code = @BranchCode AND IsDeleted = 0;";
                int updated = await connection.ExecuteAsync(
                    deleteMasterQuery,
                    new
                    {
                        PONo = poNo,
                        BranchCode = branchCode,
                        ReasonDelete = reasonDelete
                    },
                    transaction
                );

                if (updated > 0)
                {
                    transaction.Commit();
                    return true;
                }
                transaction.Rollback();
                return false;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<PurchaseOrderPrintResponse?> GetPurchaseOrderForPrint(int poNo, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            const string masterQuery = @"SELECT s.SupName AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,p.PONo, p.PODate,ISNULL(p.OrderBy, '') AS OrderBy,
            p.EffectiveFrom,p.EffectiveTo,ISNULL(p.Instruction, '') AS Instruction,
            ISNULL(p.Remarks, '') AS Remarks,p.Branch_Code,ISNULL(p.Totalamount, 0) AS TotalAmount,
            ISNULL(p.TaxAmount, 0) AS Tax,ISNULL(p.GrossAmount, 0) AS GrossAmount,
            ISNULL(p.MissChargeAmount, 0) AS MissChargeAmount 
            FROM PurchaseOrderMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE p.PONo = @PONo AND p.Branch_Code = @BranchCode and IsDeleted=0 ;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PONo,d.ItemCode,ISNULL(i.ItemName, '') AS ItemName,ISNULL(d.unit, '') AS Unit,
            ISNULL(d.POItemRate, 0) AS ItemRate,ISNULL(d.POItemQty, 0) AS ItemQty,
            ISNULL(d.POItemQty, 0) * ISNULL(d.POItemRate, 0) AS Total,
            ISNULL(i.TaxCode, 0) AS TaxCode,ISNULL(i.TaxName, '') AS TaxName,
            d.MainUnit,d.MainUnitConverstion,d.Branch_Code FROM PurchaseOrderDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.PONo = @PONo AND d.Branch_Code = @BranchCode
            ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,PO.TaxPer,
            PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage from PurchaseOrderTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Pno = @Pno AND PO.Branch_Code = @Branch_Code
            ORDER BY PO.Pno;";

            const string termsQuery = @"SELECT TermsCode,TermsTitle,MasterName,TermsDescription,Branch_Code 
            FROM TermsAndConditionsMaster 
            WHERE Branch_Code = @Branch_Code ORDER BY TermsCode;";

            var master = await connection.QueryFirstOrDefaultAsync<PurchaseOrderPrintMaster>(
                masterQuery,
                new
                {
                    PONo = poNo,
                    BranchCode = branchCode
                });

            if (master == null)
                return null;

            var details = (await connection.QueryAsync<PurchaseOrderPrintDetail>(
             detailQuery,
             new
             {
                 PONo = poNo,
                 BranchCode = branchCode
             })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseOrderTaxDetail>(
            taxQuery,
            new
            {
                Pno = poNo,
                Branch_Code = branchCode
            })).ToList();

            var termsDetails = (await connection.QueryAsync<TermsAndConditionsMaster>(
                termsQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            return new PurchaseOrderPrintResponse
            {
                Master = master,
                Details = details,
                TaxDetails = taxDetails,
                TermsMaster = termsDetails
            };
        }
        #endregion

        #region Purchase Order Approval
        public async Task<bool> CreatePurchaseOrderApproval(InventoryPurchase request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Master == null)
                throw new ArgumentException("Purchase order master details are required.");

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                int poNo = request.Master.PONo;
                string branchCode = request.Master.Branch_Code;

                const string deletemasterQuery = @"DELETE FROM ApprovalPurchaseOrderMaster
                 WHERE PONo = @PONo AND Branch_Code = @BranchCode;";
                await connection.ExecuteAsync(deletemasterQuery,
                new{PONo = poNo,BranchCode = branchCode},transaction);

                const string masterQuery = @"
                INSERT INTO ApprovalPurchaseOrderMaster
                (
                    PONo,
                    PODate,
                    SupCode,
                    Billed,
                    Branch_Code,
                    OrderBy,
                    EffectiveFrom,
                    EffectiveTo,
                    Instruction,
                    Remarks,
                    Totalamount,
                    MissChargeAmount,
                    TaxAmount,
                    GrossAmount,
                    StoreId,
                    Deliverydate,
                    POValidDate,
                    status,
                    CgstAmount,
                    SgstAmount,
                    ApprovedDate,
                    ApprovedBy

                )
                VALUES
                (
                    @PONo,
                    @PODate,
                    @SupCode,
                    @Billed,
                    @Branch_Code,
                    @OrderBy,
                    @EffectiveFrom,
                    @EffectiveTo,
                    @Instruction,
                    @Remarks,
                    @TotalAmount,
                    @MissChargeAmount,
                    @TaxAmount,
                    @GrossAmount,
                    @StoreId,
                    @Deliverydate,
                    @POValidDate,
                    @status,
                    @CgstAmount,
                    @SgstAmount,
                    @ApprovedDate,
                    @ApprovedBy
                );";
                await connection.ExecuteAsync(masterQuery, request.Master, transaction);

                const string deleteDetailQuery = @"DELETE FROM ApprovalPurchaseOrderDetail
                 WHERE PONo = @PONo AND Branch_Code = @BranchCode;";

                await connection.ExecuteAsync(deleteDetailQuery,
                new{PONo = poNo,BranchCode = branchCode},transaction);

                const string detailQuery = @"
                    INSERT INTO ApprovalPurchaseOrderDetail
                    (
                        PONo,
                        ItemCode,
                        POItemQty,
                        POItemRate,
                        Branch_Code,
                        unit,
                        POItemSuplyQty,
                        CPOItemQty,
                        ApprovedBy,
                        ApprovedDate,
                        POOrderQty,
                        UnitCode,
                        FinalApprovedQty,
                        MainUnitConverstion,
                        MainUnit
                    )
                    VALUES
                    (
                        @PONo,
                        @ItemCode,
                        @POItemQty,
                        @POItemRate,
                        @Branch_Code,
                        @Unit,
                        @POItemSuplyQty,
                        @CPOItemQty,
                        @ApprovedBy,
                        @ApprovedDate,
                        @POOrderQty,
                        @UnitCode,
                        @FinalApprovedQty,
                        @MainUnitConverstion,
                        @MainUnit
                    );";

                if (request.Details != null && request.Details.Count > 0)
                {
                    foreach (var detail in request.Details)
                    {
                        detail.PONo = poNo;
                        detail.Branch_Code = branchCode;
                        detail.FinalApprovedQty = detail.POItemQty;
                        const string getPOItemQtyQuery = @"SELECT POItemQty,POItemRate FROM PurchaseOrderDetail
                        WHERE PONo = @PONo AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                        var poItem = await connection.QuerySingleOrDefaultAsync<dynamic>(
                          getPOItemQtyQuery,
                          new
                          {
                              PONo = detail.PONo,
                              ItemCode = detail.ItemCode,
                              Branch_Code = detail.Branch_Code
                          },
                          transaction
                        );
                        detail.POOrderQty = Convert.ToDecimal(poItem?.POItemQty ?? 0f);
                        await connection.ExecuteAsync(detailQuery,detail,transaction);
                    }
                }

                const string deleteTaxQuery = @"DELETE FROM ApprovalPurchaseOrderTax
                 WHERE Pno = @PONo AND Branch_Code = @BranchCode;";

                await connection.ExecuteAsync(
                    deleteTaxQuery,
                    new
                    {
                        PONo = poNo,
                        BranchCode = branchCode
                    },
                    transaction
                );

                const string taxQuery = @"
                INSERT INTO ApprovalPurchaseOrderTax
                (
                    Pno,
                    ItemCode,
                    TaxCode,
                    TaxPer,
                    TaxAmount,
                    Branch_Code
                )
                VALUES
                (
                    @Pno,
                    @ItemCode,
                    @TaxCode,
                    @TaxPer,
                    @TaxAmount,
                    @Branch_Code
                );";

                if (request.Taxes != null && request.Taxes.Count > 0)
                {
                    foreach (var tax in request.Taxes)
                    {
                        tax.Pno = poNo;
                        tax.Branch_Code = branchCode;
                        tax.TaxPer = Math.Round(tax.TaxPer, 2);
                        tax.TaxAmount = Math.Round(tax.TaxAmount, 2);

                        await connection.ExecuteAsync(taxQuery,tax,transaction);
                    }
                }

                const string deleteMiscQuery = @"DELETE FROM ApprovalTbl_POMISC 
                WHERE Pno = @PONo AND Branch_Code = @BranchCode;";

                await connection.ExecuteAsync(
                    deleteMiscQuery,
                    new
                    {
                        PONo = poNo,
                        BranchCode = branchCode
                    },
                    transaction
                );

                const string miscQuery = @"
                INSERT INTO ApprovalTbl_POMISC
                (
                    ChargeId,
                    ChargeAmt,
                    Branch_Code,
                    Pno,
                    TaxCode
                )
                VALUES
                (
                    @ChargeId,
                    @ChargeAmt,
                    @Branch_Code,
                    @Pno,
                    @TaxCode
                );";

                if (request.Miscellaneous != null && request.Miscellaneous.Count > 0)
                {
                    foreach (var misc in request.Miscellaneous)
                    {
                        misc.Pno = poNo;
                        misc.Branch_Code = branchCode;

                        await connection.ExecuteAsync(miscQuery,misc,transaction);
                    }
                }

                transaction.Commit();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderApprovalList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT PONo,PODate,SupCode,Billed,Branch_Code,OrderBy,EffectiveFrom,EffectiveTo,
            Instruction,Remarks,Totalamount AS TotalAmount,TaxAmount,MissChargeAmount,GrossAmount,StoreId,Deliverydate,
            status,POValidDate,CgstAmount,SgstAmount,ApprovedBy,ApprovedDate
            FROM ApprovalPurchaseOrderMaster WHERE Branch_Code = @BranchCode AND status = 'A' ORDER BY PONo DESC;

            SELECT pd.PONo,im.ItemCode,pd.POItemQty,pd.POItemRate,pd.Branch_Code,pd.UnitCode,pd.unit AS Unit,
            pd.POItemSuplyQty,pd.CPOItemQty,im.TaxCode,im.TaxName,ApprovedBy,POOrderQty,ApprovedDate,
            pd.MainUnit,pd.MainUnitConverstion
            FROM ApprovalPurchaseOrderDetail pd
            left join InventoryItemMaster im on pd.ItemCode=im.ItemCode
            WHERE pd.Branch_Code = @BranchCode ORDER BY PONo DESC;

            SELECT DISTINCT tax.Pno,tax.ItemCode,tax.TaxCode,tax.TaxPer,tax.TaxAmount,
            tax.Branch_Code,im.ItemName,bill.TaxName as TaxDescription,bill.TaxPercentage
            FROM ApprovalPurchaseOrderTax tax
            INNER JOIN InventoryItemMaster im ON tax.ItemCode = im.ItemCode
            INNER JOIN BillTaxMaster bill ON bill.TaxCode = tax.TaxCode
            WHERE tax.Branch_Code = @BranchCode ORDER BY tax.Pno DESC;

            select DISTINCT pmc.ChargeId,pmc.ChargeAmt,pmc.Branch_Code,pmc.Pno,tax.TaxCode,
            tax.TaxName as TaxDescription,tax.TaxPercentage,mc.ChargeName 
            from ApprovalTbl_POMISC pmc
            left join BillTaxMaster tax on pmc.TaxCode=tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId=mc.ChargeId
            WHERE pmc.Branch_Code = @BranchCode ORDER BY Pno DESC;";

            using var multi = await connection.QueryMultipleAsync(query, new { BranchCode = branchCode });

            var masters = (await multi.ReadAsync<PurchaseOrderMasterModel>()).ToList();

            var details = (await multi.ReadAsync<PurchaseOrderDetailModel>()).ToList();

            var taxes = (await multi.ReadAsync<PurchaseOrderTaxModel>()).ToList();

            var miscellaneous = (await multi.ReadAsync<PurchaseOrderMiscModel>()).ToList();

            var result = masters.Select(master => new PurchaseOrderListResponse
            {
                Master = master,
                Details = details.Where(x => x.PONo == master.PONo && x.Branch_Code == master.Branch_Code).ToList(),
                Taxes = taxes.Where(x => x.Pno == master.PONo && x.Branch_Code == master.Branch_Code).ToList(),
                Miscellaneous = miscellaneous.Where(x => x.Pno == master.PONo && x.Branch_Code == master.Branch_Code).ToList()
            }).ToList();

            return result;
        }
        public async Task<PurchaseOrderPrintResponse?> GetPurchaseOrderApprovalPrint(int poNo, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            const string masterQuery = @"SELECT s.SupName AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,p.PONo, p.PODate,ISNULL(p.OrderBy, '') AS OrderBy,
            p.EffectiveFrom,p.EffectiveTo,ISNULL(p.Instruction, '') AS Instruction,
            ISNULL(p.Remarks, '') AS Remarks,p.Branch_Code,ISNULL(p.Totalamount, 0) AS TotalAmount,
            ISNULL(p.TaxAmount, 0) AS Tax,ISNULL(p.GrossAmount, 0) AS GrossAmount,
            ISNULL(p.MissChargeAmount, 0) AS MissChargeAmount,ISNULL(p.ApprovedBy, '') AS ApprovedBy,
            ISNULL(p.ApprovedDate, '') AS ApprovedDate
            FROM ApprovalPurchaseOrderMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE p.PONo = @PONo AND p.Branch_Code = @BranchCode;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PONo,d.ItemCode,ISNULL(i.ItemName, '') AS ItemName,ISNULL(d.unit, '') AS Unit,
            ISNULL(d.POItemRate, 0) AS ItemRate,ISNULL(d.POItemQty, 0) AS POOrderQty,
            ISNULL(d.POOrderQty, 0) AS ItemQty,d.ApprovedBy,d.ApprovedDate,
            ISNULL(d.POItemQty, 0) * ISNULL(d.POItemRate, 0) AS Total,d.Branch_Code,
            d.MainUnit,d.MainUnitConverstion,ISNULL(i.TaxCode, 0) AS TaxCode,ISNULL(i.TaxName, '') AS TaxName
            FROM ApprovalPurchaseOrderDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.PONo = @PONo AND d.Branch_Code = @BranchCode
            ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,PO.TaxPer,
            PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage from ApprovalPurchaseOrderTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Pno = @Pno AND PO.Branch_Code = @Branch_Code
            ORDER BY PO.Pno;";

            const string termsQuery = @"SELECT TermsCode,TermsTitle,TermsDescription,MasterName,Branch_Code 
            FROM TermsAndConditionsMaster 
            WHERE Branch_Code = @Branch_Code ORDER BY TermsCode;";

            var master = await connection.QueryFirstOrDefaultAsync<PurchaseOrderPrintMaster>(
                masterQuery,
                new
                {
                    PONo = poNo,
                    BranchCode = branchCode
                });

            if (master == null)
            {
                return null;
            }

            var details = (await connection.QueryAsync<PurchaseOrderPrintDetail>(
                detailQuery,
                new
                {
                    PONo = poNo,
                    BranchCode = branchCode
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseOrderTaxDetail>(
                taxQuery,
                new
                {
                    Pno = poNo,
                    Branch_Code = branchCode
                })).ToList();

            var termsDetails = (await connection.QueryAsync<TermsAndConditionsMaster>(
                termsQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            return new PurchaseOrderPrintResponse
            {
                Master = master,
                Details = details,
                TaxDetails = taxDetails,
                TermsMaster = termsDetails
            };
        }
        #endregion

        #region Goods Recived Note
        public async Task<List<PurchaseOrderListResponse>> GetPurchaseOrderGRNList(int poNo,string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT PONo,PODate,SupCode,Billed,Branch_Code,OrderBy,
            EffectiveFrom,EffectiveTo,Instruction,Remarks,Totalamount AS TotalAmount,TaxAmount,
            MissChargeAmount,GrossAmount,StoreId,Deliverydate,status,POValidDate,CgstAmount,
            SgstAmount,ApprovedBy,ApprovedDate
            FROM ApprovalPurchaseOrderMaster
            WHERE Branch_Code = @BranchCode AND PONo = @PONo ORDER BY PONo DESC;

            SELECT pd.PONo,im.ItemCode,im.ItemName,ISNULL(pd.POItemQty, 0) AS POItemQty,
            0 AS ReceivedQty,ISNULL(pd.BalanceQty, 0) AS BalanceQty,
            pd.POItemRate,pd.Branch_Code,pd.UnitCode,pd.unit AS Unit,im.TaxCode,im.TaxName,
            pd.ApprovedBy,pd.POOrderQty,pd.ApprovedDate,pd.MainUnit,pd.MainUnitConverstion
            FROM ApprovalPurchaseOrderDetail pd
            LEFT JOIN InventoryItemMaster im ON pd.ItemCode = im.ItemCode
            WHERE pd.Branch_Code = @BranchCode AND pd.PONo = @PONo AND ISNULL(pd.POItemQty, 0) > 0
            ORDER BY pd.PONo DESC;

            SELECT DISTINCT tax.Pno,tax.ItemCode,tax.TaxCode,tax.TaxPer,tax.TaxAmount,
            tax.Branch_Code,im.ItemName,bill.TaxName AS TaxDescription,bill.TaxPercentage
            FROM ApprovalPurchaseOrderTax tax
            INNER JOIN InventoryItemMaster im ON tax.ItemCode = im.ItemCode
            INNER JOIN BillTaxMaster bill ON bill.TaxCode = tax.TaxCode
            WHERE tax.Branch_Code = @BranchCode AND tax.Pno = @PONo
            ORDER BY tax.Pno DESC;

            SELECT DISTINCT pmc.ChargeId,pmc.ChargeAmt,pmc.Branch_Code,pmc.Pno,
            tax.TaxCode,tax.TaxName AS TaxDescription,tax.TaxPercentage,mc.ChargeName
            FROM ApprovalTbl_POMISC pmc
            LEFT JOIN BillTaxMaster tax ON pmc.TaxCode = tax.TaxCode
            LEFT JOIN MiscCharges mc ON pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @BranchCode AND pmc.Pno = @PONo
            AND NOT EXISTS (SELECT 1 FROM GRNMISC gm WHERE gm.ChargeId = pmc.ChargeId 
            AND gm.Branch_Code = pmc.Branch_Code AND gm.Pno = pmc.Pno)
            UNION ALL
            SELECT DISTINCT gm.ChargeId,gm.ChargeAmt,gm.Branch_Code,gm.Pno,tax.TaxCode,
            tax.TaxName AS TaxDescription,tax.TaxPercentage,mc.ChargeName
            FROM GRNMISC gm
            LEFT JOIN BillTaxMaster tax ON gm.TaxCode = tax.TaxCode
            LEFT JOIN MiscCharges mc ON gm.ChargeId = mc.ChargeId
            WHERE gm.Branch_Code = @BranchCode AND gm.Pno = @PONo
            ORDER BY Pno DESC;";

            using var multi = await connection.QueryMultipleAsync(query,
               new {BranchCode = branchCode,PONo = poNo});

            var masters =(await multi.ReadAsync<PurchaseOrderMasterModel>()).ToList();
            var details =(await multi.ReadAsync<PurchaseOrderDetailModel>()).ToList();
            var taxes =(await multi.ReadAsync<PurchaseOrderTaxModel>()).ToList();
            var miscellaneous =(await multi.ReadAsync<PurchaseOrderMiscModel>()).ToList();

            var result = masters.Select(master => new PurchaseOrderListResponse
              {
                 Master = master,
                 Details = details.Where(x =>x.PONo == master.PONo && x.Branch_Code == master.Branch_Code).ToList(),
                 Taxes = taxes.Where(x => x.Pno == master.PONo && x.Branch_Code == master.Branch_Code).ToList(),
                 Miscellaneous = miscellaneous.Where(x =>x.Pno == master.PONo && x.Branch_Code == master.Branch_Code).ToList()
              }).ToList();
            return result;
        }
        public async Task<List<PurchaseOrderPonoRequest>> GetPurchaseOrderNumber(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT DISTINCT M.PONo,M.Status
            FROM ApprovalPurchaseOrderMaster M
            INNER JOIN ApprovalPurchaseOrderDetail D ON D.PONo = M.PONo AND D.Branch_Code = M.Branch_Code
            WHERE M.Branch_Code = @BranchCode AND M.Status = 'A' or M.Status = 'G' 
            AND ISNULL(M.IsDeleted, 0) = 0 AND ISNULL(D.POItemQty, 0) > 0
            ORDER BY M.PONo DESC;";

            var result =await connection.QueryAsync<PurchaseOrderPonoRequest>(query,
            new{
                BranchCode = branchCode
            });
            return result.ToList();
        }
        public async Task<string> CreatePurchaseOrderGRN(InventoryPurchaseGRN request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            const string checkBillNoQuery = @"SELECT COUNT(1) FROM GRNMaster
            WHERE Billed = @BillNo AND Branch_Code = @Branch_Code;";

            int billNoExists = await connection.ExecuteScalarAsync<int>(
                checkBillNoQuery,
                new
                {
                    BillNo = request.BillNo,
                    Branch_Code = request.Branch_Code
                },
                transaction);

            if (billNoExists > 0)
            {
                throw new Exception($"Bill No already exists.");
            }

            try
            {
                const string insertGRNMaster = @"
                    INSERT INTO GRNMaster
                    (
                        PONo,
                        PODate,
                        SupCode,
                        Branch_Code,
                        OrderBy,
                        GRNNo,
                        GRNDate,
                        GRntime,
                        Receivedby,
                        userid,
                        ipadd,
                        Billed,
                        InspectedBy,
                        StoreId,
                        StoreName,
                        TotalAmount,
                        Tottax,
                        RoundOff,
                        NetAmount,
                        otherCharges,
                        MissChargeAmount,
                        CgstAmount,
                        SgstAmount,
                        status,
                        POValidDate
                    )
                    VALUES
                    (
                        @PONo,
                        @PODate,
                        @SupCode,
                        @Branch_Code,
                        @OrderBy,
                        @GRNNo,
                        @GRNDate,
                        @GRNTime,
                        @ReceivedBy,
                        @UserId,
                        @IpAddress,
                        @BillNo,
                        @InspectedBy,
                        @StoreId,
                        @StoreName,
                        @TotalAmount,
                        @TotalTax,
                        @RoundOff,
                        @NetAmount,
                        @OtherCharges,
                        @MissChargeAmount,
                        @CgstAmount,
                        @SgstAmount,
                        'G',
                        @POValidDate
                    );";

                await connection.ExecuteAsync(insertGRNMaster,
                    new
                    {
                        request.PONo,
                        request.PODate,
                        request.SupCode,
                        request.Branch_Code,
                        request.OrderBy,
                        request.GRNNo,
                        request.GRNDate,
                        request.GRNTime,
                        request.ReceivedBy,
                        request.UserId,
                        request.IpAddress,
                        request.BillNo,
                        request.InspectedBy,
                        request.StoreId,
                        request.StoreName,
                        request.TotalAmount,
                        request.TotalTax,
                        request.RoundOff,
                        request.NetAmount,
                        request.OtherCharges,
                        request.MissChargeAmount,
                        request.CgstAmount,
                        request.SgstAmount,
                        request.status,
                        request.POValidDate
                    },
                    transaction
                );

                if (request.Details != null && request.Details.Count > 0)
                {
                    foreach (var item in request.Details)
                    {
                        const string checkPOItem = @" SELECT COUNT(1) FROM ApprovalPurchaseOrderDetail
                        WHERE PONo = @PONo AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                        int poItemExists = await connection.ExecuteScalarAsync<int>(
                            checkPOItem,
                            new
                            {
                                PONo = request.PONo,
                                ItemCode = item.ItemCode,
                                Branch_Code = request.Branch_Code
                            },
                            transaction);

                        if (poItemExists <= 0)
                        {
                            throw new Exception($"PO item not found. " +$"PO: {request.PONo}, " +$"Item: {item.ItemCode}");
                        }
                        const string getPOQtyQuery = @"
                        SELECT
                            ISNULL(POItemQty, 0) AS POItemQty,
                            ISNULL(POItemRate, 0) AS POItemRate,
                            ISNULL(POItemSuplyQty, 0) AS POItemSuplyQty,
                            ISNULL(CPOItemQty, 0) AS CPOItemQty,
                            ISNULL(ReceivedQty, 0) AS ReceivedQty,
                            ISNULL(BalanceQty, 0) AS BalanceQty,
                            ISNULL(FinalApprovedQty, 0) AS FinalQty,
                            unit,UnitCode
                            FROM ApprovalPurchaseOrderDetail
                            WHERE PONo = @PONo AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                        var poQty = await connection.QueryFirstOrDefaultAsync<dynamic>(
                            getPOQtyQuery,
                            new
                            {
                                PONo = request.PONo,
                                ItemCode = item.ItemCode,
                                Branch_Code = request.Branch_Code
                            },
                            transaction);

                        if (poQty == null)
                        {
                            throw new Exception($"PO item not found. " +$"PO: {request.PONo}, " +$"Item: {item.ItemCode}");
                        }
                        decimal pendingQty =Convert.ToDecimal(poQty.POItemQty);
                        decimal FinalQty = Convert.ToDecimal(poQty.FinalQty);
                        decimal currentReceivedQty = Convert.ToDecimal(item.ReceivedQty);
                        if (currentReceivedQty <= 0)
                        {
                            continue;
                        }
                        if (currentReceivedQty > pendingQty)
                        {
                            throw new Exception(
                                $"Cannot receive more than pending quantity. " +
                                $"ItemCode: {item.ItemCode}, " +
                                $"Pending Qty: {pendingQty}, " +
                                $"Current Received: {currentReceivedQty}");
                        }

                        decimal newPendingQty =pendingQty - currentReceivedQty;
                        if (newPendingQty < 0)
                        {
                            newPendingQty = 0;
                        }
                        const string insertGRNDetail = @"
                            INSERT INTO GRNDETAILS
                            (
                                PONo,
                                ItemCode,
                                POItemQty,
                                POItemRate,
                                Branch_Code,
                                unit,
                                UnitCode,
                                POItemSuplyQty,
                                CPOItemQty,
                                BalanceQty,
                                ReceivedQty,
                                GRNNo,
                                FinalQty,
                                MainUnitConverstion,
                                MainUnit
                            )
                            VALUES
                            (
                                @PONo,
                                @ItemCode,
                                @POItemQty,
                                @POItemRate,
                                @Branch_Code,
                                @Unit,
                                @UnitCode,
                                @POItemSuplyQty,
                                @CPOItemQty,
                                @BalanceQty,
                                @ReceivedQty,
                                @GRNNo,
                                @FinalQty,
                                @MainUnitConverstion,
                                @MainUnit
                            );";

                        await connection.ExecuteAsync(
                            insertGRNDetail,
                            new
                            {
                                PONo = request.PONo,
                                ItemCode = item.ItemCode,
                                POItemQty = pendingQty,
                                POItemRate = item.POItemRate,
                                Branch_Code = request.Branch_Code,
                                Unit = item.Unit,
                                UnitCode = item.UnitCode,
                                POItemSuplyQty = currentReceivedQty,
                                CPOItemQty = newPendingQty,
                                ReceivedQty = currentReceivedQty,
                                BalanceQty = newPendingQty,
                                GRNNo = request.GRNNo,
                                FinalQty = FinalQty,
                                MainUnitConverstion = item.MainUnitConverstion,
                                MainUnit = item.MainUnit
                            },
                            transaction);

                        const string updatePODetail = @"
                            UPDATE ApprovalPurchaseOrderDetail
                            SET
                                POItemQty = @NewPendingQty,
                                POItemSuplyQty = @CurrentReceivedQty,
                                CPOItemQty = @NewPendingQty,
                                ReceivedQty = @CurrentReceivedQty,
                                BalanceQty = @NewPendingQty
                            WHERE PONo = @PONo AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                        await connection.ExecuteAsync(
                            updatePODetail,
                            new
                            {
                                PONo = request.PONo,
                                ItemCode = item.ItemCode,
                                Branch_Code = request.Branch_Code,
                                NewPendingQty = newPendingQty,
                                CurrentReceivedQty = currentReceivedQty
                            },
                            transaction);
                    }
                }

                if (request.Taxes != null && request.Taxes.Count > 0)
                {
                    const string insertTax = @"
                        INSERT INTO GRNTaxDescription
                        (
                            Pno,
                            GRNNo,
                            ItemCode,
                            TaxCode,
                            TaxPer,
                            TaxAmount,
                            Branch_Code
                        )
                        VALUES
                        (
                            @Pno,
                            @GRNNo,
                            @ItemCode,
                            @TaxCode,
                            @TaxPer,
                            @TaxAmount,
                            @Branch_Code
                        );";

                    foreach (var tax in request.Taxes)
                    {
                        await connection.ExecuteAsync(
                            insertTax,
                            new
                            {
                                Pno = request.PONo,
                                GRNNo = request.GRNNo,
                                ItemCode = tax.ItemCode,
                                TaxCode = tax.TaxCode,
                                TaxPer = tax.TaxPer,
                                TaxAmount = tax.TaxAmount,
                                Branch_Code = request.Branch_Code
                            },
                            transaction
                        );
                    }
                }

                if (request.Miscellaneous != null && request.Miscellaneous.Count > 0)
                {
                    const string insertMisc = @"
                        INSERT INTO GRNMISC
                        (
                            ChargeId,
                            ChargeAmt,
                            Branch_Code,
                            Pno,
                            GRNNo,
                            TaxCode
                        )
                        VALUES
                        (
                            @ChargeId,
                            @ChargeAmt,
                            @Branch_Code,
                            @Pno,
                            @GRNNo,
                            @TaxCode
                        );";

                    foreach (var misc in request.Miscellaneous)
                    {
                        await connection.ExecuteAsync(
                            insertMisc,
                            new
                            {
                                ChargeId = misc.ChargeId,
                                ChargeAmt = misc.ChargeAmt,
                                Branch_Code = request.Branch_Code,
                                Pno = request.PONo,
                                GRNNo = request.GRNNo,
                                TaxCode = misc.TaxCode
                            },
                            transaction
                        );
                    }
                }

                const string updateApprovalPOMaster = @"UPDATE ApprovalPurchaseOrderMaster 
                SET status = @Status WHERE PONo = @PONo AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    updateApprovalPOMaster,
                    new
                    {
                        Status = "G",
                        PONo = request.PONo,
                        Branch_Code = request.Branch_Code
                    },
                    transaction
                );
                transaction.Commit();
                 await CreatePurchaseFromGRN(request.GRNNo, request.Branch_Code);
                return request.GRNNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<GoodsReceivedNotePrintResponse?> GetGoodsReceivedNotePrint(int poNo, string GrnNo, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT s.SupName AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PONo,p.PODate,p.SupCode,p.POValidDate,p.Billed,p.Branch_Code,p.OrderBy,p.status,
		    p.GRNNo,p.GRNDate,p.GRNtime,p.Receivedby,p.InspectedBy,p.StoreID,p.StoreName,
		    p.userId,p.ipAdd,p.TotalAmount,p.TotTax,p.OtherCharges,p.Roundoff,p.NetAmount,
		    p.MissChargeAmount,p.CgstAmount,p.SgstAmount
            FROM GRNMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE p.PONo = @PONo  and p.GRNNo = @GrnNo AND p.Branch_Code = @BranchCode;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PONo,d.ItemCode,d.POItemQty,d.POItemRate,d.POItemSuplyQty,d.CPOItemQty,d.Branch_Code,
            ISNULL(d.unit, '') AS unit,d.GRNNo,d.UnitCode,d.BalanceQty,d.ReceivedQty,d.FinalQty, 
            ISNULL(i.ItemName, '') AS ItemName,
            ISNULL(d.FinalQty, 0) * ISNULL(d.POItemRate, 0) AS Total,
            ISNULL(d.ReceivedQty, 0) * ISNULL(d.POItemRate, 0) AS ReceivedQtyTotal,
            ISNULL(d.BalanceQty, 0) * ISNULL(d.POItemRate, 0) AS BalanceQtyTotal,
            ISNULL(d.MainUnit, '') AS MainUnit,ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(i.TaxCode, 0) AS TaxCode,ISNULL(i.TaxName, '') AS TaxName
            FROM GRNDETAILS d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.PONo = @PONo AND d.Branch_Code = @BranchCode and d.GRNNo = @GrnNo ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage,PO.GRNNo 
            from GRNTaxDescription PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Pno = @Pno AND PO.Branch_Code = @Branch_Code AND PO.GRNNo = @GrnNo
            ORDER BY PO.Pno;";

            const string termsQuery = @"SELECT TermsCode,TermsTitle,TermsDescription,MasterName,Branch_Code 
            FROM TermsAndConditionsMaster 
            WHERE Branch_Code = @Branch_Code ORDER BY TermsCode;";

            var master = await connection.QueryFirstOrDefaultAsync<GRNMasterModel>(
                masterQuery,
                new
                {
                    PONo = poNo,
                    BranchCode = branchCode,
                    GrnNo = GrnNo
                });

            if (master == null)
            {
                return null;
            }

            var details = (await connection.QueryAsync<GRNDetailsModel>(
                detailQuery,
                new
                {
                    PONo = poNo,
                    BranchCode = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            var taxDetails = (await connection.QueryAsync<GRNTaxDescriptionModel>(
                taxQuery,
                new
                {
                    Pno = poNo,
                    Branch_Code = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            var termsDetails = (await connection.QueryAsync<TermsAndConditionsMaster>(
                termsQuery,
                new
                {
                    Branch_Code = branchCode,
                })).ToList();

            return new GoodsReceivedNotePrintResponse
            {
                Master = master,
                Details = details,
                TaxDetails = taxDetails,
                TermsMaster = termsDetails
            };
        }
        public async Task<List<GoodsReceivedNoteListResponse>> GetGoodsReceivedList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT s.SupName AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PONo,p.PODate,p.SupCode,p.POValidDate,p.Billed,p.Branch_Code,p.OrderBy,p.status,
		    p.GRNNo,p.GRNDate,p.GRNtime,p.Receivedby,p.InspectedBy,p.StoreID,p.StoreName,
		    p.userId,p.ipAdd,p.TotalAmount,p.TotTax,p.OtherCharges,p.Roundoff,p.NetAmount,
		    p.MissChargeAmount,p.CgstAmount,p.SgstAmount
            FROM GRNMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE  p.Branch_Code = @BranchCode;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PONo,d.ItemCode,d.POItemQty,d.POItemRate,d.POItemSuplyQty,d.CPOItemQty,d.Branch_Code,
            ISNULL(d.unit, '') AS unit,d.GRNNo,d.UnitCode,d.BalanceQty,d.ReceivedQty,d.FinalQty, 
            ISNULL(i.ItemName, '') AS ItemName,
            ISNULL(d.FinalQty, 0) * ISNULL(d.POItemRate, 0) AS Total,
            ISNULL(d.ReceivedQty, 0) * ISNULL(d.POItemRate, 0) AS ReceivedQtyTotal,
            ISNULL(d.BalanceQty, 0) * ISNULL(d.POItemRate, 0) AS BalanceQtyTotal,
            ISNULL(d.MainUnit, '') AS MainUnit,ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion
            FROM GRNDETAILS d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage,PO.GRNNo 
            from GRNTaxDescription PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code 
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,pmc.GRNNo,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from GRNMISC pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code ORDER BY Pno DESC; ";

            var masters = (await connection.QueryAsync<GRNMasterModel>(
                masterQuery, 
                new 
                { 
                    BranchCode = branchCode 
                })).ToList();

            if (masters == null || masters.Count == 0) { 
                return new List<GoodsReceivedNoteListResponse>(); 
            }
            var details = (await connection.QueryAsync<GRNDetailsModel>(
                detailQuery,
                new
                {
                    BranchCode = branchCode
                })).ToList();

            var taxDetails = (await connection.QueryAsync<GRNTaxDescriptionModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var miscdetails = (await connection.QueryAsync<GRNMiscResponse>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var result = masters.Select(
                master => new GoodsReceivedNoteListResponse
                { 
                    Master = master, 
                    Details = details.Where(x => x.GRNNo == master.GRNNo).ToList(), 
                    TaxDetails = taxDetails.Where(x => x.GRNNo == master.GRNNo).ToList(),
                    MiscDetails = miscdetails.Where(x => x.GRNNo == master.GRNNo).ToList(),
                }).ToList();

            return result;
        }
        public async Task<string> DeletePurchaseOrderGRN(string grnNo,string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string getGRNMasterQuery = @"SELECT GRNNo,PONo,Branch_Code,status
                FROM GRNMaster
                WHERE GRNNo = @GRNNo AND Branch_Code = @Branch_Code;";

                var grnMaster = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    getGRNMasterQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                if (grnMaster == null)
                {
                    throw new Exception("GRN not found.");
                }

                string poNo = Convert.ToString(grnMaster.PONo);

                const string getGRNDetailsQuery = @"
                SELECT PONo,ItemCode,ReceivedQty,Branch_Code FROM GRNDETAILS
                WHERE GRNNo = @GRNNo AND Branch_Code = @Branch_Code;";

                var grnDetails = await connection.QueryAsync<dynamic>(
                    getGRNDetailsQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                foreach (var item in grnDetails)
                {
                    decimal receivedQty =Convert.ToDecimal(item.ReceivedQty);

                    const string updatePOQuery = @"
                    UPDATE ApprovalPurchaseOrderDetail SET
                    POItemQty = ISNULL(POItemQty, 0) + @ReceivedQty,
                    POItemSuplyQty = CASE WHEN ISNULL(POItemSuplyQty, 0) - @ReceivedQty < 0
                    THEN 0 ELSE ISNULL(POItemSuplyQty, 0) - @ReceivedQty END,
                    CPOItemQty =ISNULL(CPOItemQty, 0) + @ReceivedQty,
                    ReceivedQty =CASE WHEN ISNULL(ReceivedQty, 0) - @ReceivedQty < 0
                    THEN 0 ELSE ISNULL(ReceivedQty, 0) - @ReceivedQty END,
                    BalanceQty = ISNULL(BalanceQty, 0) + @ReceivedQty
                    WHERE PONo = @PONo AND ItemCode = @ItemCode  AND Branch_Code = @Branch_Code;";

                    await connection.ExecuteAsync(
                        updatePOQuery,
                        new
                        {
                            PONo = item.PONo,
                            ItemCode = item.ItemCode,
                            Branch_Code = branchCode,
                            ReceivedQty = receivedQty
                        },
                        transaction);
                }

                const string deleteTaxQuery = @" DELETE FROM GRNTaxDescription
                WHERE GRNNo = @GRNNo AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    deleteTaxQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                const string deleteMiscQuery = @"
                DELETE FROM GRNMISC WHERE GRNNo = @GRNNo
                AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    deleteMiscQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                const string deleteDetailsQuery = @"
                DELETE FROM GRNDETAILS WHERE GRNNo = @GRNNo AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    deleteDetailsQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                const string deleteMasterQuery = @"
                DELETE FROM GRNMaster WHERE GRNNo = @GRNNo AND Branch_Code = @Branch_Code;";

                int deletedRows = await connection.ExecuteAsync(
                    deleteMasterQuery,
                    new
                    {
                        GRNNo = grnNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                if (deletedRows == 0)
                {
                    throw new Exception("GRN could not be deleted.");
                }
                const string updatePOMasterQuery = @" UPDATE ApprovalPurchaseOrderMaster
                SET status = 'A' WHERE PONo = @PONo AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    updatePOMasterQuery,
                    new
                    {
                        PONo = poNo,
                        Branch_Code = branchCode
                    },
                    transaction);

                transaction.Commit();
                return grnNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        #endregion

        #region Purchase 
        public async Task<List<GoodsReceivedNoteListResponse>> GetPurchaseGoodsReceivedList(string branchCode, string GrnNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT s.SupName AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PONo,p.PODate,p.SupCode,p.POValidDate,p.Billed,p.Branch_Code,p.OrderBy,p.status,
		    p.GRNNo,p.GRNDate,p.GRNtime,p.Receivedby,p.InspectedBy,p.StoreID,p.StoreName,
		    p.userId,p.ipAdd,p.TotalAmount,p.TotTax,p.OtherCharges,p.Roundoff,p.NetAmount,
		    p.MissChargeAmount,p.CgstAmount,p.SgstAmount
            FROM GRNMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE  p.Branch_Code = @BranchCode AND p.GRNNo = @GrnNo;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PONo,d.ItemCode,d.POItemQty,d.POItemRate,d.POItemSuplyQty,d.CPOItemQty,d.Branch_Code,
            ISNULL(d.unit, '') AS unit,d.GRNNo,d.UnitCode,d.BalanceQty,d.ReceivedQty,d.FinalQty, 
            ISNULL(i.ItemName, '') AS ItemName,
            ISNULL(d.FinalQty, 0) * ISNULL(d.POItemRate, 0) AS Total,
            ISNULL(d.ReceivedQty, 0) * ISNULL(d.POItemRate, 0) AS ReceivedQtyTotal,
            ISNULL(d.BalanceQty, 0) * ISNULL(d.POItemRate, 0) AS BalanceQtyTotal,
            ISNULL(d.MainUnit, '') AS MainUnit,
            ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion
            FROM GRNDETAILS d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode AND d.GRNNo = @GrnNo ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage,PO.GRNNo 
            from GRNTaxDescription PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code AND PO.GRNNo = @GrnNo
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,pmc.GRNNo,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from GRNMISC pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code AND pmc.GRNNo = @GrnNo ORDER BY Pno DESC; ";

            var masters = (await connection.QueryAsync<GRNMasterModel>(
                masterQuery,
                new
                {
                    BranchCode = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<GoodsReceivedNoteListResponse>();
            }
            var details = (await connection.QueryAsync<GRNDetailsModel>(
                detailQuery,
                new
                {
                    BranchCode = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            var taxDetails = (await connection.QueryAsync<GRNTaxDescriptionModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            var miscdetails = (await connection.QueryAsync<GRNMiscResponse>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode,
                    GrnNo = GrnNo
                })).ToList();

            var result = masters.Select(
                master => new GoodsReceivedNoteListResponse
                {
                    Master = master,
                    Details = details.Where(x => x.GRNNo == master.GRNNo).ToList(),
                    TaxDetails = taxDetails.Where(x => x.GRNNo == master.GRNNo).ToList(),
                    MiscDetails = miscdetails.Where(x => x.GRNNo == master.GRNNo).ToList(),
                }).ToList();

            return result;
        }
        private async Task<int> CreatePurchaseFromGRN(string grnNo, string branchCode)
        {
            try
            {
                var Pono = await Findnextnumber("PurchaseMaster", "PNo", "Branch_Code", branchCode);
                var grnList = await GetPurchaseGoodsReceivedList(branchCode, grnNo);
                if (grnList == null || !grnList.Any())
                {
                    throw new Exception($"GRN not found. GRN No: {grnNo}");
                }
                var grn = grnList.First();
                var master = grn.Master;
                var details = grn.Details ?? new List<GRNDetailsModel>();
                var taxes = grn.TaxDetails ?? new List<GRNTaxDescriptionModel>();
                var miscDetails = grn.MiscDetails ?? new List<GRNMiscResponse>();
                var purchaseRequest = new PurchaseSaveRequest
                {
                    PNo = Pono,
                    PONo = Pono,
                    PDate = Convert.ToDateTime(master.GRNDate),
                    SupCode = Convert.ToInt32(master.SupCode),
                    SupplierName = Convert.ToString(master.VendorName),
                    PTotalAmount = Convert.ToDecimal(master.TotalAmount),
                    BillNo = Convert.ToString(master.Billed),
                    TaxAmount = Convert.ToDecimal(master.TotTax),
                    RoundOff = Convert.ToDecimal(master.Roundoff),
                    MissChargeAmount = Convert.ToDecimal(master.MissChargeAmount),
                    Discount = 0,
                    DepCode = 0,
                    PType = "GRN",
                    DirectIssue = false,
                    StoreName = Convert.ToString(master.StoreName),
                    BranchCode = Convert.ToString(master.Branch_Code),
                    UserCode = Convert.ToString(master.userId),
                    StoredId = Convert.ToString(master.StoreID) ?? "",
                    CgstAmount = Convert.ToDecimal(master.CgstAmount),
                    SgstAmount = Convert.ToDecimal(master.SgstAmount),
                    GrnNo = Convert.ToString(master.GRNNo) ?? "",
                    Details = details.Where(x => x.ReceivedQty > 0).Select(x => new PurchaseDetailRequest
                    {
                        ItemCode = Convert.ToInt32(x.ItemCode),
                        PItemQty = Convert.ToDecimal(x.ReceivedQty),
                        PItemRate = Convert.ToDecimal(x.POItemRate),
                        Unit = x.Unit,
                        QtyPer = 1,
                        NoOfQty = Convert.ToDecimal(x.ReceivedQty),
                        TotalQty = Convert.ToDecimal(x.ReceivedQty),
                        StoreName = Convert.ToString(master.StoreName),
                        StoredId = Convert.ToString(master.StoreID) ?? "",
                        NoOfDays = 0,
                        UnitCode = Convert.ToInt32(x.UnitCode),
                        ExpiryDate = DateTime.Now,
                        TrowQty = 0,
                        MainUnit = x.MainUnit,
                        MainUnitConverstion = x.MainUnitConverstion
                    }).ToList(),
                    Taxes = taxes.Select(x => new PurchaseTaxRequest
                    {
                        ItemCode = Convert.ToInt32(x.ItemCode),
                        TaxCode = Convert.ToInt32(x.TaxCode),
                        TaxAmount = Convert.ToDecimal(x.TaxAmount),
                        TaxPer = Convert.ToDecimal(x.TaxPer)
                    }).ToList(),
                    MiscDetails = miscDetails.Select(x => new PurchaseMiscRequest
                    {
                        TaxCode = Convert.ToInt32(x.TaxCode),
                        ChargeId = Convert.ToInt32(x.ChargeId),
                        ChargeCode = "",
                        ChargeAmount = Convert.ToString(x.ChargeAmt)
                    }).ToList()
                };
                return await CreatePurchase(purchaseRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreatePurchaseFromGRN Error: {ex.Message}");
                throw;
            }
        }
        public async Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            string str;
            int trno = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                str = $@"select ISNULL(MAX({column_name}), 0) AS trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where {condition_name} = @Branch ";
                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str, new { Branch = branch });

                if (ds.trno != null)
                {
                    if (trno == 0)
                    {
                        if (string.IsNullOrEmpty(ds.trno.ToString()))
                        {
                            trno = trno + 1;
                        }
                        else
                        {
                            trno = Convert.ToInt32(ds.trno);
                            trno = trno + 1;
                        }
                    }
                    else
                    {
                        trno = trno + 1;
                    }
                }
                else
                {
                    trno = trno + 1;
                }
            }
            catch
            {
                throw;
            }
            return trno;
        }
        public async Task<int> CreatePurchase(PurchaseSaveRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                long issueNo = 0;
                if (request.DirectIssue)
                {
                    issueNo = await connection.ExecuteScalarAsync<long>(
                        @"SELECT ISNULL(MAX(INo), 0) + 1 FROM ItemIssueMaster
                        WHERE Branch_Code = @BranchCode",
                        new
                        {
                            request.BranchCode
                        },
                        transaction);
                }
                if (request.DirectIssue)
                {
                    const string issueMasterQuery = @"
                        INSERT INTO ItemIssueMaster
                        (
                            INo,
                            IDate,
                            DepCode,
                            ITotalAmount,
                            Billno,
                            DIssue,
                            PNo,
                            IType,
                            StoredId,
                            Branch_Code,
                            UserCode,
                            Status
                        )
                        VALUES
                        (
                            @IssueNo,
                            @IDate,
                            @DepCode,
                            @ITotalAmount,
                            @BillNo,
                            @DIssue,
                            @PNo,
                            @IType,
                            @StoredId,
                            @Branch_Code,
                            @UserCode,
                            @Status
                        )";

                    await connection.ExecuteAsync(
                        issueMasterQuery,
                        new
                        {
                            IssueNo = issueNo,
                            IDate = request.PDate,
                            request.DepCode,
                            ITotalAmount = request.PTotalAmount,
                            request.BillNo,
                            DIssue = 1,
                            request.PNo,
                            IType = "DirectIssue",
                            StoredId = request.StoredId,
                            Branch_Code=request.BranchCode,
                            request.UserCode,
                            Status = "DirectIssue"
                        },
                        transaction);
                }
                decimal NetTotalAmount = request.PTotalAmount + request.TaxAmount + request.RoundOff + request.MissChargeAmount;
                const string purchaseMasterQuery = @"
                    INSERT INTO PurchaseMaster
                    (
                        PNo,
                        PDate,
                        SupCode,
                        SupplierName,
                        PTotalAmount,
                        BillNO,
                        TaxAmount,
                        DIssue,
                        INo,
                        DepCode,
                        NetAmount,
                        Discount,
                        PType,
                        Branch_Code,
                        RoundOff,
                        Misc,
                        UserCode,
                        PaidAmount,
                        BalanceAmount,
                        MissChargeAmount,
                        CgstAmount,
                        SgstAmount,
                        StoreId,
                        GrnNo
                    )
                    VALUES
                    (
                        @PNo,
                        @PDate,
                        @SupCode,
                        @SupplierName,
                        @PTotalAmount,
                        @BillNo,
                        @TaxAmount,
                        @DIssue,
                        @INo,
                        @DepCode,
                        @NetAmount,
                        @Discount,
                        @PType,
                        @BranchCode,
                        @RoundOff,
                        @Misc,
                        @UserCode,
                        @PaidAmount,
                        @BalanceAmount,
                        @MissChargeAmount,
                        @CgstAmount,
                        @SgstAmount,
                        @StoreId,
                        @GrnNo
                    )";
                await connection.ExecuteAsync(
                    purchaseMasterQuery,
                    new
                    {
                        request.PNo,
                        request.PDate,
                        request.SupCode,
                        request.SupplierName,
                        request.PTotalAmount,
                        request.BillNo,
                        request.TaxAmount,
                        DIssue = request.DirectIssue ? 1 : 0,
                        INo = issueNo,
                        request.DepCode,
                        NetAmount = NetTotalAmount,
                        request.Discount,
                        request.PType,
                        request.BranchCode,
                        request.RoundOff,
                        request.Misc,
                        request.UserCode,
                        PaidAmount = 0,
                        BalanceAmount = 0,
                        request.MissChargeAmount,
                        request.CgstAmount,
                        request.SgstAmount,
                        StoreId = request.StoredId,
                        request.GrnNo
                    },
                    transaction);

                int srNo = 0;
                foreach (var item in request.Details)
                {
                    int storeId = await connection.ExecuteScalarAsync<int>(
                        @"SELECT TOP 1 StoreId FROM StoreMaster WHERE StoreName = @StoreName",
                        new
                        {
                            item.StoreName
                        },
                        transaction);

                    const string purchaseDetailQuery = @"
                        INSERT INTO PurchaseDetail
                        (
                            PNo,
                            PONo,
                            ItemCode,
                            PItemQty,
                            PItemRate,
                            PItemReturnQty,
                            Unit,
                            QtyPer,
                            NoOfQty,
                            qty,
                            perrate,
                            SRNO,
                            TotalQty,
                            Branch_Code,
                            Pegs,
                            NoOfDays,
                            ExpiryDate,
                            TrowQty,
                            UnitCode,
                            StoredId,
                            MainUnit,
                            MainUnitConverstion
                        )
                        VALUES
                        (
                            @PNo,
                            @PONo,
                            @ItemCode,
                            @PItemQty,
                            @PItemRate,
                            @PItemReturnQty,
                            @Unit,
                            @QtyPer,
                            @NoOfQty,
                            @qty,
                            @perrate,
                            @SRNO,
                            @TotalQty,
                            @BranchCode,
                            @StoreId,
                            @NoOfDays,
                            @ExpiryDate,
                            @TrowQty,
                            @UnitCode,
                            @StoredId,
                            @MainUnit,
                            @MainUnitConverstion
                        )";

                    await connection.ExecuteAsync(
                        purchaseDetailQuery,
                        new
                        {
                            request.PNo,
                            request.PONo,
                            item.ItemCode,
                            item.PItemQty,
                            item.PItemRate,
                            PItemReturnQty=0,
                            item.Unit,
                            QtyPer= 1,
                            item.NoOfQty,
                            qty= item.PItemQty,
                            perrate= item.PItemRate,
                            SRNO = srNo,
                            item.TotalQty,
                            request.BranchCode,
                            StoreId = storeId,
                            item.NoOfDays,
                            ExpiryDate = item.ExpiryDate ?? request.PDate,
                            TrowQty=0,
                            item.UnitCode,
                            item.StoredId,
                            item.MainUnit,
                            item.MainUnitConverstion
                        },
                        transaction);

                    if (request.DirectIssue)
                    {
                        const string issueDetailQuery = @"
                            INSERT INTO ItemIssueDetail
                            (
                                INo,
                                ItemCode,
                                IItemQty,
                                IItemRate,
                                unit,
                                qtyPer,
                                noofqty,
                                PerRate,
                                PNo,
                                Branch_Code,
                                UnitCode,
                                DepCode,
                                IItemReturnQty,
                                AvailableQty,
                                ReturnQty,
                                MainUnit,
                                MainUnitConverstion,
                                StoreId
                            )
                            VALUES
                            (
                                @IssueNo,
                                @ItemCode,
                                @IItemQty,
                                @IItemRate,
                                @Unit,
                                @QtyPer,
                                @NoOfQty,
                                @PerRate,
                                @PNo,
                                @Branch_Code,
                                @UnitCode,
                                @DepCode,
                                @StoreId,
                                @IItemReturnQty,
                                @AvailableQty,
                                @ReturnQty,
                                @MainUnit,
                                @MainUnitConverstion
                            )";

                        await connection.ExecuteAsync(
                            issueDetailQuery,
                            new
                            {
                                IssueNo = issueNo,
                                item.ItemCode,
                                IItemQty = item.PItemQty,
                                IItemRate = item.PItemRate,
                                item.Unit,
                                QtyPer = 1,
                                NoOfQty = item.PItemQty,
                                PerRate = item.PItemRate,
                                request.PNo,
                                Branch_Code= request.BranchCode,
                                UnitCode = item.UnitCode,
                                DepCode=request.DepCode,
                                StoreId = request.StoredId,
                                IItemReturnQty = 0,
                                AvailableQty = 0,
                                ReturnQty = 0,
                                MainUnit = item.MainUnit,
                                MainUnitConverstion = item.MainUnitConverstion
                            },
                            transaction);
                    }
                    srNo++;
                }
                await SaveTaxDetailsAsync(connection,transaction,request);
                await SaveMiscDetailsAsync(connection,transaction,request);
                transaction.Commit();
                return request.PNo;
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }
        private async Task SaveTaxDetailsAsync(IDbConnection connection,IDbTransaction transaction,PurchaseSaveRequest request)
        {
            if (request.Taxes == null || request.Taxes.Count == 0)
                return;

            const string query = @"
                INSERT INTO PurchaseTax
                (
                    Pno,
                    ItemCode,
                    TaxCode,
                    TaxAmount,
                    Branch_Code,
                    TaxPer
                )
                VALUES
                (
                    @PNo,
                    @ItemCode,
                    @TaxCode,
                    @TaxAmount,
                    @Branch_Code,
                    @TaxPer
                )";

            foreach (var tax in request.Taxes)
            {
                await connection.ExecuteAsync(
                    query,
                    new
                    {
                        request.PNo,
                        tax.ItemCode,
                        tax.TaxCode,
                        TaxAmount = Math.Round(tax.TaxAmount, 2),
                        Branch_Code = request.BranchCode,
                        TaxPer = Math.Round(tax.TaxPer, 2),
                    },
                    transaction);
            }
        }
        private async Task SaveMiscDetailsAsync(IDbConnection connection,IDbTransaction transaction,PurchaseSaveRequest request)
        {
            if (request.MiscDetails == null || request.MiscDetails.Count == 0)
            {
                return;
            }
            const string query = @"
                INSERT INTO Tbl_Misc
                (
                    ChargeId,
                    ChargeAmt,
                    Branch_Code,
                    PNo,
                    TaxCode
                )
                VALUES
                (
                    @ChargeId,
                    @ChargeAmt,
                    @Branch_Code,
                    @PNo,
                    @TaxCode
                )";

            foreach (var misc in request.MiscDetails)
            {
                await connection.ExecuteAsync(
                    query,
                    new
                    {
                        ChargeId= misc.ChargeId,
                        ChargeAmt=misc.ChargeAmount,
                        Branch_Code = request.BranchCode,
                        PNo = request.PNo,
                        TaxCode = misc.TaxCode
                    },
                    transaction);
            }
        }
        public async Task<bool> DeleteDirectPurchase(int pNo,string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string checkQuery = @"SELECT COUNT(1)
                FROM PurchaseMaster
                WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                var exists = await connection.ExecuteScalarAsync<int>(
                    checkQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                if (exists == 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string deleteTaxQuery = @"DELETE FROM PurchaseTax
                WHERE Pno = @PNo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    deleteTaxQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                const string deleteMiscQuery = @"DELETE FROM Tbl_Misc
                WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    deleteMiscQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                const string deleteIssueDetailQuery = @"DELETE FROM ItemIssueDetail
                WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    deleteIssueDetailQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                const string deleteIssueMasterQuery = @" DELETE FROM ItemIssueMaster
                WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    deleteIssueMasterQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                const string deleteDetailQuery = @"
                DELETE FROM PurchaseDetail WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    deleteDetailQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                const string deleteMasterQuery = @"DELETE FROM PurchaseMaster
                WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                var affectedRows = await connection.ExecuteAsync(
                    deleteMasterQuery,
                    new
                    {
                        PNo = pNo,
                        BranchCode = branchCode
                    },
                    transaction);

                transaction.Commit();
                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<List<PurchaseOrderList>> GetPurchasePrintList(string branchCode, int pNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT ISNULL(NULLIF(s.SupName, ''), p.SupplierName) AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PNo, p.PDate, p.SupCode, p.PTotalAmount, p.billno, p.TaxAmount, p.DIssue,
            p.INo, p.DepCode, p.Netamount, p.Discount, p.PType, p.Branch_Code AS BranchCode, p.RoundOff, p.Misc, 
            p.UserCode, p.PaidAmount, p.BalanceAmount, p.MissChargeAmount, p.SgstAmount, p.CgstAmount,
            ISNULL(p.StoreId, '') AS StoreId,ISNULL(M.StoreName, '') AS StoreName
            FROM PurchaseMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            LEFT JOIN STOREMASTER M ON p.Storeid = m.Storeid AND p.Branch_Code = m.Branch_Code
            WHERE  p.Branch_Code = @BranchCode AND p.PNo = @PNo;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PNo, d.PONo, d.ItemCode, d.PItemQty, d.PItemRate, d.PItemReturnQty, d.unit AS Unit, 
            d.qtyPer AS QtyPer, d.noofqty AS NoOfQty, d.srno AS SrNo, d.Totalqty AS TotalQty,
            d.qty AS Qty, d.perrate AS PerRate,d.PIRNoQty, d.PIQty, d.PRate, d.Tax, d.TotAmt, d.Pegs, 
            d.Branch_Code AS BranchCode, d.NoOfDays, d.ExpiryDate, d.TrowQty, d.TrowDate, d.StoredId,
            ISNULL(i.ItemName, '') AS ItemName,ISNULL(d.MainUnit, '') AS MainUnit,
            ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(d.PItemQty, 0) * ISNULL(d.PItemRate, 0) AS Total,
            ISNULL(i.TaxCode, 0) AS TaxCode,ISNULL(i.TaxName, '') AS TaxName,ISNULL(d.PAQ, 0) AS PAQ
            FROM PurchaseDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode AND d.PNo = @PNo ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage
            from PurchaseTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code AND PO.Pno = @Pno
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from Tbl_Misc pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code AND pmc.Pno = @Pno ORDER BY Pno DESC; ";

            var masters = (await connection.QueryAsync<PurchaseSaveListRequest>(
                masterQuery,
                new
                {
                    BranchCode = branchCode,
                    Pno = pNo
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<PurchaseOrderList>();
            }
            var details = (await connection.QueryAsync<PurchaseOrderDetailListResponse>(
                detailQuery,
                new
                {
                    BranchCode = branchCode,
                    Pno = pNo
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseTaxModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode,
                    Pno = pNo
                })).ToList();

            var miscdetails = (await connection.QueryAsync<PurchaseMiscModel>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode,
                    Pno = pNo
                })).ToList();

            var result = masters.Select(
                master => new PurchaseOrderList
                {
                    Master = master,
                    Details = details.Where(x => x.PONo == master.PNo && x.BranchCode == master.BranchCode).ToList(),
                    TaxDetails = taxDetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                    MiscDetails = miscdetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                }).ToList();

            return result;
        }
        public async Task<List<PurchaseOrderList>> GetPurchaseList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT ISNULL(NULLIF(s.SupName, ''), p.SupplierName) AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PNo, p.PDate, p.SupCode, p.PTotalAmount, p.billno, p.TaxAmount, p.DIssue,
            p.INo, p.DepCode, p.Netamount, p.Discount, p.PType, p.Branch_Code AS BranchCode, p.RoundOff, p.Misc, 
            p.UserCode, p.PaidAmount, p.BalanceAmount, p.MissChargeAmount, p.SgstAmount, p.CgstAmount,
            ISNULL(p.StoreId, '') AS StoreId,ISNULL(M.StoreName, '') AS StoreName
            FROM PurchaseMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            LEFT JOIN STOREMASTER M ON p.Storeid = m.Storeid AND p.Branch_Code = m.Branch_Code
            WHERE  p.Branch_Code = @BranchCode ";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PNo, d.PONo, d.ItemCode, d.PItemQty, d.PItemRate, d.PItemReturnQty, d.unit AS Unit, 
            d.qtyPer AS QtyPer, d.noofqty AS NoOfQty, d.srno AS SrNo, d.Totalqty AS TotalQty,
            d.qty AS Qty, d.perrate AS PerRate,d.PIRNoQty, d.PIQty, d.PRate, d.Tax, d.TotAmt, d.Pegs, 
            d.Branch_Code AS BranchCode, d.NoOfDays, d.ExpiryDate, d.TrowQty, d.TrowDate, d.StoredId,
            ISNULL(i.ItemName, '') AS ItemName,ISNULL(d.MainUnit, '') AS MainUnit,
            ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(d.PItemQty, 0) * ISNULL(d.PItemRate, 0) AS Total,ISNULL(d.PAQ, 0) AS PAQ
            FROM PurchaseDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage
            from PurchaseTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from Tbl_Misc pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code  ORDER BY Pno DESC; ";

            var masters = (await connection.QueryAsync<PurchaseSaveListRequest>(
                masterQuery,
                new
                {
                    BranchCode = branchCode
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<PurchaseOrderList>();
            }
            var details = (await connection.QueryAsync<PurchaseOrderDetailListResponse>(
                detailQuery,
                new
                {
                    BranchCode = branchCode
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseTaxModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var miscdetails = (await connection.QueryAsync<PurchaseMiscModel>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var result = masters.Select(
                master => new PurchaseOrderList
                {
                    Master = master,
                    Details = details.Where(x => x.PONo == master.PNo && x.BranchCode == master.BranchCode).ToList(),
                    TaxDetails = taxDetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                    MiscDetails = miscdetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                }).ToList();

            return result;
        }
        public async Task<List<PurchaseOrderGRNNUmberRequest>> GetPurchaseOrderGRNNumber(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT DISTINCT G.GRNNo as GRNNumber,G.Status as status FROM GRNMASTER G
            WHERE G.Branch_Code = @BranchCode AND NOT EXISTS( SELECT 1 FROM PurchaseMaster P
            WHERE P.GrnNo IS NOT NULL AND P.GrnNo = G.GRNNo AND P.Branch_Code = @BranchCode)
            ORDER BY G.GRNNo Asc;";

            var result = await connection.QueryAsync<PurchaseOrderGRNNUmberRequest>(query,
            new
            {
                BranchCode = branchCode
            });
            return result.ToList();
        }
        #endregion

        #region Purchase Return
        public async Task<List<PurchaseOrderRequestNumber>> GetPurchaseOrderNumberReturn(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT DISTINCT G.PNo,G.PType AS PurchaseType 
            FROM PurchaseMaster G LEFT JOIN PurchaseDetail D ON G.PNo = D.PNo
            WHERE G.Branch_Code = @BranchCode AND ISNULL(D.PItemQty, 0) > 0
            AND ISNULL(D.PItemQty, 0) - ISNULL(D.PItemReturnQty, 0) - ISNULL(D.DamageQty, 0) > 0
            ORDER BY G.PNo ASC;";

            var result = await connection.QueryAsync<PurchaseOrderRequestNumber>(query,
            new
            {
                BranchCode = branchCode
            });
            return result.ToList();
        }
        public async Task<List<PurchaseOrderReturnList>> GetItemPurchaseOrderList(string branchCode, int pNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT ISNULL(NULLIF(s.SupName, ''), p.SupplierName) AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PNo, p.PDate, p.SupCode, p.PTotalAmount, p.billno, p.TaxAmount, p.DIssue,
            p.INo, p.DepCode, p.Netamount, p.Discount, p.PType, p.Branch_Code as BranchCode, p.RoundOff, p.Misc, 
            p.UserCode, p.PaidAmount, p.BalanceAmount, p.MissChargeAmount, p.SgstAmount, p.CgstAmount,
            ISNULL(p.StoreId, '') AS StoreId,ISNULL(M.StoreName, '') AS StoreName
            FROM PurchaseMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            LEFT JOIN STOREMASTER M ON p.Storeid = m.Storeid AND p.Branch_Code = m.Branch_Code
            WHERE  p.Branch_Code = @BranchCode AND p.PNo = @PNo;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PNo, d.PONo, d.ItemCode, d.PItemQty, d.PItemRate, d.PItemReturnQty, d.DamageQty, d.unit AS Unit, 
            d.UnitCode,d.qtyPer AS QtyPer, d.noofqty AS NoOfQty, d.srno AS SrNo, d.Totalqty AS TotalQty,
            d.qty AS Qty, d.perrate AS PerRate,d.PIRNoQty, d.PIQty, d.PRate, d.Tax, d.TotAmt, d.Pegs, 
            d.Branch_Code AS BranchCode, d.NoOfDays, d.ExpiryDate, d.TrowQty, d.TrowDate, d.StoredId,
            ISNULL(i.ItemName, '') AS ItemName,ISNULL(d.MainUnit, '') AS MainUnit,
            ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(d.PItemQty, 0) * ISNULL(d.PItemRate, 0) AS Total,
            CASE  WHEN ISNULL(d.PItemQty, 0) - ISNULL(d.PItemReturnQty, 0) - ISNULL(d.DamageQty, 0) < 0 
            THEN 0 ELSE ISNULL(d.PItemQty, 0) - ISNULL(d.PItemReturnQty, 0) - ISNULL(d.DamageQty, 0)
            END AS ReamingQty,ISNULL(d.PAQ, 0) AS PAQ
            FROM PurchaseDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode AND d.PNo = @PNo ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage
            from PurchaseTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code AND PO.Pno = @Pno
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from Tbl_Misc pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code AND pmc.Pno = @Pno ORDER BY Pno DESC; ";

            var masters = (await connection.QueryAsync<PurchaseSaveListRequest>(
                masterQuery,
                new
                {
                    BranchCode = branchCode,
                    Pno = pNo
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<PurchaseOrderReturnList>();
            }
            var details = (await connection.QueryAsync<PurchaseOrderReturnDetailList>(
                detailQuery,
                new
                {
                    BranchCode = branchCode,
                    Pno = pNo
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseTaxModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode,
                    Pno = pNo
                })).ToList();

            var miscdetails = (await connection.QueryAsync<PurchaseMiscModel>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode,
                    Pno = pNo
                })).ToList();

            var result = masters.Select(
                master => new PurchaseOrderReturnList
                {
                    Master = master,
                    Details = details.Where(x => x.PONo == master.PNo && x.BranchCode == master.BranchCode).ToList(),
                    TaxDetails = taxDetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                    MiscDetails = miscdetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.BranchCode).ToList(),
                }).ToList();

            return result;
        }
        public async Task<int> SavePurchaseReturnOrder(PurchaseReturnSaveRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Details == null || request.Details.Count == 0)
                throw new Exception("Please add at least one item.");

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                int prNo = request.PRNo;
                var insertMasterSql = @"
                INSERT INTO PurchaseReturnMaster
                (
                    TransactionNo,
                    PNo,
                    PRNo,
                    PRDate,
                    SupCode,
                    SupplierName,
                    Branch_Code,
                    TotalAmount,
                    PRTotalAmount,
                    TaxAmount,
                    GrossAmount,
                    MissChargeAmount,
                    CgstAmount,
                    SgstAmount
                )
                VALUES
                (
                    @TransactionNo,
                    @PNo,
                    @PRNo,
                    @PRDate,
                    @SupCode,
                    @SupplierName,
                    @BranchCode,
                    @TotalAmount,
                    @PRTotalAmount,
                    @TaxAmount,
                    @GrossAmount,
                    @MissChargeAmount,
                    @CgstAmount,
                    @SgstAmount
                );";
                await connection.ExecuteAsync(
                    insertMasterSql,
                    new
                    {
                        TransactionNo = request.TransactionNo,
                        PNo = request.PNo,
                        PRNo = request.PRNo,
                        PRDate = request.PRDate,
                        SupCode = request.SupCode,
                        BranchCode = request.BranchCode,
                        TotalAmount = request.TotalAmount,
                        PRTotalAmount = request.TotalAmount,
                        TaxAmount = request.TaxAmount,
                        GrossAmount = request.GrossAmount,
                        MissChargeAmount = request.MissChargeAmount,
                        CgstAmount = request.CgstAmount,
                        SgstAmount = request.SgstAmount,
                        SupplierName = request.SupplierName
                    },
                    transaction);
                foreach (var item in request.Details)
                {
                    var purchaseDetailSql = @"SELECT  ISNULL(PItemQty, 0) AS PItemQty,ISNULL(PAQ, 0) AS PAQ,
                    ISNULL(PItemReturnQty, 0) AS PItemReturnQty FROM PurchaseDetail
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode;";

                    var purchaseDetail = await connection.QueryFirstOrDefaultAsync(
                        purchaseDetailSql,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = request.PNo,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    decimal OriginalQty = purchaseDetail?.PItemQty == null ? 0m: Convert.ToDecimal(purchaseDetail.PItemQty);

                    var insertDetailSql = @"
                    INSERT INTO PurchaseReturnDetail
                    (
                        PRNo,
                        PNo,
                        ItemCode,
                        PRItemRate,
                        PRItemQty,
                        PRNIQty,
                        PReturnQty,
                        PRAQty,
                        Branch_Code,
                        MainUnit,
                        MainUnitConverstion,
                        UnitCode,
                        Unit,
                        OriginalQty
                    )
                    VALUES
                    (
                        @PRNo,
                        @PNo,
                        @ItemCode,
                        @PRItemRate,
                        @PRItemQty,
                        @PRNIQty,
                        @PReturnQty,
                        @PRAQty,
                        @BranchCode,
                        @MainUnit,
                        @MainUnitConverstion,
                        @UnitCode,
                        @Unit,
                        @OriginalQty
                    );";

                    await connection.ExecuteAsync(
                        insertDetailSql,
                        new
                        {
                            PRNo = prNo,
                            PNo = request.PNo,
                            ItemCode = item.ItemCode,
                            PRItemRate = item.PRItemRate,
                            PRItemQty = item.PRItemQty,
                            PRNIQty = item.PReturnQty,
                            PReturnQty = item.PReturnQty,
                            PRAQty = item.PRAQty,
                            BranchCode = request.BranchCode,
                            MainUnit = item.MainUnit,
                            MainUnitConverstion = item.MainUnitConverstion,
                            UnitCode=item.UnitCode,
                            Unit=item.Unit,
                            OriginalQty= OriginalQty
                        },
                        transaction);

                    var updatePurchaseDetailSql = @"
                    UPDATE PurchaseDetail SET PItemReturnQty = ISNULL(PItemReturnQty, 0) + @ReturnQty,
                    PAQ = ISNULL(PItemQty, 0) - (ISNULL(PItemReturnQty, 0) + @ReturnQty)
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode;";

                    await connection.ExecuteAsync(
                        updatePurchaseDetailSql,
                        new
                        {
                            ReturnQty = item.PReturnQty,
                            ItemCode = item.ItemCode,
                            PNo = request.PNo,
                            BranchCode = request.BranchCode
                        },
                        transaction);
                }
                if (request.Taxes != null && request.Taxes.Count > 0)
                {
                    const string taxQuery = @"
                    INSERT INTO PurchaseReturnTax
                    (
                        Pno,
                        ItemCode,
                        TaxCode,
                        TaxPer,
                        TaxAmount,
                        Branch_Code,
                        PRNo
                    )
                    VALUES
                    (
                        @Pno,
                        @ItemCode,
                        @TaxCode,
                        @TaxPer,
                        @TaxAmount,
                        @Branch_Code,
                        @PRNo
                    );";
                    foreach (var tax in request.Taxes)
                    {
                        tax.Pno = request.PNo;
                        tax.Branch_Code = request.BranchCode;
                        tax.TaxPer = Math.Round(tax.TaxPer, 2);
                        tax.TaxAmount = Math.Round(tax.TaxAmount, 2);
                        tax.PRNo = request.PRNo;
                        await connection.ExecuteAsync(taxQuery, tax, transaction);
                    }
                }
                if (request.Miscellaneous != null && request.Miscellaneous.Count > 0)
                {
                    const string miscQuery = @"
                    INSERT INTO Tbl_PRMISC
                    (
                        ChargeId,
                        ChargeAmt,
                        Branch_Code,
                        Pno,
                        TaxCode,
                        PRNo
                    )
                    VALUES
                    (
                        @ChargeId,
                        @ChargeAmt,
                        @Branch_Code,
                        @Pno,
                        @TaxCode,
                        @PRNo
                    );";
                    foreach (var misc in request.Miscellaneous)
                    {
                        misc.Pno = request.PNo;
                        misc.Branch_Code = request.BranchCode;
                        misc.TaxCode = misc.TaxCode;
                        misc.PRNo = request.PRNo;
                        await connection.ExecuteAsync(miscQuery, misc, transaction);
                    }
                }
                transaction.Commit();
                return prNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT ISNULL(NULLIF(s.SupName, ''), p.SupplierName) AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PRNo, p.PRDate, p.SupCode, p.PRTotalAmount, p.Branch_Code, p.TotalAmount,p.TaxAmount, 
            p.GrossAmount, p.MissChargeAmount, p.CgstAmount, p.SgstAmount, p.TransactionNo, p.PNo
            FROM PurchaseReturnMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE  p.Branch_Code = @BranchCode;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PRNo, d.PNo, d.ItemCode, d.PRItemQty, d.PRItemRate, d.PRNIQty, d.Branch_Code,d.PReturnQty, d.PRAQty,
            ISNULL(d.MainUnit, '') AS MainUnit,ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(i.ItemName, '') AS ItemName,d.UnitCode,d.unit AS Unit,
            ISNULL(d.PRItemQty, 0) * ISNULL(d.PRItemRate, 0) AS Total,
            CASE WHEN ISNULL(d.PRItemQty, 0) - ISNULL(d.PReturnQty, 0) - ISNULL(d.DamageQty, 0) < 0 THEN 0 
            ELSE ISNULL(d.PRItemQty, 0) - ISNULL(d.PReturnQty, 0) - ISNULL(d.DamageQty, 0) END AS RemainingQty,
            ISNULL(d.DamageQty, 0) AS DamageQty
            FROM PurchaseReturnDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,PO.PRNo,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage
            from PurchaseReturnTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code
            ORDER BY PO.PRNo;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,pmc.PRNo,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from Tbl_PRMISC pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code ORDER BY pmc.PRNo DESC; ";

            var masters = (await connection.QueryAsync<PurchaseReturnMasterModel>(
                masterQuery,
                new
                {
                    BranchCode = branchCode
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<PurchaseOrderReturnListResponse>();
            }
            var details = (await connection.QueryAsync<PurchaseReturnDetailModel>(
                detailQuery,
                new
                {
                    BranchCode = branchCode
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseTaxModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var miscdetails = (await connection.QueryAsync<PurchaseMiscModel>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode
                })).ToList();

            var result = masters.Select(
                master => new PurchaseOrderReturnListResponse
                {
                    Master = master,
                    Details = details.Where(x => x.PRNo == master.PRNo && x.Branch_Code == master.Branch_Code).ToList(),
                    TaxDetails = taxDetails.Where(x => x.PRNo == master.PRNo && x.Branch_Code == master.Branch_Code).ToList(),
                    MiscDetails = miscdetails.Where(x => x.PRNo == master.PRNo && x.Branch_Code == master.Branch_Code).ToList(),
                }).ToList();

            return result;
        }

        public async Task<List<PurchaseOrderReturnListResponse>> GetPurchaseReturnOrderPrintList(string branchCode, int PRNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT ISNULL(NULLIF(s.SupName, ''), p.SupplierName) AS VendorName,
            ISNULL(s.SupAdd1, '') +ISNULL(s.SupAdd2, '') +' ' +ISNULL(s.Sup_City, '') +' - ' +
            ISNULL(s.Susp_pincode, '') AS VendorAddress,ISNULL(s.SupPhone, '') AS PhoneNo,
            ISNULL(s.SupMobile, '') AS MobileNo,ISNULL(s.GSTno, '') AS GSTNo,ISNULL(s.TinNo, '') AS TinNo,
            ISNULL(s.SupFax, '') AS StateCode,
            p.PRNo, p.PRDate, p.SupCode, p.PRTotalAmount, p.Branch_Code, p.TotalAmount,p.TaxAmount, 
            p.GrossAmount, p.MissChargeAmount, p.CgstAmount, p.SgstAmount, p.TransactionNo, p.PNo
            FROM PurchaseReturnMaster p
            LEFT JOIN Supplier s ON s.SupCode = p.SupCode AND s.Branch_Code = p.Branch_Code
            WHERE  p.Branch_Code = @BranchCode AND p.PRNo = @PRNo;";

            const string detailQuery = @"SELECT ROW_NUMBER() OVER (ORDER BY d.ItemCode) AS Rno,
            d.PRNo, d.PNo, d.ItemCode, d.PRItemQty, d.PRItemRate, d.PRNIQty, d.Branch_Code,d.PReturnQty, d.PRAQty,
            ISNULL(d.MainUnit, '') AS MainUnit,ISNULL(d.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(i.ItemName, '') AS ItemName,d.UnitCode,d.unit AS Unit,
            ISNULL(i.TaxCode, 0) AS TaxCode,ISNULL(i.TaxName, '') AS TaxName,
            ISNULL(d.PRItemQty, 0) * ISNULL(d.PRItemRate, 0) AS Total,
            CASE WHEN ISNULL(d.PRItemQty, 0) - ISNULL(d.PReturnQty, 0) - ISNULL(d.DamageQty, 0) < 0 THEN 0 
            ELSE ISNULL(d.PRItemQty, 0) - ISNULL(d.PReturnQty, 0) - ISNULL(d.DamageQty, 0) END AS RemainingQty,
            ISNULL(d.DamageQty, 0) AS DamageQty
            FROM PurchaseReturnDetail d
            LEFT JOIN InventoryItemMaster i ON i.ItemCode = d.ItemCode AND i.Branch_Code = d.Branch_Code
            WHERE d.Branch_Code = @BranchCode AND d.PRNo = @PRNo ORDER BY d.ItemCode;";

            const string taxQuery = @"select distinct BT.TaxDescId,PO.Pno,ItemCode,PO.TaxCode,PO.PRNo,
            PO.TaxPer,PO.TaxAmount,PO.Branch_Code,BT.TaxDescription,BT.TaxPercentage
            from PurchaseReturnTax PO
            INNER join BillTaxDescription BT ON PO.TaxCode=BT.TaxCode
            WHERE PO.Branch_Code = @Branch_Code AND PO.PRNo = @PRNo
            ORDER BY PO.Pno;";

            const string MiscQuery = @"select DISTINCT pmc.ChargeId, pmc.ChargeAmt,pmc.PRNo,
            pmc.Branch_Code,pmc.Pno,tax.TaxCode,tax.TaxName as TaxDescription,
            tax.TaxPercentage,mc.ChargeName from Tbl_PRMISC pmc
            left join BillTaxMaster tax on pmc.TaxCode = tax.TaxCode
            left join MiscCharges mc on pmc.ChargeId = mc.ChargeId
            WHERE pmc.Branch_Code = @Branch_Code AND pmc.PRNo = @PRNo ORDER BY pmc.PRNo DESC; ";

            var masters = (await connection.QueryAsync<PurchaseReturnMasterModel>(
                masterQuery,
                new
                {
                    BranchCode = branchCode,
                    PRNo = PRNo
                })).ToList();

            if (masters == null || masters.Count == 0)
            {
                return new List<PurchaseOrderReturnListResponse>();
            }
            var details = (await connection.QueryAsync<PurchaseReturnDetailModel>(
                detailQuery,
                new
                {
                    BranchCode = branchCode,
                    PRNo = PRNo
                })).ToList();

            var taxDetails = (await connection.QueryAsync<PurchaseTaxModel>(
                taxQuery,
                new
                {
                    Branch_Code = branchCode,
                    PRNo = PRNo
                })).ToList();

            var miscdetails = (await connection.QueryAsync<PurchaseMiscModel>(
                MiscQuery,
                new
                {
                    Branch_Code = branchCode,
                    PRNo = PRNo
                })).ToList();

            var result = masters.Select(
                master => new PurchaseOrderReturnListResponse
                {
                    Master = master,
                    Details = details.Where(x => x.PNo == master.PNo && x.Branch_Code == master.Branch_Code).ToList(),
                    TaxDetails = taxDetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.Branch_Code).ToList(),
                    MiscDetails = miscdetails.Where(x => x.Pno == master.PNo && x.Branch_Code == master.Branch_Code).ToList(),
                }).ToList();

            return result;
        }
        #endregion

        #region  Item Damage Entry
        public async Task<List<PurchaseDetailResponse>> LoadPurchaseDetailReturnData(string branchCode, int ItemCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            const string query = @"SELECT PD.PNo,PD.ItemCode,
            ISNULL(NULLIF(Sp.SupName, ''), PM.SupplierName) AS Supplier,
            ISNULL(Pm.SupCode, 0) AS SupplierCode,ISNULL(PD.PItemQty, 0) AS PurQty,
            ISNULL(PD.PItemRate, 0) AS Rate,ISNULL(PD.Unit, '') AS Unit,
            ISNULL(PD.UnitCode, '') AS UnitCode, ISNULL(PD.MainUnit, '') AS MainUnit,
            ISNULL(PD.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(PD.PItemReturnQty, 0) AS RetQty,ISNULL(PD.DamageQty, 0) AS DamageQty,
            ISNULL(PD.PItemQty, 0) - ISNULL(PD.PItemReturnQty, 0) - ISNULL(PD.DamageQty, 0) AS AvailQty
            FROM Purchasedetail PD
            LEFT JOIN PurchaseMaster PM ON PM.PNo = PD.PNo
            LEFT JOIN Supplier Sp ON PM.SupCode = Sp.SupCode
            WHERE PD.ItemCode = @ItemCode AND PM.Branch_Code = @BranchCode";

            var purchaseDetails = (await connection.QueryAsync<PurchaseDetailResponse>(query,
                new
                {
                    ItemCode = ItemCode,
                    BranchCode = branchCode
                })).ToList();

            var result = new List<PurchaseDetailResponse>();

            foreach (var purchase in purchaseDetails)
            {
                decimal availQty = purchase.AvailQty;
                if (availQty <= 0)
                    continue;
                //const string issueQuery = @"SELECT COUNT(1) FROM ItemIssuedetail
                //WHERE PNo = @PNo AND Branch_Code = @BranchCode";

                //int issueCount = await connection.ExecuteScalarAsync<int>(
                //    issueQuery,
                //    new
                //    {
                //        PNo = purchase.PNo,
                //        BranchCode = branchCode
                //    });

                //bool alreadyIssued = issueCount > 0;
                result.Add(new PurchaseDetailResponse
                {
                    PNo = purchase.PNo,
                    ItemCode = purchase.ItemCode,
                    Supplier = purchase.Supplier,
                    SupplierCode = purchase.SupplierCode,
                    PurQty = purchase.PurQty,
                    RetQty = purchase.RetQty,
                    DamageQty = purchase.DamageQty,
                    AvailQty = availQty,
                    Rate = purchase.Rate,
                    Unit = purchase.Unit,
                    UnitCode = purchase.UnitCode,
                    MainUnit = purchase.MainUnit,
                    MainUnitConverstion = purchase.MainUnitConverstion
                    //IsAlreadyIssued = alreadyIssued,
                    //Message = alreadyIssued ? "Items Issued Already Entry Not Possible!!!" : string.Empty
                });
            }
            return result;
        }

        public async Task<int> SaveItemDamageAsync(ItemDamageSaveRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Details == null || request.Details.Count == 0)
                throw new Exception("Transaction Not Possible No item in list...");
            if (string.IsNullOrWhiteSpace(request.BranchCode))
                throw new Exception("Branch Code Can't Be Empty");
            if (request.DDate == default)
                throw new Exception("Date Can't Be Empty");
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                int dNo = request.DNo;
                foreach (var item in request.Details)
                {
                    if (item.ItemQty < 0)
                    {
                        throw new Exception($"Item Qty Can't Be Zero for Item {item.ItemCode}");
                    }
                    if (item.DItemQty < 0)
                    {
                        throw new Exception($"Item Damage Qty Can't Be Zero for Item {item.ItemCode}");
                    }
                    if (item.DItemQty > item.ItemQty)
                    {
                        throw new Exception($"Damage Qty Can't Be Greater Than Qty for Item {item.ItemCode}");
                    }
                    if (item.ItemBalQty < 0)
                    {
                        throw new Exception($"Invalid Balance Quantity for Item {item.ItemCode}");
                    }
                    // Check ItemMaster
                    const string itemMasterCheckQuery = @"SELECT COUNT(1) FROM InventoryItemMaster 
                    WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";

                    int itemExists = await connection.ExecuteScalarAsync<int>(
                        itemMasterCheckQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    if (itemExists == 0)
                    {
                        throw new Exception(
                            $"Item not found in ItemMaster. ItemCode: {item.ItemCode}");
                    }
                    // Check ItemIssuedetail
                    //bool issued = await CheckInIssueAsync(connection, transaction, item.PNo, request.BranchCode);
                    //if (issued)
                    //{
                    //    throw new Exception($"Items Issued Already Entry Not Possible!!! PNo: {item.PNo}");
                    //}
                    // Get Purchase Detail
                    const string purchaseQuery = @"SELECT ISNULL(PItemQty, 0) AS PItemQty,
                    ISNULL(PItemReturnQty, 0) AS PItemReturnQty, ISNULL(DamageQty, 0) AS DamageQty,
                    ISNULL(PAQ, 0) AS PAQ FROM PurchaseDetail
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode";

                    var purchase = await connection.QueryFirstOrDefaultAsync<PurchaseQuantityModel>(
                        purchaseQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = item.PNo,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    if (purchase == null)
                    {
                        throw new Exception(
                            $"Purchase detail not found for ItemCode {item.ItemCode}, PNo {item.PNo}");
                    }
                    decimal availablePurchaseQty =purchase.PItemQty - purchase.PItemReturnQty;
                    if (availablePurchaseQty < 0)
                    {
                        availablePurchaseQty = 0;
                    }
                    if (item.DItemQty > availablePurchaseQty)
                    {
                        throw new Exception(
                            $"Available Purchase Quantity is {availablePurchaseQty} " +
                            $"for Item {item.ItemCode}, PNo {item.PNo}");
                    }
                    if (item.ItemBalQty > availablePurchaseQty)
                    {
                        throw new Exception(
                            $"Damage converted quantity {item.ItemBalQty} " +
                            $"is greater than available purchase quantity " +
                            $"{availablePurchaseQty} for Item {item.ItemCode}");
                    }
                }

                // 4. INSERT DAMAGE MASTER
                const string masterQuery = @"
                INSERT INTO ItemDamageMaster
                (
                    DNo,
                    DDate,
                    Branch_Code,
                    DTotalAmount,
                    TaxAmount,
                    GrossAmount,
                    MissChargeAmount,
                    CgstAmount,
                    SgstAmount
                )
                VALUES
                (
                    @DNo,
                    @DDate,
                    @BranchCode,
                    @DTotalAmount,
                    @TaxAmount,
                    @GrossAmount,
                    @MissChargeAmount,
                    @CgstAmount,
                    @SgstAmount
                )";

                int masterInserted = await connection.ExecuteAsync(
                    masterQuery,
                    new
                    {
                        DNo = dNo,
                        DDate = request.DDate,
                        BranchCode = request.BranchCode,
                        DTotalAmount = request.DTotalAmount,
                        TaxAmount = request.TaxAmount,
                        GrossAmount = request.GrossAmount,
                        MissChargeAmount = request.MissChargeAmount,
                        CgstAmount = request.CgstAmount,
                        SgstAmount = request.SgstAmount
                    },
                    transaction);

                if (masterInserted == 0)
                {
                    throw new Exception(
                        "Item Damage Master record was not inserted.");
                }

                // 5. INSERT DETAILS + UPDATE STOCK + PURCHASE
                foreach (var item in request.Details)
                {
                    var purchaseDetailSql = @"SELECT  ISNULL(PItemQty, 0) AS PItemQty,ISNULL(PAQ, 0) AS PAQ,
                    ISNULL(PItemReturnQty, 0) AS PItemReturnQty FROM PurchaseDetail
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode;";

                    var purchaseDetail = await connection.QueryFirstOrDefaultAsync(
                        purchaseDetailSql,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = item.PNo,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    decimal OriginalQty = purchaseDetail?.PItemQty == null ? 0m : Convert.ToDecimal(purchaseDetail.PItemQty);

                    // Insert ItemDamageDetail
                    const string detailQuery = @"
                    INSERT INTO ItemDamageDetail
                    (
                        DNo,
                        ItemCode,
                        DItemRate,
                        DItemQty,
                        ItemBalQty,
                        Branch_Code,
                        ItemQty,
                        PNo,
                        OrginalQty,
                        UnitCode,
                        Unit,
                        MainUnit,
                        MainUnitConverstion
                    )
                    VALUES
                    (
                        @DNo,
                        @ItemCode,
                        @DItemRate,
                        @DItemQty,
                        @ItemBalQty,
                        @BranchCode,
                        @ItemQty,
                        @PNo,
                        @OrginalQty,
                        @UnitCode,
                        @Unit,
                        @MainUnit,
                        @MainUnitConverstion
                    )";

                    int detailInserted = await connection.ExecuteAsync(
                        detailQuery,
                        new
                        {
                            DNo = dNo,
                            ItemCode = item.ItemCode,
                            DItemRate = item.DItemRate,
                            DItemQty = item.DItemQty,
                            ItemBalQty = item.ItemBalQty,
                            BranchCode = request.BranchCode,
                            ItemQty = item.ItemQty,
                            PNo = item.PNo,
                            OrginalQty = OriginalQty,
                            UnitCode = item.UnitCode,
                            Unit = item.Unit,
                            MainUnit=item.MainUnit,
                            MainUnitConverstion = item.MainUnitConverstion
                        },
                        transaction);

                    if (detailInserted == 0)
                    {
                        throw new Exception($"Item Damage Detail not inserted for ItemCode {item.ItemCode}");
                    }

                    // Update InventoryItemMaster
                    const string itemMasterQuery = @"UPDATE InventoryItemMaster 
                    SET ItemOpStock = ISNULL(ItemOpStock, 0) - @ItemBalQty
                    WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";

                    int itemMasterUpdated = await connection.ExecuteAsync(
                        itemMasterQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            ItemBalQty = item.ItemBalQty,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    if (itemMasterUpdated == 0)
                    {
                        throw new Exception(
                            $"ItemMaster not found for ItemCode {item.ItemCode}");
                    }

                    // Update Stock
                    //const string stockQuery = @"UPDATE Stock SET
                    //PRQty = ISNULL(PRQty, 0) + @ItemBalQty,
                    //CPQty = ISNULL(CPQty, 0) - @ItemBalQty
                    //WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";

                    //int stockUpdated = await connection.ExecuteAsync(
                    //    stockQuery,
                    //    new
                    //    {
                    //        ItemCode = item.ItemCode,
                    //        ItemBalQty = item.ItemBalQty,
                    //        BranchCode = request.BranchCode
                    //    },
                    //    transaction);

                    //if (stockUpdated == 0)
                    //{
                    //    throw new Exception(
                    //        $"Stock record not found for ItemCode {item.ItemCode}");
                    //}

                    // Update PurchaseDetail
                    const string purchaseUpdateQuery = @"UPDATE PurchaseDetail
                    SET DamageQty =ISNULL(DamageQty, 0) + @DItemQty
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode";

                    int purchaseUpdated = await connection.ExecuteAsync(
                        purchaseUpdateQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = item.PNo,
                            DItemQty = item.DItemQty,
                            BranchCode = request.BranchCode
                        },
                        transaction);

                    if (purchaseUpdated == 0)
                    {
                        throw new Exception(
                            $"PurchaseDetail not found for " +
                            $"ItemCode {item.ItemCode}, PNo {item.PNo}");
                    }
                }
                transaction.Commit();
                return dNo;
            }
            catch
            {
                throw;
            }
        }

        private async Task<bool> CheckInIssueAsync(IDbConnection connection, IDbTransaction transaction,int pNo,string branchCode)
        {
            const string query = @"SELECT COUNT(1) FROM ItemIssuedetail
            WHERE PNo = @PNo AND Branch_Code = @BranchCode";

            int count = await connection.ExecuteScalarAsync<int>(
                query,
                new
                {
                    PNo = pNo,
                    BranchCode = branchCode
                },
                transaction);
            return count > 0;
        }

        public async Task<List<ItemDamageListResponselist>> GetPurchaseOrderItemDamageList(string branchCode,int damageNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string masterQuery = @"SELECT M.DNo,M.DDate,M.Branch_Code,ISNULL(M.DTotalAmount, 0) AS DTotalAmount,
            D.ItemCode,ISNULL(I.ItemName, '') AS ItemName,D.PNo,ISNULL(D.ItemQty, 0) AS ItemQty,
            ISNULL(D.DItemQty, 0) AS DItemQty,ISNULL(D.DItemRate, 0) AS DItemRate,
            ISNULL(D.DItemQty, 0) * ISNULL(D.DItemRate, 0) AS Amount,ISNULL(M.TaxAmount, 0) AS TaxAmount,
            ISNULL(M.GrossAmount, 0) AS GrossAmount,ISNULL(M.MissChargeAmount, 0) AS MissChargeAmount,
            ISNULL(M.CgstAmount, 0) AS CgstAmount,ISNULL(M.SgstAmount, 0) AS SgstAmount
            FROM ItemDamageMaster M
            INNER JOIN ItemDamageDetail D ON M.DNo = D.DNo AND M.Branch_Code = D.Branch_Code
            LEFT JOIN InventoryItemMaster I ON D.ItemCode = I.ItemCode AND D.Branch_Code = I.Branch_Code
            WHERE M.Branch_Code = @BranchCode AND (@DNo IS NULL OR M.DNo = @DNo)
            ORDER BY M.DNo DESC, D.ItemCode ASC;";

            const string detailQuery = @"SELECT D.DNo,D.ItemCode,ISNULL(D.DItemRate, 0) AS DItemRate,
            ISNULL(D.DItemQty, 0) AS DItemQty,ISNULL(D.ItemQty, 0) AS ItemQty,D.PNo,
            ISNULL(D.Unit, '') AS Unit,ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.ItemBalQty, 0) AS ItemBalQty,
            ISNULL(D.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(D.MainUnit, '') AS MainUnit,D.Branch_Code
            FROM ItemDamageDetail D
            LEFT JOIN InventoryItemMaster I ON I.ItemCode = D.ItemCode
            AND I.Branch_Code = D.Branch_Code
            WHERE D.Branch_Code = @BranchCode AND D.DNo = @DNo
            ORDER BY D.ItemCode;";

            var masters = (await connection.QueryAsync<ItemDamageListResponse>(
                masterQuery,
                new
                {
                    BranchCode = branchCode,
                    DNo = damageNo
                }
            )).ToList();
            if (masters.Count == 0)
            {
                return new List<ItemDamageListResponselist>();
            }
            var details = (await connection.QueryAsync<ItemDamageDetailRequest>(
                detailQuery,
                new
                {
                    BranchCode = branchCode,
                    DNo = damageNo
                }
            )).ToList();
            var result = masters
                .GroupBy(x => new
                {
                    x.DNo,
                    x.DDate,
                    x.Branch_Code,
                    x.DTotalAmount,
                    x.TaxAmount,
                    x.GrossAmount,
                    x.MissChargeAmount,
                    x.CgstAmount,
                    x.SgstAmount
                })
                .Select(group => new ItemDamageListResponselist
                {
                    Master = group.First(),
                    Details = details
                        .Where(x =>
                            x.DNo == group.Key.DNo &&
                            x.Branch_Code == group.Key.Branch_Code)
                        .ToList()
                })
                .ToList();
            return result;
        }
        #endregion

        #region Indent Order
        //public async Task<ItemDetailsResponse?> GetItemDetailsIndentOrder(GetItemDetailsRequest request)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    const string itemSql = @"SELECT i.ItemCode,i.ItemName,ISNULL(i.ItemRate, 0) AS ItemRate,
        //    (ISNULL((SELECT SUM(ISNULL(pd.PItemQty, 0) - ISNULL(pd.PItemReturnQty, 0) - ISNULL(pd.DamageQty, 0))
        //    FROM PurchaseDetail pd WHERE pd.ItemCode = i.ItemCode
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR pd.StoredId = @StoreId)), 0) -
        //    ISNULL(( SELECT SUM(ISNULL(iod.IOItemQty, 0) - ISNULL(iod.AvailableQty, 0)) FROM IndentOrderDetail iod
        //    WHERE iod.ItemCode = i.ItemCode AND iod.Branch_Code = @BranchCode
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR iod.StoreId = @StoreId)), 0)) AS AvailableQty,

        //    ISNULL((SELECT TOP 1 pd.UnitCode FROM PurchaseDetail pd WHERE pd.ItemCode = i.ItemCode 
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR pd.StoredId = @StoreId)), 0) AS UnitCode,

        //    ISNULL((SELECT TOP 1 pd.Unit FROM PurchaseDetail pd WHERE pd.ItemCode = i.ItemCode
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR pd.StoredId = @StoreId)), '') AS UnitName,

        //    ISNULL((SELECT TOP 1 pd.MainUnit FROM PurchaseDetail pd WHERE pd.ItemCode = i.ItemCode 
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR pd.StoredId = @StoreId)), '') AS MainUnit,

        //    ISNULL((SELECT TOP 1 pd.MainUnitConverstion FROM PurchaseDetail pd WHERE pd.ItemCode = i.ItemCode
        //    AND (@StoreId IS NULL OR @StoreId = 0 OR pd.StoredId = @StoreId)), '') AS MainUnitConverstion
        //    FROM InventoryItemMaster i
        //    WHERE i.ItemCode = @ItemCode AND i.Branch_Code = @BranchCode;";

        //    var item = await connection.QueryFirstOrDefaultAsync<ItemDetailsResponse>(
        //        itemSql,
        //        new
        //        {
        //            ItemCode = request.ItemCode,
        //            BranchCode = request.BranchCode,
        //            StoreId = request.StoreId
        //        });

        //    return item;
        //}

        public async Task<List<ItemDetailsResponse>> GetItemDetailsIndentOrder(GetItemDetailsRequest request)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //ISNULL(pd.PItemQty - ISNULL(pd.PItemReturnQty, 0) - ISNULL(pd.DamageQty, 0), 0) AS AvailableQty,

            const string itemSql = @"SELECT pd.PNo, pd.PONo,i.ItemCode,i.ItemName,ISNULL(pd.PItemRate, 0) AS ItemRate,
            CASE WHEN (ISNULL(pd.PItemQty, 0) - ISNULL(pd.PItemReturnQty, 0) - ISNULL(pd.DamageQty, 0)- 
            CASE WHEN ISNULL(pd.IndentApprovedQty, 0) > 0
            THEN ISNULL(pd.IndentApprovedQty, 0) ELSE ISNULL(pd.IndentOrderQty, 0) END) < 0 THEN 0 
            ELSE (ISNULL(pd.PItemQty, 0) - ISNULL(pd.PItemReturnQty, 0) - ISNULL(pd.DamageQty, 0) - 
            CASE WHEN ISNULL(pd.IndentApprovedQty, 0) > 0 THEN ISNULL(pd.IndentApprovedQty, 0) 
            ELSE ISNULL(pd.IndentOrderQty, 0) END) END AS AvailableQty,
            ISNULL(pd.PItemQty, 0) as PItemQty,ISNULL(pd.PItemReturnQty, 0) as PItemReturnQty, 
            ISNULL(pd.DamageQty, 0) DamageQty,pd.unit AS UnitName,ISNULL(pd.StoredId, '') StoredId, 
            pd.UnitCode,pd.MainUnit,pd.MainUnitConverstion,Pd.Branch_Code FROM PurchaseDetail pd
            INNER JOIN InventoryItemMaster i ON i.ItemCode = pd.ItemCode AND i.Branch_Code = pd.Branch_Code
            WHERE i.ItemCode = @ItemCode AND i.Branch_Code = @BranchCode;";

            var items = await connection.QueryAsync<ItemDetailsResponse>(
                itemSql,
                new
                {
                    ItemCode = request.ItemCode,
                    BranchCode = request.BranchCode,
                    StoreId = request.StoreId
                });

            return items.ToList();
        }

        public async Task<int> SaveIndentOrderAsync(IndentOrderSaveRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new Exception("Can't Save. List is Empty.");
            }
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert Master
                const string masterSql = @"
                INSERT INTO IndentOrderMaster
                (
                    PurchaseNo,
                    IONo,
                    IODate,
                    SupCode,
                    Billed,
                    Branch_Code,
                    OrderBy,
                    DepCode,
                    TaxAmount,
                    TotalAmount,
                    GrossAmount,
                    MissChargeAmount,
                    CgstAmount,
                    SgstAmount,
                    StoreId,
                    POValidDate,
                    status
                )
                VALUES
                (
                    @PurchaseNo,
                    @IONo,
                    @IODate,
                    @StoreCode,
                    @Billed,
                    @BranchCode,
                    @OrderBy,
                    @DepCode,
                    @TaxAmount,
                    @TotalAmount,
                    @GrossAmount,
                    @MissChargeAmount,
                    @CgstAmount,
                    @SgstAmount,
                    @StoreId,
                    @POValidDate,
                    @status
                );";

                await connection.ExecuteAsync(
                    masterSql,
                    new
                    {

                        PurchaseNo = string.Join(",", request.Items.Select(x => x.PNo.ToString())),
                        IONo = request.IONo,
                        IODate = request.IODate,
                        StoreCode = request.StoreCode,
                        Billed = request.Billed,
                        BranchCode = request.BranchCode,
                        OrderBy = request.OrderBy.ToUpper(),
                        DepCode = request.DepCode,
                        TaxAmount= request.TaxAmount,
                        TotalAmount = request.TotalAmount,
                        GrossAmount = request.GrossAmount,
                        MissChargeAmount = request.MissChargeAmount,
                        CgstAmount = request.CgstAmount,
                        SgstAmount = request.SgstAmount,
                        StoreId = request.StoreId,
                        POValidDate=request.POValidDate,
                        status=request.Status
                    },
                    transaction);

                // Insert Details
                const string detailSql = @"
                INSERT INTO IndentOrderDetail
                (
                    PNo,
                    IONo,
                    ItemCode,
                    IOItemQty,
                    IOItemRate,
                    Branch_Code,
                    unit,
                    UnitCode,
                    IOItemSuplyQty,
                    CPOItemQty,
                    MainUnit,
                    MainUnitConverstion,
                    StoreId,
                    IOORG,
                    IOAQty,
                    IndentOrderQty
                )
                VALUES
                (
                    @PNo,
                    @IONo,
                    @ItemCode,
                    @IOItemQty,
                    @IOItemRate,
                    @BranchCode,
                    @Unit,
                    @UnitCode,  
                    0,
                    0,
                    @MainUnit,
                    @MainUnitConverstion,
                    @StoreId,
                    @IOORG,
                    @IOAQty,
                    @IndentOrderQty
                );";

                foreach (var item in request.Items)
                {
                    if (item.ItemCode <= 0)
                    {
                        throw new Exception("Invalid ItemCode.");
                    }
                    if (item.IOItemQty <= 0)
                    {
                        throw new Exception(
                            $"Quantity must be greater than 0 for ItemCode {item.ItemCode}.");
                    }
                    await connection.ExecuteAsync(
                        detailSql,
                        new
                        {
                            PNo = item.PNo,
                            IONo = request.IONo,
                            ItemCode = item.ItemCode,
                            IOItemQty = item.IOItemQty,
                            IOItemRate = item.IOItemRate,
                            BranchCode = request.BranchCode,
                            Unit = item.Unit,
                            UnitCode = item.UnitCode,
                            MainUnit=item.MainUnit,
                            MainUnitConverstion = item.MainUnitConverstion,
                            StoreId = request.StoreId,
                            IOORG = item.IOOrginalQty,
                            IOAQty = item.IOAvailableQty,
                            IndentOrderQty = item.IOItemQty,
                        },
                        transaction);

                    const string purchaseUpdateQuery = @"UPDATE PurchaseDetail
                    SET IndentOrderQty =ISNULL(IndentOrderQty, 0) + @IndentOrderQty
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode";

                    int purchaseUpdated = await connection.ExecuteAsync(
                        purchaseUpdateQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = item.PNo,
                            IndentOrderQty = item.IOItemQty,
                            BranchCode = request.BranchCode
                        },
                        transaction);
                }

                transaction.Commit();
                return request.IONo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<IndentOrderListResponse>> GetIndentOrderListPrint(int ioNo, string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT M.IONo,M.IODate,M.POValidDate,ISNULL(M.SupCode, '') AS StoreCode,
            ISNULL(M.Billed, '') AS Billed,ISNULL(M.Branch_Code, '') AS BranchCode,
            ISNULL(M.OrderBy, '') AS OrderBy,ISNULL(M.DepCode, '') AS DepCode,
            ISNULL(M.TaxAmount, 0) AS TaxAmount,ISNULL(M.TotalAmount, 0) AS TotalAmount,
            ISNULL(M.GrossAmount, 0) AS GrossAmount,ISNULL(M.MissChargeAmount, 0) AS MissChargeAmount,
            ISNULL(M.CgstAmount, 0) AS CgstAmount,ISNULL(M.SgstAmount, 0) AS SgstAmount,
            ISNULL(M.StoreId, 0) AS StoreId,ISNULL(M.POValidDate, M.IODate) AS POValidDate,
            ISNULL(M.status, '') AS Status, ISNULL(M.PurchaseNo, '') AS PurchaseNo FROM IndentOrderMaster M
            WHERE M.Branch_Code = @BranchCode AND (@IONo = 0 OR M.IONo = @IONo)
            ORDER BY M.IONo DESC;

            SELECT D.IONo,D.ItemCode,ISNULL(I.ItemName, '') AS ItemName,ISNULL(D.IOItemQty, 0) AS IOItemQty,
            ISNULL(D.IOItemRate, 0) AS IOItemRate,ISNULL(D.unit, '') AS Unit,ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IOItemSuplyQty, 0) AS IOItemSuplyQty,ISNULL(D.CPOItemQty, 0) AS CPOItemQty,
            ISNULL(D.MainUnit, '') AS MainUnit,ISNULL(D.MainUnitConverstion, 0) AS MainUnitConverstion,
            ISNULL(D.StoreId, 0) AS StoreId,ISNULL(D.IOORG, 0) AS IOOrginalQty,
            ISNULL(D.IOAQty, 0) AS IOAvailableQty,ISNULL(D.PNo, 0) AS PNo,
            ISNULL(D.Branch_Code, '') AS Branch_Code
            FROM IndentOrderDetail D
            LEFT JOIN InventoryItemMaster I ON D.ItemCode = I.ItemCode AND D.Branch_Code = I.Branch_Code
            WHERE D.Branch_Code = @BranchCode AND (@IONo = 0 OR D.IONo = @IONo)
            ORDER BY D.IONo DESC, D.ItemCode;";


            using var multi = await connection.QueryMultipleAsync(query,
                new
                {
                    BranchCode = branchCode,
                    IONo = ioNo
                });

            var masters = (await multi.ReadAsync<IndentOrderMasterList>()).ToList();
            var details = (await multi.ReadAsync<IndentOrderSaveItem>()).ToList();
            var result = masters.Select(master => new IndentOrderListResponse
            {
                Master = master,
                Details = details.Where(x => x.IONo == master.IONo && x.Branch_Code == master.BranchCode).ToList()
            }).ToList();

            return result;
        }
        #endregion

        #region Indent Order Approval
        public async Task<List<IndentOrderListResponse>> GetIndentOrderList(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT M.IONo,M.IODate,M.POValidDate,ISNULL(M.SupCode, '') AS StoreCode,
            ISNULL(M.Billed, '') AS Billed,ISNULL(M.Branch_Code, '') AS BranchCode,
            ISNULL(M.OrderBy, '') AS OrderBy,ISNULL(M.DepCode, '') AS DepCode,
            ISNULL(M.TaxAmount, 0) AS TaxAmount,ISNULL(M.TotalAmount, 0) AS TotalAmount,
            ISNULL(M.GrossAmount, 0) AS GrossAmount,ISNULL(M.MissChargeAmount, 0) AS MissChargeAmount,
            ISNULL(M.CgstAmount, 0) AS CgstAmount,ISNULL(M.SgstAmount, 0) AS SgstAmount,
            ISNULL(M.StoreId, 0) AS StoreId,ISNULL(M.POValidDate, M.IODate) AS POValidDate,
            ISNULL(M.status, '') AS Status,ISNULL(M.PurchaseNo, '') AS PurchaseNo,ISNULL(D.DepName, '') AS DepName,
            ISNULL(S.StoreName, '') AS StoreName
            FROM IndentOrderMaster M
            LEFT JOIN Department D ON M.DepCode = D.DepCode
            LEFT JOIN StoreMaster S ON M.StoreId = S.StoreId
            WHERE M.Branch_Code = @BranchCode
            ORDER BY M.IONo DESC;

            SELECT D.IONo,D.ItemCode,ISNULL(I.ItemName, '') AS ItemName,ISNULL(D.IOItemQty, 0) AS IOItemQty,
            ISNULL(D.IOItemRate, 0) AS IOItemRate,ISNULL(D.unit, '') AS Unit,ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IOItemSuplyQty, 0) AS IOItemSuplyQty,ISNULL(D.CPOItemQty, 0) AS CPOItemQty,
            ISNULL(D.MainUnit, '') AS MainUnit,ISNULL(D.MainUnitConverstion, 0) AS MainUnitConverstion,
            ISNULL(D.StoreId, 0) AS StoreId,ISNULL(D.IOORG, 0) AS IOOrginalQty,
            ISNULL(D.IOAQty, 0) AS IOAvailableQty,ISNULL(D.Branch_Code, '') AS Branch_Code,
            ISNULL(D.ApprovedQty, 0) AS ApprovedQty,ISNULL(D.PNo, 0) AS PNo,
            CASE  WHEN ISNULL(d.IOAQty, 0) - ISNULL(d.AvailableQty, 0) < 0 
            THEN 0 ELSE ISNULL(d.IOAQty, 0) - ISNULL(d.AvailableQty, 0) END AS ReamingQty
            FROM IndentOrderDetail D
            LEFT JOIN InventoryItemMaster I ON D.ItemCode = I.ItemCode AND D.Branch_Code = I.Branch_Code
            WHERE D.Branch_Code = @BranchCode
            ORDER BY D.IONo DESC, D.ItemCode;";


            using var multi = await connection.QueryMultipleAsync(query,
                new
                {
                    BranchCode = branchCode,
                });

            var masters =(await multi.ReadAsync<IndentOrderMasterList>()).ToList();
            var details =(await multi.ReadAsync<IndentOrderSaveItem>()).ToList();
            var result = masters.Select(master => new IndentOrderListResponse
            {
                Master = master,
                Details = details.Where(x =>x.IONo == master.IONo && x.Branch_Code == master.BranchCode).ToList()
            }).ToList();

            return result;
        }

        public async Task<int> IndentOrderApprovalSave(IndentOrderApprovalSaveRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Items == null || request.Items.Count == 0)
                throw new Exception("Can't Save. List is Empty.");

            if (request.IONo <= 0)
                throw new Exception("Indent Order No. is required.");

            if (string.IsNullOrWhiteSpace(request.Branch_Code))
                throw new Exception("Branch Code is required.");

            if (string.IsNullOrWhiteSpace(request.DepCode))
                throw new Exception("Department Code is required.");

            if (string.IsNullOrWhiteSpace(request.OrderBy))
                throw new Exception("Entered By is required.");

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string masterSql = @"
                INSERT INTO IndentOrderApprovalMaster
                (
                    PurchaseNo,
                    IONo,
                    IODate,
                    SupCode,
                    Billed,
                    StoreId,
                    POValidDate,
                    Status,
                    Branch_Code,
                    OrderBy,
                    Approvedby,
                    ApprovedDate,
                    DepCode,
                    TotalAmount,
                    TaxAmount,
                    GrossAmount,
                    MissChargeAmount,
                    CgstAmount,
                    SgstAmount
                )
                VALUES
                (
                    @PurchaseNo,
                    @IONo,
                    @IODate,
                    @SupCode,
                    @Billed,
                    @StoreId,
                    @POValidDate,
                    @Status,
                    @Branch_Code,
                    @OrderBy,
                    @ApprovedBy,
                    @ApprovedDate,
                    @DepCode,
                    @TotalAmount,
                    @TaxAmount,
                    @GrossAmount,
                    @MissChargeAmount,
                    @CgstAmount,
                    @SgstAmount
                );";

                await connection.ExecuteAsync(
                    masterSql,
                    new
                    {
                        PurchaseNo = string.Join(",", request.Items.Select(x => x.PNo.ToString())),
                        request.IONo,
                        request.IODate,
                        request.SupCode,
                        request.Billed,
                        request.StoreId,
                        request.POValidDate,
                        request.Status,
                        request.Branch_Code,
                        request.OrderBy,
                        request.ApprovedBy,
                        request.ApprovedDate,
                        request.DepCode,
                        request.TotalAmount,
                        request.TaxAmount,
                        request.GrossAmount,
                        request.MissChargeAmount,
                        request.CgstAmount,
                        request.SgstAmount,
                    },
                    transaction);

                const string detailSql = @"
                    INSERT INTO IndentOrderApprovalDetail
                    (
                        PNo,
                        IONo,
                        ItemCode,
                        IOItemQty,
                        IOItemRate,
                        StoreId,
                        Branch_Code,
                        Unit,
                        UnitCode,
                        ApprovedBy,
                        ApprovedDate,
                        OrginalQty,
                        ApprovedQty,
                        BalanceQty,
                        IndentQty,
                        MainUnit,
                        MainUnitConverstion
                    )
                    VALUES
                    (
                        @PNo,
                        @IONo,
                        @ItemCode,
                        @IOItemQty,
                        @IOItemRate,
                        @StoreId,
                        @Branch_Code,
                        @Unit,
                        @UnitCode,
                        @ApprovedBy,
                        @ApprovedDate,
                        @OrginalQty,
                        @ApprovedQty,
                        @BalanceQty,
                        @IndentQty,
                        @MainUnit,
                        @MainUnitConverstion
                    );";

                foreach (var item in request.Items)
                {
                    const string getPOItemQtyQuery = @"SELECT ISNULL(IOORG, CAST(0 AS DECIMAL(18,2))) AS IOORG,
                    ISNULL(IOItemQty, CAST(0 AS DECIMAL(18,2))) AS IndentQty
                    FROM IndentOrderDetail WHERE IONo = @IONo AND ItemCode = @ItemCode
                    AND Branch_Code = @Branch_Code;";

                    var poItem = await connection.QuerySingleOrDefaultAsync<dynamic>(
                        getPOItemQtyQuery,
                        new
                        {
                            IONo = item.IONo,
                            ItemCode = item.ItemCode,
                            Branch_Code = request.Branch_Code
                        },
                        transaction
                    );

                    decimal OrginalQty = Convert.ToDecimal(poItem?.IOORG ?? 0);
                    decimal IndentQty = Convert.ToDecimal(poItem?.IndentQty ?? 0);

                    await connection.ExecuteAsync(
                        detailSql,
                        new
                        {
                            PNo = item.PNo,
                            IONo = request.IONo,
                            ItemCode = item.ItemCode,
                            IOItemQty = item.IOItemQty,
                            IOItemRate = item.IOItemRate,
                            StoreId = request.StoreId,
                            Branch_Code = request.Branch_Code,
                            Unit = item.Unit,
                            UnitCode = item.UnitCode,
                            ApprovedBy = request.ApprovedBy,
                            ApprovedDate = request.ApprovedDate,
                            OrginalQty = OrginalQty,
                            IndentQty = IndentQty,
                            ApprovedQty = item.ApprovedQty,
                            BalanceQty = item.AvailableQty,
                            MainUnit = item.MainUnit,
                            MainUnitConverstion = item.MainUnitConverstion
                        },
                        transaction
                    );

                    const string updateDetailSql = @"UPDATE IndentOrderDetail 
                    SET AvailableQty = @AvailableQty,ApprovedQty = @ApprovedQty
                    WHERE IONo = @IONo AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                    await connection.ExecuteAsync(
                        updateDetailSql,
                        new
                        {
                            IONo = request.IONo,
                            ItemCode = item.ItemCode,
                            AvailableQty = item.AvailableQty,
                            ApprovedQty = item.ApprovedQty,
                            Branch_Code = request.Branch_Code
                        },
                        transaction
                    );

                    const string purchaseUpdateQuery = @"UPDATE PurchaseDetail
                    SET IndentApprovedQty =ISNULL(IndentApprovedQty, 0) + @IndentApprovedQty
                    WHERE ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode";

                    int purchaseUpdated = await connection.ExecuteAsync(
                        purchaseUpdateQuery,
                        new
                        {
                            ItemCode = item.ItemCode,
                            PNo = item.PNo,
                            IndentApprovedQty = item.ApprovedQty,
                            BranchCode = request.Branch_Code
                        },
                        transaction);
                }
                const string updateMasterSql = @"UPDATE IndentOrderMaster 
                SET Status = @Status
                WHERE IONo = @IONo AND Branch_Code = @Branch_Code;";

                await connection.ExecuteAsync(
                    updateMasterSql,
                    new
                    {
                        request.IONo,
                        request.Status,
                        request.Branch_Code
                    },
                    transaction);
                transaction.Commit();

                return request.IONo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalPrintList(string branchCode, int ioNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT M.IONo,M.IODate,ISNULL(M.POValidDate, M.IODate) AS POValidDate,
            ISNULL(M.SupCode, 0) AS SupCode,ISNULL(M.Billed, '') AS Billed,ISNULL(M.Branch_Code, '') AS Branch_Code,
            ISNULL(M.OrderBy, '') AS OrderBy,ISNULL(M.Approvedby, '') AS ApprovedBy,ISNULL(M.Status, '') AS Status,
            ISNULL(M.DepCode, '') AS DepCode,ISNULL(M.CgstAmount, 0) AS CgstAmount,ISNULL(M.SgstAmount, 0) AS SgstAmount,
            ISNULL(M.MissChargeAmount, 0) AS MissChargeAmount,ISNULL(M.TotalAmount, 0) AS TotalAmount,
            ISNULL(M.TaxAmount, 0) AS TaxAmount,ISNULL(M.GrossAmount, 0) AS GrossAmount,
            ISNULL(M.StoreId, '') AS StoreId,ISNULL(M.ApprovedDate, M.IODate) AS ApprovedDate
            FROM IndentOrderApprovalMaster M
            WHERE M.Branch_Code = @BranchCode AND M.IONo = @IONo
            ORDER BY M.IONo DESC;

            SELECT D.IONo,D.ItemCode,ISNULL(D.Unit, '') AS Unit,ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IOItemQty, 0) AS IOItemQty,ISNULL(D.IOItemRate, 0) AS IOItemRate,
            ISNULL(D.Branch_Code, '') AS BranchCode,ISNULL(D.StoreId, '') AS StoreId,
            ISNULL(D.ApprovedBy, '') AS ApprovedBy,ISNULL(D.ApprovedDate, GETDATE()) AS ApprovedDate,
            ISNULL(D.OrginalQty, 0) AS OrginalQty,ISNULL(D.ApprovedQty, 0) AS ApprovedQty,
            ISNULL(D.BalanceQty, 0) AS AvailableQty,ISNULL(D.MainUnit, '') AS MainUnit,
            ISNULL(D.MainUnitConverstion, '') AS MainUnitConverstion,ISNULL(D.IndentQty, 0) AS IndentQty,
            ISNULL(I.ItemName, '') AS ItemName FROM IndentOrderApprovalDetail D
            LEFT JOIN InventoryItemMaster I ON D.ItemCode = I.ItemCode AND D.Branch_Code = I.Branch_Code
            WHERE D.Branch_Code = @BranchCode AND D.IONo = @IONo
            ORDER BY D.ItemCode;";

            using var multi = await connection.QueryMultipleAsync(
                query,
                new
                {
                    BranchCode = branchCode,
                    IONo = ioNo
                });

            var masters = (await multi.ReadAsync<IndentOrderApprovalModel>()).ToList();
            var details = (await multi.ReadAsync<IndentOrderApprovalItemRequest>()).ToList();
            var result = masters.Select(master => new IndentOrderApprovalListDto
            {
                Master = master,
                Details = details.Where(x => x.IONo == master.IONo && x.BranchCode == master.Branch_Code).ToList()
            }).ToList();

            return result;
        }
        #endregion

        #region Item issue
        public async Task<List<IndentOrderSearchResponse>> SearchIndentOrderAsync(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            const string sql = @"SELECT IONo FROM IndentOrderApprovalMaster
             WHERE Branch_Code = @BranchCode ORDER BY IONo DESC";

            var result = await connection.QueryAsync<IndentOrderSearchResponse>(
                sql,
                new
                {
                    BranchCode = branchCode,
                });
            return result.ToList();
        }

        public async Task<List<IndentOrderApprovalListDto>> GetIndentOrderApprovalData(string branchCode, int ioNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT M.IONo,M.IODate,ISNULL(M.POValidDate, M.IODate) AS POValidDate,
            ISNULL(M.SupCode, 0) AS SupCode,ISNULL(M.Billed, '') AS Billed,ISNULL(M.Branch_Code, '') AS Branch_Code,
            ISNULL(M.OrderBy, '') AS OrderBy,ISNULL(M.Approvedby, '') AS ApprovedBy,ISNULL(M.Status, '') AS Status,
            ISNULL(M.DepCode, '') AS DepCode,ISNULL(M.CgstAmount, 0) AS CgstAmount,ISNULL(M.SgstAmount, 0) AS SgstAmount,
            ISNULL(M.MissChargeAmount, 0) AS MissChargeAmount,ISNULL(M.TotalAmount, 0) AS TotalAmount,
            ISNULL(M.TaxAmount, 0) AS TaxAmount,ISNULL(M.GrossAmount, 0) AS GrossAmount,
            ISNULL(M.StoreId, '') AS StoreId,ISNULL(M.ApprovedDate, M.IODate) AS ApprovedDate
            FROM IndentOrderApprovalMaster M
            WHERE M.Branch_Code = @BranchCode AND M.IONo = @IONo
            ORDER BY M.IONo DESC;

            SELECT D.IONo,D.ItemCode,ISNULL(D.Unit, '') AS Unit,ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IOItemQty, 0) AS IOItemQty,ISNULL(D.IOItemRate, 0) AS IOItemRate,
            ISNULL(D.Branch_Code, '') AS BranchCode,ISNULL(D.StoreId, '') AS StoreId,
            ISNULL(D.ApprovedBy, '') AS ApprovedBy,ISNULL(D.ApprovedDate, GETDATE()) AS ApprovedDate,
            ISNULL(D.OrginalQty, 0) AS OrginalQty,ISNULL(D.ApprovedQty, 0) AS ApprovedQty,
            ISNULL(D.BalanceQty, 0) AS AvailableQty,ISNULL(D.MainUnit, '') AS MainUnit,
            ISNULL(D.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(I.ItemName, '') AS ItemName
            FROM IndentOrderApprovalDetail D
            LEFT JOIN InventoryItemMaster I ON D.ItemCode = I.ItemCode AND D.Branch_Code = I.Branch_Code
            WHERE D.Branch_Code = @BranchCode AND D.IONo = @IONo
            ORDER BY D.ItemCode;";

            using var multi = await connection.QueryMultipleAsync(
                query,
                new
                {
                    BranchCode = branchCode,
                    IONo = ioNo
                });

            var masters = (await multi.ReadAsync<IndentOrderApprovalModel>()).ToList();
            var details = (await multi.ReadAsync<IndentOrderApprovalItemRequest>()).ToList();
            var result = masters.Select(master => new IndentOrderApprovalListDto
            {
                Master = master,
                Details = details.Where(x => x.IONo == master.IONo && x.BranchCode == master.Branch_Code).ToList()
            }).ToList();

            return result;
        }

        public async Task<int> ItemIssueSave(ItemIssueSaveRequest request)
        {
            if (request.Items == null || request.Items.Count == 0)
            {
                throw new Exception("Item list is empty.");
            }
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {

                const string masterSql = @"
                INSERT INTO ItemIssueMaster
                (
                    INo,
                    IDate,
                    DepCode,
                    ITotalAmount,
                    Billno,
                    DIssue,
                    PNO,
                    IType,
                    Branch_Code,
                    UserCode,
                    StoreId,
                    Status
                )
                VALUES
                (
                    @INo,
                    @IDate,
                    @DepCode,
                    @ITotalAmount,
                    @BillNo,
                    @DIssue,
                    @PNO,
                    @IType,
                    @Branch_Code,
                    @UserCode,
                    @StoreId,
                    @Status
                )";

                await connection.ExecuteAsync(
                    masterSql,
                    new
                    {
                        INo = request.INo,
                        IDate = request.IssueDate,
                        DepCode = request.DepCode,
                        ITotalAmount = request.TotalAmount,
                        BillNo = request.BillNo,
                        DIssue = 0,
                        PNO = 0,
                        IType = request.IssueType,
                        Branch_Code = request.Branch_Code,
                        UserCode = request.UserCode,
                        StoreId = request.StoreId,
                        Status= request.Status
                    },
                    transaction);

                const string detailSql = @"
                INSERT INTO ItemIssueDetail
                (
                    INo,
                    ItemCode,
                    IItemQty,
                    IItemRate,
                    unit,
                    UnitCode,
                    PerRate,
                    PNo,
                    Branch_Code,
                    QtyPer,
                    NoOfQty,
                    StoreId,
                    DepCode,
                    MainUnitConverstion,
                    MainUnit,
                    IItemReturnQty,
                    AvailableQty,
                    ReturnQty
                )
                VALUES
                (
                    @INo,
                    @ItemCode,
                    @IItemQty,
                    @IItemRate,
                    @Unit,
                    @UnitCode,
                    @PerRate,
                    @PNo,
                    @BranchCode,
                    @QtyPer,
                    @NoOfQty,
                    @StoreId,
                    @DepCode,
                    @MainUnitConverstion,
                    @MainUnit,
                    @IItemReturnQty,
                    @AvailableQty,
                    @ReturnQty
                )";

                foreach (var item in request.Items)
                {
                    await connection.ExecuteAsync(
                        detailSql,
                        new
                        {
                            INo = request.INo,
                            ItemCode = item.ItemCode,
                            IItemQty = Math.Round(item.IssueQty, 2),
                            IItemRate = Math.Round(item.ItemRate, 2),
                            Unit = item.Unit,
                            UnitCode = item.UnitCode,
                            PerRate = Math.Round(item.ItemRate, 2),
                            PNo = 0,
                            BranchCode = request.Branch_Code,
                            QtyPer = item.QtyPer,
                            NoOfQty = item.NoOfQty,
                            StoreId = request.StoreId,
                            DepCode = request.DepCode,
                            MainUnit = item.MainUnit,
                            MainUnitConverstion = item.MainUnitConverstion,
                            IItemReturnQty = item.ReturnQty,
                            AvailableQty = item.AvailableQty,
                            ReturnQty = item.ReturnQty
                        },
                        transaction);

                    if (request.IsMinibar)
                    {
                        const string minibarSql = @"
                        INSERT INTO Tbl_ItemIssue
                        (
                            INo,
                            IssueDate,
                            ItemCode,
                            IssueQty,
                            ItemRate,
                            DepCode,
                            Branch_Code
                        )
                        VALUES
                        (
                            @INo,
                            @IssueDate,
                            @ItemCode,
                            @IssueQty,
                            @ItemRate,
                            @DepCode,
                            @BranchCode
                        );";

                        await connection.ExecuteAsync(
                            minibarSql,
                            new
                            {
                                INo = request.INo,
                                IssueDate = request.IssueDate,
                                item.ItemCode,
                                IssueQty = Math.Round(item.IssueQty, 2),
                                ItemRate = Math.Round(item.ItemRate, 2),
                                DepCode = request.DepCode,
                                BranchCode = request.Branch_Code
                            },
                            transaction);
                    }
                }

                const string indentStatusSql = @"UPDATE IndentOrderApprovalMaster
                SET Status = @Status WHERE IONo = @IONo AND Branch_Code = @BranchCode";

                await connection.ExecuteAsync(
                    indentStatusSql,
                    new
                    {
                        Status=request.Status,
                        IONo = request.IndentNo,
                        BranchCode = request.Branch_Code
                    },
                    transaction);

                transaction.Commit();
                return request.INo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<List<ItemIssueListDto>> GetItemIssuePrintData(string branchCode, int iNo)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT
            M.INo,
            M.IDate,
            ISNULL(M.DepCode, '') AS DepCode,
            ISNULL(M.ITotalAmount, 0) AS ITotalAmount,
            ISNULL(M.Billno, '') AS BillNo,
            ISNULL(M.Branch_Code, '') AS BranchCode,
            ISNULL(M.DIssue, 0) AS DIssue,
            ISNULL(M.UserCode, '') AS UserCode,
            ISNULL(M.PNO, 0) AS PNO,
            ISNULL(M.IType, '') AS IssueType,
            ISNULL(M.StoreId, '') AS StoreId,
            ISNULL(M.Status, '') AS Status
        FROM ItemIssueMaster M
        WHERE M.Branch_Code = @BranchCode
          AND M.INo = @INo
        ORDER BY M.INo DESC;

        SELECT
            D.INo,
            D.ItemCode,
            ISNULL(D.Unit, '') AS Unit,
            ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IItemQty, 0) AS IssueQty,
            ISNULL(D.IItemRate, 0) AS ItemRate,
            ISNULL(D.PerRate, 0) AS PerRate,
            ISNULL(D.PNo, 0) AS PNo,
            ISNULL(D.Branch_Code, '') AS BranchCode,
            ISNULL(D.QtyPer, 0) AS QtyPer,
            ISNULL(D.NoOfQty, 0) AS NoOfQty,
            ISNULL(D.StoreId, '') AS StoreId,
            ISNULL(D.DepCode, '') AS DepCode,
            ISNULL(I.ItemName, '') AS ItemName,
            ISNULL(D.MainUnit, '') AS MainUnit,
            ISNULL(D.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(D.ReturnQty, 0) AS ReturnQty,
            ISNULL(D.AvailableQty, 0) AS AvailableQty,
            ISNULL(D.ReturnQty, 0) AS ReturnQty,
        FROM ItemIssueDetail D
        LEFT JOIN InventoryItemMaster I
            ON D.ItemCode = I.ItemCode
           AND D.Branch_Code = I.Branch_Code
        WHERE D.Branch_Code = @BranchCode
          AND D.INo = @INo
        ORDER BY D.ItemCode;";

            using var multi = await connection.QueryMultipleAsync(
                query,
                new
                {
                    BranchCode = branchCode,
                    INo = iNo
                });

            var masters = (await multi.ReadAsync<ItemIssueList>()).ToList();

            var items = (await multi.ReadAsync<ItemIssueDetailRequest>()).ToList();

            var result = masters.Select(master => new ItemIssueListDto
            {
                Master = master,

                Items = items
                    .Where(x =>
                        x.INo == master.INo &&
                        x.Branch_Code == master.Branch_Code)
                    .ToList()

            }).ToList();

            return result;
        }
        #endregion

        #region Item Issue Return
        public async Task<List<ItemIssueListDto>> GetItemIssueData(string branchCode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT
            M.INo,
            M.IDate,
            ISNULL(M.DepCode, '') AS DepCode,
            ISNULL(M.ITotalAmount, 0) AS ITotalAmount,
            ISNULL(M.Billno, '') AS BillNo,
            ISNULL(M.Branch_Code, '') AS BranchCode,
            ISNULL(M.DIssue, 0) AS DIssue,
            ISNULL(M.UserCode, '') AS UserCode,
            ISNULL(M.PNO, 0) AS PNO,
            ISNULL(M.IType, '') AS IssueType,
            ISNULL(M.StoreId, '') AS StoreId,
            ISNULL(M.Status, '') AS Status
        FROM ItemIssueMaster M
        WHERE M.Branch_Code = @BranchCode
        ORDER BY M.INo DESC;

        SELECT
            D.INo,
            D.ItemCode,
            ISNULL(D.Unit, '') AS Unit,
            ISNULL(D.UnitCode, 0) AS UnitCode,
            ISNULL(D.IItemQty, 0) AS IssueQty,
            ISNULL(D.IItemRate, 0) AS ItemRate,
            ISNULL(D.PerRate, 0) AS PerRate,
            ISNULL(D.PNo, 0) AS PNo,
            ISNULL(D.Branch_Code, '') AS BranchCode,
            ISNULL(D.QtyPer, 0) AS QtyPer,
            ISNULL(D.NoOfQty, 0) AS NoOfQty,
            ISNULL(D.StoreId, '') AS StoreId,
            ISNULL(D.DepCode, '') AS DepCode,
            ISNULL(I.ItemName, '') AS ItemName,
            ISNULL(D.MainUnit, '') AS MainUnit,
            ISNULL(D.MainUnitConverstion, '') AS MainUnitConverstion,
            ISNULL(D.ReturnQty, 0) AS ReturnQty,
            ISNULL(D.AvailableQty, 0) AS AvailableQty,
            ISNULL(D.ReturnQty, 0) AS ReturnQty,
        FROM ItemIssueDetail D
        LEFT JOIN InventoryItemMaster I
            ON D.ItemCode = I.ItemCode
           AND D.Branch_Code = I.Branch_Code
        WHERE D.Branch_Code = @BranchCode
        ORDER BY D.ItemCode;";

            using var multi = await connection.QueryMultipleAsync(
                query,
                new
                {
                    BranchCode = branchCode
                });

            var masters = (await multi.ReadAsync<ItemIssueList>()).ToList();

            var items = (await multi.ReadAsync<ItemIssueDetailRequest>()).ToList();

            var result = masters.Select(master => new ItemIssueListDto
            {
                Master = master,

                Items = items
                    .Where(x =>
                        x.INo == master.INo &&
                        x.Branch_Code == master.Branch_Code)
                    .ToList()

            }).ToList();

            return result;
        }
        public async Task<int> ItemIssueReturnSave(ItemIssueReturnSaveRequest request)
        {
            if (request == null)
                throw new Exception("Request is required.");

            if (request.Items == null || request.Items.Count == 0)
                throw new Exception("Item list is empty.");

            if (string.IsNullOrWhiteSpace(request.BranchCode))
                throw new Exception("Branch code is required.");

            if (request.IRNo <= 0)
                throw new Exception("Invalid Return No.");

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                const string masterSql = @"
            INSERT INTO ItemIssueReturnMaster
            (
                IRNo,
                IRDate,
                DepCode,
                IRTotalAmount,
                Branch_Code,
                Status,
                IType,
                StoredId,
                DeptCode
            )
            VALUES
            (
                @IRNo,
                @IRDate,
                @DepCode,
                @IRTotalAmount,
                @BranchCode,
                @Status,
                @IType,
                @StoredId,
                @DeptCode
            )";

                await connection.ExecuteAsync(
                    masterSql,
                    new
                    {
                        request.IRNo,
                        IRDate = request.IRDate,
                        DepCode = 0,
                        request.IRTotalAmount,
                        BranchCode = request.BranchCode,
                        Status = request.Status,
                        IType = request.IType,
                        StoredId = request.StoredId,
                        DeptCode = request.DeptCode
                    },
                    transaction);


                foreach (var item in request.Items)
                {

                    const string issueQtySql = @"
                SELECT isnull(IItemQty, 0) as  IItemQty FROM ItemIssueDetail
                WHERE INo = @INo AND ItemCode = @ItemCode AND PNo = @PNo AND Branch_Code = @BranchCode";

                    decimal? balanceQty = await connection.ExecuteScalarAsync<decimal?>(
                        issueQtySql,
                        new
                        {
                            item.INo,
                            item.ItemCode,
                            item.PNo,
                            request.BranchCode
                        },
                        transaction);

                    decimal OriginalQty = balanceQty ?? 0m;

                    const string detailSql = @"
                INSERT INTO ItemIssueReturnDetail
                (
                    IRNo,
                    INo,
                    ItemCode,
                    IRItemRate,
                    IRItemQty,
                    IRNoofQty,
                    Branch_Code,
                    PNO,
                    StoredId,
                    ReturnQty,
                    AvailableQty,
                    DeptCode,
                    OriginalQty

                )
                VALUES
                (
                    @IRNo,
                    @INo,
                    @ItemCode,
                    @IRItemRate,
                    @IRItemQty,
                    @IRNoofQty,
                    @BranchCode,
                    @PNo,
                    @StoredId,
                    @ReturnQty,
                    @AvailableQty,
                    @DeptCode,
                    @OriginalQty

                )";

                    await connection.ExecuteAsync(
                        detailSql,
                        new
                        {
                            request.IRNo,
                            item.INo,
                            item.ItemCode,
                            item.IRItemRate,
                            item.IRItemQty,
                            item.IRNoofQty,
                            request.BranchCode,
                            item.PNo,
                            request.StoredId,
                            ReturnQty = item.ReturnQty,
                            AvailableQty = item.AvailableQty,
                            DeptCode = request.DeptCode,
                            OriginalQty= OriginalQty

                        },
                        transaction);

                    const string issueDetailSql = @" UPDATE ItemIssueDetail SET
                    IItemReturnQty =ISNULL(IItemReturnQty, 0) + @IRItemQty,
                    IIRNoQty = ISNULL(IIRNoQty, 0) + @IssueQty,
                    AvailableQty =ISNULL(AvailableQty, 0) + @AvailableQty,
                    ReturnQty = ISNULL(ReturnQty, 0) + @ReturnQty
                    WHERE INo = @INo AND ItemCode = @ItemCode AND PNo = @PNo
                    AND Branch_Code = @BranchCode";

                    int issueUpdated = await connection.ExecuteAsync(
                        issueDetailSql,
                        new
                        {
                            IRItemQty = item.IRItemQty,
                            IssueQty = item.IRNoofQty,
                            ReturnQty = item.ReturnQty,
                            AvailableQty = item.AvailableQty,
                            item.INo,
                            item.ItemCode,
                            item.PNo,
                            request.BranchCode
                        },
                        transaction);

                    if (issueUpdated == 0)
                    {
                        throw new Exception(
                            $"ItemIssueDetail not found. Issue No: {item.INo}, Item: {item.ItemCode}");
                    }

                    const string purchaseDetailSql = @"UPDATE PurchaseDetail SET 
                    PItemReturnQty =ISNULL(PItemReturnQty, 0) - @ReturnQty
                    WHERE ItemCode = @ItemCode AND PNO = @PNo
                    AND Branch_Code = @BranchCode";

                    await connection.ExecuteAsync(
                        purchaseDetailSql,
                        new
                        {
                            ReturnQty = item.IRItemQty,
                            item.ItemCode,
                            item.PNo,
                            request.BranchCode
                        },
                        transaction);
                }
                transaction.Commit();
                return request.IRNo;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        #endregion Item Issue Return
    }
}
