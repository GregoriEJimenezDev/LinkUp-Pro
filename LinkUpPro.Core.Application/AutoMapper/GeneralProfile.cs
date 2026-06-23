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
            //battleship
            CreateMap<BattleshipGame, GameDto>()
                .ForMember(d => d.Player1Username, opt => opt.Ignore())
                .ForMember(d => d.Player2Username, opt => opt.Ignore())
                .ForMember(d => d.WinnerUsername, opt => opt.Ignore())
                .ForMember(d => d.HoursElapsed, opt => opt.Ignore());

            //ship
            CreateMap<Ship, ShipDto>()
                .ForMember(d => d.Cells, opt => opt.MapFrom(s => s.Cells));

            //shipcell
            CreateMap<ShipCell, CellDto>()
                .ForMember(d => d.Column, opt => opt.MapFrom(s => s.Column));

            //attack
            CreateMap<Attack, AttackDto>();

            //post
            CreateMap<Post, PostViewModel>()
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.UserProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.LikesCount, opt => opt.Ignore())
                .ForMember(d => d.DislikesCount, opt => opt.Ignore())
                .ForMember(d => d.CurrentUserReaction, opt => opt.Ignore())
                .ForMember(d => d.Comments, opt => opt.Ignore());

            //savepostviewmodel
            CreateMap<SavePostViewModel, Post>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForMember(d => d.CreatedAt, opt => opt.Ignore())
                .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.MediaUrl, opt => opt.Ignore())
                .ForMember(d => d.Comments, opt => opt.Ignore())
                .ForMember(d => d.Reactions, opt => opt.Ignore());

            //friends
            CreateMap<Friendship, FriendDto>()
                .ForMember(d => d.UserId, opt => opt.Ignore())
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.FullName, opt => opt.Ignore())
                .ForMember(d => d.ProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.FriendshipId, opt => opt.MapFrom(s => s.Id));
            
            //friendrequest
            CreateMap<FriendRequest, FriendRequestDto>()
                .ForMember(d => d.SenderUsername, opt => opt.Ignore())
                .ForMember(d => d.SenderProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.ReceiverUsername, opt => opt.Ignore())
                .ForMember(d => d.MutualFriendsCount, opt => opt.Ignore());

            //comment
            CreateMap<Comment, CommentDto>()
                .ForMember(d => d.Username, opt => opt.Ignore())
                .ForMember(d => d.UserProfilePicture, opt => opt.Ignore())
                .ForMember(d => d.Replies, opt => opt.MapFrom(s => s.Replies));
        }
    }
}
