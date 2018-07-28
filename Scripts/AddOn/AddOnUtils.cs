using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class AddOnUtils
{
    private static Dictionary<string, List<MethodInfo>> cacheAddOnMethods = new Dictionary<string, List<MethodInfo>>();
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

    // Addons functions is a modification of the Finite State Machine built in a tutorial offered by Unity Gems: https://unitygem.wordpress.com/
    // The tutorial can be accessed here: https://unitygem.wordpress.com/state-machine-basic/
    private static void InvokeAddOnMethods(Type type, object obj, string baseMethodName, BindingFlags bindingFlags, params object[] args)
    {
        var key = new StringBuilder().Append(type.Name).Append('_').Append(baseMethodName).ToString();
        List<MethodInfo> methods = null;
        if (!cacheAddOnMethods.TryGetValue(key, out methods))
        {
            var suffix = new StringBuilder().Append('_').Append(baseMethodName).ToString();
            methods = type.GetMethods(bindingFlags).Where(a => a.Name.EndsWith(suffix)).ToList();
            cacheAddOnMethods[key] = methods;
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
                var parameters = method.GetParameters();
                Debug.Log(parameters.Length + " / " + args.Length);
                foreach (var parameter in parameters)
                    Debug.Log(parameter.Name + " " + parameter.ParameterType.Name);
                foreach (var arg in args)
                    Debug.Log(arg);
                Debug.LogException(ex);
            }
        }
    }
}
