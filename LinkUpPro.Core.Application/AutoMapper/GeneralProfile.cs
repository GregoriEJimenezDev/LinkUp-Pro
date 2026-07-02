using AutoMapper;
using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.DTOs.Comment;
using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.ViewModel.Save;
using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Application.AutoMapper
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile() 
        {
            CreateMap<BattleshipGame, GameDto>()
                .ForMember(d => d.Player1Id, opt => opt.MapFrom(s => s.FirstPlayerId))
                .ForMember(d => d.Player2Id, opt => opt.MapFrom(s => s.SecondPlayerId))
                .ForMember(d => d.Player1Username, opt => opt.Ignore())
                .ForMember(d => d.Player2Username, opt => opt.Ignore())
                .ForMember(d => d.WinnerUsername, opt => opt.Ignore())
                .ForMember(d => d.HoursElapsed, opt => opt.Ignore());

            CreateMap<Ship, ShipDto>()
                .ForMember(d => d.Cells, opt => opt.MapFrom(s => s.Cells));

            CreateMap<ShipCell, CellDto>()
                .ForMember(d => d.Column, opt => opt.MapFrom(s => s.Column));

            CreateMap<Attack, AttackDto>();

            CreateMap<Post, PostViewModel>()
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.UserProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.LikesCount, opt => opt.Ignore())
                .ForMember(d => d.DislikesCount, opt => opt.Ignore())
                .ForMember(d => d.CurrentUserReaction, opt => opt.Ignore())
                .ForMember(d => d.Comments, opt => opt.Ignore());

            CreateMap<SavePostViewModel, Post>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.MediaUrl, opt => opt.Ignore())
                .ForMember(d => d.Comments, opt => opt.Ignore())
                .ForMember(d => d.Reactions, opt => opt.Ignore());

            CreateMap<Friendship, FriendDto>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.FullName, opt => opt.Ignore())
                .ForMember(d => d.ProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.FriendshipId, opt => opt.MapFrom(s => s.Id));
            
            CreateMap<FriendRequest, FriendRequestDto>()
                .ForMember(d => d.SenderUsername, opt => opt.Ignore())
                .ForMember(d => d.SenderProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.ReceiverUsername, opt => opt.Ignore())
                .ForMember(d => d.MutualFriendsCount, opt => opt.Ignore());

            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.UserProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
        }
    }
}
