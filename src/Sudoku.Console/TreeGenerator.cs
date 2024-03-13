using Sudoku.Console.Supporting_Files;

namespace Sudoku.Console;

public class TreeGenerator
{
    private string _visualisation;

    private Node _root;
    
    public void Generate(int[] puzzle, string filename)
    {
        var solver = new Solver(HistoryType.AllSteps, true);

        var result = solver.Solve(puzzle);
        
        GenerateNodes(puzzle, result);
        
        _visualisation = File.ReadAllText("Supporting Files\\Template.html");

        _visualisation = _visualisation.Replace("{css}", File.ReadAllText("Supporting Files\\Styles.css"));

        //_visualisation = _visualisation.Replace("{nodes}", $"<ul>{ProcessNode(_tree.Root)}</ul>");

        File.WriteAllText($"{filename}.html", _visualisation);
    }

    private void GenerateNodes(int[] puzzle, SudokuResult result)
    {
        var node = new Node(puzzle);

        _root = node;
        
        foreach (var move in result.History)
        {
            
        }
    }
}