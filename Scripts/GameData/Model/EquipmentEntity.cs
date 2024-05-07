using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class EquipmentEntity : BaseEquipmentEntity
    {
        [Header("Refine Effects")]
        public MaterialCollection[] defaultMaterials;
        public List<EquipmentEntityEffect> effects = new List<EquipmentEntityEffect>();

        private List<GameObject> _allEffectObjects = new List<GameObject>();

        private void Awake()
        {
            if (effects != null && effects.Count > 0)
            {
                effects.Sort();
                foreach (EquipmentEntityEffect effect in effects)
                {
                    if (effect.effectObjects != null && effect.effectObjects.Length > 0)
                    {
                        foreach (GameObject effectObject in effect.effectObjects)
                        {
                            effectObject.SetActive(false);
                            _allEffectObjects.Add(effectObject);
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (MigrateMaterials())
                EditorUtility.SetDirty(this);
        }
#endif

        [ContextMenu("Migrate Materials")]
        public bool MigrateMaterials()
        {
            bool hasChanges = false;
            Renderer equipmentRenderer = GetComponent<Renderer>();
            if (defaultMaterials == null || defaultMaterials.Length == 0)
            {
                if (equipmentRenderer)
                {
                    defaultMaterials = new MaterialCollection[1]
                    {
                        new MaterialCollection()
                        {
                            renderer = equipmentRenderer,
                            materials = equipmentRenderer.sharedMaterials
                        }
                    };
                    hasChanges = true;
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            if (effects != null && effects.Count > 0)
            {
                EquipmentEntityEffect tempEffect;
                for (int i = 0; i < effects.Count; ++i)
                {
                    tempEffect = effects[i];
                    if (tempEffect.materials != null && tempEffect.materials.Length > 0 && (tempEffect.equipmentMaterials == null || tempEffect.equipmentMaterials.Length == 0))
                    {
                        MaterialCollection[] materials = new MaterialCollection[1]
                        {
                            new MaterialCollection()
                            {
                                renderer = equipmentRenderer,
                                materials = tempEffect.materials,
                            }
                        };
                        tempEffect.equipmentMaterials = materials;
                        effects[i] = tempEffect;
                        hasChanges = true;
                    }
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
        }

        public override void OnItemChanged(CharacterItem item)
        {
            int level = item.level;
            if (_allEffectObjects != null && _allEffectObjects.Count > 0)
            {
                foreach (GameObject allEffectObject in _allEffectObjects)
                {
                    if (allEffectObject.activeSelf)
                        allEffectObject.SetActive(false);
                }
            }

            bool isFoundEffect = false;
            EquipmentEntityEffect usingEffect = default;
            if (effects != null && effects.Count > 0)
            {
                foreach (EquipmentEntityEffect effect in effects)
                {
                    if (level >= effect.level)
                    {
                        isFoundEffect = true;
                        usingEffect = effect;
                    }
                    else
                        break;
                }
                if (isFoundEffect)
                {
                    // Apply materials
                    usingEffect.equipmentMaterials.ApplyMaterials();
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
            // Not found effect apply default materials
            if (!isFoundEffect)
            {
                defaultMaterials.ApplyMaterials();
            }
        }
    }
}
