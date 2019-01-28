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
        public CharacterDataEvent eventOnSelectCharacter;
        public CharacterClassEvent eventOnSelectCharacterClass;

        private UIList cacheCharacterList;
        public UIList CacheCharacterList
        {
            get
            {
                if (cacheCharacterList == null)
                {
                    cacheCharacterList = gameObject.AddComponent<UIList>();
                    if (uiCharacterPrefab != null && uiCharacterContainer != null)
                    {
                        cacheCharacterList.uiPrefab = uiCharacterPrefab.gameObject;
                        cacheCharacterList.uiContainer = uiCharacterContainer;
                    }
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
                    if (uiCharacterClassPrefab != null && uiCharacterClassContainer != null)
                    {
                        cacheCharacterClassList.uiPrefab = uiCharacterClassPrefab.gameObject;
                        cacheCharacterClassList.uiContainer = uiCharacterClassContainer;
                    }
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

        protected readonly Dictionary<int, BaseCharacterModel> CharacterModelByEntityId = new Dictionary<int, BaseCharacterModel>();
        protected BaseCharacterModel selectedModel;
        public BaseCharacterModel SelectedModel { get { return selectedModel; } }
        protected readonly Dictionary<int, PlayerCharacter[]> PlayerCharacterDataByEntityId = new Dictionary<int, PlayerCharacter[]>();
        protected PlayerCharacter[] selectableCharacterClasses;
        public PlayerCharacter[] SelectableCharacterClasses { get { return selectableCharacterClasses; } }
        protected readonly Dictionary<int, PlayerCharacter> PlayerCharacterByDataId = new Dictionary<int, PlayerCharacter>();
        protected PlayerCharacter selectedPlayerCharacter;
        public PlayerCharacter SelectedPlayerCharacter { get { return selectedPlayerCharacter; } }
        public int SelectedEntityId { get; protected set; }
        public int SelectedDataId { get; protected set; }

        protected virtual List<BasePlayerCharacterEntity> GetCreatableCharacters()
        {
            return GameInstance.PlayerCharacterEntities.Values.ToList();
        }

        protected virtual void LoadCharacters()
        {
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModelByEntityId.Clear();
            // Remove all cached data
            PlayerCharacterDataByEntityId.Clear();
            // Clear character selection
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Show list of characters that can be create
            PlayerCharacterData firstData = null;
            List<BasePlayerCharacterEntity> selectableCharacters = GetCreatableCharacters();
            CacheCharacterList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterDataByEntityId[characterEntity.EntityId] = characterEntity.playerCharacters;
                // Prepare data
                BaseCharacter playerCharacter = characterEntity.playerCharacters[0];
                PlayerCharacterData playerCharacterData = new PlayerCharacterData();
                playerCharacterData.SetNewPlayerCharacterData(characterEntity.characterTitle, playerCharacter.DataId, characterEntity.EntityId);
                // Hide all model, the first one will be shown later
                BaseCharacterModel characterModel = playerCharacterData.InstantiateModel(characterModelContainer);
                CharacterModelByEntityId[playerCharacterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                // Setup UI
                if (ui != null)
                {
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.Data = playerCharacterData;
                    CacheCharacterSelectionManager.Add(uiCharacter);
                }
                if (index == 0)
                    firstData = playerCharacterData;
            });
            // Select first entry
            if (CacheCharacterSelectionManager.Count > 0)
                CacheCharacterSelectionManager.Select(0);
            else
                OnSelectCharacter(firstData);
        }

        public override void Show()
        {
            // Setup Events
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            CacheCharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CacheCharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CacheCharacterClassSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterClass);
            CacheCharacterClassSelectionManager.eventOnSelect.AddListener(OnSelectCharacterClass);
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

        protected void OnSelectCharacter(UICharacter uiCharacter)
        {
            OnSelectCharacter(uiCharacter.Data as IPlayerCharacterData);
        }

        protected virtual void OnSelectCharacter(IPlayerCharacterData playerCharacterData)
        {
            eventOnSelectCharacter.Invoke(playerCharacterData);
            characterModelContainer.SetChildrenActive(false);
            SelectedDataId = playerCharacterData.DataId;
            SelectedEntityId = playerCharacterData.EntityId;
            CharacterModelByEntityId.TryGetValue(playerCharacterData.EntityId, out selectedModel);
            // Clear character class selection
            CacheCharacterClassSelectionManager.Clear();
            CacheCharacterClassList.HideAll();
            // Show selected model
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
            // Setup character class list
            PlayerCharacter firstData = null;
            PlayerCharacterDataByEntityId.TryGetValue(playerCharacterData.EntityId, out selectableCharacterClasses);
            CacheCharacterClassList.Generate(selectableCharacterClasses, (index, playerCharacter, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterByDataId[playerCharacter.DataId] = playerCharacter;
                // Setup UI
                if (ui != null)
                {
                    UICharacterClass uiCharacterClass = ui.GetComponent<UICharacterClass>();
                    uiCharacterClass.Data = playerCharacter;
                    CacheCharacterClassSelectionManager.Add(uiCharacterClass);
                }
                if (index == 0)
                    firstData = playerCharacter;
            });
            // Select first entry
            if (CacheCharacterClassSelectionManager.Count > 0)
                CacheCharacterClassSelectionManager.Select(0);
            else
                OnSelectCharacterClass(firstData);
        }


        protected void OnSelectCharacterClass(UICharacterClass uiCharacterClass)
        {
            OnSelectCharacterClass(uiCharacterClass.Data);
        }

        protected virtual void OnSelectCharacterClass(BaseCharacter baseCharacter)
        {
            eventOnSelectCharacterClass.Invoke(baseCharacter);
            PlayerCharacterByDataId.TryGetValue(baseCharacter.DataId, out selectedPlayerCharacter);
            if (SelectedPlayerCharacter != null)
            {
                // Set creating player character data
                SelectedDataId = baseCharacter.DataId;
                // Prepare equip items
                List<CharacterItem> equipItems = new List<CharacterItem>();
                foreach (Item armorItem in SelectedPlayerCharacter.armorItems)
                {
                    if (armorItem == null)
                        continue;
                    equipItems.Add(CharacterItem.Create(armorItem));
                }
                // Set model equip items
                SelectedModel.SetEquipItems(equipItems);
                // Prepare equip weapons
                EquipWeapons equipWeapons = new EquipWeapons();
                if (SelectedPlayerCharacter.rightHandEquipItem != null)
                    equipWeapons.rightHand = CharacterItem.Create(SelectedPlayerCharacter.rightHandEquipItem);
                if (SelectedPlayerCharacter.leftHandEquipItem != null)
                    equipWeapons.leftHand = CharacterItem.Create(SelectedPlayerCharacter.leftHandEquipItem);
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
            characterData.SetNewPlayerCharacterData(characterName, SelectedDataId, SelectedEntityId);
            characterData.SavePersistentCharacterData();
        }
    }
}
