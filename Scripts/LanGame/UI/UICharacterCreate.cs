using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UICharacterCreate : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public Transform characterModelContainer;
        [Header("UI Elements")]
        public InputField inputCharacterName;
        public Button buttonCreate;
        [Header("Event")]
        public UnityEvent eventOnCreateCharacter;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterContainer;
                }
                return cacheList;
            }
        }

        private UICharacterSelectionManager selectionManager;
        public UICharacterSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.Toggle;
                return selectionManager;
            }
        }

        protected readonly Dictionary<int, BaseCharacterModel> CharacterModels = new Dictionary<int, BaseCharacterModel>();

        protected virtual void LoadCharacters()
        {
            SelectionManager.Clear();
            // Show list of characters that can be create
            var selectableCharacters = GameInstance.PlayerCharacterEntities.Values.ToList();
            CacheList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                var character = characterEntity.database;
                var characterData = new PlayerCharacterData();
                characterData.DataId = characterEntity.DataId;
                characterData.EntityId = characterEntity.EntityId;
                characterData.SetNewPlayerCharacterData(character.title, characterEntity.DataId, characterEntity.EntityId);
                var uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = characterData;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                var characterModel = characterData.InstantiateModel(characterModelContainer);
                CharacterModels[characterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                SelectionManager.Add(uiCharacter);
            });
        }

        public override void Show()
        {
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            // Clear selection
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            SelectionManager.Clear();
            CacheList.HideAll();
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

        protected virtual void OnClickCreate()
        {
            var gameInstance = GameInstance.Singleton;
            var selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
                Debug.LogWarning("Cannot create character, did not selected character class");
                return;
            }
            var characterName = inputCharacterName.text.Trim();
            var minCharacterNameLength = gameInstance.minCharacterNameLength;
            var maxCharacterNameLength = gameInstance.maxCharacterNameLength;
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

            var characterId = GenericUtils.GetUniqueId();
            var characterData = new PlayerCharacterData();
            characterData.Id = characterId;
            characterData.SetNewPlayerCharacterData(characterName, selectedUI.Data.DataId, selectedUI.Data.EntityId);
            characterData.SavePersistentCharacterData();

            if (eventOnCreateCharacter != null)
                eventOnCreateCharacter.Invoke();
        }
    }
}
