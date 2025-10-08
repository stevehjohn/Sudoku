namespace Sudoku;

public struct NakedPairMetadata
{
    public List<int> Pair { get; }

    public List<int> Affected { get; }
    
    public NakedPairMetadata()
    {
        Pair = [];
        
        Affected = [];
    }
}