using FluentAssertions;
using ManholeCardManager.Services;
using Microsoft.Extensions.Logging;

namespace ManholeCardManager.Tests.Services;

/// <summary>
/// SimpleFileLoggerの単体テスト
/// </summary>
public class SimpleFileLoggerTests : IDisposable
{
    private readonly string _testLogDirectory;

    /// <summary>
    /// テスト初期化
    /// </summary>
    public SimpleFileLoggerTests()
    {
        _testLogDirectory = Path.Combine(Path.GetTempPath(), $"test_logs_{Guid.NewGuid()}");
    }

    /// <summary>
    /// テスト後のクリーンアップ
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testLogDirectory))
        {
            Directory.Delete(_testLogDirectory, true);
        }
    }

    /// <summary>
    /// S-050: ログ出力_Information
    /// </summary>
    [Fact]
    public void S050_Log_Information_WritesToFile()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);
        var message = "テスト情報ログ";

        // Act
        logger.LogInformation(message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.log");
        logFiles.Should().ContainSingle();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("INFORMATION");
        logContent.Should().Contain("TestCategory");
        logContent.Should().Contain(message);
    }

    /// <summary>
    /// S-051: ログ出力_Error_例外付き
    /// </summary>
    [Fact]
    public void S051_Log_Error_WithException_WritesExceptionInfo()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);
        var message = "エラーが発生しました";
        var exception = new InvalidOperationException("テスト例外");

        // Act
        logger.LogError(exception, message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.log");
        logFiles.Should().ContainSingle();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain("ERROR");
        logContent.Should().Contain(message);
        logContent.Should().Contain("Exception:");
        logContent.Should().Contain("InvalidOperationException");
        logContent.Should().Contain("テスト例外");
    }

    /// <summary>
    /// S-052: IsEnabled_None
    /// </summary>
    [Fact]
    public void S052_IsEnabled_None_ReturnsFalse()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);

        // Act
        var result = logger.IsEnabled(LogLevel.None);

        // Assert
        result.Should().BeFalse();
    }

    /// <summary>
    /// S-053: IsEnabled_Information
    /// </summary>
    [Fact]
    public void S053_IsEnabled_Information_ReturnsTrue()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);

        // Act
        var result = logger.IsEnabled(LogLevel.Information);

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// S-054: ログディレクトリ自動作成
    /// </summary>
    [Fact]
    public void S054_Constructor_CreatesLogDirectory()
    {
        // Arrange & Act
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);

        // Assert
        Directory.Exists(_testLogDirectory).Should().BeTrue();
    }

    /// <summary>
    /// 複数ログレベルのテスト
    /// </summary>
    [Theory]
    [InlineData(LogLevel.Trace)]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    [InlineData(LogLevel.Critical)]
    public void Log_DifferentLevels_WritesToFile(LogLevel logLevel)
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);
        var message = $"テストメッセージ {logLevel}";

        // Act
        logger.Log(logLevel, message);

        // Assert
        var logFiles = Directory.GetFiles(_testLogDirectory, "*.log");
        logFiles.Should().ContainSingle();

        var logContent = File.ReadAllText(logFiles[0]);
        logContent.Should().Contain(logLevel.ToString().ToUpperInvariant());
        logContent.Should().Contain(message);
    }

    /// <summary>
    /// 日付ベースのファイル名テスト
    /// </summary>
    [Fact]
    public void Log_UsesDateBasedFileName()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);

        // Act
        logger.LogInformation("テスト");

        // Assert
        var expectedFileName = $"app-{DateTime.Now:yyyy-MM-dd}.log";
        var logFiles = Directory.GetFiles(_testLogDirectory);
        logFiles.Should().ContainSingle()
            .Which.Should().EndWith(expectedFileName);
    }

    /// <summary>
    /// BeginScopeテスト
    /// </summary>
    [Fact]
    public void BeginScope_ReturnsDisposable()
    {
        // Arrange
        var logger = new SimpleFileLogger("TestCategory", _testLogDirectory);

        // Act
        using var scope = logger.BeginScope("TestScope");

        // Assert
        scope.Should().NotBeNull();
    }
}
