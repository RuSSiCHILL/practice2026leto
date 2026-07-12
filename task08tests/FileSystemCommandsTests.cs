using System;
using System.IO;
using Xunit;

public class FileSystemCommandsTests
{
    [Fact]
    public void DirectorySizeCommand_ShouldCalculateSize()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "test1.txt"), "Hello");
        File.WriteAllText(Path.Combine(testDir, "test2.txt"), "World!");

        var command = new DirectorySizeCommand(testDir);
        command.Execute();

        Assert.Equal(11, command.Size);

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void DirectorySizeCommand_WithSubdirectories()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        var subDir = Path.Combine(testDir, "SubDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(testDir, "file.txt"), "12345");
        File.WriteAllText(Path.Combine(subDir, "subfile.txt"), "123");

        var command = new DirectorySizeCommand(testDir);
        command.Execute();

        Assert.Equal(8, command.Size);

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_ShouldFindMatchingFiles()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file1.txt"), "Text");
        File.WriteAllText(Path.Combine(testDir, "file2.log"), "Log");

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();

        Assert.Single(command.FoundFiles);

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_WithSubdirectories()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        var subDir = Path.Combine(testDir, "SubDir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(testDir, "a.txt"), "");
        File.WriteAllText(Path.Combine(subDir, "b.txt"), "");
        File.WriteAllText(Path.Combine(subDir, "c.log"), "");

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();

        Assert.Equal(2, command.FoundFiles.Count);

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_NoMatches()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file.log"), "");

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();

        Assert.Empty(command.FoundFiles);

        Directory.Delete(testDir, true);
    }

    [Fact]
    public void DirectorySizeCommand_OutputToConsole()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file.txt"), "12345");

        var strwrt = new StringWriter();
        Console.SetOut(strwrt);

        var command = new DirectorySizeCommand(testDir);
        command.Execute();

        Assert.Contains("Размер каталога", strwrt.ToString());
        Directory.Delete(testDir, true);
    }

    [Fact]
    public void FindFilesCommand_OutputsToConsole()
    {
        var testDir = Path.Combine(Path.GetTempPath(), "TestDir_" + Guid.NewGuid());
        Directory.CreateDirectory(testDir);
        File.WriteAllText(Path.Combine(testDir, "file.txt"), "Text");

        var strwrt = new StringWriter();
        Console.SetOut(strwrt);

        var command = new FindFilesCommand(testDir, "*.txt");
        command.Execute();

        Assert.Contains("Найдено файлов", strwrt.ToString());
        Directory.Delete(testDir, true);
    }
}
