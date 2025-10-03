using Dll.Data;
using Dll.Data.Entities;
using Dll.Models.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using Utility;
using Utility.Exceptions;
using Utility.Models.ResponseModels;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApMenuRepository : IRepositoryBase<ApMenu>
    {
        Task<ApMenu> CustomMethodToGetMenuById(int id);
        Task CustomMethodToAddMenuAsync(ApMenu menuModel);
        Task CustomMethodToUpdateMenuByIdAsync(ApMenu menuModel);
        Task CustomMethodToDeleteMenuByIdAsync(ApMenu menuModel);
        Task<PagedListResponse<ApMenu>> CustomMethodToGetAllMenus(MenuParamsRequest menuParamsRequest);
    }
    public class MenuRepository : RepositoryBase<ApMenu>, IApMenuRepository
    {
        private readonly DotNetContext _dotNetContext;
        private readonly string _cacheName = "MenuRepository";
        private readonly IRedisService _iRedisService;
        public MenuRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "MenuRepository")
        {
            _dotNetContext = dotNetContext;
            _iRedisService = iRedisService;
        }
        public async Task<PagedListResponse<ApMenu>> CustomMethodToGetAllMenus(MenuParamsRequest menuParamsRequest)
        {
            var cacheName = await _iRedisService.GetAsync(_cacheName);
            if (cacheName != null)
            {
                return await PagingFromCache(menuParamsRequest);
            }
            var queryableMenuList = _dotNetContext.ApMenus.AsQueryable();
            if (!string.IsNullOrEmpty(menuParamsRequest.SearchString))
            {
                queryableMenuList = queryableMenuList.Where(menu => menu.Name != null && menu.Name.Contains(menuParamsRequest.SearchString) || menu.Url != null && menu.Url.Contains(menuParamsRequest.SearchString));
            }
            if (!string.IsNullOrEmpty(menuParamsRequest.Parent))
            {
                queryableMenuList = queryableMenuList.Where(menu => menu.Parent != null && menu.Parent == Convert.ToInt32(menuParamsRequest.Parent));
            }
            var serializedMenuList = JsonConvert.SerializeObject(await queryableMenuList.ToListAsync(), ProjectHelper.GetJsonSettings());
            if (string.IsNullOrEmpty(menuParamsRequest.Parent))
            {
                queryableMenuList = queryableMenuList.Where(menu => menu.Parent == null);
            }
            await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            return await PagedListResponse<ApMenu>.CreateAsync(queryableMenuList, menuParamsRequest.PageNumber, menuParamsRequest.PageSize);
        }
        public async Task<PagedListResponse<ApMenu>> PagingFromCache(MenuParamsRequest menuParamsRequest)
        {
            var cacheName = await _iRedisService.GetAsync(_cacheName);
            List<ApMenu>? menuList = new();
            if (cacheName != null)
            {
                var stringData = Encoding.UTF8.GetString(cacheName);
                menuList = JsonConvert.DeserializeObject<List<ApMenu>>(stringData) ?? [];
                if (!string.IsNullOrEmpty(menuParamsRequest.SearchString))
                {
                    menuList = menuList.Where(menu => menu.Name != null && menu.Name.Contains(menuParamsRequest.SearchString) || menu.Url != null && menu.Url.Contains(menuParamsRequest.SearchString)).ToList();
                }
                if (!string.IsNullOrEmpty(menuParamsRequest.Parent))
                {
                    menuList = menuList.Where(menu => menu.Parent != null && menu.Parent == Convert.ToInt32(menuParamsRequest.Parent)).ToList();
                }
                else
                {
                    menuList = menuList.Where(menu => menu.Parent == null).ToList();
                }
            }
            return await PagedListResponse<ApMenu>.CreateAsync(menuList, menuParamsRequest.PageNumber, menuParamsRequest.PageSize);
        }
        public async Task<ApMenu> CustomMethodToGetMenuById(int id)
        {
            ApMenu? menu = null;
            var cacheName = await _iRedisService.GetAsync(_cacheName);
            List<ApMenu>? menuList = new();
            if (cacheName != null)
            {
                var stringData = Encoding.UTF8.GetString(cacheName);
                menuList = JsonConvert.DeserializeObject<List<ApMenu>>(stringData);
            }
            if (menuList != null && menuList.Any())
            {
                menu = menuList.FirstOrDefault(x => x.Id == id);
            }
            if (menu != null)
            {
                return menu;
            }
            menu = await _dotNetContext.ApMenus.FirstOrDefaultAsync(x => x.Id == id);
            if (menu != null && menuList != null)
            {
                menuList.Add(menu);
                menuList = menuList.OrderBy(x => x.Id).ToList();
                var serializedMenuList = JsonConvert.SerializeObject(menuList, ProjectHelper.GetJsonSettings());
                await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            }
            if (menu == null)
            {
                throw new CustomApplicationException("Menu is not found", StatusCodes.Status404NotFound);
            }
            return menu;
        }
        public async Task CustomMethodToAddMenuAsync(ApMenu menuModel)
        {
            await _dotNetContext.ApMenus.AddAsync(menuModel);
            await _dotNetContext.SaveChangesAsync();

            var cacheName = await _iRedisService.GetAsync(_cacheName);
            List<ApMenu>? menuList = new();
            if (cacheName != null)
            {
                var stringData = Encoding.UTF8.GetString(cacheName);
                menuList = JsonConvert.DeserializeObject<List<ApMenu>>(stringData);
            }
            if (menuList != null && menuList.Any(x => x.Name == menuModel.Name))
            {
                menuList.RemoveAll(m => m.Name == menuModel.Name);
            }
            if (menuModel != null && menuList != null)
            {
                menuList.Add(menuModel);
                menuList = menuList.OrderBy(x => x.Id).ToList();
                var serializedMenuList = JsonConvert.SerializeObject(menuList, ProjectHelper.GetJsonSettings());
                await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            }
        }
        public async Task CustomMethodToUpdateMenuByIdAsync(ApMenu menuModel)
        {
            _dotNetContext.ApMenus.Update(menuModel);

            var cacheName = await _iRedisService.GetAsync(_cacheName);
            List<ApMenu>? menuList = new();

            if (cacheName != null)
            {
                var stringData = Encoding.UTF8.GetString(cacheName);
                menuList = JsonConvert.DeserializeObject<List<ApMenu>>(stringData);
            }
            var existingMenuIndex = -1;
            if (menuList != null && menuList.Any())
            {
                existingMenuIndex = menuList.FindIndex(x => x.Id == menuModel.Id);
            }
            if (menuList != null && existingMenuIndex != -1)
            {
                menuList[existingMenuIndex] = menuModel;
                menuList = menuList.OrderBy(x => x.Id).ToList();
                var serializedMenuList = JsonConvert.SerializeObject(menuList, ProjectHelper.GetJsonSettings());
                await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            }
            else if (menuList != null)
            {
                menuList.Add(menuModel);
                menuList = menuList.OrderBy(x => x.Id).ToList();
                var serializedMenuList = JsonConvert.SerializeObject(menuList, ProjectHelper.GetJsonSettings());
                await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            }
        }
        public async Task CustomMethodToDeleteMenuByIdAsync(ApMenu menuModel)
        {
            _dotNetContext.ApMenus.Remove(menuModel);

            var cacheName = await _iRedisService.GetAsync(_cacheName);
            List<ApMenu>? menuList = new List<ApMenu>();

            if (cacheName != null)
            {
                var stringData = Encoding.UTF8.GetString(cacheName);
                menuList = JsonConvert.DeserializeObject<List<ApMenu>>(stringData);
            }
            var existingMenuIndex = -1;
            if (menuList != null && menuList.Any())
            {
                existingMenuIndex = menuList.FindIndex(x => x.Id == menuModel.Id);
            }
            if (menuList != null && existingMenuIndex != -1)
            {
                menuList.RemoveAt(existingMenuIndex);
                var serializedMenuList = JsonConvert.SerializeObject(menuList, ProjectHelper.GetJsonSettings());
                await _iRedisService.SetAsync(_cacheName, Encoding.UTF8.GetBytes(serializedMenuList));
            }
        }
    }
}
