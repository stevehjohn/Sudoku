namespace Sudoku;

public struct NakedPairMetadata
{
    public List<int> Pair { get; set; }

    public List<int> Affected { get; set; }

    public NakedPairMetadata()
    {
        Pair = [];
        
        Affected = [];
    }
}