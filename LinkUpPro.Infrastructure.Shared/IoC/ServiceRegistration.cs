using LinkUpPro.Core.Application.AutoMapper;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Infrastructure.Shared.Services;
using LinkUpPro.Infrastructure.Shared.EmailServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Shared.IoC
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services,config);
        }
        #region private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Mapper
            services.AddAutoMapper(cfg => { }, typeof(GeneralProfile));
            
            services.Configure<EmailSettings>(options => 
            {
                options.SmtpHost = config["EmailSettings:SmtpHost"] ?? "";
                options.SmtpPort = int.TryParse(config["EmailSettings:SmtpPort"], out var port) ? port : 0;
                options.SmtpUser = config["EmailSettings:SmtpUser"] ?? "";
                options.SmtpPassword = config["EmailSettings:SmtpPassword"] ?? "";
                options.FromEmail = config["EmailSettings:FromEmail"] ?? "";
                options.FromName = config["EmailSettings:FromName"] ?? "";
                options.UseSsl = bool.TryParse(config["EmailSettings:UseSsl"], out var useSsl) ? useSsl : false;
            });
            services.AddScoped<IEmailService, EmailService>();
            
            services.AddScoped<IFileStorageService, SupabaseStorageService>();
            
            var url = config["Supabase:Url"];
            var key = config["Supabase:Key"];
            if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(key))
            {
                var options = new Supabase.SupabaseOptions
                {
                    AutoConnectRealtime = false
                };
                services.AddScoped<Supabase.Client>(_ => new Supabase.Client(url, key, options));
            }
            #endregion
        }
        #endregion
    }
}
