using Out = System.Console;

namespace Sudoku.Console;

public class ConsoleSolveVisualiser
{
    private readonly int[] _puzzle;

    private readonly SudokuResult _solution;

    private readonly List<Move> _history;

    private readonly List<int>[] _initialCandidates;

    private int _left;

    private int _top;

    private readonly ConsoleColor _consoleColor;

    private readonly int[] _speeds = [ 2000, 1000, 500, 250, 100, 50, 10, 0 ];

    private int _speedIndex = 1;
    
    public ConsoleSolveVisualiser(int[] puzzle, SudokuResult solution, List<Move> history, List<int>[] initialCandidates)
    {
        _puzzle = puzzle;

        _solution = solution;
        
        _history = history;

        _initialCandidates = initialCandidates;

        _consoleColor = Out.ForegroundColor;
    }

    public void Visualise(int left, int top)
    {
        _left = left;

        _top = top;
        
        Out.CursorVisible = false;
        
        Out.Clear();
        
        DrawBox();

        DrawPuzzle();

        DrawInitialCandidates();

        ShowText("Press S to start");

        while (char.ToLower(Out.ReadKey(true).KeyChar) != 's')
        {
        }
        
        ShowText("");

        Run();
        
        Finish();
        
        Out.CursorTop = 44;
        
        Out.CursorVisible = true;
    }

    private void Finish()
    {
        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (_puzzle[x + y * 9] == 0)
                {
                    SetCursorPosition(x, y, _solution[x + y * 9]);

                    Out.Write(_solution[x + y * 9]);
                }
            }
        }
    }

    private void Run()
    {
        var step = 0;
        
        foreach (var move in _history)
        {
            step++;
            
            SetCursorPosition(move.X, move.Y, move.Value);

            if (move.Remove)
            {
                Out.ForegroundColor = ConsoleColor.Blue;
            }
            else
            {
                Out.BackgroundColor = ConsoleColor.Green;

                Out.ForegroundColor = ConsoleColor.Black;
            }

            Out.Write(move.Value);

            Out.BackgroundColor = ConsoleColor.Black;
            
            Out.ForegroundColor = _consoleColor;
            
            ShowText($"Step {step:N0}/{_history.Count:N0}  ({1_000d / _speeds[_speedIndex]:N1} fps)");

            Thread.Sleep(_speeds[_speedIndex]);

            if (Out.KeyAvailable)
            {
                var key = Out.ReadKey(true).KeyChar;

                if (key is '<' or ',' && _speedIndex > 0)
                {
                    _speedIndex--;
                }

                if (key is '>' or '.' && _speedIndex < _speeds.Length - 1)
                {
                    _speedIndex++;
                }
            }
        }
    }

    private void ShowText(string text)
    {
        Out.CursorTop = 1;

        Out.CursorLeft = _left;

        var whitespace = new string(' ', 28 - text.Length / 2);
        
        Out.Write($"{whitespace}{text}{whitespace}");
    }

    private void DrawInitialCandidates()
    {
        Out.ForegroundColor = ConsoleColor.Magenta;
        
        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (_initialCandidates[x + y * 9] != null)
                {
                    foreach (var candidate in _initialCandidates[x + y * 9])
                    {
                        SetCursorPosition(x, y, candidate);
                        
                        Out.Write(candidate);
                    }
                }
            }
        }

        Out.ForegroundColor = _consoleColor;
    }

    private void SetCursorPosition(int x, int y, int value)
    {
        var top = _top + 2 + y * 4;

        var left = _left + 1 + x * 6;

        top += (value - 1) / 3;

        left += (value - 1) % 3 * 2;
        
        Out.CursorTop = top;

        Out.CursorLeft = left;
    }

    private void DrawPuzzle()
    {
        for (var y = 0; y < 9; y++)
        {
            for (var x = 0; x < 9; x++)
            {
                if (_puzzle[x + y * 9] != 0)
                {
                    Out.CursorTop = _top + 3 + y * 4;

                    Out.CursorLeft = _left + 3 + x * 6;
                    
                    Out.Write(_puzzle[x + y * 9]);
                }
            }
        }
    }

    private void DrawBox()
    {
        var top = _top + 1;

        Out.ForegroundColor = ConsoleColor.DarkBlue;
        
        Out.CursorTop = top++;

        Out.CursorLeft = _left;
        
        Out.Write("┏━━━━━┯━━━━━┯━━━━━┳━━━━━┯━━━━━┯━━━━━┳━━━━━┯━━━━━┯━━━━━┓");

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                for (var k = 0; k < 3; k++)
                {
                    Out.CursorTop = top++;

                    Out.CursorLeft = 1;

                    Out.Write("┃     │     │     ┃     │     │     ┃     │     │     ┃");
                }

                if (j < 2)
                {
                    Out.CursorTop = top++;

                    Out.CursorLeft = 1;

                    Out.Write("┠─────┼─────┼─────╂─────┼─────┼─────╂─────┼─────┼─────┨");
                }
            }

            if (i < 2)
            {
                Out.CursorTop = top++;

                Out.CursorLeft = 1;

                Out.Write("┣━━━━━┿━━━━━┿━━━━━╋━━━━━┿━━━━━┿━━━━━╋━━━━━┿━━━━━┿━━━━━┫");
            }
        }

        Out.CursorTop = top++;

        Out.CursorLeft = 1;
        
        Out.Write("┗━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┛ ");

        Out.CursorTop = top;

        Out.CursorLeft = 1;

        Out.ForegroundColor = _consoleColor;

        Out.ForegroundColor = ConsoleColor.Blue;
        
        Out.Write("\n                         tested");

        Out.ForegroundColor = ConsoleColor.Magenta;
        
        Out.Write("\n                        untested");

        Out.ForegroundColor = _consoleColor;
        
        Out.Write("\n\n           < or > adjust visualisation speed");
    }
}