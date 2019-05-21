using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EquipmentContainerSetter : MonoBehaviour
    {
        public GameObject defaultModel;

        public void ApplyToCharacterModel(BaseCharacterModel characterModel)
        {
            if (characterModel == null)
            {
                Debug.LogWarning("[EquipmentModelContainerSetter] Cannot find character model");
                return;
            }
            bool hasChanges = false;
            bool isFound = false;
            List<EquipmentContainer> equipmentContainers = new List<EquipmentContainer>(characterModel.equipmentContainers);
            for (int i = 0; i < equipmentContainers.Count; ++i)
            {
                EquipmentContainer equipmentContainer = equipmentContainers[i];
                if (equipmentContainer.transform == transform)
                {
                    isFound = true;
                    hasChanges = true;
                    equipmentContainer.equipSocket = name;
                    equipmentContainer.defaultModel = defaultModel;
                    equipmentContainer.transform = transform;
                    equipmentContainers[i] = equipmentContainer;
                    break;
                }
            }
            if (!isFound)
            {
                hasChanges = true;
                EquipmentContainer newEquipmentContainer = new EquipmentContainer();
                newEquipmentContainer.equipSocket = name;
                newEquipmentContainer.defaultModel = defaultModel;
                newEquipmentContainer.transform = transform;
                equipmentContainers.Add(newEquipmentContainer);
            }
            if (hasChanges)
            {
                characterModel.equipmentContainers = equipmentContainers.ToArray();
            }
        }
    }
}
