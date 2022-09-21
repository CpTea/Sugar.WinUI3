using Microsoft.UI.Xaml.Controls;

using Sugar.WinUI3.ViewModels;

namespace Sugar.WinUI3.Views;

public sealed partial class BlankPage : Page
{
    public BlankViewModel ViewModel
    {
        get;
    }

    public BlankPage()
    {
        ViewModel = App.GetService<BlankViewModel>();
        InitializeComponent();
    }
}