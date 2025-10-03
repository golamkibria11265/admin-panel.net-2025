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
            _logger.Information("IMenuService->AddOrEditMenu->Request: " + JsonConvert.SerializeObject(menuViewModel));
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
                throw new CustomApplicationException("Menu not found: ", StatusCodes.Status204NoContent);
            }
            menu.Name = menuViewModel.Name;
            menu.Url = menuViewModel.Url;
            menu.MenuOrder = menuViewModel.MenuOrder;
            menu.IsAction = menuViewModel.IsAction == true ? 1 : 0;
            if (menuViewModel.Image != null)
            {
                menu.Image = await _fileWriterService.UploadImage(menuViewModel.Image);
            }
            await _unitOfWork.MenuRepository.CustomMethodToUpdateMenuByIdAsync(menu);
            await _unitOfWork.CompleteAsync();
        }
        private async Task AddMenu(MenuViewModel menuViewModel)
        {
            ApMenu? menu = new();
            menu = await _unitOfWork.MenuRepository.GetByCondition(x => x.Name == menuViewModel.Name);
            if (menu != null)
            {
                throw new CustomApplicationException("Menu already exist: ", StatusCodes.Status409Conflict);
            }
            ApMenu menuModel = new();
            menuModel.Name = menuViewModel.Name;
            menuModel.Url = menuViewModel.Url;
            menuModel.MenuOrder = menuViewModel.MenuOrder;
            menuModel.IsAction = menuViewModel.IsAction == true ? 1 : 0;
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
            if (!string.IsNullOrEmpty(menu.Image))
            {
                _fileWriterService.DeleteImageByFileName(menu.Image);
            }
            await _unitOfWork.MenuRepository.CustomMethodToDeleteMenuByIdAsync(menu);
            await _unitOfWork.CompleteAsync();
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
