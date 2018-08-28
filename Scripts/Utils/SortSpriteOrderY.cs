using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SortSpriteOrderY : MonoBehaviour
{
    private SpriteRenderer cacheSpriteRenderer;
    public SpriteRenderer CacheSpriteRenderer
    {
        get
        {
            if (cacheSpriteRenderer == null)
                cacheSpriteRenderer = GetComponent<SpriteRenderer>();
            return cacheSpriteRenderer;
        }
    }

    void Update()
    {
        CacheSpriteRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}
