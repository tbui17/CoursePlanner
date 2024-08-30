namespace ViewModels.Interfaces;

public interface IRefresh0
{
    Task RefreshAsync();
}

public interface IRefresh : IRefresh0
{
    Task Init(int id);
}