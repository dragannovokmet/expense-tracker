using ExpenseTracker.UiTests.Pages;

namespace ExpenseTracker.UiTests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public sealed class HomePageTests : PageTest
{
    [Test]
    public async Task Home_page_shows_title_and_healthy_api_status()
    {
        var home = new HomePage(Page);

        await home.OpenAsync();

        await Expect(Page).ToHaveTitleAsync("Expense Tracker");
        await Expect(home.Title).ToHaveTextAsync("Expense Tracker");
        await Expect(home.ApiStatus).ToHaveTextAsync("Healthy");
    }
}
