using LinkUpPro.Core.Application.AutoMapper;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Infrastructure.Shared.EmailServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Shared.IoC
{
    public static class ServiceRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services,config);
        }
        #region private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Mapper
            services.AddAutoMapper(cfg => { }, typeof(GeneralProfile));
            
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }
        #endregion
    }
}
