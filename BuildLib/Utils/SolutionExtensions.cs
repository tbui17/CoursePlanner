using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Serilog;

namespace BuildLib.Utils;

public static class SolutionExtensions
{
    public static AbsolutePath GetOutputDirectory(this Solution solution)
    {
        return solution
            .Directory
            .GetDirectories()
            .Single(x => x.Name.EqualsIgnoreCase("output"));
    }

    public static AbsolutePath GetSignedAabFile(this Solution solution)
    {
        return solution
            .GetOutputDirectory()
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