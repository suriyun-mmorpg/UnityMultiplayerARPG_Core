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
                    break;
                }
            }
            foreach (FieldInfo field in exportingFields)
            {
                Type fieldType = field.FieldType;
                if (field.FieldType.IsArray)
                    fieldType = field.FieldType.GetElementType();
                if (fieldType.IsClass)
                {
                    if (fieldType.HasInterface<IExportableData>())
                    {
                        IExportableData exportableData = field.GetValue(target) as IExportableData;
                        if (exportableData != null && !string.IsNullOrEmpty(exportableData.Id))
                        {
                            result[field.Name] = new Dictionary<string, string>()
                        {
                            { "Type", fieldType.FullName },
                            { "Id",  exportableData.Id },
                        };
                        }
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        AssetReference aa = field.GetValue(target) as AssetReference;
                        if (aa.IsDataValid())
                        {
                            result[field.Name] = aa.RuntimeKey;
                        }
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(GameObject)))
                    {
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(Component)))
                    {
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(ScriptableObject)))
                    {
                        continue;
                    }
                    result[field.Name] = GetExportData(field.GetValue(target));
                }
                else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
                {
                    Debug.LogError($"S {fieldType.Name}");
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