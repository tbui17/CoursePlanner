using CommunityToolkit.Maui.Core.Extensions;
using CoursePlanner.Services;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner;

public partial class DevPage : ContentPage
{
    public DevPage(IServiceProvider provider, ILocalDbCtxFactory factory, AppService appService, ILocalNotificationService notificationService)
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

    private AppService ApplicationService { get; set; }

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
                await using var db = await Factory.CreateDbContextAsync();
                await db.Database.EnsureDeletedAsync();
                await db.Database.MigrateAsync();
                await ApplicationService.AlertAsync("Database has been reset. Closing application.");
                App.Current.Quit();
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
                await db.SaveChangesAsync();
            },
            ["Share"] = async () =>
            {
                await using var db = await Factory.CreateDbContextAsync();

                var query = db
                   .Notes
                   .Include(x => x.Course)
                   .ThenInclude(x => x.Instructor)
                   .Select(x => new ShareNote(x));

                var note = await query.FirstAsync();


                await ApplicationService.ShareAsync(new ShareTextRequest
                    {
                        Title = "Share",
                        Text = note.ToFriendlyText()
                    }
                );
            },
            ["Trigger Notification Service"] = async () => await NotificationService.Notify(),
            ["Seed database"] = async () =>
            {
                await using var db = await Factory.CreateDbContextAsync();
                await new TestDataFactory().SeedDatabase(db);
                await ApplicationService.AlertAsync("Database has been seeded.");

            }
        };
    }
}