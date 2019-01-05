using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UICharacterList : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public Transform characterModelContainer;
        [Header("UI Elements")]
        public Button buttonStart;
        public Button buttonDelete;

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

        protected readonly Dictionary<string, BaseCharacterModel> CharacterModels = new Dictionary<string, BaseCharacterModel>();

        protected virtual void LoadCharacters()
        {
            SelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            // Show list of created characters
            List<PlayerCharacterData> selectableCharacters = PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null || !GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                    selectableCharacters.RemoveAt(i);
            }
            selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
            CacheList.Generate(selectableCharacters, (index, character, ui) =>
            {
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = character;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                BaseCharacterModel characterModel = character.InstantiateModel(characterModelContainer);
                if (characterModel != null)
                {
                    CharacterModels[character.Id] = characterModel;
                    characterModel.gameObject.SetActive(false);
                    characterModel.SetEquipWeapons(character.EquipWeapons);
                    characterModel.SetEquipItems(character.EquipItems);
                    SelectionManager.Add(uiCharacter);
                }
            });
        }

        public override void Show()
        {
            buttonStart.onClick.RemoveListener(OnClickStart);
            buttonStart.onClick.AddListener(OnClickStart);
            buttonDelete.onClick.RemoveListener(OnClickDelete);
            buttonDelete.onClick.AddListener(OnClickDelete);
            // Clear selection
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            SelectionManager.Clear();
            CacheList.HideAll();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            LoadCharacters();
            base.Show();
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            base.Hide();
        }

        protected virtual void OnSelectCharacter(UICharacter ui)
        {
            buttonStart.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            IPlayerCharacterData playerCharacter = ui.Data as IPlayerCharacterData;
            ShowCharacter(playerCharacter.Id);
        }

        protected virtual void ShowCharacter(string id)
        {
            BaseCharacterModel characterModel;
            if (string.IsNullOrEmpty(id) || !CharacterModels.TryGetValue(id, out characterModel))
                return;
            characterModel.gameObject.SetActive(true);
        }

        protected virtual void OnClickStart()
        {
            UICharacter selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot start game", "Please choose character to start game");
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
            if (!gameInstance.GetGameScenes().Contains(characterData.CurrentMapName))
            {
                MapInfo startMap = (characterData.GetDatabase() as PlayerCharacter).StartMap;
                characterData.CurrentMapName = startMap.scene.SceneName;
                characterData.CurrentPosition = startMap.startPosition;
            }
            networkManager.Assets.onlineScene.SceneName = characterData.CurrentMapName;
            networkManager.selectedCharacter = characterData;
            networkManager.StartGame();
        }

        protected virtual void OnClickDelete()
        {
            UICharacter selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot delete character", "Please choose character to delete");
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
