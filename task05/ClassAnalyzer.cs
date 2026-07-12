using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public class ClassAnalyzer
{
    private Type _type;

    public ClassAnalyzer(Type type)
    {
        _type = type ?? throw new ArgumentNullException(nameof(type));
    }


    public IEnumerable<string> GetPublicMethods(){
        return _type.GetMethods().Where(s=> s.IsPublic).Select(s=> s.Name);
    }

    public IEnumerable<string> GetMethodParams(string methodName){
        var method=_type.GetMethod(methodName);
        if (method==null){
            return new List<string>();
        }
        var parameters=method.GetParameters().Select(s => s.ParameterType.Name+" "+s.Name);
        var returnType=method.ReturnType.Name;
        return parameters.Append("Returns: "+returnType);
    }

    public IEnumerable<string> GetAllFields(){
        return _type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(s => s.Name);
    }

    public IEnumerable<string> GetProperties(){
        return _type.GetProperties().Select(s => s.Name);
    }

    public bool HasAttribute<T>() where T : Attribute{
        return _type.IsDefined(typeof(T), false);  
    }
}
