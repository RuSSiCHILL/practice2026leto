using System;
using System.IO;

public class DirectorySizeCommand : ICommand
{
    private readonly string _directoryPath;
    public long Size { get; private set; }

    public DirectorySizeCommand(string directoryPath)
    {
        _directoryPath = directoryPath;
    }

    public void Execute()
    {
        Size = CalculateSize(_directoryPath);
        Console.WriteLine("Размер каталога: " + Size + " байт");
    }

    private long CalculateSize(string path)
    {
        long size = 0;
        foreach (var file in Directory.GetFiles(path))
            size += new FileInfo(file).Length;
        foreach (var dir in Directory.GetDirectories(path))
            size += CalculateSize(dir);
        return size;
    }
}