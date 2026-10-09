using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Compatibility entry points backed by the automatic serialized-field patch serializer.</summary>
    public static class PatchDataManager
    {
        public const string KEY_PATCHING = "__PATCHING";
        public const string KEY_TYPE = "__TYPE";
        public const string KEY_ID = "__ID";
        public const string KEY_KEY = "__KEY";
        public static readonly Dictionary<string, IPatchableData> PatchableData = new Dictionary<string, IPatchableData>();
        public static readonly Dictionary<string, Dictionary<string, object>> PatchingData = new Dictionary<string, Dictionary<string, object>>();

        // Keep the legacy key independent of lazy DataId initialization during OnEnable.
        public static string GetPatchKey(this IPatchableData data) => $"{data.GetType().FullName}_{data.Id}";

        public static Dictionary<string, object> GetExportDataForPatching(this IPatchableData target)
        {
            if (target == null)
                return null;
            var data = target.GetExportData();
            data[KEY_TYPE] = target.GetType().Name;
            data[KEY_ID] = target.Id;
            return data;
        }

        public static Dictionary<string, object> GetExportData(this object target)
        {
            if (target == null)
                return null;
            return DataPatchSerializer.Export(target).ToObject<Dictionary<string, object>>();
        }

        public static object ApplyPatch(this object target, Dictionary<string, object> patch)
        {
            if (target == null || patch == null)
                return target;
            var data = JObject.FromObject(patch);
            data.Remove(KEY_PATCHING);
            data.Remove(KEY_TYPE);
            data.Remove(KEY_ID);
            var database = GameInstance.Singleton.GameDatabase as GameDatabase;
            var registry = DataPatchRegistry.Runtime(database, GameInstance.Singleton.DataPatchProfile?.scannedData);
            object copy = target is ScriptableObject asset ? UnityEngine.Object.Instantiate(asset) : typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, null);
            try
            {
                DataPatchSerializer.ReadInto(copy, data, registry);
                foreach (FieldInfo field in DataPatchSerializer.Fields(target.GetType()))
                    field.SetValue(target, field.GetValue(copy));
                if (target is IPatchableData patchableData)
                    patchableData.ClearPatchCaches();
            }
            finally
            {
                if (copy is ScriptableObject staged)
                    UnityEngine.Object.DestroyImmediate(staged);
            }

            return target;
        }
    }
}
