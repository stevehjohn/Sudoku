namespace Sudoku;

public enum MoveType
{
    None,
    NakedSingle,
    HiddenSingle,
    Guess,
    Backtrack,
    XWing,
    NoCandidates
}