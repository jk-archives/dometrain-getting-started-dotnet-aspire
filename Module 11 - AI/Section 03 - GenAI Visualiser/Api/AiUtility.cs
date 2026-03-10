using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;

namespace Api;

public class AiUtility(IChatClient chatClient, IOptions<ConfigOptions> options)
{
    private static McpClient? _mcpClient;

    public async Task<string> GetPodcastDescription(string podcastName)
    {
        var mcpClient = await SetupMcpClient();
        var tools = await mcpClient.ListToolsAsync();

        var prompt =
            $"Write a short description for a podcast called {podcastName}." +
            $"Use the Tavily tool to get more information about the podcast." +
            $"Do not return any other text except for that description.";

        var response = await chatClient.GetResponseAsync(prompt,
            new ChatOptions { Tools = [.. tools] });

        return response.Text;
    }

    private async Task<McpClient> SetupMcpClient()
    {
        if (_mcpClient is not null)
            return _mcpClient;

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Tavily",
            Command = "npx",
            Arguments = ["-y", "tavily-mcp@latest"],
            EnvironmentVariables = new Dictionary<string, string?>
            {
                ["TAVILY_API_KEY"] = options.Value.TavilyApiKey,
            },
        });

        _mcpClient = await McpClient.CreateAsync(transport);

        return _mcpClient;
    }
}
