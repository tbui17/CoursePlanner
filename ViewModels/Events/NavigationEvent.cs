using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;
using ViewModels.Utils;

namespace ViewModels.Events;

public record NavigationEventArg(Page Page, int Id = 0);

public class NavigationEvent(NavigationEventArg arg) : ValueChangedMessage<NavigationEventArg>(arg)
{
}

public class NavigationSubject(ReflectionUtil util, ILogger<NavigationSubject> logger)
{
    public void Publish(NavigationEventArg arg)
    {
        logger.LogInformation("Publishing navigation event for {PageTitle}", arg.Page.Title);
        new NavigationEvent(arg).Publish();
    }

    public void Subscribe(IRefresh obj, Action<NavigationEvent> handler)
    {
        if (WeakReferenceMessenger.Default.IsRegistered<NavigationEvent>(obj))
        {
            logger.LogWarning("Object {Object} is already registered for navigation events", obj);
            return;
        }

        WeakReferenceMessenger.Default.Register<NavigationEvent>(obj, (_, message) =>
        {
            var pageType = message.Value.Page.GetType();
            var isFound = util
                .GetRefreshableViewsContainingTarget(obj.GetType())
                .Any(x => x == pageType);

            if (!isFound)
            {
                return;
            }

            handler(message);
        });
        logger.LogInformation("Subscribed {Object} to navigation events", obj);
    }

    public void Subscribe(IRefresh obj)
    {
        Subscribe(obj, x => obj.Init(x.Value.Id));
    }
}