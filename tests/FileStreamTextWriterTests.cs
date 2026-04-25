using System;
using System.IO;
using Xunit;

namespace app.Tests;

public sealed class FileStreamTextWriterTests : IDisposable
{
    private readonly string _tempFile;

    public FileStreamTextWriterTests()
    {
        _tempFile = Path.GetTempFileName();
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public void WriteLine_WritesToBothConsoleAndFile()
    {
        var consoleSink = new StringWriter();
        using (var writer = new FileStreamTextWriter(_tempFile, consoleSink))
        {
            writer.WriteLine("hello world");
        }

        Assert.Contains("hello world", consoleSink.ToString());
        Assert.Contains("hello world", File.ReadAllText(_tempFile));
    }

    [Fact]
    public void Write_String_WritesToBothConsoleAndFile()
    {
        var consoleSink = new StringWriter();
        using (var writer = new FileStreamTextWriter(_tempFile, consoleSink))
        {
            writer.Write("partial line");
        }

        Assert.Contains("partial line", consoleSink.ToString());
        Assert.Contains("partial line", File.ReadAllText(_tempFile));
    }

    [Fact]
    public void Write_Char_WritesToBothConsoleAndFile()
    {
        var consoleSink = new StringWriter();
        using (var writer = new FileStreamTextWriter(_tempFile, consoleSink))
        {
            writer.Write('X');
        }

        Assert.Contains("X", consoleSink.ToString());
        Assert.Contains("X", File.ReadAllText(_tempFile));
    }

    [Fact]
    public void MultipleLines_AllAppearInFile()
    {
        var consoleSink = new StringWriter();
        using (var writer = new FileStreamTextWriter(_tempFile, consoleSink))
        {
            writer.WriteLine("line one");
            writer.WriteLine("line two");
        }

        var fileContent = File.ReadAllText(_tempFile);
        Assert.Contains("line one", fileContent);
        Assert.Contains("line two", fileContent);
    }

    [Fact]
    public void Dispose_CanBeCalledSafely()
    {
        var consoleSink = new StringWriter();
        var writer = new FileStreamTextWriter(_tempFile, consoleSink);
        writer.Write("data");
        writer.Dispose();
        // No exception = pass. File is closed; we can read it.
        Assert.Contains("data", File.ReadAllText(_tempFile));
    }

    [Fact]
    public void Flush_DoesNotThrow()
    {
        var consoleSink = new StringWriter();
        using var writer = new FileStreamTextWriter(_tempFile, consoleSink);
        writer.Flush();
        // No exception = pass.
    }
}
