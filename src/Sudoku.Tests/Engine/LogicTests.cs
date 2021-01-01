using Sudoku.Engine;
using Sudoku.Infrastructure;
using Sudoku.Tests.Infrastructure;
using Xunit;

namespace Sudoku.Tests.Engine
{
    public class LogicTests
    {
        private readonly Board _board;

        public LogicTests()
        {
            _board = new Board();
        }

        [Fact]
        public void IsLegalMove_will_not_allow_duplicate_X_values()
        {
            var board = new Board();

            Assert.True(Logic.IsLegalMove(board, 0, 0, 1));
        }

        [Fact]
        public void IsLegalMove_will_not_allow_duplicate_Y_values()
        {
        }
    }
}