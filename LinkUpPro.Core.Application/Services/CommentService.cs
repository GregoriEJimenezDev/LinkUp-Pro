using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Services
{
    public class CommentService(ICommentRepository commentRepo, INotificationService notificationService,
        IPostRepository postRepository, IUserService userService, IFriendshipRepository friendshipRepo) : ICommentService
    {
        private readonly ICommentRepository _commentRepo = commentRepo;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IPostRepository _postRepository = postRepository;
        private readonly IUserService _userService = userService;
        private readonly IFriendshipRepository _friendshipRepo = friendshipRepo;

        public async Task<ServiceResult> AddAsync(SaveCommentViewModel vm, string userId)
        {
            var post = await _postRepository.GetByIdAsync(vm.PostId);
            if (post == null) return ServiceResult.Failure("Publicación no encontrada.");
            
            if (!post.AllowComments)
                return ServiceResult.Failure("Esta publicación no permite comentarios.");

            if (post.UserId != userId)
            {
                if (post.Privacy == PostPrivacy.OnlyMe)
                {
                    return ServiceResult.Failure("No estás autorizado para comentar en esta publicación.");
                }

                if (post.Privacy == PostPrivacy.FriendsOnly)
                {
                    bool areFriends = await _friendshipRepo.AreFriendsAsync(userId, post.UserId!);
                    if (!areFriends)
                    {
                        return ServiceResult.Failure("No estás autorizado para comentar en esta publicación.");
                    }
                }
            }

            var comment = new Comment
            {
                Content = vm.Content,
                PostId = vm.PostId,
                UserId = userId,
                ParentCommentId = vm.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);

            var commenterInfo = await _userService.GetUserBasicInfoAsync(userId);

            if (vm.ParentCommentId.HasValue)
            {
                var parentComment = await _commentRepo.GetByIdAsync(vm.ParentCommentId.Value);
                if (parentComment != null && parentComment.UserId != userId)
                {
                    await _notificationService.CreateNotificationAsync(
                        parentComment.UserId!,
                        "Nueva respuesta",
                        $"{commenterInfo.Username} respondió a tu comentario.",
                        $"/Post/Details/{parentComment.PostId}",
                        NotificationType.CommentReply
                    );
                }
            }
            else
            {
                if (post != null && post.UserId != userId)
                {
                    await _notificationService.CreateNotificationAsync(
                        post.UserId!,
                        "Nuevo comentario",
                        $"{commenterInfo.Username} comentó tu publicación.",
                        $"/Post/Details/{post.Id}",
                        NotificationType.PostComment
                    );
                }
            }

            return ServiceResult.Success();
        }
        public async Task<ServiceResult> DeleteAsync(int commentId, string userId)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return ServiceResult.Failure("Comentario no encontrado.");

            if (comment.UserId != userId)
                return ServiceResult.Failure("No estás autorizado para eliminar este comentario.");

            comment.IsDeleted = true;
            await _commentRepo.UpdateAsync(comment);
            
            return ServiceResult.Success();
        }
        public async Task<ServiceResult> UpdateAsync(SaveCommentViewModel vm, string userId)
        {
            var comment = await _commentRepo.GetByIdAsync(vm.Id);
            if (comment == null)
                return ServiceResult.Failure("Comentario no encontrado.");

            if (comment.UserId != userId)
                return ServiceResult.Failure("No estás autorizado para actualizar este comentario.");

            comment.Content = vm.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepo.UpdateAsync(comment);

            return ServiceResult.Success();
        }
    }
}
