using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                {
                    cacheCharacterSelectionManager = GetComponent<UICharacterSelectionManager>();
                    if (cacheCharacterSelectionManager == null)
                        cacheCharacterSelectionManager = gameObject.AddComponent<UICharacterSelectionManager>();
                }
                cacheCharacterSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheCharacterSelectionManager;
            }
        }

        protected readonly Dictionary<string, BaseCharacterModel> CharacterModels = new Dictionary<string, BaseCharacterModel>();
        protected readonly Dictionary<string, IPlayerCharacterData> PlayerCharacterDataDict = new Dictionary<string, IPlayerCharacterData>();
        public IPlayerCharacterData SelectedPlayerCharacterData { get; protected set; }

        protected virtual void LoadCharacters()
        {
            CacheCharacterSelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            // Remove all cached data
            PlayerCharacterDataDict.Clear();
            // Show list of created characters
            List<PlayerCharacterData> selectableCharacters = PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null || !GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                    selectableCharacters.RemoveAt(i);
            }
            selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
            CacheCharacterList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterDataDict[characterEntity.Id] = characterEntity;
                // Setup UIs
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = characterEntity;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                BaseCharacterModel characterModel = characterEntity.InstantiateModel(characterModelContainer);
                if (characterModel != null)
                {
                    CharacterModels[characterEntity.Id] = characterModel;
                    characterModel.gameObject.SetActive(false);
                    characterModel.SetEquipWeapons(characterEntity.EquipWeapons);
                    characterModel.SetEquipItems(characterEntity.EquipItems);
                    CacheCharacterSelectionManager.Add(uiCharacter);
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

        protected virtual void OnSelectCharacter(UICharacter uiCharacter)
        {
            buttonStart.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            IPlayerCharacterData playerCharacter = uiCharacter.Data as IPlayerCharacterData;
            SelectedPlayerCharacterData = PlayerCharacterDataDict[playerCharacter.Id];
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
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
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
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
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
