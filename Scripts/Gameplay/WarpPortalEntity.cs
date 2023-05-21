using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class WarpPortalEntity : BaseGameEntity, IActivatableEntity
    {
        [Category(5, "Warp Portal Settings")]
        [Tooltip("Signal to tell players that their character can enter the portal")]
        public GameObject[] warpSignals;
        [Tooltip("If this is `TRUE`, character will warp immediately when enter this warp portal")]
        public bool warpImmediatelyWhenEnter;
        [FormerlySerializedAs("type")]
        public WarpPortalType warpPortalType;
        [Tooltip("Map which character will warp to when use the warp portal, leave this empty to warp character to other position in the same map")]
        [FormerlySerializedAs("mapInfo")]
        public BaseMapInfo warpToMapInfo;
        [Tooltip("Position which character will warp to when use the warp portal")]
        [FormerlySerializedAs("position")]
        public Vector3 warpToPosition;
        [Tooltip("If this is `TRUE` it will change character's rotation when warp")]
        public bool warpOverrideRotation;
        [Tooltip("This will be used if `warpOverrideRotation` is `TRUE` to change character's rotation when warp")]
        public Vector3 warpToRotation;
        public WarpPointByCondition[] warpPointsByCondition = new WarpPointByCondition[0];

        [System.NonSerialized]
        private Dictionary<int, List<WarpPointByCondition>> _cacheWarpPointsByCondition;
        public Dictionary<int, List<WarpPointByCondition>> CacheWarpPointsByCondition
        {
            get
            {
                if (_cacheWarpPointsByCondition == null)
                {
                    _cacheWarpPointsByCondition = new Dictionary<int, List<WarpPointByCondition>>();
                    int factionDataId;
                    foreach (WarpPointByCondition warpPointByCondition in warpPointsByCondition)
                    {
                        factionDataId = 0;
                        if (warpPointByCondition.forFaction != null)
                            factionDataId = warpPointByCondition.forFaction.DataId;
                        if (!_cacheWarpPointsByCondition.ContainsKey(factionDataId))
                            _cacheWarpPointsByCondition.Add(factionDataId, new List<WarpPointByCondition>());
                        _cacheWarpPointsByCondition[factionDataId].Add(warpPointByCondition);
                    }
                }
                return _cacheWarpPointsByCondition;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            foreach (GameObject warpSignal in warpSignals)
            {
                if (warpSignal != null)
                    warpSignal.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            // Improve performance by tags
            if (!other.CompareTag(GameInstance.Singleton.playerTag))
                return;

            BasePlayerCharacterEntity playerCharacterEntity = other.GetComponentInParent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (warpImmediatelyWhenEnter && IsServer)
                EnterWarp(playerCharacterEntity);

            if (!warpImmediatelyWhenEnter)
            {
                if (playerCharacterEntity == GameInstance.PlayingCharacterEntity)
                {
                    foreach (GameObject warpSignal in warpSignals)
                    {
                        if (warpSignal != null)
                            warpSignal.SetActive(true);
                    }
                }
            }
        }

        private void TriggerExit(GameObject other)
        {
            // Improve performance by tags
            if (!other.CompareTag(GameInstance.Singleton.playerTag))
                return;

            BasePlayerCharacterEntity playerCharacterEntity = other.GetComponentInParent<BasePlayerCharacterEntity>();
            if (playerCharacterEntity == null)
                return;

            if (playerCharacterEntity == GameInstance.PlayingCharacterEntity)
            {
                foreach (GameObject warpSignal in warpSignals)
                {
                    if (warpSignal != null)
                        warpSignal.SetActive(false);
                }
            }
        }

        public virtual void EnterWarp(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity.IsDead())
                return;

            WarpPortalType portalType = warpPortalType;
            string mapName = warpToMapInfo == null ? string.Empty : warpToMapInfo.Id;
            Vector3 position = warpToPosition;
            bool overrideRotation = warpOverrideRotation;
            Vector3 rotation = warpToRotation;

            List<WarpPointByCondition> warpPoints;
            if (CacheWarpPointsByCondition.TryGetValue(playerCharacterEntity.FactionId, out warpPoints) ||
                CacheWarpPointsByCondition.TryGetValue(0, out warpPoints))
            {
                WarpPointByCondition warpPoint = warpPoints[Random.Range(0, warpPoints.Count)];
                portalType = warpPoint.warpPortalType;
                mapName = warpPoint.warpToMapInfo == null ? string.Empty : warpPoint.warpToMapInfo.Id;
                position = warpPoint.warpToPosition;
                overrideRotation = warpPoint.warpOverrideRotation;
                rotation = warpPoint.warpToRotation;
            }

            CurrentGameManager.WarpCharacter(portalType, playerCharacterEntity, mapName, position, overrideRotation, rotation);
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.conversationDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return true;
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
            GameInstance.PlayingCharacterEntity.CallServerEnterWarp(ObjectId);
        }
    }
}
