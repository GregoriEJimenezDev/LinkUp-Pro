using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IShipRepository : IGenericRepository<Ship>
    {
        Task<IEnumerable<Ship>> GetByGameAndPlayerAsync(int gameId, string playerId);
        Task<bool> PlayerFinishedPlacingAsync(int gameId, string playerId);
        Task<Ship> GetWithCellsAsync(int shipId);
        Task<IEnumerable<Ship>> GetWithCellsByGameAndPlayerAsync(int gameId, string playerId);
        Task<int> CountByGameAndPlayerAsync(int gameId, string playerId);
    }
}
