using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Sugar.WinUI3.Messengers;

internal class AppBarTitleTextChangedMessage : ValueChangedMessage<string>
{
    public AppBarTitleTextChangedMessage(string value) : base(value)
    {
    }
}