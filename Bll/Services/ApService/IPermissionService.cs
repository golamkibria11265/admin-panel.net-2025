using Dll.Data.Entities;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utility;

namespace Bll.Services.ApService
{
    public interface IPermissionService
    {
        Task<ApiResponse> SyncPermissionList();
    }
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        public PermissionService(IUnitOfWork unitOfWork, Serilog.ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }
        public async Task<ApiResponse> SyncPermissionList()
        {
            ApiResponse apiResponse = new();
            List<ApMenu> menuList = await _unitOfWork.MenuRepository.FindAll();
            if (menuList == null)
            {
                throw new ApplicationException("Menu list is not found");
            }
            List<ApPermission> permissionList = await _unitOfWork.PermissionRepository.FindAll();

            var menuListToAdd = menuList.Where(menu => !permissionList.Any(permission => permission.MenuId == menu.Id)).ToList();
            var permissionListToAdd = menuListToAdd.Select(x => new ApPermission
            {
                Name = x.Name,
                MenuId = x.Id,
                IsActive = 1,
                CreatedDate = DateTime.UtcNow
            }).ToList();
            // Only permissions whose menu name changed need an update.
            var permissionListUpdate = new List<ApPermission>();
            foreach (var permission in permissionList)
            {
                var menu = menuList.FirstOrDefault(x => x.Id == permission.MenuId);
                if (menu != null && permission.Name != menu.Name)
                {
                    permission.Name = menu.Name;
                    permission.UpdatedDate = DateTime.UtcNow;
                    permissionListUpdate.Add(permission);
                }
            }
            var permissionListToDelete = permissionList.Where(permission => !menuList.Any(menu => menu.Id == permission.MenuId)).ToList();

            if (permissionListToDelete.Any())
            {
                // Also remove the role assignments of permissions whose menu no longer exists.
                var deletedIds = permissionListToDelete.Select(p => p.Id).ToList();
                var orphanRolePermissions = await _unitOfWork.RolePermissionRepository.FindByCondition(x => deletedIds.Contains(x.PermissionId));
                if (orphanRolePermissions.Any()) await _unitOfWork.RolePermissionRepository.DeleteRange(orphanRolePermissions);
            }
            if (permissionListToAdd.Any()) await _unitOfWork.PermissionRepository.CreateRange(permissionListToAdd);
            if (permissionListUpdate.Any()) await _unitOfWork.PermissionRepository.UpdateRange(permissionListUpdate);
            if (permissionListToDelete.Any()) await _unitOfWork.PermissionRepository.DeleteRange(permissionListToDelete);
            await _unitOfWork.CompleteAsync();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string> { "Sync Successfully done" };
            return apiResponse;
        }
    }
}
