using System.Runtime.CompilerServices;

namespace Lib.Utils;

public static class Util
{
    public static string Full(string _, [CallerArgumentExpression(nameof(_))] string fullPath = default!)
    {
        return fullPath.Substring(fullPath.IndexOf('(') + 1, fullPath.IndexOf(')') - fullPath.IndexOf('(') - 1);
    }
}