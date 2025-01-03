namespace Sudoku.Console;

public static class FileHelper
{
    public static string GetPuzzlesPath()
    {
        var path = "Puzzles";

        while (! Path.Exists(path))
        {
            path = $"..{Path.DirectorySeparatorChar}{path}";
        }

        return path;
    }

    public static string GetSupportingFilesPath()
    {
        const string path = "Supporting Files";

        return Path.Exists(path) ? path : $"src/Sudoku.Console/{path}";
    }
}