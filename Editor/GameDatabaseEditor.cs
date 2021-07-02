using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameDatabaseEditor : EditorWindow
    {
        private List<BaseGameDatabaseSection> sections;
        private Vector2 menuScrollPosition;
        private int selectedMenuIndex;
        private GameDatabase selectedDatabase;

        [MenuItem("MMORPG KIT/Game Database", false, 0)]
        public static void CreateNewEditor()
        {
            GetWindow<GameDatabaseEditor>();
        }

        private void OnEnable()
        {
            selectedDatabase = null;
            sections = new List<BaseGameDatabaseSection>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; ++i)
            {
                Type[] assemblyTypes;
                try
                {
                    assemblyTypes = assemblies[i].GetTypes();
                }
                catch
                {
                    continue;
                }
                for (int j = 0; j < assemblyTypes.Length; ++j)
                {
                    if (!typeof(BaseGameDatabaseSection).IsAssignableFrom(assemblyTypes[j]))
                        continue;

                    if (assemblyTypes[j].IsAbstract)
                        continue;

                    sections.Add(Activator.CreateInstance(assemblyTypes[j]) as BaseGameDatabaseSection);
                }
            }
            sections.Sort();
        }

        private void OnDisable()
        {
            EditorGlobalData.WorkingDatabase = null;
        }

        private void OnGUI()
        {
            titleContent = new GUIContent("Game Database", null, "Game Database");
            if (EditorGlobalData.WorkingDatabase == null)
            {
                Vector2 wndRect = new Vector2(500, 100);
                minSize = wndRect;

                GUILayout.BeginVertical("Game Database", "window");
                {
                    GUILayout.BeginVertical("box");
                    {
                        if (EditorGlobalData.WorkingDatabase == null)
                            EditorGUILayout.HelpBox("Select your game database which you want to manage", MessageType.Info);
                        EditorGlobalData.WorkingDatabase = EditorGUILayout.ObjectField("Game database", EditorGlobalData.WorkingDatabase, typeof(GameDatabase), true, GUILayout.ExpandWidth(true)) as GameDatabase;
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndVertical();
                return;
            }
            else
            {
                Vector2 wndRect = new Vector2(500, 500);
                minSize = wndRect;
            }

            if (EditorGlobalData.WorkingDatabase != selectedDatabase)
            {
                selectedDatabase = EditorGlobalData.WorkingDatabase;
                selectedDatabase.LoadReferredData();
            }

            // Prepare GUI styles
            GUIStyle leftMenuButton = new GUIStyle(EditorStyles.label);
            leftMenuButton.fontSize = 10;
            leftMenuButton.alignment = TextAnchor.MiddleRight;

            GUIStyle selectedLeftMenuButton = new GUIStyle(EditorStyles.label);
            selectedLeftMenuButton.fontSize = 10;
            selectedLeftMenuButton.alignment = TextAnchor.MiddleRight;
            var background = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            background.SetPixel(0, 0, EditorGUIUtility.isProSkin ? new Color(0.243f, 0.373f, 0.588f) : new Color(0.247f, 0.494f, 0.871f));
            background.Apply();
            selectedLeftMenuButton.active.background = selectedLeftMenuButton.normal.background = background;

            // Left menu
            GUILayout.BeginArea(new Rect(0, 0, 120, position.height), string.Empty, "box");
            {
                menuScrollPosition = GUILayout.BeginScrollView(menuScrollPosition);
                GUILayout.BeginVertical();
                {
                    for (int i = 0; i < sections.Count; ++i)
                    {
                        if (GUILayout.Button(sections[i].MenuTitle, (i == selectedMenuIndex ? selectedLeftMenuButton : leftMenuButton), GUILayout.Height(32)))
                        {
                            selectedMenuIndex = i;
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();

            // Content
            GUILayout.BeginArea(new Rect(120, 0, position.width - 120, position.height), string.Empty);
            {
                sections[selectedMenuIndex].OnGUI(position.width - 120, position.height);
            }
            GUILayout.EndArea();
        }
    }
}
