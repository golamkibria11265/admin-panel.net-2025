using Bll.Services.CommonService;
using Bll.Services.Queue;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bll.Extensions
{
    public static class BllServiceExtension
    {
        public static IServiceCollection AddBusinessLogicLayerServices(this IServiceCollection _iServiceCollection, IConfiguration _iConfiguration)
        {
            _iServiceCollection.AddScoped<IFileWriterService, FileWriterService>();
            _iServiceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            #region DataSeed Injection
            _iServiceCollection.AddScoped<IDataSeedingService, DataSeedingService>();
            #endregion

            #region Ap Injection
            _iServiceCollection.AddBllApServices(_iConfiguration);
            #endregion

            _iServiceCollection.AddScoped<IIdentityService, IdentityService>();

            return _iServiceCollection;
        }   
    }
}
