using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class MinimapMaker : MonoBehaviour
    {
        public BaseMapInfo targetMapInfo;
        public string minimapSuffix = "_minimap";
        public int textureWidth = 1024;
        public int textureHeight = 1024;
        public int textureDepth = 24;
        public float yRotation = 0f;
        public float yPosition = 50f;
        public bool makeByTerrain = false;
        public bool makeByCollider = true;
        public bool makeByCollider2D = false;
        public bool makeByRenderer = false;

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
            camera.transform.position = new Vector3(bounds.center.x, yPosition, bounds.center.z);
            camera.transform.eulerAngles = new Vector3(90, yRotation, 0f);
            camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
            camera.orthographic = true;

            // Make texture
            RenderTexture renderTexture = new RenderTexture(textureWidth, textureHeight, textureDepth);
            Rect rect = new Rect(0, 0, textureWidth, textureHeight);
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

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

            var savedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (targetMapInfo != null)
            {
                targetMapInfo.MinimapTexture = savedTexture;
                targetMapInfo.MinimapPosition = new Vector3(bounds.center.x, 0, bounds.center.z);
                targetMapInfo.MinimapRotation = new Vector3(90, yRotation, 0f);
                targetMapInfo.MinimapOrthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.z);
                EditorUtility.SetDirty(targetMapInfo);
            }

            DestroyImmediate(texture);
            DestroyImmediate(renderTexture);
            DestroyImmediate(cameraGameObject);
        }
    }
}
