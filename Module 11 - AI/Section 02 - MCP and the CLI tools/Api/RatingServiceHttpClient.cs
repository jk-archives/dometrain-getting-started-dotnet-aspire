namespace Api;

public class RatingServiceHttpClient(HttpClient httpClient) : HttpClient
{
    public Task<int> GetRating(string podcastName) =>
        httpClient.GetFromJsonAsync<int>($"/ratings?podcastName={podcastName}");

    public async Task<Dictionary<string, int>> GetRatings(IEnumerable<string> podcastNames) =>
        await httpClient.PostAsJsonAsync("/ratings/batch", podcastNames)
            .ContinueWith(t => t.Result.Content.ReadFromJsonAsync<Dictionary<string, int>>())
            .Unwrap();

    public Task SubmitRating(string podcastName, int rating) =>
        httpClient.PostAsJsonAsync($"/ratings",
            new
            {
                PodcastName = podcastName,
                Rating = rating,
            });
}
