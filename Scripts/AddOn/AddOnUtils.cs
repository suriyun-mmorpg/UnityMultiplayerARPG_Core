using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
    public static void InvokeAddOnMethods<T>(this T obj, string baseMethodName, params object[] args) where T : class
    {
        var type = typeof(T);
        var key = new StringBuilder().Append(type.Name).Append('_').Append(baseMethodName).ToString();
        List<MethodInfo> methods = null;
        if (!cacheAddOnMethods.TryGetValue(key, out methods))
        {
            var suffix = new StringBuilder().Append('_').Append(baseMethodName).ToString();
            methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(a => a.Name.EndsWith(suffix)).ToList();
            cacheAddOnMethods[key] = methods;
        }
        if (methods == null || methods.Count == 0) return;
        foreach (var method in methods) method.Invoke(obj, args);
    }
}
