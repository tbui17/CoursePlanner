namespace ViewModels.Interfaces;

public interface IRefresh
{
    Task RefreshAsync();
}

public interface IRefreshId : IRefresh
{
    Task Init(int id);
}