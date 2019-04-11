using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Equipment Set", menuName = "Create GameData/Equipment Set")]
    public class EquipmentSet : BaseGameData
    {
        public EquipmentBonus[] effects;
    }
}
