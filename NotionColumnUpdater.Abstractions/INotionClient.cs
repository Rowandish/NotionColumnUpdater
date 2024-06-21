namespace NotionColumnUpdater.Abstractions;

public interface INotionClient
{
    Task<string> GetPageTitleAsync(string pageId);
    Task UpdatePageTitleAsync(string pageId, string newTitle);
    Task<NotionResponse> QueryDatabaseAsync(string databaseId);
}