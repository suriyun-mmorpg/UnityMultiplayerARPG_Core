using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiplayerARPG
{
    [InitializeOnLoad]
    public static class VehicleSeatUIBuilder
    {
        private const string _requestPath = "Temp/VehicleSeatUI.Build.request";
        private const string _resultPath = "Temp/VehicleSeatUI.Build.result";
        public const string KitPrefabPath = "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/Share/UIVehicleSeatChanger.prefab";
        public const string WzmPrefabPath = "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIVehicleSeatChanger.prefab";

        static VehicleSeatUIBuilder()
        {
            EditorApplication.update += ProcessRequest;
        }

        private static void ProcessRequest()
        {
            if (!File.Exists(_requestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            File.Delete(_requestPath);
            try
            {
                BuildAndInstall();
                File.WriteAllText(_resultPath, "SUCCESS");
            }
            catch (Exception ex)
            {
                File.WriteAllText(_resultPath, ex.ToString());
                Debug.LogException(ex);
            }
        }

        [MenuItem("Tools/MMORPG KIT/Vehicles/Create Seat Changing UI")]
        public static void BuildAndInstall()
        {
            bool wzm = AssetDatabase.IsValidFolder("Assets/__WZM");
            string path = wzm ? WzmPrefabPath : KitPrefabPath;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                prefab = CreatePrefab(path);
            string[] targets = wzm
                ? new[] { "Assets/__WZM/Prefabs/UI/v1/Gameplay/UIGameplay_Mobile.prefab" }
                : new[]
                {
                    "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplay.prefab",
                    "Assets/UnityMultiplayerARPG/Demo/Prefabs/UI/_Gameplay/CanvasGameplayMobile.prefab",
                };
            foreach (string target in targets)
                Install(prefab, target);
            AssetDatabase.SaveAssets();
        }

        public static GameObject CreatePrefab(string path)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            Scene scene = EditorSceneManager.NewPreviewScene();
            GameObject root = null;
            try
            {
                root = new GameObject("UIVehicleSeatChanger", typeof(RectTransform));
                SceneManager.MoveGameObjectToScene(root, scene);
                root.SetActive(false);
                ((RectTransform)root.transform).sizeDelta = new Vector2(196, 264);
                var changer = root.AddComponent<UIVehicleSeatChanger>();
                RectTransform controls = Rect("Controls", root.transform, new Vector2(196, 264), Vector2.zero);
                Image(controls, new Color(0.035f, 0.055f, 0.08f, 0.88f), true);
                Label("Title", controls, "CHANGE SEAT", 18, new Vector2(180, 28), new Vector2(0, 114), Color.white);
                RectTransform body = Rect("Vehicle", controls, new Vector2(150, 204), new Vector2(0, -2));
                Image(body, new Color(0.12f, 0.17f, 0.23f), false);
                var outline = body.gameObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor = new Color(0.62f, 0.72f, 0.8f, 0.7f);
                outline.effectDistance = new Vector2(2, -2);
                foreach (float x in new[] { -83f, 83f })
                    foreach (float y in new[] { -68f, 66f })
                        Image(Rect("Wheel", controls, new Vector2(10, 30), new Vector2(x, y)),
                            new Color(0.38f, 0.46f, 0.55f), false);
                Image(Rect("Windshield", body, new Vector2(112, 6), new Vector2(0, 90)),
                    new Color(0.45f, 0.66f, 0.8f), false);
                RectTransform container = Rect("Seats", controls, new Vector2(156, 194), new Vector2(0, -2));
                Label("Hint", controls, "SWIPE OR TAP", 13, new Vector2(180, 22), new Vector2(0, -119),
                    new Color(0.7f, 0.78f, 0.85f));
                RectTransform template = Rect("Seat template", root.transform, new Vector2(48, 52), Vector2.zero);
                var seat = template.gameObject.AddComponent<UIVehicleSeat>();
                seat.background = Image(template, Color.white, true);
                seat.button = template.gameObject.GetComponent<UnityEngine.UI.Button>();
                seat.button.targetGraphic = seat.background;
                seat.button.transition = UnityEngine.UI.Selectable.Transition.None;
                seat.textSeatNumber = Label("Number", template, "1", 25, new Vector2(44, 36), new Vector2(0, 5),
                    new Color(0.065f, 0.1f, 0.14f));
                seat.driverIndicator = Label("Driver", template, "DRIVER", 8, new Vector2(46, 12),
                    new Vector2(0, -17), new Color(0.065f, 0.1f, 0.14f)).gameObject;
                changer.controlsRoot = controls.gameObject;
                changer.seatsContainer = container;
                changer.seatPrefab = seat;
                changer.seatLayoutSize = new Vector2(88, 128);
                template.gameObject.SetActive(false);
                controls.gameObject.SetActive(false);
                root.SetActive(true);
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void Install(GameObject prefab, string path)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
                return;
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                if (root.GetComponentInChildren<UIVehicleSeatChanger>(true) != null)
                    return;
                UnityEngine.Canvas canvas = root.GetComponent<UnityEngine.Canvas>();
                if (canvas == null)
                    canvas = root.GetComponentInChildren<UnityEngine.Canvas>(true);
                if (canvas == null)
                    throw new InvalidOperationException("No Canvas in " + path);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas.transform);
                var rect = (RectTransform)instance.transform;
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.68f);
                rect.anchoredPosition = Vector2.zero;
                if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
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

        private static UnityEngine.UI.Image Image(RectTransform rect, Color color, bool raycast)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        private static TMPro.TextMeshProUGUI Label(string name, Transform parent, string text, float size,
            Vector2 dimensions, Vector2 position, Color color)
        {
            RectTransform rect = Rect(name, parent, dimensions, position);
            var label = rect.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            label.font = TMPro.TMP_Settings.defaultFontAsset;
            label.text = text;
            label.fontSize = size;
            label.alignment = TMPro.TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; ++i)
            {
                string child = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(child))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = child;
            }
        }
    }
}
