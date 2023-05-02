using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public class UICharacterCreate : UIBase
    {
        [Header("Game Object Elements")]
        public Transform characterModelContainer;

        [Header("UI Elements")]
        public CharacterRaceTogglePair[] raceToggles = new CharacterRaceTogglePair[0];
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public UICharacterClass uiCharacterClassPrefab;
        public Transform uiCharacterClassContainer;
        public UIFaction uiFactionPrefab;
        public Transform uiFactionContainer;

        [System.Obsolete("Deprecated, use `uiInputCharacterName` instead.")]
        [HideInInspector]
        public InputField inputCharacterName;
        public InputFieldWrapper uiInputCharacterName;
        public Button buttonCreate;

        [Header("Event")]
        public UnityEvent eventOnCreateCharacter = new UnityEvent();
        public CharacterDataEvent eventOnSelectCharacter = new CharacterDataEvent();
        public FactionEvent eventOnSelectFaction = new FactionEvent();
        public CharacterClassEvent eventOnSelectCharacterClass = new CharacterClassEvent();

        private Toggle firstRaceToggle;
        private Dictionary<CharacterRace, Toggle> cacheRaceToggles;
        public Dictionary<CharacterRace, Toggle> CacheRaceToggles
        {
            get
            {
                if (cacheRaceToggles == null)
                {
                    cacheRaceToggles = new Dictionary<CharacterRace, Toggle>();
                    foreach (CharacterRaceTogglePair raceToggle in raceToggles)
                    {
                        if (raceToggle.race == null || raceToggle.toggle == null)
                            continue;
                        cacheRaceToggles[raceToggle.race] = raceToggle.toggle;
                        if (firstRaceToggle == null)
                            firstRaceToggle = raceToggle.toggle;
                    }
                }
                return cacheRaceToggles;
            }
        }

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

        private UIList cacheFactionList;
        public UIList CacheFactionList
        {
            get
            {
                if (cacheFactionList == null)
                {
                    cacheFactionList = gameObject.AddComponent<UIList>();
                    if (uiFactionPrefab != null && uiFactionContainer != null)
                    {
                        cacheFactionList.uiPrefab = uiFactionPrefab.gameObject;
                        cacheFactionList.uiContainer = uiFactionContainer;
                    }
                }
                return cacheFactionList;
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

        private UICharacterClassSelectionManager cacheCharacterClassSelectionManager;
        public UICharacterClassSelectionManager CacheCharacterClassSelectionManager
        {
            get
            {
                if (cacheCharacterClassSelectionManager == null)
                    cacheCharacterClassSelectionManager = gameObject.GetOrAddComponent<UICharacterClassSelectionManager>();
                cacheCharacterClassSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterClassSelectionManager;
            }
        }

        private UIFactionSelectionManager cacheFactionSelectionManager;
        public UIFactionSelectionManager CacheFactionSelectionManager
        {
            get
            {
                if (cacheFactionSelectionManager == null)
                    cacheFactionSelectionManager = GetComponent<UIFactionSelectionManager>();
                if (cacheFactionSelectionManager == null)
                    cacheFactionSelectionManager = gameObject.AddComponent<UIFactionSelectionManager>();
                cacheFactionSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheFactionSelectionManager;
            }
        }

        protected readonly Dictionary<int, BaseCharacterModel> CharacterModelByEntityId = new Dictionary<int, BaseCharacterModel>();
        protected BaseCharacterModel selectedModel;
        public BaseCharacterModel SelectedModel { get { return selectedModel; } }
        protected readonly Dictionary<int, PlayerCharacter[]> PlayerCharacterDataByEntityId = new Dictionary<int, PlayerCharacter[]>();
        protected PlayerCharacter[] selectableCharacterClasses;
        public PlayerCharacter[] SelectableCharacterClasses { get { return selectableCharacterClasses; } }
        protected PlayerCharacter selectedPlayerCharacter;
        public PlayerCharacter SelectedPlayerCharacter { get { return selectedPlayerCharacter; } }
        protected readonly HashSet<CharacterRace> SelectedRaces = new HashSet<CharacterRace>();
        protected Faction selectedFaction;
        public Faction SelectedFaction { get { return selectedFaction; } }
        public int SelectedEntityId { get; protected set; }
        public int SelectedDataId { get; protected set; }
        public int SelectedFactionId { get; protected set; }

        protected override void Awake()
        {
            base.Awake();
            MigrateInputComponent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (MigrateInputComponent())
                EditorUtility.SetDirty(this);
        }
#endif

        public bool MigrateInputComponent()
        {
            bool hasChanges = false;
            InputFieldWrapper wrapper;
#pragma warning disable CS0618 // Type or member is obsolete
            if (inputCharacterName != null)
            {
                hasChanges = true;
                wrapper = inputCharacterName.gameObject.GetOrAddComponent<InputFieldWrapper>();
                wrapper.unityInputField = inputCharacterName;
                uiInputCharacterName = wrapper;
                inputCharacterName = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
        }

        protected virtual List<BasePlayerCharacterEntity> GetCreatableCharacters()
        {
            if (CacheRaceToggles.Count == 0)
                return GameInstance.PlayerCharacterEntities.Values.ToList();
            else
                return GameInstance.PlayerCharacterEntities.Values.Where((o) => SelectedRaces.Contains(o.Race)).ToList();
        }

        protected virtual List<Faction> GetSelectableFactions()
        {
            return GameInstance.Factions.Values.Where(o => !o.IsLocked).ToList();
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
            // Show list of characters that can be created
            PlayerCharacterData firstData = null;
            CacheCharacterList.Generate(GetCreatableCharacters(), (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterDataByEntityId[characterEntity.EntityId] = characterEntity.CharacterDatabases;
                // Prepare data
                BaseCharacter playerCharacter = characterEntity.CharacterDatabases[0];
                PlayerCharacterData playerCharacterData = new PlayerCharacterData();
                playerCharacterData.SetNewPlayerCharacterData(characterEntity.EntityTitle, playerCharacter.DataId, characterEntity.EntityId, characterEntity.FactionId);
                // Hide all model, the first one will be shown later
                BaseCharacterModel characterModel = playerCharacterData.InstantiateModel(characterModelContainer);
                CharacterModelByEntityId[playerCharacterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                // Setup UI
                if (ui != null)
                {
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.NotForOwningCharacter = true;
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

        protected virtual void LoadFactions()
        {
            // Clear faction selection
            CacheFactionSelectionManager.Clear();
            CacheFactionList.HideAll();
            // Show list of factions that can be selected
            Faction firstData = null;
            CacheFactionList.Generate(GetSelectableFactions(), (index, faction, ui) =>
            {
                // Setup UI
                if (ui != null)
                {
                    UIFaction uiFaction = ui.GetComponent<UIFaction>();
                    uiFaction.Data = faction;
                    CacheFactionSelectionManager.Add(uiFaction);
                }
                if (index == 0)
                    firstData = faction;
            });
            // Select first entry
            if (CacheFactionSelectionManager.Count > 0)
                CacheFactionSelectionManager.Select(0);
            else
                OnSelectFaction(firstData);
        }

        protected virtual void OnEnable()
        {
            // Setup Events
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            CacheCharacterSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            CacheCharacterSelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            CacheCharacterClassSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterClass);
            CacheCharacterClassSelectionManager.eventOnSelect.AddListener(OnSelectCharacterClass);
            CacheFactionSelectionManager.eventOnSelect.RemoveListener(OnSelectFaction);
            CacheFactionSelectionManager.eventOnSelect.AddListener(OnSelectFaction);
            SelectedRaces.Clear();
            if (CacheRaceToggles.Count > 0)
            {
                foreach (KeyValuePair<CharacterRace, Toggle> raceToggle in CacheRaceToggles)
                {
                    raceToggle.Value.SetIsOnWithoutNotify(false);
                    raceToggle.Value.onValueChanged.RemoveAllListeners();
                    raceToggle.Value.onValueChanged.AddListener((isOn) =>
                    {
                        OnRaceToggleUpdate(raceToggle.Key, isOn);
                    });
                }
                firstRaceToggle.isOn = true;
            }
            else
            {
                LoadCharacters();
            }
            LoadFactions();
        }

        protected virtual void OnDisable()
        {
            characterModelContainer.RemoveChildren();
            uiInputCharacterName.text = string.Empty;
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
            selectedPlayerCharacter = baseCharacter as PlayerCharacter;
            if (SelectedPlayerCharacter != null)
            {
                // Set creating player character data
                SelectedDataId = baseCharacter.DataId;
                // Prepare equip items
                List<CharacterItem> equipItems = new List<CharacterItem>();
                foreach (BaseItem armorItem in SelectedPlayerCharacter.ArmorItems)
                {
                    if (armorItem == null)
                        continue;
                    equipItems.Add(CharacterItem.Create(armorItem));
                }
                // Set model equip items
                SelectedModel.SetEquipItems(equipItems);
                // Prepare equip weapons
                EquipWeapons equipWeapons = new EquipWeapons();
                if (SelectedPlayerCharacter.RightHandEquipItem != null)
                    equipWeapons.rightHand = CharacterItem.Create(SelectedPlayerCharacter.RightHandEquipItem);
                if (SelectedPlayerCharacter.LeftHandEquipItem != null)
                    equipWeapons.leftHand = CharacterItem.Create(SelectedPlayerCharacter.LeftHandEquipItem);
                // Set model equip weapons
                IList<EquipWeapons> selectableWeaponSets = new List<EquipWeapons>();
                selectableWeaponSets.Add(equipWeapons);
                SelectedModel.SetEquipWeapons(selectableWeaponSets, 0, false);
            }
        }

        protected void OnSelectFaction(UIFaction uiFaction)
        {
            OnSelectFaction(uiFaction.Data);
        }

        protected virtual void OnSelectFaction(Faction faction)
        {
            eventOnSelectFaction.Invoke(faction);
            selectedFaction = faction;
            if (SelectedFaction != null)
            {
                // Set creating player character's faction
                SelectedFactionId = faction.DataId;
            }
        }

        protected virtual void OnRaceToggleUpdate(CharacterRace race, bool isOn)
        {
            if (isOn)
            {
                SelectedRaces.Add(race);
                LoadCharacters();
            }
            else
            {
                SelectedRaces.Remove(race);
            }
        }

        protected virtual void OnClickCreate()
        {
            GameInstance gameInstance = GameInstance.Singleton;
            // Validate character name
            string characterName = uiInputCharacterName.text.Trim();
            int minCharacterNameLength = gameInstance.minCharacterNameLength;
            int maxCharacterNameLength = gameInstance.maxCharacterNameLength;
            if (characterName.Length < minCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT.ToString()));
                Debug.LogWarning("Cannot create character, character name is too short");
                return;
            }
            if (characterName.Length > maxCharacterNameLength)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG.ToString()));
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
            characterData.SetNewPlayerCharacterData(characterName, SelectedDataId, SelectedEntityId, SelectedFactionId);
            GameInstance.Singleton.SaveSystem.SaveCharacter(characterData);
        }
    }
}
