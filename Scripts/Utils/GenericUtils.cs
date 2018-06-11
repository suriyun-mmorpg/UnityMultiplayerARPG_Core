using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class GenericUtils
{
    public static void SetLayerRecursively(this GameObject gameObject, int layerIndex, bool includeInactive)
    {
        var childrenTransforms = gameObject.GetComponentsInChildren<Transform>(includeInactive);
        foreach (Transform childTransform in childrenTransforms)
        {
            childTransform.gameObject.layer = layerIndex;
        }
    }

    public static void RemoveChildren(this Transform transform)
    {
        for (var i = transform.childCount - 1; i >= 0; --i)
        {
            var lastChild = transform.GetChild(i);
            Object.Destroy(lastChild.gameObject);
        }
    }

    public static void SetChildrenActive(this Transform transform, bool isActive)
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    public static void RemoveObjectsByComponentInChildren<T>(this GameObject gameObject, bool includeInactive) where T: Component
    {
        var components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveObjectsByComponentInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        var components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component.gameObject);
        }
    }

    public static void RemoveComponents<T>(this GameObject gameObject) where T : Component
    {
        var components = gameObject.GetComponents<T>();
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        var components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        var components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static string GetUniqueId(int length = 8, string mask = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
    {
        char[] chars = mask.ToCharArray();
        RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
        var data = new byte[length];
        crypto.GetNonZeroBytes(data);
        StringBuilder result = new StringBuilder(length);
        foreach (byte b in data)
        {
            result.Append(chars[b % (chars.Length - 1)]);
        }
        return result.ToString();
    }

    public static string GetMD5(string text)
    {
        // byte array representation of that string
        byte[] encodedPassword = new UTF8Encoding().GetBytes(text);

        // need MD5 to calculate the hash
        byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

        // string representation (similar to UNIX format)
        return System.BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
    }
}
