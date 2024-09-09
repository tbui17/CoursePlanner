using System.Reactive.Concurrency;

namespace ViewModels.Scheduler;

public interface ISchedulerProvider
{
    IScheduler MainThread { get; }
    IScheduler CurrentThread { get; }
    IScheduler TaskPool { get; }
}