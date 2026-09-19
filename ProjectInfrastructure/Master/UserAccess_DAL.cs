using Dapper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using HMS_360_PMS.HMS_360_PMS.Infrastructure;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HMS_360_PMS.ProjectInfrastructure.Master
{
    public class UserAccess_DAL : IUserAccess_Repository
    {
        private readonly DbConnectionFactory _factory;

        public UserAccess_DAL(DbConnectionFactory factory)
        {
            _factory = factory;
        }

        #region RoleMaster

        public async Task<IEnumerable<RoleMasterModel>> GetRoleMasterList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT RoleId, RoleName, Description, Status, BranchCode FROM RoleMaster WHERE BranchCode = @BranchCode";

            var roleList = await connection.QueryAsync<RoleMasterModel>(query, new { BranchCode = branchcode });
            return roleList;
        }

        #endregion

        #region UserAccessMaster

        public async Task<IEnumerable<UserAccessMaster>> GetUserDetailsList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT UserCode, UserName, UserPassword, UserPrivilege, EnteredBy, LastModify, Branch_code, CAST(DisPercent AS NUMERIC(10,2)) AS DisPercent, CAST(DisAmount AS NUMERIC(10,2)) AS DisAmount, RoleId FROM UserMaster WHERE Branch_Code = @BranchCode";

            var userAccessList = await connection.QueryAsync<UserAccessMaster>(query, new { BranchCode = branchcode });
            return userAccessList;
        }

        public async Task<int> CreateUserDetailsMaster(UserAccessMaster userAccessMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int newId = 0;

                var checkQuery = "SELECT COUNT(1) FROM UserMaster WHERE UserName = @UserName And RoleId = @RoleId And Branch_code = @BranchCode";
                var userExists = await connection.QuerySingleAsync<int>(checkQuery,
                    new { userAccessMaster.UserName, userAccessMaster.RoleId, BranchCode = userAccessMaster.Branch_code }, transaction);

                if (userExists > 0)
                {
                    transaction.Rollback();
                    return newId = -1; // Indicate that the username already exists
                }

                var query = @"
                INSERT INTO UserMaster (UserCode, UserName, UserPassword, UserPrivilege, EnteredBy, LastModify, Branch_code, DisPercent, DisAmount, RoleId) 
                VALUES (@UserCode, @UserName, @UserPassword, @UserPrivilege, @EnteredBy, @LastModify, @Branch_code, @DisPercent, @DisAmount, @RoleId);";

                await connection.ExecuteAsync(query, userAccessMaster, transaction);

                // =========================================
                // COPY DEFAULT ROLE PERMISSIONS
                // =========================================

                var insertPermissionQuery = @" INSERT INTO UserPermissionMaster
                    ( UserCode, UserName, RoleId, RoleName, MainMenuId, MenuName, MenuPermission, SubMenuId, SubMenuName, SubMenuPermission, IsPermission, BranchCode )
                    SELECT
                    @UserCode, @UserName, RPM.RoleId, RPM.RoleName, RPM.MainMenuId,  RPM.MenuName, RPM.MenuPermission, RPM.SubMenuId, RPM.SubMenuName,RPM.SubMenuPermission, RPM.IsPermission, @BranchCode
                    FROM RolePermissionMaster RPM WHERE RPM.RoleId = @RoleId";

                await connection.ExecuteAsync(insertPermissionQuery, new { userAccessMaster.UserCode, userAccessMaster.UserName, userAccessMaster.RoleId, BranchCode = userAccessMaster.Branch_code }, transaction);

                //var menuquery = @"INSERT INTO MainMenuMaster ( MenuName, MenuPermission, BranchCode )
                //    SELECT M.MenuName, M.MenuPermission, @NewBranch FROM MainMenuMaster M WHERE M.BranchCode = @SourceBranch
                //    AND NOT EXISTS ( SELECT 1 FROM MainMenuMaster X WHERE X.MenuName = M.MenuName AND X.BranchCode = @NewBranch );

                //    INSERT INTO SubMenuMaster ( SubMenuName, SubMenuPermission, MainMenuId, BranchCode )
                //    SELECT S.SubMenuName, S.SubMenuPermission, S.MainMenuId, @NewBranch FROM SubMenuMaster S
                //    WHERE S.BranchCode = @SourceBranch
                //    AND NOT EXISTS ( SELECT 1 FROM SubMenuMaster X WHERE X.SubMenuName = S.SubMenuName AND X.MainMenuId = S.MainMenuId AND X.BranchCode = @NewBranch );";

                //await connection.ExecuteAsync(menuquery, new { SourceBranch = "DEROY", NewBranch = userAccessMaster.Branch_code }, transaction);

               
                var menuquery =  "INSERT INTO MainMenuMaster " +
                    " ( MenuName, MenuPermission, BranchCode ) \r\n SELECT DISTINCT MenuName, MenuPermission, @NewBranch FROM MainMenuMaster M " +
                    " WHERE NOT EXISTS ( SELECT 1 FROM MainMenuMaster X WHERE X.MenuName = M.MenuName AND X.BranchCode = @NewBranch ); \r\n" +
                    " INSERT INTO SubMenuMaster ( SubMenuName, SubMenuPermission, MainMenuId, BranchCode ) \r\n" +
                    " SELECT DISTINCT SubMenuName, SubMenuPermission, MainMenuId, @NewBranch FROM SubMenuMaster S" +
                    " WHERE NOT EXISTS ( SELECT 1 FROM SubMenuMaster X WHERE X.SubMenuName = S.SubMenuName AND X.MainMenuId = S.MainMenuId AND X.BranchCode = @NewBranch ); ";

                await connection.ExecuteAsync(menuquery, new { NewBranch = userAccessMaster.Branch_code }, transaction);

                transaction.Commit();
                newId = userAccessMaster.UserCode;
                return newId;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateUserDetailsMaster(UserAccessMaster userAccessMaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // =====================================
                // 1. GET EXISTING ROLE ID
                // =====================================

                int? oldRoleId = await connection.QuerySingleOrDefaultAsync<int?>(
                    @"SELECT ISNULL(RoleId, 0) FROM UserMaster WHERE UserCode = @UserCode AND Branch_code = @BranchCode",
                    new { UserCode = userAccessMaster.UserCode, BranchCode = userAccessMaster.Branch_code },
                    transaction);

                bool roleChanged = oldRoleId != userAccessMaster.RoleId;
                //bool roleChanged = oldRoleId.HasValue &&  oldRoleId.Value != userAccessMaster.RoleId;

                var query = @"
                UPDATE UserMaster SET 
                UserCode = COALESCE(@UserCode, UserCode),
                UserName = COALESCE(@UserName, UserName),
                UserPassword = COALESCE(@UserPassword, UserPassword),
                UserPrivilege = COALESCE(@UserPrivilege, UserPrivilege),
                EnteredBy = COALESCE(@EnteredBy, EnteredBy),
                LastModify = COALESCE(@LastModify, LastModify),
                Branch_code = COALESCE(@Branch_code, Branch_code),
                DisPercent = COALESCE(@DisPercent, DisPercent),
                DisAmount = COALESCE(@DisAmount, DisAmount),
                RoleId = COALESCE(@RoleId, RoleId)
                WHERE UserCode = @UserCode";

                var affectedRows = await connection.ExecuteAsync(query, userAccessMaster, transaction);

                if (roleChanged)
                {
                    // =====================================
                    // 2. DELETE OLD PERMISSIONS
                    // =====================================

                    var deleteQuery = @" DELETE FROM UserPermissionMaster WHERE UserCode = @UserCode AND BranchCode = @BranchCode";

                    await connection.ExecuteAsync(deleteQuery, new { UserCode = userAccessMaster.UserCode, BranchCode = userAccessMaster.Branch_code }, transaction);

                    // =====================================
                    // 3. INSERT NEW ROLE PERMISSIONS
                    // =====================================

                    var insertPermissionQuery = @" INSERT INTO UserPermissionMaster
                        ( UserCode, UserName, RoleId, RoleName, MainMenuId, MenuName, MenuPermission, SubMenuId, SubMenuName, IsPermission, BranchCode )
                        SELECT
                        @usercode, @username, RPM.RoleId, RPM.RoleName, RPM.MainMenuId, RPM.MenuName, RPM.MenuPermission, RPM.SubMenuId, RPM.SubMenuName, RPM.IsPermission, @BranchCode
                        FROM RolePermissionMaster RPM WHERE RPM.RoleId = @roleid";

                    await connection.ExecuteAsync(insertPermissionQuery, new { usercode = userAccessMaster.UserCode, username = userAccessMaster.UserName, roleid = userAccessMaster.RoleId, BranchCode = userAccessMaster.Branch_code }, transaction);
                }

                // =====================================
                // COMMIT
                // =====================================

                transaction.Commit();
                return true;
            }
            catch(Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteUserDetailsMaster(int id, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var deletePermissionQuery = @" DELETE FROM UserPermissionMaster WHERE UserCode = @Id AND BranchCode = @BranchCode";

            await connection.ExecuteAsync( deletePermissionQuery, new { Id = id, BranchCode = branchcode });

            //var MenuQuery = @" DELETE FROM MainMenuMaster WHERE BranchCode = @BranchCode";

            //await connection.ExecuteAsync(deletePermissionQuery, new { BranchCode = branchcode });

            //var SubMenuQuery = @" DELETE FROM SubMenuMaster WHERE BranchCode = @BranchCode";

            //await connection.ExecuteAsync(SubMenuQuery, new { BranchCode = branchcode });

            var query = "DELETE FROM UserMaster WHERE UserCode = @Id AND Branch_code = @BranchCode";

            var affectedRows = await connection.ExecuteAsync(query, new { Id = id, BranchCode = branchcode });
            return affectedRows > 0;
        }

        public async Task<SecoundUserAccessMaster> GetSecoundUserAccessMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT SecoundUserId, SecondUserPassword, BranchCode FROM Tbl_SecondUserAccountPassword Where BranchCode = @BranchCode";

            var userAccessList = await connection.QueryFirstOrDefaultAsync<SecoundUserAccessMaster>(query, new { BranchCode = branchcode });
            return userAccessList;
        }

        public async Task<bool> UpdateSecoundUserAccessDetail(SecoundUserAccessMaster seconduseraccessamaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                var query = @"
                UPDATE Tbl_SecondUserAccountPassword SET 
                SecondUserPassword = COALESCE(@SecondUserPassword, SecondUserPassword)
                WHERE SecoundUserId = @SecoundUserId And BranchCode = @BranchCode";

                var affectedRows = await connection.ExecuteAsync(query, seconduseraccessamaster);

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<AdminAccessMaster> GetAdminAccessMaster(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT SecoundUserId, AdminPassword, BranchCode FROM Tbl_SecondUserAccountPassword Where BranchCode = @BranchCode";

            var AdminAccessList = await connection.QueryFirstOrDefaultAsync<AdminAccessMaster>(query, new { BranchCode = branchcode });
            return AdminAccessList;
        }

        public async Task<bool> UpdateAdminAccessDetail(AdminAccessMaster adminaccessamaster)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            try
            {
                var query = @"
                UPDATE Tbl_SecondUserAccountPassword SET 
                AdminPassword = COALESCE(@AdminPassword, AdminPassword)
                WHERE SecoundUserId = @SecoundUserId And BranchCode = @BranchCode";

                var affectedRows = await connection.ExecuteAsync(query, adminaccessamaster);

                return affectedRows > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region UserPermissionAccessMaster

        public async Task<IEnumerable<MainMenuModel>> GetMainMenuList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "Select MainMenuId, MenuName, MenuPermission, BranchCode From MainMenuMaster WHERE BranchCode = @BranchCode";
            var MenuList = await connection.QueryAsync<MainMenuModel>(query, new { BranchCode = branchcode});
            return MenuList;
        }

        public async Task<IEnumerable<SubMenuModel>> GetSubMenuList(string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = "SELECT SubMenuId, SubMenuName, SubMenuPermission, MainMenuId, BranchCode FROM SubMenuMaster WHERE BranchCode = @BranchCode";
            var MenuList = await connection.QueryAsync<SubMenuModel>(query, new { BranchCode = branchcode});
            return MenuList;
        }

        public async Task<int> InsertMainMenu(MainMenuModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // INSERT MAIN MENU

                string insertQuery = @"
                INSERT INTO MainMenuMaster ( MenuName, MenuPermission, BranchCode )
                VALUES
                ( @MenuName, @MenuPermission, @BranchCode )";

                await connection.ExecuteAsync(  insertQuery,  model, transaction );

                // SYNC ROLE PERMISSIONS

                await connection.ExecuteAsync( "SP_SyncRolePermissions", transaction: transaction, commandType: CommandType.StoredProcedure );

                transaction.Commit();

                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> InsertSubMenu(SubMenuModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // INSERT SUBMENU

                string query = @"
                INSERT INTO SubMenuMaster ( MainMenuId, SubMenuName, SubMenuPermission, BranchCode )
                VALUES
                ( @MainMenuId, @SubMenuName, @SubMenuPermission, @BranchCode )";
                
                await connection.ExecuteAsync( query, model, transaction );

                // SYNC ROLE PERMISSIONS

                await connection.ExecuteAsync( "SP_SyncRolePermissions", transaction: transaction, commandType: CommandType.StoredProcedure );

                transaction.Commit();

                return 1;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> SaveMenuWithSubMenu(MenuWithSubMenuModel model)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                int mainMenuId = model.MainMenuId;
                int subMenuId = 0;

                //------------------------------------------------
                // INSERT MAIN MENU IF NEW
                //------------------------------------------------

                if (!model.IsExistingMainMenu)
                {
                    string mainMenuQuery = @"
                    INSERT INTO MainMenuMaster ( MenuName, MenuPermission, BranchCode )
                    VALUES
                    ( @MenuName, @MenuPermission, @BranchCode )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    mainMenuId = await connection.ExecuteScalarAsync<int>( mainMenuQuery, model, transaction );
                }

                //------------------------------------------------
                // INSERT SUBMENU
                //------------------------------------------------
                if (model.SubMenuName != null)
                {
                    string subMenuQuery = @"
                    INSERT INTO SubMenuMaster ( MainMenuId, SubMenuName, SubMenuPermission, BranchCode )
                    VALUES
                    ( @MainMenuId, @SubMenuName, @SubMenuPermission, @BranchCode )

                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    subMenuId = await connection.ExecuteScalarAsync<int>(subMenuQuery,
                        new
                        {
                            MainMenuId = mainMenuId,
                            model.SubMenuName,
                            model.SubMenuPermission,
                            model.BranchCode
                        },
                        transaction);
                }

                //------------------------------------------------
                // SYNC ROLE PERMISSIONS
                //------------------------------------------------

                await connection.ExecuteAsync( "SP_SyncRolePermissions", transaction: transaction, commandType: CommandType.StoredProcedure );

                transaction.Commit();

                return (mainMenuId + subMenuId);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteMainMenuDetail(int MainMenuId, string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
                DELETE FROM UserPermissionMaster
                WHERE MainMenuId = @mainmenuid AND BranchCode = @branchcode;

                DELETE FROM RolePermissionMaster
                WHERE MainMenuId = @mainmenuid AND BranchCode = @branchcode;

                DELETE FROM SubMenuMaster
                WHERE MainMenuId = @mainmenuid AND BranchCode = @branchcode;

                DELETE FROM MainMenuMaster
                WHERE MainMenuId = @mainmenuid AND BranchCode = @branchcode;";

            var affectedRows = await connection.ExecuteAsync(sql, new { mainmenuid = MainMenuId, branchcode = Branchcode });
            return affectedRows > 0;
        }

        public async Task<bool> DeleteSubMenuDetail(int SubMenuId, string Branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var sql = @"
                DELETE FROM UserPermissionMaster
                WHERE SubMenuId = @submenuid AND BranchCode = @branchcode;

                DELETE FROM RolePermissionMaster
                WHERE SubMenuId = @submenuid AND BranchCode = @branchcode;

                DELETE FROM SubMenuMaster
                WHERE SubMenuId = @submenuid AND BranchCode = @branchcode;";

            var affectedRows = await connection.ExecuteAsync(sql, new { submenuid = SubMenuId, branchcode = Branchcode });
            return affectedRows > 0;
        }

        //public async Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, int usercode)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    var query = "SELECT UserCode, UserName, RoleId, RoleName, MainMenuId, MenuName, MenuPermission, SubMenuId, SubMenuName, IsPermission, BranchCode FROM UserPermissionMaster WHERE BranchCode = @BranchCode AND UserCode = @UserCode";
        //    var userAccessList = await connection.QueryAsync<UserPermissionAccessMaster>(query, new { BranchCode = branchcode, UserCode = usercode });
        //    return userAccessList;
        //}

        public async Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, int usercode, int roleId)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"

                --------------------------------------------------------
                -- EXISTING USER PERMISSIONS
                --------------------------------------------------------

                SELECT UP.UserCode, UP.UserName, UP.RoleId, UP.RoleName, UP.MainMenuId, UP.MenuName, UP.MenuPermission, UP.SubMenuId, UP.SubMenuName, UP.SubMenuPermission, UP.IsPermission, UP.BranchCode
                FROM UserPermissionMaster UP
                WHERE UP.BranchCode = @BranchCode AND UP.UserCode = @UserCode

                UNION
                --------------------------------------------------------
                -- NEW ROLE PERMISSIONS NOT IN USER TABLE
                --------------------------------------------------------

                SELECT @UserCode AS UserCode, '' AS UserName,  RP.RoleId, RP.RoleName,  RP.MainMenuId, RP.MenuName, RP.MenuPermission,  RP.SubMenuId, RP.SubMenuName, RP.SubMenuPermission,  RP.IsPermission,  RP.BranchCode
                FROM RolePermissionMaster RP
                WHERE RP.BranchCode = @BranchCode AND RP.RoleId = @RoleId 
                
                AND NOT EXISTS
                (
                SELECT 1 FROM UserPermissionMaster UP WHERE UP.UserCode = @UserCode AND UP.MainMenuId = RP.MainMenuId AND UP.SubMenuId = RP.SubMenuId
                ) ORDER BY MainMenuId, SubMenuId ";

                var result = await connection.QueryAsync<UserPermissionAccessMaster>( query, new { BranchCode = branchcode, UserCode = usercode, RoleId = roleId });

            return result;
        }


        //public async Task<int> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission)
        //{
        //    using var connection = _factory.CreateConnection(DbNames.POS);

        //    var query = @"
        //        INSERT INTO UserPermissionMaster (UserCode, UserName, RoleId, RoleName, MainMenuId, MenuName, MenuPermission, SubMenuId, SubMenuName, IsPermission, BranchCode) 
        //        VALUES (@UserCode, @UserName, @RoleId, @RoleName, @MainMenuId, @MenuName, @MenuPermission, @SubMenuId, @SubMenuName, @IsPermission, @BranchCode);";

        //    int totalInserted = 0;
        //    foreach (var permission in userPermission)
        //    {
        //        int rowsAffected = await connection.ExecuteAsync(query, permission);
        //        totalInserted += rowsAffected;
        //    }

        //    return totalInserted;
        //}

        public async Task<int> CreateUserPermissionAccessMaster(  List<UserPermissionAccessMaster> userPermission)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var query = @" 
                INSERT INTO UserPermissionMaster 
                ( UserCode, UserName, RoleId, RoleName, MainMenuId, MenuName, MenuPermission, SubMenuId, SubMenuName, SubMenuPermission, IsPermission, BranchCode )
                VALUES
                ( @UserCode, @UserName, @RoleId, @RoleName, @MainMenuId, @MenuName, @MenuPermission, @SubMenuId, @SubMenuName, @SubMenuPermission, @IsPermission, @BranchCode );";

                //------------------------------------------------
                // DAPPER BULK EXECUTION
                //------------------------------------------------

                int rowsAffected = await connection.ExecuteAsync( query,  userPermission,  transaction );

                transaction.Commit();

                return rowsAffected;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<int> SaveUserPermissions( List<UserPermissionAccessMaster> permissions)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                string query = @"

        MERGE UserPermissionMaster AS TARGET

        USING
        (
            SELECT
                @UserCode AS UserCode,
                @UserName AS UserName,
                @RoleId AS RoleId,
                @RoleName AS RoleName,
                @MainMenuId AS MainMenuId,
                @MenuName AS MenuName,
                @MenuPermission AS MenuPermission,
                @SubMenuId AS SubMenuId,
                @SubMenuName AS SubMenuName,
                @SubMenuPermission AS SubMenuPermission,
                @IsPermission AS IsPermission,
                @BranchCode AS BranchCode
        ) AS SOURCE

        ON TARGET.UserCode = SOURCE.UserCode
        AND TARGET.MainMenuId = SOURCE.MainMenuId
        AND TARGET.SubMenuId = SOURCE.SubMenuId

        WHEN MATCHED THEN
            UPDATE SET
                TARGET.IsPermission = SOURCE.IsPermission,
                TARGET.MenuPermission = SOURCE.MenuPermission,
                TARGET.SubMenuPermission = SOURCE.SubMenuPermission

        WHEN NOT MATCHED THEN

            INSERT
            (
                UserCode,
                UserName,
                RoleId,
                RoleName,
                MainMenuId,
                MenuName,
                MenuPermission,
                SubMenuId,
                SubMenuName,
                SubMenuPermission,
                IsPermission,
                BranchCode
            )
            VALUES
            (
                SOURCE.UserCode,
                SOURCE.UserName,
                SOURCE.RoleId,
                SOURCE.RoleName,
                SOURCE.MainMenuId,
                SOURCE.MenuName,
                SOURCE.MenuPermission,
                SOURCE.SubMenuId,
                SOURCE.SubMenuName,
                SOURCE.SubMenuPermission,
                SOURCE.IsPermission,
                SOURCE.BranchCode
            );";

                int affectedRows = await connection.ExecuteAsync(
                    query,
                    permissions,
                    transaction
                );

                transaction.Commit();

                return affectedRows;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> DeleteUserpermissionDetails(int UserCode, int roleid, string branchcode)
        {
            using var connection = _factory.CreateConnection(DbNames.POS);

            var query = @"DELETE FROM UserPermissionMaster 
                 WHERE UserCode = @Ids AND RoleId = @RoleId AND BranchCode = @BranchCode";

            var affectedRows = await connection.ExecuteAsync(query, new { Ids = UserCode, RoleId = roleid, BranchCode = branchcode });
            return affectedRows > 0;
        }


        #endregion
    }
}
