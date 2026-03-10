namespace RatingService;

public class SubmitRatingRequestBody
{
    public string PodcastName { get; set; } = null!;
    public int Rating { get; set; }
}