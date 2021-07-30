using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class GameDataCreatorEditor : EditorWindow
    {
        public static string[] assemblyNames = new string[] { "Assembly-CSharp" };
        public GameDatabase workingDatabase { get; set; }
        public Type workingFieldType { get; set; }
        public string workingFieldName { get; set; }
        public ScriptableObject[] workingArray { get; set; }

        public static void CreateNewEditor(GameDatabase workingDatabase, Type workingFieldType, string workingFieldName, ScriptableObject[] workingArray)
        {
            GameDataCreatorEditor window = GetWindow<GameDataCreatorEditor>();
            window.workingDatabase = workingDatabase;
            window.workingFieldType = workingFieldType;
            window.workingFieldName = workingFieldName;
            window.workingArray = workingArray;
        }

        private Vector2 scrollViewPosition = Vector2.zero;

        List<Type> FindTypes(string name)
        {
            List<Type> types = new List<Type>();
            try
            {
                // get project assembly
                Assembly asm = Assembly.Load(new AssemblyName(name));

                // filter out all the ScriptableObject types
                foreach (Type t in asm.GetTypes())
                {
                    if ((t == workingFieldType || t.IsSubclassOf(workingFieldType)) && !t.IsAbstract)
                        types.Add(t);
                }
            }
            catch { }

            return types;
        }

        void OnGUI()
        {
            GUILayout.Label("Select the type to create:");
            scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition, false, false);
            foreach (string assemblyName in assemblyNames)
            {
                foreach (Type t in FindTypes(assemblyName))
                {
                    if (GUILayout.Button(t.FullName))
                    {
                        // create the asset, select it, allow renaming, close
                        ScriptableObject newData = CreateInstance(t);
                        string savedPath = EditorUtility.SaveFilePanel("Save asset", "Assets", t.Name + ".asset", "asset");
                        if (!string.IsNullOrEmpty(savedPath))
                        {
                            savedPath = savedPath.Substring(savedPath.IndexOf("Assets"));
                            AssetDatabase.CreateAsset(newData, savedPath);
                            List<ScriptableObject> appending = new List<ScriptableObject>(workingArray);
                            appending.Add(AssetDatabase.LoadAssetAtPath<ScriptableObject>(savedPath));
                            Array newArray = Array.CreateInstance(workingFieldType, appending.Count);
                            for (int i = 0; i < newArray.Length; ++i)
                            {
                                newArray.SetValue(appending[i], i);
                            }
                            workingDatabase.GetType().GetField(workingFieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(workingDatabase, newArray);
                            EditorUtility.SetDirty(workingDatabase);
                            Close();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }
}
