using System;
using Sudoku.Infrastructure;

namespace Sudoku.Engine
{
    public static class Logic
    {
        public static bool IsLegalMove(Board board, int x, int y, byte value)
        {
            return CheckX() && CheckY() && CheckSector();
        }

        private static bool CheckX()
        {
            throw new NotImplementedException();
        }

        private static bool CheckY()
        {
            throw new NotImplementedException();
        }

        private static bool CheckSector()
        {
            throw new NotImplementedException();
        }
    }
}