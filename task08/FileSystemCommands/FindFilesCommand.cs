using System;
using System.Collections.Generic;
using System.IO;

[DisplayName("Команда размера каталога")]
[Version(1, 0)]
public class FindFilesCommand : ICommand
{
    private readonly string _directoryPath;
    private readonly string _mask;
    public List<string> FoundFiles { get; private set; }

    public FindFilesCommand(string directoryPath, string mask)
    {
        _directoryPath = directoryPath;
        _mask = mask;
    }

    public void Execute()
    {
        FoundFiles = new List<string>(Directory.GetFiles(_directoryPath, _mask, SearchOption.AllDirectories));
        Console.WriteLine("Найдено файлов: " + FoundFiles.Count);
    }
}