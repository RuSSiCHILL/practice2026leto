using System;
using System.IO;
using System.Linq;
using System.Reflection;

var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSystemCommands.dll");

if (!File.Exists(dllPath))
{
    Console.WriteLine("DLL не найдена.");
    return;
}

var assembly = Assembly.LoadFrom(dllPath);

foreach (var type in assembly.GetTypes().Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract))
{
    var command = (ICommand)Activator.CreateInstance(type);
    command.Execute();
}