namespace Frontend;

public class BackendHttpClient(HttpClient httpClient) : HttpClient
{
    public Task<List<Podcast>?> GetPodcasts() =>
        httpClient.GetFromJsonAsync<List<Podcast>>("/podcasts");

    public Task SubmitRating(string podcastName, int rating) =>
        httpClient.PostAsJsonAsync("/rating", new
        {
            PodcastName = podcastName,
            Rating = rating
        });
}

public class Podcast
{
    public string PodcastName { get; set; } = null!;
    public int Rating { get; set; }
}
