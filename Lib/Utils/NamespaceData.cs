using System.Runtime.CompilerServices;

namespace Lib.Utils;

public record NamespaceData
{
    private NamespaceData()
    {
    }

    public string AssemblyName { get; private set; } = "";
    public string FullNamespace { get; private set; } = "";

    private static string ParseNameofExpression(string nameofExpressionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nameofExpressionString);
        try
        {
            return nameofExpressionString.Substring(7, nameofExpressionString.Length - 8);
        }
        catch (ArgumentOutOfRangeException e)
        {
            var exc = new ArgumentOutOfRangeException(nameof(nameofExpressionString), nameofExpressionString,
                $"The nameof expression '{nameofExpressionString}' is invalid. Original exception: {e.Message}");
            throw exc;
        }
    }


    public static NamespaceData FromNameofExpression(
        // ReSharper disable once EntityNameCapturedOnly.Global
        string value,
        [CallerArgumentExpression(nameof(value))]
        string nameofExpression = "")
    {
        var path = ParseNameofExpression(nameofExpression);
        var parts = path.Split('.');
        return new NamespaceData
        {
            AssemblyName = parts[0],
            FullNamespace = path
        };
    }
}