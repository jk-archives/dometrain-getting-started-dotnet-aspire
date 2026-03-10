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

    public async Task<Dictionary<string, int>> GetAverageRatings(IEnumerable<string> names)
    {
        var db = connection.GetDatabase();
        var batch = db.CreateBatch();
        var tasks = names.ToDictionary(
            name => name,
            name => batch.ListRangeAsync(name)
        );
        batch.Execute();

        var results = new Dictionary<string, int>();
        foreach (var (name, task) in tasks)
        {
            var values = await task;
            results[name] = values.Length == 0
                ? 0
                : (int)Math.Round(values.Select(x => (int)x).Average(), 0);
        }
        return results;
    }
}
