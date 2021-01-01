using System;

namespace Sudoku.Infrastructure
{
    public class Board
    {
        private byte[] _board;

        public Board()
        {
            _board = new byte[Constants.BoardSize * Constants.BoardSize];
        }

        public byte Peek(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void Poke(int x, int y, byte value)
        {
        }
    }
}