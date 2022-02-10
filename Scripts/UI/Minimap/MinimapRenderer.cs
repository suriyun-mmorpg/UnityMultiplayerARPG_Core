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
        public float allyMarkerDistance = 10000f;
        public float enemyMarkerDistance = 5f;
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
            Transform playingCharacterTransform = isTestMode ? testingPlayingCharacterTransform : GameInstance.PlayingCharacterEntity.CacheTransform;
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
                        playingCharacterMarker.localPosition = new Vector2((playingCharacterTransform.position.x - currentMapInfo.MinimapPosition.x) * sizeRate, (playingCharacterTransform.position.z - currentMapInfo.MinimapPosition.z) * sizeRate);
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
                    imageMinimap.transform.localPosition = -new Vector2((playingCharacterTransform.position.x - currentMapInfo.MinimapPosition.x) * sizeRate, (playingCharacterTransform.position.z - currentMapInfo.MinimapPosition.z) * sizeRate);
                }
                if (GameInstance.PlayingCharacterEntity != null)
                {
                    GameInstance.PlayingCharacterEntity.FindNearestAliveCharacter(GameInstance.PlayingCharacterEntity.CacheTransform.position, true, false, false);
                    GameInstance.PlayingCharacterEntity.FindNearestCharacter(GameInstance.PlayingCharacterEntity.CacheTransform.position, false, true, true);
                    // Update ally
                    if (allyMemberMarkerPrefab != null)
                    {

                    }
                    // Update party members
                    if (partyMemberMarkerPrefab != null)
                    {

                    }
                    // Update guild members
                    if (guildMemberMarkerPrefab != null)
                    {

                    }
                    // Update enemy
                    if (enemyMarkerPrefab != null)
                    {

                    }
                    // Update neutral
                    if (neutralMarkerPrefab != null)
                    {

                    }
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
