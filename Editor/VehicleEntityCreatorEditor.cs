using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.AI;
using StandardAssets.Characters.Physics;
using LiteNetLibManager;
using MultiplayerARPG.GameData.Model.Playables;

namespace MultiplayerARPG
{
    public class VehicleEntityCreatorEditor : EditorWindow
    {
        public enum CharacterModelType
        {
            PlayableCharacterModel,
            AnimatorCharacterModel,
            AnimationCharacterModel,
        }

        public enum EntityMovementType
        {
            CharacterController,
            NavMesh,
            Rigidbody,
        }

        private string fileName;
        private CharacterModelType characterModelType;
        private EntityMovementType entityMovementType;
        private GameDatabase gameDatabase;
        private GameObject fbx;

        [MenuItem("MMORPG KIT/Vehicle Entity Creator (3D)", false, 103)]
        public static void CreateNewVehicleEntity()
        {
            bool gettingWindow;
            if (EditorGlobalData.EditorScene.HasValue)
            {
                gettingWindow = true;
                EditorSceneManager.CloseScene(EditorGlobalData.EditorScene.Value, true);
                EditorGlobalData.EditorScene = null;
            }
            else
            {
                gettingWindow = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }
            if (gettingWindow)
            {
                EditorGlobalData.EditorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                GetWindow<VehicleEntityCreatorEditor>();
            }
        }

        private void OnGUI()
        {
            Vector2 wndRect = new Vector2(500, 500);
            maxSize = wndRect;
            minSize = wndRect;
            titleContent = new GUIContent("Vehicle Entity", null, "Vehicle Entity Creator (3D)");
            GUILayout.BeginVertical("Vehicle Entity Creator", "window");
            {
                GUILayout.BeginVertical("box");
                {
                    fileName = EditorGUILayout.TextField("Filename", fileName);
                    characterModelType = (CharacterModelType)EditorGUILayout.EnumPopup("Character model type", characterModelType);
                    entityMovementType = (EntityMovementType)EditorGUILayout.EnumPopup("Entity movement type", entityMovementType);
                    if (gameDatabase == null)
                        EditorGUILayout.HelpBox("Select your game database which you want to add new vehicle entity, leave it `None` if you don't want to add vehicle entity to game database", MessageType.Info);
                    gameDatabase = EditorGUILayout.ObjectField("Game database", gameDatabase, typeof(GameDatabase), false, GUILayout.ExpandWidth(true)) as GameDatabase;
                    if (fbx == null)
                        EditorGUILayout.HelpBox("Select your FBX model which you want to create character entity", MessageType.Info);
                    fbx = EditorGUILayout.ObjectField("FBX", fbx, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
                }
                GUILayout.EndVertical();

                if (fbx != null && !string.IsNullOrEmpty(fileName))
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Create", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
                        Create();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void Create()
        {
            var path = EditorUtility.SaveFolderPanel("Save data to folder", "Assets", "");
            path = path.Substring(path.IndexOf("Assets"));

            var newObject = Instantiate(fbx, Vector3.zero, Quaternion.identity);
            newObject.AddComponent<LiteNetLibIdentity>();

            Animator animator;
            BaseCharacterModel characterModel = null;
            switch (characterModelType)
            {
                case CharacterModelType.PlayableCharacterModel:
                    characterModel = newObject.AddComponent<PlayableCharacterModel>();
                    animator = newObject.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        Debug.LogError("Cannot create new entity with `PlayableCharacterModel`, can't find `Animator` component");
                        DestroyImmediate(newObject);
                        return;
                    }
                    (characterModel as PlayableCharacterModel).animator = animator;
                    break;
                case CharacterModelType.AnimatorCharacterModel:
                    characterModel = newObject.AddComponent<AnimatorCharacterModel>();
                    animator = newObject.GetComponentInChildren<Animator>();
                    if (animator == null)
                    {
                        Debug.LogError("Cannot create new entity with `AnimatorCharacterModel`, can't find `Animator` component");
                        DestroyImmediate(newObject);
                        return;
                    }
                    (characterModel as AnimatorCharacterModel).animator = animator;
                    break;
                case CharacterModelType.AnimationCharacterModel:
                    characterModel = newObject.AddComponent<AnimationCharacterModel>();
                    var animation = newObject.GetComponentInChildren<Animation>();
                    if (animation == null)
                    {
                        Debug.LogError("Cannot create new entity with `AnimationCharacterModel`, can't find `Animation` component");
                        DestroyImmediate(newObject);
                        return;
                    }
                    (characterModel as AnimationCharacterModel).legacyAnimation = animation;
                    break;
            }
            
            Bounds bounds = default;
            var meshes = newObject.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < meshes.Length; ++i)
            {
                if (i > 0)
                    bounds.Encapsulate(meshes[i].bounds);
                else
                    bounds = meshes[i].bounds;
            }

            var skinnedMeshes = newObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            for (int i = 0; i < skinnedMeshes.Length; ++i)
            {
                if (i > 0)
                    bounds.Encapsulate(skinnedMeshes[i].bounds);
                else
                    bounds = skinnedMeshes[i].bounds;
            }

            switch (entityMovementType)
            {
                case EntityMovementType.CharacterController:
                    var characterController = newObject.AddComponent<CharacterController>();
                    characterController.height = bounds.size.y;
                    characterController.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    characterController.center = Vector3.zero + (Vector3.up * characterController.height * 0.5f);
                    newObject.AddComponent<CharacterControllerEntityMovement>();
                    break;
                case EntityMovementType.NavMesh:
                    var navMeshAgent = newObject.AddComponent<NavMeshAgent>();
                    navMeshAgent.height = bounds.size.y;
                    navMeshAgent.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    newObject.AddComponent<NavMeshEntityMovement>();
                    break;
                case EntityMovementType.Rigidbody:
                    newObject.AddComponent<Rigidbody>();
                    var capsuleCollider = newObject.AddComponent<CapsuleCollider>();
                    capsuleCollider.height = bounds.size.y;
                    capsuleCollider.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    capsuleCollider.center = Vector3.zero + (Vector3.up * capsuleCollider.height * 0.5f);
                    var openCharacterController = newObject.AddComponent<OpenCharacterController>();
                    openCharacterController.SetRadiusHeightAndCenter(capsuleCollider.radius, capsuleCollider.height, capsuleCollider.center, false, false);
                    newObject.AddComponent<RigidBodyEntityMovement>();
                    break;
            }

            VehicleEntity baseVehicleEntity = newObject.AddComponent<VehicleEntity>();
            if (baseVehicleEntity != null)
            {
                var tpsCamTarget = new GameObject("_TpsCamTarget");
                tpsCamTarget.transform.parent = baseVehicleEntity.transform;
                tpsCamTarget.transform.localPosition = Vector3.zero;
                tpsCamTarget.transform.localRotation = Quaternion.identity;
                tpsCamTarget.transform.localScale = Vector3.one;
                baseVehicleEntity.CameraTargetTransform = tpsCamTarget.transform;

                var fpsCamTarget = new GameObject("_FpsCamTarget");
                fpsCamTarget.transform.parent = baseVehicleEntity.transform;
                fpsCamTarget.transform.localPosition = Vector3.zero;
                fpsCamTarget.transform.localRotation = Quaternion.identity;
                fpsCamTarget.transform.localScale = Vector3.one;
                baseVehicleEntity.FpsCameraTargetTransform = fpsCamTarget.transform;

                var combatTextObj = new GameObject("_CombatText");
                combatTextObj.transform.parent = baseVehicleEntity.transform;
                combatTextObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                combatTextObj.transform.localRotation = Quaternion.identity;
                combatTextObj.transform.localScale = Vector3.one;
                baseVehicleEntity.CombatTextTransform = combatTextObj.transform;

                var opponentAimObj = new GameObject("_OpponentAim");
                opponentAimObj.transform.parent = baseVehicleEntity.transform;
                opponentAimObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                opponentAimObj.transform.localRotation = Quaternion.identity;
                opponentAimObj.transform.localScale = Vector3.one;
                baseVehicleEntity.OpponentAimTransform = opponentAimObj.transform;

                var savePath = path + "\\" + fileName + ".prefab";
                Debug.Log("Saving character entity to " + savePath);
                AssetDatabase.DeleteAsset(savePath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(baseVehicleEntity.gameObject, savePath, InteractionMode.AutomatedAction);

                if (gameDatabase != null)
                {
                    GameObject savedObject = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
                    VehicleEntity savedEntity = savedObject.GetComponent<VehicleEntity>();
                    if (savedEntity is VehicleEntity)
                    {
                        List<VehicleEntity> list = new List<VehicleEntity>(gameDatabase.vehicleEntities);
                        list.Add(savedEntity as VehicleEntity);
                        gameDatabase.vehicleEntities = list.ToArray();
                    }
                    EditorUtility.SetDirty(gameDatabase);
                }
            }
        }
    }
}
