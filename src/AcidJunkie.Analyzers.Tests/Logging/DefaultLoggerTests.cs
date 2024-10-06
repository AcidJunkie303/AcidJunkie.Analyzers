using AcidJunkie.Analyzers.Logging;
using FluentAssertions;

namespace AcidJunkie.Analyzers.Tests.Logging;

public sealed class DefaultLoggerTests
{
    [Fact]
    public void WriteLine_ShouldContainAllInformation()
    {
        // arrange
        var sut = new DefaultLogger<DefaultLoggerTests>();
        EnsureLogFileIsDeleted();

        // act
        sut.WriteLine(() => "test1");

        // assert
        var logFileContent = GetLogFileContent();
        logFileContent.Should().NotBeEmpty();
        logFileContent.Should().Contain("Context=DefaultLoggerTests");
        logFileContent.Should().Contain($"Method={nameof(WriteLine_ShouldContainAllInformation)}");
        logFileContent.Should().Contain("Message=test1");
        logFileContent.Should().Contain($"PID={Environment.ProcessId}");
        logFileContent.Should().Contain($"TID={Environment.CurrentManagedThreadId}");
    }
    private static string GetLogFileContent()
    {
#pragma warning disable MA0045
        return File.ReadAllText(DefaultLogger.LogFilePath);
#pragma warning restore MA0045
    }

    private static void EnsureLogFileIsDeleted()
    {
        if (File.Exists(DefaultLogger.LogFilePath))
        {
            File.Delete(DefaultLogger.LogFilePath);
        }
    }
}
