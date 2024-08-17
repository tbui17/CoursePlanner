using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;

namespace ViewModels.PageViewModels;

public partial class AssessmentItemViewModel
    : ObservableObject, IAssessmentForm
{
    public AssessmentItemViewModel()
    {

    }

    public AssessmentItemViewModel(Assessment assessment)
    {
        this.SetFromAssessmentForm(assessment);
    }

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

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.DefaultType;

    public ObservableCollection<string> AssessmentTypes { get; } = Assessment.Types.ToObservableCollection();

    public string Type
    {
        get => SelectedAssessmentType;
        set => SelectedAssessmentType = value;
    }

    public int CourseId { get; set; }
}