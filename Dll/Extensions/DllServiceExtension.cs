using Dll.Data;
using Dll.UnitOfWorks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dll.Extensions
{
    public static class DllServiceExtension
    {
        public static IServiceCollection AddDataLogicLayerServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            _iServiceCollection.AddDbContext<DotNetContext>(options =>
            {
                options.UseSqlServer(_iConfiguration.GetConnectionString("DefaultConnection"));
            });

            #region Repository Injection
            _iServiceCollection.AddScoped<IUnitOfWork, UnitOfWork>();
            #endregion

            #region Ap Injection
            _iServiceCollection.AddDllApServices(_iConfiguration);
            #endregion

            return _iServiceCollection;
        }
    }
}
