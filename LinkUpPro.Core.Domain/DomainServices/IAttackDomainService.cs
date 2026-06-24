using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.DomainServices
{
    public class AttackResult
    {
        public bool IsHit { get; set; }
        public Ship? HitShip { get; set; }
        public ShipCell? HitCell { get; set; }
    }

    public interface IAttackDomainService
    {
        DomainResult ValidateTurn(BattleshipGame game, string attackerId);
        bool IsDuplicateAttack(IEnumerable<Attack> existingAttacks, int row, int col);
        AttackResult EvaluateImpact(IEnumerable<Ship> opponentShips, int row, int col);
        bool CheckIfSunk(Ship ship);
        bool CheckVictory(IEnumerable<Ship> allOpponentShips);
    }
}
