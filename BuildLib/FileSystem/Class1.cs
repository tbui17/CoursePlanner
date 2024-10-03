namespace BuildLib.FileSystem;

public class SolutionFinder(SolutionFileName solutionFileName)
{
    public DirectoryInfo StartingDirectory { get; set; } = new(".");

    public FileInfo? FindSolutionFile(CancellationToken token = default)
    {
        var fileName = solutionFileName.GetValue();
        var directory = StartingDirectory;

        while (directory is not null)
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

            directory = directory.Parent;
        }

        return null;
    }
}