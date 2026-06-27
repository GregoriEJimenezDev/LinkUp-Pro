using LinkUpPro.Core.Application.DTOs.Friend;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Friend;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class FriendRequestService(IFriendRequestRepository friendRequestRepository,
        IFriendshipRepository friendshipRepository, IUserService userService, INotificationService notificationService) : IFriendRequestService
    {
        private readonly IFriendRequestRepository _friendRequestRepository = friendRequestRepository;
        private readonly IFriendshipRepository _friendshipRepository = friendshipRepository;
        private readonly IUserService _userService = userService;
        private readonly INotificationService _notificationService = notificationService;

        public async Task<ServiceResult> AcceptAsync(int requestId, string userId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return ServiceResult.Failure("Solicitud de amistad no encontrada.");

            if (request.ReceiverId != userId)
                return ServiceResult.Failure("No estás autorizado para aceptar esta solicitud de amistad.");

            request.Status = FriendRequestStatus.Accepted;
            request.RespondedAt = DateTime.UtcNow;
            await _friendRequestRepository.UpdateAsync(request);

            if (string.IsNullOrEmpty(request.SenderId) || string.IsNullOrEmpty(request.ReceiverId))
                return ServiceResult.Failure("Los datos de la solicitud de amistad no son válidos.");

            await _friendshipRepository.AddAsync(new Friendship
            {
                FirstUserId = request.SenderId,
                SecondUserId = request.ReceiverId,
                CreatedAt = DateTime.UtcNow
            });

            var receiverInfo = await _userService.GetUserBasicInfoAsync(userId);
            await _notificationService.CreateNotificationAsync(
                request.SenderId, 
                "Solicitud de amistad aceptada", 
                $"{receiverInfo.Username} aceptó tu solicitud de amistad.", 
                "/Friendship/Index",
                NotificationType.FriendRequestAccepted
            );

            return ServiceResult.Success();
        }
        public async Task<ServiceResult> DeleteAsync(int requestId, string userId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return ServiceResult.Failure("Solicitud de amistad no encontrada.");
            if (request.SenderId != userId && request.ReceiverId != userId)
                return ServiceResult.Failure("No estás autorizado para eliminar esta solicitud de amistad.");

            await _friendRequestRepository.DeleteAsync(request);
            return ServiceResult.Success();
        }
        public async Task<SendFriendRequestViewModel> GetAvailableUsersAsync(string userId, string? search)
        {
            var allUsers = await _userService.GetAllActiveUsersAsync();
            var friendIds = await GetFriendIdsAsync(userId);

            var available = new List<UserToAddViewModel>();

            foreach (var user in allUsers)
            {
                if (user.Id == userId) continue;
                if (friendIds.Contains(user.Id!)) continue;
                var hasPending = await _friendRequestRepository.HasPendingRequestAsync(userId, user.Id!);
                if (hasPending) continue;
                if (!string.IsNullOrWhiteSpace(search) &&
                    !user.Username.Contains(search, StringComparison.OrdinalIgnoreCase))
                    continue;

                var mutual = await GetMutualFriendsCountAsync(userId, user.Id!);

                available.Add(new UserToAddViewModel
                {
                    UserId = user.Id!,
                    Username = user.Username,
                    ProfilePicture = user.ProfilePictureUrl,
                    MutualFriendsCount = mutual
                });
            }

            return new SendFriendRequestViewModel
            {
                AvailableUsers = available,
                SearchUsername = search
            };
        }

        public async Task<FriendRequestIndexViewModel> GetRequestsAsync(string userId)
        {
            var received = await _friendRequestRepository.GetReceivedByUserAsync(userId);
            var sent = await _friendRequestRepository.GetSentByUserAsync(userId);

            var receivedDtos = new List<FriendRequestDto>();
            foreach (var r in received)
            {
                var senderInfo = await _userService.GetUserBasicInfoAsync(r.SenderId!);
                var mutual = await GetMutualFriendsCountAsync(userId, r.SenderId!);
                receivedDtos.Add(new FriendRequestDto
                {
                    Id = r.Id,
                    SenderId = r.SenderId!,
                    SenderUsername = senderInfo.Username,
                    SenderProfilePicture = senderInfo.ProfilePictureUrl,
                    ReceiverId = r.ReceiverId!,
                    Status = r.Status,
                    SentAt = r.SentAt,
                    MutualFriendsCount = mutual
                });
            }

            var sentDtos = new List<FriendRequestDto>();
            foreach (var s in sent)
            {
                var receiverInfo = await _userService.GetUserBasicInfoAsync(s.ReceiverId!);
                var mutual = await GetMutualFriendsCountAsync(userId, s.ReceiverId!);
                sentDtos.Add(new FriendRequestDto
                {
                    Id = s.Id,
                    SenderId = s.SenderId!,
                    ReceiverId = s.ReceiverId!,
                    ReceiverUsername = receiverInfo.Username,
                    Status = s.Status,
                    SentAt = s.SentAt,
                    MutualFriendsCount = mutual
                });
            }

            return new FriendRequestIndexViewModel
            {
                Received = receivedDtos,
                Sent = sentDtos
            };
        }

        public async Task<ServiceResult> RejectAsync(int requestId, string userId)
        {
            var request = await _friendRequestRepository.GetByIdAsync(requestId);
            if (request == null)
                return ServiceResult.Failure("Solicitud de amistad no encontrada.");
            if (request.ReceiverId != userId)
                return ServiceResult.Failure("No estás autorizado para rechazar esta solicitud de amistad.");

            request.Status = FriendRequestStatus.Rejected;
            request.RespondedAt = DateTime.UtcNow;

            await _friendRequestRepository.UpdateAsync(request);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> SendAsync(string senderId, string receiverId)
        {
            var hasPending = await _friendRequestRepository.HasPendingRequestAsync(senderId, receiverId);
            if (hasPending)
                return ServiceResult.Failure("Ya existe una solicitud de amistad pendiente entre estos usuarios.");

            var areFriends = await _friendshipRepository.AreFriendsAsync(senderId, receiverId);
            if (areFriends)
                return ServiceResult.Failure("Ya eres amigo de este usuario.");

            await _friendRequestRepository.AddAsync(new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendRequestStatus.Pending,
                SentAt = DateTime.UtcNow
            });

            var senderInfo = await _userService.GetUserBasicInfoAsync(senderId);
            await _notificationService.CreateNotificationAsync(
                receiverId,
                "Nueva solicitud de amistad",
                $"{senderInfo.Username} te ha enviado una solicitud de amistad.",
                "/FriendRequest/Index",
                NotificationType.FriendRequestReceived
            );

            return ServiceResult.Success();
        }

        #region Private Helpers
        private async Task<List<string>> GetFriendIdsAsync(string userId)
        {
            var friendships = await _friendshipRepository.GetFriendsByUserIdAsync(userId);
            return friendships
                .Select(f => f.FirstUserId == userId ? f.SecondUserId : f.FirstUserId)
                .ToList()!;
        }
        private async Task<int> GetMutualFriendsCountAsync(string userId1, string userId2)
        {
            var friends1 = await GetFriendIdsAsync(userId1);
            var friends2 = await GetFriendIdsAsync(userId2);
            return friends1.Intersect(friends2).Count();
        }
        #endregion
    }
}
