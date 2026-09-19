using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;

namespace HMS_360_PMS.ProjectServiceLayer.Master.Services
{
    public class UserAccess_Service : IUserAccess_Service
    {
        public readonly IUserAccess_Repository _userAccessRepository;

        public UserAccess_Service(IUserAccess_Repository userAccessRepository)
        {
            _userAccessRepository = userAccessRepository;
        }

        #region RoleMaster

        public async Task<IEnumerable<RoleMasterModel>> GetRoleMasterList(string branchcode)
        {
            var roleList = await _userAccessRepository.GetRoleMasterList(branchcode);
            return roleList;
        }

        #endregion

        #region UserAccessMaster

        public async Task<IEnumerable<UserAccessMaster>> GetUserDetailsList(string branchcode)
        {
            var userAccessList = await _userAccessRepository.GetUserDetailsList(branchcode);
            
            //var FilteredUserAccessList = userAccessList.Where(u => u.UserName.ToLower() != "admin" && u.UserName.ToLower() != "cogwave").ToList(); // Exclude Admin And Cogwave

            return userAccessList;
        }

        public async Task<ServiceResult> CreateUserDetailsMaster(UserAccessMaster userAccessMaster)
        {

            var result = await _userAccessRepository.CreateUserDetailsMaster(userAccessMaster);

            if (result > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "User Detail inserted successfully",
                    Data = result
                };
            }
            else if (result == -1)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Username already exists. Please don't use Same Username.",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert User Detail",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> UpdateUserDetailsMaster(UserAccessMaster userAccessMaster)
        {
            var UpdateUser = await _userAccessRepository.UpdateUserDetailsMaster(userAccessMaster);
            if (UpdateUser)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "User Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "User Detail not found",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> DeleteUserDetailsMaster(int id, string branchcode)
        {
            var success = await _userAccessRepository.DeleteUserDetailsMaster(id, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "User Detail not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "User Detail deleted successfully",
                Data = null
            };
        }

        public async Task<SecoundUserAccessMaster> GetSecoundUserAccessMaster(string branchcode)
        {
            var SeconduserAccessList = await _userAccessRepository.GetSecoundUserAccessMaster(branchcode);

            return SeconduserAccessList;
        }

        public async Task<ServiceResult> UpdateSecoundUserAccessDetail(SecoundUserAccessMaster seconduseraccessamaster)
        {
            var UpdateUser = await _userAccessRepository.UpdateSecoundUserAccessDetail(seconduseraccessamaster);
            if (UpdateUser)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Secound User Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Second User Detail not found",
                    Data = null
                };
            }
        }

        public async Task<AdminAccessMaster> GetAdminAccessMaster(string branchcode)
        {
            var AdminAccessList = await _userAccessRepository.GetAdminAccessMaster(branchcode);

            return AdminAccessList;
        }

        public async Task<ServiceResult> UpdateAdminAccessDetail(AdminAccessMaster adminaccessamaster)
        {
            var UpdateUser = await _userAccessRepository.UpdateAdminAccessDetail(adminaccessamaster);
            if (UpdateUser)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Admin Access Detail updated successfully",
                    Data = null
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Admin Access Detail not found",
                    Data = null
                };
            }
        }

        #endregion

        #region UserPermissionAccessMaster

        public async Task<IEnumerable<MainMenuModel>> GetMainMenuList(string branchcode)
        {
            var MainMenuList = await _userAccessRepository.GetMainMenuList(branchcode);

            return MainMenuList;
        }

        public async Task<IEnumerable<MainMenuGroupModel>> GetSubMenuList(string branchcode)
        {
            var subMenuList = await _userAccessRepository.GetSubMenuList(branchcode);

            var groupedList = subMenuList
                .GroupBy(x => x.MainMenuId)
                .Select(g => new MainMenuGroupModel
                {
                    MainMenuId = g.Key,
                    SubMenus = g.ToList()
                })
                .ToList();

            return groupedList;
        }

        public async Task<ServiceResult> InsertMainMenu(MainMenuModel model)
        {
            var Details = await _userAccessRepository.InsertMainMenu(model);
            if (Details > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Main Menu inserted successfully",
                    Data = Details
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert Main Menu",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> InsertSubMenu(SubMenuModel model)
        {
            var Details = await _userAccessRepository.InsertSubMenu(model);
            if (Details > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "Sub Menu inserted successfully",
                    Data = Details
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to insert Sub Menu",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> SaveMenuWithSubMenu( MenuWithSubMenuModel model)
        {
            var Details = await _userAccessRepository.SaveMenuWithSubMenu(model);
            if (Details > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "User Permission Access inserted successfully",
                    Data = Details
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to User Permission Access",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> DeleteMainMenuDetail(int MainMenuId, string branchcode)
        {
            var success = await _userAccessRepository.DeleteMainMenuDetail(MainMenuId, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Main Menu not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Main Menu Removed successfully",
                Data = null
            };
        }

        public async Task<ServiceResult> DeleteSubMenuDetail(int SubMenuId, string branchcode)
        {
            var success = await _userAccessRepository.DeleteSubMenuDetail(SubMenuId, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Sub Menu not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "Sub Menu Removed successfully",
                Data = null
            };
        }


        //public async Task<IEnumerable<UserPermissionAccessMaster>> GetUserPermissionAccessList(string branchcode, int usercode)
        //{
        //    var userPermissionAccessList = await _userAccessRepository.GetUserPermissionAccessList(branchcode, usercode);

        //    return userPermissionAccessList;
        //}

        public async Task<UserPermissionResponse> GetUserPermissionAccessList( string branchcode, int usercode, int roleId)
        {
            var data = await _userAccessRepository.GetUserPermissionAccessList(branchcode, usercode, roleId);

            var first = data.FirstOrDefault();

            if (first == null)
                return new UserPermissionResponse();

            var result = new UserPermissionResponse
            {
                UserCode = first.UserCode,
                UserName = first.UserName,
                RoleId = first.RoleId,
                RoleName = first.RoleName,
                BranchCode = first.BranchCode,

                Menus = data .GroupBy(x => new { x.MainMenuId, x.MenuName, x.MenuPermission })
                .Select(menuGroup => new MenuResponse
                {
                    MainMenuId = menuGroup.Key.MainMenuId,
                    MenuName = menuGroup.Key.MenuName,
                    MenuPermission = menuGroup.Key.MenuPermission,

                    SubMenus = menuGroup
                    .Select(sub => new SubMenuResponse
                    {
                        SubMenuId = sub.SubMenuId,
                        SubMenuName = sub.SubMenuName,
                        SubMenuPermission = sub.SubMenuPermission,
                        IsPermission = sub.IsPermission
                    }).ToList()
                }).ToList()
            };

            return result;
        }

        public async Task<ServiceResult> CreateUserPermissionAccessMaster(List<UserPermissionAccessMaster> userPermission)
        {
            var userCode = userPermission[0].UserCode;
            var roleId = userPermission[0].RoleId;
            var branchCode = userPermission[0].BranchCode;

            //var success = await _userAccessRepository.DeleteUserpermissionDetails(userCode, roleId, branchCode);
            //if (!success)
            //{
            //    return new ServiceResult
            //    {
            //        Success = false,
            //        Message = "User Permission Access not found",
            //        Data = null
            //    };
            //}

            //var result = await _userAccessRepository.CreateUserPermissionAccessMaster(userPermission);
            var result = await _userAccessRepository.SaveUserPermissions(userPermission);

            if (result > 0)
            {
                return new ServiceResult
                {
                    Success = true,
                    Message = "User Permission Access inserted successfully",
                    Data = result
                };
            }
            else
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "Failed to User Permission Access",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult> DeleteUserPermissionAccessMaster(int UserCode, int RoleId, string branchcode)
        {
            var success = await _userAccessRepository.DeleteUserpermissionDetails(UserCode, RoleId, branchcode);
            if (!success)
            {
                return new ServiceResult
                {
                    Success = false,
                    Message = "User Permission Access not found",
                    Data = null
                };
            }

            return new ServiceResult
            {
                Success = true,
                Message = "User Permission Access Removed successfully",
                Data = null
            };
        }

        #endregion
    }
}
