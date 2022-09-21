namespace Sugar.WinUI3.Contracts.Services;

public interface ISortOrientationSelectorService
{
    bool IsPositive
    {
        get;
    }

    Task InitializeAsync();

    Task SetOrientationAsync(bool isPositive);
}