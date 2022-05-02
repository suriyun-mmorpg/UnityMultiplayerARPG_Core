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
    public class CharacterEntityCreatorEditor : EditorWindow
    {
        public enum CharacterEntityType
        {
            PlayerCharacterEntity,
            MonsterCharacterEntity,
        }

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
        private string dataId;
        private CharacterEntityType characterEntityType;
        private CharacterModelType characterModelType;
        private EntityMovementType entityMovementType;
        private GameDatabase gameDatabase;
        private GameObject fbx;

        [MenuItem(EditorMenuConsts.CHARACTER_ENTITY_CREATOR_MENU, false, EditorMenuConsts.CHARACTER_ENTITY_CREATOR_ORDER)]
        public static void CreateNewCharacterEntity()
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
                GetWindow<CharacterEntityCreatorEditor>();
            }
        }

        private void OnGUI()
        {
            Vector2 wndRect = new Vector2(500, 500);
            maxSize = wndRect;
            minSize = wndRect;
            titleContent = new GUIContent("Character Entity", null, "Character Entity Creator (3D)");
            GUILayout.BeginVertical("Character Entity Creator", "window");
            {
                GUILayout.BeginVertical("box");
                {
                    fileName = EditorGUILayout.TextField("Filename", fileName);
                    characterEntityType = (CharacterEntityType)EditorGUILayout.EnumPopup("Character entity type", characterEntityType);
                    characterModelType = (CharacterModelType)EditorGUILayout.EnumPopup("Character model type", characterModelType);
                    entityMovementType = (EntityMovementType)EditorGUILayout.EnumPopup("Entity movement type", entityMovementType);
                    if (gameDatabase == null)
                        EditorGUILayout.HelpBox("Select your game database which you want to add new character entity, leave it `None` if you don't want to add character entity to game database", MessageType.Info);
                    gameDatabase = EditorGUILayout.ObjectField("Game database", gameDatabase, typeof(GameDatabase), false, GUILayout.ExpandWidth(true)) as GameDatabase;
                    if (fbx == null)
                        EditorGUILayout.HelpBox("Select your FBX model which you want to create character entity", MessageType.Info);
                    fbx = EditorGUILayout.ObjectField("FBX", fbx, typeof(GameObject), false, GUILayout.ExpandWidth(true)) as GameObject;
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                {
                    EditorGUILayout.HelpBox("Leave `Character Data ID` to be empty to NOT create character data for this entity", MessageType.Info);
                    dataId = EditorGUILayout.TextField("Character Data ID", dataId);
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
            newObject.AddComponent<CharacterRecoveryComponent>();
            newObject.AddComponent<CharacterSkillAndBuffComponent>();

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

            var characterModelManager = newObject.AddComponent<CharacterModelManager>();
            characterModelManager.MainTpsModel = characterModel;

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
            CharacterController characterController;
            CapsuleCollider capsuleCollider;
            NavMeshAgent navMeshAgent;
            switch (entityMovementType)
            {
                case EntityMovementType.CharacterController:
                    characterController = newObject.AddComponent<CharacterController>();
                    characterController.height = bounds.size.y;
                    characterController.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    characterController.center = Vector3.zero + (Vector3.up * characterController.height * 0.5f);
                    newObject.AddComponent<CharacterControllerEntityMovement>();
                    break;
                case EntityMovementType.NavMesh:
                    capsuleCollider = newObject.AddComponent<CapsuleCollider>();
                    capsuleCollider.height = bounds.size.y;
                    capsuleCollider.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    capsuleCollider.center = Vector3.zero + (Vector3.up * capsuleCollider.height * 0.5f);
                    capsuleCollider.isTrigger = true;
                    navMeshAgent = newObject.AddComponent<NavMeshAgent>();
                    navMeshAgent.height = bounds.size.y;
                    navMeshAgent.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    newObject.AddComponent<NavMeshEntityMovement>();
                    break;
                case EntityMovementType.Rigidbody:
                    newObject.AddComponent<Rigidbody>();
                    capsuleCollider = newObject.AddComponent<CapsuleCollider>();
                    capsuleCollider.height = bounds.size.y;
                    capsuleCollider.radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
                    capsuleCollider.center = Vector3.zero + (Vector3.up * capsuleCollider.height * 0.5f);
                    var openCharacterController = newObject.AddComponent<OpenCharacterController>();
                    openCharacterController.SetRadiusHeightAndCenter(capsuleCollider.radius, capsuleCollider.height, capsuleCollider.center, false, false);
                    newObject.AddComponent<RigidBodyEntityMovement>();
                    break;
            }

            BaseCharacterEntity baseCharacterEntity = null;
            switch (characterEntityType)
            {
                case CharacterEntityType.PlayerCharacterEntity:
                    newObject.AddComponent<PlayerCharacterCraftingComponent>();
                    newObject.AddComponent<PlayerCharacterItemLockAndExpireComponent>();
                    PlayerCharacterEntity playerCharacterEntity = newObject.AddComponent<PlayerCharacterEntity>();
                    baseCharacterEntity = playerCharacterEntity;
                    if (!string.IsNullOrEmpty(dataId))
                    {
                        PlayerCharacter data = CreateInstance<PlayerCharacter>();
                        data.Id = dataId;
                        data.Stats = new CharacterStatsIncremental()
                        {
                            baseStats = new CharacterStats()
                            {
                                hp = 100,
                                moveSpeed = 5,
                                atkSpeed = 1,
                            }
                        };
                        var dataSavePath = path + "\\" + fileName + "_PlayerCharacter.asset";
                        Debug.Log("Saving character data to " + dataSavePath);
                        AssetDatabase.DeleteAsset(dataSavePath);
                        AssetDatabase.CreateAsset(data, dataSavePath);
                        PlayerCharacter savedData = AssetDatabase.LoadAssetAtPath<PlayerCharacter>(dataSavePath);
                        playerCharacterEntity.CharacterDatabases = new PlayerCharacter[]
                        {
                            savedData
                        };
                        if (gameDatabase != null)
                        {
                            List<PlayerCharacter> list = new List<PlayerCharacter>(gameDatabase.playerCharacters);
                            list.Add(savedData);
                            gameDatabase.playerCharacters = list.ToArray();
                        }
                    }
                    break;
                case CharacterEntityType.MonsterCharacterEntity:
                    newObject.AddComponent<MonsterActivityComponent>();
                    MonsterCharacterEntity monsterCharacterEntity = newObject.AddComponent<MonsterCharacterEntity>();
                    baseCharacterEntity = monsterCharacterEntity;
                    if (!string.IsNullOrEmpty(dataId))
                    {
                        MonsterCharacter data = CreateInstance<MonsterCharacter>();
                        data.Id = dataId;
                        data.Stats = new CharacterStatsIncremental()
                        {
                            baseStats = new CharacterStats()
                            {
                                hp = 100,
                                moveSpeed = 5,
                                atkSpeed = 1,
                            }
                        };
                        var dataSavePath = path + "\\" + fileName + "_MonsterCharacter.asset";
                        Debug.Log("Saving character data to " + dataSavePath);
                        AssetDatabase.DeleteAsset(dataSavePath);
                        AssetDatabase.CreateAsset(data, dataSavePath);
                        MonsterCharacter savedData = AssetDatabase.LoadAssetAtPath<MonsterCharacter>(dataSavePath);
                        monsterCharacterEntity.CharacterDatabase = savedData;
                        if (gameDatabase != null)
                        {
                            List<MonsterCharacter> list = new List<MonsterCharacter>(gameDatabase.monsterCharacters);
                            list.Add(savedData);
                            gameDatabase.monsterCharacters = list.ToArray();
                        }
                    }
                    break;
            }

            if (baseCharacterEntity != null)
            {
                var tpsCamTarget = new GameObject("_TpsCamTarget");
                tpsCamTarget.transform.parent = baseCharacterEntity.transform;
                tpsCamTarget.transform.localPosition = Vector3.zero;
                tpsCamTarget.transform.localRotation = Quaternion.identity;
                tpsCamTarget.transform.localScale = Vector3.one;
                baseCharacterEntity.CameraTargetTransform = tpsCamTarget.transform;

                var fpsCamTarget = new GameObject("_FpsCamTarget");
                fpsCamTarget.transform.parent = baseCharacterEntity.transform;
                fpsCamTarget.transform.localPosition = Vector3.zero;
                fpsCamTarget.transform.localRotation = Quaternion.identity;
                fpsCamTarget.transform.localScale = Vector3.one;
                baseCharacterEntity.FpsCameraTargetTransform = fpsCamTarget.transform;

                var combatTextObj = new GameObject("_CombatText");
                combatTextObj.transform.parent = baseCharacterEntity.transform;
                combatTextObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                combatTextObj.transform.localRotation = Quaternion.identity;
                combatTextObj.transform.localScale = Vector3.one;
                baseCharacterEntity.CombatTextTransform = combatTextObj.transform;

                var opponentAimObj = new GameObject("_OpponentAim");
                opponentAimObj.transform.parent = baseCharacterEntity.transform;
                opponentAimObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                opponentAimObj.transform.localRotation = Quaternion.identity;
                opponentAimObj.transform.localScale = Vector3.one;
                baseCharacterEntity.OpponentAimTransform = opponentAimObj.transform;

                var meleeDamageObj = new GameObject("_MeleeDamage");
                meleeDamageObj.transform.parent = baseCharacterEntity.transform;
                meleeDamageObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                meleeDamageObj.transform.localRotation = Quaternion.identity;
                meleeDamageObj.transform.localScale = Vector3.one;
                baseCharacterEntity.MeleeDamageTransform = meleeDamageObj.transform;

                var missileDamageObj = new GameObject("_MissileDamage");
                missileDamageObj.transform.parent = baseCharacterEntity.transform;
                missileDamageObj.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y * 0.75f);
                missileDamageObj.transform.localRotation = Quaternion.identity;
                missileDamageObj.transform.localScale = Vector3.one;
                baseCharacterEntity.MissileDamageTransform = missileDamageObj.transform;

                var characterUiObj = new GameObject("_CharacterUi");
                characterUiObj.transform.parent = baseCharacterEntity.transform;
                characterUiObj.transform.localPosition = Vector3.zero;
                characterUiObj.transform.localRotation = Quaternion.identity;
                characterUiObj.transform.localScale = Vector3.one;
                baseCharacterEntity.CharacterUiTransform = characterUiObj.transform;

                var miniMapUiObj = new GameObject("_MiniMapUi");
                miniMapUiObj.transform.parent = baseCharacterEntity.transform;
                miniMapUiObj.transform.localPosition = Vector3.zero;
                miniMapUiObj.transform.localRotation = Quaternion.identity;
                miniMapUiObj.transform.localScale = Vector3.one;
                baseCharacterEntity.MiniMapUiTransform = miniMapUiObj.transform;

                var bubbleChat = new GameObject("_ChatBubble");
                bubbleChat.transform.parent = baseCharacterEntity.transform;
                bubbleChat.transform.localPosition = Vector3.zero + (Vector3.up * bounds.size.y);
                bubbleChat.transform.localRotation = Quaternion.identity;
                bubbleChat.transform.localScale = Vector3.one;
                baseCharacterEntity.ChatBubbleTransform = bubbleChat.transform;

                var savePath = path + "\\" + fileName + ".prefab";
                Debug.Log("Saving character entity to " + savePath);
                AssetDatabase.DeleteAsset(savePath);
                PrefabUtility.SaveAsPrefabAssetAndConnect(baseCharacterEntity.gameObject, savePath, InteractionMode.AutomatedAction);

                if (gameDatabase != null)
                {
                    GameObject savedObject = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
                    BaseCharacterEntity savedEntity = savedObject.GetComponent<BaseCharacterEntity>();
                    if (savedEntity is BasePlayerCharacterEntity)
                    {
                        List<BasePlayerCharacterEntity> list = new List<BasePlayerCharacterEntity>(gameDatabase.playerCharacterEntities);
                        list.Add(savedEntity as BasePlayerCharacterEntity);
                        gameDatabase.playerCharacterEntities = list.ToArray();
                    }
                    else if (savedEntity is BaseMonsterCharacterEntity)
                    {
                        List<BaseMonsterCharacterEntity> list = new List<BaseMonsterCharacterEntity>(gameDatabase.monsterCharacterEntities);
                        list.Add(savedEntity as BaseMonsterCharacterEntity);
                        gameDatabase.monsterCharacterEntities = list.ToArray();
                    }
                    EditorUtility.SetDirty(gameDatabase);
                }
            }
        }
    }
}
