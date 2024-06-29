using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoursePlanner.Services;
using Microsoft.EntityFrameworkCore;

namespace CoursePlanner.ViewModels;

public partial class EditAssessmentViewModel(ILocalDbCtxFactory factory, AppService appShell)
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
        var assessment = await db
           .Assessments
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        assessment.Name = Name;
        assessment.Start = Start;
        assessment.End = End;

        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await appShell.PopAsync();
    }

    public async Task Init(int assessmentId)
    {
        await using var db = await factory.CreateDbContextAsync();

        var assessment = await db
               .Assessments
               .FirstOrDefaultAsync(x => x.Id == assessmentId) ??
            new();

        Id = assessment.Id;
        Name = assessment.Name;
        Start = assessment.Start;
        End = assessment.End;
        ShouldNotify = assessment.ShouldNotify;
        
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}