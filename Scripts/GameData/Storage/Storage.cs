using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Storage", menuName = "Create GameData/Storage")]
    public class Storage : BaseGameData
    {
        public float weightLimit;
        [Tooltip("If this is 0 it will have no slot limit")]
        public int slotLimit;
    }
}
