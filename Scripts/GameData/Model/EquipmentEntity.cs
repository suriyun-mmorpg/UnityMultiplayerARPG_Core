using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EquipmentEntity : BaseEquipmentEntity
    {
        public List<EquipmentEntityEffect> effects = new List<EquipmentEntityEffect>();
        private Renderer equipmentRenderer;
        private Material[] defaultMaterials;
        private List<GameObject> allEffectObjects = new List<GameObject>();
        private EquipmentEntityEffect usingEffect;

        private void Awake()
        {
            equipmentRenderer = GetComponent<Renderer>();
            if (equipmentRenderer != null)
                defaultMaterials = equipmentRenderer.materials;
            
            effects.Sort();

            if (effects != null && effects.Count > 0)
            {
                foreach (EquipmentEntityEffect effect in effects)
                {
                    if (effect.effectObjects != null && effect.effectObjects.Length > 0)
                    {
                        foreach (GameObject effectObject in effect.effectObjects)
                        {
                            effectObject.SetActive(false);
                            allEffectObjects.Add(effectObject);
                        }
                    }
                }
            }
        }

        public override void OnLevelChanged(int level)
        {
            if (allEffectObjects != null && allEffectObjects.Count > 0)
            {
                foreach (GameObject allEffectObject in allEffectObjects)
                {
                    if (allEffectObject.activeSelf)
                        allEffectObject.SetActive(false);
                }
            }

            if (effects != null && effects.Count > 0)
            {
                foreach (EquipmentEntityEffect effect in effects)
                {
                    if (level >= effect.level)
                        usingEffect = effect;
                    else
                        break;
                }
                // Apply materials
                if (equipmentRenderer != null)
                    equipmentRenderer.materials = usingEffect.materials;
                // Activate effect objects
                if (usingEffect.effectObjects != null && usingEffect.effectObjects.Length > 0)
                {
                    foreach (GameObject effectObject in usingEffect.effectObjects)
                    {
                        effectObject.SetActive(true);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct EquipmentEntityEffect : IComparer<EquipmentEntityEffect>
    {
        public int level;
        public Material[] materials;
        public GameObject[] effectObjects;

        public int Compare(EquipmentEntityEffect x, EquipmentEntityEffect y)
        {
            return x.level.CompareTo(y.level);
        }
    }
}
