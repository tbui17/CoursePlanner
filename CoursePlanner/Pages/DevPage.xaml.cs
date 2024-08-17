using CommunityToolkit.Maui.Core.Extensions;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace CoursePlanner.Pages;

public partial class DevPage
{
    public DevPage(
        IServiceProvider provider,
        ILocalDbCtxFactory factory,
        IAppService appService,
        ILocalNotificationService notificationService)
    {
        Provider = provider;
        Factory = factory;
        ApplicationService = appService;
        NotificationService = notificationService;

        Actions = CreateActions();


        InitializeComponent();


        Input.ItemsSource = Actions.Keys.ToObservableCollection();
        Input.SelectedIndex = 0;
    }

    public ILocalNotificationService NotificationService { get; set; }

    private Dictionary<string, Func<Task>> Actions { get; }

    private ILocalDbCtxFactory Factory { get; set; }

    private IAppService ApplicationService { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private IServiceProvider Provider { get; set; }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        var text = (string)Input.SelectedItem;

        if (Actions.TryGetValue(text, out var action))
        {
            await action();
        }
        else
        {
            await ApplicationService.AlertAsync("Command not found.");
        }
    }

    private Dictionary<string, Func<Task>> CreateActions()
    {
        return new()
        {
            ["Reset"] = async () =>
            {
                try
                {
                    await using var db = await Factory.CreateDbContextAsync();
                    await db.Database.EnsureDeletedAsync();
                    await db.Database.MigrateAsync();
                    await ApplicationService.AlertAsync(
                        "Database has been reset. Open the application and seed the database afterwards. Closing application now.");
                }

                catch (Exception e)
                {
                    await ApplicationService.AlertAsync(
                        "There was an error attempting to reset the database. Application closing." +
                        Environment.NewLine + e.Message);
                }
                finally
                {
                    Application.Current!.Quit();
                }
            },
            ["Set Notification Data"] = async () =>
            {
                await using var db = await Factory.CreateDbContextAsync();
                var course = await db
                    .Courses
                    .AsTracking()
                    .OrderBy(x => x.Id)
                    .FirstAsync();

                course.ShouldNotify = true;
                course.Start = DateTime.Now.AddHours(3);
                course.End = DateTime.Now.AddHours(6);

                var success = false;
                try
                {
                    await db.SaveChangesAsync();
                    success = true;
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error setting notification data: " + e.Message);
                }

                if (success)
                {
                    await ApplicationService.AlertAsync("Notification data has been set.");
                }
            },
            ["Share"] = async () =>
            {
                ShareNote note;
                try
                {
                    await using var db = await Factory.CreateDbContextAsync();

                    var query = db
                        .Notes
                        .Include(x => x.Course)
                        .ThenInclude(x => x.Instructor)
                        .Select(x => new ShareNote(x));

                    note = await query.FirstAsync();
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error getting note: " + e.Message);
                    return;
                }


                try
                {
                    await ApplicationService.ShareAsync(new ShareTextRequest
                        {
                            Title = "ShareNote12345",
                            Text = note.ToFriendlyText()
                        }
                    );
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error sharing note: " + e.Message);
                }
            },
            ["Trigger Notification Service"] = async () =>
            {
                int count;
                try
                {
                    count = await NotificationService.SendUpcomingNotifications();
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error triggering notification service: " + e.Message);
                    return;
                }

                await ApplicationService.AlertAsync(
                    $"{count} Notifications found. Batched and sent notifications if any.");
            },
            ["Seed database"] = async () =>
            {
                try
                {
                    await using var db = await Factory.CreateDbContextAsync();
                    await new TestDataFactory().SeedDatabase(db);
                    await ApplicationService.AlertAsync("Database has been seeded.");
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync(
                        "There was an error attempting to seed the database. Did you forget to reset before using this action?" +
                        Environment.NewLine + e.Message);
                }
            },
            ["Create Test Data"] = async () =>
            {
                try
                {
                    await using var db = await Factory.CreateDbContextAsync();
                    await db.Database.EnsureCreatedAsync();

                    await new TestDataFactory().SeedDatabase(db);
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error creating test data: " + e.Message);
                }
            }
        };
    }
}