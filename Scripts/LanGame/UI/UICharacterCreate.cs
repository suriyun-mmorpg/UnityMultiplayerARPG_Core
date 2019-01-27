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
        protected BaseCharacterModel selectedModel;
        public BaseCharacterModel SelectedModel { get { return selectedModel; } }
        protected readonly Dictionary<int, BasePlayerCharacterEntity> PlayerCharacterEntities = new Dictionary<int, BasePlayerCharacterEntity>();
        protected BasePlayerCharacterEntity selectedPlayerCharacterEntity;
        public BasePlayerCharacterEntity SelectedPlayerCharacterEntity { get { return selectedPlayerCharacterEntity; } }
        protected readonly Dictionary<int, PlayerCharacter> PlayerCharacters = new Dictionary<int, PlayerCharacter>();
        protected PlayerCharacter selectedPlayerCharacter;
        public PlayerCharacter SelectedPlayerCharacter { get { return selectedPlayerCharacter; } }
        public PlayerCharacterData CreatingPlayerCharacterData { get; private set; }

        protected virtual List<BasePlayerCharacterEntity> GetCreatableCharacters()
        {
            return GameInstance.PlayerCharacterEntities.Values.ToList();
        }

        protected virtual void LoadCharacters()
        {
            CacheCharacterSelectionManager.Clear();
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            // Remove all cached data
            PlayerCharacterEntities.Clear();
            // Show list of characters that can be create
            List<BasePlayerCharacterEntity> selectableCharacters = GetCreatableCharacters();
            CacheCharacterList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterEntities[characterEntity.EntityId] = characterEntity;
                // Setup UIs
                BaseCharacter playerCharacter = characterEntity.playerCharacters[0];
                PlayerCharacterData playerCharacterData = new PlayerCharacterData();
                playerCharacterData.SetNewPlayerCharacterData(characterEntity.characterTitle, playerCharacter.DataId, characterEntity.EntityId);
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = playerCharacterData;
                // Hide all model, the first one will be shown later
                BaseCharacterModel characterModel = playerCharacterData.InstantiateModel(characterModelContainer);
                CharacterModels[playerCharacterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                CacheCharacterSelectionManager.Add(uiCharacter);
            });
            CacheCharacterSelectionManager.Select(0);
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
            // Load characters
            LoadCharacters();
            base.Show();
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            inputCharacterName.text = "";
            base.Hide();
        }

        protected virtual void OnSelectCharacter(UICharacter uiCharacter)
        {
            characterModelContainer.SetChildrenActive(false);
            PlayerCharacterData playerCharacterData = uiCharacter.Data as PlayerCharacterData;
            if (CreatingPlayerCharacterData == null)
                CreatingPlayerCharacterData = new PlayerCharacterData();
            playerCharacterData.CloneTo(CreatingPlayerCharacterData);
            CharacterModels.TryGetValue(playerCharacterData.EntityId, out selectedModel);
            PlayerCharacterEntities.TryGetValue(playerCharacterData.EntityId, out selectedPlayerCharacterEntity);
            // Show selected model
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
            // Setup character class list
            CacheCharacterClassList.Generate(SelectedPlayerCharacterEntity.playerCharacters, (index, playerCharacter, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacters[playerCharacter.DataId] = playerCharacter;
                // Setup UIs
                UICharacterClass uiCharacterClass = ui.GetComponent<UICharacterClass>();
                uiCharacterClass.Data = playerCharacter;
                CacheCharacterClassSelectionManager.Add(uiCharacterClass);
            });
            CacheCharacterClassSelectionManager.Select(0);
        }

        protected virtual void OnSelectCharacterClass(UICharacterClass uiCharacterClass)
        {
            BaseCharacter baseCharacter = uiCharacterClass.Data;
            PlayerCharacters.TryGetValue(baseCharacter.DataId, out selectedPlayerCharacter);
            if (SelectedPlayerCharacter != null)
            {
                // Set creating player character data
                CreatingPlayerCharacterData.SetNewPlayerCharacterData(CreatingPlayerCharacterData.CharacterName, baseCharacter.DataId, CreatingPlayerCharacterData.EntityId);
                // Prepare equip items
                List<CharacterItem> equipItems = new List<CharacterItem>();
                foreach (Item armorItem in SelectedPlayerCharacter.armorItems)
                {
                    equipItems.Add(CharacterItem.Create(armorItem));
                }
                // Set model equip items
                SelectedModel.SetEquipItems(equipItems);
                // Prepare equip weapons
                EquipWeapons equipWeapons = new EquipWeapons();
                equipWeapons.leftHand = CharacterItem.Create(SelectedPlayerCharacter.leftHandEquipItem);
                equipWeapons.rightHand = CharacterItem.Create(SelectedPlayerCharacter.rightHandEquipItem);
                // Set model equip weapons
                SelectedModel.SetEquipWeapons(equipWeapons);
            }
        }

        protected virtual void OnClickCreate()
        {
            GameInstance gameInstance = GameInstance.Singleton;
            // Validate character name
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

            SaveCreatingPlayerCharacter(characterName);

            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }

        protected virtual void SaveCreatingPlayerCharacter(string characterName)
        {
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = GenericUtils.GetUniqueId();
            characterData.SetNewPlayerCharacterData(characterName, CreatingPlayerCharacterData.DataId, CreatingPlayerCharacterData.EntityId);
            characterData.SavePersistentCharacterData();
        }
    }
}
