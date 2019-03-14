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
        public BasePlayerCharacterEntity nearestPlayer { get; private set; }
        public BaseMonsterCharacterEntity nearestMonster { get; private set; }
        public BaseCharacterEntity nearestCharacter { get; private set; }
        public NpcEntity nearestNpc { get; private set; }
        public ItemDropEntity nearestItemDrop { get; private set; }
        public BuildingEntity nearestBuilding { get; private set; }
        public readonly HashSet<BasePlayerCharacterEntity> players = new HashSet<BasePlayerCharacterEntity>();
        public readonly HashSet<BaseMonsterCharacterEntity> monsters = new HashSet<BaseMonsterCharacterEntity>();
        public readonly HashSet<NpcEntity> npcs = new HashSet<NpcEntity>();
        public readonly HashSet<BuildingEntity> buildings = new HashSet<BuildingEntity>();
        public readonly HashSet<ItemDropEntity> itemDrops = new HashSet<ItemDropEntity>();
        private readonly HashSet<Collider> excludeColliders = new HashSet<Collider>();
        private readonly HashSet<Collider2D> excludeCollider2Ds = new HashSet<Collider2D>();
        private SphereCollider cacheCollider;
        private CircleCollider2D cacheCollider2D;

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
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
                cacheCollider.radius = detectingRadius;
            else
                cacheCollider2D.radius = detectingRadius;

            CacheTransform.position = BasePlayerCharacterController.OwningCharacter.CacheTransform.position;
            // Find nearby entities
            nearestPlayer = FindNearestEntity(players);
            nearestMonster = FindNearestEntity(monsters);
            nearestNpc = FindNearestEntity(npcs);
            nearestItemDrop = FindNearestEntity(itemDrops);
            nearestBuilding = FindNearestEntity(buildings);
            nearestCharacter = nearestPlayer;
            if (nearestCharacter == null)
            {
                // If no nearest player, nearest monster is nearest character
                nearestCharacter = nearestMonster;
            }
            else if (nearestMonster != null &&
                Vector3.Distance(nearestCharacter.CacheTransform.position, CacheTransform.position) >
                Vector3.Distance(nearestMonster.CacheTransform.position, CacheTransform.position))
            {
                // If monster is nearer, nearest monster is nearest character
                nearestCharacter = nearestMonster;
            }
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
                players.Add(player);
                return true;
            }
            if (monster != null)
            {
                monsters.Add(monster);
                return true;
            }
            if (npc != null)
            {
                npcs.Add(npc);
                return true;
            }
            if (building != null)
            {
                buildings.Add(building);
                return true;
            }
            if (itemDrop != null)
            {
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
            FindEntity(other, out player, out monster, out npc, out itemDrop, out building);

            if (player != null)
                return players.Remove(player);
            if (monster != null)
                return monsters.Remove(monster);
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
            out BuildingEntity building)
        {
            player = null;
            if (findPlayer)
            {
                player = other.GetComponent<BasePlayerCharacterEntity>();
                if (player == BasePlayerCharacterController.OwningCharacter)
                    player = null;
                if (findOnlyAlivePlayers && player != null && player.IsDead())
                    player = null;
                if (findPlayerToAttack && player != null && player.IsAlly(BasePlayerCharacterController.OwningCharacter))
                    player = null;
            }

            monster = null;
            if (findMonster)
            {
                monster = other.GetComponent<BaseMonsterCharacterEntity>();
                if (findOnlyAliveMonsters && monster != null && monster.IsDead())
                    monster = null;
                if (findMonsterToAttack && monster != null && monster.IsAlly(BasePlayerCharacterController.OwningCharacter))
                    monster = null;
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
                if (findOnlyAliveBuildings && building != null && building.IsDead())
                    building = null;
                if (findOnlyActivatableBuildings && building != null && !building.Activatable)
                    building = null;
            }
        }

        private T FindNearestEntity<T>(HashSet<T> entities) where T : BaseGameEntity
        {
            T nearestEntity = null;
            float nearestDistance = float.MaxValue;
            float distance;
            foreach (T entity in entities)
            {
                distance = Vector3.Distance(entity.CacheTransform.position, CacheTransform.position);
                if (distance < nearestDistance)
                {
                    nearestEntity = entity;
                    nearestDistance = distance;
                }
            }
            return nearestEntity;
        }
    }
}
