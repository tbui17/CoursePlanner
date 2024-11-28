using CommunityToolkit.Mvvm.Messaging.Messages;
using Lib.Interfaces;

namespace ViewModels.Events;

public class LoginEvent(IUserDetail? value) : ValueChangedMessage<IUserDetail?>(value);