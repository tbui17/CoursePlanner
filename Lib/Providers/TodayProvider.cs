using Lib.Attributes;
using Lib.Interfaces;

namespace Lib.Providers;

[Inject(typeof(ITodayProvider))]
public class TodayProvider : ITodayProvider
{
    public DateTime Today()
    {
        return DateTime.Today.Date;
    }
}