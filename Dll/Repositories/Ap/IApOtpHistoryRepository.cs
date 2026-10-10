using Dll.Data.Entities;
using Dll.Data;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApOtpHistoryRepository : IRepositoryBase<ApOtpHistory>
    {
    }
    public class OtpHistoryRepository : RepositoryBase<ApOtpHistory>, IApOtpHistoryRepository
    {
        private readonly DotNetContext _context;
        private readonly IRedisService _iRedisService;
        public OtpHistoryRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, null) // OTPs are secrets: never cached
        {
            _context = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
