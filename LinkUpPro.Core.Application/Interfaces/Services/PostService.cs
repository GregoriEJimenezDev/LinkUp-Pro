using LinkUpPro.Core.Application.DTOs.Comment;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.ViewModel.Save;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class PostService(IPostRepository postRepository, IReactionRepository reactionRepository,
        IFriendshipRepository friendshipRepository, IUserService userService) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository;
        private readonly IReactionRepository _reactionRepository = reactionRepository;
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly IUserService _userService = userService;

        public async Task<ServiceResult> CreateAsync(SavePostViewModel vm, string userId)
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
                return ServiceResult.Failure("El contenido de la publicación no puede estar vacío.");

            bool hasImage = vm.ImageFile != null && vm.ImageFile.Length > 0;
            bool hasVideo = !string.IsNullOrWhiteSpace(vm.VideoUrl);

            if (hasImage && hasVideo)
                return ServiceResult.Failure("Solo puedes adjuntar una imagen o un video de YouTube, no ambos.");
            if (!hasImage && !hasVideo)
                return ServiceResult.Failure("Debes proporcionar exactamente un contenido multimedia (Imagen o Video de YouTube).");

            if (vm.MediaType == MediaType.Image && !hasImage)
                return ServiceResult.Failure("Debes proporcionar un archivo de imagen para las publicaciones de tipo imagen.");
            if (vm.MediaType == MediaType.Video && !hasVideo)
                return ServiceResult.Failure("Debes proporcionar una URL de video para las publicaciones de tipo video.");

            string? mediaUrl = null;

            if (vm.MediaType == MediaType.Image && vm.ImageFile != null)
            {
                string extension = ".jpg";
                mediaUrl = await SaveImageAsync(vm.ImageFile, extension);
            }
            else if (vm.MediaType == MediaType.Video && !string.IsNullOrEmpty(vm.VideoUrl))
            {
                mediaUrl = ExtractYouTubeEmbedUrl(vm.VideoUrl);
                if (mediaUrl == null)
                    return ServiceResult.Failure("El formato de la URL de YouTube no es válido.");
            }

            var post = new Post
            {
                Content = vm.Content,
                MediaType = vm.MediaType,
                MediaUrl = mediaUrl,
                Privacy = vm.Privacy,
                AllowComments = vm.AllowComments,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.AddAsync(post);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> DeleteAsync(int postId, string userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return ServiceResult.Failure("Publicación no encontrada.");
            if (post.UserId != userId)
                return ServiceResult.Failure("No estás autorizado para eliminar esta publicación.");

            await _postRepository.DeleteAsync(post);
            return ServiceResult.Success();
        }

        public async Task<List<PostViewModel>> GetByFriendsAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
            var friendIds = friendships.Select(f => f.FirstUserId == userId ? f.SecondUserId : f.FirstUserId)
                .Where(id => id != null).ToList();

            var allposts = await _postRepository.GetAllPostWithDetailsAsync();
            
            // Feed Principal: Tus posts + Posts Públicos + Posts "Solo Amigos" (validando que la amistad esté Activa)
            var feedPosts = allposts.Where(p => 
                p.UserId == userId || 
                p.Privacy == PostPrivacy.Public || 
                (p.Privacy == PostPrivacy.FriendsOnly && friendIds.Contains(p.UserId))
            ).OrderByDescending(p => p.CreatedAt).ToList();

            var result = new List<PostViewModel>();

            foreach (var post in feedPosts)
            {
                var authorInfo = await _userService.GetUserBasicInfoAsync(post.UserId!);
                var vm = await MapSinglePost(post, userId, authorInfo);
                result.Add(vm);
            }

            return result;
        }

        public async Task<SavePostViewModel> GetByIdForEditAsync(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return new SavePostViewModel();

            return new SavePostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                MediaType = post.MediaType,
                Privacy = post.Privacy,
                AllowComments = post.AllowComments,
                VideoUrl = post.MediaType == MediaType.Video ? post.MediaUrl : null
            };
        }

        public async Task<List<PostViewModel>> GetByUserAsync(string targetUserId, string currentUserId)
        {
            var posts = await _postRepository.GetByUserIdAsync(targetUserId);
            var userInfo = await _userService.GetUserBasicInfoAsync(targetUserId);
            return await MapToViewModels(posts, currentUserId, userInfo);
        }

        public async Task<ServiceResult> UpdateAsync(SavePostViewModel vm, string userId)
        {
            var post = await _postRepository.GetByIdAsync(vm.Id);

            if (post == null) return ServiceResult.Failure("Publicación no encontrada.");
            if (post.UserId != userId) return ServiceResult.Failure("No estás autorizado para editar esta publicación.");

            if (string.IsNullOrWhiteSpace(vm.Content))
                return ServiceResult.Failure("El contenido de la publicación no puede estar vacío.");

            bool hasImage = vm.ImageFile != null && vm.ImageFile.Length > 0;
            bool hasVideo = !string.IsNullOrWhiteSpace(vm.VideoUrl);
            
            // Si el post actual ya tiene imagen, y no suben una nueva, consideramos que sigue teniendo imagen.
            // Si el post actual tiene video, y no mandan URL, se quita. 
            // Para simplificar, forzamos a que si cambian de tipo, envíen el nuevo archivo/url.
            // Si mantienen el tipo y no envían nada (en caso de imagen), conserva la anterior.
            bool isKeepingExistingImage = (vm.MediaType == MediaType.Image && post.MediaType == MediaType.Image && !hasImage && !string.IsNullOrEmpty(post.MediaUrl));
            
            if (hasImage && hasVideo)
                return ServiceResult.Failure("Solo puedes adjuntar una imagen o un video de YouTube, no ambos.");
            if (!hasImage && !hasVideo && !isKeepingExistingImage)
                return ServiceResult.Failure("Debes proporcionar exactamente un contenido multimedia (Imagen o Video de YouTube).");

            if (vm.MediaType == MediaType.Image)
            {
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    string extension = ".jpg";
                    post.MediaUrl = await SaveImageAsync(vm.ImageFile, extension);
                }
            }
            else if (vm.MediaType == MediaType.Video)
            {
                if (!string.IsNullOrEmpty(vm.VideoUrl))
                {
                    var mediaUrl = ExtractYouTubeEmbedUrl(vm.VideoUrl);
                    if (mediaUrl == null)
                        return ServiceResult.Failure("El formato de la URL de YouTube no es válido.");

                    post.MediaUrl = mediaUrl;
                }
                else
                {
                    return ServiceResult.Failure("Debes proporcionar una URL de video para las publicaciones de tipo video.");
                }
            }

            post.Content = vm.Content;
            post.MediaType = vm.MediaType;
            post.Privacy = vm.Privacy;
            post.AllowComments = vm.AllowComments;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdateAsync(post);
            return ServiceResult.Success();
        }

        #region Private Helpers
        private async Task<List<PostViewModel>> MapToViewModels(IEnumerable<Post> posts, string currentUserId, UserBasicDto authorInfo)
        {
            var result = new List<PostViewModel>();
            foreach (var post in posts)
            {
                var vm = await MapSinglePost(post, currentUserId, authorInfo);
                result.Add(vm);
            }
            return result;
        }

        private async Task<PostViewModel> MapSinglePost(Post post, string currentUserId, UserBasicDto authorInfo)
        {
            var reactions = await _reactionRepository.GetByPostIdAsync(post.Id);
            var userReaction = reactions.FirstOrDefault(r => r.UserId == currentUserId);
            var comment = new List<CommentDto>();
            if (post.Comments != null)
            {
                foreach (var cm in post.Comments.Where(c => c.ParentCommentId == null).OrderBy(c => c.CreatedAt))
                {
                    var commentUserInfo = await _userService.GetUserBasicInfoAsync(cm.UserId!);
                    var dto = await MapCommentWhitUser(cm, commentUserInfo);
                    comment.Add(dto);
                }
            }

            return new PostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                MediaType = post.MediaType,
                MediaUrl = post.MediaUrl,
                CreatedAt = post.CreatedAt,
                Privacy = post.Privacy,
                AllowComments = post.AllowComments,
                UserId = post.UserId!,
                Username = authorInfo.Username,
                UserProfilePicture = authorInfo.ProfilePictureUrl,
                LikesCount = reactions.Count(r => r.IsLike),
                DislikesCount = reactions.Count(r => !r.IsLike),
                CurrentUserReaction = userReaction?.IsLike,
                Comments = comment
            };
        }

        private async Task<CommentDto> MapCommentWhitUser(Comment comment, UserBasicDto userInfo)
        {
            var replies = new List<CommentDto>();
            if (comment.Replies != null)
            {
                foreach (var reply in comment.Replies.OrderBy(r => r.CreatedAt))
                {
                    var replyUserInfo = await _userService.GetUserBasicInfoAsync(reply.UserId!);
                    var replyDto = await MapCommentWhitUser(reply, replyUserInfo);
                    replies.Add(replyDto);
                }
            }
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId!,
                Username = userInfo.Username,
                UserProfilePicture = userInfo.ProfilePictureUrl,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                Replies = replies
            };
        }

        private static CommentDto MapComment(Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UserId = comment.UserId!,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                Replies = comment.Replies?
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => MapComment(r))
                    .ToList() ?? []
            };
        }

        private static async Task<string> SaveImageAsync(byte[] fileContent, string extension)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            var path = Path.Combine("wwwroot", "uploads", "posts", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, fileContent);

            return $"/uploads/posts/{fileName}";
        }

        private static string? ExtractYouTubeEmbedUrl(string url)
        {
            string? videoId = null;

            if (url.Contains("youtube.com/watch"))
            {
                var uri = new Uri(url);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                videoId = query["v"];
            }
            else if (url.Contains("youtu.be/"))
            {
                videoId = url.Split("youtu.be/").Last().Split('?').First();
            }

            return videoId != null ? $"https://www.youtube.com/embed/{videoId}" : null;
        }
        #endregion

    }
}