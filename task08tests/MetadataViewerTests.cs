using System;
using System.IO;
using Xunit;

public class MetadataViewerTests
{
    private string GetTestDllPath() => typeof(DirectorySizeCommand).Assembly.Location;

    [Fact]
    public void ShowsClassAndAttributes()
    {
        var sw = new StringWriter();
        MetadataViewer.Run(GetTestDllPath(), sw);
        var output = sw.ToString();

        Assert.Contains("DirectorySizeCommand", output);
        Assert.Contains("DisplayNameAttribute", output);
    }

    [Fact]
    public void ShowsConstructorsAndMethods()
    {
        var sw = new StringWriter();
        MetadataViewer.Run(GetTestDllPath(), sw);
        var output = sw.ToString();

        Assert.Contains(".ctor(String directoryPath)", output);
        Assert.Contains("Void Execute()", output);
    }

    [Fact]
    public void FileNotFound_ShowsError()
    {
        var sw = new StringWriter();
        MetadataViewer.Run("nonexistent.dll", sw);
        Assert.Contains("Ошибка", sw.ToString());
    }
}
