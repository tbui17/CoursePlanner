namespace ViewModels.Interfaces;

public interface IRefresh
{
    public Task RefreshAsync();
}

public interface IRefreshId : IRefresh
{
    public Task Init(int id);
}