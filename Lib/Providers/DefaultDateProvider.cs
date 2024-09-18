using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;

namespace Lib.Providers;

[Inject(typeof(IDefaultDateProvider))]
public sealed class DefaultDateProvider : IDefaultDateProvider
{
    public IDateTimeRange DateRange
    {
        get
        {
            var now = DateTime.Now.Date;
            return new DateTimeRange
            {
                Start = now,
                End = now.AddMonths(1)
            };
        }
    }
}