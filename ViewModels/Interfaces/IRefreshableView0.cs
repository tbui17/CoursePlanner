namespace ViewModels.Interfaces;

public interface IRefreshableView0<T> where T : IRefresh0
{
    public T Model { get; set; }
}