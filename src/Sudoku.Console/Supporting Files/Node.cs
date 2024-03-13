namespace Sudoku.Console.Supporting_Files;

public class Node
{
    private readonly List<Node> _children = [];

    private readonly Node _parent;

    private readonly int[] _puzzleState = new int[81];
    
    public Move Move { get; }

    public IReadOnlyList<Node> Children => _children;

    public Node(int[] puzzleState)
    {
        for (var i = 0; i < 81; i++)
        {
            _puzzleState[i] = puzzleState[i];
        }
    }

    public Node(Move move)
    {
        Move = move;
    }

    private Node(Move move, Node parent)
    {
        Move = move;

        _parent = parent;
    }

    public int this[int index] => _puzzleState[index];

    public void AddChild(Move child)
    {
        _children.Add(new Node(child, this));
    }

    public Node Parent()
    {
        return _parent;
    }
}