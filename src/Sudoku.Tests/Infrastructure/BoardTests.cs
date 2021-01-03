using Sudoku.Exceptions;
using Sudoku.Infrastructure;
using Xunit;

namespace Sudoku.Tests.Infrastructure
{
    public class BoardTests
    {
        [Fact]
        public void Poke_throws_exception_on_illegal_move()
        {
            var board = new Board();

            board.Poke(0, 0, 1);

            Assert.Throws<BoardException>(() => board.Poke(0, 1, 1));
        }
    }
}