using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class MinimapRenderer : MonoBehaviour
    {
        public enum Mode
        {
            Default,
            FollowPlayingCharacter,
        }
        [Header("Settings")]
        public Mode mode;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform playingCharacterMarker;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform allyCharacterMarkerPrefab;
        [Tooltip("Marker's anchor min, max and pivot must be 0.5")]
        public RectTransform enemyCharacterMarkerPrefab;
        [Tooltip("Image's anchor min, max and pivot must be 0.5")]
        public Image imageMinimap;
        [Tooltip("You can use Unity's plane as mesh minimap")]
        public MeshRenderer meshMinimapPrefab;
        public float meshYOffsets = -100f;
        public float meshXZScaling = 0.1f;
        [Header("Testing")]
        public bool isTestMode;
        public BaseMapInfo testingMapInfo;
        public Transform testingPlayingCharacterTransform;
        private BaseMapInfo currentMapInfo;
        private MeshRenderer meshMinimap;

        private void Start()
        {
            if (meshMinimapPrefab)
                meshMinimap = Instantiate(meshMinimapPrefab);
        }

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null)
            {
                if (imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(false);
                return;
            }
            Transform owningCharacterTransform = isTestMode ? testingPlayingCharacterTransform : GameInstance.PlayingCharacterEntity.CacheTransform;
            currentMapInfo = mapInfo;

            // Use bounds size to calculate transforms
            float boundsSizeX = currentMapInfo.MinimapBoundsSizeX;
            float boundsSizeZ = currentMapInfo.MinimapBoundsSizeZ;
            float maxBoundsSize = Mathf.Max(boundsSizeX, boundsSizeZ);

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
                if (playingCharacterMarker != null)
                {
                    playingCharacterMarker.SetAsLastSibling();
                    if (mode == Mode.Default)
                    {
                        playingCharacterMarker.localPosition = new Vector2((owningCharacterTransform.position.x - currentMapInfo.MinimapPosition.x) * sizeRate, (owningCharacterTransform.position.z - currentMapInfo.MinimapPosition.z) * sizeRate);
                    }
                    else
                    {
                        playingCharacterMarker.localPosition = Vector2.zero;
                    }
                }
                if (mode == Mode.Default)
                {
                    imageMinimap.transform.localPosition = Vector2.zero;
                }
                else
                {
                    imageMinimap.transform.localPosition = -new Vector2((owningCharacterTransform.position.x - currentMapInfo.MinimapPosition.x) * sizeRate, (owningCharacterTransform.position.z - currentMapInfo.MinimapPosition.z) * sizeRate);
                }
            }

            if (meshMinimap != null)
            {
                meshMinimap.transform.position = currentMapInfo.MinimapPosition + (Vector3.up * meshYOffsets);
                meshMinimap.transform.localScale = (new Vector3(1f, 0f, 1f) * maxBoundsSize * meshXZScaling) + Vector3.up;
                meshMinimap.material.mainTexture = currentMapInfo.MinimapSprite.texture;
            }
        }
    }
}
