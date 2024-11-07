using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.KeyVault;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Spectre.Console;

namespace BuildLib.Commands;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class PopulateKeyUriCommand
{
    public async Task ExecuteAsync()
    {
        var log = Log.ForContext<PopulateKeyUriCommand>();

        var armClient = new ArmClient(new AzureCliCredential());
        var subscription = await armClient.GetDefaultSubscriptionAsync();
        var keyVaults = await subscription
            .GetKeyVaultsAsync()
            .Select(x => x.Data.Properties.VaultUri.ToString())
            .ToListAsync();

        log.Debug("Found {Count} key vaults.", keyVaults.Count);

        if (keyVaults.Count is 0)
        {
            throw new InvalidDataException("No key vaults found.");
        }

        var prompt = new SelectionPrompt<string>()
            .Title($"Select a key vault. ({keyVaults.Count} found)")
            .AddChoices(keyVaults);


        if (AnsiConsole.Prompt(prompt) is not { } choice)
        {
            return;
        }

        var config = new AppConfiguration
        {
            AzureKeyVaultConfiguration = new()
            {
                Uri = choice
            }
        };

        var manager = new UserSecretManager<Container>();
        var file = manager.GetUserSecretFile();
        file.Merge(config);
        manager.Save(file);
    }
}