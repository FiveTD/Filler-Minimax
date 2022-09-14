namespace Minimax
{
    public interface IMinimax
    {
        // move
        public abstract void Move(int[] move);

        // unmove
        public abstract void Unmove();

        // score position
        public abstract int Score { get; }

        // valid moves
        public abstract List<int[]> ValidMoves();
    }
}