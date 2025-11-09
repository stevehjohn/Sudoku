// ReSharper disable AccessToDisposedClosure

namespace Sudoku;

public static class BulkGenerator
{
    public static void Generate(int quantity, int cluesToLeave, Action<int[]> callback)
    {
        var workers = Environment.ProcessorCount / 2;

        var count = 0;

        using var cancellationTokenSource = new CancellationTokenSource();

        var cancellationToken = cancellationTokenSource.Token;
        
        for (var i = 0; i < workers; i++)
        {
            var generator = new Generator();

            Task.Run(() =>
            {
                while (! cancellationToken.IsCancellationRequested)
                {
                    if (Volatile.Read(ref count) >= quantity)
                    {
                        cancellationTokenSource.Cancel();
                        
                        break;
                    }

                    var result = generator.Generate(cluesToLeave, cancellationToken);

                    if (! result.Succeeded || cancellationToken.IsCancellationRequested)
                    {
                        continue;
                    }

                    callback(result.Puzzle);

                    if (Interlocked.Increment(ref count) >= quantity)
                    {
                        cancellationTokenSource.Cancel();
                        
                        break;
                    }
                }
            }, cancellationToken);
        }
    }
}