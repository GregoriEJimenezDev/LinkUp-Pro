namespace LinkUpPro.Core.Application.DTOs.Battleship
{
    public class LeaderboardDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public int TotalGames { get; set; }
        public int WonGames { get; set; }
        public int LostGames { get; set; }
        public double WinRatio { get; set; }
        public int Score { get; set; }
    }
}
