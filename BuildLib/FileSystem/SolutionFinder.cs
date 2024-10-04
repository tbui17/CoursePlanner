namespace BuildLib.FileSystem;

public class SolutionFinder(SolutionFileName solutionFileName)
{
    public DirectoryInfo StartingDirectory { get; set; } = new(".");

    public FileInfo? FindSolutionFile(CancellationToken token = default)
    {
        var fileName = solutionFileName.GetValue();


        for (var directory = StartingDirectory; directory is not null; directory = directory.Parent)
        {
            token.ThrowIfCancellationRequested();
            var children = directory.EnumerateFiles();
            var solutionFile = children.FirstOrDefault(x =>
                x.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase) &&
                x.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase)
            );

            if (solutionFile is not null)
            {
                return solutionFile;
            }
        }

        return null;
    }
}