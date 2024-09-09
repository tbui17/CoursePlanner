using System.Reactive.Concurrency;
using Lib.Attributes;
using ReactiveUI;

namespace ViewModels.Scheduler;

[Inject]
public class DefaultSchedulerProvider : ISchedulerProvider
{
    public IScheduler MainThread => RxApp.MainThreadScheduler;
    public IScheduler CurrentThread => System.Reactive.Concurrency.Scheduler.CurrentThread;
    public IScheduler TaskPool => RxApp.TaskpoolScheduler;
}