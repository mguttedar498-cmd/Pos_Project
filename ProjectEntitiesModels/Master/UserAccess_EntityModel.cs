using DocumentFormat.OpenXml.Spreadsheet;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace HMS_360_PMS.ProjectEntitiesModels.Master
{
    public class UserAccess_EntityModel
    {

        #region RoleMaster

        public class RoleMasterModel
        {
            public int RoleId { get; set; } = 0;
            public string RoleName { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
        }

        #endregion


        #region UserAccessMaster

        public class UserAccessMaster
        {
            public int UserCode { get; set; } = 0;
            public string UserName { get; set; } = string.Empty;
            public string UserPassword { get; set; } = string.Empty;
            public string UserPrivilege { get; set; } = string.Empty;
            public string EnteredBy { get; set; } = string.Empty;
            public string LastModify { get; set; } = string.Empty;
            public string Branch_code { get; set; } = string.Empty;
            public int storeid { get; set; } = 0;
            public double DisPercent { get; set; } = 0;
            public double DisAmount { get; set; } = 0;
            public int RoleId { get; set; } = 0;
        }

        public class SecoundUserAccessMaster
        {

            public int SecoundUserId { get; set; } = 0;
            public string SecondUserPassword { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
        }

        public class AdminAccessMaster
        {
            public int SecoundUserId { get; set; } = 0;
            public string AdminPassword { get; set; } = string.Empty;
            public string BranchCode { get; set; } = string.Empty;
        }

        #endregion

        #region UserPermissionAccessMaster

        public class MainMenuModel
        {
            public int MainMenuId { get; set; } = 0;
            public string MenuName { get; set; } = string.Empty;
            public bool MenuPermission { get; set; } = false;
            public string BranchCode { get; set; } = string.Empty;  
        }

        public class SubMenuModel
        {
            public int SubMenuId { get; set; } = 0;
            public string SubMenuName { get; set; } = string.Empty;
            public bool SubMenuPermission { get; set; } = false;
            public int MainMenuId { get; set; } = 0;
            public string BranchCode { get; set; } = string.Empty;
        }

        public class MenuWithSubMenuModel
        {
            public int MainMenuId { get; set; }
            public string MenuName { get; set; } = string.Empty;
            public bool MenuPermission { get; set; }
            public int SubMenuId { get; set; }
            public string SubMenuName { get; set; } = string.Empty;
            public bool SubMenuPermission { get; set; }
            public string BranchCode { get; set; } = string.Empty;
            public bool IsExistingMainMenu { get; set; }
        }

        public class UserPermissionAccessMaster
        {
            public int UserCode { get; set; } = 0;
            public string UserName { get; set; } = string.Empty;
            public int RoleId { get; set; } = 0;
            public string RoleName { get; set; } = string.Empty;
            public int MainMenuId { get; set; } = 0;
            public string MenuName { get; set; } = string.Empty;
            public bool MenuPermission { get; set; } = false;
            public int SubMenuId { get; set; } = 0;
            public string SubMenuName { get; set; } = string.Empty;
            public bool SubMenuPermission { get; set; } = false;
            public bool IsPermission { get; set; } = false;
            public string BranchCode { get; set; } = string.Empty;
        }

        #endregion

    }
}
