using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;

namespace LinkUpPro.Core.Domain.Interfaces
{
    public interface IAttackRepository : IGenericRepository<Attack>
    {
        Task<IEnumerable<Attack>> GetByGameAndAttackerAsync(int gameId, string attackerId);
        Task<bool> CellAlreadyAttackedAsync(int gameId, string attackerId, int row, int col);
    }
}
