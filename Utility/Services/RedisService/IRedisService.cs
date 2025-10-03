using Microsoft.Extensions.Caching.Distributed;
namespace Utility.Services.RedisService
{
    public interface IRedisService
    {
        Task SetStringAsync(string key, string value);
        Task SetAsync(string key, byte[] value);
        Task<byte[]?> GetAsync(string key);
        Task RemoveAsync(string key);
    }
    public class RedisService : IRedisService
    {
        private readonly IDistributedCache _cache;
        public RedisService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task SetStringAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            await _cache.SetStringAsync(key, value);
        }
        public async Task SetAsync(string key, byte[] value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
            };
            await _cache.SetAsync(key, value, options);
        }
        public async Task<byte[]?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }
            return await _cache.GetAsync(key);
        }
        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
            }
            await _cache.RemoveAsync(key);
        }
    }
    public class OpRedisService : IRedisService
    {
        public Task SetStringAsync(string key, string value)
        {
            return Task.CompletedTask;
        }
        public Task SetAsync(string key, byte[] value)
        {
            return Task.CompletedTask;
        }
        public Task<byte[]?> GetAsync(string key)
        {
            return Task.FromResult<byte[]?>(null);
        }
        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}
