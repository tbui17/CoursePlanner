using Lib.Attributes;
using Lib.Services;
using ViewModels.Services;

namespace ViewModels.Domain;

[Inject]
public class InstructorFormViewModelFactory(
    ILocalDbCtxFactory factory,
    INavigationService navService,
    IAppService appService,
    InstructorService instructorService
)
{
    public IInstructorFormViewModel CreateAddingModel()
    {
        var model = CreateModel();
        SetAdding();

        return model;

        void SetAdding()
        {
            model.Title = "Add Instructor";
            model.InstructorPersistence = instructorService.Add;
        }
    }

    private InstructorFormViewModel CreateModel()
    {
        return new InstructorFormViewModel(factory, navService, appService);
    }

    public IInstructorFormViewModel CreateEditingModel(int instructorId)
    {
        var model = CreateModel();
        SetEditing();
        return model;

        void SetEditing()
        {
            model.Title = "Edit Instructor";
            model.Id = instructorId;
            model.InstructorPersistence = instructorService.Update;
        }
    }

    public async Task<IInstructorFormViewModel> CreateInitializedEditingModel(int instructorId)
    {
        var model = CreateEditingModel(instructorId);
        await model.Init(instructorId);
        return model;
    }
}