using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

[PluginLoad]
public class PluginA : ICommand
{
    public static bool Executed { get; set; }
    public void Execute() { Executed = true; }
}

[PluginLoad]
public class PluginB : ICommand
{
    public static bool Executed { get; set; }
    public void Execute() { Executed = true; }
}

[PluginLoad(typeof(PluginA))]
public class PluginC : ICommand
{
    public static bool Executed { get; set; }
    public void Execute() { Executed = true; }
}

[PluginLoad(typeof(PluginA), typeof(PluginB))]
public class PluginD : ICommand
{
    public static bool Executed { get; set; }
    public void Execute() { Executed = true; }
}

public class PluginLoaderTests
{
    private string GetTestFolder() => Path.GetDirectoryName(typeof(PluginA).Assembly.Location);

    [Fact]
    public void LoadsAllPlugins()
    {
        var loader = new PluginLoader();
        var plugins = loader.LoadPlugins(GetTestFolder());
        Assert.Equal(4, plugins.Count);
    }

    [Fact]
    public void ExecutesAllPlugins()
    {
        PluginA.Executed = false;
        PluginB.Executed = false;
        PluginC.Executed = false;
        PluginD.Executed = false;

        new PluginLoader().LoadPlugins(GetTestFolder());

        Assert.True(PluginA.Executed);
        Assert.True(PluginB.Executed);
        Assert.True(PluginC.Executed);
        Assert.True(PluginD.Executed);
    }

    [Fact]
    public void DependencyLoadedBeforeDependent()
    {
        var plugins = new PluginLoader().LoadPlugins(GetTestFolder());

        var indexA = plugins.FindIndex(p => p is PluginA);
        var indexC = plugins.FindIndex(p => p is PluginC);

        Assert.True(indexA >= 0 && indexC >= 0);
        Assert.True(indexA < indexC, "PluginA должен быть загружен раньше PluginC");
    }

    [Fact]
    public void EmptyFolderReturnsEmpty()
    {
        var temp = Path.Combine(Path.GetTempPath(), "PluginEmpty_" + Guid.NewGuid());
        Directory.CreateDirectory(temp);

        var plugins = new PluginLoader().LoadPlugins(temp);
        Assert.Empty(plugins);

        Directory.Delete(temp, true);
    }

    [Fact]
    public void NonExistentFolderReturnsEmpty()
    {
        var plugins = new PluginLoader().LoadPlugins("/nonexistent/plugin/folder");
        Assert.Empty(plugins);
    }
}