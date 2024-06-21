using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotionColumnUpdater.Abstractions;

namespace NotionColumnUpdater;

internal static class Program
{
    private static async Task Main()
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var notionService = serviceProvider.GetService<INotionService>();
        if (notionService != null)
            await notionService.UpdateDatabaseNamesAsync();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var configuration = LoadConfiguration();
        // Usando il metodo Configure utilizzo il pattern con IOptions che è più completo
        services.Configure<AppConfig>(configuration);
        services.AddLogging(configure => configure.AddConsole());
        services.AddSingleton<INotionService, NotionService>();
        // Aggiungo una classe che riceverà già in ingresso un HTTPClient configurato così a costruttore
        services.AddHttpClient<INotionClient, NotionClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.notion.com/v1/");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", Environment.GetEnvironmentVariable("NOTION_API_KEY"));
            client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
        });
    }

    private static IConfiguration LoadConfiguration()
    {
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);

        return configurationBuilder.Build();
    }
}