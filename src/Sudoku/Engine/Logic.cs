using Sudoku.Infrastructure;

namespace Sudoku.Engine
{
    public static class Logic
    {
        public static bool IsLegalMove(Board board, int x, int y, byte value)
        {
            return CheckX(board, y, value) && CheckY(board, x, value) && CheckSector(board, x, y, value);
        }

        private static bool CheckX(Board board, int y, byte value)
        {
            for (var x = 0; x < Constants.BoardSize; x++)
            {
                if (board.Peek(x, y) == value)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckY(Board board, int x, byte value)
        {
            for (var y = 0; y < Constants.BoardSize; y++)
            {
                if (board.Peek(x, y) == value)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CheckSector(Board board, int x, int y, byte value)
        {
            var sectorX = x / Constants.SectorSize;
            var sectorY = y / Constants.SectorSize;

            for (var ix = sectorX * Constants.SectorSize; ix < (sectorX + 1) * Constants.SectorSize; ix++)
            {
                for (var iy = sectorY * Constants.SectorSize; iy < (sectorY + 1) * Constants.SectorSize; iy++)
                {
                    if (board.Peek(ix, iy) == value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}