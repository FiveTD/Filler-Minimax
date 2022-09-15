using System.Data.Common;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Minimax;

namespace FillerBoard
{
    public class FillerBoard : IMinimaxBoard
    {
        const int DEF_SIZE = 8;
        const byte DEF_COLORS = 6;
        const byte DEF_PLAYERS = 2;

        private readonly byte[,] board;
        private readonly int x, y;

        private readonly byte numColors;
        private readonly byte numPlayers;

        private readonly Dictionary<byte, byte> playerColors = new();
        private readonly Dictionary<byte, HashSet<Tuple<int, int>>> playerTerritory = new();

        private struct MoveRec
        {
            public byte PrevColor;
            public Tuple<int, int>[] Changed;

            public MoveRec(byte prev, params Tuple<int, int>[] changed)
            {
                PrevColor = prev;
                Changed = changed;
            }
        }
        private readonly Stack<MoveRec> moveHistory = new();
        
        public byte Turn { get; set; }
        public int Score { get => throw new NotImplementedException(); }

        public FillerBoard(int size) :
            this(size, size, DEF_COLORS, DEF_PLAYERS) { }

        public FillerBoard(int x = DEF_SIZE, int y = DEF_SIZE, byte colors = DEF_COLORS, byte players = DEF_PLAYERS)
        {
            this.x = x; this.y = y;
            board = new byte[x, y];
            numColors = colors;
            numPlayers = players;

            Generate();

            for (byte p = 0; p < numPlayers; p++)
            {
                playerTerritory.Add(p, new());
                playerTerritory[p].Add(PlayerCorner(p));
            }
        }

        private void Generate()
        {
            Random rand = new();
            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    board[i, j] = (byte)rand.Next(0, numColors);
                }
            }
        }

        public byte this[int x, int y]
        {
            get { return board[x, y]; }
        }

        public bool IsValid(byte color, byte player)
        {
            var _c = PlayerCorner(player);
            return !(playerColors.ContainsValue(color) || board[_c.Item1, _c.Item2] == color);
        }

        public void Move(int[] color)
        {
            Move((byte)color[0]);
        }

        public void Move(byte color)
        {
            if (!IsValid(color, Turn)) return;

            var _c = PlayerCorner(Turn);
            byte prevColor = board[_c.Item1, _c.Item2];

            foreach (var sp in playerTerritory[Turn])
            {
               board[sp.Item1, sp.Item2] = color;
            }

            HashSet<Tuple<int, int>> affected = new();
            foreach (var adj in Adjacent(Turn))
            {
                if (board[adj.Item1, adj.Item2] == color)
                {
                    playerTerritory[Turn].Add(adj);
                    affected.Add(adj);
                }
            }

            moveHistory.Push(new MoveRec(prevColor, affected.ToArray()));

            Turn++;
            if (Turn == numPlayers) Turn = 0;
        }

        public void Unmove()
        {
            Turn--;
            if (Turn == 255) Turn = numPlayers;

            MoveRec prev = moveHistory.Pop();

            foreach (var adj in prev.Changed)
            {
                playerTerritory[Turn].Remove(adj);
            }
            foreach (var sp in playerTerritory[Turn])
            {
                board[sp.Item1, sp.Item2] = prev.PrevColor;
            }
        }

        public int Size(byte player)
        {
            return playerTerritory[player].Count;
        }

        public List<byte> ValidMoves(byte player)
        {
            List<byte> validMoves = new();
            for (byte c = 0; c < numColors; c++)
            {
                if (IsValid(c, player)) validMoves.Add(c);
            }
            return validMoves;
        }

        public List<int[]> ValidMoves()
        {
            List<int[]> validMoves = new();
            foreach (byte move in ValidMoves(Turn))
            {
                validMoves.Add(new int[] { move });
            }
            return validMoves;
        }

        public bool Won(out byte winner)
        {
            int totalSize = 0;
            int largestSize = 0;
            winner = 0;

            for (byte p = 0; p < numPlayers; p++)
            {
                int size = playerTerritory[p].Count;
                totalSize += size;

                if (size >= largestSize)
                {
                    largestSize = size;
                    winner = p;
                }
            }

            return totalSize == x * y; // all spaces captured = winner exists
        }

        private Tuple<int, int> PlayerCorner(byte player)
        {
            return player switch
            {
                0 => Tuple.Create(0, y - 1),
                1 => Tuple.Create(x - 1, 0),
                2 => Tuple.Create(0, 0),
                3 => Tuple.Create(x - 1, y - 1),
                _ => Tuple.Create(0, 0),
            };
        }

        private HashSet<Tuple<int, int>> Adjacent(byte player)
        {
            HashSet<Tuple<int, int>> adjacent = new();
            foreach (var coord in playerTerritory[player])
            {
                void AddAdjacent(int xAdj, int yAdj)
                {
                    var adj = Tuple.Create(coord.Item1 + xAdj, coord.Item2 + yAdj);
                    
                    if (!playerTerritory[player].Contains(adj))
                    {
                        if (adj.Item1 >= 0 && adj.Item1 < x &&
                            adj.Item2 >= 0 && adj.Item2 < y)
                            adjacent.Add(adj);
                    }
                }

                AddAdjacent(1, 0);
                AddAdjacent(0, 1);
                AddAdjacent(-1, 0);
                AddAdjacent(0, -1);
            }

            return adjacent;
        }

        //public FillerBoard Clone()
        //{
        //    var clone = new FillerBoard(board.GetLength(0), board.GetLength(1), numColors, numPlayers);
        //    for (int i = 0; i < board.GetLength(0); i++) ; //TODO: Deep clone

        //    return clone;
        //}
    }
}