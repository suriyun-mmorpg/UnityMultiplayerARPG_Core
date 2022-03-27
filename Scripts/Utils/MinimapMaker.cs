using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class MinimapMaker : MonoBehaviour
    {
        public const int TEXTURE_WIDTH = 1024;
        public const int TEXTURE_HEIGHT = 1024;
        public const int TEXTURE_DEPTH = 24;
        public const float SPRITE_PIXELS_PER_UNIT = 100f;
        public BaseMapInfo targetMapInfo;
        public DimensionType dimensionType;
        public LayerMask cullingMask = ~0;
        public Color clearFlagsBackgroundColor = Color.black;
        public string minimapSuffix = "_minimap";
        [StringShowConditional(nameof(dimensionType), nameof(DimensionType.Dimension3D))]
        public float cameraYPosition = 50f;
        [StringShowConditional(nameof(dimensionType), nameof(DimensionType.Dimension2D))]
        public float cameraZPosition = -1f;
        public bool makeByTerrain = false;
        public bool makeByCollider = true;
        public bool makeByCollider2D = true;
        public bool makeByRenderer = false;
#if UNITY_EDITOR
        [InspectorButton(nameof(Make))]
        public bool make;
#endif

#if UNITY_EDITOR
        [ContextMenu("Make")]
        public void Make()
        {
            // Find bounds
            Bounds bounds = default;
            bool setBoundsOnce = false;
            if (makeByTerrain)
            {
                TerrainCollider[] objects = FindObjectsOfType<TerrainCollider>();
                foreach (var obj in objects)
                {
                    if (!setBoundsOnce)
                        bounds = obj.bounds;
                    else
                        bounds.Encapsulate(obj.bounds);
                    setBoundsOnce = true;
                }
            }
            if (makeByCollider)
            {
                Collider[] objects = FindObjectsOfType<Collider>();
                foreach (var obj in objects)
                {
                    if (obj is TerrainCollider)
                        continue;
                    if (!setBoundsOnce)
                        bounds = obj.bounds;
                    else
                        bounds.Encapsulate(obj.bounds);
                    setBoundsOnce = true;
                }
            }
            if (makeByCollider2D)
            {
                Collider2D[] objects = FindObjectsOfType<Collider2D>();
                foreach (var obj in objects)
                {
                    if (!setBoundsOnce)
                        bounds = obj.bounds;
                    else
                        bounds.Encapsulate(obj.bounds);
                    setBoundsOnce = true;
                }
            }
            if (makeByRenderer)
            {
                Renderer[] objects = FindObjectsOfType<Renderer>();
                foreach (var obj in objects)
                {
                    if (!setBoundsOnce)
                        bounds = obj.bounds;
                    else
                        bounds.Encapsulate(obj.bounds);
                    setBoundsOnce = true;
                }
            }

            // Create camera
            GameObject cameraGameObject = new GameObject("_MinimapMakerCamera");
            Camera camera = cameraGameObject.AddComponent<Camera>();
            switch (dimensionType)
            {
                case DimensionType.Dimension2D:
                    camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, cameraZPosition);
                    camera.transform.eulerAngles = Vector3.zero;
                    camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y);
                    break;
                default:
                    camera.transform.position = new Vector3(bounds.center.x, cameraYPosition, bounds.center.z);
                    camera.transform.eulerAngles = new Vector3(90f, 0f, 0f);
                    camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
                    break;
            }
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = clearFlagsBackgroundColor;
            camera.orthographic = true;
            camera.cullingMask = cullingMask.value;

            // Make texture
            RenderTexture renderTexture = new RenderTexture(TEXTURE_WIDTH, TEXTURE_HEIGHT, TEXTURE_DEPTH);
            Rect rect = new Rect(0, 0, TEXTURE_WIDTH, TEXTURE_HEIGHT);
            Texture2D texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBA32, false);

            camera.targetTexture = renderTexture;
            camera.Render();

            // Switch render texture to apply pixel to texture
            RenderTexture currentRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            // Switch render texture back
            camera.targetTexture = null;
            RenderTexture.active = currentRenderTexture;

            // Save texture
            string path;
            if (targetMapInfo != null)
            {
                path = AssetDatabase.GetAssetPath(targetMapInfo);
                path = path.Substring(0, path.Length - ".asset".Length);
                path += minimapSuffix + ".png";
            }
            else
            {
                path = EditorUtility.SaveFilePanel("Save texture as", "Assets", "minimap", "png");
            }
            Debug.Log("Saving character data to " + path);
            AssetDatabase.DeleteAsset(path);
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
                File.WriteAllBytes(path, pngData);
            AssetDatabase.Refresh();

            TextureImporter tempTextureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            tempTextureImporter.textureType = TextureImporterType.Sprite;
            tempTextureImporter.spriteImportMode = SpriteImportMode.Single;
            tempTextureImporter.spritePivot = Vector2.one * 0.5f;
            tempTextureImporter.spritePixelsPerUnit = SPRITE_PIXELS_PER_UNIT;
            EditorUtility.SetDirty(tempTextureImporter);
            tempTextureImporter.SaveAndReimport();
            var tempSprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (targetMapInfo != null)
            {
                targetMapInfo.MinimapSprite = tempSprite;
                switch (dimensionType)
                {
                    case DimensionType.Dimension2D:
                        targetMapInfo.MinimapPosition = new Vector3(bounds.center.x, bounds.center.y, 0);
                        targetMapInfo.MinimapBoundsWidth = bounds.size.x;
                        targetMapInfo.MinimapBoundsLength = bounds.size.y;
                        targetMapInfo.MinimapOrthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y);
                        break;
                    default:
                        targetMapInfo.MinimapPosition = new Vector3(bounds.center.x, 0, bounds.center.z);
                        targetMapInfo.MinimapBoundsWidth = bounds.size.x;
                        targetMapInfo.MinimapBoundsLength = bounds.size.z;
                        targetMapInfo.MinimapOrthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
                        break;
                }
                EditorUtility.SetDirty(targetMapInfo);
            }

            DestroyImmediate(texture);
            DestroyImmediate(renderTexture);
            DestroyImmediate(cameraGameObject);
        }
#endif
    }
}
