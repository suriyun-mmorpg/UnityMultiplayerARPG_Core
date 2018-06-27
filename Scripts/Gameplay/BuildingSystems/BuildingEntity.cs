using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public sealed class BuildingEntity : DamageableNetworkEntity, IBuildingSaveData
    {
        [Header("Save Data")]
        [SerializeField]
        private SyncFieldString id = new SyncFieldString();
        [SerializeField]
        private SyncFieldString parentId = new SyncFieldString();
        [SerializeField]
        private SyncFieldInt dataId = new SyncFieldInt();
        [SerializeField]
        private SyncFieldString creatorId = new SyncFieldString();
        [SerializeField]
        private SyncFieldString creatorName = new SyncFieldString();
        private BuildingObject buildingObject;

        public string Id
        {
            get { return id; }
            set { id.Value = value; }
        }

        public string ParentId
        {
            get { return parentId; }
            set { parentId.Value = value; }
        }

        public int DataId
        {
            get { return dataId; }
            set { dataId.Value = value; }
        }

        public Vector3 Position
        {
            get { return CacheTransform.position; }
            set { CacheTransform.position = value; }
        }

        public Quaternion Rotation
        {
            get { return CacheTransform.rotation; }
            set { CacheTransform.rotation = value; }
        }

        public string CreatorId
        {
            get { return creatorId; }
            set { creatorId.Value = value; }
        }

        public string CreatorName
        {
            get { return creatorName; }
            set { creatorName.Value = value; }
        }

        public override string Title
        {
            get { return buildingObject == null ? "Unknow" : buildingObject.title; }
            set { }
        }

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.buildingTag;
            gameObject.layer = gameInstance.buildingLayer;
        }

        public override void OnSetup()
        {
            dataId.onChange += OnDataIdChange;
        }

        private void OnDestroy()
        {
            dataId.onChange -= OnDataIdChange;
        }

        private void OnDataIdChange(int dataId)
        {
            // Instantiate object
            BuildingObject buildingObjectPrefab;
            if (GameInstance.BuildingObjects.TryGetValue(dataId, out buildingObjectPrefab))
            {
                if (buildingObject != null)
                    Destroy(buildingObject.gameObject);
                buildingObject = Instantiate(buildingObjectPrefab);
                buildingObject.buildingEntity = this;
                buildingObject.CacheTransform.parent = CacheTransform;
                buildingObject.CacheTransform.localPosition = Vector3.zero;
                buildingObject.CacheTransform.localRotation = Quaternion.identity;
                buildingObject.CacheTransform.localScale = Vector3.one;
                buildingObject.gameObject.SetLayerRecursively(gameInstance.buildingLayer, true);
                combatTextTransform = buildingObject.CombatTextTransform;
            }
        }

        public override void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId)
        {
            // TODO: Reduce current hp

        }
    }
}
