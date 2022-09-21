using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sugar.WinUI3.ViewModels;
public partial class ImageBrowserSettingsDialogContentViewModel : ObservableRecipient
{
    public ImageBrowserSettingsDialogContentViewModel()
    {
        _upscaleRatios = new ObservableCollection<int>() { 2, 3, 4, };
        _selectedUpscaleRatio = _upscaleRatios.LastOrDefault();
        
        _imageFormats = new ObservableCollection<string> { "png", "jpg", "webp", };
        _selectedImageFormat = _imageFormats.First();

        _networks = new ObservableCollection<string> 
        {
            "realesr-animevideov3",
            "realesrgan-x4plus",
            "realesrgan-x4plus-anime",
        };
        _selectedNetwork = _networks.Last();
        _enableTTA = false;
        _enableVerbose = false;
    }

    [ObservableProperty]
    private ObservableCollection<int> _upscaleRatios;

    [ObservableProperty]
    private int _selectedUpscaleRatio;

    [ObservableProperty]
    private ObservableCollection<string> _imageFormats;

    [ObservableProperty]
    private string _selectedImageFormat;

    [ObservableProperty]
    private ObservableCollection<string> _networks;

    [ObservableProperty]
    private string _selectedNetwork;

    [ObservableProperty]
    private bool _enableTTA;

    [ObservableProperty]
    private bool _enableVerbose;

    [ObservableProperty]
    private string? _currentFilePath;

    private string? _saveFileNameWithoutExtension;
    public string? SaveFileNameWithoutExtension
    {
        get => _saveFileNameWithoutExtension ?? GetDefaultFileNameWithoutExtension(CurrentFilePath);
        set => SetProperty(ref _saveFileNameWithoutExtension, value);
    }

    private string? _saveDirectoryName;
    public string? SaveDirectoryName
    {
        get => _saveDirectoryName ?? GetDefaultSaveDirectoryName(CurrentFilePath);
        set => SetProperty(ref _saveDirectoryName, value);
    }

    private static string? GetDefaultFileNameWithoutExtension(string? filePath) =>
        !string.IsNullOrEmpty(filePath) ? Path.GetFileNameWithoutExtension(filePath) : null;

    private static string? GetDefaultSaveDirectoryName(string? filePath) =>
        !string.IsNullOrEmpty(filePath) ? Path.Combine(Path.GetDirectoryName(filePath)!, "realesr") : null;
}
