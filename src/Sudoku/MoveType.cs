namespace Sudoku;

public enum MoveType
{
    None,
    NakedSingle,
    HiddenSingle,
    Guess,
    Backtrack,
    NoCandidates
}