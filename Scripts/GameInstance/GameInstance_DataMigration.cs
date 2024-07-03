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
            if (levelUpEffect != null)
            {
                if (levelUpEffects == null || levelUpEffects.Length == 0)
                    levelUpEffects = new List<GameEffect>() { levelUpEffect }.ToArray();
                levelUpEffect = null;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }

        public static async void MigrateEquipmentEntities(IEnumerable<EquipmentModel> equipmentModels)
        {
            if (equipmentModels == null)
                return;
            List<GameObject> modelObjects = new List<GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
            {
                GameObject meshPrefab = await equipmentModel.GetMeshPrefab();
                if (meshPrefab == null)
                    continue;
                modelObjects.Add(meshPrefab);
            }
            List<EquipmentEntity> equipmentEntities = modelObjects.GetComponents<EquipmentEntity>();
            foreach (EquipmentEntity equipmentEntity in equipmentEntities)
            {
                equipmentEntity.MigrateMaterials();
            }
        }
    }
}
