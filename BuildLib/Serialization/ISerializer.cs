namespace BuildLib.Serialization;

public interface ISerializer
{
    string Serialize(object obj);
}