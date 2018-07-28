using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class DevExtUtils
{
    private static Dictionary<string, List<MethodInfo>> cacheDevExtMethods = new Dictionary<string, List<MethodInfo>>();
    /// <summary>
    /// This will calls all methods from `obj` that have names as "[anything]_`baseMethodName`" with any number of arguments that can be set via `args`
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="baseMethodName"></param>
    /// <param name="args"></param>
    public static void InvokeClassAddOnMethods<T>(this T obj, string baseMethodName, params object[] args) where T : class
    {
        InvokeAddOnMethods(obj.GetType(), obj, baseMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, args);
    }

    public static void InvokeStaticAddOnMethods(Type type, string baseMethodName, params object[] args)
    {
        InvokeAddOnMethods(type, null, baseMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, args);
    }
    
    private static void InvokeAddOnMethods(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
    {
        var key = new StringBuilder().Append(type.Name).Append('_').Append(baseMethodName).ToString();
        List<MethodInfo> methods = null;
        if (!cacheDevExtMethods.TryGetValue(key, out methods))
        {
            methods = type.GetMethods(bindingFlags).Where(a =>
            {
                var attribute = (DevExtMethodsAttribute)a.GetCustomAttribute(typeof(DevExtMethodsAttribute), true);
                return attribute != null && attribute.BaseMethodName.Equals(baseMethodName);
            }).ToList();
            cacheDevExtMethods[key] = methods;
        }
        if (methods == null || methods.Count == 0) return;
        foreach (var method in methods)
        {
            try
            {
                method.Invoke(obj, args);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
