using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Models;

namespace CoursePlanner.ViewModels;

public partial class AppShellViewModel(IServiceProvider provider) : ObservableObject
{
    private T Resolve<T>() where T : notnull => provider.GetRequiredService<T>();

    public async Task GoToMainPageAsync()
    {
        var vm = Resolve<MainViewModel>();
        await vm.Init();
        await AppShell.Current.GoToAsync("///MainPage");
    }
    
    public async Task GoBackAsync()
    {
        await AppShell.GoBackAsync();
    }

    public async Task GoToDetailedTermPageAsync(int termId)
    {
        var vm = Resolve<DetailedTermViewModel>();
        await vm.Init(termId);
        await AppShell.GoToAsync<DetailedTermPage>();
    }

    public async Task GoBackToDetailedTermPageAsync()
    {
        var vm = Resolve<DetailedTermViewModel>();
        await vm.RefreshAsync();
        await AppShell.GoToAsync<DetailedTermPage>();
    }

    public async Task<string?> DisplayNamePromptAsync()
    {
        return await AppShell.DisplayNamePromptAsync();
    }

    public async Task GoToDetailedCoursesPageAsync(int courseId) { }

    public async Task GoToEditTermPageAsync()
    {
        var detailVm = Resolve<DetailedTermViewModel>();
        var term = detailVm.Term;
        
        
        await AppShell.GoToAsync<EditTermPage>(new Dictionary<string, object>
        {
            ["Id"] = term.Id,
            ["Name"] = term.Name,
            ["Start"] = term.Start,
            ["End"] = term.End
        });
    }

    public async Task GoBackToDetailedCoursePageAsync() { }

    public async Task GoToEditCoursePageAsync(int courseId) { }

    public async Task GoToAddInstructorPageAsync() { }

    public async Task GoToInstructorDetailsPageAsync(int instructorId) { }

    public async Task GoToAssessmentDetailsPageAsync(int assessmentId) { }

    public async Task GoToNoteDetailsPageAsync(int noteId) { }
}