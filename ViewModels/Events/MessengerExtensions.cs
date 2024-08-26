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
}