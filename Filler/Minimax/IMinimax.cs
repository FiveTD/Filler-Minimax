namespace Minimax
{
    public interface IMinimaxBoard
    {
        // move
        public abstract void Move(int[] move);

        // unmove
        public abstract void Unmove();

        // score position
        public abstract int Score { get; }

        public abstract byte Turn { get; protected set; }

        // valid moves
        public abstract List<int[]> ValidMoves();

        //public abstract bool Won(out byte winner);
    }
}