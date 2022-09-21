using Windows.Storage.Search;

namespace Sugar.WinUI3.Contracts.Services;

public interface ISortSelectorService
{
    CommonFileQuery Query
    {
        get;
    }

    Task InitializeAsync();

    Task SetFileQueryAsync(CommonFileQuery query);
}