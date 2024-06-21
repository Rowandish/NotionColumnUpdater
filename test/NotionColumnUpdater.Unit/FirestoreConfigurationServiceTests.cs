using FluentAssertions;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using NotionColumnUpdater.Abstractions;
using NSubstitute;
using Xunit;

namespace NotionColumnUpdater.Unit;

public class FirestoreConfigurationServiceTests
{
    private readonly FirestoreDb _mockFirestoreDb;
    private readonly ILogger<FirestoreConfigurationService> _mockLogger;
    private readonly FirestoreConfigurationService _service;

    public FirestoreConfigurationServiceTests()
    {
        _mockFirestoreDb = Substitute.For<FirestoreDb>("test-project-id");
        _mockLogger = Substitute.For<ILogger<FirestoreConfigurationService>>();
        _service = new FirestoreConfigurationService("test-project-id", _mockLogger);
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldReturnConfiguration_WhenDocumentExists()
    {
        // Arrange
        var docRef = Substitute.For<DocumentReference>();
        var docSnap = Substitute.For<DocumentSnapshot>();
        var expectedConfig = new Configuration
        {
            Databases = new System.Collections.Generic.List<DatabaseConfiguration>
            {
                new DatabaseConfiguration
                {
                    HoursDbId = "db1",
                    DbId = "db1",
                    RelationColumn = "relation",
                    NameColumn = "name"
                }
            }
        };

        docSnap.Exists.Returns(true);
        docSnap.ConvertTo<Configuration>().Returns(expectedConfig);

        _mockFirestoreDb.Collection(Arg.Any<string>()).Document(Arg.Any<string>()).Returns(docRef);
        docRef.GetSnapshotAsync().Returns(Task.FromResult(docSnap));

        // Act
        var config = await _service.GetConfigurationAsync();

        // Assert
        config.Should().BeEquivalentTo(expectedConfig);
    }

    [Fact]
    public async Task GetConfigurationAsync_ShouldThrowException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var docRef = Substitute.For<DocumentReference>();
        var docSnap = Substitute.For<DocumentSnapshot>();

        docSnap.Exists.Returns(false);
        _mockFirestoreDb.Collection(Arg.Any<string>()).Document(Arg.Any<string>()).Returns(docRef);
        docRef.GetSnapshotAsync().Returns(Task.FromResult(docSnap));

        // Act
        Func<Task> act = async () => await _service.GetConfigurationAsync();

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Configuration document not found.");
    }
}