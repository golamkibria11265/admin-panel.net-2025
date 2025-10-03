using Dll.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Text;
using Newtonsoft.Json;
using Utility;
using Utility.Services.RedisService;

namespace Dll.Repositories
{
    public interface IRepositoryBase<T> where T : class
    {
        IQueryable<T> FindAllQueryable();
        IQueryable<T> FindAllQueryable(Expression<Func<T, bool>> expression);
        Task<List<T>> FindAll();
        Task<List<T>> FindByCondition(Expression<Func<T, bool>> expression);
        Task Create(T entity);
        Task CreateRange(List<T> entity);
        Task Update(T entity);
        Task UpdateRange(List<T> entryList);
        Task Delete(T entity);
        Task DeleteRange(List<T> entryList);
        Task<T?> FindFirstOrDefault(Expression<Func<T, bool>> expression);
        Task<T?> FindLastOrDefault(Expression<Func<T, bool>> expression);
        Task<T?> FindFirstOrDefaultDB(Expression<Func<T, bool>> expression);
        Task<T?> GetById(int id);
        Task<T?> GetByCondition(Expression<Func<T, bool>> expression);
        void Detach(T entity);
    }
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly DotNetContext _dotNetContext;
        private readonly string? _cacheName;
        private readonly IRedisService _iRedisService;

        protected RepositoryBase(DotNetContext dotNetContext, IRedisService iRedisService, string? cacheName = null)
        {
            _dotNetContext = dotNetContext;
            _cacheName = cacheName;
            _iRedisService = iRedisService;
        }
        public virtual IQueryable<T> FindAllQueryable()
        {
            return _dotNetContext.Set<T>().AsNoTracking();
        }
        public IQueryable<T> FindAllQueryable(Expression<Func<T, bool>> expression)
        {
            return _dotNetContext.Set<T>().Where(expression).AsNoTracking();
        }
        public virtual async Task<List<T>> FindAll()
        {
            if (_cacheName == null)
            {
                return await _dotNetContext.Set<T>().AsNoTracking().ToListAsync();
            }
            var value = await _iRedisService.GetAsync(_cacheName);
            if (value != null)
            {
                var stringData = Encoding.UTF8.GetString(value);
                var list = JsonConvert.DeserializeObject<List<T>>(stringData, ProjectHelper.GetJsonSettings());
                if (list != null)
                {
                    return list;
                }
                return [];
            }
            var info = await _dotNetContext.Set<T>().AsNoTracking().ToListAsync();

            await _iRedisService.SetStringAsync(_cacheName, JsonConvert.SerializeObject(info, ProjectHelper.GetJsonSettings()));
            return info;
        }
        public virtual async Task<List<T>> FindByCondition(Expression<Func<T, bool>> expression)
        {
            if (_cacheName == null)
            {
                return await _dotNetContext.Set<T>().Where(expression).AsNoTracking().ToListAsync();
            }
            var value = await _iRedisService.GetAsync(_cacheName);
            if (value != null)
            {
                var stringData = Encoding.UTF8.GetString(value);
                var list = JsonConvert.DeserializeObject<List<T>>(stringData, ProjectHelper.GetJsonSettings());
                if (list != null)
                {
                    return list.AsQueryable().Where(expression).ToList();
                }
                return [];
            }
            else
            {
                var data = await FindAll();
                return data.AsQueryable().Where(expression).ToList();
            }
        }
        public virtual async Task Create(T entity)
        {
            await RemoveDataCache();
            await _dotNetContext.Set<T>().AddAsync(entity);
        }
        public virtual async Task CreateRange(List<T> entity)
        {
            await RemoveDataCache();
            await _dotNetContext.Set<T>().AddRangeAsync(entity);
        }
        public virtual async Task Update(T entity)
        {
            await RemoveDataCache();
            _dotNetContext.Set<T>().Update(entity);
        }
        public virtual async Task UpdateRange(List<T> entryList)
        {
            await RemoveDataCache();
            _dotNetContext.Set<T>().UpdateRange(entryList);
        }
        public virtual async Task Delete(T entity)
        {
            await RemoveDataCache();
            _dotNetContext.Set<T>().Remove(entity);
        }
        public virtual async Task DeleteRange(List<T> entryList)
        {
            await RemoveDataCache();
            _dotNetContext.Set<T>().RemoveRange(entryList);
        }
        public virtual async Task<T?> FindFirstOrDefault(Expression<Func<T, bool>> expression)
        {
            if (_cacheName == null)
            {
                return await _dotNetContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(expression);
            }
            var result = await FindByCondition(expression);
            return result.FirstOrDefault();
        }
        public virtual async Task<T?> FindLastOrDefault(Expression<Func<T, bool>> expression)
        {
            return await _dotNetContext.Set<T>().AsNoTracking().LastOrDefaultAsync(expression);
        }
        public virtual async Task<T?> GetById(int id)
        {
            return await _dotNetContext.Set<T>().FindAsync(id);
        }
        public virtual async Task<T?> GetByCondition(Expression<Func<T, bool>> expression)
        {
            return await _dotNetContext.Set<T>().Where(expression).AsNoTracking().FirstOrDefaultAsync();
        }
        public void Detach(T entity)
        {
            _dotNetContext.Entry(entity).State = EntityState.Detached;
        }
        private async Task RemoveDataCache()
        {
            if (_cacheName != null)
            {
                await _iRedisService.RemoveAsync(_cacheName);
            }
        }
        public virtual async Task<T?> FindFirstOrDefaultDB(Expression<Func<T, bool>> expression)
        {
            return await _dotNetContext.Set<T>().AsNoTracking().FirstOrDefaultAsync(expression);
        }
    }
}
