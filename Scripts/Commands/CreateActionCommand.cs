using BuildLib.Utils;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Utilities.Text.Yaml;
using Spectre.Console;

namespace Scripts.Commands;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class CreateActionCommand(Solution solution, ILogger<CreateActionCommand> logger)
{
    public async Task ExecuteAsync(string actionName)
    {
        var actionsDir = solution.Directory.GetDirectories().Single(x => x.Name is "actions");
        var actionDir = actionsDir / actionName;

        var shouldContinue = CheckExistingDirectory();
        if (!shouldContinue)
        {
            return;
        }

        actionDir.CreateDirectory();
        var actionFile = actionDir / "action.yml";
        actionFile.TouchFile();
        var yaml = CreateYaml();
        logger.LogInformation("Creating action {ActionName} at {ActionFile} with text:\n {Text}",
            actionName,
            actionFile,
            yaml
        );

        actionFile.WriteAllText(yaml);
        await Task.CompletedTask;
        return;

        bool CheckExistingDirectory()
        {
            if (!actionDir.Exists()) return true;

            var prompt = new SelectionPrompt<bool>()
                .Title($"The file [green]{actionDir}[/] already exists. Do you want to overwrite it?")
                .AddChoices(true, false);
            var shouldOverwrite = AnsiConsole.Prompt(prompt);
            if (!shouldOverwrite)
            {
                logger.LogInformation("Exiting without creating action");
                return false;
            }

            logger.LogInformation("Overwriting existing action {actionDir}", actionDir);

            return true;
        }

        string CreateYaml()
        {
            var titleCaseName = actionName.Humanize(LetterCasing.Title);

            var json = new
            {
                name = titleCaseName,
                runs = new Dictionary<string, object>
                {
                    ["using"] = "composite",
                    ["steps"] = new[] { new { name = titleCaseName } }
                }
            };
            return json.ToYaml();
        }
    }
}