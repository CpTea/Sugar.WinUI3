using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Sugar.WinUI3.Components;
using Sugar.WinUI3.Contracts.Services;
using Sugar.WinUI3.Helpers;
using Sugar.WinUI3.Messengers;
using Sugar.WinUI3.Views;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;

namespace Sugar.WinUI3.ViewModels;

public partial class ImageBrowserViewModel : ObservableRecipient
{
    #region Data

    private DispatcherQueue? _dispatcherQueue;
    private readonly ISortOrientationSelectorService _sortOrientationSelectorService;
    private readonly ISortSelectorService _sortSelectorService;

    #endregion Data

    #region ctor

    public ImageBrowserViewModel(
        ISortSelectorService sortSelectorService,
        ISortOrientationSelectorService sortOrientationSelectorService)
    {
        _fileIndex = 0;
        _zoomFactorText = string.Empty;
        _imagePath = string.Empty;
        _imageFiles = null;
        _dispatcherQueue = null;
        _sortSelectorService = sortSelectorService;
        _sortOrientationSelectorService = sortOrientationSelectorService;

        _fileQuery = _sortSelectorService.Query;
        _sortOrientation = _sortOrientationSelectorService.IsPositive;
    }

    #endregion ctor

    #region Properties

    [ObservableProperty]
    private IReadOnlyList<StorageFile>? _imageFiles;

    [ObservableProperty]
    private int _fileIndex;

    [ObservableProperty]
    private double _progressRingValue;

    [ObservableProperty]
    private string _zoomFactorText;

    [ObservableProperty]
    private CommonFileQuery _fileQuery;

    [ObservableProperty]
    private bool _sortOrientation;

    private string _imagePath;

    public string ImagePath
    {
        get => _imagePath;
        set
        {
            SetProperty(ref _imagePath, value);
            Messenger.Send(new AppBarTitleTextChangedMessage(value));
        }
    }

    #endregion Properties

    #region Commands

    [RelayCommand]
    private async void OpenFile()
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            FileTypeFilter = { ".jpg", ".jpeg", ".png", ".webp" }
        };
        WinUIConversionHelper.InitFileOpenPicker(picker);
        var file = await picker.PickSingleFileAsync();
        await GetFilesFromSingleAsync(file, FileQuery, SortOrientation);
    }

    [RelayCommand]
    private async Task ProcessImage()
    {
        if (ImageFiles?.Count > 0)
        {
            var filePath = ImageFiles?[FileIndex]?.Path;
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var vm = new ImageBrowserSettingsDialogContentViewModel()
            {
                CurrentFilePath = filePath,
            };
            var dialog = new ContentDialog()
            {
                DefaultButton = ContentDialogButton.Primary,
                Content = new ImageBrowserSettingsDialogContent(vm),
                XamlRoot = App.MainWindow.Content.XamlRoot,
                Title = ResourceExtensions.GetLocalized("ImageBrowser_SettingsDialogContent_Title"),
                PrimaryButtonText = ResourceExtensions.GetLocalized("ImageBrowser_SettingsDialogContent_PrimaryButtonText"),
                CloseButtonText = ResourceExtensions.GetLocalized("ImageBrowser_SettingsDialogContent_CloseButtonText"),
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var outputName =
                    FilePathHelper.GetUniqueFilePath(
                        Path.Combine(
                        vm.SaveDirectoryName!,
                        $"{vm.SaveFileNameWithoutExtension}@{vm.SelectedUpscaleRatio}×.{vm.SelectedImageFormat}"));

                var esrganExe = Path.Combine(
                    Package.Current.InstalledLocation.Path,
                    "realesrgan-ncnn-vulkan",
                    "realesrgan-ncnn-vulkan.exe");

                using Process process = new();
                process.StartInfo.FileName = esrganExe;
                process.StartInfo.Arguments = ParseRealesrganArgs(
                    vm.CurrentFilePath,
                    outputName,
                    vm.SelectedUpscaleRatio,
                    vm.SelectedNetwork,
                    vm.SelectedImageFormat,
                    vm.EnableTTA,
                    vm.EnableVerbose);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.ErrorDataReceived += Process_DataReceived;
                process.OutputDataReceived += Process_DataReceived;

                try
                {
                    _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    await process.WaitForExitAsync();
                    process.Close();
                }
                finally
                {
                    ProgressRingValue = 0;
                    _dispatcherQueue = null;
                }
            }
        }
    }

    [RelayCommand]
    private void ActualSize()
    {
    }

    [RelayCommand]
    private async Task SelectFileQueryOption(CommonFileQuery query)
    {
        if (FileQuery != query)
        {
            FileQuery = query;
            if (ImageFiles?.Count > 0)
            {
                await GetFilesFromSingleAsync(ImageFiles[FileIndex], query, SortOrientation);
            }

            await _sortSelectorService.SetFileQueryAsync(query);
        }
    }

    [RelayCommand]
    private async Task SelectSortOrientation(bool orientation)
    {
        if (SortOrientation != orientation)
        {
            SortOrientation = orientation;
            if (ImageFiles?.Count > 0)
            {
                ReverseReadOnlyList(ref _imageFiles, ref _fileIndex);
                OnPropertyChanged(nameof(ImageFiles));
                OnPropertyChanged(nameof(FileIndex));
            }
            await _sortOrientationSelectorService.SetOrientationAsync(orientation);
        }
    }

    #endregion Commands

    #region Events

    private async void Process_DataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null && Regex.IsMatch(e.Data, @"\d*[.]?\d*%$"))
        {
            var valueStr = Regex.Replace(e.Data, @"[^\d(.\d)?$]", "");
            if (Regex.IsMatch(valueStr, @"^[+-]?\d*[.]?\d*$"))
            {
                await _dispatcherQueue!.EnqueueAsync(() =>
                {
                    ProgressRingValue = double.Parse(valueStr);
                });
            }
        }
    }

    #endregion Events

    #region Methods

    private static string ParseRealesrganArgs(
        string inputName,
        string outputName,
        int upscaleRatio,
        string networkName,
        string imageFormat,
        bool tta,
        bool verbose)
    {
        var enableTTA = tta ? "-x" : "";
        var enableVerboseOutput = verbose ? "-v" : "";
        return $"-i {inputName} -o {outputName} -n {networkName} -f {imageFormat} -s {upscaleRatio} {enableTTA} {enableVerboseOutput}";
    }

    private async Task GetFilesFromSingleAsync(StorageFile file, CommonFileQuery query, bool positive)
    {
        if (file != null)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(file.Path));
            QueryOptions queryOption = new(query, new string[] { ".jpg", ".jpeg", ".png", ".webp" })
            {
                FolderDepth = FolderDepth.Shallow
            };
            var queryResult = folder.CreateFileQueryWithOptions(queryOption);
            var files = await queryResult.GetFilesAsync();
            var index = Convert.ToInt32(await queryResult.FindStartIndexAsync(file));
            if (!positive)
            {
                ReverseReadOnlyList(ref files, ref index);
            }
            ImageFiles = files;
            FileIndex = index;
        }
    }

    private static void ReverseReadOnlyList<T>(ref IReadOnlyList<T>? src, ref int index)
    {
        if (src?.Count() > 0)
        {
            src = src.Reverse().ToList();
            index = src.Count() - 1 - index;
        }
    }

    #endregion Methods
}