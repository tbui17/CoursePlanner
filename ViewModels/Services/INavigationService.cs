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
    LoginPage,
    TermListPage,
}

public interface INavigationService
{
    Task GoToAsync(NavigationTarget target);
    Task GoToDetailedTermPageAsync(int termId);
    Task GoToDetailedCoursesPageAsync(int id);
    Task GoToEditTermPageAsync(int id);
    Task GoToEditCoursePageAsync(int courseId);
    Task GoToAddInstructorPageAsync();
    Task GotoEditInstructorPageAsync(int instructorId);
    Task GoToAssessmentDetailsPageAsync(int assessmentId);
    Task GoToNoteDetailsPageAsync(int noteId);
    Task GoBackToDetailedTermPageAsync();
    Task PopAsync();
}