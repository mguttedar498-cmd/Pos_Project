using HMS_360_PMS.HMS_360_PMS.API.Models;
using HMS_360_PMS.ProjectEntitiesModels.Master;
using HMS_360_PMS.ProjectServiceLayer.Master.DTOs;
using HMS_360_PMS.ProjectServiceLayer.Master.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static HMS_360_PMS.ProjectEntitiesModels.Master.UserAccess_EntityModel;


namespace HMS_360_PMS.ProjectAPI.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAccessController : BaseController
    {
        private readonly ILogger<UserAccessController> _logger;
        private readonly IUserAccess_Service _userAccessService;

        public UserAccessController(ILogger<UserAccessController> logger, IUserAccess_Service userAccessService)
        {
            _logger = logger;
            _userAccessService = userAccessService;
        }

        #region RoleMaster

        [HttpGet("GetRoleMasterList")]
        public async Task<IActionResult> GetRoleMasterList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetRoleMasterList(branchcode);

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<RoleMasterModel>>
                    .SuccessResult(data, "No Role Master Details Found"));
            return Ok(ApiResponse<IEnumerable<RoleMasterModel>>
                .SuccessResult(data, "Role Master Details fetched successfully"));
        }

        #endregion

        #region UserAccessMaster

        [HttpGet("GetUserDetailsList")]
        public async Task<IActionResult> GetUserDetailsList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetUserDetailsList(branchcode);

            if (!data.Any())
                return Ok(ApiResponse<IEnumerable<UserAccessMaster>>
                    .SuccessResult(data, "No User Access Details Found"));
            return Ok(ApiResponse<IEnumerable<UserAccessMaster>>
                .SuccessResult(data, "User Access Details fetched successfully"));
        }

        [HttpPost("CreateUserDetailsMaster")]
        public async Task<IActionResult> CreateUserDetailsMaster([FromBody] UserAccessMaster userAccessMaster)
        {
            if (userAccessMaster == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (string.IsNullOrWhiteSpace(userAccessMaster.UserName))
                return Fail("User Name is required");

            var result = await _userAccessService.CreateUserDetailsMaster(userAccessMaster);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetUserDetailsList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "User Access Master created successfully"));
        }

        [HttpPut("UpdateUserDetailsMaster")]
        public async Task<IActionResult> UpdateUserDetailsMaster([FromBody] UserAccessMaster userAccessMaster)
        {
            if (userAccessMaster == null || userAccessMaster.UserCode <= 0)
                return Fail("Invalid data");

            var success = await _userAccessService.UpdateUserDetailsMaster(userAccessMaster);

            if (!success.Success)
                return NotFoundResponse("User Details Master not found");

            return Success(true, "Updated successfully");
        }

        [HttpDelete("DeleteUserDetailsMaster")]
        public async Task<IActionResult> DeleteUserDetailsMaster([FromQuery] int id, string branchcode)
        {
            if (id <= 0)
                return Fail("Invalid id");

            var result = await _userAccessService.DeleteUserDetailsMaster(id, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        [HttpGet("GetSecoundUserAccessMaster")]
        public async Task<IActionResult> GetSecoundUserAccessMaster([FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetSecoundUserAccessMaster(BranchCode);

            if (data == null)
                return Ok(ApiResponse<SecoundUserAccessMaster>
                    .SuccessResult(data, "No Second User Access Details Found"));
            return Ok(ApiResponse<SecoundUserAccessMaster>
                .SuccessResult(data, "Secound User Access Details fetched successfully"));
        }

        [HttpPut("UpdateSecoundUserAccessDetail")]
        public async Task<IActionResult> UpdateSecoundUserAccessDetail([FromBody] SecoundUserAccessMaster seconduseraccessamaster)
        {
            if (seconduseraccessamaster == null || seconduseraccessamaster.SecondUserPassword == null)
                return Fail("Invalid data");

            var success = await _userAccessService.UpdateSecoundUserAccessDetail(seconduseraccessamaster);

            if (!success.Success)
                return NotFoundResponse("Admin Details Master not found");

            return Success(true, "Updated successfully");
        }

        [HttpGet("GetAdminAccessMaster")]
        public async Task<IActionResult> GetAdminAccessMaster([FromQuery] string BranchCode)
        {
            if (string.IsNullOrWhiteSpace(BranchCode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetAdminAccessMaster(BranchCode);

            if (data == null)
                return Ok(ApiResponse<AdminAccessMaster>
                    .SuccessResult(data, "No Second User Access Details Found"));
            return Ok(ApiResponse<AdminAccessMaster>
                .SuccessResult(data, "Secound User Access Details fetched successfully"));
        }

        [HttpPut("UpdateAdminAccessDetail")]
        public async Task<IActionResult> UpdateAdminAccessDetail([FromBody] AdminAccessMaster adminaccessamaster)
        {
            if (adminaccessamaster == null || adminaccessamaster.AdminPassword == null)
                return Fail("Invalid data");

            var success = await _userAccessService.UpdateAdminAccessDetail(adminaccessamaster);

            if (!success.Success)
                return NotFoundResponse("Admin Details Master not found");

            return Success(true, "Updated successfully");
        }

        #endregion

        #region UserPermissionAccessMaster

        [HttpGet("GetMainMenuList")]
        public async Task<IActionResult> GetMainMenuList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetMainMenuList(branchcode);

            if (data == null)
                return Ok(ApiResponse<IEnumerable<MainMenuModel>>
                    .SuccessResult(data, "No Main Menu Details Found"));
            return Ok(ApiResponse<IEnumerable<MainMenuModel>>
                .SuccessResult(data, "Main Menu Details fetched successfully"));
        }

        [HttpGet("GetSubMenuList")]
        public async Task<IActionResult> GetSubMenuList([FromQuery] string branchcode)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetSubMenuList(branchcode);

            if (data == null)
                return Ok(ApiResponse<IEnumerable<MainMenuGroupModel>>
                    .SuccessResult(data, "No Sub Menu Details Found"));
            return Ok(ApiResponse<IEnumerable<MainMenuGroupModel>>
                .SuccessResult(data, "Sub Menu Details fetched successfully"));
        }

        //[HttpPost("InsertMainMenu")]
        //public async Task<IActionResult> InsertMainMenu( [FromBody] MainMenuModel menumodel)
        //{
        //    if (menumodel == null)
        //        return Fail("Invalid request payload");

        //    if (!ModelState.IsValid)
        //        return Fail("Validation failed");

        //    if (string.IsNullOrWhiteSpace(menumodel.MenuName))
        //        return Fail("Menu Name is required");

        //    var result = await _userAccessService.InsertMainMenu(menumodel);
        //    return CreatedAtAction(nameof(GetUserPermissionAccessList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Main Menu created successfully"));
        //}

        //[HttpPost("InsertSubMenu")]
        //public async Task<IActionResult> InsertSubMenu( [FromBody] SubMenuModel submodel)
        //{
        //    if (submodel == null)
        //        return Fail("Invalid request payload");

        //    if (!ModelState.IsValid)
        //        return Fail("Validation failed");

        //    if (string.IsNullOrWhiteSpace(submodel.SubMenuName))
        //        return Fail("Menu Name is required");

        //    var result = await _userAccessService.InsertSubMenu(submodel);
        //    return CreatedAtAction(nameof(GetUserPermissionAccessList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Sub Menu created successfully"));
        //}

        [HttpPost("SaveMenuWithSubMenu")]
        public async Task<IActionResult> SaveMenuWithSubMenu( [FromBody] MenuWithSubMenuModel requestmodel)
        {
            if (requestmodel == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            //if (string.IsNullOrWhiteSpace(requestmodel.SubMenuName))
            //    return Fail("Menu Name is required");

            var result = await _userAccessService.SaveMenuWithSubMenu(requestmodel);
            return CreatedAtAction(nameof(GetUserPermissionAccessList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "Sub Menu created successfully"));
        }

        [HttpDelete("DeleteMainMenuDetail")]
        public async Task<IActionResult> DeleteMainMenuDetail([FromQuery] int MainMenuId, [FromQuery] string branchcode)
        {
            if (MainMenuId <= 0 || string.IsNullOrWhiteSpace(branchcode))
                return Fail("Invalid id");

            var result = await _userAccessService.DeleteMainMenuDetail(MainMenuId, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        [HttpDelete("DeleteSubMenuDetail")]
        public async Task<IActionResult> DeleteSubMenuDetail([FromQuery] int SubMenuId, [FromQuery] string branchcode)
        {
            if (SubMenuId <= 0 || string.IsNullOrWhiteSpace(branchcode))
                return Fail("Invalid id");

            var result = await _userAccessService.DeleteSubMenuDetail(SubMenuId, branchcode);

            if (!result.Success)
                return NotFoundResponse(result.Message);

            return Success(true, "Deleted successfully");
        }

        [HttpGet("GetUserPermissionAccessList")]
        public async Task<IActionResult> GetUserPermissionAccessList([FromQuery] string branchcode, [FromQuery] int usercode, [FromQuery] int roleId)
        {
            if (string.IsNullOrWhiteSpace(branchcode))
                return BadRequest(ApiResponse<string>.Failure("Branch code is required"));

            var data = await _userAccessService.GetUserPermissionAccessList(branchcode, usercode, roleId);

            if (data == null)
                return Ok(ApiResponse<UserPermissionResponse>
                    .SuccessResult(data, "No User Permission Access Details Found"));
            return Ok(ApiResponse<UserPermissionResponse>
                .SuccessResult(data, "User Permission Access Details fetched successfully"));
        }

        [HttpPost("InsertUserPermissionAccessMaster")]
        public async Task<IActionResult> InsertUserPermissionAccessMaster([FromBody] List<UserPermissionAccessMaster> userPermission)
        {
            if (userPermission == null)
                return Fail("Invalid request payload");

            if (!ModelState.IsValid)
                return Fail("Validation failed");

            if (userPermission.Any(up => string.IsNullOrWhiteSpace(up.UserName)))
                return Fail("User Name is required");

            var result = await _userAccessService.CreateUserPermissionAccessMaster(userPermission);
            return CreatedAtAction(nameof(GetUserPermissionAccessList), new { id = result.Data }, ApiResponse<int>.SuccessResult((int)result.Data, "User Permission Access Master created successfully"));
        }

        //[HttpDelete("DeleteUserPermissionAccessMaster")]
        //public async Task<IActionResult> DeleteUserPermissionAccessMaster([FromQuery] int UserCode , [FromQuery] int RoleId, [FromQuery] string branchcode)
        //{
        //    if (UserCode <= 0 || RoleId <= 0 || string.IsNullOrWhiteSpace(branchcode))
        //        return Fail("Invalid id");

        //    var result = await _userAccessService.DeleteUserPermissionAccessMaster(UserCode, RoleId, branchcode);

        //    if (!result.Success)
        //        return NotFoundResponse(result.Message);

        //    return Success(true, "Deleted successfully");
        //}
        #endregion
    }
}
