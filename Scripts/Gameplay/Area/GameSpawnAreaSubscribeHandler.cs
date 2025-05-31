using Insthync.SpatialPartitioningSystems;
using LiteNetLibManager;
using System.Collections.Generic;
using Unity.Mathematics;

namespace MultiplayerARPG
{
    public class GameSpawnAreaSubscribeHandler : ISpatialObjectComponent
    {
        public enum SpawnState
        {
            Despawned,
            Spawned,
        }

        public uint SpatialObjectId { get; set; } = 0;
        public bool SpatialObjectEnabled { get; set; } = false;
        public float3 SpatialObjectPosition => GameSpawnArea.transform.position;

        public SpatialObjectShape SpatialObjectShape
        {
            get
            {
                switch (GameSpawnArea.type)
                {
                    case GameAreaType.Square:
                        return SpatialObjectShape.Box;
                    default:
                        return SpatialObjectShape.Sphere;
                }
            }
        }

        public float SpatialObjectRadius
        {
            get
            {
                switch (GameSpawnArea.type)
                {
                    case GameAreaType.Radius:
                        return GameSpawnArea.randomRadius;
                }
                return 0f;
            }
        }

        public float3 SpatialObjectExtents
        {
            get
            {
                switch (GameSpawnArea.type)
                {
                    case GameAreaType.Square:
                        switch (GameInstance.Singleton.DimensionType)
                        {
                            case DimensionType.Dimension2D:
                                return new float3(GameSpawnArea.squareSizeX * 0.5f, GameSpawnArea.squareSizeZ * 0.5f, 0f);
                            default:
                                return new float3(GameSpawnArea.squareSizeX * 0.5f, 0f, GameSpawnArea.squareSizeZ * 0.5f);
                        }
                }
                return new float3(0f, 0f, 0f);
            }
        }

        public GameSpawnArea GameSpawnArea { get; private set; }

        private readonly HashSet<GameSpawnAreaEntityHandler> _entityHandlers = new HashSet<GameSpawnAreaEntityHandler>();
        private readonly HashSet<uint> _subscribers = new HashSet<uint>();
        private float _timer = 0f;
        private SpawnState _spawnState = SpawnState.Despawned;

        public GameSpawnAreaSubscribeHandler(GameSpawnArea gameSpawnArea)
        {
            GameSpawnArea = gameSpawnArea;
        }

        public void Clean()
        {
            _entityHandlers.Clear();
            _subscribers.Clear();
        }

        public void AddSubscriber(uint id)
        {
            _subscribers.Add(id);
        }

        public void ClearSubscribers()
        {
            _subscribers.Clear();
        }

        public void AddEntity(LiteNetLibBehaviour entity)
        {
            if (entity == null || entity.TryGetComponent(out GameSpawnAreaEntityHandler entityHandler))
                return;
            entityHandler = entity.gameObject.AddComponent<GameSpawnAreaEntityHandler>();
            entityHandler.Handler = this;
            entityHandler.Entity = entity;
            _entityHandlers.Add(entityHandler);
        }

        public void OnEntityHandlerDestroy(GameSpawnAreaEntityHandler entityHandler)
        {
            if (entityHandler == null)
                return;
            _entityHandlers.Remove(entityHandler);
        }

        public void Update(float deltaTime, float unspawnDelayInSeconds)
        {
            switch (_spawnState)
            {
                case SpawnState.Spawned:
                    UpdateSpawned(deltaTime, unspawnDelayInSeconds);
                    break;
                case SpawnState.Despawned:
                    UpdateDespawned(deltaTime, unspawnDelayInSeconds);
                    break;
            }
        }

        private void UpdateSpawned(float deltaTime, float unspawnDelayInSeconds)
        {
            if (_subscribers.Count > 0)
            {
                _timer = 0f;
                return;
            }

            _timer += deltaTime;
            if (_timer >= unspawnDelayInSeconds)
            {
                _timer = 0f;
                foreach (GameSpawnAreaEntityHandler entityHandler in _entityHandlers)
                {
                    if (entityHandler.Entity == null)
                        continue;
                    entityHandler.Handler = null;
                    entityHandler.Entity.NetworkDestroy();
                }
                _entityHandlers.Clear();
                _spawnState = SpawnState.Despawned;
            }
        }

        private void UpdateDespawned(float deltaTime, float unspawnDelayInSeconds)
        {
            if (_subscribers.Count <= 0)
                return;
            GameSpawnArea.SpawnAll();
            _spawnState = SpawnState.Spawned;
        }
    }
}
