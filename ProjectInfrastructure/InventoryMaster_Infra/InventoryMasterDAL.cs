using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.InventoryMasterModels;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.InventoryMasterServices;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using System.Transactions;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace HMS_360_PMS.ProjectInfrastructure.InventoryMaster_Infra
{
    public class InventoryMasterDAL : IInventoryMaster_Repository
    {
        private readonly DbConnectionFactory _factory;

        public InventoryMasterDAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        #region Suuplier Master

        public async Task<IEnumerable<InventorySupplierMaster>> GetSupplierList(string branchcode)
        {
            try{
                using var connection = _factory.CreateConnection(DbNames.POS);
                var sql =" SELECT SupCode, SupName,SupCPerson,SupAdd1,SupAdd2,SupAdd3,SupPhone,SupFax,SupMobile,\r\n" +
                         " SupLSTNo,SupLSTDate,SupCSTNo,SupCSTDate,AcGroupCode,AcCode,Email,GSTNo,TINNo,\r\n" +
                         " Branch_Code AS BranchCode,Susp_pincode as SuspPincode,Sup_City as SupCity \r\n" +
                         " FROM Supplier WHERE Branch_code=@BranchCode ORDER BY SupCode ";
                return await connection.QueryAsync<InventorySupplierMaster>(sql, new { BranchCode = branchcode });
            }catch (Exception ex){
                throw ex;
            }
        }

        public async Task<int> InsertSupplierDetails(InventorySupplierMaster supplier)
        {
            try{
                using var connection = _factory.CreateConnection(DbNames.POS);

                var sql = @"INSERT INTO Supplier(
                            SupCode,
                            SupName,
                            SupCPerson,
                            SupAdd1,
                            SupAdd2,
                            SupAdd3,
                            SupPhone,
                            SupFax,
                            SupMobile,
                            SupLSTNo,
                            SupLSTDate,
                            SupCSTNo,
                            SupCSTDate,
                            AcGroupCode,
                            AcCode,
                            Email,
                            Branch_Code,
                            Susp_pincode,
                            Sup_City,
                            GSTNo,
                            TINNo
                        )
                        VALUES
                        (
                            @SupCode,
                            @SupName,
                            @SupCPerson,
                            @SupAdd1,
                            @SupAdd2,
                            @SupAdd3,
                            @SupPhone,
                            @SupFax,
                            @SupMobile,
                            @SupLSTNo,
                            @SupLSTDate,
                            @SupCSTNo,
                            @SupCSTDate,
                            @AcGroupCode,
                            @AcCode,
                            @Email,
                            @BranchCode,
                            @SuspPincode,
                            @SupCity,
                            @GSTNo,
                            @TINNo
                        );
                        SELECT @SupCode;";
                return await connection.ExecuteScalarAsync<int>(sql, supplier);
            }catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateSupplierDetails(InventorySupplierMaster supplier)
        {
            try{
                using var connection = _factory.CreateConnection(DbNames.POS);

                var sql = @"UPDATE Supplier
                        SET
                            SupName=@SupName,
                            SupCPerson=@SupCPerson,
                            SupAdd1=@SupAdd1,
                            SupAdd2=@SupAdd2,
                            SupAdd3=@SupAdd3,
                            SupPhone=@SupPhone,
                            SupFax=@SupFax,
                            SupMobile=@SupMobile,
                            SupLSTNo=@SupLSTNo,
                            SupLSTDate=@SupLSTDate,
                            SupCSTNo=@SupCSTNo,
                            SupCSTDate=@SupCSTDate,
                            AcGroupCode=@AcGroupCode,
                            AcCode=@AcCode,
                            Email=@Email,
                            Susp_pincode=@SuspPincode,
                            Sup_City=@SupCity,
                            GSTNo=@GSTNo,
                            TINNo=@TINNo
                        WHERE SupCode=@SupCode AND Branch_Code=@BranchCode";

                var rows = await connection.ExecuteAsync(sql, supplier);

                return rows > 0;
            }
            catch(Exception ex){
                throw ex;
            }
        }

        public async Task<bool> DeleteSupplierDetails(int SupCode, string branchcode)
        {
            try{
                using var connection = _factory.CreateConnection(DbNames.POS);

                var sql = @"DELETE FROM Supplier WHERE SupCode=@Id AND Branch_Code=@Branch";
                var rows = await connection.ExecuteAsync(sql, new { Id = SupCode, Branch = branchcode });

                return rows > 0;
            }catch(Exception ex){
                throw ex;
            }
        }

        #endregion

        #region Inventory Category Master
        public async Task<IEnumerable<InventoryCategoryMaster>> GetCategoryList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select CatCode, CatName, UserCode, LastModify, Branch_Code, ImageUrl" +
                " FROM InventoryItemCategory WHERE Branch_Code = @BranchCode " +
                " Order by CatCode";

            return await connection.QueryAsync<InventoryCategoryMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertCategoryDetails(InventoryCategoryMaster category)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
                            INSERT INTO InventoryItemCategory
                            (CatCode, CatName, UserCode, LastModify, Branch_Code, ImageUrl)
                            VALUES
                            (@CatCode, @CatName, @UserCode, @LastModify, @Branch_Code, @ImageUrl);

                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, category);
            return id;
        }

        public async Task<bool> UpdateCategoryDetails(InventoryCategoryMaster category)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
                                UPDATE InventoryItemCategory SET
                                CatCode = COALESCE(@CatCode, CatCode),
                                CatName = COALESCE(@CatName, CatName),
                                UserCode = COALESCE(@UserCode, UserCode),
                                LastModify = COALESCE(@LastModify, LastModify),
                                ImageUrl = COALESCE(@ImageUrl, ImageUrl)
                                WHERE CatCode = @CatCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, category);
            return rows > 0;
        }

        public async Task<bool> DeleteCategoryDetails(int CategoryCode, string branchcode)
        {
            const string query = "DELETE FROM InventoryItemCategory WHERE CatCode = @CatCode And Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { CatCode = CategoryCode, BranchCode = branchcode });
            return rows > 0;
        }
        #endregion

        #region Sub Category Master
        public async Task<IEnumerable<InventorySubCategoryMaster>> GetSubCategoryList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select CatCode, CatName, SubCatCode, SubCatName, UserCode, Branch_Code, TrDate " +
                " FROM InventoryItemSubCategory WHERE Branch_Code = @BranchCode " +
                " Order by CatCode";

            return await connection.QueryAsync<InventorySubCategoryMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertSubCategoryDetails(InventorySubCategoryMaster subCategory)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
                INSERT INTO InventoryItemSubCategory
                (CatCode, CatName, SubCatCode, SubCatName, UserCode, Branch_Code, TrDate)
                VALUES
                (@CatCode, @CatName, @SubCatCode, @SubCatName, @UserCode, @Branch_Code, @TrDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var id = await connection.ExecuteScalarAsync<int>(query, subCategory);
            return id;
        }

        public async Task<bool> UpdateSubCategoryDetails(InventorySubCategoryMaster subCategory)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                    UPDATE InventoryItemSubCategory SET
                    CatCode = COALESCE(@CatCode, CatCode),
                    CatName = COALESCE(@CatName, CatName),
                    SubCatName = COALESCE(@SubCatName, SubCatName),
                    UserCode = COALESCE(@UserCode, UserCode),
                    TrDate = COALESCE(@TrDate, TrDate)
                    WHERE SubCatCode = @SubCatCode And Branch_Code = @Branch_Code ";
                var rows = await connection.ExecuteAsync(query, subCategory);
                return rows > 0;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteSubCategoryDetails(int SubCategoryCode, string branchcode)
        {
            const string query = "DELETE FROM InventoryItemSubCategory WHERE SubCatCode = @SubCatCode And " +
                "Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { SubCatCode = SubCategoryCode, BranchCode = branchcode });
            return rows > 0;
        }
        #endregion

        #region Inventory Store Master
        public async Task<IEnumerable<InventoryStoreMaster>> GetInventoryStoreMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select Storeid as StoreId, StoreName as StoreName, StoreLocation as StoreLocation, " +
                " StoreIncharge as StoreIncharge, Branch_Code as Branch_Code " +
                " FROM STOREMASTER WHERE Branch_Code = @BranchCode " +
                " Order by StoreId";

            return await connection.QueryAsync<InventoryStoreMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> CreateInventoryStoreMaster(InventoryStoreMaster inventorystore)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                INSERT INTO STOREMASTER
                (StoreName, StoreLocation, StoreIncharge, Branch_Code)
                VALUES
                (@StoreName, @StoreLocation, @StoreIncharge,@Branch_Code);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var id = await connection.ExecuteScalarAsync<int>(query, inventorystore);
                return id;
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateInventoryStoreMaster(InventoryStoreMaster inventorystore)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                    UPDATE STOREMASTER SET
                    StoreName = COALESCE(@StoreName, StoreName),
                    StoreLocation = COALESCE(@StoreLocation, StoreLocation),
                    StoreIncharge = COALESCE(@StoreIncharge, StoreIncharge)
                    WHERE Storeid = @StoreId And Branch_Code = @Branch_Code ";
                var rows = await connection.ExecuteAsync(query, inventorystore);
                return rows > 0;
            }catch(Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> DeleteInventoryStoreMaster(int StoreId, string branchcode)
        {
            const string query = "DELETE FROM STOREMASTER WHERE Storeid = @StoreId And " +
                "Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { StoreId = StoreId, BranchCode = branchcode });
            return rows > 0;
        }
        #endregion

        #region Item Master
        public async Task<IEnumerable<InventoryMasterItemModel>> GetItemsList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string selectQuery = @"SELECT
            im.ItemCode,im.ItemName,im.CatCode,im.SubCatCode,im.Storeid,im.GrpCode,im.UnitCode,im.UnitName,im.PurchaseRate,
            im.NoofUnits,im.ItemRate,im.ItemOpStock,im.ItemOpRate,im.ItemROQ,im.ItemROL,im.BarCode,im.Picture,im.UserCode,
            im.LastModify,im.mostrunningitemsrno AS MostRunningItemSrNo,im.Branch_Code,
            ec.ChargeCode AS TaxCode,ec.ChargeName AS TaxName,
            iud.UUNIT AS FirstUnit,iud.UDESC AS FirstUnitDesc,iud.LUNIT AS FinalUnit,iud.LDESC AS FinalUnitDesc
            FROM dbo.InventoryItemMaster im
            OUTER APPLY (SELECT TOP 1 ChargeCode,ChargeName FROM InventoryExtraCharges ec 
            WHERE ec.ItemCode = im.ItemCode AND ec.Branch_Code = im.Branch_Code) ec
            LEFT JOIN dbo.ITEMUNITSDESC iud ON iud.ITCODE = im.ItemCode AND iud.BRANCH_CODE = im.Branch_Code
            WHERE im.Branch_Code = @Branchcode ORDER BY im.ItemCode;";
            return await connection.QueryAsync<InventoryMasterItemModel>(selectQuery,
                new
                {
                    Branchcode = branchcode
                }
            );
        }
        public async Task<int> InsertItemDetails(InventoryMasterItemModel model)
        {
            const string insertItemMasterSql = @"
                            INSERT INTO InventoryItemMaster
                            (
                                ItemCode,
                                ItemName,
                                CatCode,
                                SubCatCode,
                                Storeid,
                                GrpCode,
                                UnitCode,
                                UnitName,
                                PurchaseRate,
                                NoofUnits,
                                ItemRate,
                                ItemOpStock,
                                ItemOpRate,
                                ItemROQ,
                                ItemROL,
                                BarCode,
                                TaxCode,
                                TaxName,
                                Picture,
                                UserCode,
                                LastModify,
                                mostrunningitemsrno,
                                Branch_Code
                            )
                            VALUES
                            (
                                @ItemCode,
                                @ItemName,
                                @CatCode,
                                @SubCatCode,
                                @Storeid,
                                @GrpCode,
                                @UnitCode,
                                @UnitName,
                                @PurchaseRate,
                                @NoofUnits,
                                @ItemRate,
                                @ItemOpStock,
                                @ItemOpRate,
                                @ItemROQ,
                                @ItemROL,
                                @BarCode,
                                @TaxCode,
                                @TaxName,
                                @Picture,
                                @UserCode,
                                @LastModify,
                                @MostRunningItemSrNo,
                                @Branch_Code
                            );";

            const string insertItemUnitDescSql = @"
                            INSERT INTO ITEMUNITSDESC
                            (
                                ITCODE,
                                UUNIT,
                                UDESC,
                                LUNIT,
                                LDESC,
                                BRANCH_CODE
                            )
                            VALUES
                            (
                                @ITCODE,
                                @UUNIT,
                                @UDESC,
                                @LUNIT,
                                @LDESC,
                                @BRANCH_CODE
                            );";

            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ItemCode", model.ItemCode);
                parameters.Add("@ItemName", model.ItemName?.ToUpper());
                parameters.Add("@CatCode", model.CatCode);
                parameters.Add("@SubCatCode", model.SubCatCode);
                parameters.Add("@Storeid", model.Storeid);
                parameters.Add("@GrpCode", model.GrpCode);
                parameters.Add("@UnitCode", model.UnitCode);
                parameters.Add("@UnitName", model.UnitName);
                parameters.Add("@PurchaseRate", model.PurchaseRate);
                parameters.Add("@NoofUnits", model.NoofUnits);
                parameters.Add("@ItemRate", model.ItemRate);
                parameters.Add("@ItemOpStock", model.ItemOpStock);
                parameters.Add("@ItemOpRate", model.ItemOpRate);
                parameters.Add("@ItemROQ", model.ItemROQ);
                parameters.Add("@ItemROL", model.ItemROL);
                parameters.Add("@BarCode", model.BarCode);
                parameters.Add("@TaxCode", model.TaxCode);
                parameters.Add("@TaxName", model.TaxName);
                parameters.Add("@Picture", model.Picture);
                parameters.Add("@UserCode", model.UserCode);
                parameters.Add("@LastModify", model.LastModify);
                parameters.Add("@MostRunningItemSrNo", model.MostRunningItemSrNo);
                parameters.Add("@Branch_Code", model.Branch_Code);

                var itemRowsAffected = await connection.ExecuteAsync(
                    insertItemMasterSql,parameters,transaction
                );

                if (itemRowsAffected <= 0){
                    throw new Exception( "Failed to insert item into InventoryItemMaster.");
                }

                var unitParameters = new DynamicParameters();
                unitParameters.Add("@ITCODE", model.ItemCode);
                unitParameters.Add("@UUNIT", model.FirstUnit);
                unitParameters.Add("@UDESC", model.FirstUnitDesc);
                unitParameters.Add("@LUNIT", model.FinalUnit);
                unitParameters.Add("@LDESC", model.FinalUnitDesc);
                unitParameters.Add("@BRANCH_CODE", model.Branch_Code);

                var unitRowsAffected = await connection.ExecuteAsync( 
                    insertItemUnitDescSql,unitParameters,transaction
                    );

                if (unitRowsAffected <= 0){
                    throw new Exception("Failed to insert item into ITEMUNITSDESC.");
                }
                transaction.Commit();
                return itemRowsAffected;
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
        }

        public async Task<bool> UpdateItemDetails(InventoryMasterItemModel item)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateItemMaster = @"UPDATE InventoryItemMaster
                SET
                ItemName = COALESCE(@ItemName, ItemName),
                CatCode = COALESCE(@CatCode, CatCode),
                SubCatCode = COALESCE(@SubCatCode, SubCatCode),
                Storeid = COALESCE(@Storeid, Storeid),
                GrpCode = COALESCE(@GrpCode, GrpCode),
                UnitCode = COALESCE(@UnitCode, UnitCode),
                UnitName = COALESCE(@UnitName, UnitName),
                PurchaseRate = COALESCE(@PurchaseRate, PurchaseRate),
                NoofUnits = COALESCE(@NoofUnits, NoofUnits),
                ItemRate = COALESCE(@ItemRate, ItemRate),
                ItemOpStock = COALESCE(@ItemOpStock, ItemOpStock),
                ItemOpRate = COALESCE(@ItemOpRate, ItemOpRate),
                ItemROQ = COALESCE(@ItemROQ, ItemROQ),
                ItemROL = COALESCE(@ItemROL, ItemROL),
                BarCode = COALESCE(@BarCode, BarCode),
                TaxCode = COALESCE(@TaxCode, TaxCode),
                TaxName = COALESCE(@TaxName, TaxName),
                Picture = COALESCE(@Picture, Picture),
                UserCode = COALESCE(@UserCode, UserCode),
                LastModify = COALESCE(@LastModify, LastModify),
                mostrunningitemsrno = COALESCE(@MostRunningItemSrNo,mostrunningitemsrno)
                WHERE ItemCode = @ItemCode AND Branch_Code = @Branch_Code;";

                var parameters = new DynamicParameters();
                parameters.Add("@ItemCode", item.ItemCode);
                parameters.Add("@ItemName", item.ItemName?.ToUpper());
                parameters.Add("@CatCode", item.CatCode);
                parameters.Add("@SubCatCode", item.SubCatCode);
                parameters.Add("@Storeid", item.Storeid);
                parameters.Add("@GrpCode", item.GrpCode);
                parameters.Add("@UnitCode", item.UnitCode);
                parameters.Add("@UnitName", item.UnitName);
                parameters.Add("@PurchaseRate", item.PurchaseRate);
                parameters.Add("@NoofUnits", item.NoofUnits);
                parameters.Add("@ItemRate", item.ItemRate);
                parameters.Add("@ItemOpStock", item.ItemOpStock);
                parameters.Add("@ItemOpRate", item.ItemOpRate);
                parameters.Add("@ItemROQ", item.ItemROQ);
                parameters.Add("@ItemROL", item.ItemROL);
                parameters.Add("@BarCode", item.BarCode);
                parameters.Add("@TaxCode", item.TaxCode);
                parameters.Add("@TaxName", item.TaxName);
                parameters.Add("@Picture", item.Picture);
                parameters.Add("@UserCode", item.UserCode);
                parameters.Add("@LastModify", item.LastModify);
                parameters.Add("@MostRunningItemSrNo", item.MostRunningItemSrNo);
                parameters.Add("@Branch_Code", item.Branch_Code);

                int itemRowsAffected = await connection.ExecuteAsync(
                    updateItemMaster,
                    parameters,
                    transaction
                );

                if (itemRowsAffected <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string updateItemUnitDesc = @"UPDATE ITEMUNITSDESC
                SET UUNIT = @UUNIT, UDESC = @UDESC,LUNIT = @LUNIT,LDESC = @LDESC
                WHERE ITCODE = @ITCODE AND BRANCH_CODE = @Branch_Code;";

                var unitParameters = new DynamicParameters();
                unitParameters.Add("@ITCODE", item.ItemCode);
                unitParameters.Add("@UUNIT", item.FirstUnit);
                unitParameters.Add("@UDESC", item.FirstUnitDesc);
                unitParameters.Add("@LUNIT", item.FinalUnit);
                unitParameters.Add("@LDESC", item.FinalUnitDesc);
                unitParameters.Add("@Branch_Code", item.Branch_Code);

                int unitRowsAffected = await connection.ExecuteAsync(updateItemUnitDesc,unitParameters,transaction);
                if (unitRowsAffected == 0)
                {
                    const string insertItemUnitDesc = @"
                INSERT INTO ITEMUNITSDESC
                (
                    ITCODE,
                    UUNIT,
                    UDESC,
                    LUNIT,
                    LDESC,
                    BRANCH_CODE
                )
                VALUES
                (
                    @ITCODE,
                    @UUNIT,
                    @UDESC,
                    @LUNIT,
                    @LDESC,
                    @Branch_Code
                );";

                    await connection.ExecuteAsync(insertItemUnitDesc, unitParameters,transaction);
                }
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteItemDetails(int itemcode,string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var parameters = new
                {
                    ItemCode = itemcode,
                    BranchCode = branchcode
                };
                const string deleteItemUnitDesc = @"DELETE FROM ITEMUNITSDESC WHERE ITCODE = @ItemCode
                 AND BRANCH_CODE = @BranchCode;";
                await connection.ExecuteAsync( deleteItemUnitDesc, parameters,transaction);

                const string deleteExtraCharges = @"DELETE FROM InventoryExtraCharges WHERE ItemCode = @ItemCode 
                AND Branch_Code = @BranchCode;";
                await connection.ExecuteAsync( deleteExtraCharges, parameters,transaction);

                const string deleteItemMaster = @"DELETE FROM InventoryItemMaster WHERE ItemCode = @ItemCode 
                AND Branch_Code = @BranchCode;";
                int rowsAffected = await connection.ExecuteAsync(deleteItemMaster, parameters,transaction);

                if (rowsAffected <= 0)
                {
                    transaction.Rollback();
                    return false;
                }
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode)
        {
            string str;
            int exist = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                str = $@"select {columnname} from {tablename} where {columnname} = @Parameter and branch_code = @Branch ";

                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str, 
                    new { Parameter = parameter, Branch = BranchCode });

                if (ds != null && ds.Count > 0)
                {
                    return exist = 1;
                }
                else
                {
                    return exist = 0;
                }
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> ItemNameExist(int id, string itemName, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT 1 FROM InventoryItemMaster 
            WHERE ItemCode = @ItemCode AND ItemName = @ItemName AND Branch_Code = @BranchCode";

            var result = await connection.QueryFirstOrDefaultAsync<int?>(query,
                new
                {
                    ItemCode = id,
                    ItemName = itemName,
                    BranchCode = branchcode
                });

            return result.HasValue;
        }
        public async Task<int> DeleteExtraChargesAsync(int itemCode, string branchCode, string Storeid, 
            string outletname, IDbConnection con)
        {
            string query = string.Empty;
            query = @" DELETE FROM InventoryExtraCharges WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";
            int result = await con.ExecuteAsync(query, new { ItemCode = itemCode, BranchCode = branchCode });
            return result;
        }

        public async Task<int> InsertExtraChargesAsync(int chargeCode,string chargeName,int itemCode,string branchCode,
        DateTime currentTime,string Storeid,string outletName,IDbConnection con)
        {
            string StoreQuery = @"SELECT Storeid FROM STOREMASTER WHERE Branch_Code = @BranchCode";

            var storecodes = (await con.QueryAsync<int>(StoreQuery,new { BranchCode = branchCode })).ToList();

            if (!string.IsNullOrWhiteSpace(Storeid) && !string.IsNullOrWhiteSpace(outletName) &&
                !outletName.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                var storeIds = Storeid.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).Where(x => int.TryParse(x, out _))
                    .Select(int.Parse).ToList();

                storecodes = storecodes.Where(x => storeIds.Contains(x)).ToList();
            }

            string insertQuery = @"
                INSERT INTO InventoryExtraCharges
                (
                    ChargeCode,
                    ChargeName,
                    ItemCode,
                    Branch_Code,
                    Storeid,
                    lastmodify
                )
                VALUES
                (
                    @ChargeCode,
                    @ChargeName,
                    @ItemCode,
                    @BranchCode,
                    @Storeid,
                    @lastmodify
                )";

            foreach (var storeId in storecodes)
            {
                await con.ExecuteAsync(insertQuery, new
                {
                    ChargeCode = chargeCode,
                    ChargeName = chargeName,
                    ItemCode = itemCode,
                    BranchCode = branchCode,
                    Storeid = storeId,
                    lastmodify = currentTime
                });
            }

            return 1;
        }
        #endregion

        #region Inventory Misc Master
        public async Task<IEnumerable<MiscellaneousInventory>> GetInventoryMiscList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "Select ChargeId,ChargeName,Branch_Code,TaxCode " +
                " FROM MiscCharges WHERE Branch_Code = @BranchCode " +
                " Order by ChargeId";

            return await connection.QueryAsync<MiscellaneousInventory>(query, new { BranchCode = branchcode });
        }

        public async Task<int> CreateInventoryMisc(MiscellaneousInventory misc)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                INSERT INTO MiscCharges(ChargeName, Branch_Code,TaxCode)
                VALUES (@ChargeName,@Branch_Code,@TaxCode);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                var id = await connection.ExecuteScalarAsync<int>(query, misc);
                return id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateInventoryMisc(MiscellaneousInventory misc)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                    UPDATE MiscCharges SET
                    ChargeName = COALESCE(@ChargeName, ChargeName),
                    TaxCode = COALESCE(@TaxCode, TaxCode)
                    WHERE chargeId = @chargeId And Branch_Code = @Branch_Code ";
                var rows = await connection.ExecuteAsync(query, misc);
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> DeleteInventoryMisc(int chargeId, string branchcode)
        {
            const string query = "DELETE FROM MiscCharges WHERE ChargeId = @ChargeId And " +
                "Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { ChargeId = chargeId, BranchCode = branchcode });
            return rows > 0;
        }
        #endregion

        #region Inventory GRN Miscellaneous
        public async Task<IEnumerable<GRNMiscellaneousInventory>> GetInventoryGRNMiscList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select GRNId,Pno,ChargeAmt,ChargeId,Branch_Code " +
                " FROM Tbl_GRNMISC WHERE Branch_Code = @BranchCode " +
                " Order by GRNId";

            return await connection.QueryAsync<GRNMiscellaneousInventory>(query, new { BranchCode = branchcode });
        }

        public async Task<int> CreateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc)
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
               INSERT INTO Tbl_GRNMISC(GRNId,ChargeId,ChargeAmt,Branch_Code,Pno)
               VALUES(@GRNId,@ChargeId,@ChargeAmt,@Branch_Code,@Pno);
               SELECT @GRNId;";

                var parameters = new
                {
                    grnMisc.GRNId,
                    grnMisc.ChargeId,
                    grnMisc.ChargeAmt,
                    grnMisc.Branch_Code,
                    grnMisc.Pno,
                };

                var pno = await connection.ExecuteScalarAsync<int>(query, parameters);
                return pno;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateInventoryGRNMisc(GRNMiscellaneousInventory grnMisc    )
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                const string query = @"
                    UPDATE Tbl_GRNMISC SET
                    ChargeId = COALESCE(@ChargeId, ChargeId),
                    ChargeAmt = COALESCE(@ChargeAmt, ChargeAmt),
                    Pno = COALESCE(@Pno, Pno)
                    WHERE GRNId = @GRNId And Branch_Code = @Branch_Code ";
                var rows = await connection.ExecuteAsync(query, grnMisc    );
                return rows > 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteInventoryGRNMisc(int GRNId, string branchcode)
        {
            const string query = "DELETE FROM Tbl_GRNMISC WHERE GRNId = @GRNId And " +
                "Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { GRNId = GRNId, BranchCode = branchcode });
            return rows > 0;
        }
        #endregion

        #region ImportItemMaster
        public async Task<bool> ItemExists(int itemCode, string branch)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM InventoryItemMaster WHERE ItemCode = @ItemCode AND Branch_Code = @Branch";

            var count = await con.ExecuteScalarAsync<int>(query, new { itemCode, Branch = branch });
            return count > 0;
        }

        public async Task<bool> ItemNameExists(string itemname, string branch)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM InventoryItemMaster WHERE ItemName = @ItemName AND Branch_Code = @Branch";

            var count = await con.ExecuteScalarAsync<int>(query, new { itemname, Branch = branch });
            return count > 0;
        }

        public async Task<string> GetOrCreate(string table, string codeCol, string nameCol, string value, string branchcode, string usercode, DateTime currentime, InventoryImportItemRow item)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = string.Empty;

            if (table == "BillTaxMaster")
            {
                query = $"SELECT {codeCol} FROM {table} WHERE {nameCol} = @Name And BranchCode = @BranchCode";
            }
            else
            {
                query = $"SELECT {codeCol} FROM {table} WHERE {nameCol} = @Name And Branch_Code = @BranchCode";
            }

            var code = await con.ExecuteScalarAsync<string>(query, new { Name = value, BranchCode = branchcode });

            if (!string.IsNullOrEmpty(code))
            {
                return code;
            }
            else
            {
                if (table == "InventoryItemCategory")
                {
                    int nxtnumber = await Findnextnumber("InventoryItemCategory", "CatCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO InventoryItemCategory(CatCode, CatName, UserCode, LastModify, Branch_Code)" +
                        $" OUTPUT INSERTED.CatCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @LastModify, @BranchCode)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, 
                        new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode });
                }
                else if (table == "ItemGroup")
                {
                    int nxtnumber = await Findnextnumber("ItemGroup", "GrpCode", "Branch_Code", branchcode);

                    var dep = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);

                    string insertQuery = $"INSERT INTO ItemGroup(GrpCode, GrpName, UserCode, Branch_code, LastModify, Isuploaded, Dep)" +
                        $" OUTPUT INSERTED.GrpCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @BranchCode, @LastModify, 0, @Dep)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode, Dep = dep.ToUpper() });
                }
                else if (table == "UnitMaster")
                {
                    int nxtnumber = await Findnextnumber("UnitMaster", "UnitCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO UnitMaster(UnitCode, UnitName, UnitSymbol, Branch_Code) " +
                        $" OUTPUT INSERTED.UnitCode " +
                        $" VALUES (@Nxtnumber, @Name, @Name, @BranchCode)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, BranchCode = branchcode });
                }
                else if (table == "BillTaxMaster")
                {
                    int nxtnumber = await Findnextnumber("BillTaxMaster", "TaxCode", "BranchCode", branchcode);

                    string text = value;

                    string number = Regex.Match(text, @"\d+(\.\d+)?").Value;

                    decimal gst = decimal.Parse(number);

                    string insertQuery = $"INSERT INTO BillTaxMaster(TaxCode, TaxName, TaxPercentage, UserCode, BranchCode, IsActive, FromDate, Todate) " +
                        $" OUTPUT INSERTED.TaxCode " +
                        $" VALUES (@Nxtnumber, @Name, @gstper, @usercode, @BranchCode, @IsActive, @FromDate, @Todate)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value.ToUpper(), gstper = gst, usercode = usercode, BranchCode = branchcode, IsActive = 1, FromDate = currentime, Todate = currentime });
                }
                else
                {
                    return (0).ToString();
                }

            }
        }

        public async Task<string> GetOrCreateSubCategory(string category, string subCategory, string branchcode, DateTime currentTime, string userCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query1 = @"SELECT SubCatCode FROM InventoryItemSubCategory 
              WHERE SubCatName=@Sub AND CatName=@Cat AND Branch_Code = @BranchCode";

            var code = await con.ExecuteScalarAsync<string>(query1, new { Sub = subCategory, Cat = category, BranchCode = branchcode });
            if (!string.IsNullOrEmpty(code))
                return code;
            else
            {

                int nxtnumber = await Findnextnumber("InventoryItemSubCategory", "SubCatCode", "Branch_Code", branchcode);

                string Ccode = await GetOrCreate("InventoryItemCategory", "CatCode", "CatName", category, branchcode, userCode, DateTime.Now, null);

                string insertQuery = $@"INSERT INTO InventoryItemSubCategory (CatCode, CatName, SubCatCode, SubCatName, UserCode, Trdate, Branch_Code) 
                  OUTPUT INSERTED.SubCatCode
                  VALUES (@catcode, @Cat, @Nxtnumber, @Sub, @UserCode, @Trdate, @BranchCode)";

                return await con.ExecuteScalarAsync<string>(insertQuery,
                    new { catcode = Ccode, Cat = category, Nxtnumber = nxtnumber, Sub = subCategory.ToUpper(), UserCode = userCode, Trdate = currentTime, BranchCode = branchcode });
            }
        }

        public async Task InsertItemMaster(InventoryImportItemRow item, List<int> Storeids, string Catcode, string SubCatcode, string Groupcode,  string Unitcode, string Taxcode, string Taxname, string Branchcode, string usercode, DateTime currentdate)
        {
            string storeIdsString = string.Join(", ", Storeids);
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = $@"Insert Into InventoryItemMaster(ItemCode,ItemName,CatCode,SubCatCode,Storeid,GrpCode,UnitCode,UnitName,
                        PurchaseRate,NoofUnits,ItemRate,ItemOpStock,ItemOpRate,ItemROQ,ItemROL,BarCode,TaxCode,TaxName,Picture,
                        UserCode,LastModify,mostrunningitemsrno,Branch_Code)
                        VALUES(@ItemCode,@ItemName,@CatCode,@SubCatCode,@Storeid,@GrpCode,@UnitCode,@UnitName,@PurchaseRate,@NoofUnits,
                          @ItemRate,@ItemOpStock,@ItemOpRate,@ItemROQ,@ItemROL,@BarCode,@TaxCode,@TaxName,@Picture,@UserCode,
                          @LastModify,@MostRunningItemSrNo,@Branch_Code)";
            await con.ExecuteAsync(sql, new
            {
                ItemCode = item.ItemCode,
                ItemName = item.ItemName.ToUpper(),
                CatCode = Catcode,
                SubCatCode = SubCatcode,
                Storeid = storeIdsString,
                GrpCode = Groupcode,
                UnitCode = Unitcode,
                UnitName = item.Unit,
                PurchaseRate = item.PurchaseRate,
                NoofUnits = item.NoofUnits,
                ItemRate = item.ItemRate,
                ItemOpStock = 0,
                ItemOpRate = 0,
                ItemROQ = 0,
                ItemROL = 0,
                BarCode = "",
                TaxCode = Taxcode,
                TaxName = Taxname,
                Picture = "",
                UserCode = usercode,
                LastModify = currentdate,
                branch_code = Branchcode,
                MostRunningItemSrNo = 0
            });

            const string insertItemUnitDesc = @"
            INSERT INTO ITEMUNITSDESC (ITCODE,UUNIT,UDESC,LUNIT,LDESC,BRANCH_CODE)
            VALUES(@ITCODE,@UUNIT,@UDESC,@LUNIT,@LDESC,@BRANCH_CODE);";

            var unitParameters = new DynamicParameters();
            unitParameters.Add("ITCODE", item.ItemCode);
            unitParameters.Add("UUNIT", Unitcode);
            unitParameters.Add("UDESC", item.Unit);
            unitParameters.Add("LUNIT", Unitcode);
            unitParameters.Add("LDESC", item.Unit);
            unitParameters.Add("BRANCH_CODE", Branchcode);
            await con.ExecuteAsync(insertItemUnitDesc, unitParameters);

            if (Storeids != null && Storeids.Any())
            {
                foreach (var store in Storeids)
                {
                    string insertQuery = @"
                         INSERT INTO InventoryExtraCharges ( ChargeCode, ChargeName, ItemCode, Branch_Code, Storeid, lastmodify )
                         VALUES ( @ChargeCode, @ChargeName, @ItemCode, @BranchCode, @Storeid, @lastmodify )";

                    await con.ExecuteAsync(insertQuery, new
                    {
                        ChargeCode = Taxcode,
                        ChargeName = Taxname,
                        ItemCode = item.ItemCode,
                        BranchCode = Branchcode,
                        Storeid = store,
                        lastmodify = currentdate
                    });
                }
            }
        }

        public async Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            string str;
            int trno = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                if (table_name == "InventoryItemMaster")
                {
                    str = $@"select (MAX({column_name}), 0) as trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where {condition_name} = @Branch";
                }
                else
                {
                    str = $@"select ISNULL(MAX({column_name}), 0) AS trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where {condition_name} = @Branch ";
                }

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
        #endregion

        #region Unit Conversion Master
        public async Task<IEnumerable<UnitConversionMaster>> GetUnitConversionList(string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"SELECT UnitCode,UnitName,Qty,IsActive,Branch_Code,CreatedBy,
            CreatedDate FROM InventoryUnitMaster WHERE Branch_Code = @Branch ORDER BY UnitCode DESC";

            return await connection.QueryAsync<UnitConversionMaster>(query,new { Branch = branch });
        }

        public async Task<int> CreateUnitConversion(UnitConversionMaster model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            const string query = @"
                INSERT INTO InventoryUnitMaster
                (
                    UnitCode,
                    UnitName,
                    Qty,
                    IsActive,
                    Branch_Code,
                    CreatedBy,
                    CreatedDate
                )
                VALUES
                (
                    @UnitCode,
                    @UnitName,
                    @Qty,
                    @IsActive,
                    @Branch_Code,
                    @CreatedBy,
                    GETDATE()
                );
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await connection.ExecuteScalarAsync<int>(query, model);
        }

        public async Task<bool> UpdateUnitConversion(UnitConversionMaster model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
                UPDATE InventoryUnitMaster
                SET
                    UnitName = @UnitName,
                    Qty = @Qty,
                    IsActive = @IsActive
                WHERE UnitCode = @UnitCode AND Branch_Code = @Branch_Code";

            var rows = await connection.ExecuteAsync(query, model);
            return rows > 0;
        }
        public async Task<bool> DeleteUnitConversion(int UnitCode, string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"DELETE FROM InventoryUnitMaster
                        WHERE UnitCode = @UnitCode AND Branch_Code = @Branch";
            var rows = await connection.ExecuteAsync(query,
                new{UnitCode = UnitCode, Branch = branch});
            return rows > 0;
        }
        #endregion

        #region Terms And Conditions Master
        public async Task<IEnumerable<TermsAndConditionsMaster>> GetTermsAndConditionsList(string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
             SELECT TermsCode,TermsTitle,TermsDescription,MasterName,Branch_Code FROM TermsAndConditionsMaster
             WHERE Branch_Code = @Branch ORDER BY TermsCode DESC";

            return await connection.QueryAsync<TermsAndConditionsMaster>(query,new { Branch = branch });
        }


        public async Task<int> CreateTermsAndConditions(TermsAndConditionsMaster model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"INSERT INTO TermsAndConditionsMaster
                (TermsTitle,TermsDescription,MasterName,Branch_Code)
                VALUES(@TermsTitle,@TermsDescription,@MasterName,@Branch_Code);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(query,model);
        }


        public async Task<bool> UpdateTermsAndConditions(TermsAndConditionsMaster model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @" UPDATE TermsAndConditionsMaster
            SET TermsTitle = @TermsTitle,
            TermsDescription = @TermsDescription,
            MasterName = @MasterName
            WHERE TermsCode = @TermsCode AND Branch_Code = @Branch_Code";

            var rows = await connection.ExecuteAsync(query, model);
            return rows > 0;
        }


        public async Task<bool> DeleteTermsAndConditions(int TermsCode,string branch)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"DELETE FROM TermsAndConditionsMaster
            WHERE TermsCode = @TermsCode AND Branch_Code = @Branch";

            var rows = await connection.ExecuteAsync(query,
                new{TermsCode = TermsCode,Branch = branch});
            return rows > 0;
        }
        #endregion
    }
}
