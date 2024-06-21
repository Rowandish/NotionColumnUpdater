using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using NotionColumnUpdater;
using NotionColumnUpdater.Abstractions;
using NSubstitute;

namespace NotionColumnUpdaterTests.Unit;

[ExcludeFromCodeCoverage]
public class MockHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(MockSend(request, cancellationToken));
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return MockSend(request, cancellationToken);
    }

    public virtual HttpResponseMessage MockSend(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

[ExcludeFromCodeCoverage]
public class NotionClientTests
{
    private NotionClient BuildSut(HttpResponseMessage responseMessage)
    {
        // See https://stackoverflow.com/a/71033310/2965109
        var mockHttpMessageHandler = Substitute.ForPartsOf<MockHttpMessageHandler>();
        mockHttpMessageHandler.MockSend(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(responseMessage);
        var httpClient = new HttpClient(mockHttpMessageHandler);
        httpClient.BaseAddress = new Uri("https://foo.bar/");
        return new NotionClient(httpClient);
    }

    [Fact]
    public async Task GetPageTitleAsync_WhenCalled_ParseJsonAndReturnString()
    {
        // Arrange
        var jsonContent =
            await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "NotionClient", "getPageTitle.json"));
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        var sut = BuildSut(mockResponse);

        // Act
        var title = await sut.GetPageTitleAsync("foo");

        // Assert
        title.Should().Be("Source-controlling Config files in Git (dotfiles?) - Dan Clarke");
    }

    [Fact]
    public async Task GetPageTitleAsync_WhenResponseError_ThrowException()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);
        var sut = BuildSut(mockResponse);

        // Act
        var act = async () => await sut.GetPageTitleAsync("foo");

        // Assert
        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    [Fact]
    public async Task QueryDatabaseAsync_WhenResponseError_ThrowException()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);
        var sut = BuildSut(mockResponse);

        // Act
        var act = async () => await sut.QueryDatabaseAsync("foo");

        // Assert
        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }

    [Fact]
    public async Task QueryDatabaseAsync_WhenCalled_ParseJsonAndReturnNotionResponse()
    {
        // Arrange
        var jsonContent =
            await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "NotionClient",
                "queryDatabaseAsync.json"));
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
        };
        var sut = BuildSut(mockResponse);

        // Act
        var notionResponse = await sut.QueryDatabaseAsync("foo");

        // Assert
        var deserialize = JsonSerializer.Deserialize<NotionResponse>(jsonContent);
        deserialize.Results.Should().BeEquivalentTo(notionResponse.Results);
    }

    [Fact]
    public async Task UpdatePageTitleAsync_WhenResponseError_ThrowException()
    {
        // Arrange
        var mockResponse = new HttpResponseMessage(HttpStatusCode.Forbidden);
        var sut = BuildSut(mockResponse);

        // Act
        var act = async () => await sut.UpdatePageTitleAsync("foo", "bar");

        // Assert
        await act.Should().ThrowExactlyAsync<HttpRequestException>();
    }
}