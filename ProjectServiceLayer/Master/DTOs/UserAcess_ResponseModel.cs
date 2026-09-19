using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;

namespace HMS_360_PMS.ProjectServiceLayer.Master.DTOs
{

    public class MainMenuGroupModel
    {
        public int MainMenuId { get; set; }

        public List<SubMenuModel> SubMenus { get; set; } = new();
    }

    public class UserPermissionResponse
    {
        public int UserCode { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string BranchCode { get; set; } = string.Empty;

        public List<MenuResponse> Menus { get; set; } = new();
    }

    public class MenuResponse
    {
        public int MainMenuId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public bool MenuPermission { get; set; } = false;
        public List<SubMenuResponse> SubMenus { get; set; } = new();
    }

    public class SubMenuResponse
    {
        public int SubMenuId { get; set; }
        public string SubMenuName { get; set; } = string.Empty;
        public bool SubMenuPermission { get; set; }
        public bool IsPermission { get; set; }
    }
}
