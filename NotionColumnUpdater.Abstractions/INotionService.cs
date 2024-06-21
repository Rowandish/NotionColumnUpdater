namespace NotionColumnUpdater.Abstractions;

public interface INotionService
{
    Task UpdateDatabaseNamesAsync();
}