namespace ViewModels.Interfaces;

public interface IRefreshableView<out T> where T : IRefresh
{
    public T Model { get; }

}