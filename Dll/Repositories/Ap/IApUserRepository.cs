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
        public UserRepository(DotNetContext dotNetContext, IRedisService iRedisService) : base(dotNetContext, iRedisService, "UserRepository")
        {
            _dotNetContext = dotNetContext;
            _iRedisService = iRedisService;
        }
    }
}
