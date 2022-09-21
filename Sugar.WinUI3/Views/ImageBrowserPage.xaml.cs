using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Sugar.WinUI3.ViewModels;

namespace Sugar.WinUI3.Views;

public sealed partial class ImageBrowserPage : Page
{
    public ImageBrowserViewModel ViewModel
    {
        get;
    }

    public ImageBrowserPage()
    {
        ViewModel = App.GetService<ImageBrowserViewModel>();
        InitializeComponent();

        // TODO: Implement with MVVM?
        App.MainWindow.Content.KeyUp += ImageBrowserKeyUp;
    }

    private void ImageBrowserKeyUp(object sender, KeyRoutedEventArgs e)
    {
        base.OnKeyUp(e);
        if (e.Key == Windows.System.VirtualKey.Control && PART_ImageView != null)
        {
            PART_ImageView.OnScaled();
        }
    }
}