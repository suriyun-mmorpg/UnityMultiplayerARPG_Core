using UnityEngine;

namespace MultiplayerARPG
{
    public class MinimapRenderer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("You can use Unity's plane as mesh minimap")]
        public float spriteOffsets3D = -100f;
        public float spriteOffsets2D = 1f;
        public Sprite noMinimapSprite = null;
        public UnityLayer layer;

        [Header("Testing")]
        public bool isTestMode;
        public BaseMapInfo testingMapInfo;
        public DimensionType testingDimensionType;

        private BaseMapInfo currentMapInfo;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = new GameObject("__MinimapRenderer").AddComponent<SpriteRenderer>();
            spriteRenderer.gameObject.layer = layer.LayerIndex;
        }

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null || mapInfo == currentMapInfo)
                return;
            currentMapInfo = mapInfo;

            // Use bounds size to calculate transforms
            float boundsWidth = currentMapInfo.MinimapBoundsWidth;
            float boundsLength = currentMapInfo.MinimapBoundsLength;
            float maxBoundsSize = Mathf.Max(boundsWidth, boundsLength);

            // Set dimention type
            DimensionType dimensionType = GameInstance.Singleton == null || isTestMode ? testingDimensionType : GameInstance.Singleton.DimensionType;

            if (spriteRenderer != null)
            {
                switch (dimensionType)
                {
                    case DimensionType.Dimension2D:
                        spriteRenderer.transform.position = currentMapInfo.MinimapPosition + (Vector3.forward * spriteOffsets2D);
                        spriteRenderer.transform.eulerAngles = Vector3.zero;
                        break;
                    default:
                        spriteRenderer.transform.position = currentMapInfo.MinimapPosition + (Vector3.up * spriteOffsets3D);
                        spriteRenderer.transform.eulerAngles = new Vector3(90f, 0f, 0f);
                        break;
                }
                spriteRenderer.sprite = currentMapInfo.MinimapSprite != null ? currentMapInfo.MinimapSprite : noMinimapSprite;
                if (spriteRenderer.sprite != null)
                    spriteRenderer.transform.localScale = new Vector3(1f, 1f) * maxBoundsSize * spriteRenderer.sprite.pixelsPerUnit / Mathf.Max(spriteRenderer.sprite.texture.width, spriteRenderer.sprite.texture.height);
            }
        }
    }
}
