using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using Microsoft.AspNetCore.Mvc;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Interfaces
{
    public interface IUserAccess_Service
    {

        #region RoleMaster

        Task<IEnumerable<RoleMasterModel>> GetRoleMasterList(string branchcode);

        #endregion

        #region UserAccessMaster

        Task<IEnumerable<UserAccessMaster>> GetUserDetailsList(string branchcode);

         Task<ServiceResult> CreateUserDetailsMaster(UserAccessMaster userAccessMaster);

         Task<ServiceResult> UpdateUserDetailsMaster(UserAccessMaster userAccessMaster);

         Task<ServiceResult> DeleteUserDetailsMaster(int id, string branchcode);

        Task<SecoundUserAccessMaster> GetSecoundUserAccessMaster(string branchcode);

        Task<ServiceResult> UpdateSecoundUserAccessDetail(SecoundUserAccessMaster seconduseraccessamaster);

        Task<AdminAccessMaster> GetAdminAccessMaster(string branchcode);

        Task<ServiceResult> UpdateAdminAccessDetail(AdminAccessMaster adminaccessamaster);


        #endregion

        #region UserPermissionAccessMaster

        Task<IEnumerable<MainMenuModel>> GetMainMenuList(string branchcode);

        Task<IEnumerable<MainMenuGroupModel>> GetSubMenuList(string branchcode);

        Task<ServiceResult> InsertMainMenu(MainMenuModel model);

        Task<ServiceResult> InsertSubMenu(SubMenuModel model);

        Task<ServiceResult> SaveMenuWithSubMenu(MenuWithSubMenuModel model);

        Task<ServiceResult> DeleteMainMenuDetail(int MainMenuId, string branchcode);

        Task<ServiceResult> DeleteSubMenuDetail(int SubMenuId, string branchcode);

        Task<UserPermissionResponse> GetUserPermissionAccessList(string branchcode, int usercode, int roleId);

        Task<ServiceResult> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission);

        //Task<ServiceResult> DeleteUserPermissionAccessMaster(int UserCode, int RoleId, string branchcode);

        #endregion
    }
}
