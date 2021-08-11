using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "New Character Setting", menuName = "Create GameData/New Character Setting", order = -4696)]
    public partial class NewCharacterSetting : ScriptableObject
    {
        [Header("New Character Configs")]
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems;
    }
}
