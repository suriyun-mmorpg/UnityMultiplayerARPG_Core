using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class EquipmentModelContainerSetter : MonoBehaviour
    {
        public GameObject defaultModel;

        private void OnValidate()
        {
#if UNITY_EDITOR
            BaseCharacterModel characterModel = GetComponentInParent<BaseCharacterModel>();
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
                if (equipmentContainer.transform == this &&
                    (!equipmentContainer.equipSocket.Equals(name) ||
                    equipmentContainer.defaultModel != defaultModel))
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
                EditorUtility.SetDirty(characterModel);
            }
#endif
        }
    }
}
