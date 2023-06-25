using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIBodyPartManager : MonoBehaviour
    {
        public string modelSettingId;

        [Header("Model Option Settings")]
        public GameObject uiModelRoot;
        public UIBodyPartModelOption uiSelectedModel;
        public UIBodyPartModelOption uiModelPrefab;
        public Transform uiModelContainer;

        [Header("Color Option Settings")]
        public GameObject uiColorRoot;
        public UIBodyPartColorOption uiSelectedColor;
        public UIBodyPartColorOption uiColorPrefab;
        public Transform uiColorContainer;

        private UIList _modelList;
        public UIList ModelList
        {
            get
            {
                if (_modelList == null)
                {
                    _modelList = gameObject.AddComponent<UIList>();
                    _modelList.uiPrefab = uiModelPrefab.gameObject;
                    _modelList.uiContainer = uiModelContainer;
                }
                return _modelList;
            }
        }

        private UIBodyPartModelListSelectionManager _modelSelectionManager;
        public UIBodyPartModelListSelectionManager ModelSelectionManager
        {
            get
            {
                if (_modelSelectionManager == null)
                    _modelSelectionManager = gameObject.GetOrAddComponent<UIBodyPartModelListSelectionManager>();
                _modelSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _modelSelectionManager;
            }
        }

        private UIList _colorList;
        public UIList ColorList
        {
            get
            {
                if (_colorList == null)
                {
                    _colorList = gameObject.AddComponent<UIList>();
                    _colorList.uiPrefab = uiColorPrefab.gameObject;
                    _colorList.uiContainer = uiColorContainer;
                }
                return _colorList;
            }
        }


        private UIBodyPartColorListSelectionManager _colorSelectionManager;
        public UIBodyPartColorListSelectionManager ColorSelectionManager
        {
            get
            {
                if (_colorSelectionManager == null)
                    _colorSelectionManager = gameObject.GetOrAddComponent<UIBodyPartColorListSelectionManager>();
                _colorSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _colorSelectionManager;
            }
        }

        private UISelectionManagerShowOnSelectEventManager<PlayerCharacterBodyPartComponent.ModelOption, UIBodyPartModelOption> _modelUIEventSetupManager = new UISelectionManagerShowOnSelectEventManager<PlayerCharacterBodyPartComponent.ModelOption, UIBodyPartModelOption>();
        private UISelectionManagerShowOnSelectEventManager<PlayerCharacterBodyPartComponent.ColorOption, UIBodyPartColorOption> _colorUIEventSetupManager = new UISelectionManagerShowOnSelectEventManager<PlayerCharacterBodyPartComponent.ColorOption, UIBodyPartColorOption>();

        private void OnEnable()
        {
            _modelUIEventSetupManager.OnEnable(ModelSelectionManager, uiSelectedModel);
            _colorUIEventSetupManager.OnEnable(ColorSelectionManager, uiSelectedColor);
        }

        private void OnDisable()
        {
            _modelUIEventSetupManager.OnDisable();
            _colorUIEventSetupManager.OnDisable();
        }

        // TODO: Setup the list
    }
}