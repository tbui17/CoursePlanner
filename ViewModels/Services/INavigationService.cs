namespace ViewModels.Services;

public enum NavigationTarget
{
    MainPage,
    TermDetailsPage,
    CourseDetailsPage,
    InstructorDetailsPage,
    InstructorFormPage,
    CourseFormPage,
    TermFormPage,
    AssessmentDetailsPage,
    NoteDetailsPage
}

public interface INavigationService
{
    Task GoToPageAsync(NavigationTarget target);
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