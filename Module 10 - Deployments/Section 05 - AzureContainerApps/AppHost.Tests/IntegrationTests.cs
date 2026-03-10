using Microsoft.Extensions.Logging;

namespace AppHost.Tests;

public class IntegrationTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task GetPodcastsShouldReturnOkay()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.AppHost>();

        await using var app = await appHost.BuildAsync().WaitAsync(DefaultTimeout);
        await app.StartAsync().WaitAsync(DefaultTimeout);

        // Wait for API to be healthy
        await app.ResourceNotifications.WaitForResourceHealthyAsync("api").WaitAsync(DefaultTimeout);

        // Act
        var httpClient = app.CreateHttpClient("api");
        var response = await httpClient.GetAsync("/podcasts");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Unhandled Exception podcast", await response.Content.ReadAsStringAsync());
    }
}