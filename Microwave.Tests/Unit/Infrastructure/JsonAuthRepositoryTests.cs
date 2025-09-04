using Xunit;
using Microwave.Infrastructure.Repositories;
using Microwave.Domain.DTOs;

namespace Microwave.Tests.Unit.Infrastructure;

public class JsonAuthRepositoryTests : IDisposable
{
    private readonly string _testFilePath;
    private readonly JsonAuthRepository _repository;

    public JsonAuthRepositoryTests()
    {
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test-auth-{Guid.NewGuid()}.json");
        _repository = new JsonAuthRepository(_testFilePath);
    }

    [Fact]
    public async Task GetAuthSettingsAsync_WhenFileDoesNotExist_ReturnsNull()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"nonexistent-{Guid.NewGuid()}.json");
        var repository = new JsonAuthRepository(tempPath);

        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }

        var result = await repository.GetAuthSettingsAsync();

        Assert.Null(result);

        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task SaveAuthSettingsAsync_WithValidSettings_SavesSuccessfully()
    {
        var settings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword123",
            ConnectionString = "encrypted-connection-string",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveAuthSettingsAsync(settings);

        Assert.True(File.Exists(_testFilePath));
    }

    [Fact]
    public async Task GetAuthSettingsAsync_AfterSaving_ReturnsCorrectSettings()
    {
        var originalSettings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword123",
            ConnectionString = "encrypted-connection-string",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.SaveAuthSettingsAsync(originalSettings);
        var retrievedSettings = await _repository.GetAuthSettingsAsync();

        Assert.NotNull(retrievedSettings);
        Assert.Equal(originalSettings.Username, retrievedSettings.Username);
        Assert.Equal(originalSettings.PasswordHash, retrievedSettings.PasswordHash);
        Assert.Equal(originalSettings.ConnectionString, retrievedSettings.ConnectionString);
        Assert.Equal(originalSettings.CreatedAt.ToString("O"), retrievedSettings.CreatedAt.ToString("O"));
    }

    [Fact]
    public async Task ExistsAsync_WhenNoSettings_ReturnsFalse()
    {
        var exists = await _repository.ExistsAsync();

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsAsync_WhenSettingsExist_ReturnsTrue()
    {
        var settings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword123"
        };

        await _repository.SaveAuthSettingsAsync(settings);
        var exists = await _repository.ExistsAsync();

        Assert.True(exists);
    }

    [Fact]
    public async Task SaveAuthSettingsAsync_UpdatesExistingSettings()
    {
        var originalSettings = new AuthSettings
        {
            Username = "originaluser",
            PasswordHash = "originalhash"
        };

        var updatedSettings = new AuthSettings
        {
            Username = "updateduser",
            PasswordHash = "updatedhash",
            LastLoginAt = DateTime.UtcNow
        };

        await _repository.SaveAuthSettingsAsync(originalSettings);
        await _repository.SaveAuthSettingsAsync(updatedSettings);
        var retrievedSettings = await _repository.GetAuthSettingsAsync();

        Assert.NotNull(retrievedSettings);
        Assert.Equal(updatedSettings.Username, retrievedSettings.Username);
        Assert.Equal(updatedSettings.PasswordHash, retrievedSettings.PasswordHash);
        Assert.NotNull(retrievedSettings.LastLoginAt);
    }

    [Fact]
    public async Task GetAuthSettingsAsync_WithCorruptedFile_ReturnsNull()
    {
        await File.WriteAllTextAsync(_testFilePath, "invalid json content");

        var result = await _repository.GetAuthSettingsAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task SaveAuthSettingsAsync_CreatesDirectoryIfNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"test-dir-{Guid.NewGuid()}");
        var filePath = Path.Combine(tempDir, "auth-settings.json");
        var repository = new JsonAuthRepository(filePath);

        var settings = new AuthSettings
        {
            Username = "testuser",
            PasswordHash = "hashedpassword123"
        };

        try
        {
            await repository.SaveAuthSettingsAsync(settings);

            Assert.True(Directory.Exists(tempDir));
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }
}
