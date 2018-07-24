using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [System.Obsolete("`Building Object` is deprecated and will be removed later, setup `Building Entity` instead")]
    public class BuildingObject : MonoBehaviour
    {
        [Header("Generice data")]
        public string title;
        [Header("Building Data")]
        [Tooltip("Type of building you can set it as Foundation, Wall, Door anything as you wish")]
        public string buildingType;
        public float characterForwardDistance = 4;
        public int maxHp = 100;
        public Transform combatTextTransform;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            Migrate();
        }

        public BuildingEntity Migrate()
        {
            var identity = GetComponent<LiteNetLibManager.LiteNetLibIdentity>();
            if (identity == null)
                identity = gameObject.AddComponent<LiteNetLibManager.LiteNetLibIdentity>();
            var buildingEntity = GetComponent<BuildingEntity>();
            if (buildingEntity == null)
                buildingEntity = gameObject.AddComponent<BuildingEntity>();
            buildingEntity.Title = title;
            buildingEntity.buildingType = buildingType;
            buildingEntity.characterForwardDistance = characterForwardDistance;
            buildingEntity.maxHp = maxHp;
            buildingEntity.combatTextTransform = combatTextTransform;
            Destroy(this);
            EditorUtility.SetDirty(gameObject);
            return buildingEntity;
        }
#endif
    }
}
