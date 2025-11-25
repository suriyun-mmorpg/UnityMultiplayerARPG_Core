using Insthync.AddressableAssetTools;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        private void OnValidate()
        {
            MigrateLevelUpEffect();
        }

        private void MigrateLevelUpEffect()
        {
#if !EXCLUDE_PREFAB_REFS
            if (levelUpEffect != null)
            {
                if (levelUpEffects == null || levelUpEffects.Length == 0)
                    levelUpEffects = new List<GameEffect>() { levelUpEffect }.ToArray();
                levelUpEffect = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
#endif
        }

        public static void MigrateEquipmentEntities(IEnumerable<EquipmentModel> equipmentModels)
        {
#if UNITY_EDITOR
            if (equipmentModels == null)
                return;
            List<GameObject> modelObjects = new List<GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
            {
                GameObject meshPrefab;
                EquipmentEntity entity;

                meshPrefab = equipmentModel.MeshPrefab;
                if (meshPrefab != null && meshPrefab.TryGetComponent(out entity))
                    entity.MigrateMaterials();

                try
                {
                    meshPrefab = equipmentModel.AddressableMeshPrefab.LoadObject<GameObject>();
                    if (meshPrefab != null && meshPrefab.TryGetComponent(out entity))
                        entity.MigrateMaterials();
                }
                catch (System.Exception ex)
                {
                    Debug.LogException(ex);
                }
                finally
                {
                    equipmentModel.AddressableMeshPrefab.Release();
                }
            }
#endif
        }
    }
}
