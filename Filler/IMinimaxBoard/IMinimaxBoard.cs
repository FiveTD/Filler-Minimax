using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimax;

public interface IMinimaxBoard
{
    // turn
    public abstract byte Turn { get; protected set; }

    // move
    public abstract void Move(int[] move);

    // unmove
    public abstract void Unmove();

    // score position
    public abstract int Score(byte turn = 255);

    // valid moves
    public abstract List<int[]> ValidMoves();

    public abstract bool Won(out byte? winner);
}
