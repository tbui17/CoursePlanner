using CommunityToolkit.Mvvm.ComponentModel;

namespace ViewModels.Interfaces;

public interface IVmView<T> where T : ObservableObject
{
    public T Model { get; set; }
}