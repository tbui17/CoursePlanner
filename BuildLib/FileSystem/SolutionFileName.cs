namespace BuildLib.FileSystem;

public class SolutionFileName
{
    public string Value { get; set; } = "";

    public string GetValue()
    {
        if (!Value.EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
        {
            return Value + ".sln";
        }

        return Value;
    }

    public static implicit operator SolutionFileName(string value) => new() { Value = value };
}