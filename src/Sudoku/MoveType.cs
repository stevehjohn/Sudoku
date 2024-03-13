namespace Sudoku;

public enum MoveType
{
    None,
    LastPossibleNumber,
    HiddenSingle,
    Guess,
    Backtrack
}