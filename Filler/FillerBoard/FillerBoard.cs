using Minimax;
using System.Text;

namespace Filler
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

        private readonly Dictionary<byte, byte> playerColors = new(); // Stores the colors of the players
        private readonly Dictionary<byte, HashSet<(int X, int Y)>> playerTerritory = new(); // Stores the coordinates of all squares under player control

        /// <summary>
        /// Saves information about a move.
        /// </summary>
        private struct MoveRec
        {
            public byte PrevColor;
            public (int X, int Y)[] Changed;

            public MoveRec(byte prev, params (int X, int Y)[] changed)
            {
                PrevColor = prev;
                Changed = changed;
            }
        }
        private readonly Stack<MoveRec> moveHistory = new();
        
        public byte Turn { get; set; }
        public int X { get => x; }
        public int Y { get => y; }

        /// <summary>
        /// Creates a new Filler board with default parameters.
        /// </summary>
        /// <param name="size"></param>
        public FillerBoard(int size) :
            this(size, size, DEF_COLORS, DEF_PLAYERS) { }

        /// <summary>
        /// Creates a new Filler board with the given parameters.
        /// </summary>
        /// <param name="x">Width of the board</param>
        /// <param name="y">Length of the board</param>
        /// <param name="colors">Number of colors on the board</param>
        /// <param name="players">Number of players (2-4)</param>
        public FillerBoard(int x = DEF_SIZE, int y = DEF_SIZE, byte colors = DEF_COLORS, byte players = DEF_PLAYERS)
        {
            this.x = x; this.y = y;
            board = new byte[x, y];
            numColors = colors;
            numPlayers = players;
            if (numColors < 2) throw new ArgumentException("Invalid number of colors.");
            if (numPlayers < 2 || numPlayers > 4) throw new ArgumentException("Invalid number of players.");

            Generate();

            for (byte p = 0; p < numPlayers; p++)
            {
                playerTerritory.Add(p, new());
                playerTerritory[p].Add(PlayerCorner(p));
            }
        }

        /// <summary>
        /// Generate a new random board configuration.
        /// </summary>
        private void Generate()
        {
            // Generate board
            Random rand = new();
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    while (true)
                    {
                        board[i, j] = (byte)rand.Next(0, numColors);
                        // Check for neighboring duplicates
                        if (i >= 1 && board[i - 1, j] == board[i, j]) continue;
                        else if (j >= 1 && board[i, j - 1] == board[i, j]) continue;
                        break;
                    } 
                }
            }

            // Update player color values
            for (byte i = 0; i < numPlayers; i++)
            {
                var _c = PlayerCorner(i);

                playerColors[i] = board[_c.X, _c.Y];
            }
        }

        /// <summary>
        /// Represents the board as a grid of values from 0 to numColors-1.
        /// </summary>
        public override string ToString()
        {
            StringBuilder printStr = new();

            for (int i = 0; i < y; i++)
            {
                for (int j = 0; j < x; j++)
                {
                    printStr.Append(board[j, i]);
                }
                printStr.Append('\n');
            }

            return printStr.ToString();
        }

        /// <summary>
        /// Get the color at a specified location.
        /// </summary>
        public byte this[int x, int y]
        {
            get { return board[x, y]; }
        }

        /// <summary>
        /// Makes a move using the int[] format, where int[0] is the move.
        /// Does not check for validity.
        /// </summary>
        /// <param name="color"></param>
        public void Move(int[] color)
        {
            Move((byte)color[0]);
        }

        /// <summary>
        /// Makes a move. Does not check for validity.
        /// </summary>
        /// <param name="color"></param>
        public void Move(byte color)
        {
            byte prevColor = playerColors[Turn];

            // Swap all player colors to the new color
            playerColors[Turn] = color;
            foreach (var sp in playerTerritory[Turn])
            {
               board[sp.X, sp.Y] = color;
            }

            // Determine added spaces
            HashSet<(int X, int Y)> affected = new();
            foreach (var adj in Adjacent(Turn))
            {
                if (board[adj.X, adj.Y] == color)
                {
                    playerTerritory[Turn].Add(adj);
                    affected.Add(adj);
                }
            }

            moveHistory.Push(new MoveRec(prevColor, affected.ToArray()));

            Turn++;
            if (Turn == numPlayers) Turn = 0;
        }

        /// <summary>
        /// Undoes the latest move.
        /// </summary>
        public void Unmove()
        {
            Turn--;
            if (Turn == 255) Turn = (byte)(numPlayers - 1);

            MoveRec prev = moveHistory.Pop();

            // Remove added spaces
            foreach (var adj in prev.Changed)
            {
                playerTerritory[Turn].Remove(adj);
            }

            // Return to previous color
            playerColors[Turn] = prev.PrevColor;
            foreach (var sp in playerTerritory[Turn])
            {
                board[sp.X, sp.Y] = prev.PrevColor;
            }
        }

        /// <summary>
        /// Gets the size of a player's territory.
        /// </summary>
        public int Size(byte player)
        {
            return playerTerritory[player].Count;
        }

        /// <summary>
        /// Gets a score for the current board state for the given player.
        /// Defaults to the current player if none is given.
        /// </summary>
        public int Score(byte player = 255)
        {
            if (player == 255) player = Turn;
            //TODO: assumes two players
            if (player == 0) return Size(player) - Size(1);
            else return Size(player) - Size(0);
        }

        /// <summary>
        /// Determine whether a move is valid.
        /// </summary>
        /// <param name="color">The color to switch to</param>
        /// <returns></returns>
        public bool IsValid(byte color)
        {
            // Cannot change to another player's color
            if (playerColors.ContainsValue(color)) return false;

            // Color must be adjacent to your color or another player's color,
            // unless a player has no adjacent unowned squares.
            // (Forces computers to finish a losing game)
            for (byte p = 0; p < numPlayers; p++)
            {
                var adj = Adjacent(p);
                if (adj.Count == 0) return true;
                foreach (var coord in adj)
                {
                    if (board[coord.X, coord.Y] == color) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns a list of all valid moves.
        /// </summary>
        public List<byte> ValidMovesByte()
        {
            // Collect all valid moves
            List<byte> validMoves = new();
            for (byte c = 0; c < numColors; c++)
            {
                if (IsValid(c)) validMoves.Add(c);
            }

            // TODO: Sort efficiently

            //// Sort by number adjacent
            //Dictionary<byte, int> numAdjacent = new();
            //for (byte i = 0; i < numColors; i++) numAdjacent[i] = 0;
            //foreach (var coord in Adjacent(Turn))
            //{
            //    numAdjacent[board[coord.X, coord.Y]]++;
            //}
            //validMoves.Sort((a, b) => numAdjacent[a] - numAdjacent[b]);

            return validMoves;
        }

        /// <summary>
        /// Returns a list of all valid moves in the int[] format, where int[0] is the move.
        /// Moves are sorted by player size immediately following the move.
        /// </summary>
        public List<int[]> ValidMoves()
        {
            List<int[]> validMoves = new();
            foreach (byte move in ValidMovesByte())
            {
                validMoves.Add(new int[] { move });
            }
            return validMoves;
        }

        /// <summary>
        /// Determines whether the game has been completed.
        /// </summary>
        /// <param name="winner">The player who won or is leading in the game, null in case of a tie.</param>
        /// <returns>True if a winner exists.</returns>
        public bool Won(out byte? winner)
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

        /// <summary>
        /// Returns a tuple with the coordinates to the player's corner.
        /// </summary>
        private (int X, int Y) PlayerCorner(byte player)
        {
            return player switch
            {
                0 => (0, y - 1),
                1 => (x - 1, 0),
                2 => (0, 0),
                3 => (x - 1, y - 1),
                _ => (0, 0),
            };
        }

        /// <summary>
        /// Returns all adjacent spaces to a player's territory.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private HashSet<(int X, int Y)> Adjacent(byte player)
        {
            HashSet<(int X, int Y)> adjacent = new();
            foreach (var coord in playerTerritory[player])
            {
                // Determine if an offset coordinate is an unowned square, and track it if it is.
                void AddAdjacent(int xAdj, int yAdj)
                {
                    (int X, int Y) adj = (coord.X + xAdj, coord.Y + yAdj);

                    // Remove all owned squares
                    for (byte p = 0; p < numPlayers; p++)
                        if (playerTerritory[p].Contains(adj)) return;

                    if (adj.X >= 0 && adj.X < x &&
                        adj.Y >= 0 && adj.Y < y)
                        adjacent.Add(adj);
                }

                AddAdjacent(1, 0);
                AddAdjacent(0, 1);
                AddAdjacent(-1, 0);
                AddAdjacent(0, -1);
            }

            return adjacent;
        }
    }
}