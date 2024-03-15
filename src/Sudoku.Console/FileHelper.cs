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

        if (Path.Exists(path))
        {
            return path;
        }

        return $"src/Sudoku.Console/{path}";
    }
}