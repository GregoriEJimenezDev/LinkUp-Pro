namespace LinkUpPro.Core.Application.ViewModel.Select
{
    public class SelectCellViewModel
    {
        public int GameId { get; set; }
        public string ShipType { get; set; } = string.Empty;
        public int ShipSize { get; set; }
        public List<(int Row, int Col)> OccupiedCells { get; set; } = [];
    }
}
