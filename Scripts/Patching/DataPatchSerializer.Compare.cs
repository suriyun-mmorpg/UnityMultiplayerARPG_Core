using System;
using System.Collections;
using System.Globalization;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public static partial class DataPatchSerializer
    {
        // Compare the export schema directly against an existing token tree. Keep leaf
        // equality delegated to Json.NET so numeric/enum semantics match Export exactly.
        internal static bool MatchesExport(object value, JToken token, bool root = true, int depth = 0)
        {
            if (depth > 32)
                throw new InvalidOperationException("Patch nesting exceeds 32 levels.");
            if (value == null || value is UnityEngine.Object obj && obj == null)
                return token != null && token.Type == JTokenType.Null;
            if (token == null)
                return false;
            if (!root && value is IPatchableData reference)
                return token is JObject dataId && dataId.Count == 1 && MatchesLeaf(reference.DataId, dataId["dataId"]);
            if (value is Vector2 vector)
                return token is JObject v2 && v2.Count == 2 && MatchesLeaf(vector.x, v2["x"]) && MatchesLeaf(vector.y, v2["y"]);
            if (value is Vector3 vector3)
                return token is JObject v3 && v3.Count == 3 && MatchesLeaf(vector3.x, v3["x"]) && MatchesLeaf(vector3.y, v3["y"]) && MatchesLeaf(vector3.z, v3["z"]);
            if (value is Vector2Int vector2Int)
                return token is JObject v2i && v2i.Count == 2 && MatchesLeaf(vector2Int.x, v2i["x"]) && MatchesLeaf(vector2Int.y, v2i["y"]);
            if (value is Vector3Int vector3Int)
                return token is JObject v3i && v3i.Count == 3 && MatchesLeaf(vector3Int.x, v3i["x"]) && MatchesLeaf(vector3Int.y, v3i["y"]) && MatchesLeaf(vector3Int.z, v3i["z"]);
            if (value is LayerMask mask)
                return token is JObject layer && layer.Count == 1 && MatchesLeaf(mask.value, layer["value"]);
            if (value is Rect rect)
                return token is JObject rectangle && rectangle.Count == 4 && MatchesLeaf(rect.x, rectangle["x"]) && MatchesLeaf(rect.y, rectangle["y"])
                    && MatchesLeaf(rect.width, rectangle["width"]) && MatchesLeaf(rect.height, rectangle["height"]);
            if (value is Bounds bounds)
                return token is JObject box && box.Count == 2 && MatchesExport(bounds.center, box["center"], false, depth + 1)
                    && MatchesExport(bounds.size, box["size"], false, depth + 1);
            Type type = value.GetType();
            if (type.IsEnum)
                return MatchesLeaf(Convert.ToInt64(value, CultureInfo.InvariantCulture), token);
            if (value is char character)
                return MatchesLeaf(character.ToString(), token);
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
                return MatchesLeaf(value, token);
            if (value is IList list)
            {
                if (!(token is JArray array) || array.Count != list.Count)
                    return false;
                for (int i = 0; i < list.Count; i++)
                    if (!MatchesExport(list[i], array[i], false, depth + 1))
                        return false;
                return true;
            }
            if (!root && value is UnityEngine.Object)
                throw new InvalidOperationException($"Unsupported patch reference: {type.Name}");
            if (!(token is JObject data))
                return false;
            var fields = OrderedFields(type);
            if (data.Count != fields.Length)
                return false;
            foreach (var field in fields)
                if (!MatchesExport(field.GetValue(value), data[field.Name], false, depth + 1))
                    return false;
            return true;
        }

        private static bool MatchesLeaf(object value, JToken token)
        {
            try
            {
                return JToken.DeepEquals(new JValue(value), token);
            }
            catch (OverflowException)
            {
                // Json.NET may try to convert double.MaxValue to decimal when comparing
                // numeric domains. Such values cannot match; use the normal read/validation path.
                return false;
            }
        }
    }
}
