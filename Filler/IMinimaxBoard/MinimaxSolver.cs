namespace Minimax;

public class MinimaxSolver
{
    const int MaxScore = int.MaxValue;
    const int MinScore = int.MinValue + 1;

    private readonly byte turn;
    private readonly int maxDepth;

    public int NodesSearched { get; private set; }

    public MinimaxSolver(byte turn, int maxDepth)
    {
        this.maxDepth = maxDepth;
        this.turn = turn;
    }

    /// <summary>
    /// Determines the best move on the board.
    /// </summary>
    /// <param name="board"></param>
    /// <returns></returns>
    public int[] Move(IMinimaxBoard board)
    {
        NodesSearched = 0;
        Analyze(board, maxDepth, out int[] bestMove);
        return bestMove;
    }

    // TODO: assumes two players
    private int Analyze(IMinimaxBoard board, int depth, out int[] move, int alpha = MinScore, int beta = MaxScore)
    {
        move = Array.Empty<int>();

        if (depth == 0) return board.Score();
        //if (board.Won(out byte? winner))
        //{
        //    if (winner is null) return 0;
        //    else if (winner == turn) return MaxScore - 1;
        //    else return MinScore + 1;
        //}

        foreach (int[] m in board.ValidMoves())
        {
            NodesSearched++;
            board.Move(m);
            int score = -Analyze(board, depth - 1, out _, -beta, -alpha);
            board.Unmove();

            // Move is too good, opponent wouldn't allow it
            // Prune this branch
            if (score >= beta)
            {
                move = m;
                return beta;
            }

            // Move is the best we've seen so far
            if (score > alpha)
            {
                alpha = score;
                move = m;
            }
        }

        //if (move.Length == 0) move = board.ValidMoves()[0];
        return alpha;
    }
}
