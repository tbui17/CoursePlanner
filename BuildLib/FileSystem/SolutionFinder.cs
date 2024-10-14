using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;

namespace BuildLib.FileSystem;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class DirectoryService(ILogger<DirectoryService> logger)
{
    public IEnumerable<FileInfo> GetFilesInAncestorDirectories(
        FindFileArgs args
    )
    {
        logger.LogDebug("Searching for solution files in {Directory}", args.StartingDirectory.FullName);
        for (var current = args.StartingDirectory; current is not null; current = current.Parent)
        {
            var solutionFiles = current.EnumerateFiles(args.FileName);
            foreach (var solutionFile in solutionFiles)
            {
                args.Token.ThrowIfCancellationRequested();
                yield return solutionFile;
            }
        }
    }

    public Solution GetSolution()
    {
        var solutionFile =
            GetFilesInAncestorDirectories(new FindSolutionArgs() { FileName = "*.sln" }).FirstOrDefault() ??
            throw new FileNotFoundException("No solution file found");

        var solution = SolutionModelTasks.ParseSolution(solutionFile.FullName);
        logger.LogDebug("Found solution {Solution}", solution);

        return solution;
    }

    public FileInfo GetOrThrowSolutionFile(FindSolutionArgs args)
    {
        logger.LogDebug("Finding solution file {FileName}", args.FileName);
        return GetFilesInAncestorDirectories(args).FirstOrDefault() ??
               throw new FileNotFoundException($"No solution file found in {args.StartingDirectory}");
    }

    public IEnumerable<FileInfo> GetFilesInDescendantDirectories(
        FindFileArgs args
    )
    {
        logger.LogDebug("Searching for project files in {Directory}", args.StartingDirectory.FullName);


        return new Matcher()
            .AddInclude($"**/{args.FileName}")
            .AddExclude("**/bin/**/*")
            .GetResultsInFullPath(args.StartingDirectory.FullName)
            .Select(x =>
                {
                    args.Token.ThrowIfCancellationRequested();
                    return new FileInfo(x);
                }
            );
    }

    public FileInfo GetOrThrowProjectFile(FindProjectArgs args)
    {
        return GetFilesInDescendantDirectories(args)
                   .FirstOrDefault() ??
               throw new FileNotFoundException($"No project file found in {args.StartingDirectory}");
    }
}