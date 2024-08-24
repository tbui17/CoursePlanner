using CoursePlanner.Pages;
using ViewModels.Services;
#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

namespace CoursePlanner.Utils;

public class PageResolver(IServiceProvider provider)
{

    private T Resolve<T>() where T : notnull => provider.GetRequiredService<T>();


    public Page GetPage(NavigationTarget target)
    {
        return target switch
        {
            NavigationTarget.MainPage => Resolve<MainPage>(),
            NavigationTarget.DetailedTermPage => Resolve<DetailedTermPage>(),
            NavigationTarget.EditTermPage => Resolve<EditTermPage>(),
            NavigationTarget.DetailedCoursePage => Resolve<DetailedCoursePage>(),
            NavigationTarget.EditCoursePage => Resolve<EditCoursePage>(),
            NavigationTarget.InstructorFormPage => Resolve<InstructorFormPage>(),
            NavigationTarget.EditNotePage => Resolve<EditNotePage>(),
            NavigationTarget.EditAssessmentPage => Resolve<EditAssessmentPage>(),
            NavigationTarget.DevPage => Resolve<DevPage>(),
            NavigationTarget.LoginPage => Resolve<MainPage>()
        };
    }
}