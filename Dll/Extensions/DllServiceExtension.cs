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
            string? connectionString = _iConfiguration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is not configured. Set the environment variable " +
                    "ConnectionStrings__DefaultConnection (never put the password in appsettings.json).");
            }
            _iServiceCollection.AddDbContext<DotNetContext>(options =>
            {
                options.UseSqlServer(connectionString);
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
