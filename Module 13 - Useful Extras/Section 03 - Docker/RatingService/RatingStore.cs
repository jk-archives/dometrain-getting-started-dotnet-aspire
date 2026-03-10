using StackExchange.Redis;

namespace RatingService;

public class RatingStore(IConnectionMultiplexer connection)
{
    public void AddRating(string name, int rating)
    {
        var db = connection.GetDatabase();
        db.ListRightPushAsync(name, rating);
    }

    public async Task<int> GetAverageRating(string name)
    {
        var db = connection.GetDatabase();
        var values = await db.ListRangeAsync(name);
        if (values.Length == 0) return 0;
        return (int)Math.Round(values.Select(x => (int)x).Average(), 0);
    }

    public void Clear()
    {
        var db = connection.GetDatabase();
        db.Execute("FLUSHDB");
    }
}
