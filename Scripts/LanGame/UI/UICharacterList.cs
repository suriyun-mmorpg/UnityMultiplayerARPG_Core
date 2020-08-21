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
        public UnityEvent eventOnAbleToCreateCharacter;
        public UnityEvent eventOnNotAbleToCreateCharacter;

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
                    cacheCharacterSelectionManager = gameObject.GetOrAddComponent<UICharacterSelectionManager>();
                cacheCharacterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterSelectionManager;
            }
        }

        protected readonly Dictionary<string, BaseCharacterModel> CharacterModelById = new Dictionary<string, BaseCharacterModel>();
        protected BaseCharacterModel selectedModel;
        public BaseCharacterModel SelectedModel { get { return selectedModel; } }
        protected readonly Dictionary<string, PlayerCharacterData> PlayerCharacterDataById = new Dictionary<string, PlayerCharacterData>();
        protected PlayerCharacterData selectedPlayerCharacterData;
        public PlayerCharacterData SelectedPlayerCharacterData { get { return selectedPlayerCharacterData; } }

        protected virtual void LoadCharacters()
        {
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Unenabled buttons
            if (buttonStart)
                buttonStart.gameObject.SetActive(false);
            if (buttonDelete)
                buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModelById.Clear();
            // Remove all cached data
            PlayerCharacterDataById.Clear();
            // Show list of created characters
            List<PlayerCharacterData> selectableCharacters = GameInstance.Singleton.SaveSystem.LoadCharacters();
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

            if (GameInstance.Singleton.maxCharacterSaves > 0 &&
                selectableCharacters.Count >= GameInstance.Singleton.maxCharacterSaves)
                eventOnNotAbleToCreateCharacter.Invoke();
            else
                eventOnAbleToCreateCharacter.Invoke();

            // Clear selected character data, will select first in list if available
            (BaseGameNetworkManager.Singleton as LanRpgNetworkManager).selectedCharacter = selectedPlayerCharacterData = null;

            // Generate list entry by saved characters
            if (selectableCharacters.Count > 0)
            {
                selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
                CacheCharacterList.Generate(selectableCharacters, (index, characterData, ui) =>
                {
                    // Cache player character to dictionary, we will use it later
                    PlayerCharacterDataById[characterData.Id] = characterData;
                    // Setup UIs
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.NotForOwningCharacter = true;
                    uiCharacter.Data = characterData;
                    // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                    BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                    if (characterModel != null)
                    {
                        CharacterModelById[characterData.Id] = characterModel;
                        characterModel.SetEquipWeapons(characterData.EquipWeapons);
                        characterModel.SetEquipItems(characterData.EquipItems);
                        characterModel.gameObject.SetActive(false);
                        CacheCharacterSelectionManager.Add(uiCharacter);
                    }
                });
            }
            else
            {
                eventOnNoCharacter.Invoke();
            }
        }

        public override void Show()
        {
            if (buttonStart)
            {
                buttonStart.onClick.RemoveListener(OnClickStart);
                buttonStart.onClick.AddListener(OnClickStart);
                buttonStart.gameObject.SetActive(false);
            }
            if (buttonDelete)
            {
                buttonDelete.onClick.RemoveListener(OnClickDelete);
                buttonDelete.onClick.AddListener(OnClickDelete);
                buttonDelete.gameObject.SetActive(false);
            }
            // Clear selection
            CacheCharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CacheCharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
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
            if (buttonStart)
                buttonStart.gameObject.SetActive(true);
            if (buttonDelete)
                buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            // Load selected character and also validate its data
            PlayerCharacterDataById.TryGetValue(playerCharacterData.Id, out selectedPlayerCharacterData);
            // Validate map data
            if (!GameInstance.Singleton.GetGameMapIds().Contains(SelectedPlayerCharacterData.CurrentMapName))
            {
                PlayerCharacter database = SelectedPlayerCharacterData.GetDatabase() as PlayerCharacter;
                SelectedPlayerCharacterData.CurrentMapName = database.StartMap.Id;
                SelectedPlayerCharacterData.CurrentPosition = database.StartPosition;
            }
            // Set selected character to network manager
            (BaseGameNetworkManager.Singleton as LanRpgNetworkManager).selectedCharacter = SelectedPlayerCharacterData;
            // Show selected character model
            CharacterModelById.TryGetValue(playerCharacterData.Id, out selectedModel);
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
        }

        public virtual void OnClickStart()
        {
            if (SelectedPlayerCharacterData == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_START.ToString()));
                Debug.LogWarning("Cannot start game, No chosen character");
                return;
            }
            (BaseGameNetworkManager.Singleton as LanRpgNetworkManager).StartGame();
        }

        public virtual void OnClickDelete()
        {
            if (SelectedPlayerCharacterData == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE.ToString()));
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }
            SelectedPlayerCharacterData.DeletePersistentCharacterData();
            // Reload characters
            LoadCharacters();
        }
    }
}
