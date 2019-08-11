using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class NearbyEntityDetector : MonoBehaviour
    {
        private Transform cacheTransform;
        public Transform CacheTransform
        {
            get
            {
                if (cacheTransform == null)
                    cacheTransform = GetComponent<Transform>();
                return cacheTransform;
            }
        }

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
        public readonly List<BaseCharacterEntity> characters = new List<BaseCharacterEntity>();
        public readonly List<BasePlayerCharacterEntity> players = new List<BasePlayerCharacterEntity>();
        public readonly List<BaseMonsterCharacterEntity> monsters = new List<BaseMonsterCharacterEntity>();
        public readonly List<NpcEntity> npcs = new List<NpcEntity>();
        public readonly List<ItemDropEntity> itemDrops = new List<ItemDropEntity>();
        public readonly List<BuildingEntity> buildings = new List<BuildingEntity>();
        private readonly HashSet<Collider> excludeColliders = new HashSet<Collider>();
        private readonly HashSet<Collider2D> excludeCollider2Ds = new HashSet<Collider2D>();
        private SphereCollider cacheCollider;
        private CircleCollider2D cacheCollider2D;

        private void Awake()
        {
            // Set layer to ignore raycast
            gameObject.layer = 2;
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
            if (BasePlayerCharacterController.OwningCharacter == null)
                return;

            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
                cacheCollider.radius = detectingRadius;
            else
                cacheCollider2D.radius = detectingRadius;

            CacheTransform.position = BasePlayerCharacterController.OwningCharacter.CacheTransform.position;
            // Find nearby entities
            SortNearestEntity(characters);
            SortNearestEntity(players);
            SortNearestEntity(monsters);
            SortNearestEntity(npcs);
            SortNearestEntity(itemDrops);
            SortNearestEntity(buildings);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (excludeColliders.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
                excludeColliders.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (excludeColliders.Contains(other))
                return;
            RemoveEntity(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (excludeCollider2Ds.Contains(other))
                return;
            if (!AddEntity(other.gameObject))
                excludeCollider2Ds.Add(other);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (excludeCollider2Ds.Contains(other))
                return;
            RemoveEntity(other.gameObject);
        }

        private bool AddEntity(GameObject other)
        {
            BasePlayerCharacterEntity player = null;
            BaseMonsterCharacterEntity monster = null;
            NpcEntity npc = null;
            ItemDropEntity itemDrop = null;
            BuildingEntity building = null;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building);

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
            if (building != null)
            {
                if (!buildings.Contains(building))
                    buildings.Add(building);
                return true;
            }
            if (itemDrop != null)
            {
                if (!itemDrops.Contains(itemDrop))
                    itemDrops.Add(itemDrop);
                return true;
            }
            return false;
        }

        private bool RemoveEntity(GameObject other)
        {
            BasePlayerCharacterEntity player = null;
            BaseMonsterCharacterEntity monster = null;
            NpcEntity npc = null;
            ItemDropEntity itemDrop = null;
            BuildingEntity building = null;
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building, false);

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
            return false;
        }

        private void FindEntity(GameObject other,
            out BasePlayerCharacterEntity player,
            out BaseMonsterCharacterEntity monster,
            out NpcEntity npc,
            out ItemDropEntity itemDrop,
            out BuildingEntity building,
            bool findWithAdvanceOptions = true)
        {
            player = null;
            if (findPlayer)
            {
                player = other.GetComponent<BasePlayerCharacterEntity>();
                if (player == BasePlayerCharacterController.OwningCharacter)
                    player = null;
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAlivePlayers && player != null && player.IsDead())
                        player = null;
                    if (findPlayerToAttack && player != null && !player.CanReceiveDamageFrom(BasePlayerCharacterController.OwningCharacter))
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
                    if (findMonsterToAttack && monster != null && !monster.CanReceiveDamageFrom(BasePlayerCharacterController.OwningCharacter))
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
                    building = buildingMaterial.buildingEntity;
                if (findWithAdvanceOptions)
                {
                    if (findOnlyAliveBuildings && building != null && building.IsDead())
                        building = null;
                    if (findOnlyActivatableBuildings && building != null && !building.Activatable)
                        building = null;
                }
            }
        }

        private void SortNearestEntity<T>(List<T> entities) where T : BaseGameEntity
        {
            T temp;
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities[i] == null)
                    entities.RemoveAt(i);
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
