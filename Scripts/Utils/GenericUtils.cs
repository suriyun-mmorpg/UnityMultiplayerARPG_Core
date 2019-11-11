using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class GenericUtils
{
    private static List<InputField> inputFields;
#if USE_TEXT_MESH_PRO
    private static List<TMP_InputField> textMeshInputFields;
#endif
    private static bool isSetOnActiveSceneChanged_ResetInputField;

    public static bool IsFocusInputField()
    {
        GameObject[] rootObjects;
#if USE_TEXT_MESH_PRO
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
#else
        if (inputFields == null)
        {
            inputFields = new List<InputField>();
            rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject rootObject in rootObjects)
            {
                inputFields.AddRange(rootObject.GetComponentsInChildren<InputField>(true));
            }
        }
#endif

        foreach (InputField inputField in inputFields)
        {
            if (inputField.isFocused)
                return true;
        }
#if USE_TEXT_MESH_PRO
        foreach (TMP_InputField inputField in textMeshInputFields)
        {
            if (inputField.isFocused)
                return true;
        }
#endif
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
#if USE_TEXT_MESH_PRO
        textMeshInputFields = null;
#endif
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layerIndex, bool includeInactive)
    {
        if (gameObject == null)
            return;
        Transform[] childrenTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform childTransform in childrenTransforms)
        {
            childTransform.gameObject.layer = layerIndex;
        }
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (gameObject == null)
            return null;
        T result = gameObject.GetComponent<T>();
        if (result == null)
            result = gameObject.AddComponent<T>();
        return result;
    }

    public static void RemoveChildren(this Transform transform)
    {
        if (transform == null)
            return;
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform lastChild = transform.GetChild(i);
            Object.Destroy(lastChild.gameObject);
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

    public static string GetUniqueId(int length = 8, string mask = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
    {
        char[] chars = mask.ToCharArray();
        RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        byte[] data = new byte[length];
        crypto.GetNonZeroBytes(data);
        StringBuilder result = new StringBuilder(length);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length - 1)]);
        }
        return result.ToString();
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
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = parentRect.rect.size;
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector2.one;
    }

    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
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

    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null || list.Count <= 1)
            return;
        int tempRandomIndex;
        T tempEntry;
        for (int i = 0; i < list.Count - 1; ++i)
        {
            tempRandomIndex = Random.Range(i, list.Count);
            tempEntry = list[i];
            list[i] = list[tempRandomIndex];
            list[tempRandomIndex] = tempEntry;
        }
    }
}
