using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    [InitializeOnLoad]
    public static class VehicleFuelUIBuilder
    {
        public const string KitPrefabPath = "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/Share/UIVehicleFuel.prefab";
        public const string WzmPrefabPath = "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIVehicleFuel.prefab";

        static VehicleFuelUIBuilder() { EditorApplication.update += ProcessRequest; }

        private static void ProcessRequest()
        {
            const string request = "Temp/VehicleFuelUI.Build.request";
            if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            File.Delete(request);
            try
            {
                BuildAndInstall();
                File.WriteAllText("Temp/VehicleFuelUI.Build.result", "SUCCESS");
            }
            catch (Exception ex)
            {
                File.WriteAllText("Temp/VehicleFuelUI.Build.result", ex.ToString());
                Debug.LogException(ex);
            }
        }

        [MenuItem("Tools/MMORPG KIT/Vehicles/Create Fuel UI")]
        public static void BuildAndInstall()
        {
            bool wzm = AssetDatabase.IsValidFolder("Assets/__WZM");
            string path = wzm ? WzmPrefabPath : KitPrefabPath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                prefab = CreatePrefab(path);
            string[] targets = wzm ? new[] { "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIGameplay_Mobile.prefab" } : new[]
            {
                "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplay.prefab",
                "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplayMobile.prefab",
            };
            foreach (string target in targets)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(target);
                try
                {
                    if (root.GetComponentInChildren<UIVehicleFuel>(true) != null)
                        continue;
                    Transform parent = root.transform;
                    if (wzm)
                    {
                        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                            if (child.name == "CanvasMobileController") { parent = child; break; }
                    }
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
                    var rect = (RectTransform)instance.transform;
                    rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.68f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = new Vector2(0f, -190f);
                    PrefabUtility.SaveAsPrefabAsset(root, target);
                }
                finally { PrefabUtility.UnloadPrefabContents(root); }
            }
            AssetDatabase.SaveAssets();
        }

        public static GameObject CreatePrefab(string path)
        {
            Scene scene = EditorSceneManager.NewPreviewScene();
            try
            {
                GameObject root = new GameObject("UIVehicleFuel", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(root, scene);
                root.SetActive(false);
                ((RectTransform)root.transform).sizeDelta = new Vector2(196f, 90f);
                UIVehicleFuel ui = root.AddComponent<UIVehicleFuel>();
                RectTransform controls = Rect("Controls", root.transform, new Vector2(196f, 90f), Vector2.zero);
                Image(controls, new Color(0.035f, 0.055f, 0.08f, 0.88f));
                ui.controlsRoot = controls.gameObject;
                ui.textFuel = Label("Fuel", controls, "FUEL", new Vector2(180f, 24f), new Vector2(0f, 26f));
                RectTransform bar = Rect("Fuel bar", controls, new Vector2(172f, 12f), new Vector2(0f, 3f));
                Image(bar, new Color(0.18f, 0.24f, 0.29f));
                RectTransform fill = Rect("Fill", bar, Vector2.zero, Vector2.zero);
                fill.anchorMin = Vector2.zero;
                fill.anchorMax = Vector2.one;
                ui.fuelFill = Image(fill, ui.normalColor);
                ui.fuelFill.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                ui.fuelFill.type = UnityEngine.UI.Image.Type.Filled;
                ui.fuelFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
                RectTransform button = Rect("Refuel", controls, new Vector2(172f, 26f), new Vector2(0f, -28f));
                var buttonImage = Image(button, new Color(0.2f, 0.36f, 0.45f));
                buttonImage.raycastTarget = true;
                ui.buttonRefuel = button.gameObject.AddComponent<UnityEngine.UI.Button>();
                ui.buttonRefuel.targetGraphic = buttonImage;
                Label("Label", button, "REFUEL (CAN)", new Vector2(168f, 24f), Vector2.zero);
                controls.gameObject.SetActive(false);
                root.SetActive(true);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 size, Vector2 position)
        {
            var rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            return rect;
        }

        private static UnityEngine.UI.Image Image(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextWrapper Label(string name, Transform parent, string text, Vector2 size, Vector2 position)
        {
            var label = Rect(name, parent, size, position).gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            label.font = TMPro.TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = 14f;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;
            var wrapper = label.gameObject.AddComponent<TextWrapper>();
            wrapper.textMeshText = label;
            return wrapper;
        }
    }
}
