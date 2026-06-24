namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class GameStatsDto
    {
        public int TotalGames { get; set; }
        public int WonGames { get; set; }
        public int LostGames { get; set; }
        public double WinRatio { get; set; }
        public int TotalAttacks { get; set; }
        public int TotalHits { get; set; }
        public double Accuracy { get; set; }
    }
}
