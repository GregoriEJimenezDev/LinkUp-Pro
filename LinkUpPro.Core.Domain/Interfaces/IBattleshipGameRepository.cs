using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IBattleshipGameRepository : IGenericRepository<BattleshipGame>
    {
        Task<IEnumerable<BattleshipGame>> GetActiveByUserIdAsync(string userId);
        Task<IEnumerable<BattleshipGame>> GetFinishedByUserIdAsync(string userId);
        Task<BattleshipGame> GetWithDetailsAsync(int gameId);
        Task<bool> HasActiveGameWithFriendAsync(string userId, string friendId);
        Task<IEnumerable<BattleshipGame>> GetAllActiveAsync();
        Task<IEnumerable<BattleshipGame>> GetByPlayerAsync(string userId);
    }
}
