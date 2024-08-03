namespace ViewModels.Services;

public interface INavigationService
{
    Task GoToMainPageAsync();
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