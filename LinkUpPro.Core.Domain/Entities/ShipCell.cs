namespace LinkUpPro.Core.Domain.Entities
{
    public class ShipCell
    {
        public int Id { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public bool WasAttacked { get; set; } = false;

        public int ShipId { get; set; }
        public Ship? Ship { get; set; }
    }
}
