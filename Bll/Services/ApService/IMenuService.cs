using Bll.Services.CommonService;
using Dll.Data;
using Dll.Data.Entities;
using Dll.Models.RequestModels;
using Dll.Models.ViewModels;
using Dll.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;

namespace Bll.Services.ApService
{
    public interface IMenuService
    {
        Task<ApiResponse> AddOrEditMenu(MenuViewModel menuViewModel);
        Task<ApiResponse> DeleteMenuById(int id);
        Task<ApiResponse> GetMenuById(int id);
        Task<ApiResponse> GetMenuList(MenuParamsRequest menuParamsRequest);
    }
    public class MenuService : IMenuService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly Serilog.ILogger _logger;
        private readonly IFileWriterService _fileWriterService;
        public MenuService(IUnitOfWork unitOfWork, Serilog.ILogger logger, IFileWriterService fileWriterService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _fileWriterService = fileWriterService;
        }

        public async Task<ApiResponse> AddOrEditMenu(MenuViewModel menuViewModel)
        {
            _logger.Information("IMenuService->AddOrEditMenu->Request: Id={Id}, Name={Name}, Url={Url}, Parent={Parent}, HasImage={HasImage}",
                menuViewModel.Id, menuViewModel.Name, menuViewModel.Url, menuViewModel.Parent, menuViewModel.Image != null);
            ApiResponse apiResponse = new();
            bool result = false;
            if (menuViewModel.Id > 0)
            {
                await UpdateMenu(menuViewModel);
            }
            else
            {
                await AddMenu(menuViewModel);
            }
            result = true;
            if (result)
            {
                apiResponse.code = StatusCodes.Status200OK;
                apiResponse.messages = new List<string>() { "Success" };
            }
            else
            {
                apiResponse.code = StatusCodes.Status417ExpectationFailed;
                apiResponse.messages = new List<string>() { "Failed" };
            }
            _logger.Information("IMenuService->AddOrEditMenu->Response: " + JsonConvert.SerializeObject(apiResponse));
            return apiResponse;
        }
        private async Task UpdateMenu(MenuViewModel menuViewModel)
        {
            ApMenu? menu = new();
            menu = await _unitOfWork.MenuRepository.GetById(menuViewModel.Id);
            if (menu == null)
            {
                throw new CustomApplicationException("Menu not found", StatusCodes.Status404NotFound);
            }
            // Names must be unique among menus with the same parent (e.g. "Add" may exist under several pages).
            var duplicate = await _unitOfWork.MenuRepository.GetByCondition(x => x.Name == menuViewModel.Name && x.Parent == menu.Parent && x.Id != menu.Id);
            if (duplicate != null)
            {
                throw new CustomApplicationException("A menu with this name already exists at this level", StatusCodes.Status409Conflict);
            }
            string? oldImage = menu.Image;
            menu.Name = menuViewModel.Name;
            menu.Url = menuViewModel.Url;
            menu.MenuOrder = menuViewModel.MenuOrder;
            menu.IsAction = menuViewModel.IsAction == true ? 1 : 0;
            menu.UpdatedDate = DateTime.UtcNow;
            if (menuViewModel.Image != null)
            {
                // Throws CustomApplicationException for files that are not allowed; nothing is saved then.
                menu.Image = await _fileWriterService.UploadImage(menuViewModel.Image);
            }
            await _unitOfWork.MenuRepository.CustomMethodToUpdateMenuByIdAsync(menu);
            await _unitOfWork.CompleteAsync();
            if (menuViewModel.Image != null && !string.IsNullOrEmpty(oldImage) && oldImage != menu.Image)
            {
                _fileWriterService.DeleteImageByFileName(oldImage);
            }
        }
        private async Task AddMenu(MenuViewModel menuViewModel)
        {
            ApMenu? menu = new();
            int? parentId = menuViewModel.Parent > 0 ? menuViewModel.Parent : null;
            menu = await _unitOfWork.MenuRepository.GetByCondition(x => x.Name == menuViewModel.Name && x.Parent == parentId);
            if (menu != null)
            {
                throw new CustomApplicationException("A menu with this name already exists at this level", StatusCodes.Status409Conflict);
            }
            ApMenu menuModel = new();
            menuModel.Name = menuViewModel.Name;
            menuModel.Url = menuViewModel.Url;
            menuModel.MenuOrder = menuViewModel.MenuOrder;
            menuModel.IsAction = menuViewModel.IsAction == true ? 1 : 0;
            menuModel.CreatedDate = DateTime.UtcNow;
            if (menuViewModel.Parent > 0)
            {
                menuModel.Parent = menuViewModel.Parent;
            }
            if (menuViewModel.Image != null)
            {
                menuModel.Image = await _fileWriterService.UploadImage(menuViewModel.Image);
            }
            await _unitOfWork.MenuRepository.CustomMethodToAddMenuAsync(menuModel);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<ApiResponse> DeleteMenuById(int id)
        {
            ApMenu? menu = await _unitOfWork.MenuRepository.GetById(id);
            if (menu == null)
            {
                throw new CustomApplicationException("Menu is not found", StatusCodes.Status404NotFound);
            }
            // Do not leave sub-menus pointing to a parent that no longer exists.
            var children = await GetNestedChildren(id);
            if (children.Any())
            {
                throw new CustomApplicationException("This menu has sub-menus. Delete or move them first.", StatusCodes.Status409Conflict);
            }
            // Remove the menu's permissions and the role assignments of those permissions as well.
            var permissions = await _unitOfWork.PermissionRepository.FindByCondition(x => x.MenuId == id);
            if (permissions.Any())
            {
                var permissionIds = permissions.Select(p => p.Id).ToList();
                var rolePermissions = await _unitOfWork.RolePermissionRepository.FindByCondition(x => permissionIds.Contains(x.PermissionId));
                if (rolePermissions.Any())
                {
                    await _unitOfWork.RolePermissionRepository.DeleteRange(rolePermissions);
                }
                await _unitOfWork.PermissionRepository.DeleteRange(permissions);
            }
            string? image = menu.Image;
            await _unitOfWork.MenuRepository.CustomMethodToDeleteMenuByIdAsync(menu);
            await _unitOfWork.CompleteAsync();
            // Delete the file only after the database change succeeded.
            if (!string.IsNullOrEmpty(image))
            {
                _fileWriterService.DeleteImageByFileName(image);
            }
            ApiResponse apiResponse = new();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            return apiResponse;
        }
        private async Task<List<ApMenu>> GetNestedChildren(int rootParentId)
        {
            async Task<List<ApMenu>> GetChildrenRecursive(int parentId)
            {
                List<ApMenu> allChildren = new List<ApMenu>();
                List<ApMenu> children = await _unitOfWork.MenuRepository.FindByCondition(x => x.Parent == parentId);
                foreach (var child in children)
                {
                    allChildren.Add(child);
                    allChildren.AddRange(await GetChildrenRecursive(child.Id));
                }
                return allChildren;
            }
            return await GetChildrenRecursive(rootParentId);
        }
        public async Task<ApiResponse> GetMenuById(int id)
        {
            _logger.Information("IMenuService->GetMenuById->Request: " + JsonConvert.SerializeObject(id));
            ApMenu? menu = new();
            menu = await _unitOfWork.MenuRepository.CustomMethodToGetMenuById(id);
            if (menu == null)
            {
                throw new CustomApplicationException("Menu is not found", StatusCodes.Status404NotFound);
            }
            MenuViewModel menuViewModel = new();
            menuViewModel.Id = menu.Id;
            menuViewModel.Name = menu.Name;
            menuViewModel.Url = menu.Url;
            menuViewModel.Parent = menu.Parent;
            menuViewModel.IsAction = menu.IsAction == 1 ? true : false;
            if (menu.Parent != null && menu.Parent > 0)
            {
                ApMenu? parentMenu = new();
                parentMenu = await _unitOfWork.MenuRepository.CustomMethodToGetMenuById(menu.Parent ?? 0);
                menuViewModel.ParentName = parentMenu.Name;
            }
            menuViewModel.MenuOrder = menu.MenuOrder;
            menuViewModel.ImageUrl = _fileWriterService.ImageUrlByFileName(menu.Image ?? string.Empty);
            ApiResponse apiResponse = new();
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = menuViewModel;
            _logger.Information("IMenuService->GetMenuById->Response: " + JsonConvert.SerializeObject(apiResponse));
            return apiResponse;
        }
        public async Task<ApiResponse> GetMenuList(MenuParamsRequest menuParamsRequest)
        {
            ApiResponse apiResponse = new();
            PagedListResponse<ApMenu>? menus = await _unitOfWork.MenuRepository.CustomMethodToGetAllMenus(menuParamsRequest);
            if (menus == null)
            {
                throw new CustomApplicationException("No content found", StatusCodes.Status204NoContent);
            }
            List<MenuViewModel> menuViewModels = new();
            foreach (var menu in menus)
            {
                MenuViewModel menuViewModel = new();
                menuViewModel.Id = menu.Id;
                menuViewModel.Name = menu.Name;
                menuViewModel.Url = menu.Url;
                menuViewModel.IsAction = menu.IsAction == 1 ? true : false;
                menuViewModel.MenuOrder = menu.MenuOrder;
                menuViewModel.ImageUrl = _fileWriterService.ImageUrlByFileName(menu.Image ?? string.Empty);
                menuViewModels.Add(menuViewModel);
            }
            apiResponse.code = StatusCodes.Status200OK;
            apiResponse.messages = new List<string>() { "Success" };
            apiResponse.data = menuViewModels;
            apiResponse.CurrentPage = menus.CurrentPage;
            apiResponse.PageSize = menus.PageSize;
            apiResponse.TotalCount = menus.TotalCount;
            apiResponse.TotalPages = menus.TotalPages;
            return apiResponse;
        }
    }
}
