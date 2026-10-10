using Dll.Data.Entities;
using Dll.Models.ViewModels;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Utility;

namespace Bll.Services.ApService
{
    public interface IRolePermissionService
    {
        Task<ApiResponse> GetRolePermissionsByRoleId(int roleId);
        Task<ApiResponse> UpdateRolePermissionsByRoleId(RolePermissionNestedViewModel rolePermissionNestedViewModel);
    }
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        public RolePermissionService(IUnitOfWork unitOfWork, Serilog.ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<ApiResponse> GetRolePermissionsByRoleId(int roleId)
        {
            ApiResponse apiResponse = new();
            List<ApMenu> menuList = await _unitOfWork.MenuRepository.FindAll();
            if (menuList == null)
            {
                throw new ApplicationException("Menu list is not found");
            }
            List<ApPermission> permissionList = await _unitOfWork.PermissionRepository.FindAll();
            if (permissionList == null)
            {
                throw new ApplicationException("Permission list is not found");
            }
            List<ApRolePermission> rolePermissionList = await _unitOfWork.RolePermissionRepository.FindByCondition(x => x.RoleId == roleId);

            List<RolePermissionViewModel> rolePermissionViewModelList = new List<RolePermissionViewModel>();

            foreach (var permission in permissionList)
            {
                var rolePermission = rolePermissionList.FirstOrDefault(x => x.PermissionId == permission.Id);
                RolePermissionMapping(rolePermissionViewModelList, permission, rolePermission);
            }
            foreach (var rolePermissionNested in rolePermissionViewModelList)
            {
                var menu = menuList.FirstOrDefault(x => x.Id == rolePermissionNested.MenuId);
                if (menu != null)
                {
                    rolePermissionNested.Name = menu.Name;
                    rolePermissionNested.ParentId = menu.Parent;
                }
            }
            rolePermissionViewModelList = GetNested(rolePermissionViewModelList, null);
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string> { "Successfully fetched" };
            apiResponse.data = new RolePermissionNestedViewModel
            {
                RoleId = roleId,
                RolePermissionNestedViewModelList = rolePermissionViewModelList
            };
            return apiResponse;
        }
        private static void RolePermissionMapping(List<RolePermissionViewModel> rolePermissionViewModelList, ApPermission permission, ApRolePermission? rolePermission)
        {
            if (rolePermission != null)
            {
                rolePermissionViewModelList.Add(new RolePermissionViewModel
                {
                    Id = rolePermission.Id,
                    RoleId = rolePermission.RoleId,
                    PermissionId = rolePermission.PermissionId,
                    MenuId = permission.MenuId,
                    IsSelected = true
                });
            }
            else
            {
                rolePermissionViewModelList.Add(new RolePermissionViewModel
                {
                    PermissionId = permission.Id,
                    MenuId = permission.MenuId,
                    IsSelected = false
                });
            }
        }
        private static List<RolePermissionViewModel> GetNested(List<RolePermissionViewModel> rolePermissionViewModelList, int? parentId)
        {
            return rolePermissionViewModelList
                .Where(c => c.ParentId == parentId)
                .Select(c => new RolePermissionViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    RoleId = c.RoleId,
                    PermissionId = c.PermissionId,
                    MenuId = c.MenuId,
                    ParentId = c.ParentId,
                    IsSelected = c.IsSelected,
                    ChildRolePermissionViewModels = GetNested(rolePermissionViewModelList, c.MenuId)
                })
                .ToList();
        }
        public async Task<ApiResponse> UpdateRolePermissionsByRoleId(RolePermissionNestedViewModel rolePermissionNestedViewModel)
        {
            ApiResponse apiResponse = new();
            try
            {
                if (rolePermissionNestedViewModel == null)
                {
                    apiResponse.code = StatusCodes.Status204NoContent;
                    apiResponse.messages = new List<string> { "No content is selected" };
                    return apiResponse;
                }
                if (rolePermissionNestedViewModel.RolePermissionNestedViewModelList == null)
                {
                    apiResponse.code = StatusCodes.Status204NoContent;
                    apiResponse.messages = new List<string> { "No permission is selected" };
                    return apiResponse;
                }
                if (!IsPermissionSelectionValid(rolePermissionNestedViewModel.RolePermissionNestedViewModelList))
                {
                    apiResponse.code = StatusCodes.Status400BadRequest;
                    apiResponse.messages = new List<string> { "Invalid permission selection" };
                    return apiResponse;
                }
                int roleId = rolePermissionNestedViewModel.RoleId;
                // The role must exist (a brand-new role has no permissions yet, which is fine).
                var role = await _unitOfWork.RoleRepository.FindFirstOrDefault(x => x.Id == roleId);
                if (roleId <= 0 || role == null)
                {
                    apiResponse.code = StatusCodes.Status400BadRequest;
                    apiResponse.messages = new List<string> { "Invalid selected role" };
                    return apiResponse;
                }
                // Selecting nothing is allowed: it removes all permissions from the role.
                var selectedPermissionIds = GetFlat(rolePermissionNestedViewModel.RolePermissionNestedViewModelList)
                    .Select(x => x.PermissionId)
                    .Distinct()
                    .ToList();
                // Only permissions that really exist may be granted (ignore tampered ids).
                var existingPermissionIds = (await _unitOfWork.PermissionRepository.FindAll()).Select(p => p.Id).ToHashSet();
                if (selectedPermissionIds.Any(id => !existingPermissionIds.Contains(id)))
                {
                    apiResponse.code = StatusCodes.Status400BadRequest;
                    apiResponse.messages = new List<string> { "Invalid permission selection" };
                    return apiResponse;
                }
                var rolePermissionListFromDb = await _unitOfWork.RolePermissionRepository.FindByCondition(x => x.RoleId == roleId);
                var rolePermissionListToAdd = selectedPermissionIds
                    .Where(permissionId => !rolePermissionListFromDb.Any(y => y.PermissionId == permissionId))
                    .Select(permissionId => new ApRolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId,
                        IsActive = 1,
                        CreatedDate = DateTime.UtcNow
                    })
                    .ToList();
                var rolePermissionListToDelete = rolePermissionListFromDb
                    .Where(x => !selectedPermissionIds.Contains(x.PermissionId))
                    .ToList();
                if (rolePermissionListToAdd.Any()) await _unitOfWork.RolePermissionRepository.CreateRange(rolePermissionListToAdd);
                if (rolePermissionListToDelete.Any()) await _unitOfWork.RolePermissionRepository.DeleteRange(rolePermissionListToDelete);
                await _unitOfWork.CompleteAsync();
                apiResponse.code = StatusCodes.Status200OK;
                apiResponse.messages = new List<string> { "Successful" };
                return apiResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while updating role permissions");

                apiResponse.code = StatusCodes.Status500InternalServerError;    
                apiResponse.messages = new List<string> { "Something went wrong" };
                return apiResponse;
            }
        }
        public bool IsPermissionSelectionValid(List<RolePermissionViewModel> rolePermissionViewModelList)
        {
            foreach (var item in rolePermissionViewModelList)
            {

                if (!IsPermissionHierarchyValid(item, item.IsSelected))
                {
                    return false;
                }
            }
            return true;
        }
        private bool IsPermissionHierarchyValid(RolePermissionViewModel current, bool parentSelected)
        {
            if (current.IsSelected && !parentSelected)
            {
                return false;
            }
            if (current.ChildRolePermissionViewModels != null && current.ChildRolePermissionViewModels.Any())
            {
                foreach (var child in current.ChildRolePermissionViewModels)
                {
                    bool isValid = IsPermissionHierarchyValid(child, current.IsSelected);
                    if (!isValid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static List<RolePermissionViewModel> GetFlat(List<RolePermissionViewModel> rolePermissionViewModelList)
        {
            var flatList = new List<RolePermissionViewModel>();
            foreach (var item in rolePermissionViewModelList)
            {
                if (item.IsSelected)
                {
                    flatList.Add(new RolePermissionViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        RoleId = item.RoleId,
                        PermissionId = item.PermissionId,
                        MenuId = item.MenuId,
                        ParentId = item.ParentId,
                        IsSelected = item.IsSelected
                    });
                }
                if (item.ChildRolePermissionViewModels != null && item.ChildRolePermissionViewModels.Any())
                {
                    flatList.AddRange(GetFlat(item.ChildRolePermissionViewModels));
                }
            }
            return flatList;
        }
    }
}
