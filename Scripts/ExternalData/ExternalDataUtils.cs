using Insthync.AddressableAssetTools;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG
{
    public static class ExternalDataUtils
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

        public static Dictionary<string, object> GetExportData(this object target)
        {
            if (target == null)
                return null;
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<FieldInfo> exportingFields = new List<FieldInfo>();
            FieldInfo[] publicFields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] nonPublicFields = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            exportingFields.AddRange(publicFields);
            for (int i = 0; i < nonPublicFields.Length; ++i)
            {
                if (nonPublicFields[i].HasAttribute<SerializeField>())
                {
                    exportingFields.Add(nonPublicFields[i]);
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
                        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                        if (elementType.HasInterface<IGameData>())
                        {
                            for (int i = 0; i < arr.Length; ++i)
                            {
                                IGameData gameData = arr.GetValue(i) as IGameData;
                                if (gameData != null && !string.IsNullOrEmpty(gameData.Id))
                                {
                                    dicts.Add(new Dictionary<string, object>()
                                    {
                                        { "Type", elementType.FullName },
                                        { "Id",  gameData.Id },
                                    });
                                }
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(AssetReference)))
                        {
                            for (int i = 0; i < arr.Length; ++i)
                            {
                                AssetReference aa = arr.GetValue(i) as AssetReference;
                                if (aa.IsDataValid())
                                {
                                    dicts.Add(new Dictionary<string, object>()
                                    {
                                        { "Type", elementType.FullName },
                                        { "Key", aa.RuntimeKey },
                                    });
                                }
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            continue;
                        }
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            dicts.Add(GetExportData(arr.GetValue(i)));
                        }
                        result[field.Name] = dicts;
                    }
                    else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
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
                        if (gameData != null && !string.IsNullOrEmpty(gameData.Id))
                        {
                            result[field.Name] = new Dictionary<string, string>()
                            {
                                { "Type", fieldType.FullName },
                                { "Id",  gameData.Id },
                            };
                        }
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        AssetReference aa = field.GetValue(target) as AssetReference;
                        if (aa.IsDataValid())
                        {
                            result[field.Name] = new Dictionary<string, object>()
                            {
                                { "Type", fieldType.FullName },
                                { "Key", aa.RuntimeKey },
                            };
                        }
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
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