using LinkUpPro.Core.Application.DTOs.Comment;
using LinkUpPro.Core.Application.DTOs.User;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Post;
using LinkUpPro.Core.Application.ViewModel.Save;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace LinkUpPro.Core.Application.Services
{
    public class PostService(IPostRepository postRepository, IReactionRepository reactionRepository,
        IFriendshipRepository friendshipRepository, IUserService userService, IFileStorageService fileStorageService, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository;
        private readonly IReactionRepository _reactionRepository = reactionRepository;
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly IUserService _userService = userService;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly Microsoft.Extensions.Caching.Memory.IMemoryCache _cache = memoryCache;

        public async Task<ServiceResult> CreateAsync(SavePostViewModel vm, string userId)
        {
            if (string.IsNullOrWhiteSpace(vm.Content))
                return ServiceResult.Failure("El contenido de la publicación no puede estar vacío.");

            bool hasImage = vm.ImageFile != null && vm.ImageFile.Length > 0;
            bool hasVideo = !string.IsNullOrWhiteSpace(vm.VideoUrl);

            if (hasVideo) vm.MediaType = MediaType.Video;
            else if (hasImage) vm.MediaType = MediaType.Image;

            if (hasImage && hasVideo)
                return ServiceResult.Failure("Solo puedes adjuntar una imagen o un video de YouTube, no ambos.");
            if (!hasImage && !hasVideo)
                return ServiceResult.Failure("Debes adjuntar obligatoriamente una imagen o un video.");

            string? mediaUrl = null;

            if (vm.MediaType == MediaType.Image && hasImage)
            {
                if (vm.ImageFile!.Length > 5 * 1024 * 1024)
                    return ServiceResult.Failure("La imagen de la publicación no puede superar los 5 MB.");
                string extension = string.IsNullOrEmpty(vm.ImageFileName) ? ".jpg" : Path.GetExtension(vm.ImageFileName).ToLower();
                
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                if (!validExtensions.Contains(extension))
                {
                    return ServiceResult.Failure("El formato de la imagen no es válido. Formatos permitidos: JPG, PNG, WEBP, GIF.");
                }

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

            post.IsDeleted = true;
            await _postRepository.UpdateAsync(post);
            return ServiceResult.Success();
        }

        public async Task<bool> IsPostAvailableAsync(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            return post != null && !post.IsDeleted;
        }

        public async Task<List<PostViewModel>> GetByFriendsAsync(string userId, bool includeGlobalPublic = false, bool includeSelf = true)
        {
            var cacheKey = $"FeedPosts_{userId}_{includeGlobalPublic}";
            
            if (_cache.TryGetValue(cacheKey, out List<PostViewModel>? cachedPosts))
            {
                return cachedPosts ?? new List<PostViewModel>();
            }

            var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
            var friendIds = friendships.Select(f => f.FirstUserId == userId ? f.SecondUserId : f.FirstUserId)
                .Where(id => id != null).ToList();

            var feedPosts = await _postRepository.GetFeedPostsAsync(userId, friendIds!, includeGlobalPublic, includeSelf);

            var userIdsToFetch = new HashSet<string>();
            foreach (var post in feedPosts)
            {
                userIdsToFetch.Add(post.UserId!);
                if (post.Comments != null)
                {
                    foreach(var comment in post.Comments)
                    {
                        userIdsToFetch.Add(comment.UserId!);
                        if(comment.Replies != null)
                        {
                            foreach(var reply in comment.Replies)
                            {
                                userIdsToFetch.Add(reply.UserId!);
                            }
                        }
                    }
                }
            }

            var usersInfo = await _userService.GetUsersBasicInfoAsync(userIdsToFetch);

            var result = new List<PostViewModel>();

            foreach (var post in feedPosts)
            {
                var authorInfo = usersInfo.TryGetValue(post.UserId!, out var u) ? u : new UserBasicDto();
                if (string.IsNullOrEmpty(authorInfo.Username)) continue;

                var vm = MapSinglePostSync(post, userId, authorInfo, usersInfo);
                result.Add(vm);
            }

            // Cache for 30 seconds
            _cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));

            return result;
        }

        public async Task<SavePostViewModel?> GetByIdForEditAsync(int postId, string userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.UserId != userId) return null;

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

        public async Task<List<PostViewModel>> GetByUserAsync(string targetUserId, string currentUserId, bool areFriends = false)
        {
            var posts = await _postRepository.GetByUserIdAsync(targetUserId);
            var activePosts = posts.Where(p => !p.IsDeleted).ToList();

            if (targetUserId != currentUserId)
            {
                if (areFriends)
                {
                    activePosts = activePosts.Where(p => p.Privacy == PostPrivacy.Public || p.Privacy == PostPrivacy.FriendsOnly).ToList();
                }
                else
                {
                    activePosts = activePosts.Where(p => p.Privacy == PostPrivacy.Public).ToList();
                }
            }

            if (!activePosts.Any()) return [];

            var userIdsToFetch = new HashSet<string>();
            foreach (var post in activePosts)
            {
                if (post.Comments != null)
                {
                    foreach (var comment in post.Comments)
                    {
                        userIdsToFetch.Add(comment.UserId!);
                        if (comment.Replies != null)
                        {
                            foreach (var reply in comment.Replies)
                            {
                                userIdsToFetch.Add(reply.UserId!);
                            }
                        }
                    }
                }
            }

            var usersInfo = await _userService.GetUsersBasicInfoAsync(userIdsToFetch);
            var userInfo = await _userService.GetUserBasicInfoAsync(targetUserId);
            
            if (string.IsNullOrEmpty(userInfo.Username)) return [];

            return MapToViewModelsSync(activePosts, currentUserId, userInfo, usersInfo);
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

            if (hasVideo) vm.MediaType = MediaType.Video;
            else if (hasImage) vm.MediaType = MediaType.Image;

            bool isKeepingExistingImage = (vm.MediaType == MediaType.Image && post.MediaType == MediaType.Image && !hasImage && !string.IsNullOrEmpty(post.MediaUrl));
            
            if (hasImage && hasVideo)
                return ServiceResult.Failure("Solo puedes adjuntar una imagen o un video de YouTube, no ambos.");
            if (!hasImage && !hasVideo && !isKeepingExistingImage)
                return ServiceResult.Failure("Debes proporcionar exactamente un contenido multimedia (Imagen o Video de YouTube).");

            if (vm.MediaType == MediaType.Image)
            {
                if (vm.ImageFile != null && vm.ImageFile.Length > 0)
                {
                    if (vm.ImageFile.Length > 5 * 1024 * 1024)
                        return ServiceResult.Failure("La imagen de la publicación no puede superar los 5 MB.");
                    string extension = string.IsNullOrEmpty(vm.ImageFileName) ? ".jpg" : Path.GetExtension(vm.ImageFileName).ToLower();
                    
                    var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                    if (!validExtensions.Contains(extension))
                    {
                        return ServiceResult.Failure("El formato de la imagen no es válido. Formatos permitidos: JPG, PNG, WEBP, GIF.");
                    }

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
            
            _cache.Remove($"FeedPosts_{userId}_True");
            _cache.Remove($"FeedPosts_{userId}_False");

            return ServiceResult.Success();
        }

        #region Private Methods

        private List<PostViewModel> MapToViewModelsSync(IEnumerable<Post> posts, string currentUserId, UserBasicDto authorInfo, Dictionary<string, UserBasicDto> usersInfo)
        {
            var result = new List<PostViewModel>();
            foreach (var post in posts)
            {
                result.Add(MapSinglePostSync(post, currentUserId, authorInfo, usersInfo));
            }
            return result;
        }

        private PostViewModel MapSinglePostSync(Post post, string userId, UserBasicDto authorInfo, Dictionary<string, UserBasicDto> usersInfo)
        {
            var userReaction = post.Reactions?.FirstOrDefault(r => r.UserId == userId);
            
            var comment = post.Comments?
                .Where(c => c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => 
                {
                    var cUserInfo = usersInfo.TryGetValue(c.UserId!, out var cu) ? cu : new UserBasicDto();
                    return MapCommentWhitUserSync(c, cUserInfo, usersInfo);
                })
                .ToList() ?? [];

            return new PostViewModel
            {
                Id = post.Id,
                Content = post.Content,
                MediaType = post.MediaType,
                MediaUrl = post.MediaUrl,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                Privacy = post.Privacy,
                AllowComments = post.AllowComments,
                UserId = post.UserId!,
                Username = authorInfo.Username,
                UserProfilePicture = authorInfo.ProfilePictureUrl,
                LikesCount = post.Reactions?.Count(r => r.IsLike) ?? 0,
                DislikesCount = post.Reactions?.Count(r => !r.IsLike) ?? 0,
                CurrentUserReaction = userReaction?.IsLike,
                CommentsCount = CountCommentsRecursively(comment),
                Comments = comment
            };
        }

        private int CountCommentsRecursively(IEnumerable<CommentDto> comments)
        {
            if (comments == null) return 0;
            return comments.Sum(c => (c.IsDeleted ? 0 : 1) + CountCommentsRecursively(c.Replies));
        }

        private CommentDto MapCommentWhitUserSync(Comment comment, UserBasicDto userInfo, Dictionary<string, UserBasicDto> usersInfo)
        {
            var replies = new List<CommentDto>();
            if (comment.Replies != null)
            {
                foreach (var reply in comment.Replies.DistinctBy(r => r.Id).OrderBy(r => r.CreatedAt))
                {
                    var replyUserInfo = usersInfo.TryGetValue(reply.UserId!, out var ru) ? ru : new UserBasicDto();
                    var replyDto = MapCommentWhitUserSync(reply, replyUserInfo, usersInfo);
                    replies.Add(replyDto);
                }
            }
            return new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId!,
                Username = string.IsNullOrEmpty(userInfo.Username) ? "Usuario Eliminado" : userInfo.Username,
                UserProfilePicture = string.IsNullOrEmpty(userInfo.Username) ? "/placeholder.svg" : userInfo.ProfilePictureUrl,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                IsDeleted = comment.IsDeleted,
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
                UpdatedAt = comment.UpdatedAt,
                UserId = comment.UserId!,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                IsDeleted = comment.IsDeleted,
                Replies = comment.Replies?
                    .OrderBy(r => r.CreatedAt)
                    .Select(r => MapComment(r))
                    .ToList() ?? []
            };
        }

        private async Task<string> SaveImageAsync(byte[] fileContent, string extension)
        {
            var fileName = $"{Guid.NewGuid()}{extension}";
            var contentType = extension == ".png" ? "image/png" : (extension == ".webp" ? "image/webp" : "image/jpeg");
            return await _fileStorageService.UploadFileAsync(fileContent, fileName, "posts", contentType);
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
            else if (url.Contains("youtube.com/shorts/"))
            {
                videoId = url.Split("youtube.com/shorts/").Last().Split('?').First();
            }
            else if (url.Contains("youtube.com/embed/"))
            {
                videoId = url.Split("youtube.com/embed/").Last().Split('?').First();
            }

            return videoId != null ? $"https://www.youtube.com/embed/{videoId}" : null;
        }
        #endregion

    }
}
