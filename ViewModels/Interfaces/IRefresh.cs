namespace ViewModels.Interfaces;

public interface IRefresh
{
    Task RefreshAsync();
    Task Init(int id);
}