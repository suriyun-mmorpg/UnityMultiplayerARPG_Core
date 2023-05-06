using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIMinimapRenderer : MonoBehaviour
    {
        private struct MarkerData
        {
            public BaseCharacterEntity Character { get; set; }
            public RectTransform Marker { get; set; }
            public Vector3 MarkerRotateOffsets { get; set; }
        }
        public enum Mode
        {
            Default,
            FollowPlayingCharacter,
        }
        [Header("Settings")]
        public Mode mode;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform playingCharacterMarker;
        public Vector3 playingCharacterRotateOffsets = Vector3.zero;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform allyMemberMarkerPrefab;
        public Vector3 allyMemberRotateOffsets = Vector3.zero;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform partyMemberMarkerPrefab;
        public Vector3 partyMemberRotateOffsets = Vector3.zero;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform guildMemberMarkerPrefab;
        public Vector3 guildMemberRotateOffsets = Vector3.zero;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform enemyMarkerPrefab;
        public Vector3 enemyRotateOffsets = Vector3.zero;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform neutralMarkerPrefab;
        public Vector3 neutralRotateOffsets = Vector3.zero;
        public RectTransform nonPlayingCharacterMarkerContainer;
        public float allyMarkerDistance = 10000f;
        public float enemyOrNeutralMarkerDistance = 5f;
        public float updateMarkerDuration = 1f;
        [Tooltip("Image's anchor min, max and pivot must be 0.5")]
        public Image imageMinimap;
        [Header("Testing")]
        public bool isTestMode;
        public BaseMapInfo testingMapInfo;
        public Transform testingPlayingCharacterTransform;

        private float _updateMarkerCountdown;
        private BaseMapInfo _currentMapInfo;
        private List<MarkerData> _markers = new List<MarkerData>();

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null)
            {
                _updateMarkerCountdown = 0f;
                if (imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(false);
                return;
            }
            _currentMapInfo = mapInfo;

            // Use bounds size to calculate transforms
            float boundsWidth = _currentMapInfo.MinimapBoundsWidth;
            float boundsLength = _currentMapInfo.MinimapBoundsLength;
            float maxBoundsSize = Mathf.Max(boundsWidth, boundsLength);

            // Prepare target transform to follow
            Transform playingCharacterTransform = isTestMode ? testingPlayingCharacterTransform : GameInstance.PlayingCharacterEntity.EntityTransform;

            if (imageMinimap != null)
            {
                imageMinimap.sprite = _currentMapInfo.MinimapSprite;
                imageMinimap.preserveAspect = true;
                if (!imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(true);

                float imageSizeX = imageMinimap.rectTransform.sizeDelta.x;
                float imageSizeY = imageMinimap.rectTransform.sizeDelta.y;
                float minImageSize = Mathf.Min(imageSizeX, imageSizeY);

                float sizeRate = -(minImageSize / maxBoundsSize);

                _updateMarkerCountdown -= Time.deltaTime;
                if (_updateMarkerCountdown <= 0f)
                {
                    _updateMarkerCountdown = updateMarkerDuration;
                    InstantiateEntitiesMarkers(sizeRate);
                }
                UpdateEntitiesMarkersPosition(sizeRate);

                if (playingCharacterMarker != null)
                {
                    playingCharacterMarker.SetAsLastSibling();
                    SetMarkerPositionAndRotation(playingCharacterMarker, playingCharacterTransform, sizeRate, playingCharacterRotateOffsets);
                }
                if (mode == Mode.Default)
                {
                    imageMinimap.transform.localPosition = Vector2.zero;
                }
                else
                {
                    imageMinimap.transform.localPosition = -new Vector2((_currentMapInfo.MinimapPosition.x - playingCharacterTransform.position.x) * sizeRate, (_currentMapInfo.MinimapPosition.z - playingCharacterTransform.position.z) * sizeRate);
                }
            }
        }

        private void UpdateEntitiesMarkersPosition(float sizeRate)
        {
            for (int i = _markers.Count - 1; i >= 0; --i)
            {
                if (_markers[i].Character == null)
                {
                    Destroy(_markers[i].Marker.gameObject);
                    _markers.RemoveAt(i);
                    continue;
                }

                SetMarkerPositionAndRotation(_markers[i].Marker, _markers[i].Character.EntityTransform, sizeRate, _markers[i].MarkerRotateOffsets);
            }
        }

        private void InstantiateEntitiesMarkers(float sizeRate)
        {
            for (int i = _markers.Count - 1; i >= 0; --i)
            {
                Destroy(_markers[i].Marker.gameObject);
            }
            _markers.Clear();

            if (GameInstance.PlayingCharacterEntity != null)
            {
                int overlapMask = GameInstance.Singleton.playerLayer.Mask | GameInstance.Singleton.playingLayer.Mask | GameInstance.Singleton.monsterLayer.Mask;
                List<BaseCharacterEntity> allies = GameInstance.PlayingCharacterEntity.FindEntities<BaseCharacterEntity>(allyMarkerDistance, true, true, false, false, overlapMask);
                List<BaseCharacterEntity> enemies = GameInstance.PlayingCharacterEntity.FindEntities<BaseCharacterEntity>(enemyOrNeutralMarkerDistance, true, false, true, true, overlapMask);
                EntityInfo entityInfo;
                RectTransform markerPrefab;
                Vector3 markerRotateOffsets;
                foreach (BaseCharacterEntity entry in allies)
                {
                    markerPrefab = null;
                    markerRotateOffsets = Vector3.zero;
                    entityInfo = entry.GetInfo();
                    if (guildMemberMarkerPrefab != null && entityInfo.GuildId > 0 && entityInfo.GuildId == GameInstance.PlayingCharacterEntity.GuildId)
                    {
                        markerPrefab = guildMemberMarkerPrefab;
                        markerRotateOffsets = guildMemberRotateOffsets;
                    }
                    else if (partyMemberMarkerPrefab != null && entityInfo.PartyId > 0 && entityInfo.PartyId == GameInstance.PlayingCharacterEntity.PartyId)
                    {
                        markerPrefab = partyMemberMarkerPrefab;
                        markerRotateOffsets = partyMemberRotateOffsets;
                    }
                    else if (allyMemberMarkerPrefab != null)
                    {
                        markerPrefab = allyMemberMarkerPrefab;
                        markerRotateOffsets = allyMemberRotateOffsets;
                    }
                    if (markerPrefab != null)
                    {
                        InstantiateEntityMarker(entry, markerRotateOffsets, sizeRate, markerPrefab);
                    }
                }
                foreach (BaseCharacterEntity entry in enemies)
                {
                    markerPrefab = null;
                    markerRotateOffsets = Vector3.zero;
                    entityInfo = entry.GetInfo();
                    if (enemyMarkerPrefab != null && GameInstance.PlayingCharacterEntity.IsEnemy(entityInfo))
                    {
                        markerPrefab = enemyMarkerPrefab;
                        markerRotateOffsets = enemyRotateOffsets;
                    }
                    else if (neutralMarkerPrefab != null)
                    {
                        markerPrefab = neutralMarkerPrefab;
                        markerRotateOffsets = neutralRotateOffsets;
                    }
                    if (markerPrefab != null)
                    {
                        InstantiateEntityMarker(entry, markerRotateOffsets, sizeRate, markerPrefab);
                    }
                }
            }
        }

        private void InstantiateEntityMarker(BaseCharacterEntity character, Vector3 markerRotateOffsets, float sizeRate, RectTransform prefab)
        {
            RectTransform newMarker = Instantiate(prefab);
            newMarker.SetParent(nonPlayingCharacterMarkerContainer);
            newMarker.transform.localScale = Vector3.one;
            SetMarkerPositionAndRotation(newMarker, character.EntityTransform, sizeRate, markerRotateOffsets);
            _markers.Add(new MarkerData()
            {
                Character = character,
                Marker = newMarker,
                MarkerRotateOffsets = markerRotateOffsets,
            });
        }

        private void SetMarkerPositionAndRotation(RectTransform makerTransform, Transform entityTransform, float sizeRate, Vector3 markerRotateOffsets)
        {
            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension2D:
                    makerTransform.localPosition = new Vector2(
                                                (_currentMapInfo.MinimapPosition.x - entityTransform.position.x) * sizeRate,
                                                (_currentMapInfo.MinimapPosition.y - entityTransform.position.y) * sizeRate);
                    makerTransform.localEulerAngles = Vector3.zero;
                    break;
                default:
                    makerTransform.localPosition = new Vector2(
                                                (_currentMapInfo.MinimapPosition.x - entityTransform.position.x) * sizeRate,
                                                (_currentMapInfo.MinimapPosition.z - entityTransform.position.z) * sizeRate);
                    makerTransform.localEulerAngles = markerRotateOffsets + (Vector3.back * entityTransform.eulerAngles.y);
                    break;
            }
        }
    }
}
