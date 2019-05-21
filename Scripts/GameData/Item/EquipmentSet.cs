using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Set", menuName = "Create GameData/Equipment Set", order = -4897)]
    public class EquipmentSet : BaseGameData
    {
        [Header("Equipment Set Configs")]
        public EquipmentBonus[] effects;
    }
}
