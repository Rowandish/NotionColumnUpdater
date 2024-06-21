using System.Text;
using System.Text.Json;
using NotionColumnUpdater.Abstractions;

namespace NotionColumnUpdater;

public class NotionClient : INotionClient
{
    private readonly HttpClient _httpClient;

    public NotionClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetPageTitleAsync(string pageId)
    {
        var response = await _httpClient.GetAsync($"pages/{pageId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        return doc
            .RootElement
            .GetProperty("properties")
            .GetProperty("Name")
            .GetProperty("title")[0]
            .GetProperty("text")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    public async Task UpdatePageTitleAsync(string pageId, string newTitle)
    {
        var payload = new
        {
            properties = new
            {
                Name = new
                {
                    title = new[]
                    {
                        new
                        {
                            text = new
                            {
                                content = newTitle
                            }
                        }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync($"pages/{pageId}", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<NotionResponse> QueryDatabaseAsync(string databaseId)
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"databases/{databaseId}/query", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<NotionResponse>(responseBody)!;
    }
}