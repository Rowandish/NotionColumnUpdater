using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NotionColumnUpdater;
using NotionColumnUpdater.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NotionColumnUpdaterTests.Unit;

[ExcludeFromCodeCoverage]
public class NotionServiceTests
{
    private INotionClient _client;

    private INotionService BuildSut(NotionResponse response)
    {
        const string tableHoursDbId = "52ea8203d91747be8fbe09b8524d19f3";
        _client = Substitute.For<INotionClient>();
        _client.QueryDatabaseAsync(tableHoursDbId).Returns(response);
        var configs = Substitute.For<IOptions<AppConfig>>();
        configs.Value.Returns(new AppConfig
        {
            Databases = [new DatabaseConfig { TableHoursDbId = tableHoursDbId }]
        });
        var logger = Substitute.For<ILogger<NotionService>>();
        return new NotionService(_client, configs, logger);
    }

    [Fact]
    public async Task UpdateDatabaseNamesAsync_WhenCalledWithNormalPage_ShouldCallUpdatePageTitleAsync()
    {
        // Arrange
        const string relationId = "bar";
        const string pageId = "foo";
        var sut = BuildSut(BuildNotionResponse(pageId, relationId));

        // Act
        await sut.UpdateDatabaseNamesAsync();

        // Assert
        await _client.Received(1).GetPageTitleAsync(relationId);
        await _client.Received(1).UpdatePageTitleAsync(pageId, Arg.Any<string>());
    }

    private static NotionResponse BuildNotionResponse(string pageId, string relationId)
    {
        return new NotionResponse
        {
            Results =
            [
                new Page
                {
                    Id = pageId,
                    Properties = new Dictionary<string, Property>
                    {
                        {
                            "____", new Property
                            {
                                Type = "relation",
                                Relation =
                                [
                                    new Relation
                                    {
                                        Id = relationId
                                    }
                                ]
                            }
                        }
                    }
                }
            ]
        };
    }

    [Fact]
    public async Task UpdateDatabaseNamesAsync_WhenAPageHaveNoPropertyWithRelation_ShouldNotConsiderIt()
    {
        // Arrange
        const string relationId = "bar";
        const string pageId = "foo";
        var notionResponse = BuildNotionResponse(pageId, relationId);
        notionResponse.Results[0].Properties["____"].Type = "notRelation";
        var sut = BuildSut(notionResponse);

        // Act
        await sut.UpdateDatabaseNamesAsync();

        // Assert
        await _client.DidNotReceiveWithAnyArgs().GetPageTitleAsync(default!);
        await _client.DidNotReceiveWithAnyArgs().UpdatePageTitleAsync(default!, default!);
    }

    [Fact]
    public async Task UpdateDatabaseNamesAsync_WhenAPageHaveRelationPropertyWithEmptyList_ShouldNotConsiderIt()
    {
        // Arrange
        const string relationId = "bar";
        const string pageId = "foo";
        var notionResponse = BuildNotionResponse(pageId, relationId);
        notionResponse.Results[0].Properties["____"].Relation.Clear();
        var sut = BuildSut(notionResponse);

        // Act
        await sut.UpdateDatabaseNamesAsync();

        // Assert
        await _client.DidNotReceiveWithAnyArgs().GetPageTitleAsync(default!);
        await _client.DidNotReceiveWithAnyArgs().UpdatePageTitleAsync(default!, default!);
    }

    [Fact]
    public async Task UpdateDatabaseNamesAsync_WhenQueryDatabaseAsyncThrowException_ShouldNotConsiderIt()
    {
        // Arrange
        _client = Substitute.For<INotionClient>();
        var exception = new Exception();
        var sut = BuildSut(new NotionResponse());
        _client.QueryDatabaseAsync(Arg.Any<string>()).ThrowsAsync(exception);

        // Act
        var act = async () => await sut.UpdateDatabaseNamesAsync();
        await act.Should().NotThrowAsync<Exception>();

        // Assert
        await _client.DidNotReceiveWithAnyArgs().GetPageTitleAsync(default!);
        await _client.DidNotReceiveWithAnyArgs().UpdatePageTitleAsync(default!, default!);
    }
}