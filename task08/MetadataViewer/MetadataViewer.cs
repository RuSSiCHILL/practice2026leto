
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class MetadataViewer
{
    public static void Run(string dllPath, TextWriter output)
    {
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);

            foreach (var type in assembly.GetTypes())
            {
                output.WriteLine("Класс: " + type.Name);

                foreach (var attr in type.GetCustomAttributes(false))
                    output.WriteLine("  Атрибут: " + attr.GetType().Name);

                output.WriteLine("  Конструкторы:");
                foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                    output.WriteLine("  .ctor(" + string.Join(", ", ctor.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");

                output.WriteLine("  Методы:");
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    output.WriteLine("    " + method.ReturnType.Name + " " + method.Name + "(" + string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name + " " + p.Name)) + ")");

                output.WriteLine();
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            output.WriteLine("Ошибка загрузки типов: " + ex.Message);
        }
        catch (Exception ex)
        {
            output.WriteLine("Ошибка: " + ex.Message);
        }
    }

    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Укажите путь к DLL.");
            return;
        }
        Run(args[0], Console.Out);
    }
}
