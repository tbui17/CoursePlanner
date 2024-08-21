using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModels.Interfaces;

public interface IPageModel<T> where T : ObservableObject
{

    public T Model { get; set; }


}