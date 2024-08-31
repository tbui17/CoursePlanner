using System.Linq.Expressions;
using Lib.Interfaces;
using LinqKit;

namespace Lib.Services.NotificationService;

public class DateTimeRangePredicateFactory<T, TEntity> where TEntity : IDateTimeRange
{
    private readonly Expression<Func<DateTime, T>> _select;

    internal DateTimeRangePredicateFactory(Expression<Func<DateTime, T>> select)
    {
        _select = select;
    }

    public DateTime Date { get; init; }

    public Expression<Func<TEntity, bool>> Where(Expression<Func<T, T, bool>> comparer) =>
        Start(x => comparer.Invoke(_select.Invoke(x.Start), _select.Invoke(Date)));

    public Expression<Func<TEntity, bool>> StartEqual() =>
        Start(x => Equals(_select.Invoke(x.Start), _select.Invoke(Date)));

    public Expression<Func<TEntity, bool>> EndEqual() =>
        Start(x => Equals(_select.Invoke(x.End), _select.Invoke(Date)));

    private static Expression<Func<TEntity, bool>> Start(Expression<Func<TEntity, bool>> expr) =>
        PredicateBuilder.New<TEntity>().Start(expr);
}

public static class DateTimeRangePredicateFactory<TEntity> where TEntity : IDateTimeRange
{
    internal static DateTimeRangePredicateFactory<T, TEntity> Create<T>(Expression<Func<DateTime, T>> select,
        DateTime date) => new(select) { Date = date };
}