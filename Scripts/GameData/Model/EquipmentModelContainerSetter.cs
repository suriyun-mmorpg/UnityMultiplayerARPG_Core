using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EquipmentModelContainerSetter : MonoBehaviour
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
            List<EquipmentModelContainer> equipmentContainers = new List<EquipmentModelContainer>(characterModel.equipmentContainers);
            for (int i = 0; i < equipmentContainers.Count; ++i)
            {
                EquipmentModelContainer equipmentContainer = equipmentContainers[i];
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
                EquipmentModelContainer newEquipmentContainer = new EquipmentModelContainer();
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
