using Sugar.WinUI3.Contracts.Services;

namespace Sugar.WinUI3.Services;

public class SortOrientationSelectorService : ISortOrientationSelectorService
{
    private const string SettingsKey = "RequestedSortOrientation";

    private readonly ILocalSettingsService _localSettingsService;

    public bool IsPositive { get; set; } = true;

    public SortOrientationSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public async Task InitializeAsync()
    {
        IsPositive = await LoadOrientationFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetOrientationAsync(bool isPositive)
    {
        IsPositive = isPositive;
        await SaveOrientationInSettingsAsync(isPositive);
    }

    private async Task<bool> LoadOrientationFromSettingsAsync() => await _localSettingsService.ReadSettingAsync<bool>(SettingsKey);

    private async Task SaveOrientationInSettingsAsync(bool isPositive) => await _localSettingsService.SaveSettingAsync(SettingsKey, isPositive);
}