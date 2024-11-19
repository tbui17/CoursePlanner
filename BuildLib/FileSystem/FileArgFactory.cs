using BuildLib.Utils;
using FluentValidation;

namespace BuildLib.FileSystem;

public interface IFileArgFactory
{
    FindSolutionArgs CreateSolution(FindFileArgs args);
    FindProjectArgs CreateProject(FindFileArgs args);
}

[Inject(typeof(IFileArgFactory))]
public class FileArgFactory(
    SolutionFileValidator solutionValidator,
    ProjectFileValidator projectFileValidator) : IFileArgFactory
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