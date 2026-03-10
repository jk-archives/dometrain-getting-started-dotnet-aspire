using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Api.TestsWithoutAspire;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configBuilder =>
        {
            configBuilder.AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:podcasts", "Server=127.0.0.1,1433;User ID=sa;Password=Dometrain#123;TrustServerCertificate=true;Initial Catalog=podcasts"),
                new KeyValuePair<string, string?>("services:ratingservice:http:0", "http://localhost:5143"),
                new KeyValuePair<string, string?>("services:ratingservice:https:0", "https://localhost:7093")
            ]);
        });

        return base.CreateHost(builder);
    }
}