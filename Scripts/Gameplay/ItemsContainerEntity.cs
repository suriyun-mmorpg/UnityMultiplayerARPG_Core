using System.Collections.Generic;
using Cysharp.Text;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public class ItemsContainerEntity : BaseGameEntity, IActivatableEntity
    {
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;

        [Category(5, "Items Container Settings")]
        [Tooltip("Delay before the entity destroyed, you may set some delay to play destroyed animation by `onItemDropDestroy` event before it's going to be destroyed from the game.")]
        [SerializeField]
        protected float destroyDelay = 0f;
        [SerializeField]
        [Tooltip("Format => {0} = {Title}")]
        protected UILocaleKeySetting formatKeyCorpseTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CORPSE_TITLE);

        [Category("Events")]
        [SerializeField]
        protected UnityEvent onItemsContainerDestroy;

        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[1000];

        protected SyncFieldString dropperTitle = new SyncFieldString();
        public SyncFieldString DropperTitle
        {
            get { return dropperTitle; }
        }
        protected SyncFieldInt dropperEntityId = new SyncFieldInt();
        public SyncFieldInt DropperEntityId
        {
            get { return dropperEntityId; }
        }
        protected SyncListCharacterItem items = new SyncListCharacterItem();
        public SyncListCharacterItem Items
        {
            get { return items; }
        }
        public RewardGivenType GivenType { get; protected set; }
        public HashSet<string> Looters { get; protected set; }
        public override string EntityTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(dropperTitle.Value))
                {
                    return ZString.Format(LanguageManager.GetText(formatKeyCorpseTitle), DropperTitle.Value);
                }
                if (GameInstance.MonsterCharacterEntities.ContainsKey(dropperEntityId.Value))
                {
                    return ZString.Format(LanguageManager.GetText(formatKeyCorpseTitle), GameInstance.MonsterCharacterEntities[dropperEntityId.Value].EntityTitle);
                }
                return base.EntityTitle;
            }
        }

        // Private variables
        protected bool isDestroyed;
        protected float dropTime;
        protected float appearDuration;

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            dropperTitle.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            dropperEntityId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            items.forOwnerOnly = false;
        }

        public override void OnSetup()
        {
            base.OnSetup();
            NetworkDestroy(appearDuration);
        }

        [AllRpc]
        protected virtual void AllOnItemsContainerDestroy()
        {
            if (onItemsContainerDestroy != null)
                onItemsContainerDestroy.Invoke();
        }

        public void CallAllOnItemDropDestroy()
        {
            RPC(AllOnItemsContainerDestroy);
        }

        public virtual bool IsAbleToLoot(BaseCharacterEntity baseCharacterEntity)
        {
            if ((Looters == null || Looters.Count == 0 || Looters.Contains(baseCharacterEntity.Id) ||
                Time.unscaledTime - dropTime > CurrentGameInstance.itemLootLockDuration) && !isDestroyed)
                return true;
            return false;
        }

        /// <summary>
        /// This function should be called by server only when picked up some (or all) items from this container
        /// </summary>
        public virtual void PickedUp()
        {
            if (!IsServer)
                return;
            if (Items.Count > 0)
                return;
            if (isDestroyed)
                return;
            // Mark as destroyed
            isDestroyed = true;
            // Tell clients that the item drop destroy to play animation at client
            CallAllOnItemDropDestroy();
            // Destroy this entity
            NetworkDestroy(destroyDelay);
        }

        public static ItemsContainerEntity DropItems(ItemsContainerEntity prefab, BaseGameEntity dropper, RewardGivenType givenType, IEnumerable<CharacterItem> dropItems, IEnumerable<string> looters, float appearDuration, bool randomPosition = false, bool randomRotation = false)
        {
            Vector3 dropPosition = dropper.EntityTransform.position;
            Quaternion dropRotation = dropper.EntityTransform.rotation;
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    if (randomPosition)
                    {
                        // Random position around dropper with its height
                        dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, GROUND_DETECTION_Y_OFFSETS, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    }
                    if (randomRotation)
                    {
                        // Random rotation
                        dropRotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
                    }
                    break;
                case DimensionType.Dimension2D:
                    if (randomPosition)
                    {
                        // Random position around dropper
                        dropPosition += new Vector3(Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance, Random.Range(-1f, 1f) * GameInstance.Singleton.dropDistance);
                    }
                    break;
            }
            return DropItems(prefab, dropper, dropPosition, dropRotation, givenType, dropItems, looters, appearDuration);
        }

        public static ItemsContainerEntity DropItems(ItemsContainerEntity prefab, BaseGameEntity dropper, Vector3 dropPosition, Quaternion dropRotation, RewardGivenType givenType, IEnumerable<CharacterItem> dropItems, IEnumerable<string> looters, float appearDuration)
        {
            if (prefab == null)
                return null;

            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                // Find drop position on ground
                dropPosition = PhysicUtils.FindGroundedPosition(dropPosition, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GameInstance.Singleton.GetItemDropGroundDetectionLayerMask());
            }
            LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                prefab.Identity.HashAssetId,
                dropPosition, dropRotation);
            ItemsContainerEntity itemsContainerEntity = spawnObj.GetComponent<ItemsContainerEntity>();
            itemsContainerEntity.Items.AddRange(dropItems);
            itemsContainerEntity.GivenType = givenType;
            itemsContainerEntity.Looters = new HashSet<string>(looters);
            itemsContainerEntity.isDestroyed = false;
            itemsContainerEntity.dropTime = Time.unscaledTime;
            itemsContainerEntity.appearDuration = appearDuration;
            if (dropper != null)
            {
                if (!string.IsNullOrEmpty(dropper.SyncTitle))
                    itemsContainerEntity.DropperTitle.Value = dropper.SyncTitle;
                else
                    itemsContainerEntity.DropperEntityId.Value = dropper.EntityId;
            }
            BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            return itemsContainerEntity;
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.pickUpItemDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return false;
        }

        public virtual bool ShouldBeAttackTarget()
        {
            return false;
        }

        public virtual bool ShouldNotActivateAfterFollowed()
        {
            return false;
        }

        public virtual bool CanActivate()
        {
            return true;
        }

        public virtual void OnActivate()
        {
            BaseUISceneGameplay.Singleton.ShowItemsContainerDialog(this);
        }
    }
}
