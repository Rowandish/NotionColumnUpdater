using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionColumnUpdater.Abstractions;

namespace NotionColumnUpdater;

public class NotionService : INotionService
{
    private readonly AppConfig _configs;
    private readonly ILogger<NotionService> _logger;
    private readonly INotionClient _notionClient;

    public NotionService(INotionClient notionClient, IOptions<AppConfig> configs,
        ILogger<NotionService> logger)
    {
        _logger = logger;
        _notionClient = notionClient;
        _configs = configs.Value;
    }

    public async Task UpdateDatabaseNamesAsync()
    {
        if (_configs.Databases == null)
        {
            _logger.LogError("Error getting table id from config");
            return;
        }
        foreach (var databaseId in _configs.Databases)
            try
            {
                _logger.LogInformation("Processing table {databaseId}", databaseId);

                var pages = await _notionClient.QueryDatabaseAsync(databaseId);
                var result = new List<PageRelation>();
                foreach (var page in pages.Results)
                {
                    var relationProperty =
                        page.Properties.FirstOrDefault(p => p.Value.Type == "relation" && p.Value.Relation.Count != 0);
                    // Se non ho alcuna relation impostata non aggiorno nulla
                    if (relationProperty.Equals(new KeyValuePair<string, Property>()))
                        continue;
                    var properties = relationProperty.Value.Relation[0].Id;
                    result.Add(new PageRelation(page.Id, properties));
                }

                foreach (var (pageId, relationId) in result)
                {
                    var relatedPageTitle = await _notionClient.GetPageTitleAsync(relationId);
                    _logger.LogInformation("Setting page title of page {pageId} -> {relatedPageTitle}...", pageId,
                        relatedPageTitle);
                    await _notionClient.UpdatePageTitleAsync(pageId, relatedPageTitle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing table {databaseId}: {Message}", databaseId, ex.Message);
            }
    }

    private record PageRelation(string PageId, string RelationId);
}