using System.Reactive;
using ReactiveUI;

namespace ViewModels.Utils;

public static class Util
{
    public static ReactiveCommand<T, Unit> CreateActionCommand<T>(Action<T> action)
    {
        return ReactiveCommand.Create<T>(action);
    }
}