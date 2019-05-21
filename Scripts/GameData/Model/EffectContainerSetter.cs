using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EffectContainerSetter : MonoBehaviour
    {
        public GameObject defaultModel;

        public void ApplyToCharacterModel(GameEntityModel gameEntityModel)
        {
            if (gameEntityModel == null)
            {
                Debug.LogWarning("[EffectContainerSetter] Cannot find game entity model");
                return;
            }
            bool hasChanges = false;
            bool isFound = false;
            List<EffectContainer> effectContainers = new List<EffectContainer>(gameEntityModel.effectContainers);
            for (int i = 0; i < effectContainers.Count; ++i)
            {
                EffectContainer effectContainer = effectContainers[i];
                if (effectContainer.transform == transform)
                {
                    isFound = true;
                    hasChanges = true;
                    effectContainer.effectSocket = name;
                    effectContainer.transform = transform;
                    effectContainers[i] = effectContainer;
                    break;
                }
            }
            if (!isFound)
            {
                hasChanges = true;
                EffectContainer newEffectContainer = new EffectContainer();
                newEffectContainer.effectSocket = name;
                newEffectContainer.transform = transform;
                effectContainers.Add(newEffectContainer);
            }
            if (hasChanges)
            {
                gameEntityModel.effectContainers = effectContainers.ToArray();
            }
        }
    }
}
