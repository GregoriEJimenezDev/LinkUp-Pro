using LinkUpPro.Core.Application.AutoMapper;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.Services;
using LinkUpPro.Core.Domain.DomainServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LinkUpPro.Core.Application.IoC
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
            
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<ICommentService, CommentService>();
            services.AddScoped<IReactionService, ReactionService>();
            services.AddScoped<IFriendRequestService, FriendRequestService>();
            services.AddScoped<IFriendshipService, FriendshipService>();
            services.AddScoped<IBattleshipService, BattleshipService>();
            services.AddScoped<INotificationService, NotificationService>();

            #region Domain Services
            services.AddScoped<IShipPlacementDomainService, ShipPlacementDomainService>();
            services.AddScoped<IAttackDomainService, AttackDomainService>();
            #endregion
            #endregion
        }
        #endregion
    }
}
