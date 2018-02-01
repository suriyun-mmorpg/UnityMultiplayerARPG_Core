using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericExtension
{
    public static void RemoveChildren(this Transform transform)
    {
        while (transform.childCount > 0)
        {
            var lastChild = transform.GetChild(transform.childCount - 1);
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
}
