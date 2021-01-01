using System;

namespace Sudoku.Exceptions
{
    public class BoardException : Exception
    {
        public BoardException(string message) : base(message)
        {
        }
    }
}