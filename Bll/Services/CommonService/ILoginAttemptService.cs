using Microsoft.Extensions.Caching.Memory;

namespace Bll.Services.CommonService
{
    /// <summary>
    /// Brute-force protection for the login form (PCI-DSS 8.3.4):
    /// a user name is locked for 30 minutes after 10 failed attempts, and a single IP address
    /// is throttled after 50 failed attempts in 30 minutes (stops guessing across many user names).
    /// Counters live in memory, so they reset when the application restarts.
    /// </summary>
    public interface ILoginAttemptService
    {
        bool IsLockedOut(string userName, string? ipAddress);
        void RegisterFailure(string userName, string? ipAddress);
        void RegisterSuccess(string userName);
        TimeSpan LockoutDuration { get; }
    }

    public class LoginAttemptService : ILoginAttemptService
    {
        private const int MaxFailuresPerUser = 10;
        private const int MaxFailuresPerIp = 50;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(30);
        private readonly IMemoryCache _cache;
        private readonly object _sync = new();

        public LoginAttemptService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public TimeSpan LockoutDuration => Window;

        public bool IsLockedOut(string userName, string? ipAddress)
        {
            return GetCount(UserKey(userName)) >= MaxFailuresPerUser
                || (!string.IsNullOrEmpty(ipAddress) && GetCount(IpKey(ipAddress)) >= MaxFailuresPerIp);
        }

        public void RegisterFailure(string userName, string? ipAddress)
        {
            Increment(UserKey(userName));
            if (!string.IsNullOrEmpty(ipAddress))
            {
                Increment(IpKey(ipAddress));
            }
        }

        public void RegisterSuccess(string userName)
        {
            _cache.Remove(UserKey(userName));
        }

        private int GetCount(string key) => _cache.TryGetValue(key, out int count) ? count : 0;

        private void Increment(string key)
        {
            lock (_sync)
            {
                int count = GetCount(key) + 1;
                // The window restarts with every failure, so the lock lasts 30 minutes after the LAST attempt.
                _cache.Set(key, count, Window);
            }
        }

        private static string UserKey(string userName) => "login-fail:user:" + (userName ?? string.Empty).Trim().ToLowerInvariant();
        private static string IpKey(string ipAddress) => "login-fail:ip:" + ipAddress;
    }
}
