using Microsoft.Playwright;

namespace ExpenseTracker.UiTests.Pages;

/// <summary>Page object for the home page.</summary>
public sealed class HomePage(IPage page)
{
    public ILocator Title => page.GetByTestId("home-title");

    public ILocator ApiStatus => page.GetByTestId("api-status");

    public async Task OpenAsync() => await page.GotoAsync(TestSettings.WebBaseUrl);
}
