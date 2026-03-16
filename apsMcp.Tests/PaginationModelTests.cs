using Xunit;
using FluentAssertions;
using apsMcp.Tools.Models;
using Newtonsoft.Json;

namespace apsMcp.Tests;

/// <summary>
/// Tests for pagination model classes and response parsing functionality.
/// Validates PaginationInfo and PaginatedResponse behavior.
/// </summary>
public class PaginationModelTests
{
    [Fact]
    public void PaginationInfo_HasNextPage_ReturnsTrueWhenCursorExists()
    {
        // Arrange
        var paginationInfo = new PaginationInfo
        {
            Cursor = "abc123",
            PageSize = 25
        };

        // Act & Assert
        paginationInfo.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PaginationInfo_HasNextPage_ReturnsFalseWhenCursorIsNull()
    {
        // Arrange
        var paginationInfo = new PaginationInfo
        {
            Cursor = null,
            PageSize = 25
        };

        // Act & Assert
        paginationInfo.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PaginationInfo_HasNextPage_ReturnsFalseWhenCursorIsEmpty()
    {
        // Arrange
        var paginationInfo = new PaginationInfo
        {
            Cursor = "",
            PageSize = 25
        };

        // Act & Assert
        paginationInfo.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PaginationInfo_HasNextPage_ReturnsFalseWhenCursorIsWhitespace()
    {
        // Arrange
        var paginationInfo = new PaginationInfo
        {
            Cursor = "   ",
            PageSize = 25
        };

        // Act & Assert
        paginationInfo.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void PaginatedResponse_InitializesWithDefaults()
    {
        // Act
        var response = new PaginatedResponse<CachedHub>();

        // Assert
        response.Pagination.Should().NotBeNull();
        response.Results.Should().NotBeNull();
        response.Results.Should().BeEmpty();
    }

    [Fact]
    public void PaginatedResponse_CanStoreTypedResults()
    {
        // Arrange
        var hubs = new List<CachedHub>
        {
            new() { Id = "hub1", Name = "Test Hub 1" },
            new() { Id = "hub2", Name = "Test Hub 2" }
        };

        var pagination = new PaginationInfo
        {
            Cursor = "next_page_cursor",
            PageSize = 25
        };

        // Act
        var response = new PaginatedResponse<CachedHub>
        {
            Pagination = pagination,
            Results = hubs
        };

        // Assert
        response.Pagination.Should().Be(pagination);
        response.Results.Should().HaveCount(2);
        response.Results[0].Id.Should().Be("hub1");
        response.Results[1].Id.Should().Be("hub2");
    }

    [Fact]
    public void PaginationInfo_JsonSerialization_PreservesAllProperties()
    {
        // Arrange
        var original = new PaginationInfo
        {
            Cursor = "test_cursor_123",
            Limit = 50,
            PageSize = 25
        };

        // Act
        var json = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<PaginationInfo>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Cursor.Should().Be("test_cursor_123");
        deserialized.Limit.Should().Be(50);
        deserialized.PageSize.Should().Be(25);
        deserialized.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public void PaginatedResponse_JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var original = new PaginatedResponse<CachedHub>
        {
            Pagination = new PaginationInfo
            {
                Cursor = "cursor_abc",
                PageSize = 10
            },
            Results = new List<CachedHub>
            {
                new() { Id = "hub1", Name = "Hub One" }
            }
        };

        // Act
        var json = JsonConvert.SerializeObject(original);
        var deserialized = JsonConvert.DeserializeObject<PaginatedResponse<CachedHub>>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Pagination.Cursor.Should().Be("cursor_abc");
        deserialized.Pagination.PageSize.Should().Be(10);
        deserialized.Results.Should().HaveCount(1);
        deserialized.Results[0].Id.Should().Be("hub1");
        deserialized.Results[0].Name.Should().Be("Hub One");
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("cursor", true)]
    [InlineData("abc123xyz", true)]
    public void PaginationInfo_HasNextPage_HandlesVariousCursorValues(string? cursor, bool expectedHasNext)
    {
        // Arrange
        var paginationInfo = new PaginationInfo
        {
            Cursor = cursor,
            PageSize = 25
        };

        // Act & Assert
        paginationInfo.HasNextPage.Should().Be(expectedHasNext);
    }

    [Fact]
    public void PaginatedResponse_SupportsGenericTypes()
    {
        // Test with different model types to ensure generics work
        var hubResponse = new PaginatedResponse<CachedHub>();
        var projectResponse = new PaginatedResponse<CachedProject>();
        var elementGroupResponse = new PaginatedResponse<CachedElementGroup>();

        // All should initialize properly
        hubResponse.Results.Should().NotBeNull();
        projectResponse.Results.Should().NotBeNull();
        elementGroupResponse.Results.Should().NotBeNull();

        // Type safety check
        hubResponse.Results.Should().BeOfType<List<CachedHub>>();
        projectResponse.Results.Should().BeOfType<List<CachedProject>>();
        elementGroupResponse.Results.Should().BeOfType<List<CachedElementGroup>>();
    }

    [Fact]
    public void PaginationInfo_DefaultValues_AreCorrect()
    {
        // Act
        var paginationInfo = new PaginationInfo();

        // Assert
        paginationInfo.Cursor.Should().BeNull();
        paginationInfo.Limit.Should().BeNull();
        paginationInfo.PageSize.Should().Be(0);
        paginationInfo.HasNextPage.Should().BeFalse();
    }
}