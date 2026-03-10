using System.Net;

namespace Api.TestsWithoutAspire;

public class IntegrationTests
{
    [Fact]
    public async Task GetPodcastsShouldReturnOkay()
    {
        var waf = new CustomWebApplicationFactory();
        var httpClient = waf.CreateClient();
        var response = await httpClient.GetAsync("/podcasts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Unhandled Exception podcast", await response.Content.ReadAsStringAsync());
    }
}