using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class MinimapRenderer : MonoBehaviour
    {
        public Transform owningCharacterMarker;
        public Image imageMinimap;
        public MeshRenderer meshMinimap;
        public float meshYOffsets = -100f;
        public float meshXZScaling = 0.1f;
        public bool isTestMode;
        public BaseMapInfo testingMapInfo;
        public Transform testingOwningCharacterTransform;
        private BaseMapInfo currentMapInfo;

        private void Update()
        {
            BaseMapInfo mapInfo = isTestMode ? testingMapInfo : BaseGameNetworkManager.CurrentMapInfo;
            if (mapInfo == null)
            {
                if (imageMinimap.gameObject.activeSelf)
                    imageMinimap.gameObject.SetActive(false);
                return;
            }
            Transform owningCharacterTransform = isTestMode ? testingOwningCharacterTransform : GameInstance.PlayingCharacterEntity.CacheTransform;
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
                if (owningCharacterMarker != null)
                    owningCharacterMarker.localPosition = new Vector3((owningCharacterTransform.position.x - currentMapInfo.MinimapPosition.x) * sizeRate, (owningCharacterTransform.position.z - currentMapInfo.MinimapPosition.z) * sizeRate, 0f);
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
