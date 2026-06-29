using LinkUpPro.Core.Domain.Entities;

namespace LinkUpPro.Core.Domain.DomainServices
{
    public class AttackResult
    {
        public bool IsHit { get; set; }
        public Ship? HitShip { get; set; }
        public ShipCell? HitCell { get; set; }
    }
}
