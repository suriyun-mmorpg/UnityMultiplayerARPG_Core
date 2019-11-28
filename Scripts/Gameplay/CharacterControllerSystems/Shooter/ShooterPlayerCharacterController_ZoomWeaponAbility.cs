using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class ShooterPlayerCharacterController
    {
        [SerializeField]
        public Image zoomCrosshairImage;

        public void SetZoomCrosshairSprite(Sprite sprite)
        {
            if (zoomCrosshairImage != null)
                zoomCrosshairImage.sprite = sprite;
        }
        
        public void SetActiveZoomCrosshair(bool isActive)
        {
            if (zoomCrosshairImage != null &&
                zoomCrosshairImage.gameObject.activeSelf != isActive)
            {
                // Hide crosshair when not active
                zoomCrosshairImage.gameObject.SetActive(isActive);
            }
        }
    }
}
