using System.Diagnostics;
using Lib.Interfaces;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface IDateChangedArgs
{
    DateTime OldDate { get; }
    DateTime NewDate { get; }
}

public abstract record DateChangedArgsBase : IDateChangedArgs
{
    public required DateTime OldDate { get; init; }
    public required DateTime NewDate { get; init; }
}

public record DateStartChangedArgs : DateChangedArgsBase;

public record DateEndChangedArgs : DateChangedArgsBase;

public class DateState
{
    public required IDateTimeRange ModelRange { get; init; }
    public required IDateChangedArgs Args { get; init; }

    internal bool IsInvalid => Args switch
    {
        DateStartChangedArgs => NewGtEnd,
        DateEndChangedArgs => NewLtStart,
        _ => throw new UnreachableException()
    };

    internal bool IsInvalidAtEdge => IsInvalid && Args switch
    {
        DateStartChangedArgs => OldEqStart,
        DateEndChangedArgs => OldEqEnd,
        _ => throw new UnreachableException()
    };

    private bool NewGtEnd => Args.NewDate > ModelRange.End;
    private bool NewLtStart => Args.NewDate < ModelRange.Start;
    private bool OldEqStart => Args.OldDate == ModelRange.Start;
    private bool OldEqEnd => Args.OldDate == ModelRange.End;
}

public interface IDateHandler
{
    void Invoke();
};

public interface IStartHandler : IDateHandler;

public interface IEndHandler : IDateHandler;
public class StartInvalidHandler(NotificationDataViewModel model) :  IStartHandler
{
    public void Invoke()
    {
        model.Start = model.End;
    }
};

public class StartInvalidAtEdgeHandler(NotificationDataViewModel model) :  IStartHandler
{
    public void Invoke()
    {
        model._startDateOverride.OnNext(model.End);

    }
}

public class StartValidHandler(NotificationDataViewModel model, IDateChangedArgs args) :  IStartHandler
{
    public void Invoke()
    {
        model.Start = args.NewDate;
    }
}

public class EndInvalidHandler(NotificationDataViewModel model) :  IEndHandler
{
    public void Invoke()
    {
        model.End = model.Start;
    }
}

public class EndValidHandler(NotificationDataViewModel model, IDateChangedArgs args) :  IEndHandler
{
    public void Invoke()
    {
        model.End = args.NewDate;
    }
}

public class EndInvalidAtEdgeHandler(NotificationDataViewModel model) :  IEndHandler
{
    public void Invoke()
    {
        model._endDateOverride.OnNext(model.Start);
    }
}

public class HandlerFactory(NotificationDataViewModel model)
{
    public IDateHandler Create(DateState state) => state switch
    {
        { Args: DateStartChangedArgs, IsInvalidAtEdge: true } => new StartInvalidAtEdgeHandler(model),
        { Args: DateStartChangedArgs, IsInvalid: true } => new StartInvalidHandler(model),
        { Args: DateStartChangedArgs, IsInvalid: false } => new StartValidHandler(model,state.Args),
        { Args: DateEndChangedArgs, IsInvalidAtEdge: true } => new EndInvalidAtEdgeHandler(model),
        { Args: DateEndChangedArgs, IsInvalid: true } => new EndInvalidHandler(model),
        { Args: DateEndChangedArgs, IsInvalid: false } => new EndValidHandler(model,state.Args),
        _ => throw new UnreachableException()
    };
}
