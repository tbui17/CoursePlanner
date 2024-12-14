using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Traits;

namespace ViewModels.Domain;

[Inject]
public partial class AssessmentItemViewModel
    : ObservableObject, IAssessmentForm
{
    [ObservableProperty]
    private DateTime _end;

    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = "";

    [ObservableProperty]
    private string _selectedAssessmentType = Assessment.DefaultType;

    [ObservableProperty]
    private bool _shouldNotify;

    [ObservableProperty]
    private DateTime _start;

    public AssessmentItemViewModel()
    {
    }

    public AssessmentItemViewModel(Assessment assessment)
    {
        this.Assign(assessment);
    }

    public ObservableCollection<string> AssessmentTypes { get; } = Assessment.Types.ToObservableCollection();

    public string Type
    {
        get => SelectedAssessmentType;
        set => SelectedAssessmentType = value;
    }

    public int CourseId { get; set; }
}