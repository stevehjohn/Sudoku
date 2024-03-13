namespace Sudoku.Console.Supporting_Files;

public class Node
{
    private readonly List<Node> _children = [];

    private readonly int[] _puzzleState = new int[81];
    
    public Move Move { get; }

    public Node Parent { get; }

    public IReadOnlyList<Node> Children => _children;

    public Node(int[] puzzleState)
    {
        for (var i = 0; i < 81; i++)
        {
            _puzzleState[i] = puzzleState[i];
        }
    }
    
    private Node(Move move, Node parent)
    {
        Move = move;

        Parent = parent;

        for (var i = 0; i < 81; i++)
        {
            _puzzleState[i] = parent._puzzleState[i];
        }

        _puzzleState[move.X + move.Y * 9] = move.Type == MoveType.Backtrack ? 0 : move.Value;
    }

    public int this[int index] => _puzzleState[index];

    public Node AddChild(Move child)
    {
        var node = new Node(child, this);
        
        _children.Add(node);

        return node;
    }
}