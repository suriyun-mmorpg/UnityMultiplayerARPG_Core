using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UICharacterCreate : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public UICharacterClass uiCharacterClassPrefab;
        public Transform uiCharacterClassContainer;

        public Transform characterModelContainer;
        [Header("UI Elements")]
        public InputField inputCharacterName;
        public Button buttonCreate;
        [Header("Event")]
        public UnityEvent eventOnCreateCharacter;

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

        private UIList cacheCharacterClassList;
        public UIList CacheCharacterClassList
        {
            get
            {
                if (cacheCharacterClassList == null)
                {
                    cacheCharacterClassList = gameObject.AddComponent<UIList>();
                    cacheCharacterClassList.uiPrefab = uiCharacterClassPrefab.gameObject;
                    cacheCharacterClassList.uiContainer = uiCharacterClassContainer;
                }
                return cacheCharacterClassList;
            }
        }

        private UICharacterSelectionManager cacheCharacterSelectionManager;
        public UICharacterSelectionManager CacheCharacterSelectionManager
        {
            get
            {
                if (cacheCharacterSelectionManager == null)
                {
                    cacheCharacterSelectionManager = GetComponent<UICharacterSelectionManager>();
                    if (cacheCharacterSelectionManager == null)
                        cacheCharacterSelectionManager = gameObject.AddComponent<UICharacterSelectionManager>();
                }
                cacheCharacterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterSelectionManager;
            }
        }

        private UICharacterClassSelectionManager cacheCharacterClassSelectionManager;
        public UICharacterClassSelectionManager CacheCharacterClassSelectionManager
        {
            get
            {
                if (cacheCharacterClassSelectionManager == null)
                {
                    cacheCharacterClassSelectionManager = GetComponent<UICharacterClassSelectionManager>();
                    if (cacheCharacterClassSelectionManager == null)
                        cacheCharacterClassSelectionManager = gameObject.AddComponent<UICharacterClassSelectionManager>();
                }
                cacheCharacterClassSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterClassSelectionManager;
            }
        }

        protected readonly Dictionary<int, BaseCharacterModel> CharacterModels = new Dictionary<int, BaseCharacterModel>();

        protected virtual List<BasePlayerCharacterEntity> GetCreatableCharacters()
        {
            return GameInstance.PlayerCharacterEntities.Values.ToList();
        }

        protected virtual void LoadCharacters()
        {
            CacheCharacterSelectionManager.Clear();
            // Show list of characters that can be create
            List<BasePlayerCharacterEntity> selectableCharacters = GetCreatableCharacters();
            CacheCharacterList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                BaseCharacter character = characterEntity.database;
                PlayerCharacterData characterData = new PlayerCharacterData();
                characterData.DataId = characterEntity.DataId;
                characterData.EntityId = characterEntity.EntityId;
                characterData.SetNewPlayerCharacterData(character.Title, characterEntity.DataId, characterEntity.EntityId);
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = characterData;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                CharacterModels[characterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                CacheCharacterSelectionManager.Add(uiCharacter);
            });
        }

        public override void Show()
        {
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            // Clear character selection
            CacheCharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CacheCharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Clear character class selection
            CacheCharacterClassSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterClass);
            CacheCharacterClassSelectionManager.eventOnSelect.AddListener(OnSelectCharacterClass);
            CacheCharacterClassSelectionManager.Clear();
            CacheCharacterClassList.HideAll();
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            LoadCharacters();
            base.Show();
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            inputCharacterName.text = "";
            base.Hide();
        }

        protected virtual void OnSelectCharacter(UICharacter ui)
        {
            characterModelContainer.SetChildrenActive(false);
            ShowCharacter(ui.Data.EntityId);
        }

        protected virtual void ShowCharacter(int id)
        {
            BaseCharacterModel characterModel;
            if (!CharacterModels.TryGetValue(id, out characterModel))
                return;
            characterModel.gameObject.SetActive(true);
        }

        protected virtual void OnSelectCharacterClass(UICharacterClass ui)
        {

        }

        protected virtual void OnClickCreate()
        {
            GameInstance gameInstance = GameInstance.Singleton;
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
                Debug.LogWarning("Cannot create character, did not selected character class");
                return;
            }
            string characterName = inputCharacterName.text.Trim();
            int minCharacterNameLength = gameInstance.minCharacterNameLength;
            int maxCharacterNameLength = gameInstance.maxCharacterNameLength;
            if (characterName.Length < minCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Character name is too short");
                Debug.LogWarning("Cannot create character, character name is too short");
                return;
            }
            if (characterName.Length > maxCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Character name is too long");
                Debug.LogWarning("Cannot create character, character name is too long");
                return;
            }

            string characterId = GenericUtils.GetUniqueId();
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = characterId;
            characterData.SetNewPlayerCharacterData(characterName, selectedUI.Data.DataId, selectedUI.Data.EntityId);
            characterData.SavePersistentCharacterData();

            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }
    }
}
