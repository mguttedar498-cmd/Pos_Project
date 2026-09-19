using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Interfaces
{
    public interface IUserAccess_Repository
    {
        #region RoleMaster

        Task<IEnumerable<RoleMasterModel>> GetRoleMasterList(string branchcode);

        #endregion

        #region UserAccessMaster

        Task<IEnumerable<UserAccessMaster>> GetUserDetailsList(string branchcode);

        Task<int> CreateUserDetailsMaster(UserAccessMaster userAccessMaster);

        Task<bool> UpdateUserDetailsMaster(UserAccessMaster userAccessMaster);

        Task<bool> DeleteUserDetailsMaster(int id, string branchcode);

        Task<SecoundUserAccessMaster> GetSecoundUserAccessMaster(string branchcode);

        Task<bool> UpdateSecoundUserAccessDetail(SecoundUserAccessMaster seconduseraccessamaster);

        Task<AdminAccessMaster> GetAdminAccessMaster(string branchcode);

        Task<bool> UpdateAdminAccessDetail(AdminAccessMaster adminaccessamaster);

        #endregion

        #region UserPermissionAccessMaster


        Task<IEnumerable<MainMenuModel>> GetMainMenuList(string branchcode);

        Task<IEnumerable<SubMenuModel>> GetSubMenuList(string branchcode);

        Task<int> InsertMainMenu(MainMenuModel model);

        Task<int> InsertSubMenu(SubMenuModel model);

        Task<int> SaveMenuWithSubMenu(MenuWithSubMenuModel model);

        Task<bool> DeleteMainMenuDetail(int MainMenuId, string branchcode);

        Task<bool> DeleteSubMenuDetail(int SubMenuId, string branchcode);

        Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, int usercode, int roleId);

        Task <int> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission);
        Task<int> SaveUserPermissions(List<UserPermissionAccessMaster> permissions);

        Task<bool> DeleteUserpermissionDetails(int UserCode, int RoleId, string branchcode);

        #endregion

    }
}
