using Microsoft.Playwright;

namespace ExpenseTracker.ApiTests;

/// <summary>
/// Base class for black-box API tests: each test gets its own Playwright request context
/// pointed at the running API.
/// </summary>
public abstract class ApiTestBase : PlaywrightTest
{
    protected IAPIRequestContext Api { get; private set; } = null!;

    [SetUp]
    public async Task CreateApiContext()
    {
        Api = await Playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            BaseURL = TestSettings.ApiBaseUrl,
        });
    }

    [TearDown]
    public async Task DisposeApiContext()
    {
        await Api.DisposeAsync();
    }
}
