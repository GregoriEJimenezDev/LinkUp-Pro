namespace LinkUpPro.Core.Domain.ValueObjects
{
    public record BoardPosition
    {
        public const int BoardSize = 12;

        public int Row { get; }
        public int Column { get; }

        public BoardPosition(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public bool IsWithinBoard =>
            Row >= 0 && Row < BoardSize && Column >= 0 && Column < BoardSize;
    }
}
