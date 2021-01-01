using Sudoku.Engine;
using Sudoku.Infrastructure;
using Xunit;

namespace Sudoku.Tests.Engine
{
    public class LogicTests
    {
        [Fact]
        public void IsLegalMove_will_not_allow_duplicate_X_values()
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(4, 0, 1);

            Assert.False(Logic.IsLegalMove(board, 0, 0, 1));
        }

        [Fact]
        public void IsLegalMove_will_not_allow_duplicate_Y_values()
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(0, 4, 1);

            Assert.False(Logic.IsLegalMove(board, 0, 0, 1));
        }

        [Fact]
        public void IsLegalMove_will_not_allow_duplicate_sector_values()
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(1, 1, 1);

            Assert.False(Logic.IsLegalMove(board, 0, 0, 1));
        }
    }
}