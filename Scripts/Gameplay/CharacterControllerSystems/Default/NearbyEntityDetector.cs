using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class NearbyEntityDetector : MonoBehaviour
    {
        public Transform CacheTransform { get; private set; }

        public float detectingRadius;
        public bool findPlayer;
        public bool findOnlyAlivePlayers;
        public bool findPlayerToAttack;
        public bool findMonster;
        public bool findOnlyAliveMonsters;
        public bool findMonsterToAttack;
        public bool findNpc;
        public bool findItemDrop;
        public bool findBuilding;
        public bool findOnlyAliveBuildings;
        public bool findOnlyActivatableBuildings;
        public bool findVehicle;
        public bool findWarpPortal;
        public bool findItemsContainer;
        public readonly List<BaseCharacterEntity> characters = new List<BaseCharacterEntity>();
        public readonly List<BasePlayerCharacterEntity> players = new List<BasePlayerCharacterEntity>();
        public readonly List<BaseMonsterCharacterEntity> monsters = new List<BaseMonsterCharacterEntity>();
        public readonly List<NpcEntity> npcs = new List<NpcEntity>();
        public readonly List<ItemDropEntity> itemDrops = new List<ItemDropEntity>();
        public readonly List<BuildingEntity> buildings = new List<BuildingEntity>();
        public readonly List<VehicleEntity> vehicles = new List<VehicleEntity>();
        public readonly List<WarpPortalEntity> warpPortals = new List<WarpPortalEntity>();
        public readonly List<ItemsContainerEntity> itemsContainers = new List<ItemsContainerEntity>();
        private readonly HashSet<Collider> excludeColliders = new HashSet<Collider>();
        private readonly HashSet<Collider2D> excludeCollider2Ds = new HashSet<Collider2D>();
        private SphereCollider cacheCollider;
        private CircleCollider2D cacheCollider2D;

        public System.Action onUpdateList;

        private void Awake()
        {
            CacheTransform = transform;
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        private void Start()
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
            {
                cacheCollider = gameObject.AddComponent<SphereCollider>();
                cacheCollider.radius = detectingRadius;
                cacheCollider.isTrigger = true;
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                cacheCollider2D = gameObject.AddComponent<CircleCollider2D>();
                cacheCollider2D.radius = detectingRadius;
                cacheCollider2D.isTrigger = true;
                Rigidbody2D rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
                rigidbody2D.isKinematic = true;
                rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }

        private void Update()
        {
            if (GameInstance.PlayingCharacterEntity == null)
                return;

            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
                cacheCollider.radius = detectingRadius;
            else
                cacheCollider2D.radius = detectingRadius;

            CacheTransform.position = GameInstance.PlayingCharacterEntity.CacheTransform.position;
            // Find nearby entities
            RemoveInactiveAndSortNearestEntity(characters);
            RemoveInactiveAndSortNearestEntity(players);
            RemoveInactiveAndSortNearestEntity(monsters);
            RemoveInactiveAndSortNearestEntity(npcs);
            RemoveInactiveAndSortNearestEntity(itemDrops);
            RemoveInactiveAndSortNearestEntity(buildings);
            RemoveInactiveAndSortNearestEntity(vehicles);
            RemoveInactiveAndSortNearestEntity(warpPortals);
            RemoveInactiveAndSortNearestEntity(itemsContainers);
        }

        public void ClearExcludeColliders()
        {
            excludeColliders.Clear();
            excludeCollider2Ds.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (excludeColliders.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
            {
                excludeColliders.Add(other);
                return;
            }
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (excludeColliders.Contains(other))
                return;
            if (!RemoveEntity(other.gameObject))
                return;
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (excludeCollider2Ds.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
            {
                excludeCollider2Ds.Add(other);
                return;
            }
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (excludeCollider2Ds.Contains(other))
                return;
            if (!RemoveEntity(other.gameObject))
                return;
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private bool AddEntity(GameObject other)
        {
            BasePlayerCharacterEntity player;
            BaseMonsterCharacterEntity monster;
            NpcEntity npc;
            ItemDropEntity itemDrop;
            BuildingEntity building;
            VehicleEntity vehicle;
            WarpPortalEntity warpPortal;
            ItemsContainerEntity itemsContainer;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal, out itemsContainer);

            if (player != null)
            {
                if (!characters.Contains(player))
                    characters.Add(player);
                if (!players.Contains(player))
                    players.Add(player);
                return true;
            }
            if (monster != null)
            {
                if (!characters.Contains(monster))
                    characters.Add(monster);
                if (!monsters.Contains(monster))
                    monsters.Add(monster);
                return true;
            }
            if (npc != null)
            {
                if (!npcs.Contains(npc))
                    npcs.Add(npc);
                return true;
            }
            if (itemDrop != null)
            {
                if (!itemDrops.Contains(itemDrop))
                    itemDrops.Add(itemDrop);
                return true;
            }
            if (building != null)
            {
                if (!buildings.Contains(building))
                    buildings.Add(building);
                return true;
            }
            if (vehicle != null)
            {
                if (!vehicles.Contains(vehicle))
                    vehicles.Add(vehicle);
                return true;
            }
            if (warpPortal != null)
            {
                if (!warpPortals.Contains(warpPortal))
                    warpPortals.Add(warpPortal);
                return true;
            }
            if (itemsContainer != null)
            {
                if (!itemsContainers.Contains(itemsContainer))
                    itemsContainers.Add(itemsContainer);
                return true;
            }
            return false;
        }

        private bool RemoveEntity(GameObject other)
        {
            BasePlayerCharacterEntity player;
            BaseMonsterCharacterEntity monster;
            NpcEntity npc;
            ItemDropEntity itemDrop;
            BuildingEntity building;
            VehicleEntity vehicle;
            WarpPortalEntity warpPortal;
            ItemsContainerEntity itemsContainer;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal, out itemsContainer, false);

            if (player != null)
                return characters.Remove(player) && players.Remove(player);
            if (monster != null)
                return characters.Remove(monster) && monsters.Remove(monster);
            if (npc != null)
                return npcs.Remove(npc);
            if (itemDrop != null)
                return itemDrops.Remove(itemDrop);
            if (building != null)
                return buildings.Remove(building);
            if (vehicle != null)
                return vehicles.Remove(vehicle);
            if (warpPortal != null)
                return warpPortals.Remove(warpPortal);
            if (itemsContainer != null)
                return itemsContainers.Remove(itemsContainer);
            return false;
        }

        private void FindEntity(GameObject other,
            out BasePlayerCharacterEntity player,
            out BaseMonsterCharacterEntity monster,
            out NpcEntity npc,
            out ItemDropEntity itemDrop,
            out BuildingEntity building,
            out VehicleEntity vehicle,
            out WarpPortalEntity warpPortal,
            out ItemsContainerEntity itemsContainer,
            bool findWithAdvanceOptions = true)
        {
            player = null;
            monster = null;
            npc = null;
            itemDrop = null;
            building = null;
            vehicle = null;
            warpPortal = null;
            itemsContainer = null;

            IGameEntity gameEntity = other.GetComponent<IGameEntity>();
            if (gameEntity == null)
                return;

            if (findPlayer)
            {
                player = gameEntity.Entity as BasePlayerCharacterEntity;
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
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAliveMonsters && monster != null && monster.IsDead())
                        monster = null;
                    if (findMonsterToAttack && monster != null && !monster.CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo()))
                        monster = null;
                }
            }

            if (findNpc)
                npc = gameEntity.Entity as NpcEntity;

            if (findItemDrop)
                itemDrop = gameEntity.Entity as ItemDropEntity;

            if (findBuilding)
            {
                building = gameEntity.Entity as BuildingEntity;
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAliveBuildings && building != null && building.IsDead())
                        building = null;
                    if (findOnlyActivatableBuildings && building != null && !building.Activatable)
                        building = null;
                }
            }

            if (findVehicle)
                vehicle = gameEntity.Entity as VehicleEntity;

            if (findWarpPortal)
                warpPortal = gameEntity.Entity as WarpPortalEntity;

            if (findItemsContainer)
                itemsContainer = gameEntity.Entity as ItemsContainerEntity;
        }

        private void RemoveInactiveAndSortNearestEntity<T>(List<T> entities) where T : BaseGameEntity
        {
            T temp;
            bool hasUpdate = false;
            for (int i = entities.Count - 1; i >= 0; --i)
            {
                if (entities[i] == null || !entities[i].gameObject.activeInHierarchy)
                {
                    entities.RemoveAt(i);
                    hasUpdate = true;
                }
            }
            if (hasUpdate && onUpdateList != null)
                onUpdateList.Invoke();
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = 0; j < entities.Count - 1; j++)
                {
                    if (Vector3.Distance(entities[j].CacheTransform.position, CacheTransform.position) >
                        Vector3.Distance(entities[j + 1].CacheTransform.position, CacheTransform.position))
                    {
                        temp = entities[j + 1];
                        entities[j + 1] = entities[j];
                        entities[j] = temp;
                    }
                }
            }
        }
    }
}
