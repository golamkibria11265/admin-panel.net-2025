using Dll.Data;
using Dll.Data.Entities;
using Utility.Services.RedisService;

namespace Dll.Repositories.Ap
{
    public interface IApUserRepository : IRepositoryBase<ApUser>
    {

    }
    public class UserRepository : RepositoryBase<ApUser>, IApUserRepository
    {
        private readonly DotNetContext _dotNetContext;
        private readonly IRedisService _iRedisService;
        // No cache name on purpose: the user table holds password hashes and account status, which must never be
        // copied into Redis and must always be read fresh (a disabled user must be blocked immediately).
        public UserRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, null)
        {
            _dotNetContext = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
