using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimax;

public class MinimaxSolver
{
    private readonly byte turn;
    private readonly int maxDepth;

    public MinimaxSolver(byte turn, int maxDepth)
    {
        this.maxDepth = maxDepth;
        this.turn = turn;
    }

    public int[] Move(IMinimaxBoard board)
    {
        Analyze(board, maxDepth, out int[] bestMove);
        return bestMove;
    }

    private int Analyze(IMinimaxBoard board, int depth, out int[] move, int alpha = int.MinValue, int beta = int.MaxValue)
    {
        move = Array.Empty<int>();

        if (depth == 0) return board.Score;

        bool turnMatches = board.Turn == turn;

        int bestScore = int.MaxValue * (turnMatches ? -1 : 1); // low value if maximizing, high value if minimizing
        int[] bestMove = move;

        foreach (int[] m in board.ValidMoves())
        {
            board.Move(m);
            int score = Analyze(board, depth - 1, out _, alpha, beta);
            board.Unmove();

            if (turnMatches)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = m;
                    alpha = Math.Max(score, alpha);
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = m;
                    beta = Math.Min(score, beta);
                }
            }

            if (beta <= alpha) break;
        }

        move = bestMove; 
        return bestScore;
    }
}
