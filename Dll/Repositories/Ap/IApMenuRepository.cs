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

    /// <summary>
    /// The cache key "MenuRepository" always holds the COMPLETE menu list (written only by RepositoryBase.FindAll).
    /// Every change removes the key so the next read reloads it from the database. Partial or filtered lists are
    /// never written to the cache.
    /// </summary>
    public class MenuRepository : RepositoryBase<ApMenu>, IApMenuRepository
    {
        private const string CacheName = "MenuRepository";
        private readonly DotNetContext _dotNetContext;
        private readonly IRedisService _iRedisService;

        public MenuRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, CacheName)
        {
            _dotNetContext = dotNetContext;
            _iRedisService = iRedisService;
        }

        public async Task<PagedListResponse<ApMenu>> CustomMethodToGetAllMenus(MenuParamsRequest menuParamsRequest)
        {
            int pageNumber = menuParamsRequest.PageNumber < 1 ? 1 : menuParamsRequest.PageNumber;
            int pageSize = menuParamsRequest.PageSize < 1 ? 10 : menuParamsRequest.PageSize;

            IQueryable<ApMenu> query = _dotNetContext.ApMenus.AsNoTracking();
            if (!string.IsNullOrEmpty(menuParamsRequest.SearchString))
            {
                string search = menuParamsRequest.SearchString;
                query = query.Where(menu => (menu.Name != null && menu.Name.Contains(search)) || (menu.Url != null && menu.Url.Contains(search)));
            }
            if (!string.IsNullOrEmpty(menuParamsRequest.Parent) && int.TryParse(menuParamsRequest.Parent, out int parentId) && parentId > 0)
            {
                query = query.Where(menu => menu.Parent == parentId);
            }
            else
            {
                query = query.Where(menu => menu.Parent == null);
            }
            // A stable order is required for Skip/Take paging, otherwise rows repeat or go missing between pages.
            query = query.OrderBy(menu => menu.MenuOrder).ThenBy(menu => menu.Id);
            return await PagedListResponse<ApMenu>.CreateAsync(query, pageNumber, pageSize);
        }

        public async Task<ApMenu> CustomMethodToGetMenuById(int id)
        {
            var cached = await _iRedisService.GetAsync(CacheName);
            if (cached != null)
            {
                var menuList = JsonConvert.DeserializeObject<List<ApMenu>>(Encoding.UTF8.GetString(cached), ProjectHelper.GetJsonSettings());
                var fromCache = menuList?.FirstOrDefault(x => x.Id == id);
                if (fromCache != null)
                {
                    return fromCache;
                }
            }
            var menu = await _dotNetContext.ApMenus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (menu == null)
            {
                throw new CustomApplicationException("Menu is not found", StatusCodes.Status404NotFound);
            }
            return menu;
        }

        public async Task CustomMethodToAddMenuAsync(ApMenu menuModel)
        {
            await _iRedisService.RemoveAsync(CacheName);
            await _dotNetContext.ApMenus.AddAsync(menuModel);
        }

        public async Task CustomMethodToUpdateMenuByIdAsync(ApMenu menuModel)
        {
            await _iRedisService.RemoveAsync(CacheName);
            _dotNetContext.ApMenus.Update(menuModel);
        }

        public async Task CustomMethodToDeleteMenuByIdAsync(ApMenu menuModel)
        {
            await _iRedisService.RemoveAsync(CacheName);
            _dotNetContext.ApMenus.Remove(menuModel);
        }
    }
}
