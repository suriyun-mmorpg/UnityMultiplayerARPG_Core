using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "New Character Setting", menuName = "Create GameData/New Character Setting", order = -4697)]
    public partial class NewCharacterSetting : ScriptableObject
    {
        [Header("New Character Configs")]
        public int startGold = 0;
        public ItemAmount[] startItems;
    }
}
