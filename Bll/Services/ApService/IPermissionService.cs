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
                MenuId = x.Id
            }).ToList();
            var permissionListUpdate = permissionList.Where(permission => menuList.Any(menu => menu.Id == permission.MenuId)).ToList();
            foreach (var permission in permissionListUpdate)
            {
                var menu = menuList.FirstOrDefault(x => x.Id == permission.MenuId);
                if (menu != null)
                {
                    permission.Name = menu.Name;
                }
            }
            var permissionListToDelete = permissionList.Where(permission => !menuList.Any(menu => menu.Id == permission.MenuId)).ToList();

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
