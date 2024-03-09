using Out = System.Console;

namespace Sudoku.Console;

public class ConsoleSolveVisualiser
{
    private readonly int[] _puzzle;

    private List<Move> _history;

    private int _left;

    private int _top;

    public ConsoleSolveVisualiser(int[] puzzle, List<Move> history)
    {
        _puzzle = puzzle;
        
        _history = history;
    }

    public void Visualise(int left, int top)
    {
        _left = left;

        _top = top;
        
        Out.CursorVisible = false;
        
        Out.Clear();
        
        DrawBox();

        DrawPuzzle();

        Out.CursorTop = 39;
        
        Out.CursorVisible = true;
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