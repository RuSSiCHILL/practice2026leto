using System;

[AttributeUsage(AttributeTargets.Class)]
public class PluginLoadAttribute : Attribute
{
    public Type[] Dependencies { get; }

    public PluginLoadAttribute(params Type[] dependencies)
    {
        Dependencies = dependencies;
    }
}
