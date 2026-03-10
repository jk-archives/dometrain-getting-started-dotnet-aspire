namespace Api;

public class RatingServiceHttpClient(HttpClient httpClient) : HttpClient
{
    public Task<int> GetRating(string podcastName) =>
        httpClient.GetFromJsonAsync<int>($"/ratings?podcastName={podcastName}");

    public Task SubmitRating(string podcastName, int rating) =>
        httpClient.PostAsJsonAsync($"/ratings",
            new
            {
                PodcastName = podcastName,
                Rating = rating,
            });
}
