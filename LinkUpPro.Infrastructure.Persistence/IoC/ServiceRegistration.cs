using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Domain.DomainServices;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;
using LinkUpPro.Infrastructure.Persistence.Context;
using LinkUpPro.Infrastructure.Persistence.Repositories;
using LinkUpPro.Infrastructure.Persistence.Repositories.Generic;
using LinkUpPro.Infrastructure.Persistence.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Infrastructure.Persistence.IoC
{
    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);
        }

        #region private methods
        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Context
            bool useInMemory = config["UseInMemoryDatabase"] == "True";

            if (useInMemory)
            {
                services.AddDbContext<LinkUpProDbContext>(opt =>
                    opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("SupabaseConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'SupabaseConnection' not found.");
                }

                services.AddDbContext<LinkUpProDbContext>(opt =>
                {
                    opt.EnableSensitiveDataLogging();
                    opt.UseNpgsql(connectionString, m => m.MigrationsAssembly(typeof(LinkUpProDbContext).Assembly.FullName));
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Scoped);
            }
            #endregion

            #region IOC
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IPostRepository, PostRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IReactionRepository, ReactionRepository>();
            services.AddScoped<IFriendRequestRepository, FriendRequestRepository>();
            services.AddScoped<IFriendshipRepository, FriendshipRepository>();
            services.AddScoped<IBattleshipGameRepository, BattleshipGameRepository>();
            services.AddScoped<IShipRepository, ShipRepository>();
            services.AddScoped<IAttackRepository, AttackRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IUnitOfWork, EFUnitOfWork>();
            services.AddScoped<IShipPlacementDomainService, ShipPlacementDomainService>();
            services.AddScoped<IAttackDomainService, AttackDomainService>();
            #endregion
        }
        #endregion
    }
}
