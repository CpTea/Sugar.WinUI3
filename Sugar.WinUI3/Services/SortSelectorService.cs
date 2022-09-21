using Sugar.WinUI3.Contracts.Services;
using Windows.Storage.Search;

namespace Sugar.WinUI3.Services;

public class SortSelectorService : ISortSelectorService
{
    private const string SettingsKey = "RequestedFileQuery";

    private readonly ILocalSettingsService _localSettingsService;

    public SortSelectorService(ILocalSettingsService localSettingsService)
    {
        _localSettingsService = localSettingsService;
    }

    public CommonFileQuery Query { get; set; } = CommonFileQuery.DefaultQuery;

    public async Task InitializeAsync()
    {
        Query = await LoadFileQueryFromSettingsAsync();
        await Task.CompletedTask;
    }

    public async Task SetFileQueryAsync(CommonFileQuery query)
    {
        Query = query;
        await SaveFileQueryInSettingsAsync(query);
    }

    private async Task<CommonFileQuery> LoadFileQueryFromSettingsAsync()
    {
        var fileQuery = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

        if (Enum.TryParse(fileQuery, out CommonFileQuery cacheFileQuery))
        {
            return cacheFileQuery;
        }

        return CommonFileQuery.DefaultQuery;
    }

    private async Task SaveFileQueryInSettingsAsync(CommonFileQuery query) => await _localSettingsService.SaveSettingAsync(SettingsKey, query.ToString());
}