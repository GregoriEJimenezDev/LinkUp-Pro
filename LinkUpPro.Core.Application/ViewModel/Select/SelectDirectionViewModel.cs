using LinkUpPro.Core.Domain.Enum;

namespace LinkUpPro.Core.Application.ViewModel.Select
{
    public class SelectDirectionViewModel
    {
        public int GameId { get; set; }
        public string ShipType { get; set; } = string.Empty;
        public int ShipSize { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public ShipDirection Direction { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
