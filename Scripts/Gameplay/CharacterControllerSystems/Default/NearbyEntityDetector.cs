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
        public readonly List<BaseCharacterEntity> characters = new List<BaseCharacterEntity>();
        public readonly List<BasePlayerCharacterEntity> players = new List<BasePlayerCharacterEntity>();
        public readonly List<BaseMonsterCharacterEntity> monsters = new List<BaseMonsterCharacterEntity>();
        public readonly List<NpcEntity> npcs = new List<NpcEntity>();
        public readonly List<ItemDropEntity> itemDrops = new List<ItemDropEntity>();
        public readonly List<BuildingEntity> buildings = new List<BuildingEntity>();
        public readonly List<VehicleEntity> vehicles = new List<VehicleEntity>();
        public readonly List<WarpPortalEntity> warpPortals = new List<WarpPortalEntity>();
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
            SortNearestEntity(characters);
            SortNearestEntity(players);
            SortNearestEntity(monsters);
            SortNearestEntity(npcs);
            SortNearestEntity(itemDrops);
            SortNearestEntity(buildings);
            SortNearestEntity(vehicles);
            SortNearestEntity(warpPortals);
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
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal);

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
            if (warpPortals != null)
            {
                if (!warpPortals.Contains(warpPortal))
                    warpPortals.Add(warpPortal);
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
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal, false);

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
            bool findWithAdvanceOptions = true)
        {
            player = null;
            if (findPlayer)
            {
                player = other.GetComponent<BasePlayerCharacterEntity>();
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

            monster = null;
            if (findMonster)
            {
                monster = other.GetComponent<BaseMonsterCharacterEntity>();
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAliveMonsters && monster != null && monster.IsDead())
                        monster = null;
                    if (findMonsterToAttack && monster != null && !monster.CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo()))
                        monster = null;
                }
            }

            npc = null;
            if (findNpc)
                npc = other.GetComponent<NpcEntity>();

            itemDrop = null;
            if (findItemDrop)
                itemDrop = other.GetComponent<ItemDropEntity>();

            building = null;
            if (findBuilding)
            {
                BuildingMaterial buildingMaterial = other.GetComponent<BuildingMaterial>();
                if (buildingMaterial != null)
                    building = buildingMaterial.BuildingEntity;
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAliveBuildings && building != null && building.IsDead())
                        building = null;
                    if (findOnlyActivatableBuildings && building != null && !building.Activatable)
                        building = null;
                }
            }

            vehicle = null;
            if (findVehicle)
                vehicle = other.GetComponent<VehicleEntity>();

            warpPortal = null;
            if (findWarpPortal)
                warpPortal = other.GetComponent<WarpPortalEntity>();
        }

        private void SortNearestEntity<T>(List<T> entities) where T : BaseGameEntity
        {
            T temp;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities[i] == null)
                {
                    entities.RemoveAt(i);
                    if (onUpdateList != null)
                        onUpdateList.Invoke();
                }
            }
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
