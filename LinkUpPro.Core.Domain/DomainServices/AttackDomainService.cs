using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Domain.DomainServices
{
    public class AttackDomainService : IAttackDomainService
    {
        public DomainResult ValidateTurn(BattleshipGame game, string attackerId)
        {
            if (game.Status != GameStatus.InProgress)
                return DomainResult.Failure("La partida no está en progreso.");

            if (game.CurrentTurnPlayerId != attackerId)
                return DomainResult.Failure("No es tu turno.");

            return DomainResult.Success();
        }

        public bool IsDuplicateAttack(IEnumerable<Attack> existingAttacks, int row, int col)
        {
            return existingAttacks.Any(a => a.Row == row && a.Column == col);
        }

        public AttackResult EvaluateImpact(IEnumerable<Ship> opponentShips, int row, int col)
        {
            foreach (var ship in opponentShips)
            {
                foreach (var cell in ship.Cells)
                {
                    if (cell.Row == row && cell.Column == col)
                    {
                        return new AttackResult
                        {
                            IsHit = true,
                            HitShip = ship,
                            HitCell = cell
                        };
                    }
                }
            }

            return new AttackResult { IsHit = false };
        }

        public bool CheckIfSunk(Ship ship)
        {
            return ship.Cells.All(c => c.WasAttacked);
        }

        public bool CheckVictory(IEnumerable<Ship> allOpponentShips)
        {
            return allOpponentShips.All(s => s.IsSunk);
        }
    }
}
