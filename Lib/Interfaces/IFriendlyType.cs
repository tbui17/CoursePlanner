namespace Lib.Interfaces;

public interface IFriendlyType
{
    public string GetFriendlyType()
    {
        return GetType().Name;
    }
}