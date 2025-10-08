namespace Sudoku;

public enum MoveType
{
    None,
    NakedSingle,
    HiddenSingle,
    Guess,
    Backtrack,
    NakedPairRow,
    NakedPairColumn,
    NakedPairBox,
    NoCandidates
}