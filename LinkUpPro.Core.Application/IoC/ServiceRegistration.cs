using LinkUpPro.Core.Application.AutoMapper;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.Interfaces.Services;
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
            #endregion
        }
        #endregion
    }
}
