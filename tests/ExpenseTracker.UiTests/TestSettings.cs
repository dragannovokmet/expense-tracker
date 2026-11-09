namespace ExpenseTracker.UiTests;

/// <summary>Where the running web app is. Override with the WEB_BASE_URL environment variable.</summary>
public static class TestSettings
{
    public static string WebBaseUrl { get; } =
        Environment.GetEnvironmentVariable("WEB_BASE_URL") ?? "http://localhost:5190";
}
