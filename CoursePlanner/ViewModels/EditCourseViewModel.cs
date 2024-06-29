using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Microsoft.EntityFrameworkCore;
namespace CoursePlanner.ViewModels;

public partial class EditCourseViewModel(ILocalDbCtxFactory factory, AppService appShell)
    : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private DateTime _start;

    [ObservableProperty]
    private DateTime _end;
    
    [ObservableProperty]
    private bool _shouldNotify;

    [RelayCommand]
    public async Task SaveAsync()
    {
        var db = await factory.CreateDbContextAsync();
        var course = await db
           .Courses
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        course.Name = Name;
        course.Start = Start;
        course.End = End;
        course.ShouldNotify = ShouldNotify;

        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await appShell.PopAsync();
    }

    public async Task Init(int id)
    {
        await using var db = await factory.CreateDbContextAsync();

        var course = await db
               .Courses
               .FirstOrDefaultAsync(x => x.Id == id) ??
            new();

        Id = course.Id;
        Name = course.Name;
        Start = course.Start;
        End = course.End;
        ShouldNotify = course.ShouldNotify;
    }
    
    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}