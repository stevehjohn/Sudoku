using Out = System.Console;

namespace Sudoku.Console;

public class ConsoleSolveVisualiser
{
    private readonly int[] _puzzle;

    private readonly List<Move> _history;

    private readonly List<int>[] _initialCandidates;

    private int _left;

    private int _top;

    private readonly ConsoleColor _consoleColor;
    
    public ConsoleSolveVisualiser(int[] puzzle, List<Move> history, List<int>[] initialCandidates)
    {
        _puzzle = puzzle;
        
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
        
        Out.CursorTop = 39;
        
        Out.CursorVisible = true;
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
        Out.ForegroundColor = ConsoleColor.Blue;
        
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

        Out.CursorTop = top;

        Out.CursorLeft = 1;
        
        Out.Write("┗━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┻━━━━━┷━━━━━┷━━━━━┛ ");
    }
}