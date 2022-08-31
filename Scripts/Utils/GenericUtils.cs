using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Networking;
using TMPro;

public static class GenericUtils
{
    private static List<InputField> inputFields;
    private static List<TMP_InputField> textMeshInputFields;
    private static bool isSetOnActiveSceneChanged_ResetInputField;
    private static System.Random randomizer = new System.Random();

    public static bool IsFocusInputField()
    {
        GameObject[] rootObjects;
        if (inputFields == null || textMeshInputFields == null)
        {
            inputFields = new List<InputField>();
            textMeshInputFields = new List<TMP_InputField>();
            rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                inputFields.AddRange(rootObject.GetComponentsInChildren<InputField>(true));
                textMeshInputFields.AddRange(rootObject.GetComponentsInChildren<TMP_InputField>(true));
            }
        }
        foreach (InputField inputField in inputFields)
        {
            if (inputField.isFocused)
                return true;
        }
        foreach (TMP_InputField inputField in textMeshInputFields)
        {
            if (inputField.isFocused)
                return true;
        }
        if (!isSetOnActiveSceneChanged_ResetInputField)
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged_ResetInputField;
            isSetOnActiveSceneChanged_ResetInputField = true;
        }
        return false;
    }

    public static void OnActiveSceneChanged_ResetInputField(Scene scene1, Scene scene2)
    {
        inputFields = null;
        textMeshInputFields = null;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layerIndex, bool includeInactive)
    {
        if (gameObject == null)
            return;
        gameObject.layer = layerIndex;
        Transform[] childrenTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform childTransform in childrenTransforms)
        {
            childTransform.gameObject.layer = layerIndex;
        }
    }

    public static List<T> GetComponents<T>(this IEnumerable<GameObject> gameObjects) where T : Component
    {
        List<T> result = new List<T>();
        T comp;
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == null)
                continue;
            if (gameObject.TryGetComponent(out comp))
                result.Add(comp);
        }
        return result;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return null;
        T result;
        if (!gameObject.TryGetComponent(out result))
            result = gameObject.AddComponent<T>();
        return result;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject, System.Action<T> onAddComponent) where T : Component
    {
        if (gameObject == null)
            return null;
        T result;
        if (!gameObject.TryGetComponent(out result))
        {
            result = gameObject.AddComponent<T>();
            onAddComponent.Invoke(result);
        }
        return result;
    }

    public static TInterface GetOrAddComponent<TInterface, T>(this GameObject gameObject)
        where T : Component, TInterface
    {
        if (gameObject == null)
            return default;
        TInterface result;
        if (!gameObject.TryGetComponent(out result))
            result = gameObject.AddComponent<T>();
        return result;
    }

    public static TInterface GetOrAddComponent<TInterface, T>(this GameObject gameObject, System.Action<TInterface> onAddComponent)
        where T : Component, TInterface
    {
        if (gameObject == null)
            return default;
        TInterface result;
        if (!gameObject.TryGetComponent(out result))
        {
            result = gameObject.AddComponent<T>();
            onAddComponent.Invoke(result);
        }
        return result;
    }

    public static void RemoveChildren(this Transform transform, bool immediatelyMode = false)
    {
        if (transform == null)
            return;
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform lastChild = transform.GetChild(i);
            if (!immediatelyMode)
                Object.Destroy(lastChild.gameObject);
            else
                Object.DestroyImmediate(lastChild.gameObject);
        }
    }

    public static void SetChildrenActive(this Transform transform, bool isActive)
    {
        if (transform == null)
            return;
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    public static void RemoveObjectsByComponentInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        if (gameObject == null)
            return;
        T[] components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveObjectsByComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        if (gameObject == null)
            return;
        T[] components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveComponents<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return;
        T[] components = gameObject.GetComponents<T>();
        foreach (T component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        if (gameObject == null)
            return;
        T[] components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        if (gameObject == null)
            return;
        T[] components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static string GetUniqueId(int length = 16, string mask = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-")
    {
        return Nanoid.Nanoid.Generate(mask, length);
    }

    public static string GetMD5(this string text)
    {
        // byte array representation of that string
        byte[] encodedPassword = new UTF8Encoding().GetBytes(text);

        // need MD5 to calculate the hash
        byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

        // string representation (similar to UNIX format)
        return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
    }

    public static int GenerateHashId(this string id)
    {
        if (string.IsNullOrEmpty(id))
            return 0;

        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < id.Length && id[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ id[i];
                if (i == id.Length - 1 || id[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ id[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }

    public static int GetNegativePositive()
    {
        return Random.value > 0.5f ? 1 : -1;
    }

    public static void SetAndStretchToParentSize(this RectTransform rect, RectTransform parentRect)
    {
        rect.SetParent(parentRect);
        rect.localScale = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one * 0.5f;
        rect.sizeDelta = Vector3.zero;
    }

    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static Vector3 GetXZ(this Vector3 position)
    {
        return new Vector3(position.x, 0f, position.z);
    }

    public static Vector3 GetXY(this Vector3 position)
    {
        return new Vector3(position.x, position.y, 0f);
    }

    public static bool IsPointInBox(Vector3 center, Vector3 half, Vector3 dirX, Vector3 dirY, Vector3 dirZ, Vector3 point)
    {
        Vector3 d = point - center;
        return Mathf.Abs(Vector3.Dot(d, dirX)) <= half.x &&
            Mathf.Abs(Vector3.Dot(d, dirY)) <= half.y &&
            Mathf.Abs(Vector3.Dot(d, dirZ)) <= half.z;
    }

    public static bool ColliderIntersect(this Collider source, Collider dest, float sourceSizeRate = 1f)
    {
        Vector3 sourceSize;
        Vector3[] sourcePoints;
        source.GetActualSizeBoundsPoints(out sourceSize, out sourcePoints);
        Vector3 sourceCenter = (sourcePoints[0] + sourcePoints[7]) * 0.5f;
        Vector3 sourceHalf = sourceSize * sourceSizeRate * 0.5f;
        Collider[] results = Physics.OverlapBox(sourceCenter, sourceHalf, source.transform.rotation, LayerMask.GetMask(LayerMask.LayerToName(dest.gameObject.layer)), QueryTriggerInteraction.Collide);
        for (int i = 0; i < results.Length; ++i)
        {
            if (results[i] == dest)
                return true;
        }
        return false;
    }

    public static bool ColliderIntersect(this Collider2D source, Collider2D dest, float sourceSizeRate = 1f)
    {
        Vector3 sourceSize;
        Vector3[] sourcePoints;
        source.GetActualSizeBoundsPoints(out sourceSize, out sourcePoints);
        Vector3 sourceCenter = (sourcePoints[0] + sourcePoints[7]) * 0.5f;
        Vector3 sourceHalf = sourceSize * sourceSizeRate * 0.5f;
        Collider2D[] results = Physics2D.OverlapBoxAll(sourceCenter, sourceHalf, source.transform.eulerAngles.z, LayerMask.GetMask(LayerMask.LayerToName(dest.gameObject.layer)));
        for (int i = 0; i < results.Length; ++i)
        {
            if (results[i] == dest)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Get OBB size and points
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="size"></param>
    /// <param name="points"></param>
    public static void GetActualSizeBoundsPoints(this Collider collider, out Vector3 size, out Vector3[] points)
    {
        points = new Vector3[8];
        Vector3[] sourcePoints = new Vector3[8];
        Transform transform = collider.transform;

        // Store original rotation
        Quaternion originalRotation = transform.rotation;

        // Reset rotation
        transform.rotation = Quaternion.identity;

        // Get object bounds from unrotated object
        Bounds bounds = collider.bounds;
        size = bounds.size;

        // Get the unrotated points
        sourcePoints[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - transform.position; // Bot left near
        sourcePoints[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - transform.position; // Bot right near
        sourcePoints[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - transform.position; // Top left near
        sourcePoints[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - transform.position; // Top right near
        sourcePoints[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - transform.position; // Bot left far
        sourcePoints[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - transform.position; // Bot right far
        sourcePoints[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - transform.position; // Top left far
        sourcePoints[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - transform.position; // Top right far

        // Apply scaling
        for (int i = 0; i < sourcePoints.Length; i++)
        {
            sourcePoints[i] = new Vector3(sourcePoints[i].x / transform.localScale.x,
                sourcePoints[i].y / transform.localScale.y,
                sourcePoints[i].z / transform.localScale.z);
        }

        // Restore rotation
        transform.rotation = originalRotation;

        // Transform points from local to world space
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.TransformPoint(sourcePoints[i]);
        }
    }

    /// <summary>
    /// Get OBB size and points
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="size"></param>
    /// <param name="points"></param>
    public static void GetActualSizeBoundsPoints(this Collider2D collider, out Vector3 size, out Vector3[] points)
    {
        points = new Vector3[8];
        Vector3[] sourcePoints = new Vector3[8];
        Transform transform = collider.transform;

        // Store original rotation
        Quaternion originalRotation = transform.rotation;

        // Reset rotation
        transform.rotation = Quaternion.identity;

        // Get object bounds from unrotated object
        Bounds bounds = collider.bounds;
        size = bounds.size;

        // Get the unrotated points
        sourcePoints[0] = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z) - transform.position; // Bot left near
        sourcePoints[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z) - transform.position; // Bot right near
        sourcePoints[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z) - transform.position; // Top left near
        sourcePoints[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z) - transform.position; // Top right near
        sourcePoints[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z) - transform.position; // Bot left far
        sourcePoints[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z) - transform.position; // Bot right far
        sourcePoints[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z) - transform.position; // Top left far
        sourcePoints[7] = new Vector3(bounds.max.x, bounds.max.y, bounds.max.z) - transform.position; // Top right far

        // Apply scaling
        for (int i = 0; i < sourcePoints.Length; i++)
        {
            sourcePoints[i] = new Vector3(sourcePoints[i].x / transform.localScale.x,
                sourcePoints[i].y / transform.localScale.y,
                sourcePoints[i].z / transform.localScale.z);
        }

        // Restore rotation
        transform.rotation = originalRotation;

        // Transform points from local to world space
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.TransformPoint(sourcePoints[i]);
        }
    }

    public static string ToBonusString(this short value, string format = "N0")
    {
        return value >= 0 ? "+" + value.ToString(format) : value.ToString(format);
    }

    public static string ToBonusString(this int value, string format = "N0")
    {
        return value >= 0 ? "+" + value.ToString(format) : value.ToString(format);
    }

    public static string ToBonusString(this float value, string format = "N0")
    {
        return value >= 0 ? "+" + value.ToString(format) : value.ToString(format);
    }

    public static void Shuffle<T>(this IList<T> list, System.Random random)
    {
        if (list == null || list.Count <= 1)
            return;
        int tempRandomIndex;
        T tempEntry;
        for (int i = 0; i < list.Count - 1; ++i)
        {
            tempRandomIndex = random.Next(i, list.Count);
            tempEntry = list[i];
            list[i] = list[tempRandomIndex];
            list[tempRandomIndex] = tempEntry;
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        randomizer.Next();
        Shuffle(list, randomizer);
    }

    public static int Increase(this int a, int b)
    {
        try
        {
            checked
            {
                return a + b;
            }
        }
        catch (System.OverflowException)
        {
            return int.MaxValue;
        }
    }

    public static float RandomFloat(this System.Random random, float min, float max)
    {
        return (float)random.NextDouble() * (max - min) + min;
    }

    public static float RandomFloat(int seed, float min, float max)
    {
        return new System.Random(seed).RandomFloat(min, max);
    }

    public static int RandomInt(this System.Random random, int min, int max)
    {
        return random.Next(min, max);
    }

    public static int RandomInt(int seed, int min, int max)
    {
        return new System.Random(seed).RandomInt(min, max);
    }

    public static string GetPrettyDate(this System.DateTime dateTime,
        string textNow = "a few seconds ago",
        string textAMinuteAgo = "1 Minute ago",
        string textAHourAgo = "1 Hour ago",
        string textYesterday = "Yesterday",
        string formatMinutesAgo = "{0} Minutes ago",
        string formatHoursAgo = "{0} Hours ago",
        string formatDaysAgo = "{0} Days ago",
        string formatWeeksAgo = "{0} Weeks ago",
        string formatMonthsAgo = "{0} Months ago",
        string textUnknow = "Unknow")
    {
        System.TimeSpan dateTimeDiff = System.DateTime.Now.Subtract(dateTime);
        int monthDiff = (int)(dateTimeDiff.TotalDays / 30);
        int dayDiff = (int)dateTimeDiff.TotalDays;
        int secDiff = (int)dateTimeDiff.TotalSeconds;

        // Don't allow out of range values.
        if (dayDiff < 0)
            return textUnknow;

        // Handle same-day times.
        if (dayDiff == 0)
        {
            // Less than one minute ago.
            if (secDiff < 60)
                return textNow;
            // Less than 2 minutes ago.
            if (secDiff < 120)
                return textAMinuteAgo;
            // Less than one hour ago.
            if (secDiff < 3600)
                return string.Format(formatMinutesAgo, Mathf.FloorToInt((float)secDiff / 60f));
            // Less than 2 hours ago.
            if (secDiff < 7200)
                return textAHourAgo;
            // Less than one day ago.
            if (secDiff < 86400)
                return string.Format(formatHoursAgo, Mathf.FloorToInt((float)secDiff / 3600f));
        }
        // Handle previous days.
        if (dayDiff == 1)
            return textYesterday;
        if (dayDiff < 7)
            return string.Format(formatDaysAgo, dayDiff);
        if (dayDiff < 30)
            return string.Format(formatWeeksAgo, Mathf.CeilToInt((float)dayDiff / 7f));
        if (monthDiff < 12)
            return string.Format(formatMonthsAgo, monthDiff);

        return textUnknow;
    }

    public static System.Uri Append(this System.Uri uri, params string[] paths)
    {
        return new System.Uri(paths.Aggregate(uri.AbsoluteUri, (current, path) => string.Format("{0}/{1}", current.TrimEnd('/'), path.TrimStart('/'))));
    }

    public static bool IsError(this UnityWebRequest unityWebRequest)
    {
#if UNITY_2020_2_OR_NEWER
            UnityWebRequest.Result result = unityWebRequest.result;
            return (result == UnityWebRequest.Result.ConnectionError)
                || (result == UnityWebRequest.Result.DataProcessingError)
                || (result == UnityWebRequest.Result.ProtocolError);
#else
        return unityWebRequest.isHttpError || unityWebRequest.isNetworkError;
#endif
    }

    public static void DrawSquareGizmos(Transform transform, Color color, float squareSizeX, float squareSizeZ)
    {
        // Set color
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;
        // 1--2
        // |  |
        // 3--4
        Vector3 p1 = new Vector3(transform.position.x - squareSizeX / 2, transform.position.y, transform.position.z - squareSizeZ / 2);
        Vector3 p2 = new Vector3(transform.position.x + squareSizeX / 2, transform.position.y, transform.position.z - squareSizeZ / 2);
        Vector3 p3 = new Vector3(transform.position.x - squareSizeX / 2, transform.position.y, transform.position.z + squareSizeZ / 2);
        Vector3 p4 = new Vector3(transform.position.x + squareSizeX / 2, transform.position.y, transform.position.z + squareSizeZ / 2);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p4);
        Gizmos.DrawLine(p4, p3);
        Gizmos.DrawLine(p3, p1);
        // Restore default colors
        Gizmos.color = defaultColor;
    }

    public static void DrawCircleGizmos(Transform transform, Color color, float radius)
    {
        // Set matrix
        Matrix4x4 defaultMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        // Set color
        Color defaultColor = Gizmos.color;
        Gizmos.color = color;
        // Draw a ring
        Vector3 beginPoint = Vector3.zero;
        Vector3 firstPoint = Vector3.zero;
        for (float theta = 0; theta < 2 * Mathf.PI; theta += 0.1f)
        {
            float x = radius * Mathf.Cos(theta);
            float z = radius * Mathf.Sin(theta);
            Vector3 endPoint = new Vector3(x, 0, z);
            if (theta == 0)
            {
                firstPoint = endPoint;
            }
            else
            {
                Gizmos.DrawLine(beginPoint, endPoint);
            }
            beginPoint = endPoint;
        }
        // Draw the last segment
        Gizmos.DrawLine(firstPoint, beginPoint);
        // Restore default colors
        Gizmos.color = defaultColor;
        // Restore default matrix
        Gizmos.matrix = defaultMatrix;
    }
}
