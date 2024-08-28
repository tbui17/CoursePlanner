using CommunityToolkit.Maui.Core.Extensions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services;
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
        INavigationService navService,
        ILocalNotificationService notificationService,
        ISessionService sessionService,
        AccountService accountService
        )
    {
        Provider = provider;
        Factory = factory;
        ApplicationService = appService;
        NotificationService = notificationService;
        SessionService = sessionService;
        AccountService = accountService;
        NavService = navService;


        Actions = CreateActions();


        InitializeComponent();


        Input.ItemsSource = Actions.Keys.ToObservableCollection();
        Input.SelectedIndex = 0;
    }

    public INavigationService NavService { get; set; }

    public AccountService AccountService { get; set; }

    public ISessionService SessionService { get; set; }

    public ILocalNotificationService NotificationService { get; set; }

    private Dictionary<string, Func<Task>> Actions { get; }

    private ILocalDbCtxFactory Factory { get; set; }

    private IAppService ApplicationService { get; set; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private IServiceProvider Provider { get; set; }

    private async void Button_OnClicked(object? sender, EventArgs e)
    {
        string text;
        try
        {
            text = (string)Input.SelectedItem;
        }
        catch (Exception ex)
        {
            await ApplicationService.AlertAsync("Error parsing command: " + ex.Message);
            return;
        }


        if (Actions.TryGetValue(text, out var action))
        {
            try
            {
                await action();
                return;
            }
            catch (Exception exc)
            {
                await ApplicationService.AlertAsync("Error executing command: " + exc.Message);
                return;
            }
        }

        await ApplicationService.AlertAsync($"Command not found. {text}");
    }

    private Dictionary<string, Func<Task>> CreateActions()
    {
        // must restart after resetting in MAUI; ID sequence is not reset without app restart
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


                try
                {
                    var course = await db
                        .Courses
                        .AsTracking()
                        .OrderBy(x => x.Id)
                        .FirstOrDefaultAsync();

                    if (course is null)
                    {
                        await ApplicationService.AlertAsync(
                            "No courses found. Did you forget to create test data in the database?");
                        return;
                    }

                    course.ShouldNotify = true;
                    course.Start = DateTime.Now.Date;
                    course.End = DateTime.Now.Date;
                    await db.SaveChangesAsync();
                    await ApplicationService.AlertAsync("Notification data has been set.");
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync("Error setting notification data: " + e.Message);
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
            ["Create C6 Test Data"] = async () =>
            {
                try
                {
                    await using var db = await Factory.CreateDbContextAsync();
                    await db.Database.EnsureCreatedAsync();

                    var factory = new TestDataFactory();
                    var data = factory.CreateC6Data();
                    db.Terms.Add(data);
                    await db.SaveChangesAsync();
                    await ApplicationService.AlertAsync("Test data has been created.");
                }
                catch (Exception e)
                {
                    await ApplicationService.AlertAsync(
                        "There was an error creating the test data. Did you forget to reset the database? " +
                        e.Message);
                }
            },
            ["Nav Data Page"] = async () =>
            {
                var login = new LoginDetails("testaccount", "testpass");
                await AccountService.CreateAsync(login);
                await SessionService.LoginAsync(login);
                await Shell.Current.GoToAsync($"///{nameof(NotificationDataPage)}");
                var page = (NotificationDataPage)Shell.Current.CurrentPage;
                page.Model.MonthDate = new DateTime(2023, 9, 1);
            }
        };
    }
}