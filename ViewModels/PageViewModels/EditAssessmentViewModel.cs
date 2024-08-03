using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using ViewModels.Services;

namespace ViewModels.PageViewModels;

public partial class EditAssessmentViewModel(ILocalDbCtxFactory factory, INavigationService navService)
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

    public ObservableCollection<string> AssessmentTypes { get; } = Assessment.Types.ToObservableCollection();

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.Types.First();

    [RelayCommand]
    public async Task SaveAsync()
    {
        await using var db = await factory.CreateDbContextAsync();
        var assessment = await db
           .Assessments
           .AsTracking()
           .FirstAsync(x => x.Id == Id);

        assessment.Name = Name;
        assessment.Start = Start;
        assessment.End = End;
        assessment.ShouldNotify = ShouldNotify;
        assessment.Type = SelectedAssessmentType;

        await db.SaveChangesAsync();
        await BackAsync();
    }

    [RelayCommand]
    public async Task BackAsync()
    {
        await navService.PopAsync();
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
        SelectedAssessmentType = assessment.Type;
    }

    public async Task RefreshAsync()
    {
        await Init(Id);
    }
}