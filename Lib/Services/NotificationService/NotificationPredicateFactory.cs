using System.Linq.Expressions;
using Lib.Interfaces;
using LinqKit;

namespace Lib.Services;

internal static class NotificationPredicateFactory
{
    internal static NotificationPredicateFactory<T> Create<T>(Expression<Func<DateTime, T>> select, DateTime date)
        where T : struct => new(select) { Date = date };
}

internal class NotificationPredicateFactory<T>
{
    private readonly Expression<Func<DateTime, T>> _select;

    internal NotificationPredicateFactory(Expression<Func<DateTime, T>> select)
    {
        _select = select;
    }

    public DateTime Date { get; init; }


    public Expression<Func<INotification, bool>> StartEqual() =>
        PredicateBuilder.New<INotification>()
            .Start(x => Equals(_select.Invoke(x.Start), _select.Invoke(Date)));

    public Expression<Func<INotification, bool>> EndEqual() => PredicateBuilder.New<INotification>()
        .Start(x => Equals(_select.Invoke(x.End), _select.Invoke(Date)));
}