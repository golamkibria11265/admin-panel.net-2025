using Bll.Services.ApService;
using Bll.Services.CommonService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bll.Extensions
{
    public static class BllApServiceExtension
    {
        public static IServiceCollection AddBllApServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            #region Ap Injection
            _iServiceCollection.AddScoped<IMenuService, MenuService>();
            _iServiceCollection.AddScoped<IUserService, UserService>();
            _iServiceCollection.AddScoped<IPermissionService, PermissionService>();
            _iServiceCollection.AddScoped<IRolePermissionService, RolePermissionService>();
            _iServiceCollection.AddScoped<IRoleService, RoleService>();
            _iServiceCollection.AddScoped<IUserRoleService, UserRoleService>();
            _iServiceCollection.AddScoped<ICommonService, CommonService>();
            #endregion
            return _iServiceCollection;
        }   
    }
}
