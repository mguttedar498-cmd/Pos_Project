using Dapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using HMS_360_PMS.EntitiesModels.POS;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using Microsoft.Data.SqlClient;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table.PivotTable;
using System.Data;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.Arm;
using System.Text.RegularExpressions;
using System.Transactions;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HMS_360_PMS.ProjectInfrastructure.Master
{
    public class Master_DAL : IMaster_Repository
    {
        private readonly DbConnectionFactory _factory;

        public Master_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        #region CompanyMaster

        public async Task<IEnumerable<CompaniesMaster>> GetCompaniesList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "SELECT CompanyCode, CompanyName, ContactPerson, Address, City, Pincode, Phone, Email, GSTNo, UserCode, LastModify, Branch_code " +
                " FROM CompanyMaster WHERE Branch_code = @Branch_code " +
                " Order by CompanyCode";

            return await connection.QueryAsync<CompaniesMaster>(query, new { Branch_code = branchcode });
        }

        public async Task<int> InsertCompanyDetails(CompaniesMaster company)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO CompanyMaster
            (CompanyCode, CompanyName, ContactPerson, Address, City, Pincode, Phone, Email, GSTNo, UserCode, LastModify, Branch_code)
            VALUES
            (@CompanyCode, @CompanyName, @ContactPerson, @Address, @City, @Pincode, @Phone, @Email, @GSTNo, @UserCode, @LastModify, @Branch_code);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, company);
            return id;
        }

        public async Task<bool> UpdateCompanyDetails(CompaniesMaster company)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE CompanyMaster SET
            CompanyName = COALESCE(@CompanyName, CompanyName),
            ContactPerson = COALESCE(@ContactPerson, ContactPerson),
            Address = COALESCE(@Address, Address),
            City = COALESCE(@City, City),
            Pincode = COALESCE(@Pincode, Pincode),
            Phone = COALESCE(@Phone, Phone),
            Email = COALESCE(@Email, Email),
            GSTNo = COALESCE(@GSTNo, GSTNo),
            UserCode = @UserCode,
            LastModify = @LastModify
            WHERE CompanyCode = @CompanyCode And Branch_code = @Branch_code";

            var rows = await connection.ExecuteAsync(query, company);
            return rows > 0;
        }

        public async Task<bool> DeleteCompanyDetails(int companyCode, string branchcode)
        {
            const string query = "DELETE FROM CompanyMaster WHERE CompanyCode = @CompanyCode And Branch_code = @Branch_code";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { CompanyCode = companyCode, Branch_code = branchcode });
            return rows > 0;
        }
        #endregion

        #region TaxMaster

        public async Task<IEnumerable<BillTaxMaster>> GetTaxMasterList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "SELECT TaxCode, TaxName, TaxPercentage, IsActive, FromDate, Todate, UserCode, BranchCode " +
                " FROM BillTaxMaster WHERE BranchCode = @Branch_code " +
                " Order by TaxCode";

            return await connection.QueryAsync<BillTaxMaster>(query, new { Branch_code = branchcode });
        }

        public async Task<int> InsertTaxMasterDetails(BillTaxMaster taxmaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO BillTaxMaster
            (TaxCode, TaxName, TaxPercentage, IsActive, FromDate, Todate, UserCode, BranchCode)
            VALUES
            (@TaxCode ,@TaxName, @TaxPercentage, @IsActive, @FromDate, @Todate, @UserCode, @BranchCode);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, taxmaster);
            return id;
        }

        public async Task<bool> UpdateTaxMasterDetails(BillTaxMaster taxmaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE BillTaxMaster SET
            TaxName = COALESCE(@TaxName, TaxName),
            TaxPercentage = COALESCE(@TaxPercentage, TaxPercentage),
            IsActive = COALESCE(@IsActive, IsActive),
            FromDate = COALESCE(@FromDate, FromDate),
            Todate = COALESCE(@Todate, Todate),
            UserCode = COALESCE(@UserCode, UserCode)
            WHERE TaxCode = @TaxCode And BranchCode = @BranchCode";

            var rows = await connection.ExecuteAsync(query, taxmaster);
            return rows > 0;  
        }

        public async Task<bool> DeleteTaxMasterDetails(int taxcode, string branchcode)
        {
            const string query = "DELETE FROM BillTaxMaster WHERE TaxCode = @TaxCode And BranchCode = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { TaxCode = taxcode, BranchCode = branchcode });
            return rows > 0;
        }

        public async Task<IEnumerable<BillTaxDescription>> GetTaxDescriptionList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "SELECT TaxDescId, TaxCode, TaxDescription, TaxPercentage, IsActive, UserCode, BranchCode " +
                " FROM BillTaxDescription WHERE BranchCode = @BranchCode " +
                " Order by TaxDescId";

            return await connection.QueryAsync<BillTaxDescription>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertTaxDescriptionDetails(BillTaxDescription taxdescription)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO BillTaxDescription  
            (TaxCode, TaxDescription, TaxPercentage, IsActive, UserCode, BranchCode)
            VALUES
            (@TaxCode, @TaxDescription, @TaxPercentage, @IsActive, @UserCode, @BranchCode);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, taxdescription);
            return id;
        }

        public async Task<bool> UpdateTaxDescriptionDetails(BillTaxDescription taxdescription)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE BillTaxDescription SET
            TaxCode = COALESCE(@TaxCode, TaxCode),
            TaxDescription = COALESCE(@TaxDescription, TaxDescription),
            TaxPercentage = COALESCE(@TaxPercentage, TaxPercentage),
            IsActive = COALESCE(@IsActive, IsActive),
            UserCode = COALESCE(@UserCode, UserCode)
            WHERE TaxDescId = @TaxDescId And BranchCode = @BranchCode";

            var rows = await connection.ExecuteAsync(query, taxdescription);
            return rows > 0;
        }

        public async Task<bool> DeleteTaxDescriptionDetails(int taxDescriptionCode, string branchcode)
        {
            const string query = "DELETE FROM BillTaxDescription WHERE TaxDescId = @TaxDescId And BranchCode = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { TaxDescId = taxDescriptionCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region DepartmentMaster

        public async Task<IEnumerable<DepartmentMaster>> GetDepartmentList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "SELECT DepCode, DepName, DepHead, POSCode, Branch_code " +
                " FROM Department WHERE Branch_code = @BranchCode " +
                " Order by DepCode";

            return await connection.QueryAsync<DepartmentMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertDepartmentDetails(DepartmentMaster department)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO Department
            (DepCode, DepName, DepHead, POSCode, Branch_code)
            VALUES
            (@DepCode, @DepName, @DepHead, @POSCode, @Branch_code);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, department);
            return id;
        }

        public async Task<bool> UpdateDepartmentDetails(DepartmentMaster department)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE Department SET
            DepName = COALESCE(@DepName, DepName),
            DepHead = COALESCE(@DepHead, DepHead),
            POSCode = COALESCE(@POSCode, POSCode)
            WHERE DepCode = @DepCode And Branch_code = @Branch_code";
            var rows = await connection.ExecuteAsync(query, department);
            return rows > 0;
        }

        public async Task<bool> DeleteDepartmentDetails(int departmentCode, string branchcode)
        {
            const string query = "DELETE FROM Department WHERE DepCode = @DepCode AND Branch_code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { DepCode = departmentCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region OutletMaster

        public async Task<IEnumerable<OutletsMaster>> GetOutletsList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = "SELECT OltCode, POSCode, OltName, OltIsRoomService, OltServiceTaxRequired, OltAddress1," +
                " OltAddress2, TaxCode, UserCode, TRY_CAST(LastModify AS DATETIME) AS LastModify, ServiceCharge, branch_code, OltIsParcelService, isuploaded," +
                " ismodified, TinNo, SBCess, KKCess, InExTax, OltIsFastFood, IsDirectKOTandBill, IsDirectPaxandStw, IsDirectBill " +
                " FROM OutletMaster WHERE Branch_code = @BranchCode " +
                " Order by OltCode";

            return await connection.QueryAsync<OutletsMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertOutletDetails(OutletsMaster outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO OutletMaster
            (OltCode, POSCode, OltName, OltIsRoomService, OltServiceTaxRequired, OltAddress1, OltAddress2, TaxCode, UserCode, LastModify, ServiceCharge, branch_code, OltIsParcelService, isuploaded, ismodified, TinNo, SBCess, KKCess, InExTax, OltIsFastFood, IsDirectKOTandBill, IsDirectPaxandStw, IsDirectBill)
            VALUES
            (@OltCode, @POSCode, @OltName, @OltIsRoomService, @OltServiceTaxRequired, @OltAddress1, @OltAddress2, @TaxCode, @UserCode, @LastModify, @ServiceCharge, @Branch_code, @OltIsParcelService, @IsUploaded, @IsModified, @TinNo, @SBCess, @KKCess, @InExTax, @OltIsFastFood, @IsDirectKOTandBill, @IsDirectPaxandStw, @IsDirectBill);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, outlet);
            return id;
        }

        public async Task<bool> UpdateOutletDetails(OutletsMaster outlet)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE OutletMaster SET
            OltName = COALESCE(@OltName, OltName),
            OltIsRoomService = COALESCE(@OltIsRoomService, OltIsRoomService),
            OltServiceTaxRequired = COALESCE(@OltServiceTaxRequired, OltServiceTaxRequired),
            OltAddress1 = COALESCE(@OltAddress1, OltAddress1),
            OltAddress2 = COALESCE(@OltAddress2, OltAddress2),
            TaxCode = COALESCE(@TaxCode, TaxCode),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify),
            ServiceCharge = COALESCE(@ServiceCharge, ServiceCharge),
            OltIsParcelService = COALESCE(@OltIsParcelService, OltIsParcelService),
            IsModified = COALESCE(@IsModified, IsModified),
            TinNo = COALESCE(@TinNo, TinNo),
            SBCess = COALESCE(@SBCess, SBCess),
            KKCess = COALESCE(@KKCess, KKCess),
            InExTax = COALESCE(@InExTax, InExTax),
            OltIsFastFood = COALESCE(@OltIsFastFood, OltIsFastFood),
            IsDirectKOTandBill = COALESCE(@IsDirectKOTandBill, IsDirectKOTandBill),
            IsDirectPaxandStw = COALESCE(@IsDirectPaxandStw, IsDirectPaxandStw),
            IsDirectBill = COALESCE(@IsDirectBill, IsDirectBill)
            WHERE OltCode = @OltCode And Branch_code = @Branch_code ";
            var rows = await connection.ExecuteAsync(query, outlet);
            return rows > 0;
        }

        public async Task<bool> DeleteOutletDetails(int outletCode, string branchcode)
        {
            const string query = "DELETE FROM OutletMaster WHERE OltCode = @OutletCode And branch_code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { OutletCode = outletCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region TableMaster

        public async Task<IEnumerable<TablesMaster>> GetTablesList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " SELECT TblCode, OltCode, TblNo, TblSeatCount, UserCode, LastModify, POSCODE, c, Branch_Code, TableQRImage " +
                " FROM TableMaster WHERE Branch_Code = @BranchCode " +
                " Order by TblCode";

            return await connection.QueryAsync<TablesMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertTableDetails(InsertUpdateTableMaster table)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO TableMaster
            (TblCode, OltCode, TblNo, TblSeatCount, UserCode, LastModify, POSCODE, Branch_Code, TableQRImage)
            VALUES
            (@TblCode, @OltCode, @TblNo, @TblSeatCount, @UserCode, @LastModify, @POSCODE, @Branch_Code, @TableQRImage);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, table);
            return id;
        }

        public async Task<bool> UpdateTableDetails(InsertUpdateTableMaster table)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE TableMaster SET
            TblCode = COALESCE(@TblCode, TblCode),
            OltCode = COALESCE(@OltCode, OltCode),
            TblNo = COALESCE(@TblNo, TblNo),
            TblSeatCount = COALESCE(@TblSeatCount, TblSeatCount),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify),
            POSCODE = COALESCE(@POSCODE, POSCODE),
            TableQRImage = COALESCE(@TableQRImage, TableQRImage)
            WHERE TblCode = @TblCode And Branch_Code = @Branch_Code";
            var rows = await connection.ExecuteAsync(query, table);
            return rows > 0;
        }

        public async Task<bool> DeleteTableDetails(int tableCode, string branchcode)
        {
            const string query = "DELETE FROM TableMaster WHERE TblCode = @TableCode AND Branch_Code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { TableCode = tableCode, BranchCode = branchcode });
            return rows > 0;
        }

        public async Task<string> ServerName()
        {
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                string query = @"SELECT TOP 1 servername FROM ServerMaster";

                var servername = await connection.QueryFirstOrDefaultAsync<string>(query);

                return servername ?? string.Empty;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GetOutletName(int outlet, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var selectquery = "Select OltName from OutletMaster where OltCode= @Outlet and branch_code = @BranchCode";
            return await connection.QueryFirstOrDefaultAsync<string>(selectquery, new { Outlet = outlet, BranchCode = branchcode });
        }

        #endregion

        #region UnitMaster

        public async Task<IEnumerable<UnitMaster>> GetUnitList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select UnitCode, UnitName, UnitSymbol, Branch_Code " +
                " FROM UnitMaster WHERE Branch_Code = @BranchCode " +
                " Order by UnitCode";

            return await connection.QueryAsync<UnitMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertUnitDetails(UnitMaster unit)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO UnitMaster
            (UnitCode, UnitName, UnitSymbol, Branch_Code )
            VALUES
            (@UnitCode, @UnitName, @UnitSymbol, @Branch_Code);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, unit);
            return id;
        }

        public async Task<bool> UpdateUnitDetails(UnitMaster unit)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE UnitMaster SET
            UnitCode = COALESCE(@UnitCode, UnitCode),
            UnitName = COALESCE(@UnitName, UnitName),
            UnitSymbol = COALESCE(@UnitSymbol, UnitSymbol)
            WHERE UnitCode = @UnitCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, unit);
            return rows > 0;
        }

        public async Task<bool> DeleteUnitDetails(int unitCode, string branchcode)
        {
            const string query = "DELETE FROM UnitMaster WHERE UnitCode = @UnitCode And Branch_Code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { UnitCode = unitCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region GroupMaster

        public async Task<IEnumerable<GroupMaster>> GetGroupList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select GrpCode, GrpName, UserCode, LastModify, Branch_Code, Isuploaded, Dep " +
                " FROM ItemGroup WHERE Branch_Code = @BranchCode " +
                " Order by GrpCode";

            return await connection.QueryAsync<GroupMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertGroupDetails(GroupMaster group)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO ItemGroup
            (GrpCode, GrpName, UserCode, LastModify, Branch_Code, Isuploaded, Dep)
            VALUES
            (@GrpCode, @GrpName, @UserCode, @LastModify, @Branch_Code, @Isuploaded, @Dep);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, group);
            return id;
        }

        public async Task<bool> UpdateGroupDetails(GroupMaster group)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE ItemGroup SET
            GrpCode = COALESCE(@GrpCode, GrpCode),
            GrpName = COALESCE(@GrpName, GrpName),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify),
            Isuploaded = COALESCE(@Isuploaded, Isuploaded),
            Dep = COALESCE(@Dep, Dep)
            WHERE GrpCode = @GrpCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, group);
            return rows > 0;
        }

        public async Task<bool> DeleteGroupDetails(int GroupCode, string branchcode)
        {
            const string query = "DELETE FROM ItemGroup WHERE GrpCode = @GrpCode And Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { GrpCode = GroupCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region CategoryMaster

        public async Task<IEnumerable<CategoryMaster>> GetCategoryList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select CatCode, CatName, UserCode, LastModify, Branch_Code, SubCat, ImageUrl" +
                " FROM ItemCategory WHERE Branch_Code = @BranchCode " +
                " Order by CatCode";

            return await connection.QueryAsync<CategoryMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertCategoryDetails(CategoryMaster category)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO ItemCategory
            (CatCode, CatName, UserCode, LastModify, Branch_Code, SubCat, ImageUrl)
            VALUES
            (@CatCode, @CatName, @UserCode, @LastModify, @Branch_Code, @SubCat, @ImageUrl);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, category);
            return id;
        }

        public async Task<bool> UpdateCategoryDetails(CategoryMaster category)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE ItemCategory SET
            CatCode = COALESCE(@CatCode, CatCode),
            CatName = COALESCE(@CatName, CatName),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify),
            SubCat = COALESCE(@SubCat, SubCat),
            ImageUrl = COALESCE(@ImageUrl, ImageUrl)
            WHERE CatCode = @CatCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, category);
            return rows > 0;
        }

        public async Task<bool> DeleteCategoryDetails(int CategoryCode, string branchcode)
        {
            const string query = "DELETE FROM ItemCategory WHERE CatCode = @CatCode And Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { CatCode = CategoryCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region SubCategoryMaster

        public async Task<IEnumerable<SubCategoryMaster>> GetSubCategoryList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select CatCode, CatName, SubCatCode, SubCatName, UserCode, Branch_Code, TrDate, SubCat " +
                " FROM ItemSubCategory WHERE Branch_Code = @BranchCode " +
                " Order by CatCode";

            return await connection.QueryAsync<SubCategoryMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertSubCategoryDetails(SubCategoryMaster subCategory)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO ItemSubCategory
            (CatCode, CatName, SubCatCode, SubCatName, UserCode, Branch_Code, TrDate, SubCat)
            VALUES
            (@CatCode, @CatName, @SubCatCode, @SubCatName, @UserCode, @Branch_Code, @TrDate, @SubCat);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, subCategory);
            return id;
        }

        public async Task<bool> UpdateSubCategoryDetails(SubCategoryMaster subCategory)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE ItemSubCategory SET
            CatCode = COALESCE(@CatCode, CatCode),
            CatName = COALESCE(@CatName, CatName),
            SubCatCode = COALESCE(@SubCatCode, SubCatCode),
            SubCatName = COALESCE(@SubCatName, SubCatName),
            UserCode = COALESCE(@UserCode, UserCode),
            TrDate = COALESCE(@TrDate, TrDate),
            SubCat = COALESCE(@SubCat, SubCat)
            WHERE SubCatCode = @SubCatCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, subCategory);
            return rows > 0;
        }

        public async Task<bool> DeleteSubCategoryDetails(int SubCategoryCode, string branchcode)
        {
            const string query = "DELETE FROM ItemSubCategory WHERE SubCatCode = @SubCatCode And Branch_Code = @BranchCode ";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { SubCatCode = SubCategoryCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region ItemMaster

        public async Task<IEnumerable<MasterItemModel>> GetItemsList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            //var selectquery = "SELECT ItemCode, ItemName, ItemDisplayName, QPB, CatCode, GrpCode, ItemDiscountAllowed, ItemRate, ItemSaleQtyUnit, UserCode, LastModify, Unit, ItemType, mostrunningitemsrno, subItem, ItemOpStock, ItemCurStock, ItemOpRate, ItemCurRate, UnitCode, ItemROQ, ItemROL, Dep, Opstock, Ctstock, perqty, DepCode, perrate, SUnit, branch_code, picture, thumb, Barcode, IsVeg FROM dbo.ItemMaster WHERE Branch_Code = @Branchcode";

            var selectquery = $"SELECT im.ItemCode, ItemName, ItemDisplayName, QPB, CatCode, GrpCode, ItemDiscountAllowed, ItemRate," +
                $" ItemSaleQtyUnit, UserCode, im.LastModify, Unit, ItemType, mostrunningitemsrno, subItem, ItemOpStock, " +
                $" ItemCurStock, ItemOpRate, ItemCurRate, UnitCode, ItemROQ, ItemROL, Dep, Opstock, Ctstock, perqty, DepCode, " +
                $" perrate, SUnit, im.branch_code, picture, thumb, Barcode, IsVeg, ec.ChargeCode as TaxCode, ec.ChargeName as TaxName, olt.OltCode as oltcode " +
                $" FROM dbo.ItemMaster im " +
                $" OUTER APPLY ( SELECT TOP 1 ChargeCode, ChargeName FROM ExtraCharges ec WHERE ec.ItemCode = im.ItemCode " +
                $" AND ec.Branch_Code = im.branch_code ORDER BY ec.OltCode ) ec " +
                $" OUTER APPLY(SELECT STUFF ( ( SELECT ',' + CAST(od2.OltCode AS VARCHAR(10)) FROM OltItemDetails od2 WHERE od2.ItemCode = im.ItemCode " +
                $" order by od2.OltCode  FOR XML PATH(''), TYPE ).value('.', 'NVARCHAR(MAX)') ,1,1,'') AS OltCode) olt " +
                $" WHERE Branch_Code = @Branchcode order by im.ItemCode";

            return await connection.QueryAsync<MasterItemModel>(selectquery, new { Branchcode = branchcode });
        }

        public async Task<int> InsertItemDetails(InsertMasterItemModel item)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO ItemMaster
            (ItemCode, ItemName, ItemDisplayName, CatCode, GrpCode, ItemDiscountAllowed, ItemRate, ItemSaleQtyUnit, UserCode, LastModify, ItemType, VATAPP, VATPER, subItem, ItemOpStock, ItemCurStock, ItemOpRate, ItemCurRate, UnitCode, ItemROQ, ItemROL, Dep, perqty, Opstock, Ctstock, DepCode, perrate, Unit, SUnit, branch_code, Picture, thumb, Barcode, mostrunningitemsrno, QPB, IsVeg)
            VALUES
            (@itemcode, @itemname, @itemdisplayname, @catcode, @grpcode, @itemdiscountallowed, @itemrate, @itemsaleqtyunit, @usercode, @lastmodify, @itemtype, @vatapp, @vatper, @subItem, @itemopstock, @itemcurstock, @itemoprate, @itemcurrate, @unitcode, @itemroq, @itemrol, @dep, @perqty, @opstock, @ctstock,  @depcode, @perrate, @unit, @sunit, @branch_code, @picture, @thumb, @barcode, @mostrunningitemsrno, @qpb, @isveg);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = connection.ExecuteScalar<int>(query, new
            {
                itemcode = item.ItemCode,
                itemname = item.ItemName.ToUpper(),
                itemdisplayname = item.ItemName.ToUpper(),
                catcode = item.CatCode,
                grpcode = item.GrpCode,
                itemdiscountallowed = item.ItemDiscountAllowed,
                itemrate = item.ItemRate,
                itemsaleqtyunit = "1",
                usercode = item.UserCode,
                lastmodify = item.LastModify,
                itemtype = 0,
                vatapp = 0,
                vatper = 0,
                subItem = 0,
                itemopstock = 0,
                itemcurstock = 0,
                itemoprate = item.ItemRate,
                itemcurrate = 0,
                unitcode = item.UnitCode,
                itemroq = 0,
                itemrol = 0,
                dep = string.IsNullOrEmpty(item.Dep) ? "" : item.Dep.Substring(0, 1),
                perqty = 0,
                opstock = 0,
                ctstock = 0,
                depcode = item.DepCode,
                perrate = 0,
                unit = item.UnitName,
                sunit = item.UnitName,
                branch_code = item.BranchCode,
                picture = item.SACCode,
                thumb = item.Thumb,
                barcode = item.Barcode,
                mostrunningitemsrno = item.PrintDepartment,
                qpb = item.SubCatCode,
                isveg = item.IsVeg
            });

            var outlets = connection.Query("SELECT OltCode FROM outletmaster where branch_code = @branch_code", new { branch_code = item.BranchCode }).ToList();

            //if(item.OltCode != null && item.OltCode > 0 && !string.IsNullOrEmpty(item.OutletName) && item.OutletName != "All")
            //{
            //    outlets = outlets.Where(x => x.OltCode == item.OltCode).ToList();
            //}
            //else if(item.OltCode != null && item.OltCode == 0 && !string.IsNullOrEmpty(item.OutletName) && item.OutletName == "All")
            //{
            //    outlets = outlets.ToList();
            //}
            //else
            //{
            //    outlets = null;
            //}

            if (item.OltCodes != null && item.OltCodes.Any()) 
            {
                outlets = outlets.Where(x => item.OltCodes.Contains((int)x.OltCode)) .ToList();
            }
            else
            {
                outlets = outlets.ToList();
            }

            foreach (var outlet in outlets)
            {
                  int CreateOIDcode = await Findnextnumber("OltItemDetails", "OIDCode", "Branch_Code", item.BranchCode);

                const string insertQuery = @"
                INSERT INTO OltItemDetails(OIDCode, OltCode, POSCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, Branch_Code, vatper, IsFree)
                VALUES
                (@OIDCode, @OltCode, @POSCode, @ItemCode, @TaxCode, @OIDRate, @OIDAvailable, @ItemDiscountRequired, @usercode, @lastmodify, @branchcode, @vatper, @IsFree);";

                connection.Execute(insertQuery, new
                {
                    OIDCode = CreateOIDcode,
                    OltCode = Convert.ToInt16(outlet.OltCode),
                    POSCode = 1,
                    ItemCode = Convert.ToInt16(item.ItemCode),
                    TaxCode = Convert.ToInt16(item.TaxCode),
                    OIDRate = Convert.ToDecimal(item.ItemRate),
                    OIDAvailable = 1,
                    ItemDiscountRequired = item.ItemDiscountAllowed,
                    usercode = Convert.ToInt32(item.UserCode),
                    lastmodify = item.LastModify,
                    branchcode = item.BranchCode,
                    vatper = 0,
                    IsFree = 0
                });
            }

            return id;
        }

        public async Task<bool> UpdateItemDetails(InsertMasterItemModel item)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            connection.Open();

            string updateOltItemDetails = string.Empty;
            int trno = 0;

            using var transaction = connection.BeginTransaction();

            try
            {
                const string updateItemMaster = @"
                UPDATE ItemMaster SET
                ItemName = COALESCE(@itemname, ItemName),
                ItemDisplayName = COALESCE(@itemname, ItemDisplayName),
                ItemDiscountAllowed = COALESCE(@itemdiscount, ItemDiscountAllowed),
                CatCode = COALESCE(@catcode, CatCode),
                GrpCode = COALESCE(@grpcode, GrpCode),
                ItemRate = COALESCE(@itemrate, ItemRate),
                Unit = COALESCE(@unitname, Unit),
                SUnit = COALESCE(@sunit, SUnit),
                UnitCode = COALESCE(@unitcode, UnitCode),
                UserCode = COALESCE(@usercode, UserCode),
                LastModify = COALESCE(@lastmodify, LastModify),
                ItemOpRate = COALESCE(@itemoprate, ItemOpRate),
                Dep = COALESCE(@dep, Dep),
                DepCode = COALESCE(@depcode, DepCode),
                Isuploaded = 0,
                mostrunningitemsrno = COALESCE(@mostrunningitemsrno, mostrunningitemsrno),
                QPB = COALESCE(@qpb, QPB),
                BarCode = COALESCE(@barcode, BarCode),
                IsVeg = COALESCE(@isveg, IsVeg),
                Picture = COALESCE(@picture, Picture),
                Thumb = COALESCE(@thumb, Thumb)
                WHERE ItemCode = @itemcode AND Branch_code = @branchcode;";

                var parameters = new
                {
                    itemcode = item.ItemCode,
                    itemname = item.ItemName?.ToUpper(),
                    itemdisplayname = item.ItemName?.ToUpper(),
                    catcode = item.CatCode,
                    grpcode = item.GrpCode,
                    itemdiscount = item.ItemDiscountAllowed,
                    itemrate = item.ItemRate,
                    usercode = item.UserCode,
                    lastmodify = item.LastModify,
                    itemoprate = item.ItemRate,
                    unitcode = item.UnitCode,
                    unitname = item.UnitName,
                    sunit = item.UnitName,
                    dep = string.IsNullOrEmpty(item.Dep) ? "" : item.Dep.Substring(0, 1),
                    depcode = item.DepCode,
                    mostrunningitemsrno = item.PrintDepartment,
                    picture = item.SACCode,
                    qpb = item.SubCatCode,
                    barcode = item.Barcode,
                    isveg = item.IsVeg,
                    thumb = item.Thumb,
                    branchcode = item.BranchCode,
                    oltcode = item.OltCodes,
                    //outletname = item.OutletName,
                    //oldoltcode = item.oldOltCode,

                    // For other tables
                    catid = item.CatCode,
                    taxcode = item.TaxCode,
                    vatper = 0
                };

                int r1 = await connection.ExecuteAsync(updateItemMaster, parameters, transaction);

                // =========================
                // Delete Existing Outlet Mapping
                // =========================

                const string deleteOutletDetails = @" DELETE FROM OltItemDetails WHERE ItemCode = @itemcode AND Branch_Code = @branchcode;";

                await connection.ExecuteAsync( deleteOutletDetails, new { itemcode = item.ItemCode, branchcode = item.BranchCode }, transaction);

                // =========================
                // Insert Selected Outlets
                // =========================

                if (item.OltCodes != null && item.OltCodes.Any())
                {
                    const string insertOutletQuery = @" 
                    INSERT INTO OltItemDetails
                    ( OIDCode, OltCode, POSCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, Branch_Code, vatper, IsFree )
                    VALUES
                    ( @OIDCode, @OltCode, @POSCode, @ItemCode, @TaxCode, @OIDRate, @OIDAvailable, @ItemDiscountRequired, @UserCode, @LastModify, @BranchCode, @vatper, @IsFree );";

                    foreach (var oltCode in item.OltCodes)
                    {
                        var str = $@"select ISNULL(MAX(OIDCode), 0) AS trno from OltItemDetails WITH (HOLDLOCK, ROWLOCK) where Branch_Code = @Branch ";
                        var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str, new { Branch = item.BranchCode }, transaction);
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

                        int CreateOIDcode = trno;


                        await connection.ExecuteAsync( insertOutletQuery,
                            new
                            {
                                OIDCode = CreateOIDcode,
                                OltCode = oltCode,
                                POSCode = 1,
                                ItemCode = item.ItemCode,
                                TaxCode = item.TaxCode,
                                OIDRate = item.ItemRate,
                                OIDAvailable = 1,
                                ItemDiscountRequired = item.ItemDiscountAllowed,
                                UserCode = item.UserCode,
                                LastModify = item.LastModify,
                                BranchCode = item.BranchCode,
                                vatper = 0,
                                IsFree = 0
                            }, transaction);
                    }
                }

                    ////if (item.OltCode != null && item.OltCode > 0 && !string.IsNullOrEmpty(item.OutletName) && item.OutletName != "All")
                    //if (item.OltCodes != null && item.OltCodes.Any() && !string.IsNullOrEmpty(item.OutletName) && item.OutletName != "All")
                    //{
                    //    if(item.IsOltchanged)
                    //    {
                    //        updateOltItemDetails = @"
                    //        UPDATE OltItemDetails SET
                    //        OltCode = @oltcode,
                    //        OIDRATE = COALESCE(@itemrate, OIDRATE),
                    //        taxcode = COALESCE(@taxcode, taxcode),
                    //        vatper = COALESCE(@vatper, vatper)
                    //        WHERE ItemCode = @itemcode AND OltCode = @oldoltcode AND Branch_code = @branchcode; ";
                    //    }
                    //    else
                    //    {
                    //        updateOltItemDetails = @"
                    //        UPDATE OltItemDetails SET
                    //        OIDRATE = COALESCE(@itemrate, OIDRATE),
                    //        taxcode = COALESCE(@taxcode, taxcode),
                    //        vatper = COALESCE(@vatper, vatper)
                    //        WHERE ItemCode = @itemcode AND OltCode = @oltcode AND Branch_code = @branchcode; ";
                    //    }
                    //}
                    ////else if (item.OltCode != null && item.OltCode == 0 && !string.IsNullOrEmpty(item.OutletName) && item.OutletName == "All")
                    //else if (!string.IsNullOrEmpty(item.OutletName) && item.OutletName == "All")
                    //{
                    //    updateOltItemDetails = @"
                    //    UPDATE OltItemDetails SET
                    //    OIDRATE = COALESCE(@itemrate, OIDRATE),
                    //    taxcode = COALESCE(@taxcode, taxcode),
                    //    vatper = COALESCE(@vatper, vatper)
                    //    WHERE ItemCode = @itemcode AND Branch_code = @branchcode;";
                    //}


                    //int r3 = await connection.ExecuteAsync(updateOltItemDetails, parameters, transaction);

                    transaction.Commit();

                return (r1) > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteItemDetails(int itemcode, string branchcode, List<int> oltcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string deleteOltItemDetails = string.Empty;
            string deleteExtraCharges = string.Empty;
            string deleteItemMaster = string.Empty;

            //var oltcodeString = string.Join(",", oltcode);


            if ((oltcode != null && oltcode.Any()))
            {
                deleteItemMaster = @" DELETE FROM ItemMaster WHERE ItemCode = @ItemCode AND branch_code = @BranchCode";

                deleteOltItemDetails = @" DELETE FROM OltItemDetails WHERE ItemCode = @ItemCode AND OltCode IN @OltCode AND branch_code = @BranchCode"; 
                
                deleteExtraCharges = @" DELETE FROM ExtraCharges WHERE ItemCode = @ItemCode AND OltCode IN @OltCode AND Branch_Code = @BranchCode";        
            }
            else
            {
                deleteItemMaster = @" DELETE FROM ItemMaster WHERE ItemCode = @ItemCode AND branch_code = @BranchCode";

                deleteOltItemDetails = @" DELETE FROM OltItemDetails WHERE ItemCode = @ItemCode AND branch_code = @BranchCode";

                deleteExtraCharges = @" DELETE FROM ExtraCharges WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";
            }

            // Execute all deletes
            int r1 = await connection.ExecuteAsync(deleteItemMaster, new { ItemCode = itemcode, BranchCode = branchcode });
            int r2 = await connection.ExecuteAsync(deleteOltItemDetails, new { ItemCode = itemcode, OltCode = oltcode, BranchCode = branchcode });
            int r3 = await connection.ExecuteAsync(deleteExtraCharges, new { ItemCode = itemcode, OltCode = oltcode, BranchCode = branchcode });

            return r1 > 0 && r2 > 0 && r3 > 0;
        }

        public async Task<bool> ItemNameExist(int id, string itemName, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            bool isExist = false;

            const string query = "Select * FROM ItemMaster WHERE ItemCode = @ItemCode AND ItemName = @ItemName AND Branch_Code = @BranchCode and subitem = 0";

            var presentdetail = await connection.QueryFirstOrDefaultAsync<MasterItemModel>(query, new { itemcode = id, itemname = itemName, branchcode });

            if (presentdetail != null && presentdetail.ItemName == itemName)
            {
                return isExist = true;
            }
            else
            {
                return isExist = false;
            }
        }

        public async Task<bool> IsDeletable(int itemcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM KOTDetails WHERE ItemCode = @ItemCode AND branch_code = @BranchCode";
            int count = connection.ExecuteScalar<int>(query, new
            { ItemCode = itemcode, BranchCode = branchcode });

            return count > 0;
        }

        public async Task<int> DeleteExtraChargesAsync(int itemCode, string branchCode, List<int> oltcode, string outletname, IDbConnection con)
        {
            string query = string.Empty;

            ////if ((oltcode != null || oltcode > 0) && (!string.IsNullOrEmpty(outletname) || outletname != "All"))
            //if (oltcode != null && oltcode.Any() && !string.IsNullOrEmpty(outletname) && outletname != "All")
            //{
            //    if (isOltChanged)
            //    {
            //        query = @" DELETE FROM ExtraCharges WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode AND OltCode = @OldOltCode";
            //    }
            //}
            ////else if ((oltcode != null || oltcode == 0) && (!string.IsNullOrEmpty(outletname) || outletname == "All"))
            //else if ((oltcode != null || oltcode == 0) && (!string.IsNullOrEmpty(outletname) || outletname == "All"))
            //{
            //    query = @" DELETE FROM ExtraCharges WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";
            //}

            query = @" DELETE FROM ExtraCharges WHERE ItemCode = @ItemCode AND Branch_Code = @BranchCode";

            int result = await con.ExecuteAsync(query, new { ItemCode = itemCode, BranchCode = branchCode});
            return result;
        }

        public async Task<int> InsertExtraChargesAsync(int chargeCode, string chargeName, int itemCode, string branchCode, DateTime currentTime, List<int> oltCode, string outletName, IDbConnection con)
        {

            string outletQuery = @"SELECT OltCode FROM OutletMaster Where branch_code = @BranchCode";

            var outletCodes = await con.QueryAsync<int>(outletQuery, new { BranchCode = branchCode });

            //if (oltCode != null && oltCode > 0 && !string.IsNullOrEmpty(outletName) && outletName != "All")
            if (oltCode != null && oltCode.Any() && !string.IsNullOrEmpty(outletName) && outletName != "All")
            {
                outletCodes = outletCodes.Where(x => oltCode.Contains(x)).ToList();
            }
            //else if (oltCode != null && oltCode == 0 && !string.IsNullOrEmpty(outletName) && outletName == "All")
            else
            {
                outletCodes = outletCodes.ToList();
            }

            // Insert query
            string insertQuery = @"
                INSERT INTO ExtraCharges ( ChargeCode, ChargeName, ItemCode, Branch_Code, OltCode, lastmodify )
                VALUES ( @ChargeCode, @ChargeName, @ItemCode, @BranchCode, @OltCode, @lastmodify )";

            foreach (var olt in outletCodes)
            {
                await con.ExecuteAsync(insertQuery, new
                {
                    ChargeCode = chargeCode,
                    ChargeName = chargeName,
                    ItemCode = itemCode,
                    BranchCode = branchCode,
                    OltCode = olt,
                    lastmodify = currentTime
                });
            }

            return 1; // You can return the number of records inserted if needed
        }

        #endregion

        #region StewardMaster

        public async Task<IEnumerable<MasterSteward>> GetStewardList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select StwCode, POSCode, StwName, UserCode, LastModify, Branch_Code, MobNo" +
                " FROM StewardMaster WHERE Branch_Code = @BranchCode " +
                " Order by StwCode";

            return await connection.QueryAsync<MasterSteward>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertStewardDetails(MasterSteward steward)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO StewardMaster
            (StwCode, StwName, POSCode, UserCode, LastModify, Branch_code, MobNo)
            VALUES
            (@StwCode, @StwName, @POSCode, @UserCode, @LastModify, @Branch_code, @MobNo);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, steward);
            return id;
        }

        public async Task<bool> UpdateStewardDetails(MasterSteward steward)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE StewardMaster SET
            StwName = COALESCE(@StwName, StwName),
            MobNo = COALESCE(@MobNo, MobNo),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify)
            WHERE StwCode = @StwCode And Branch_Code = @Branch_Code ";
            var rows = await connection.ExecuteAsync(query, steward);
            return rows > 0;
        }

        public async Task<bool> DeleteStewardDetails(int StewardCode, string branchcode)
        {
            const string query = "DELETE FROM StewardMaster WHERE StwCode = @StewardCode AND Branch_Code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { StewardCode = StewardCode, BranchCode = branchcode });
            return rows > 0;
        }

        public async Task<bool> IsStewardDeletable(int stwCode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "Select Count(1) as RecCount from KOTMaster Where StwCode = @Code And Branch_Code = @BranchCode";
            int count = connection.ExecuteScalar<int>(query, new
            { Code = stwCode, BranchCode = branchcode });

            return count > 0;
        }

        #endregion

        #region NCDepartmentMaster

        public async Task<IEnumerable<NCDepartmentMaster>> GetNCDepartmentList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select NCDepCode, NCDepName, UserId, lastmodify, Branch_Code" +
                " FROM NCDepartment WHERE Branch_Code = @BranchCode " +
                " Order by NCDepCode";

            return await connection.QueryAsync<NCDepartmentMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertNCDepartmentDetails(NCDepartmentMaster ncdepartment)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO NCDepartment
            (NCDepCode, NCDepName, UserId, LastModify, Branch_code)
            VALUES
            (@NCDepCode, @NCDepName, @UserId, @LastModify, @Branch_code);
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, ncdepartment);
            return id;
        }

        public async Task<bool> UpdateNCDepartmentDetails(NCDepartmentMaster ncdepartment)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE NCDepartment SET
            NCDepName = COALESCE(@NCDepName, NCDepName),
            UserId = COALESCE(@UserId, UserId),
            LastModify = COALESCE(@LastModify, LastModify)
            WHERE NCDepCode = @NCDepCode And Branch_code = @Branch_code ";
            var rows = await connection.ExecuteAsync(query, ncdepartment);
            return rows > 0;
        }

        public async Task<bool> DeleteNCDepartmentDetails(int NCDeptCode, string branchcode)
        {
            const string query = "DELETE FROM NCDepartment WHERE NCDepCode = @NCDeptCode AND Branch_Code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { NCDeptCode = NCDeptCode, BranchCode = branchcode });
            return rows > 0;
        }

        public async Task<bool> IsNCDepartmentDeletable(int ncDeptCode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            string query = "Select Count(1) as RecCount from KOTMaster Where RefKotNo = @Code And Branch_Code = @BranchCode";
            int count = connection.ExecuteScalar<int>(query, new
            { Code = ncDeptCode, BranchCode = branchcode });

            return count > 0;
        }

        #endregion

        #region PrintingMaster

        public async Task<IEnumerable<PrintingMaster>> GetPrintingMasterList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = " Select DepCode, DepName, usercode, lastmodify, Branch_Code, isuploaded " +
                " FROM PrintingDepartment WHERE Branch_Code = @BranchCode " +
                " Order by DepCode";

            return await connection.QueryAsync<PrintingMaster>(query, new { BranchCode = branchcode });
        }

        public async Task<int> InsertPrintingMasterDetails(PrintingMaster printingmaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            INSERT INTO PrintingDepartment 
            (DepCode, DepName, UserCode, LastModify, Branch_code, IsUploaded)
            VALUES
            (@DepCode, @DepName, @UserCode, @LastModify, @Branch_code, '0');
        
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var id = await connection.ExecuteScalarAsync<int>(query, printingmaster);
            return id;
        }

        public async Task<bool> UpdatePrintingMasterDetails(PrintingMaster printingmaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            const string query = @"
            UPDATE PrintingDepartment SET
            DepName = COALESCE(@DepName, DepName),
            UserCode = COALESCE(@UserCode, UserCode),
            LastModify = COALESCE(@LastModify, LastModify)
            WHERE DepCode = @DepCode And Branch_code = @Branch_code ";
            var rows = await connection.ExecuteAsync(query, printingmaster);
            return rows > 0;
        }

        public async Task<bool> DeletePrintingMasterDetails(int printingMasterCode, string branchcode)
        {
            const string query = "DELETE FROM PrintingDepartment WHERE DepCode = @DepCode AND Branch_Code = @BranchCode";

            using var connection = _factory.CreateConnection(DbNames.POS);

            var rows = await connection.ExecuteAsync(query, new { DepCode = printingMasterCode, BranchCode = branchcode });
            return rows > 0;
        }

        #endregion

        #region ImportItemMaster

        public async Task<bool> ItemExists(int itemCode, string branch)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM ItemMaster WHERE ItemCode = @ItemCode AND Branch_Code = @Branch";

            var count = await con.ExecuteScalarAsync<int>(query, new { itemCode, Branch = branch });
            return count > 0;
        }

        public async Task<bool> ItemNameExists(string itemname, string branch)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query = "SELECT COUNT(1) FROM ItemMaster WHERE ItemName = @ItemName AND Branch_Code = @Branch";

            var count = await con.ExecuteScalarAsync<int>(query, new { itemname, Branch = branch });
            return count > 0;
        }

        public async Task<string> GetOrCreate(string table, string codeCol, string nameCol, string value, string branchcode, string usercode, DateTime currentime, ImportItemRow item)
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
                if (table == "OutletMaster")
                {
                    int nxtnumber = await Findnextnumber("OutletMaster", "OltCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO OutletMaster(OltCode, OltName, UserCode, LastModify, Branch_Code)" +
                        $" OUTPUT INSERTED.OltCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @LastModify, @BranchCode)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode });
                }
                else if (table == "ItemCategory")
                {
                    int nxtnumber = await Findnextnumber("ItemCategory", "CatCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO ItemCategory(CatCode, CatName, UserCode, LastModify, Branch_Code, SubCat)" +
                        $" OUTPUT INSERTED.CatCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @LastModify, @BranchCode, 0)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode });
                }
                else if(table == "ItemGroup")
                {
                    int nxtnumber = await Findnextnumber("ItemGroup", "GrpCode", "Branch_Code", branchcode);

                    var dep = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);

                    string insertQuery = $"INSERT INTO ItemGroup(GrpCode, GrpName, UserCode, Branch_code, LastModify, Isuploaded, Dep)" +
                        $" OUTPUT INSERTED.GrpCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @BranchCode, @LastModify, 0, @Dep)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode, Dep = dep.ToUpper() });
                }
                else if(table == "Department")
                {
                    int nxtnumber = await Findnextnumber("Department", "DepCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO Department(DepCode, DepName, DepHead, Branch_code)" +
                        $" OUTPUT INSERTED.DepCode " +
                        $" VALUES (@Nxtnumber, @Name, @Name, @BranchCode)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value.ToUpper(), BranchCode = branchcode});
                }
                else if(table == "PrintingDepartment")
                {
                    int nxtnumber = await Findnextnumber("PrintingDepartment", "DepCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO PrintingDepartment(DepCode, DepName, UserCode, Branch_code, LastModify, Isuploaded) " +
                        $" OUTPUT INSERTED.DepCode " +
                        $" VALUES (@Nxtnumber, @Name, @usercode, @BranchCode, @LastModify, 0)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, usercode = usercode, LastModify = currentime, BranchCode = branchcode });
                }
                else if(table == "UnitMaster")
                {
                    int nxtnumber = await Findnextnumber("UnitMaster", "UnitCode", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO UnitMaster(UnitCode, UnitName, UnitSymbol, Branch_Code) " +
                        $" OUTPUT INSERTED.UnitCode " +
                        $" VALUES (@Nxtnumber, @Name, @Name, @BranchCode)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value, BranchCode = branchcode });
                }
                else if(table == "ItemCategoryNew")
                {
                    int nxtnumber = await Findnextnumber("ItemCategoryNew", "CatgoryId", "Branch_Code", branchcode);

                    string insertQuery = $"INSERT INTO ItemCategoryNew(CatgoryId, CategoryName, Branch_Code, Priority) " +
                        $" OUTPUT INSERTED.CatgoryId " +
                        $" VALUES (@Nxtnumber, @Name, @BranchCode, @priority)";

                    return await con.ExecuteScalarAsync<string>(insertQuery, new { Nxtnumber = nxtnumber, Name = value.ToUpper(),  BranchCode = branchcode, priority = nxtnumber });
                }
                else if(table == "BillTaxMaster")
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

        //public Task<string> GetOrCreateCategory(string category)
        //{
        //    return GetOrCreate("ItemCategory", "CatCode", "CatName", category);
        //}

        public async Task<string> GetOrCreateSubCategory(string category, string subCategory, string branchcode, DateTime currentTime, string userCode)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            string query1 = @"SELECT SubCatCode FROM ItemSubCategory 
              WHERE SubCatName=@Sub AND CatName=@Cat AND Branch_Code = @BranchCode";

            var code = await con.ExecuteScalarAsync<string>(query1, new { Sub = subCategory, Cat = category, BranchCode = branchcode });
            if (!string.IsNullOrEmpty(code))
                return code;
            else
            {

                int nxtnumber = await Findnextnumber("ItemSubCategory", "SubCatCode", "Branch_Code", branchcode);

                string Ccode = await GetOrCreate("ItemCategory", "CatCode", "CatName", category, branchcode, userCode, DateTime.Now, null);

                string insertQuery = $@"INSERT INTO ItemSubCategory (CatCode, CatName, SubCatCode, SubCatName, UserCode, Trdate, Branch_Code, SubCat) 
                  OUTPUT INSERTED.SubCatCode
                  VALUES (@catcode, @Cat, @Nxtnumber, @Sub, @UserCode, @Trdate, @BranchCode, 0)";
                return await con.ExecuteScalarAsync<string>(insertQuery, new { catcode = Ccode, Cat = category, Nxtnumber = nxtnumber, Sub = subCategory.ToUpper(), UserCode = userCode, Trdate = currentTime, BranchCode = branchcode });   
            }
        }

        public async Task InsertItemMaster(ImportItemRow item, List<int> Oltcodes, string Catcode, string SubCatcode, string Groupcode, string Deptcode, string Printdeptcode, string Unitcode, string Taxcode, string Taxname, string Dep, string Branchcode, string usercode, DateTime currentdate)
        {
            using var con = _factory.CreateConnection(DbNames.POS);

            var sql = $@"Insert Into ItemMaster(ItemCode, ItemName, ItemDisplayName, CatCode, GrpCode, ItemDiscountAllowed, ItemRate, ItemSaleQtyUnit, UserCode, LastModify, ItemType, VATAPP, VATPER, subitem, ItemOpStock, ItemCurStock, ItemOpRate, ItemCurRate, UnitCode, ItemROQ, ItemROL, dep, perqty, opstock, ctstock, DepCode, perrate, unit, sunit, Branch_Code, Picture, Isuploaded, mostrunningitemsrno, QPB, IsVeg) 
              VALUES
              (@itemcode, @itemname, @itemdisplayname, @catcode, @grpcode, @itemdiscountallowed, @itemrate, @itemsaleqtyunit, @Usercode, @lastmodify, @itemtype, @vatapp, @vatper, @subitem, @itemopstock, @itemcurstock, @itemoprate, @itemcurrate, @unitcode, @itemroq, @itemrol, @dep, @perqty, @opstock, @ctstock, @depcode, @perrate, @unit, @sunit, @branch_code, @picture, @isuploaded, @mostrunningitemsrno, @qpb, @isveg)";

            await con.ExecuteAsync(sql, new
            {
                itemcode = item.ItemCode,
                itemname = item.ItemName.ToUpper(),
                itemdisplayname = item.ItemName.ToUpper(),
                catcode = Catcode,
                grpcode = Groupcode,
                itemdiscountallowed = 1,
                itemrate = item.Rate,
                itemsaleqtyunit = 1,
                Usercode = usercode,
                lastmodify = currentdate,
                itemtype = 0,
                vatapp = 0,
                vatper = 0,
                subitem = 0,
                itemopstock = 0,
                itemcurstock = 0,
                itemoprate = item.Rate,
                itemcurrate = 0,
                unitcode = Unitcode,
                itemroq = 0,
                itemrol = 0,
                dep = Dep,
                perqty = 0,
                opstock = 0,
                ctstock = 0,
                depcode = Deptcode,
                perrate = 0,
                unit = item.Unit,
                sunit = item.Unit,
                branch_code = Branchcode,
                picture = item.SacCode,
                isuploaded = 0,
                mostrunningitemsrno = Printdeptcode.ToString(),
                qpb = SubCatcode,
                isveg = item.IsVeg
            });

            if (Oltcodes != null && Oltcodes.Any())
            {
                foreach (var outlet in Oltcodes)
                {
                    int CreateOIDcode = await Findnextnumber("OltItemDetails", "OIDCode", "Branch_Code", Branchcode);

                    const string insertOltQuery = @"
                        INSERT INTO OltItemDetails(OIDCode, OltCode, POSCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, Branch_Code, vatper, IsFree)
                        VALUES
                        (@OIDCode, @OltCode, @POSCode, @ItemCode, @TaxCode, @OIDRate, @OIDAvailable, @ItemDiscountRequired, @usercode, @lastmodify, @branchcode, @vatper, @IsFree);";

                    await con.ExecuteAsync(insertOltQuery,
                    new
                    {
                        OIDCode = CreateOIDcode,
                        OltCode = outlet,
                        POSCode = 1,
                        taxcode = Taxcode,
                        itemcode = item.ItemCode,
                        OIDRate = item.Rate,
                        OIDAvailable = 1,
                        ItemDiscountRequired = false,
                        UserCode = usercode,
                        LastModify = currentdate,
                        BranchCode = Branchcode,
                        vatper = 0,
                        IsFree = 0
                    });

                    string insertQuery = @"
                         INSERT INTO ExtraCharges ( ChargeCode, ChargeName, ItemCode, Branch_Code, OltCode, lastmodify )
                         VALUES ( @ChargeCode, @ChargeName, @ItemCode, @BranchCode, @OltCode, @lastmodify )";

                    await con.ExecuteAsync(insertQuery, new
                    {
                        ChargeCode = Taxcode,
                        ChargeName = Taxname,
                        ItemCode = item.ItemCode,
                        BranchCode = Branchcode,
                        OltCode = outlet,
                        lastmodify = currentdate
                    });
                }

                //int trno = 0;
                //const string insertOutletQuery = @" 
                //    INSERT INTO OltItemDetails
                //    ( OIDCode, OltCode, POSCode, Taxcode, ItemCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, Branch_Code, vatper, IsFree )
                //    VALUES
                //    ( @OIDCode, @OltCode, @POSCode, @taxcode, @itemcode, @OIDRate, @OIDAvailable, @ItemDiscountRequired, @UserCode, @LastModify, @BranchCode, @vatper, @IsFree );";

                //var str = $@"select ISNULL(MAX(OIDCode), 0) AS trno from OltItemDetails WITH (HOLDLOCK, ROWLOCK) where Branch_Code = @Branch ";
                //var ds = await con.QueryFirstOrDefaultAsync<dynamic>(str, new { Branch = Branchcode });
                //if (ds.trno != null)
                //{
                //    if (trno == 0)
                //    {
                //        if (string.IsNullOrEmpty(ds.trno.ToString()))
                //        {
                //            trno = trno + 1;
                //        }
                //        else
                //        {
                //            trno = Convert.ToInt32(ds.trno);
                //            trno = trno + 1;
                //        }
                //    }
                //    else
                //    {
                //        trno = trno + 1;
                //    }
                //}

                //int CreateOIDcode = trno;


                //await con.ExecuteAsync(insertOutletQuery,
                //    new
                //    {
                //        OIDCode = CreateOIDcode,
                //        OltCode = Oltcode,
                //        POSCode = 1,
                //        taxcode = Taxcode,
                //        itemcode = item.ItemCode,
                //        OIDRate = item.Rate,
                //        OIDAvailable = 1,
                //        ItemDiscountRequired = false,
                //        UserCode = usercode,
                //        LastModify = currentdate,
                //        BranchCode = Branchcode,
                //        vatper = 0,
                //        IsFree = 0
                //    });

                //string insertQuery = @"
                //INSERT INTO ExtraCharges ( ChargeCode, ChargeName, ItemCode, Branch_Code, OltCode, lastmodify )
                //VALUES ( @ChargeCode, @ChargeName, @ItemCode, @BranchCode, @OltCode, @lastmodify )";

                //await con.ExecuteAsync(insertQuery, new
                //{
                //    ChargeCode = Taxcode,
                //    ChargeName = Taxname,
                //    ItemCode = item.ItemCode,
                //    BranchCode = Branchcode,
                //    OltCode = Oltcode,
                //    lastmodify = currentdate
                //});
            }
        }

        //public async Task InsertItem(ImportItemRow item, string tabcode, string deptcode, string branchcode)
        //{
        //    using var con = _factory.CreateConnection(DbNames.POS);

        //    await con.ExecuteAsync(
        //        "INSERT INTO items VALUES (@ItemCode,@ItemName,@Tab,@Branch,@Dept)",
        //        new
        //        {
        //            item.ItemCode,
        //            item.ItemName,
        //            Tab = tabcode,
        //            Branch = branchcode,
        //            Dept = deptcode
        //        });
        //}

        #endregion

        #region OutletItemDetails

        public async Task<IEnumerable<OutletItemDetails>> GetOutletItemDetailsList(string branchcode, string oltcode, bool isavaliable)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);
            IEnumerable<OutletItemDetails> result = new List<OutletItemDetails>();

            //var sql = $"Select Distinct i.itemcode, i.itemname, i.itemrate, o.oidrate, o.oidavailable, o.TaxCode, o.Vatper, Discount, " +
            //    $" FreeItemCode, baseitemQty As FreeItemName, FreeItemQty, IsFree as IsHappyHour, i.GrpCode " +
            //    $" From ItemMaster i " +
            //    $" left outer join OltItemDetails o " +
            //    $" left join ItemMaster FI On o.FreeItemCode = fi.ItemCode  on i.itemcode = o.itemcode " +
            //    $" Where i.subitem = 0 and o.oltcode = @Oltcode and i.Branch_Code = @BranchCode and o.oidavailable = @IsAvailable " +
            //    $" UNION ALL " +
            //    $" Select Distinct i.itemcode, i.itemname, i.itemrate, i.itemrate, o.oidavailable, o.TaxCode, o.Vatper, Discount, " +
            //    $" FreeItemCode, o.baseitemQty As FreeItemName, FreeItemQty, IsFree  as IsHappyHour, i.GrpCode " +
            //    $" From ItemMaster i " +
            //    $" left outer join OltItemDetails o " +
            //    $" left join ItemMaster FI On o.FreeItemCode = fi.ItemCode  on i.itemcode = o.itemcode " +
            //    $" Where i.subitem = 0 and o.oltcode <> @Oltcode and i.Branch_Code = @BranchCode  and o.oidavailable = @IsAvailable " +
            //    $" and i.ItemCode not in (Select ItemCode from OltItemDetails where OltCode= @Oltcode)" +
            //    $" order by I.itemname ";

            var countsql = @"SELECT COUNT(*) FROM OltItemDetails WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code";
            int iscount = await connection.ExecuteScalarAsync<int>(countsql, new { OltCode = oltcode, Branch_Code = branchcode });

            if (iscount > 0)
            {
                var sql = $"Select distinct i.ItemCode, i.ItemName, o.OIDRate, o.OIDAvailable, Discount, \r\n " +
                $" FreeItemCode, baseitemQty As FreeItemName, FreeItemQty, IsFree as IsHappyHour, i.GrpCode \r\n" +
                $" From ItemMaster i \r\n" +
                $" Inner join OltItemDetails o  on i.itemcode = o.itemcode AND i.Branch_Code = o.Branch_Code \r\n" +
                $" where i.branch_code = @BranchCode and OltCode = @Oltcode and o.oidavailable = @IsAvailable \r\n" +
                $" UNION ALL \r\n" +
                $" Select distinct i.ItemCode, i.ItemName, i.ItemRate as OIDRate, 0 as OIDAvailable,  Discount, \r\n" +
                $" FreeItemCode, o.baseitemQty As FreeItemName, FreeItemQty, IsFree as IsHappyHour, i.GrpCode \r\n" +
                $" From ItemMaster i \r\n" +
                $" left outer join OltItemDetails o on i.itemcode = o.itemcode \r\n" +
                $" where i.Branch_Code = @BranchCode and o.oidavailable = @IsAvailable and \r\n" +
                $" i.ItemCode not in (select ItemCode from OltItemDetails where Branch_Code = @BranchCode and OltCode = @Oltcode)";

                result = await connection.QueryAsync<OutletItemDetails>(sql, new { BranchCode = branchcode, Oltcode = oltcode, IsAvailable = isavaliable });
            }
            else
            {
                var sql = $"Select distinct i.ItemCode, i.ItemName, i.ItemRate as OIDRate, 0 as OIDAvailable, 0 as Discount, " +
                    $" 0 as FreeItemCode, 0 As FreeItemName, 0 as FreeItemQty, 0 as IsHappyHour, i.GrpCode  " +
                    $" From ItemMaster i " +
                    $" where i.branch_code = @BranchCode";

                result = await connection.QueryAsync<OutletItemDetails>(sql, new { BranchCode = branchcode, Oltcode = oltcode, IsAvailable = isavaliable });
            }
            return result;
        }

        public async Task<int> InsertTmpItemAsync(OutletItemDetails item, string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            //var sql = @"INSERT INTO TmpOltItemDetails SELECT * FROM OltItemDetails
            //        WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code AND ItemCode = @itemcode";
            //return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode, itemcode = item.ItemCode }, transaction);

            var sql = @"INSERT INTO TmpOltItemDetails 
                 ( OIDCode, POSCode, OltCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, VATAPP, VATPER, Branch_Code, Discount, FreeItemCode, FreeItemQty, PrintDep, CaptainPoint, GuestPoint, IsFree, IItemRate, IVAT, ISTax, ISBCess, IKKCess ) 
                   SELECT OIDCode, POSCode, OltCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, VATAPP, VATPER, Branch_Code, Discount, FreeItemCode, FreeItemQty, PrintDep, CaptainPoint, GuestPoint, IsFree, IItemRate, IVAT, ISTax, ISBCess, IKKCess
                    FROM OltItemDetails
                    WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code AND ItemCode = @itemcode";

            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode, itemcode = item.ItemCode }, transaction);
        }

        public async Task<int> DeleteItemAsync(int itemCode, string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = @"DELETE FROM OltItemDetails
                    WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code AND ItemCode = @ItemCode";
            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode, ItemCode = itemCode }, transaction);
        }

        public async Task<int> InsertItemAsync(OutletItemDetails item, string outletCode, string branchCode, int oidCode, DateTime currenttime, string usercode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var isDisc = item.Discount > 0 ? 1 : 0;
            var isFree = item.IsHappyHour ? 1 : 0;

            var sql = @"
            INSERT INTO OltItemDetails
            (OIDCode, POSCode, OltCode, ItemCode, OIDRate, OIDAvailable, UserCode, LastModify,
             itemdiscountrequired, Branch_Code, Discount, FreeItemCode, FreeItemQty,IsFree, IItemRate)
            VALUES
            (@oidcode, @POSCode, @OltCode, @itemCode, @OIDRate, @oidavailable, @UserCode, @LastModify,
             @IsDisc, @Branch_Code, @ItemDiscount, @freeItemCode, @freeItemQty, @IsFree, @OIDRate)";

            return await con.ExecuteAsync(sql, new
            {
                oidcode = oidCode,
                POSCode = 1,
                OltCode = outletCode,
                itemCode = item.ItemCode,
                //TaxCode = item.TaxCode,
                OIDRate = item.OIDRate,
                oidavailable = item.OIDAvailable,
                UserCode = usercode,
                LastModify = currenttime,
                IsDisc = isDisc,
                Branch_Code = branchCode,
                ItemDiscount = item.Discount,
                freeItemCode = item.FreeItemCode,
                freeItemQty = item.FreeItemQty,
                IsFree = isFree
            }, transaction);
        }

        public async Task<int> CleanupTmpAsync(string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            return await con.ExecuteAsync("DELETE FROM TmpOltItemDetails WHERE Branch_Code = @BranchCode", new { BranchCode = branchCode }, transaction);
            //return await con.ExecuteAsync( "DELETE FROM TmpOltItemDetails", null, transaction);
        }

        public async Task<int> CountExistingAsync(int itemCode, string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = @"SELECT COUNT(*) FROM OltItemDetails WHERE OltCode = @OltCode AND ItemCode = @ItemCode AND Branch_Code = @Branch_Code";
            return await con.ExecuteScalarAsync<int>(sql, new { OltCode = outletCode, ItemCode = itemCode, Branch_Code = branchCode }, transaction);
        }


        public async Task<int> InsertExtraCharge(int itemcode, string chargecode, string chargename, string outletCode, DateTime currentTime, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = $"Insert into ExtraCharges(ChargeCode, ChargeName, ItemCode, Branch_Code, OltCode, lastmodify) " +
                $" values(@ChargeCode, @ChargeName, @ItemCode, @Branch_Code, @OltCode, @lastmodify)";
            var insertresult =  await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode, ItemCode = itemcode, ChargeCode = chargecode, ChargeName = chargename, lastmodify = currentTime }, transaction);

            //const string query = @"
            //UPDATE OltItemDetails SET
            //TaxCode = @Chargecode
            //WHERE ItemCode = @ItemCode and OltCode = @OltCode and Branch_code = @Branch_code";

            //var rows = await con.ExecuteAsync(query, new { OltCode = outletCode, Branch_Code = branchCode, ItemCode = itemcode, Chargecode = chargecode }, transaction);

            return insertresult;
        }

        public async Task<int> DeleteExtraCharge(int itemCode, string chargeCode, string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = @"Delete from ExtraCharges where ItemCode = @ItemCode and chargecode = @ChargeCode and Branch_Code= @Branch_Code and OltCode= @OltCode";
            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode, ItemCode = itemCode, ChargeCode = chargeCode }, transaction);
        }

        public async Task<int> ElseDeleteItemAsync(string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = @"DELETE FROM OltItemDetails
                    WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code ";
            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode }, transaction);
        }

        public async Task<int> ElseInsertOltItemAsync(string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;

            var sql = @"INSERT INTO OltItemDetails 
                 ( OIDCode, POSCode, OltCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, VATAPP, VATPER, Branch_Code, Discount, FreeItemCode, FreeItemQty, PrintDep, CaptainPoint, GuestPoint, IsFree, IItemRate, IVAT, ISTax, ISBCess, IKKCess ) 
                   SELECT OIDCode, POSCode, OltCode, ItemCode, TaxCode, OIDRate, OIDAvailable, ItemDiscountRequired, UserCode, LastModify, VATAPP, VATPER, Branch_Code, Discount, FreeItemCode, FreeItemQty, PrintDep, CaptainPoint, GuestPoint, IsFree, IItemRate, IVAT, ISTax, ISBCess, IKKCess
                    FROM TmpOltItemDetails
                    WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code ";

            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode }, transaction);
        }

        public async Task<int> ElseDeleteTempItemAsync(string outletCode, string branchCode, IDbTransaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            var con = transaction.Connection;
            var sql = @"DELETE FROM TmpOltItemDetails
                    WHERE OltCode = @OltCode AND Branch_Code = @Branch_Code ";
            return await con.ExecuteAsync(sql, new { OltCode = outletCode, Branch_Code = branchCode }, transaction);
        }

        #endregion

        #region PropertyMaster

        public async Task<IEnumerable<PropertyMasterModel>> GetPropertyDetailsList()
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT Company_Name, Company_code, StartYear, Address1, Address2, Phone_number, Mob_number, OwnerName, " +
                " Owner_Number, Fax_number, Email_id, Tin_no, Licence_number, Branch_code, STDCODE " +
                " FROM Tbl_companyinfo ";

            var PropertyList = await connection.QueryAsync<PropertyMasterModel>(query);
            return PropertyList;
        }

        public async Task<int> CreatePropertyDetailsMaster(PropertyMasterModel propertyMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                int newId = 0;

                var checkQuery = "SELECT COUNT(1) FROM Tbl_companyinfo WHERE Company_Name = @CompanyName And Branch_code = @BranchCode";
                var userExists = await connection.QuerySingleAsync<int>(checkQuery,
                    new { CompanyName = propertyMaster.Company_Name, BranchCode = propertyMaster.Branch_code });

                if (userExists > 0)
                {
                    return newId = -1; // Indicate that the company already exists
                }

                var query = @"
                INSERT INTO Tbl_companyinfo (Company_Name, Company_code, StartYear, Address1, Address2, Phone_number, Mob_number, OwnerName,
                Owner_Number, Fax_number, Email_id, Tin_no, Licence_number, Branch_code, STDCODE) 
                VALUES (@Company_Name, @Company_code, @StartYear, @Address1, @Address2, @Phone_number, @Mob_number, @OwnerName,
                @Owner_Number, @Fax_number, @Email_id, @Tin_no, @Licence_number, @Branch_code, @STDCODE);";

                await connection.ExecuteAsync(query, propertyMaster);

                newId = propertyMaster.Company_code;
                return newId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePropertyDetailsMaster(PropertyMasterModel propertyMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                var query = @"
                UPDATE Tbl_companyinfo SET 
                Company_Name = COALESCE(@Company_Name, Company_Name),
                StartYear = COALESCE(@StartYear, StartYear),
                Address1 = COALESCE(@Address1, Address1),
                Address2 = COALESCE(@Address2, Address2),
                Phone_number = COALESCE(@Phone_number, Phone_number),
                Mob_number = COALESCE(@Mob_number, Mob_number),
                OwnerName = COALESCE(@OwnerName, OwnerName),
                Owner_Number = COALESCE(@Owner_Number, Owner_Number),
                Fax_number = COALESCE(@Fax_number, Fax_number),
                Email_id = COALESCE(@Email_id, Email_id),
                Tin_no = COALESCE(@Tin_no, Tin_no),
                Licence_number = COALESCE(@Licence_number, Licence_number),
                Branch_code = COALESCE(@Branch_code, Branch_code),
                STDCODE = COALESCE(@STDCODE, STDCODE)
                WHERE Company_code = @Company_code";

                var affectedRows = await connection.ExecuteAsync(query, propertyMaster);
                return affectedRows > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeletePropertyDetailsMaster(int id)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "DELETE FROM Tbl_companyinfo WHERE Company_code = @Id";

            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            return affectedRows > 0;
        }

        #endregion

        #region BranchMaster

        public async Task<IEnumerable<BranchMasterModel>> GetBranchDetailsList(int propertyid)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT BrId, Company_code, Branch_code, Branch_name, Address1, Address2, Phone_number, Mob_number, Fax_number, Email_id, Tin_no, Licence_number " +
                " FROM Tbl_branch WHERE Company_code = @CompanyCode ";

            var BranchList = await connection.QueryAsync<BranchMasterModel>(query, new { CompanyCode = propertyid });
            return BranchList;
        }

        public async Task<int> CreateBranchDetailsMaster(BranchMasterModel branchMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                // Check whether Branch_code already exists for another branch
                var duplicateQuery = @" SELECT COUNT(1)  FROM Tbl_branch WHERE Branch_code = @Branch_code";

                var duplicateCount = await connection.ExecuteScalarAsync<int>( duplicateQuery, new { branchMaster.Branch_code});

                if (duplicateCount > 0)
                {
                    return -1; // Indicate that the branch code already exists
                }

                var query = @"
                INSERT INTO Tbl_branch (Company_code, Branch_code, Branch_name, Address1, Address2, Phone_number, Mob_number, Fax_number, Email_id, Tin_no, Licence_number ) 
                VALUES (@Company_code, @Branch_code, @Branch_name, @Address1, @Address2, @Phone_number, @Mob_number, @Fax_number, @Email_id, @Tin_no, @Licence_number) SELECT CAST(SCOPE_IDENTITY() AS INT); ";

            var id = await connection.ExecuteScalarAsync<int>(query, branchMaster);
                return id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateBranchDetailsMaster(BranchMasterModel branchMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                var query = @"
                UPDATE Tbl_branch SET 
                Company_code = COALESCE(@Company_code, Company_code),
                Branch_name = COALESCE(@Branch_name, Branch_name),
                Address1 = COALESCE(@Address1, Address1),
                Address2 = COALESCE(@Address2, Address2),
                Phone_number = COALESCE(@Phone_number, Phone_number),
                Mob_number = COALESCE(@Mob_number, Mob_number),
                Fax_number = COALESCE(@Fax_number, Fax_number),
                Email_id = COALESCE(@Email_id, Email_id),
                Tin_no = COALESCE(@Tin_no, Tin_no),
                Licence_number = COALESCE(@Licence_number, Licence_number)
                WHERE BrId = @BrId AND Branch_code = @Branch_code";

                var affectedRows = await connection.ExecuteAsync(query, branchMaster);
                return affectedRows > 0;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> DeleteBranchDetailsMaster(int id)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "DELETE FROM Tbl_branch WHERE BrId = @BrId";

            var affectedRows = await connection.ExecuteAsync(query, new { BrId = id});
            return affectedRows > 0;
        }

        #endregion

        #region AddOnItems

        public async Task<IEnumerable<AddOnItemModel>> GetItemWiseAddOnDetailsList(string branchcode, int itemcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT ItemCode, AddOnItemCode, AddOnName, ItemRate, IsActive, UserCode, BranchCode, thumb" +
                " FROM AddOnItemMaster WHERE BranchCode = @BranchCode AND ItemCode = @ItemCode ";

            var List = await connection.QueryAsync<AddOnItemModel>(query, new { BranchCode = branchcode, ItemCode = itemcode });
            return List;
        }

        public async Task<IEnumerable<AdditionalOnItemModel>> GetAdditionalAddonDetailsList(int Itemcode, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT a.ItemCode, a.AddOnItemCode, a.AddOnName, a.ItemRate, a.IsActive, " +
                " CAST(a.UserCode AS VARCHAR(50)) AS UserCode, a.BranchCode, a.thumb, 1 AS SortOrder " +
                " FROM AddOnItemMaster a " +
                " WHERE a.BranchCode = @BranchCode AND a.ItemCode = @itemcode " +
                " UNION ALL " +
                " SELECT im.ItemCode AS ItemCode, 0 AS AddOnItemCode, im.ItemName AS AddOnName, im.ItemRate," +
                " 0 AS IsActive, im.UserCode, im.Branch_Code AS BranchCode, im.thumb, 2 AS SortOrder " +
                " FROM ItemMaster im " +
                " WHERE im.Branch_Code = @BranchCode AND im.ItemCode<> @itemcode AND NOT EXISTS " +
                " ( SELECT 1 FROM AddOnItemMaster a WHERE a.BranchCode = @BranchCode AND a.ItemCode = @itemcode AND a.AddOnItemCode = im.ItemCode ) " +
                " ORDER BY SortOrder, AddOnItemCode, ItemCode ASC; ";

            //var query = "SELECT im.ItemCode, im.ItemName AS AddOnName, im.ItemRate, " +
            //    " 0 AS IsActive, im.UserCode, NULL AS CreatedDate, im.Branch_Code AS BranchCode " +
            //    " FROM ItemMaster im " +
            //    " WHERE im.Branch_Code = @BranchCode AND NOT EXISTS ( SELECT 1 FROM AddOnItemMaster ai WHERE ai.AddOnItemCode = im.ItemCode ) " +
            //    " ORDER BY im.ItemCode; ";

            var AddOnList = await connection.QueryAsync<AdditionalOnItemModel>(query, new { itemcode = Itemcode, BranchCode = branchcode });
            return AddOnList;
        }

        public async Task<bool> InsertorUpdateAddOnDetails(List<AddOnItemModel> details)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int result = 0;
                // =====================================
                // 1. DELETE OLD Addon
                // =====================================

                var deleteQuery = @" DELETE FROM AddOnItemMaster WHERE ItemCode = @itemcode and UserCode = @usercode AND BranchCode = @branchcode";

                result = await connection.ExecuteAsync(deleteQuery, new { itemcode = details.FirstOrDefault().ItemCode, usercode = details.FirstOrDefault().UserCode, branchcode = details.FirstOrDefault().BranchCode }, transaction);

                // =====================================
                // 2. INSERT NEW Addon
                // =====================================

                var insertPermissionQuery = @" INSERT INTO AddOnItemMaster
                        ( ItemCode, AddOnItemCode, AddOnName, ItemRate, IsActive, UserCode, BranchCode, thumb )
                        Values
                        ( @itemcode, @addonitemcode, @addonname, @itemrate, @isactive, @usercode, @branchcode, @Thumb )";

                foreach (var dts in details)
                {
                    result = await connection.ExecuteAsync(insertPermissionQuery, new { itemcode = dts.ItemCode, addonitemcode = dts.AddOnItemCode, addonname = dts.AddOnName, itemrate = dts.ItemRate, isactive = dts.IsActive, usercode = dts.UserCode, branchcode = dts.BranchCode, Thumb = dts.thumb }, transaction);

                }
                // =====================================
                // COMMIT
                // =====================================

                transaction.Commit();
                return result > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        #endregion

        #region Common Methods

        public async Task<int> Findnextnumber(string table_name, string column_name, string condition_name, string branch)
        {
            string str;
            int trno = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                if (table_name == "ITEMMASTER")
                {
                    str = $@"select (MAX({column_name}), 0) as trno from {table_name} WITH (HOLDLOCK, ROWLOCK) where SubItem = 0 And {condition_name} = @Branch";
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

        public async Task<int> RecordsExist(string tablename, string columnname, long parameter, string BranchCode)
        {
            string str;
            int exist = 0;
            try
            {
                using var connection = _factory.CreateConnection(DbNames.POS);

                str = $@"select {columnname} from {tablename} where {columnname} = @Parameter and branch_code = @Branch ";

                var ds = await connection.QueryFirstOrDefaultAsync<dynamic>(str, new { Parameter = parameter, Branch = BranchCode });

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

        #endregion
    }
}
