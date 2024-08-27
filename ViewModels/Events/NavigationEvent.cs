using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Lib.Services;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;

namespace ViewModels.Events;

public record NavigationEventArg(Type Page, int Id = 0);

public class NavigationEvent(NavigationEventArg arg) : ValueChangedMessage<NavigationEventArg>(arg);

public class NavigationSubject(RefreshableViewService util, ILogger<NavigationSubject> logger)
{
    public void Publish(NavigationEventArg arg)
    {
        logger.LogInformation("Publishing navigation event for {PageTitle}", arg.Page.Name);
        new NavigationEvent(arg).Publish();
    }

    public void Subscribe(IRefresh obj, Func<NavigationEvent, Task> handler)
    {
        var messenger = WeakReferenceMessenger.Default;
        if (messenger.IsRegistered<NavigationEvent>(obj))
        {
            logger.LogInformation("Object {Object} is already registered for navigation events", obj);
            return;
        }

        messenger.Register<NavigationEvent>(obj, Handler);
        logger.LogInformation("Subscribed {Object} to navigation events", obj);
        return;

        async void Handler(object _, NavigationEvent message)
        {
            var pageType = message.Value.Page;
            var containsTarget = util.GetRefreshableViewsContainingTarget(obj.GetType())
                .Any(x => x == pageType);

            if (!containsTarget)
            {
                return;
            }

            await handler(message);
        }
    }

    public void Subscribe(IRefresh obj)
    {
        Subscribe(obj, x => obj.Init(x.Value.Id));
    }
}