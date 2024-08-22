using System.Runtime.CompilerServices;
using Lib.Exceptions;

namespace Lib;

public static class Globals
{
    public static DateTime DefaultStart() => DateTime.Now;
    public static DateTime DefaultEnd() => DateTime.Now.AddDays(1);

    public static DomainException? AggregateValidation(params DomainException?[] exceptions)
    {
        var res = exceptions.OfType<DomainException>().Select(x => x.Message).ToList();
        if (res.Count == 0)
        {
            return null;
        }

        return new DomainException(string.Join(Environment.NewLine, res));
    }
}