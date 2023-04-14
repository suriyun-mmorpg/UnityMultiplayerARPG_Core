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
        public bool findActivatableEntity;
        public bool findHoldActivatableEntity;
        public bool findPickupActivatableEntity;
        public readonly List<BaseCharacterEntity> characters = new List<BaseCharacterEntity>();
        public readonly List<BasePlayerCharacterEntity> players = new List<BasePlayerCharacterEntity>();
        public readonly List<BaseMonsterCharacterEntity> monsters = new List<BaseMonsterCharacterEntity>();
        public readonly List<NpcEntity> npcs = new List<NpcEntity>();
        public readonly List<ItemDropEntity> itemDrops = new List<ItemDropEntity>();
        public readonly List<BuildingEntity> buildings = new List<BuildingEntity>();
        public readonly List<VehicleEntity> vehicles = new List<VehicleEntity>();
        public readonly List<WarpPortalEntity> warpPortals = new List<WarpPortalEntity>();
        public readonly List<ItemsContainerEntity> itemsContainers = new List<ItemsContainerEntity>();
        public readonly List<IActivatableEntity> activatableEntities = new List<IActivatableEntity>();
        public readonly List<IHoldActivatableEntity> holdActivatableEntities = new List<IHoldActivatableEntity>();
        public readonly List<IPickupActivatableEntity> pickupActivatableEntities = new List<IPickupActivatableEntity>();
        private readonly HashSet<Collider> _excludeColliders = new HashSet<Collider>();
        private readonly HashSet<Collider2D> _excludeCollider2Ds = new HashSet<Collider2D>();
        private SphereCollider _cacheCollider;
        private CircleCollider2D _cacheCollider2D;

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
                _cacheCollider = gameObject.AddComponent<SphereCollider>();
                _cacheCollider.radius = detectingRadius;
                _cacheCollider.isTrigger = true;
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.useGravity = false;
                rigidbody.isKinematic = true;
                rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }
            else
            {
                _cacheCollider2D = gameObject.AddComponent<CircleCollider2D>();
                _cacheCollider2D.radius = detectingRadius;
                _cacheCollider2D.isTrigger = true;
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
                _cacheCollider.radius = detectingRadius;
            else
                _cacheCollider2D.radius = detectingRadius;

            CacheTransform.position = GameInstance.PlayingCharacterEntity.EntityTransform.position;
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
            RemoveInactiveAndSortNearestActivatableEntity(activatableEntities);
            RemoveInactiveAndSortNearestActivatableEntity(holdActivatableEntities);
            RemoveInactiveAndSortNearestActivatableEntity(pickupActivatableEntities);
        }

        public void ClearExcludeColliders()
        {
            _excludeColliders.Clear();
            _excludeCollider2Ds.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_excludeColliders.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
            {
                _excludeColliders.Add(other);
                return;
            }
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!RemoveEntity(other.gameObject))
                return;
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_excludeCollider2Ds.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
            {
                _excludeCollider2Ds.Add(other);
                return;
            }
            if (onUpdateList != null)
                onUpdateList.Invoke();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
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
            IActivatableEntity activatableEntity;
            IHoldActivatableEntity holdActivatableEntity;
            IPickupActivatableEntity pickupActivatableEntity;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal, out itemsContainer, out activatableEntity, out holdActivatableEntity, out pickupActivatableEntity, true);

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
            IActivatableEntity activatableEntity;
            IHoldActivatableEntity holdActivatableEntity;
            IPickupActivatableEntity pickupActivatableEntity;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, out vehicle, out warpPortal, out itemsContainer, out activatableEntity, out holdActivatableEntity, out pickupActivatableEntity, false);

            bool removeSomething = false;
            if (player != null)
                removeSomething = removeSomething || characters.Remove(player) && players.Remove(player);
            if (monster != null)
                removeSomething = removeSomething || characters.Remove(monster) && monsters.Remove(monster);
            if (npc != null)
                removeSomething = removeSomething || npcs.Remove(npc);
            if (itemDrop != null)
                removeSomething = removeSomething || itemDrops.Remove(itemDrop);
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
                        if (findOnlyActivatableBuildings && building != null && !building.CanActivate())
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

            if (findActivatableEntity)
            {
                activatableEntity = other.GetComponent<IActivatableEntity>();
                if (!activatableEntity.IsNull() && activatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                    activatableEntity = null;
            }

            if (findHoldActivatableEntity)
            {
                holdActivatableEntity = other.GetComponent<IHoldActivatableEntity>();
                if (!holdActivatableEntity.IsNull() && holdActivatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                    holdActivatableEntity = null;
            }

            if (findPickupActivatableEntity)
            {
                pickupActivatableEntity = other.GetComponent<IPickupActivatableEntity>();
                if (!pickupActivatableEntity.IsNull() && pickupActivatableEntity.EntityGameObject == GameInstance.PlayingCharacterEntity.EntityGameObject)
                    pickupActivatableEntity = null;
            }
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
                    if (Vector3.Distance(entities[j].transform.position, CacheTransform.position) >
                        Vector3.Distance(entities[j + 1].transform.position, CacheTransform.position))
                    {
                        temp = entities[j + 1];
                        entities[j + 1] = entities[j];
                        entities[j] = temp;
                    }
                }
            }
        }

        private void RemoveInactiveAndSortNearestActivatableEntity<T>(List<T> entities) where T : IBaseActivatableEntity
        {
            T temp;
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
            if (hasUpdate && onUpdateList != null)
                onUpdateList.Invoke();
            for (int i = 0; i < entities.Count; i++)
            {
                for (int j = 0; j < entities.Count - 1; j++)
                {
                    if (Vector3.Distance(entities[j].EntityTransform.position, CacheTransform.position) >
                        Vector3.Distance(entities[j + 1].EntityTransform.position, CacheTransform.position))
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
