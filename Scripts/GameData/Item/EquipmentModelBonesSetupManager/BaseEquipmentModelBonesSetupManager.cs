using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseEquipmentModelBonesSetupManager : ScriptableObject
    {
        public abstract void Setup(BaseCharacterModel characterModel, EquipmentModel data, GameObject newModel, EquipmentContainer equipmentContainer);
    }
}