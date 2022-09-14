using System.Data.Common;
using Minimax;

namespace FillerBoard
{
    public class FillerBoard : IMinimax
    {
        const int DEF_SIZE = 8;
        const byte DEF_COLORS = 6;
        const byte DEF_PLAYERS = 2;

        private byte[,] board;

        private readonly byte numColors;
        private readonly byte numPlayers;

        private byte lastColor;
        
        public byte Turn { get; private set; }
        public int Score { get => throw new NotImplementedException(); }

        public FillerBoard(int size) :
            this(size, size, DEF_COLORS, DEF_PLAYERS) { }

        public FillerBoard(int x = DEF_SIZE, int y = DEF_SIZE, byte colors = DEF_COLORS, byte players = DEF_PLAYERS)
        {
            board = new byte[x, y];
            numColors = colors;
            numPlayers = players;
        }

        public byte this[int x, int y]
        {
            get { return board[x, y]; }
        }

        public int Size(int dimension = 0)
        {
            return board.GetLength(dimension);
        }

        public bool IsValid(byte color)
        {
            return true;
        }

        public void Move(int[] color)
        {
            Move((byte)color[0]);
        }

        public void Move(byte color)
        {
            return;
        }

        public void Unmove()
        {
            return;
        }

        public int Size(byte player)
        {
            return 0;
        }

        public List<int[]> ValidMoves()
        {
            return new List<int[]>();
        }


        //public FillerBoard Clone()
        //{
        //    var clone = new FillerBoard(board.GetLength(0), board.GetLength(1), numColors, numPlayers);
        //    for (int i = 0; i < board.GetLength(0); i++) ; //TODO: Deep clone

        //    return clone;
        //}
    }
}