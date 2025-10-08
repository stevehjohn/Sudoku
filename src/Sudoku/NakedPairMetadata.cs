namespace Sudoku;

public struct NakedPairMetadata
{
    public List<int> Pair { get; }

    public List<int> Affected { get; }
    
    public int UnitIndex { get; }
    
    public NakedPairMetadata(int unitIndex)
    {
        UnitIndex = unitIndex;
        
        Pair = [];
        
        Affected = [];
    }
}