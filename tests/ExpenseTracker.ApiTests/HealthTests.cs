namespace ExpenseTracker.ApiTests;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public sealed class HealthTests : ApiTestBase
{
    [Test]
    public async Task Health_endpoint_returns_healthy()
    {
        var response = await Api.GetAsync("/health");

        Assert.That(response.Status, Is.EqualTo(200));
        Assert.That(await response.TextAsync(), Is.EqualTo("Healthy"));
    }
}
