using Dll.Data;
using Dll.Repositories.Ap;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Utility.Services.RedisService;

namespace Dll.UnitOfWorks
{
    public interface IUnitOfWork
    {
        #region Ap Declaration
        IApMenuRepository MenuRepository { get; }
        IApUserRepository UserRepository { get; }
        IApPermissionRepository PermissionRepository { get; }
        IApRolePermissionRepository RolePermissionRepository { get; }
        IApRoleRepository RoleRepository { get; }
        IApUserRoleRepository UserRoleRepository { get; }
        IApOtpHistoryRepository OtpHistoryRepository { get; }
        #endregion


        Task<int> CompleteAsync();
        int Complete();
        void ResetContextState();
    }
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly DotNetContext _dotNetContext;
        private readonly IConfiguration _iConfiguration;
        private readonly IRedisService _iRedisService;
        public UnitOfWork(DotNetContext dotNetContext, IConfiguration iConfiguration, IRedisService iRedisService)
        {
            _dotNetContext = dotNetContext;
            _iConfiguration = iConfiguration;
            _iRedisService = iRedisService;
        }
        #region Ap Declaration
        private IApMenuRepository? _menuRepository;
        public IApMenuRepository MenuRepository => _menuRepository ?? (_menuRepository = new MenuRepository(_dotNetContext, _iRedisService));

        private IApUserRepository? _userRepository;
        public IApUserRepository UserRepository => _userRepository ?? (_userRepository = new UserRepository(_dotNetContext, _iRedisService));

        private IApPermissionRepository? _permissionRepository;
        public IApPermissionRepository PermissionRepository => _permissionRepository ?? (_permissionRepository = new PermissionRepository(_dotNetContext, _iRedisService));

        private IApRolePermissionRepository? _rolePermissionRepository;
        public IApRolePermissionRepository RolePermissionRepository => _rolePermissionRepository ?? (_rolePermissionRepository = new RolePermissionRepository(_dotNetContext, _iRedisService));

        private IApRoleRepository? _roleRepository;
        public IApRoleRepository RoleRepository => _roleRepository ?? (_roleRepository = new RoleRepository(_dotNetContext, _iRedisService));

        private IApUserRoleRepository? _userRoleRepository;
        public IApUserRoleRepository UserRoleRepository => _userRoleRepository ?? (_userRoleRepository = new UserRoleRepository(_dotNetContext, _iRedisService));

        private IApOtpHistoryRepository? _otpHistoryRepository;
        public IApOtpHistoryRepository OtpHistoryRepository => _otpHistoryRepository ?? (_otpHistoryRepository = new OtpHistoryRepository(_dotNetContext, _iRedisService));
        #endregion
        public int Complete()
        {
            return 1;
        }

        public async Task<int> CompleteAsync()
        {
            return await _dotNetContext.SaveChangesAsync();
        }

        public void ResetContextState() => _dotNetContext.ChangeTracker.Entries().Where(e => e.Entity != null).ToList().ForEach(e => e.State = EntityState.Detached);

        public void Dispose() => _dotNetContext.Dispose();
    }
}
