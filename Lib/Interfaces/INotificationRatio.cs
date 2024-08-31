namespace Lib.Interfaces;

public interface INotificationRatio
{
    public int Total { get; }
    public int Active { get; }
    public int Inactive => Total - Active;
    public double Percentage => (double)Active / Total * 100;
}