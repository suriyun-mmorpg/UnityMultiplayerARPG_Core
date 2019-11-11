using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterList : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public Transform characterModelContainer;
        [Header("UI Elements")]
        public Button buttonStart;
        public Button buttonDelete;
        [Header("Event")]
        public UnityEvent eventOnNoCharacter;

        private UIList cacheCharacterList;
        public UIList CacheCharacterList
        {
            get
            {
                if (cacheCharacterList == null)
                {
                    cacheCharacterList = gameObject.AddComponent<UIList>();
                    cacheCharacterList.uiPrefab = uiCharacterPrefab.gameObject;
                    cacheCharacterList.uiContainer = uiCharacterContainer;
                }
                return cacheCharacterList;
            }
        }

        private UICharacterSelectionManager cacheCharacterSelectionManager;
        public UICharacterSelectionManager CacheCharacterSelectionManager
        {
            get
            {
                if (cacheCharacterSelectionManager == null)
                    cacheCharacterSelectionManager = GetComponent<UICharacterSelectionManager>();
                if (cacheCharacterSelectionManager == null)
                    cacheCharacterSelectionManager = gameObject.AddComponent<UICharacterSelectionManager>();
                cacheCharacterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterSelectionManager;
            }
        }

        protected readonly Dictionary<string, BaseCharacterModel> CharacterModelById = new Dictionary<string, BaseCharacterModel>();
        protected BaseCharacterModel selectedModel;
        public BaseCharacterModel SelectedModel { get { return selectedModel; } }
        protected readonly Dictionary<string, IPlayerCharacterData> PlayerCharacterDataById = new Dictionary<string, IPlayerCharacterData>();
        protected IPlayerCharacterData selectedPlayerCharacterData;
        public IPlayerCharacterData SelectedPlayerCharacterData { get { return selectedPlayerCharacterData; } }

        protected virtual void LoadCharacters()
        {
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModelById.Clear();
            // Remove all cached data
            PlayerCharacterDataById.Clear();
            // Show list of created characters
            List<PlayerCharacterData> selectableCharacters = PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null ||
                    !GameInstance.PlayerCharacterEntities.ContainsKey(selectableCharacter.EntityId) ||
                    !GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                {
                    // If invalid entity id or data id, remove from selectable character list
                    selectableCharacters.RemoveAt(i);
                }
            }

            if (selectableCharacters.Count > 0)
            {
                selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
                CacheCharacterList.Generate(selectableCharacters, (index, characterData, ui) =>
                {
                    // Cache player character to dictionary, we will use it later
                    PlayerCharacterDataById[characterData.Id] = characterData;
                    // Setup UIs
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.Data = characterData;
                    // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                    BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                    if (characterModel != null)
                    {
                        CharacterModelById[characterData.Id] = characterModel;
                        characterModel.SetEquipWeapons(characterData.EquipWeapons);
                        characterModel.SetEquipItems(characterData.EquipItems);
                        characterModel.SetMovementState(MovementState.IsGrounded);
                        characterModel.gameObject.SetActive(false);
                        CacheCharacterSelectionManager.Add(uiCharacter);
                    }
                });
            }
            else
            {
                if (eventOnNoCharacter != null)
                    eventOnNoCharacter.Invoke();
            }
        }

        private void Update()
        {
            // Update model animation
            if (SelectedModel != null)
            {
                SelectedModel.SetIsDead(false);
                SelectedModel.SetMoveAnimationSpeedMultiplier(1);
                SelectedModel.SetMovementState(MovementState.IsGrounded);
            }
        }

        public override void Show()
        {
            buttonStart.onClick.RemoveListener(OnClickStart);
            buttonStart.onClick.AddListener(OnClickStart);
            buttonDelete.onClick.RemoveListener(OnClickDelete);
            buttonDelete.onClick.AddListener(OnClickDelete);
            // Clear selection
            CacheCharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CacheCharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Load characters
            LoadCharacters();
            base.Show();
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            base.Hide();
        }

        protected void OnSelectCharacter(UICharacter uiCharacter)
        {
            OnSelectCharacter(uiCharacter.Data as IPlayerCharacterData);
        }

        protected virtual void OnSelectCharacter(IPlayerCharacterData playerCharacterData)
        {
            buttonStart.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            PlayerCharacterDataById.TryGetValue(playerCharacterData.Id, out selectedPlayerCharacterData);
            CharacterModelById.TryGetValue(playerCharacterData.Id, out selectedModel);
            // Show selected model
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
        }

        protected virtual void OnClickStart()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_START.ToString()));
                Debug.LogWarning("Cannot start game, No chosen character");
                return;
            }
            // Load gameplay scene, we're going to manage maps in gameplay scene later
            // So we can add gameplay UI just once in gameplay scene
            PlayerCharacterData characterData = new PlayerCharacterData();
            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            playerCharacter.CloneTo(characterData);
            GameInstance gameInstance = GameInstance.Singleton;
            LanRpgNetworkManager networkManager = BaseGameNetworkManager.Singleton as LanRpgNetworkManager;
            if (!gameInstance.GetGameMapIds().Contains(characterData.CurrentMapName))
            {
                MapInfo startMap = (characterData.GetDatabase() as PlayerCharacter).StartMap;
                characterData.CurrentMapName = startMap.Id;
                characterData.CurrentPosition = startMap.startPosition;
            }
            networkManager.Assets.onlineScene.SceneName = GameInstance.MapInfos[characterData.CurrentMapName].GetSceneName();
            networkManager.selectedCharacter = characterData;
            networkManager.StartGame();
        }

        protected virtual void OnClickDelete()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE.ToString()));
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            playerCharacter.DeletePersistentCharacterData();
            // Reload characters
            LoadCharacters();
        }
    }
}
