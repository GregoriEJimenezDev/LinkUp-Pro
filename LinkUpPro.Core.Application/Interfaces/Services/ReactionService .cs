using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class ReactionService(IReactionRepository reactionRepository) : IReactionService
    {
        private readonly IReactionRepository _reactionRepository = reactionRepository;

        public async Task<ServiceResult> ReactAsync(int postId, string userId, bool isLike)
        {
            var existing = await _reactionRepository.GetByPostAndUserAsync(postId, userId);
            if (existing == null)
            {
                await _reactionRepository.AddAsync(new Reaction 
                {
                    PostId = postId,
                    UserId = userId,
                    IsLike = isLike
                });
            }
            else if(existing.IsLike == isLike)
            {
                return ServiceResult.Success();
            }
            else
            {
                existing.IsLike = isLike;
                await _reactionRepository.UpdateAsync(existing);
            }


            return ServiceResult.Success();
        }
    }
}
