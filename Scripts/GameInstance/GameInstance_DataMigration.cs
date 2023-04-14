using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        public static void MigrateEquipmentEntities(IEnumerable<EquipmentModel> equipmentModels)
        {
            if (equipmentModels == null)
                return;
            List<GameObject> modelObjects = new List<GameObject>();
            foreach (EquipmentModel equipmentModel in equipmentModels)
            {
                if (equipmentModel.meshPrefab == null)
                    continue;
                modelObjects.Add(equipmentModel.meshPrefab);
            }
            List<EquipmentEntity> equipmentEntities = modelObjects.GetComponents<EquipmentEntity>();
            foreach (EquipmentEntity equipmentEntity in equipmentEntities)
            {
                equipmentEntity.MigrateMaterials();
            }
        }
    }
}
