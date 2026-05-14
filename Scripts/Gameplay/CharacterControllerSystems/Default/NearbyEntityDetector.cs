using LiteNetLibManager;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class NearbyEntityDetector : MonoBehaviour
    {
        public float detectingRadius;
        public int resultAllocSize = 128;
        public bool findPlayer;
        public bool findOnlyAlivePlayers;
        public bool findPlayerToAttack;
        public bool findMonster;
        public bool findOnlyAliveMonsters;
        public bool findMonsterToAttack;
        public bool findNpc;
        public bool findItemDrop;
        public bool findRewardDrop;
        public bool findBuilding;
        public bool findOnlyAliveBuildings;
        public bool findOnlyActivatableBuildings;
        public bool findVehicle;
        public bool findWarpPortal;
        public bool findItemsContainer;
        public bool findActivatableEntity;
        public bool findHoldActivatableEntity;
        public bool findPickupActivatableEntity;
        public readonly List<BaseCharacterEntity> characters = new List<BaseCharacterEntity>();
        public readonly List<BasePlayerCharacterEntity> players = new List<BasePlayerCharacterEntity>();
        public readonly List<BaseMonsterCharacterEntity> monsters = new List<BaseMonsterCharacterEntity>();
        public readonly List<NpcEntity> npcs = new List<NpcEntity>();
        public readonly List<ItemDropEntity> itemDrops = new List<ItemDropEntity>();
        public readonly List<BaseRewardDropEntity> rewardDrops = new List<BaseRewardDropEntity>();
        public readonly List<BuildingEntity> buildings = new List<BuildingEntity>();
        public readonly List<VehicleEntity> vehicles = new List<VehicleEntity>();
        public readonly List<WarpPortalEntity> warpPortals = new List<WarpPortalEntity>();
        public readonly List<ItemsContainerEntity> itemsContainers = new List<ItemsContainerEntity>();
        public readonly List<IActivatableEntity> activatableEntities = new List<IActivatableEntity>();
        public readonly List<IHoldActivatableEntity> holdActivatableEntities = new List<IHoldActivatableEntity>();
        public readonly List<IPickupActivatableEntity> pickupActivatableEntities = new List<IPickupActivatableEntity>();
        private readonly HashSet<Collider> _excludeColliders = new HashSet<Collider>();
        private readonly HashSet<Collider2D> _excludeCollider2Ds = new HashSet<Collider2D>();

        public System.Action onUpdateList;

        private void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
            NearbyEntityDetectorManager.Register(this);
        }

        private void OnDestroy()
        {
            ClearDetection();
            ClearExclusion();
            onUpdateList = null;
            NearbyEntityDetectorManager.Unregister(this);
        }

        public void ClearDetection()
        {
            characters.Nullify();
            characters?.Clear();
            players.Nullify();
            players?.Clear();
            monsters.Nullify();
            monsters?.Clear();
            npcs.Nullify();
            npcs?.Clear();
            itemDrops.Nullify();
            itemDrops?.Clear();
            rewardDrops.Nullify();
            rewardDrops?.Clear();
            buildings.Nullify();
            buildings?.Clear();
            vehicles.Nullify();
            vehicles?.Clear();
            warpPortals.Nullify();
            warpPortals?.Clear();
            itemsContainers.Nullify();
            itemsContainers?.Clear();
            activatableEntities?.Clear();
            holdActivatableEntities?.Clear();
            pickupActivatableEntities?.Clear();
        }


        public void ClearExclusion()
        {
            _excludeColliders.Clear();
            _excludeCollider2Ds.Clear();
        }

        internal void TriggerOnUpdateList()
        {
            onUpdateList?.Invoke();
        }

        internal void DetectEntities()
        {
            int tempHitCount;
            ClearDetection();
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension2D:
                    Collider2D[] collider2Ds = ArrayPool<Collider2D>.Shared.Rent(resultAllocSize);
                    ContactFilter2D contactFilter2D = new ContactFilter2D();
                    tempHitCount = Physics2D.OverlapCircle(GameInstance.PlayingCharacterEntity.EntityTransform.position, detectingRadius, contactFilter2D, collider2Ds);
                    for (int i = 0; i < tempHitCount; ++i)
                    {
                        Collider2D other = collider2Ds[i];
                        if (other == null || _excludeCollider2Ds.Contains(other))
                            continue;
                        AddEntity(other.gameObject);
                    }
                    ArrayPool<Collider2D>.Shared.Return(collider2Ds);
                    break;
                default:
                    Collider[] colliders = ArrayPool<Collider>.Shared.Rent(resultAllocSize);
                    tempHitCount = Physics.OverlapSphereNonAlloc(GameInstance.PlayingCharacterEntity.EntityTransform.position, detectingRadius, colliders);
                    for (int i = 0; i < tempHitCount; ++i)
                    {
                        Collider other = colliders[i];
                        if (other == null || _excludeColliders.Contains(other))
                            continue;
                        AddEntity(other.gameObject);
                    }
                    ArrayPool<Collider>.Shared.Return(colliders);
                    break;
            }
        }

        internal bool RemoveAllInactiveEntities()
        {
            bool hasChanges = false;
            hasChanges |= RemoveInactiveEntities(characters);
            hasChanges |= RemoveInactiveEntities(players);
            hasChanges |= RemoveInactiveEntities(monsters);
            hasChanges |= RemoveInactiveEntities(npcs);
            hasChanges |= RemoveInactiveEntities(itemDrops);
            hasChanges |= RemoveInactiveEntities(rewardDrops);
            hasChanges |= RemoveInactiveEntities(buildings);
            hasChanges |= RemoveInactiveEntities(vehicles);
            hasChanges |= RemoveInactiveEntities(warpPortals);
            hasChanges |= RemoveInactiveEntities(itemsContainers);
            hasChanges |= RemoveInactiveActivatableEntities(activatableEntities);
            hasChanges |= RemoveInactiveActivatableEntities(holdActivatableEntities);
            hasChanges |= RemoveInactiveActivatableEntities(pickupActivatableEntities);
            return hasChanges;
        }

        internal void SortAllEntities()
        {
            SortEntities(characters);
            SortEntities(players);
            SortEntities(monsters);
            SortEntities(npcs);
            SortEntities(itemDrops);
            SortEntities(rewardDrops);
            SortEntities(buildings);
            SortEntities(vehicles);
            SortEntities(warpPortals);
            SortEntities(itemsContainers);
            SortActivatableEntities(activatableEntities);
            SortActivatableEntities(holdActivatableEntities);
            SortActivatableEntities(pickupActivatableEntities);
        }

        public bool AddEntity(GameObject other)
        {
            BasePlayerCharacterEntity player;
            BaseMonsterCharacterEntity monster;
            NpcEntity npc;
            ItemDropEntity itemDrop;
            BaseRewardDropEntity rewardDrop;
            BuildingEntity building;
            VehicleEntity vehicle;
            WarpPortalEntity warpPortal;
            ItemsContainerEntity itemsContainer;
            IActivatableEntity activatableEntity;
            IHoldActivatableEntity holdActivatableEntity;
            IPickupActivatableEntity pickupActivatableEntity;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out rewardDrop, out building, out vehicle, out warpPortal, out itemsContainer, out activatableEntity, out holdActivatableEntity, out pickupActivatableEntity, true);

            bool foundSomething = false;
            if (player != null)
            {
                if (!characters.Contains(player))
                    characters.Add(player);
                if (!players.Contains(player))
                    players.Add(player);
                foundSomething = true;
            }
            if (monster != null)
            {
                if (!characters.Contains(monster))
                    characters.Add(monster);
                if (!monsters.Contains(monster))
                    monsters.Add(monster);
                foundSomething = true;
            }
            if (npc != null)
            {
                if (!npcs.Contains(npc))
                    npcs.Add(npc);
                foundSomething = true;
            }
            if (itemDrop != null)
            {
                if (!itemDrops.Contains(itemDrop))
                    itemDrops.Add(itemDrop);
                foundSomething = true;
            }
            if (rewardDrop != null)
            {
                if (!rewardDrops.Contains(rewardDrop))
                    rewardDrops.Add(rewardDrop);
                foundSomething = true;
            }
            if (building != null)
            {
                if (!buildings.Contains(building))
                    buildings.Add(building);
                foundSomething = true;
            }
            if (vehicle != null)
            {
                if (!vehicles.Contains(vehicle))
                    vehicles.Add(vehicle);
                foundSomething = true;
            }
            if (warpPortal != null)
            {
                if (!warpPortals.Contains(warpPortal))
                    warpPortals.Add(warpPortal);
                foundSomething = true;
            }
            if (itemsContainer != null)
            {
                if (!itemsContainers.Contains(itemsContainer))
                    itemsContainers.Add(itemsContainer);
                foundSomething = true;
            }
            if (!activatableEntity.IsNull())
            {
                if (!activatableEntities.Contains(activatableEntity))
                    activatableEntities.Add(activatableEntity);
                foundSomething = true;
            }
            if (!holdActivatableEntity.IsNull())
            {
                if (!holdActivatableEntities.Contains(holdActivatableEntity))
                    holdActivatableEntities.Add(holdActivatableEntity);
                foundSomething = true;
            }
            if (!pickupActivatableEntity.IsNull())
            {
                if (!pickupActivatableEntities.Contains(pickupActivatableEntity))
                    pickupActivatableEntities.Add(pickupActivatableEntity);
                foundSomething = true;
            }
            return foundSomething;
        }

        public bool RemoveEntity(GameObject other)
        {
            BasePlayerCharacterEntity player;
            BaseMonsterCharacterEntity monster;
            NpcEntity npc;
            ItemDropEntity itemDrop;
            BaseRewardDropEntity rewardDrop;
            BuildingEntity building;
            VehicleEntity vehicle;
            WarpPortalEntity warpPortal;
            ItemsContainerEntity itemsContainer;
            IActivatableEntity activatableEntity;
            IHoldActivatableEntity holdActivatableEntity;
            IPickupActivatableEntity pickupActivatableEntity;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out rewardDrop, out building, out vehicle, out warpPortal, out itemsContainer, out activatableEntity, out holdActivatableEntity, out pickupActivatableEntity, false);

            bool removeSomething = false;
            if (player != null)
                removeSomething = removeSomething || characters.Remove(player) && players.Remove(player);
            if (monster != null)
                removeSomething = removeSomething || characters.Remove(monster) && monsters.Remove(monster);
            if (npc != null)
                removeSomething = removeSomething || npcs.Remove(npc);
            if (itemDrop != null)
                removeSomething = removeSomething || itemDrops.Remove(itemDrop);
            if (rewardDrop != null)
                removeSomething = removeSomething || rewardDrops.Remove(rewardDrop);
            if (building != null)
                removeSomething = removeSomething || buildings.Remove(building);
            if (vehicle != null)
                removeSomething = removeSomething || vehicles.Remove(vehicle);
            if (warpPortal != null)
                removeSomething = removeSomething || warpPortals.Remove(warpPortal);
            if (itemsContainer != null)
                removeSomething = removeSomething || itemsContainers.Remove(itemsContainer);
            if (!activatableEntity.IsNull())
                removeSomething = removeSomething || activatableEntities.Remove(activatableEntity);
            if (!holdActivatableEntity.IsNull())
                removeSomething = removeSomething || holdActivatableEntities.Remove(holdActivatableEntity);
            if (!pickupActivatableEntity.IsNull())
                removeSomething = removeSomething || pickupActivatableEntities.Remove(pickupActivatableEntity);
            return removeSomething;
        }

        private void FindEntity(GameObject other,
            out BasePlayerCharacterEntity player,
            out BaseMonsterCharacterEntity monster,
            out NpcEntity npc,
            out ItemDropEntity itemDrop,
            out BaseRewardDropEntity rewardDrop,
            out BuildingEntity building,
            out VehicleEntity vehicle,
            out WarpPortalEntity warpPortal,
            out ItemsContainerEntity itemsContainer,
            out IActivatableEntity activatableEntity,
            out IHoldActivatableEntity holdActivatableEntity,
            out IPickupActivatableEntity pickupActivatableEntity,
            bool findWithAdvanceOptions)
        {
            player = null;
            monster = null;
            npc = null;
            itemDrop = null;
            rewardDrop = null;
            building = null;
            vehicle = null;
            warpPortal = null;
            itemsContainer = null;
            activatableEntity = null;
            holdActivatableEntity = null;
            pickupActivatableEntity = null;

            IGameEntity gameEntity = other.GetComponent<IGameEntity>();
            if (!gameEntity.IsNull())
            {
                if (findPlayer)
                {
                    player = gameEntity.Entity as BasePlayerCharacterEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && player != null && player.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        player = null;
                    if (player == GameInstance.PlayingCharacterEntity)
                        player = null;
                    if (findWithAdvanceOptions)
                    {
                        if (findOnlyAlivePlayers && player != null && player.IsDead())
                            player = null;
                        if (findPlayerToAttack && player != null && !player.CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo()))
                            player = null;
                    }
                }

                if (findMonster)
                {
                    monster = gameEntity.Entity as BaseMonsterCharacterEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && monster != null && monster.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        monster = null;
                    if (findWithAdvanceOptions)
                    {
                        if (findOnlyAliveMonsters && monster != null && monster.IsDead())
                            monster = null;
                        if (findMonsterToAttack && monster != null && !monster.CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo()))
                            monster = null;
                    }
                }

                if (findNpc)
                {
                    npc = gameEntity.Entity as NpcEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && npc != null && npc.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        npc = null;
                }

                if (findItemDrop)
                {
                    itemDrop = gameEntity.Entity as ItemDropEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && itemDrop != null && itemDrop.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        itemDrop = null;
                }

                if (findRewardDrop)
                {
                    rewardDrop = gameEntity.Entity as BaseRewardDropEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && rewardDrop != null && rewardDrop.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        rewardDrop = null;
                }

                if (findBuilding)
                {
                    building = gameEntity.Entity as BuildingEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && building != null && building.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        building = null;
                    if (findWithAdvanceOptions)
                    {
                        if (findOnlyAliveBuildings && building != null && building.IsDead())
                            building = null;
                        if (findOnlyActivatableBuildings && building != null && !building.CanActivate())
                            building = null;
                    }
                }

                if (findVehicle)
                {
                    vehicle = gameEntity.Entity as VehicleEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && vehicle != null && vehicle.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        vehicle = null;
                }

                if (findWarpPortal)
                {
                    warpPortal = gameEntity.Entity as WarpPortalEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && warpPortal != null && warpPortal.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        warpPortal = null;
                }

                if (findItemsContainer)
                {
                    itemsContainer = gameEntity.Entity as ItemsContainerEntity;
                    if (GameInstance.PlayingCharacterEntity.IsServer && itemsContainer != null && itemsContainer.Identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        itemsContainer = null;
                }
            }

            if (findActivatableEntity)
            {
                activatableEntity = other.GetComponent<IActivatableEntity>();
                if (!activatableEntity.IsNull())
                {
                    if (activatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                        activatableEntity = null;
                    if (GameInstance.PlayingCharacterEntity.IsServer && !activatableEntity.IsNull() && activatableEntity.EntityGameObject.TryGetComponent(out LiteNetLibIdentity identity) && identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        activatableEntity = null;
                }
            }

            if (findHoldActivatableEntity)
            {
                holdActivatableEntity = other.GetComponent<IHoldActivatableEntity>();
                if (!holdActivatableEntity.IsNull())
                {
                    if (holdActivatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                        holdActivatableEntity = null;
                    if (GameInstance.PlayingCharacterEntity.IsServer && !holdActivatableEntity.IsNull() && holdActivatableEntity.EntityGameObject.TryGetComponent(out LiteNetLibIdentity identity) && identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        holdActivatableEntity = null;
                }
            }

            if (findPickupActivatableEntity)
            {
                pickupActivatableEntity = other.GetComponent<IPickupActivatableEntity>();
                if (!pickupActivatableEntity.IsNull())
                {
                    if (pickupActivatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                        pickupActivatableEntity = null;
                    if (GameInstance.PlayingCharacterEntity.IsServer && !pickupActivatableEntity.IsNull() && pickupActivatableEntity.EntityGameObject.TryGetComponent(out LiteNetLibIdentity identity) && identity.IsHideFrom(GameInstance.PlayingCharacterEntity.Identity))
                        pickupActivatableEntity = null;
                }
            }
        }


        private bool RemoveInactiveEntities<T>(List<T> entities) where T : BaseGameEntity
        {
            bool hasUpdate = false;
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                if (entities[i] == null || !entities[i].gameObject.activeInHierarchy)
                {
                    entities.RemoveAt(i);
                    hasUpdate = true;
                }
            }
            return hasUpdate;
        }

        private bool RemoveInactiveActivatableEntities<T>(List<T> entities) where T : IBaseActivatableEntity
        {
            bool hasUpdate = false;
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                if (entities[i] == null || (entities[i] is Object unityObj && unityObj == null) ||
                    !entities[i].EntityGameObject.activeInHierarchy)
                {
                    entities.RemoveAt(i);
                    hasUpdate = true;
                }
            }
            return hasUpdate;
        }

        private void SortEntities<T>(List<T> entities) where T : BaseGameEntity
        {
            if (entities == null || entities.Count == 0) return;
            Vector3 pos = transform.position;
            entities.Sort((a, b) => (a.transform.position - pos).sqrMagnitude.CompareTo((b.transform.position - pos).sqrMagnitude));
        }

        private void SortActivatableEntities<T>(List<T> entities) where T : IBaseActivatableEntity
        {
            if (entities == null || entities.Count == 0) return;
            Vector3 pos = transform.position;
            entities.Sort((a, b) => (a.EntityTransform.position - pos).sqrMagnitude.CompareTo((b.EntityTransform.position - pos).sqrMagnitude));
        }
    }
}
