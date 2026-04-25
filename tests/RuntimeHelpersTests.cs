using System;
using System.IO;
using Xunit;

namespace app.Tests;

public sealed class RuntimeHelpersTests : IDisposable
{
    private readonly string _tempDir;

    public RuntimeHelpersTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    [Fact]
    public void CleanupOldLogFiles_NonExistentDirectory_DoesNotThrow()
    {
        var nonExistentDir = Path.Combine(_tempDir, "missing");
        RuntimeHelpers.CleanupOldLogFiles(nonExistentDir, 14, "current.log");
        // Pass if no exception.
    }

    [Fact]
    public void CleanupOldLogFiles_DeletesFilesOlderThanRetentionPeriod()
    {
        var oldDate = DateTime.Today.AddDays(-30);
        var oldFile = Path.Combine(_tempDir, $"{oldDate:yyyy-MM-dd}_Container.log");
        File.WriteAllText(oldFile, "old log");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, 14, "current.log");

        Assert.False(File.Exists(oldFile));
    }

    [Fact]
    public void CleanupOldLogFiles_KeepsFilesWithinRetentionPeriod()
    {
        var recentDate = DateTime.Today.AddDays(-1);
        var recentFile = Path.Combine(_tempDir, $"{recentDate:yyyy-MM-dd}_Container.log");
        File.WriteAllText(recentFile, "recent log");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, 14, "current.log");

        Assert.True(File.Exists(recentFile));
    }

    [Fact]
    public void CleanupOldLogFiles_NeverDeletesCurrentLogFile()
    {
        // Even if the file date is ancient, the current log must never be removed.
        var oldDate = DateTime.Today.AddDays(-365);
        var currentLogPath = Path.Combine(_tempDir, $"{oldDate:yyyy-MM-dd}_Container.log");
        File.WriteAllText(currentLogPath, "current log");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, 14, currentLogPath);

        Assert.True(File.Exists(currentLogPath));
    }

    [Fact]
    public void CleanupOldLogFiles_SkipsFilesWithUnrecognisedNames()
    {
        var unknownFile = Path.Combine(_tempDir, "not-a-date_Container.log");
        File.WriteAllText(unknownFile, "data");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, 14, "current.log");

        Assert.True(File.Exists(unknownFile));
    }

    [Fact]
    public void CleanupOldLogFiles_RetainsFileExactlyAtCutoffBoundary()
    {
        // cutoffDate = today - (retentionDays - 1); files on that date are kept.
        const int retention = 14;
        var cutoffDate = DateTime.Today.AddDays(-(retention - 1));
        var boundaryFile = Path.Combine(_tempDir, $"{cutoffDate:yyyy-MM-dd}_Container.log");
        File.WriteAllText(boundaryFile, "boundary log");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, retention, "current.log");

        Assert.True(File.Exists(boundaryFile));
    }

    [Fact]
    public void CleanupOldLogFiles_DeletesFileOneDayBeforeCutoff()
    {
        // One day before the cutoff date must be deleted.
        const int retention = 14;
        var oneDayBeforeCutoff = DateTime.Today.AddDays(-(retention));
        var staleFile = Path.Combine(_tempDir, $"{oneDayBeforeCutoff:yyyy-MM-dd}_Container.log");
        File.WriteAllText(staleFile, "stale log");

        RuntimeHelpers.CleanupOldLogFiles(_tempDir, retention, "current.log");

        Assert.False(File.Exists(staleFile));
    }
}
