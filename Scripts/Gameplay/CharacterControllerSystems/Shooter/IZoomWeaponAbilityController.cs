using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public interface IZoomWeaponAbilityController : IWeaponAbilityController
    {
        void InitialZoomCrosshair();
        void SetZoomCrosshairSprite(Sprite sprite);
        bool ShowZoomCrosshair { get; set; }
    }
}
