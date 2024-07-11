using Insthync.AddressableAssetTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG
{
    public static class PatchDataUtils
    {
        public const string KEY_PATCHING = "__PATCHING";
        public const string KEY_TYPE = "__TYPE";
        public const string KEY_ID = "__ID";
        public const string KEY_KEY = "__KEY";

        /// <summary>
        /// Dictionary: [$"{TYPE}_{ID}, DATA]
        /// </summary>
        public static readonly Dictionary<string, object> PatchingData = new Dictionary<string, object>();

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

        public static bool IsListOrArray(this Type type, out Type itemType)
        {
            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    itemType = type.GetGenericArguments()[0];
                    return true;
                }
            }
            itemType = null;
            return false;
        }

        public static Dictionary<string, object> GetExportDataForPatching(this IPatchableData target)
        {
            if (target == null)
                return null;
            Dictionary<string, object> result = GetExportData(target);
            result[KEY_TYPE] = target.GetType().FullName;
            result[KEY_ID] = target.Id;
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
                if (fieldType.IsListOrArray(out Type elementType))
                {
                    if (elementType.IsClass)
                    {
                        if (elementType == typeof(string))
                        {
                            result[field.Name] = field.GetValue(target);
                            continue;
                        }
                        IList arr = field.GetValue(target) as IList;
                        if (arr == null)
                        {
                            continue;
                        }
                        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                        if (elementType.HasInterface<IGameData>())
                        {
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                StoreGameDataPatchData(dicts, arr[i] as IGameData);
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(AssetReference)))
                        {
                            for (int i = 0; i < arr.Count; ++i)
                            {
                                StoreAddressablePatchData(dicts, arr[i] as AssetReference);
                            }
                            result[field.Name] = dicts;
                            continue;
                        }
                        if (elementType.IsSubclassOf(typeof(UnityEngine.Object)) ||
                            elementType.IsSubclassOf(typeof(Delegate)))
                        {
                            continue;
                        }
                        for (int i = 0; i < arr.Count; ++i)
                        {
                            dicts.Add(GetExportData(arr[i]));
                        }
                        result[field.Name] = dicts;
                    }
                    else if (elementType.IsValueType && !elementType.IsPrimitive && !elementType.IsEnum)
                    {
                        IList arr = field.GetValue(target) as IList;
                        if (arr == null)
                        {
                            continue;
                        }
                        List<Dictionary<string, object>> dicts = new List<Dictionary<string, object>>();
                        for (int i = 0; i < arr.Count; ++i)
                        {
                            dicts.Add(GetExportData(arr[i]));
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
                        StoreGameDataPatchData(result, field.Name, field.GetValue(target) as IGameData);
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        StoreAddressablePatchData(result, field.Name, field.GetValue(target) as AssetReference);
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
            result[KEY_PATCHING] = true;
            return result;
        }

        private static void StoreGameDataPatchData(List<Dictionary<string, object>> list, IGameData gameData)
        {
            if (gameData == null || string.IsNullOrEmpty(gameData.Id))
                return;
            list.Add(GetGameDataPatchData(gameData));
        }

        private static void StoreAddressablePatchData(List<Dictionary<string, object>> list, AssetReference aa)
        {
            if (!aa.IsDataValid())
                return;
            list.Add(GetAddressablePatchData(aa));
        }

        private static void StoreGameDataPatchData(Dictionary<string, object> result, string fieldName, IGameData gameData)
        {
            if (gameData == null || string.IsNullOrEmpty(gameData.Id))
                return;
            result[fieldName] = GetGameDataPatchData(gameData);
        }

        private static void StoreAddressablePatchData(Dictionary<string, object> result, string fieldName, AssetReference aa)
        {
            if (!aa.IsDataValid())
                return;
            result[fieldName] = GetAddressablePatchData(aa);
        }

        private static Dictionary<string, object> GetGameDataPatchData(IGameData gameData)
        {
            if (gameData == null || string.IsNullOrEmpty(gameData.Id))
                return null;
            return new Dictionary<string, object>()
            {
                { KEY_TYPE, gameData.GetType().FullName },
                { KEY_ID,  gameData.Id },
            };
        }

        private static Dictionary<string, object> GetAddressablePatchData(AssetReference aa)
        {
            if (!aa.IsDataValid())
                return null;
            return new Dictionary<string, object>()
            {
                { KEY_TYPE, aa.GetType().FullName },
                { KEY_KEY, aa.RuntimeKey },
            };
        }

        public static object ApplyPatch(this object target, Dictionary<string, object> patchingData)
        {
            if (target == null || patchingData == null || !patchingData.ContainsKey(KEY_PATCHING))
                return target;
            Type targetType = target.GetType();
            foreach (var entry in patchingData)
            {
                string fieldName = entry.Key;
                FieldInfo field = targetType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field == null)
                    continue;
                Type fieldType = field.FieldType;
                if (fieldType.IsListOrArray(out Type elementType))
                {
                    if (elementType.IsClass)
                    {
                        if (elementType == typeof(string))
                        {
                            field.SetValue(target, entry.Value);
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
                                    { KEY_TYPE, gameData.GetType().FullName },
                                    { KEY_ID,  gameData.Id },
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
                                    { KEY_TYPE, aa.GetType().FullName },
                                    { KEY_KEY, aa.RuntimeKey },
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
                        IList arr = field.GetValue(target) as IList;
                        Array convArr = Array.CreateInstance(elementType, arr.Count);
                        if (arr == null)
                        {
                            continue;
                        }
                        for (int i = 0; i < arr.Count; ++i)
                        {
                            arr[i] = ApplyPatch(arr[i], entry.Value as Dictionary<string, object>);
                            convArr.SetValue(arr[i], i);
                        }
                        if (fieldType.IsArray)
                        {
                            // Array
                            field.SetValue(target, convArr);
                        }
                        else
                        {
                            // List
                            field.SetValue(target, arr);
                        }
                    }
                    else
                    {
                        field.SetValue(target, entry.Value);
                    }
                }
                else if (fieldType.IsClass)
                {
                    if (fieldType == typeof(string))
                    {
                        field.SetValue(target, entry.Value);
                        continue;
                    }
                    if (fieldType.HasInterface<IGameData>())
                    {
                        ApplyGameDataPatchData(field, target, entry.Value as Dictionary<string, object>);
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(AssetReference)))
                    {
                        ApplyAddressablePatchData(field, target, entry.Value as Dictionary<string, object>);
                        continue;
                    }
                    if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)) ||
                        fieldType.IsSubclassOf(typeof(Delegate)))
                    {
                        continue;
                    }
                    field.SetValue(target, ApplyPatch(field.GetValue(target), entry.Value as Dictionary<string, object>));
                }
                else if (fieldType.IsValueType && !fieldType.IsPrimitive && !fieldType.IsEnum)
                {
                    field.SetValue(target, ApplyPatch(field.GetValue(target), entry.Value as Dictionary<string, object>));
                }
                else
                {
                    field.SetValue(target, entry.Value);
                }
            }
            return target;
        }

        private static void ApplyGameDataPatchData(FieldInfo field, object target, Dictionary<string, object> patchData)
        {
            if (field.DeclaringType is not IGameData)
                return;
            if (!patchData.TryGetValue(KEY_TYPE, out object type) || !patchData.TryGetValue(KEY_ID, out object id))
                return;
            // TODO: Get data by type and id
            IGameData foundData = null;
            field.SetValue(target, foundData);
        }

        private static void ApplyAddressablePatchData(FieldInfo field, object target, Dictionary<string, object> patchData)
        {
            AssetReference aa = field.GetValue(target) as AssetReference;
            if (aa == null || patchData == null)
                return;
            if (!patchData.TryGetValue(KEY_KEY, out object aaKey))
                return;
            FieldInfo guidField = aa.GetType().GetField("m_AssetGUID", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            guidField.SetValue(aa, aaKey);
        }
    }
}