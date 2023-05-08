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
        public CharacterModelEvent eventOnBeforeUpdateAnimation = new CharacterModelEvent();
        public CharacterModelEvent eventOnAfterUpdateAnimation = new CharacterModelEvent();

        private Toggle firstRaceToggle;
        private Dictionary<CharacterRace, Toggle> _cacheRaceToggles;
        public Dictionary<CharacterRace, Toggle> CacheRaceToggles
        {
            get
            {
                if (_cacheRaceToggles == null)
                {
                    _cacheRaceToggles = new Dictionary<CharacterRace, Toggle>();
                    foreach (CharacterRaceTogglePair raceToggle in raceToggles)
                    {
                        if (raceToggle.race == null || raceToggle.toggle == null)
                            continue;
                        _cacheRaceToggles[raceToggle.race] = raceToggle.toggle;
                        if (firstRaceToggle == null)
                            firstRaceToggle = raceToggle.toggle;
                    }
                }
                return _cacheRaceToggles;
            }
        }

        private UIList _cacheCharacterList;
        public UIList CacheCharacterList
        {
            get
            {
                if (_cacheCharacterList == null)
                {
                    _cacheCharacterList = gameObject.AddComponent<UIList>();
                    if (uiCharacterPrefab != null && uiCharacterContainer != null)
                    {
                        _cacheCharacterList.uiPrefab = uiCharacterPrefab.gameObject;
                        _cacheCharacterList.uiContainer = uiCharacterContainer;
                    }
                }
                return _cacheCharacterList;
            }
        }

        private UIList _cacheCharacterClassList;
        public UIList CacheCharacterClassList
        {
            get
            {
                if (_cacheCharacterClassList == null)
                {
                    _cacheCharacterClassList = gameObject.AddComponent<UIList>();
                    if (uiCharacterClassPrefab != null && uiCharacterClassContainer != null)
                    {
                        _cacheCharacterClassList.uiPrefab = uiCharacterClassPrefab.gameObject;
                        _cacheCharacterClassList.uiContainer = uiCharacterClassContainer;
                    }
                }
                return _cacheCharacterClassList;
            }
        }

        private UIList _cacheFactionList;
        public UIList CacheFactionList
        {
            get
            {
                if (_cacheFactionList == null)
                {
                    _cacheFactionList = gameObject.AddComponent<UIList>();
                    if (uiFactionPrefab != null && uiFactionContainer != null)
                    {
                        _cacheFactionList.uiPrefab = uiFactionPrefab.gameObject;
                        _cacheFactionList.uiContainer = uiFactionContainer;
                    }
                }
                return _cacheFactionList;
            }
        }

        private UICharacterSelectionManager _cacheCharacterSelectionManager;
        public UICharacterSelectionManager CacheCharacterSelectionManager
        {
            get
            {
                if (_cacheCharacterSelectionManager == null)
                    _cacheCharacterSelectionManager = gameObject.GetOrAddComponent<UICharacterSelectionManager>();
                _cacheCharacterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _cacheCharacterSelectionManager;
            }
        }

        private UICharacterClassSelectionManager _cacheCharacterClassSelectionManager;
        public UICharacterClassSelectionManager CacheCharacterClassSelectionManager
        {
            get
            {
                if (_cacheCharacterClassSelectionManager == null)
                    _cacheCharacterClassSelectionManager = gameObject.GetOrAddComponent<UICharacterClassSelectionManager>();
                _cacheCharacterClassSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _cacheCharacterClassSelectionManager;
            }
        }

        private UIFactionSelectionManager _cacheFactionSelectionManager;
        public UIFactionSelectionManager CacheFactionSelectionManager
        {
            get
            {
                if (_cacheFactionSelectionManager == null)
                    _cacheFactionSelectionManager = GetComponent<UIFactionSelectionManager>();
                if (_cacheFactionSelectionManager == null)
                    _cacheFactionSelectionManager = gameObject.AddComponent<UIFactionSelectionManager>();
                _cacheFactionSelectionManager.selectionMode = UISelectionMode.Toggle;
                return _cacheFactionSelectionManager;
            }
        }

        protected readonly Dictionary<int, BaseCharacterModel> _characterModelByEntityId = new Dictionary<int, BaseCharacterModel>();
        protected BaseCharacterModel _selectedModel;
        public BaseCharacterModel SelectedModel { get { return _selectedModel; } }
        protected readonly Dictionary<int, PlayerCharacter[]> _playerCharacterDataByEntityId = new Dictionary<int, PlayerCharacter[]>();
        protected PlayerCharacter[] _selectableCharacterClasses;
        public PlayerCharacter[] SelectableCharacterClasses { get { return _selectableCharacterClasses; } }
        protected PlayerCharacter _selectedPlayerCharacter;
        public PlayerCharacter SelectedPlayerCharacter { get { return _selectedPlayerCharacter; } }
        protected readonly HashSet<CharacterRace> SelectedRaces = new HashSet<CharacterRace>();
        protected Faction _selectedFaction;
        public Faction SelectedFaction { get { return _selectedFaction; } }
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
            _characterModelByEntityId.Clear();
            // Remove all cached data
            _playerCharacterDataByEntityId.Clear();
            // Clear character selection
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Show list of characters that can be created
            PlayerCharacterData firstData = null;
            CacheCharacterList.Generate(GetCreatableCharacters(), (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                _playerCharacterDataByEntityId[characterEntity.EntityId] = characterEntity.CharacterDatabases;
                // Prepare data
                BaseCharacter playerCharacter = characterEntity.CharacterDatabases[0];
                PlayerCharacterData playerCharacterData = new PlayerCharacterData();
                playerCharacterData.SetNewPlayerCharacterData(characterEntity.EntityTitle, playerCharacter.DataId, characterEntity.EntityId, characterEntity.FactionId);
                // Hide all model, the first one will be shown later
                BaseCharacterModel characterModel = playerCharacterData.InstantiateModel(characterModelContainer);
                _characterModelByEntityId[playerCharacterData.EntityId] = characterModel;
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

        protected virtual void Update()
        {
            if (SelectedModel != null)
            {
                eventOnBeforeUpdateAnimation.Invoke(SelectedModel);
                SelectedModel.UpdateAnimation(Time.deltaTime);
                eventOnAfterUpdateAnimation.Invoke(SelectedModel);
            }
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
            _characterModelByEntityId.TryGetValue(playerCharacterData.EntityId, out _selectedModel);
            // Clear character class selection
            CacheCharacterClassSelectionManager.Clear();
            CacheCharacterClassList.HideAll();
            // Show selected model
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
            // Setup character class list
            PlayerCharacter firstData = null;
            _playerCharacterDataByEntityId.TryGetValue(playerCharacterData.EntityId, out _selectableCharacterClasses);
            CacheCharacterClassList.Generate(_selectableCharacterClasses, (index, playerCharacter, ui) =>
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
            _selectedPlayerCharacter = baseCharacter as PlayerCharacter;
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
                IList<EquipWeapons> selectableWeaponSets = new List<EquipWeapons>
                {
                    equipWeapons
                };
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
            _selectedFaction = faction;
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
