using CommunityToolkit.Mvvm.Messaging.Messages;
using Lib.Models;

namespace ViewModels.Events;

public class LoginEvent(User? value) : ValueChangedMessage<User?>(value);