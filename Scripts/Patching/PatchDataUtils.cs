using Insthync.AddressableAssetTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG
{
    public static class PatchDataUtils
    {
        public static bool HasAttribute<TAttributeType>(this FieldInfo field)
            where TAttributeType : System.Attribute
        {
            foreach (System.Attribute attr in field.GetCustomAttributes())
            {
                if (attr.GetType() == typeof(TAttributeType))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool HasInterface<TInterfaceType>(this Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType == typeof(TInterfaceType))
                {
                    return true;
                }
            }
            return false;
        }

        public static Dictionary<string, object> GetExportDataForPatching(this IPatchableData patchable)
        {
            if (patchable == null)
                return null;
            Dictionary<string, object> result = GetExportData(patchable);
            result["Type"] = patchable.GetType().FullName;
            result["Id"] = patchable.Id;
            return result;
        }

        public static Dictionary<string, object> GetExportData(this object target)
        {
            if (target == null)
                return null;
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<FieldInfo> exportingFields = new List<FieldInfo>(target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            for (int i = exportingFields.Count - 1; i >= 0; --i)
            {
                bool isRemoving = false;
                if (!exportingFields[i].IsPublic && !exportingFields[i].HasAttribute<SerializeField>())
                {
                    isRemoving = true;
                }
                if (exportingFields[i].HasAttribute<NonSerializedAttribute>())
                {
                    isRemoving = true;
                }
                if (isRemoving)
                {
                    exportingFields.RemoveAt(i);
                }
            }
            foreach (FieldInfo field in exportingFields)
            {
                Type fieldType = field.FieldType;
                if (fieldType.IsArray)
                {
                    Type elementType = field.FieldType.GetElementType();
                    if (elementType.IsClass)
                    {
                        if (elementType == typeof(string))
                        {
                            result[field.Name] = field.GetValue(target);
                            continue;
                        }
                        Array arr = field.GetValue(target) as Array;
                        if (arr == null)
                        {
                            continue;
                        }
                        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                        if (elementType.HasInterface<IGameData>())
                        {
                            for (int i = 0; i < arr.Length; ++i)
                            {
                                IGameData gameData = arr.GetValue(i) as IGameData;
                                if (gameData == null || string.IsNullOrEmpty(gameData.Id))
                                    continue;
                                dicts.Add(new Dictionary<string, object>()
                                {
                                    { "Type", gameData.GetType().FullName },
                                    { "Id",  gameData.Id },
                                });
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(AssetReference)))
                        {
                            for (int i = 0; i < arr.Length; ++i)
                            {
                                AssetReference aa = arr.GetValue(i) as AssetReference;
                                if (!aa.IsDataValid())
                                    continue;
                                dicts.Add(new Dictionary<string, object>()
                                {
                                    { "Type", aa.GetType().FullName },
                                    { "Key", aa.RuntimeKey },
                                });
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(UnityEngine.Object)) ||
                            elementType.IsSubclassOf(typeof(Delegate)))
                        {
                            continue;
                        }
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            dicts.Add(GetExportData(arr.GetValue(i)));
                        }
                        result[field.Name] = dicts;
                    }
                    else if (elementType.IsValueType && !elementType.IsPrimitive && !elementType.IsEnum)
                    {
                        Array arr = field.GetValue(target) as Array;
                        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            dicts.Add(GetExportData(arr.GetValue(i)));
                        }
                        result[field.Name] = dicts;
                    }
                    else
                    {
                        result[field.Name] = field.GetValue(target);
                    }
                }
                else if (fieldType.IsClass)
                {
                    if (fieldType == typeof(string))
                    {
                        result[field.Name] = field.GetValue(target);
                        continue;
                    }
                    if (fieldType.HasInterface<IGameData>())
                    {
                        IGameData gameData = field.GetValue(target) as IGameData;
                        if (gameData == null || string.IsNullOrEmpty(gameData.Id))
                            continue;
                        result[field.Name] = new Dictionary<string, object>()
                        {
                            { "Type", gameData.GetType().FullName },
                            { "Id",  gameData.Id },
                        };
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        AssetReference aa = field.GetValue(target) as AssetReference;
                        if (!aa.IsDataValid())
                            continue;
                        result[field.Name] = new Dictionary<string, object>()
                        {
                            { "Type", aa.GetType().FullName },
                            { "Key", aa.RuntimeKey },
                        };
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) ||
                        fieldType.IsSubclassOf(typeof(Delegate)))
                    {
                        continue;
                    }
                    result[field.Name] = GetExportData(field.GetValue(target));
                }
                else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
                {
                    result[field.Name] = GetExportData(field.GetValue(target));
                }
                else
                {
                    result[field.Name] = field.GetValue(target);
                }
            }
            return result;
        }
    }
}