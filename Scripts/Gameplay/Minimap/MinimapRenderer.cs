using UnityEngine;

namespace MultiplayerARPG
{
    public class MinimapRenderer : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("You can use Unity's plane as mesh minimap")]
        public MeshRenderer meshMinimapPrefab;
        public float meshYOffsets = -100f;
        public float meshXZScaling = 0.1f;

        [Header("Testing")]
        public bool isTestMode;
        public BaseMapInfo testingMapInfo;

        private BaseMapInfo currentMapInfo;
        private MeshRenderer meshMinimap;

        private void Start()
        {
            if (!meshMinimapPrefab)
                meshMinimapPrefab = Resources.Load<MeshRenderer>("__DefaultMinimapMesh");
            meshMinimap = Instantiate(meshMinimapPrefab);
        }

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null)
                return;
            currentMapInfo = mapInfo;

            // Use bounds size to calculate transforms
            float boundsSizeX = currentMapInfo.MinimapBoundsSizeX;
            float boundsSizeZ = currentMapInfo.MinimapBoundsSizeZ;
            float maxBoundsSize = Mathf.Max(boundsSizeX, boundsSizeZ);

            if (meshMinimap != null)
            {
                meshMinimap.transform.position = currentMapInfo.MinimapPosition + (Vector3.up * meshYOffsets);
                meshMinimap.transform.localScale = (new Vector3(1f, 0f, 1f) * maxBoundsSize * meshXZScaling) + Vector3.up;
                meshMinimap.material.mainTexture = currentMapInfo.MinimapSprite.texture;
                meshMinimap.material.color = Color.white;
            }
        }
    }
}
