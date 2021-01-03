using Sudoku.Engine;
using Sudoku.Infrastructure;
using Xunit;

namespace Sudoku.Tests.Engine
{
    public class LogicTests
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(8, true)]
        public void IsLegalMove_will_not_allow_duplicate_X_values(int y, bool legal)
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(4, 0, 1);

            Assert.Equal(legal, Logic.IsLegalMove(board, 0, y, 1));
        }

        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(4, true)]
        [InlineData(5, true)]
        [InlineData(6, true)]
        [InlineData(7, true)]
        [InlineData(8, true)]
        public void IsLegalMove_will_not_allow_duplicate_Y_values(int x, bool legal)
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(0, 4, 1);

            Assert.Equal(legal, Logic.IsLegalMove(board, x, 0, 1));
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(1, 0, false)]
        [InlineData(2, 0, false)]
        [InlineData(3, 0, true)]
        [InlineData(0, 1, false)]
        [InlineData(1, 1, false)]
        [InlineData(2, 1, false)]
        [InlineData(3, 1, false)]
        [InlineData(0, 2, false)]
        [InlineData(1, 2, false)]
        [InlineData(2, 2, false)]
        [InlineData(3, 2, true)]
        [InlineData(0, 3, true)]
        [InlineData(1, 3, false)]
        [InlineData(2, 3, true)]
        [InlineData(3, 3, true)]
        public void IsLegalMove_will_not_allow_duplicate_sector_values(int x, int y, bool legal)
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));

            board.Poke(1, 1, 1);

            Assert.Equal(legal, Logic.IsLegalMove(board, x, y, 1));
        }
    }
}