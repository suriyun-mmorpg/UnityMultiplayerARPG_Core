using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class GenericUtils
{
    private static string findInputFieldScene = string.Empty;
    private static InputField[] inputFields;
#if USE_TEXT_MESH_PRO
    private static TMP_InputField[] textMeshInputFields;
#endif

    public static bool IsFocusInputField()
    {
        if (!findInputFieldScene.Equals(SceneManager.GetActiveScene().name))
        {
            inputFields = null;
#if USE_TEXT_MESH_PRO
            textMeshInputFields = null;
#endif
            findInputFieldScene = SceneManager.GetActiveScene().name;
        }

        if (inputFields == null)
            inputFields = Object.FindObjectsOfType<InputField>();
#if USE_TEXT_MESH_PRO
        if (textMeshInputFields == null)
            textMeshInputFields = Object.FindObjectsOfType<TMP_InputField>();
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
        return false;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layerIndex, bool includeInactive)
    {
        Transform[] childrenTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform childTransform in childrenTransforms)
        {
            childTransform.gameObject.layer = layerIndex;
        }
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T result = gameObject.GetComponent<T>();
        if (result == null)
            result = gameObject.AddComponent<T>();
        return result;
    }

    public static void RemoveChildren(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            Transform lastChild = transform.GetChild(i);
            Object.Destroy(lastChild.gameObject);
        }
    }

    public static void SetChildrenActive(this Transform transform, bool isActive)
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    public static void RemoveObjectsByComponentInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        T[] components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveObjectsByComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        T[] components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveComponents<T>(this GameObject gameObject) where T : Component
    {
        T[] components = gameObject.GetComponents<T>();
        foreach (T component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        T[] components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (T component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
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
}
