using LinkUpPro.Core.Application.DTOs.Battleship;
using LinkUpPro.Core.Application.Interfaces.Services;
using LinkUpPro.Core.Application.ViewModel.Game;
using LinkUpPro.Core.Application.ViewModel.Select;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IBattleshipService
    {
        Task<BattleshipIndexViewModel> GetIndexAsync(string userId);
        Task<ServiceResult<int>> CreateGameAsync(string player1Id, string player2Id);
        Task<SelectShipViewModel> GetPendingShipsAsync(int gameId, string playerId);
        Task<SelectCellViewModel> GetBoardForPlacementAsync(int gameId, string playerId, string shipType);
        Task<ServiceResult> PlaceShipAsync(int gameId, string playerId, string shipType, int row, int col, ShipDirection direction);
        Task<AttackBoardViewModel> GetAttackBoardAsync(int gameId, string playerId);
        Task<ServiceResult> AttackAsync(int gameId, string attackerId, int row, int col);
        Task<ServiceResult> SurrenderAsync(int gameId, string playerId);
        Task<GameResultViewModel> GetResultAsync(int gameId, string playerId);
        Task CheckTimeoutsAsync();
        Task<GameStatsDto> GetStatsAsync(string userId);
    }
}
