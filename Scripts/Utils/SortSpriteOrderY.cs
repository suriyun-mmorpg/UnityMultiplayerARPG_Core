using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortSpriteOrderY : MonoBehaviour
{
    private Renderer cacheRenderer;
    public Renderer CacheRenderer
    {
        get
        {
            if (cacheRenderer == null)
                cacheRenderer = GetComponent<Renderer>();
            return cacheRenderer;
        }
    }

    void Update()
    {
        CacheRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}
