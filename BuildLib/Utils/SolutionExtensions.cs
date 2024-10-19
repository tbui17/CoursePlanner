using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

namespace BuildLib.Utils;

public static class SolutionExtensions
{
    public static AbsolutePath GetOrCreateOutputDirectory(this Solution solution)
    {
        var dir = solution
            .Directory
            .GetDirectories()
            .SingleOrDefault(x => x.Name.EqualsIgnoreCase("output"));

        if (dir is not null)
        {
            return dir;
        }

        var outputDir = solution.Directory / "output";
        return outputDir.CreateDirectory();
    }

    public static AbsolutePath CleanOutputDirectory(this Solution solution)
    {
        var dir = solution
            .Directory
            .GetDirectories()
            .SingleOrDefault(x => x.Name.EqualsIgnoreCase("output"));

        if (dir is not null)
        {
            return dir;
        }

        var outputDir = solution.Directory / "output";
        return outputDir.CreateOrCleanDirectory();
    }

    public static AbsolutePath GetSignedAabFile(this Solution solution)
    {
        return solution
            .GetOrCreateOutputDirectory()
            .GetFiles("*.aab")
            .Single(x => x.Name.ContainsIgnoreCase("signed"));
    }

    public static Project GetProjectWithValidation(this Solution solution, string project)
    {
        if (project.EndsWith(".csproj"))
        {
            var prev = project;


            project = project.Replace(".csproj", "");
            Log
                .ForContext<Solution>()
                .Information(
                    "Automatically adjusted project argument {Project} to {NewProject} for retrieval of project in solution {Solution}",
                    prev,
                    project,
                    solution
                );
        }

        return solution.GetProject(project).NotNull();
    }
}