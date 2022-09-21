using Microsoft.UI.Xaml;

using Sugar.WinUI3.Contracts.Services;
using Sugar.WinUI3.ViewModels;

namespace Sugar.WinUI3.Activation;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args) =>
        // None of the ActivationHandlers has handled the activation.
        _navigationService.Frame?.Content == null;

    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(ImageBrowserViewModel).FullName!, args.Arguments);

        await Task.CompletedTask;
    }
}