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

        private float updateMarkerCountdown;
        private BaseMapInfo currentMapInfo;
        private List<MarkerData> markers = new List<MarkerData>();

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null)
            {
                updateMarkerCountdown = 0f;
                if (imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(false);
                return;
            }
            currentMapInfo = mapInfo;

            // Use bounds size to calculate transforms
            float boundsWidth = currentMapInfo.MinimapBoundsWidth;
            float boundsLength = currentMapInfo.MinimapBoundsLength;
            float maxBoundsSize = Mathf.Max(boundsWidth, boundsLength);

            // Prepare target transform to follow
            Transform playingCharacterTransform = isTestMode ? testingPlayingCharacterTransform : GameInstance.PlayingCharacterEntity.CacheTransform;

            if (imageMinimap != null)
            {
                imageMinimap.sprite = currentMapInfo.MinimapSprite;
                imageMinimap.preserveAspect = true;
                if (!imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(true);

                float imageSizeX = imageMinimap.rectTransform.sizeDelta.x;
                float imageSizeY = imageMinimap.rectTransform.sizeDelta.y;
                float minImageSize = Mathf.Min(imageSizeX, imageSizeY);

                float sizeRate = -(minImageSize / maxBoundsSize);

                updateMarkerCountdown -= Time.deltaTime;
                if (updateMarkerCountdown <= 0f)
                {
                    updateMarkerCountdown = updateMarkerDuration;
                    InstantiateEntitiesMarkers(sizeRate);
                }
                UpdateEntitiesMarkersPosition(sizeRate);

                if (playingCharacterMarker != null)
                {
                    playingCharacterMarker.SetAsLastSibling();
                    if (mode == Mode.Default)
                    {
                        playingCharacterMarker.localPosition = new Vector2((currentMapInfo.MinimapPosition.x - playingCharacterTransform.position.x) * sizeRate, (currentMapInfo.MinimapPosition.z - playingCharacterTransform.position.z) * sizeRate);
                    }
                    else
                    {
                        playingCharacterMarker.localPosition = Vector2.zero;
                    }
                    playingCharacterMarker.localEulerAngles = playingCharacterRotateOffsets + (Vector3.back * playingCharacterTransform.eulerAngles.y);
                }
                if (mode == Mode.Default)
                {
                    imageMinimap.transform.localPosition = Vector2.zero;
                }
                else
                {
                    imageMinimap.transform.localPosition = -new Vector2((currentMapInfo.MinimapPosition.x - playingCharacterTransform.position.x) * sizeRate, (currentMapInfo.MinimapPosition.z - playingCharacterTransform.position.z) * sizeRate);
                }
            }
        }

        private void UpdateEntitiesMarkersPosition(float sizeRate)
        {
            for (int i = markers.Count - 1; i >= 0; --i)
            {
                if (markers[i].Character == null)
                {
                    Destroy(markers[i].Marker.gameObject);
                    markers.RemoveAt(i);
                    continue;
                }

                markers[i].Marker.localPosition = new Vector2(
                                            (currentMapInfo.MinimapPosition.x - markers[i].Character.CacheTransform.position.x) * sizeRate,
                                            (currentMapInfo.MinimapPosition.z - markers[i].Character.CacheTransform.position.z) * sizeRate);
                markers[i].Marker.localEulerAngles = markers[i].MarkerRotateOffsets + (Vector3.back * markers[i].Character.CacheTransform.eulerAngles.y);
            }
        }

        private void InstantiateEntitiesMarkers(float sizeRate)
        {
            for (int i = markers.Count - 1; i >= 0; --i)
            {
                Destroy(markers[i].Marker.gameObject);
            }
            markers.Clear();

            if (GameInstance.PlayingCharacterEntity != null)
            {
                List<BaseCharacterEntity> allies = GameInstance.PlayingCharacterEntity.FindCharacters<BaseCharacterEntity>(allyMarkerDistance, true, true, false, false);
                List<BaseCharacterEntity> enemies = GameInstance.PlayingCharacterEntity.FindCharacters<BaseCharacterEntity>(enemyOrNeutralMarkerDistance, true, false, true, true);
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
            newMarker.localPosition = new Vector2(
                                        (currentMapInfo.MinimapPosition.x - character.CacheTransform.position.x) * sizeRate,
                                        (currentMapInfo.MinimapPosition.z - character.CacheTransform.position.z) * sizeRate);
            newMarker.localEulerAngles = markerRotateOffsets + (Vector3.back * character.CacheTransform.eulerAngles.y);
            markers.Add(new MarkerData()
            {
                Character = character,
                Marker = newMarker,
                MarkerRotateOffsets = markerRotateOffsets,
            });
        }
    }
}
