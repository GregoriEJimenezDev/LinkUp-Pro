using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Infrastructure.Identity.Context;
using LinkUpPro.Infrastructure.Identity.Entities;
using LinkUpPro.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Identity.IoC
{
    public static class ServiceRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            #region Authentication & Cookies
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/Auth/Index";
                options.AccessDeniedPath = "/Auth/AccessDenied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.Events.OnSigningIn = context =>
                {
                    if (context.Properties.IsPersistent)
                    {
                        context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
                    }
                    return Task.CompletedTask;
                };
            });
            #endregion

            #region Identity
            services.AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.Password.RequireDigit = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireUppercase = true;
                opts.Password.RequireLowercase = true;
                opts.User.RequireUniqueEmail = true;

                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<IdentityProContext>()
            .AddSignInManager<SignInManager<ApplicationUser>>()
            .AddDefaultTokenProviders();
            #endregion

            #region Services Registration
            services.AddHttpContextAccessor();
            services.AddTransient<IUserService, AuthServices>();
            #endregion
        }

        #region private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Context
            bool useInMemory = config["UseInMemoryDatabase"] == "True";

            if (useInMemory)
            {
                services.AddDbContext<IdentityProContext>(opt => opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("SupabaseConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.");
                }

                services.AddDbContext<IdentityProContext>(opt =>
                {
                    opt.EnableSensitiveDataLogging();
                    opt.UseNpgsql(connectionString, m => m.MigrationsAssembly(typeof(IdentityProContext).Assembly.FullName));
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Scoped);
            }
            #endregion
        }
        #endregion
    }
}
