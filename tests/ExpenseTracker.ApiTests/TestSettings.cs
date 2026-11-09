namespace ExpenseTracker.ApiTests;

/// <summary>Where the running API is. Override with the API_BASE_URL environment variable.</summary>
public static class TestSettings
{
    public static string ApiBaseUrl { get; } =
        Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:5180";
}
