using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Notion.Client;
using NotionColumnUpdater;
using NotionColumnUpdater.Abstractions;
using NSubstitute.ExceptionExtensions;
using Xunit;

public class NotionServiceTests
{
    private readonly INotionClient _mockNotionClient;
    private readonly ILogger<NotionService> _mockLogger;
    private readonly INotionService _service;

    public NotionServiceTests()
    {
        _mockNotionClient = Substitute.For<INotionClient>();
        _mockLogger = Substitute.For<ILogger<NotionService>>();
        _service = new NotionService(_mockNotionClient, _mockLogger);
    }

    [Fact]
    public async Task UpdateRecordNameAsync_ShouldUpdateRecord_WhenValidResponse()
    {
        // Arrange
        var databaseId = "database-id";
        var recordId = "record-id";
        var relationColumn = "relation";
        var nameColumn = "name";
        var projectId = "project-id";

        var databaseQueryResult = new PaginatedList<Page>
        {
            Results = new List<Page>
            {
                new Page
                {
                    Id = recordId,
                    Properties = new Dictionary<string, PropertyValue>
                    {
                        {
                            relationColumn, new RelationPropertyValue
                            {
                                Relation = new List<Relation>
                                {
                                    new Relation { Id = projectId }
                                }
                            }
                        }
                    }
                }
            }
        };

        var projectPage = new Page
        {
            Properties = new Dictionary<string, PropertyValue>
            {
                {
                    "Name", new TitlePropertyValue
                    {
                        Title =
                        [
                            new RichTextText
                            {
                                Text = new Text { Content = "project-name" }
                            }
                        ]
                    }
                }
            }
        };

        _mockNotionClient.Databases.QueryAsync(databaseId, Arg.Any<DatabasesQueryParameters>()).Returns(databaseQueryResult);
        _mockNotionClient.Pages.RetrieveAsync(projectId).Returns(projectPage);

        // Act
        Func<Task> act = async () => await _service.UpdateRecordNameAsync(databaseId, recordId, relationColumn, nameColumn);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateRecordNameAsync_ShouldLogError_WhenExceptionIsThrown()
    {
        // Arrange
        var databaseId = "database-id";
        var recordId = "record-id";
        var relationColumn = "relation";
        var nameColumn = "name";

        _mockNotionClient.Databases.QueryAsync(databaseId, Arg.Any<DatabasesQueryParameters>()).Throws(new Exception("Request failed"));

        // Act
        Func<Task> act = async () => await _service.UpdateRecordNameAsync(databaseId, recordId, relationColumn, nameColumn);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Request failed");
        _mockLogger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error updating record name in Notion.")),
            Arg.Any<Exception>(),
            Arg.Any<Func<It.IsAnyType, Exception, string>>());
    }
}
