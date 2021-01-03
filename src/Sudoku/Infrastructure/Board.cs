using System;
using Sudoku.Engine;
using Sudoku.Exceptions;

namespace Sudoku.Infrastructure
{
    public class Board
    {
        private readonly byte[] _board;

        public Board()
        {
            _board = new byte[Constants.BoardSize * Constants.BoardSize];
        }

        public byte Peek(int x, int y)
        {
            return _board[x + y * Constants.BoardSize];
        }

        public void Poke(int x, int y, byte value)
        {
            if (! Logic.IsLegalMove(this, x, y, value))
            {
                throw new BoardException("This move is not valid.");
            }

            _board[x + y * Constants.BoardSize] = value;
        }

        public void LoadState(byte[] board)
        {
            Buffer.BlockCopy(board, 0, _board, 0, _board.Length);
        }

        public byte[] SaveState()
        {
            var state = new byte[_board.Length];

            Buffer.BlockCopy(_board, 0, state, 0, _board.Length);

            return state;
        }
    }
}