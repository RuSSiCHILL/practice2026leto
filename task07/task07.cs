using System;
using System.Linq;
using System.Reflection;


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class DisplayNameAttribute : Attribute
{
    public string DisplayName { get; }
    public DisplayNameAttribute(string displayName) => DisplayName = displayName;
}

[AttributeUsage(AttributeTargets.Class)]
public class VersionAttribute : Attribute
{
    public int Major { get; }
    public int Minor { get; }
    public VersionAttribute(int major, int minor) 
    { 
        Major = major; 
        Minor = minor; 
    }
}

[DisplayName("Пример класса")]
[Version(1, 0)]
public class SampleClass
{
    [DisplayName("Тестовый метод")]
    public void TestMethod() { }
    [DisplayName("Числовое свойство")]
    public int Number { get; set; }
}


public static class ReflectionHelper
{
    public static void PrintTypeInfo(Type type)
    {
        var displayName = type.GetCustomAttribute<DisplayNameAttribute>();
        var version = type.GetCustomAttribute<VersionAttribute>();

        if (displayName!= null)
            System.Console.WriteLine($"Имя: {displayName.DisplayName}");

        if (version!= null)
            System.Console.WriteLine($"Версия: {version.Major}.{version.Minor}");

        System.Console.WriteLine("Методы:");
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            var methodDisplay = method.GetCustomAttribute<DisplayNameAttribute>();
            var name = methodDisplay?.DisplayName ?? method.Name;
            System.Console.WriteLine($"  {name}");
        }

        System.Console.WriteLine("Свойства:");
        foreach (var prop in type.GetProperties())
        {
            var propDisplay = prop.GetCustomAttribute<DisplayNameAttribute>();
            var name = propDisplay?.DisplayName ?? prop.Name;
            System.Console.WriteLine($"  {name}");
        }
    }
}