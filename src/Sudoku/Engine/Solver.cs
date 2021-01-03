using Sudoku.Infrastructure;

namespace Sudoku.Engine
{
    public class Solver
    {
        public void Solve(Board input)
        {
            var startIdx = 1;

            var initialState = input.SaveState();

            //while (true)
            {
                var board = new Board();

                board.LoadState(initialState);

                for (var x = 0; x < Constants.BoardSize; x++)
                {
                    for (var y = 0; y < Constants.BoardSize; y++)
                    {
                        if (board.Peek(x, y) != 0)
                        {
                            continue;
                        }

                        for (var i = 1; i < 10; i++)
                        {
                        }
                    }
                }
            }
        }
    }
}