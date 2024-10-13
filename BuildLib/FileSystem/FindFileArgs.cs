using BuildLib.Utils;
using FluentValidation;

namespace BuildLib.FileSystem;

public record FindFileArgs
{
    public required string FileName { get; init; }
    public DirectoryInfo StartingDirectory { get; init; } = new(Directory.GetCurrentDirectory());
    public CancellationToken Token { get; init; }
    public static implicit operator FindFileArgs(string fileName) => new() { FileName = fileName };
}

public record FindSolutionArgs : FindFileArgs
{
    internal FindSolutionArgs()
    {
    }
}

public record FindProjectArgs : FindFileArgs
{
    internal FindProjectArgs()
    {
    }
}

public static class FindFileArgExtensions
{
    public static T WithCurrentStartingDirectory<T>(this T args) where T : FindFileArgs
    {
        return args with { StartingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()) };
    }
}

[Inject]
public class FileArgFactory(
    SolutionFileValidator solutionValidator,
    ProjectFileValidator projectFileValidator)
{
    public FindSolutionArgs CreateSolution(FindFileArgs args)
    {
        var solution = new FindSolutionArgs
            { FileName = args.FileName, StartingDirectory = args.StartingDirectory, Token = args.Token };
        solutionValidator.ValidateAndThrow(solution.FileName);
        return solution;
    }

    public FindProjectArgs CreateProject(FindFileArgs args)
    {
        var project = new FindProjectArgs
            { FileName = args.FileName, StartingDirectory = args.StartingDirectory, Token = args.Token };
        projectFileValidator.ValidateAndThrow(project.FileName);
        return project;
    }
}