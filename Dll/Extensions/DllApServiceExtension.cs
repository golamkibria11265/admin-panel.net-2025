using Dll.Repositories.Ap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dll.Extensions
{
    public static class DllApServiceExtension
    {
        public static IServiceCollection AddDllApServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            #region Ap Injection
            _iServiceCollection.AddScoped<IApMenuRepository, MenuRepository>();
            _iServiceCollection.AddScoped<IApUserRepository, UserRepository>();
            _iServiceCollection.AddScoped<IApPermissionRepository, PermissionRepository>();
            _iServiceCollection.AddScoped<IApRolePermissionRepository, RolePermissionRepository>();
            _iServiceCollection.AddScoped<IApRoleRepository, RoleRepository>();
            _iServiceCollection.AddScoped<IApUserRoleRepository, UserRoleRepository>();
            _iServiceCollection.AddScoped<IApOtpHistoryRepository, OtpHistoryRepository>();
            #endregion
            return _iServiceCollection;
        }
    }
}
