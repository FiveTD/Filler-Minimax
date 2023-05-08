using Filler;
using Minimax;
using System.Text;

const int PTURN = 1;
const int CTURN = 0;

static void printBoard(FillerBoard board)
{
    StringBuilder printStr = new();

    printStr.Append(board.ToString());
    printStr.Append(string.Format("Player: {0}, COM: {1}", board.Size(PTURN), board.Size(CTURN)));

    Console.WriteLine(printStr.ToString());
}

FillerBoard board = new(10);
MinimaxSolver computerPlayer;

int comDepth = 10;
//do
//{ 
//    Console.Write("Computer depth: ");
//} while (!int.TryParse(Console.ReadLine(), out comDepth) || comDepth < 1);

computerPlayer = new(CTURN, comDepth);

printBoard(board);
byte? winner;
while (!board.Won(out winner))
{
    if (board.Turn == PTURN)
    {
        byte move;
        do
        {
            Console.Write("Player move: ");
        } while (!byte.TryParse(Console.ReadLine(), out move) || !board.IsValid(move));
        board.Move(move);
    }
    else
    {
        Console.Write("Computer move: ");
        byte move = (byte)computerPlayer.Move(board)[0];
        Console.WriteLine(move);
        board.Move(move);
        Console.WriteLine(string.Format("Nodes searched: {0}", computerPlayer.NodesSearched));
        printBoard(board);
    }
}

if (winner is null) Console.WriteLine("hmmm");
else if (winner == PTURN) Console.WriteLine("booyah");
else Console.WriteLine("sadge");