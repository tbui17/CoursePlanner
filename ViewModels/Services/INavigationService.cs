namespace ViewModels.Services;

public enum NavigationTarget
{
    MainPage,
    DetailedTermPage,
    EditTermPage,
    DetailedCoursePage,
    EditCoursePage,
    InstructorFormPage,
    EditNotePage,
    EditAssessmentPage,
    DevPage,
    LoginPage
}

public interface INavigationService
{
    Task GoToAsync(NavigationTarget target);
    Task GoToDetailedTermPageAsync(int id);
    Task GoToDetailedCoursesPageAsync(int id);
    Task GoToEditTermPageAsync(int id);
    Task GoToEditCoursePageAsync(int id);
    Task GoToAddInstructorPageAsync();
    Task GotoEditInstructorPageAsync(int id);
    Task GoToAssessmentDetailsPageAsync(int id);
    Task GoToNoteDetailsPageAsync(int id);

    Task PopAsync();
}