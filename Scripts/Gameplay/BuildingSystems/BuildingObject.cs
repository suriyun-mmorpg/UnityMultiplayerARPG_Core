using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Obsolete("`Building Object` is deprecated and will be removed later, setup `Building Entity` instead")]
    public class BuildingObject : MonoBehaviour
    {
        [Header("Generice data")]
        public string title;
        [Header("Building Data")]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
        public string buildingType;
        public float characterForwardDistance = 4;
        public int maxHp = 100;
        public Transform combatTextTransform;
    }
}
