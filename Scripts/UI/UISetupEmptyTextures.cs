using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetupEmptyTextures : MonoBehaviour {
    public Sprite sprite;

    [ContextMenu("SetupEmptyTextures")]
    public void SetupEmptyTextures()
    {
        if (sprite != null)
        {
            var images = GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image.sprite == null)
                    image.sprite = sprite;
            }
        }
    }
}
