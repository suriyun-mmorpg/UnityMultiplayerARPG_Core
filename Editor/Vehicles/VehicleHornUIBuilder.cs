using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    [InitializeOnLoad]
    public static class VehicleHornUIBuilder
    {
        public const string KitPrefabPath = "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/Share/UIVehicleHorn.prefab";
        public const string WzmPrefabPath = "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIVehicleHorn.prefab";
        static VehicleHornUIBuilder() { EditorApplication.update += ProcessRequest; }

        private static void ProcessRequest()
        {
            const string request = "Temp/VehicleHornUI.Build.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            File.Delete(request);
            try { BuildAndInstall(); File.WriteAllText("Temp/VehicleHornUI.Build.result", "SUCCESS"); }
            catch (Exception ex) { File.WriteAllText("Temp/VehicleHornUI.Build.result", ex.ToString()); }
        }

        [MenuItem("Tools/MMORPG KIT/Vehicles/Create Horn UI")]
        public static void BuildAndInstall()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode before updating the horn UI.");
            bool wzm = AssetDatabase.IsValidFolder("Assets/__WZM");
            string path = wzm ? WzmPrefabPath : KitPrefabPath;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) prefab = CreatePrefab(path);
            string[] targets = wzm ? new[] { "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIGameplay_Mobile.prefab" } : new[]
            {
                "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplay.prefab",
                "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplayMobile.prefab",
            };
            foreach (string target in targets)
            {
                var root = PrefabUtility.LoadPrefabContents(target);
                try
                {
                    if (root.GetComponentInChildren<UIVehicleHorn>(true) != null) continue;
                    Transform parent = root.transform;
                    if (wzm)
                        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                            if (child.name == "CanvasMobileController") { parent = child; break; }
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                    var rect = (RectTransform)instance.transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.68f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(144f, -90f);
                    PrefabUtility.SaveAsPrefabAsset(root, target);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        private static GameObject CreatePrefab(string path)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                var root = new GameObject("UIVehicleHorn", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(root, scene);
                root.SetActive(false);
                ((RectTransform)root.transform).sizeDelta = new Vector2(68f, 68f);
                var ui = root.AddComponent<UIVehicleHorn>();
                var controls = (RectTransform)new GameObject("Controls", typeof(RectTransform)).transform;
                controls.SetParent(root.transform, false);
                controls.sizeDelta = new Vector2(68f, 68f);
                var image = controls.gameObject.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(0.2f, 0.36f, 0.45f, 1f);
                image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                image.type = UnityEngine.UI.Image.Type.Sliced;
                image.raycastTarget = true;
                var button = controls.gameObject.AddComponent<UnityEngine.UI.Button>();
                button.targetGraphic = image;
                var handler = controls.gameObject.AddComponent<UIVehicleHornPressHandler>();
                handler.ui = ui;
                var label = new GameObject("Label", typeof(RectTransform)).AddComponent<TMPro.TextMeshProUGUI>();
                label.transform.SetParent(controls, false);
                label.rectTransform.sizeDelta = new Vector2(64f, 60f);
                label.font = TMPro.TMP_Settings.defaultFontAsset;
                label.fontSize = 13f;
                label.text = "HORN\nH";
                label.alignment = TMPro.TextAlignmentOptions.Center;
                label.color = Color.white;
                label.raycastTarget = false;
                ui.controlsRoot = controls.gameObject;
                ui.buttonHorn = button;
                controls.gameObject.SetActive(false);
                root.SetActive(true);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
