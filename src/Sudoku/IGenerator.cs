namespace Sudoku;

public interface IGenerator
{
    (bool Succeeded, int[] Puzzle) Generate(int[] solvedPuzzle, int cluesToLeave, CancellationToken cancellationToken);

    public bool CreateSolvedPuzzle(Span<int> puzzle, CancellationToken cancellationToken);
}