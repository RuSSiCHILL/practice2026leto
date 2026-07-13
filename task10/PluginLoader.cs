using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

public class PluginLoader
{
    public List<ICommand> LoadPlugins(string folderPath)
    {
        var result = new List<ICommand>();
        var loadedTypes = new Dictionary<Type, ICommand>();

        if (!Directory.Exists(folderPath))
            return result;
        var pluginTypes = new List<Type>();
        foreach (var dll in Directory.GetFiles(folderPath, "*.dll"))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dll);
                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttribute<PluginLoadAttribute>() != null
                           && typeof(ICommand).IsAssignableFrom(t)
                           && !t.IsAbstract
                           && t.GetConstructor(Type.EmptyTypes) != null);

                pluginTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Ошибка загрузки DLL " + dll + ": " + ex.Message);
            }
        }

        int maxIterations = pluginTypes.Count * pluginTypes.Count;
        int iterations = 0;

        while (loadedTypes.Count < pluginTypes.Count && iterations < maxIterations)
        {
            iterations++;
            bool loadedAny = false;

            foreach (var type in pluginTypes)
            {
                if (loadedTypes.ContainsKey(type))
                    continue;

                var attr = type.GetCustomAttribute<PluginLoadAttribute>();
                bool allDepsLoaded = attr.Dependencies.All(dep => loadedTypes.ContainsKey(dep));

                if (allDepsLoaded)
                {
                    var command = (ICommand)Activator.CreateInstance(type);
                    loadedTypes[type] = command;
                    result.Add(command);
                    command.Execute();
                    loadedAny = true;
                }
            }

            if (!loadedAny)
                throw new InvalidOperationException("Циклическая зависимость или отсутствует зависимый плагин.");
        }

        return result;
    }
}