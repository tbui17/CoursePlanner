using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ViewModels.Events;

public static class MessengerExtensions
{
    public static void Subscribe<T>(this ObservableObject observableObject, Action<T> action) where T : class
    {
        WeakReferenceMessenger.Default.Register<T>(observableObject, (_, message) => action(message));
    }

    public static void Publish<T>(this T message) where T : class
    {
        WeakReferenceMessenger.Default.Send(message);
    }

    public static void SubscribeNavigation(this ObservableObject observableObject, Action<NavigationEvent> action)
    {
        WeakReferenceMessenger.Default.Register<NavigationEvent>(observableObject, (_, message) =>
        {
            var observableType = observableObject.GetType();
            var type = message.Value.GetType();
            var constructors = type.GetConstructors();
            var res = constructors
                .SelectMany(x => x.GetParameters())
                .Select(x => x.ParameterType)
                .FirstOrDefault(x =>
                {
                    if (x.IsInterface)
                    {
                        return x.IsAssignableFrom(observableType);
                    }

                    return x == observableType;
                });

            if (res is not null)
            {
                action(message);
            }
        });
    }
}