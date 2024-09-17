using Lib.Interfaces;

namespace ViewModels.Services;


public interface INavigationService
{
    Task GoToMainPageAsync();
    Task GoToDetailedTermPageAsync(int id);
    Task GoToDetailedCoursesPageAsync(int id);
    Task GoToEditTermPageAsync(int id);
    Task GoToEditCoursePageAsync(int id);
    Task GoToAddInstructorPageAsync();
    Task GotoEditInstructorPageAsync(int id);
    Task GoToAssessmentDetailsPageAsync(int id);
    Task GoToNoteDetailsPageAsync(int id);
    Task GoToNotificationDetailsPage(INotification notification);

    Task PopAsync();
}