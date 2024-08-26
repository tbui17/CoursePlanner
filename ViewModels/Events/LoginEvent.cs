using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Lib.Models;
using ViewModels.PageViewModels;

namespace ViewModels.Events;

public class LoginEvent(User? value) : ValueChangedMessage<User?>(value);

public class NavigationEvent(Page page) : ValueChangedMessage<Page>(page)
{

}