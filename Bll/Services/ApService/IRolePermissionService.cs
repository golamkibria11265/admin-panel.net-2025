using Dll.Data.Entities;
using Dll.Models.ViewModels;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var flatRolePermissionViewList = GetFlat(rolePermissionNestedViewModel.RolePermissionNestedViewModelList);
            if (!flatRolePermissionViewList.Any())
            {
                apiResponse.code = StatusCodes.Status204NoContent;
                apiResponse.messages = new List<string> { "No content is selected" };
                return apiResponse;
            }
            var rolePermissionListFromUi = flatRolePermissionViewList.Select(x => new ApRolePermission
            {
                Id = x.Id,
                RoleId = rolePermissionNestedViewModel.RoleId,
                PermissionId = x.PermissionId
            });
            var rolePermissionListFromDb = await _unitOfWork.RolePermissionRepository.FindByCondition(x => x.RoleId == rolePermissionNestedViewModel.RoleId);
            if(!rolePermissionListFromDb.Any())
            {
                apiResponse.code = StatusCodes.Status204NoContent;
                apiResponse.messages = new List<string> { "Invalid selected role" };
                return apiResponse;
            }
            var rolePermissionListToAdd = rolePermissionListFromUi.Where(x => !rolePermissionListFromDb.Any(y => y.RoleId == x.RoleId && y.PermissionId == x.PermissionId)).ToList();
            var rolePermissionListToDelete = rolePermissionListFromDb.Where(x => !rolePermissionListFromUi.Any(y => y.RoleId == x.RoleId && y.PermissionId == x.PermissionId)).ToList();
            if (rolePermissionListToAdd.Any()) await _unitOfWork.RolePermissionRepository.CreateRange(rolePermissionListToAdd);
            if (rolePermissionListToDelete.Any()) await _unitOfWork.RolePermissionRepository.DeleteRange(rolePermissionListToDelete);
            await _unitOfWork.CompleteAsync();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string> { "Successful" };
            return apiResponse;
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
