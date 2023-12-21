using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class WorkbenchEntity : BuildingEntity
    {
        [Category(5, "Building Settings")]
        [SerializeField]
        [Tooltip("If this is `TRUE` this building entity cannot be attacked and is marked as InSafeArea and Immune")]
        protected bool cannotBeAttacked = false;
        [SerializeField]
        [Tooltip("Distance to activate this building")]
        protected float activateDistance = 5.0f;


        [Category(6, "Workbench Settings")]
        [SerializeField]
        protected ItemCraft[] itemCrafts = new ItemCraft[0];
        public ItemCraft[] ItemCrafts { get { return itemCrafts; } }


        private Dictionary<int, ItemCraft> _cacheItemCrafts;
        public Dictionary<int, ItemCraft> CacheItemCrafts
        {
            get
            {
                if (_cacheItemCrafts == null)
                {
                    _cacheItemCrafts = new Dictionary<int, ItemCraft>();
                    foreach (ItemCraft itemCraft in itemCrafts)
                    {
                        if (itemCraft.CraftingItem == null)
                            continue;
                        _cacheItemCrafts[itemCraft.CraftingItem.DataId] = itemCraft;
                    }
                }
                return _cacheItemCrafts;
            }
        }


        protected override void EntityAwake()
        {
            base.EntityAwake();
            if (cannotBeAttacked)
            {
                this.IsInSafeArea = true;
                this.IsImmune = true;
            }
        }

        public void CraftItem(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
            ItemCraft itemCraft;
            if (!CacheItemCrafts.TryGetValue(dataId, out itemCraft))
                return;

            UITextKeys gameMessage;
            if (!itemCraft.CanCraft(playerCharacterEntity, out gameMessage))
                GameInstance.ServerGameMessageHandlers.SendGameMessage(playerCharacterEntity.ConnectionId, gameMessage);
            else
                itemCraft.CraftItem(playerCharacterEntity);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            if (CacheItemCrafts.Count > 0)
            {
                List<BaseItem> items = new List<BaseItem>();
                foreach (ItemCraft itemCraft in CacheItemCrafts.Values)
                {
                    items.Add(itemCraft.CraftingItem);
                    GameInstance.AddItems(itemCraft.RequireItems);
                    GameInstance.AddCurrencies(itemCraft.RequireCurrencies);
                }
                GameInstance.AddItems(items);
            }
        }

        public override bool CanActivate()
        {
            return !this.IsDead();
        }

        public override void OnActivate()
        {
            BaseUISceneGameplay.Singleton.ShowWorkbenchDialog(this);
        }

        public override float GetActivatableDistance()
        {
            return activateDistance;
        }

    }
}
