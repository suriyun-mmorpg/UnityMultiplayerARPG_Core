using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBodyPartComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [System.Serializable]
        public class MaterialGroup
        {
            [Tooltip("Material Settings for each mesh's materials, its index is index of `MeshRenderer` -> `materials`")]
            public Material[] materials = new Material[0];
        }

        [System.Serializable]
        public class ModelColorSetting
        {
            [Tooltip("Material Settings for each mesh's materials, its index is index of `MeshRenderer` -> `materials`")]
            public Material[] materials = new Material[0];
            public MaterialGroup[] materialGroups = new MaterialGroup[0];
        }

        [System.Serializable]
        public class ColorOption
        {
            [Header("Settings for UIs")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];
            [PreviewSprite(50)]
            public Sprite icon;
            public Color iconColor = Color.white;

            [Header("Settings for In-Game Appearances")]
            [Tooltip("Color settings for each model, its index is index of `models`")]
            public ModelColorSetting[] ModelColorSettings = new ModelColorSetting[0];

            public string Title
            {
                get { return Language.GetText(languageSpecificTitles, defaultTitle); }
            }
        }

        [System.Serializable]
        public class ModelOption
        {
            [Header("Settings for UIs")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];
            [PreviewSprite(50)]
            public Sprite icon;

            [Header("Settings for In-Game Appearances")]
            public EquipmentModel[] models = new EquipmentModel[0];
            public ColorOption[] colors = new ColorOption[0];

            public string Title
            {
                get { return Language.GetText(languageSpecificTitles, defaultTitle); }
            }
        }

        public string modelSettingId;
        public string colorSettingId;
        public ModelOption[] options = new ModelOption[0];
        protected int _currentModelIndex;
        protected int _currentColorIndex;
        public IEnumerable<ModelOption> ModelOptions { get => options; }
        public int MaxModelOptions { get => options.Length; }
        public IEnumerable<ColorOption> ColorOptions { get => options[_currentModelIndex].colors; }
        public int MaxColorOptions { get => options[_currentModelIndex].colors.Length; }

        private BaseCharacterModel[] _models;

        public override void EntityStart()
        {
            _models = GetComponentsInChildren<BaseCharacterModel>(true);
            SetupEvents();
            ApplyModelAndColorBySavedData();
        }

        public override void EntityOnDestroy()
        {
            ClearEvents();
            if (_models != null && _models.Length > 0)
            {
                for (int i = 0; i < _models.Length; ++i)
                {
                    _models[i] = null;
                }
            }
        }

        public void SetupEvents()
        {
            ClearEvents();
            for (int i = 0; i < _models.Length; ++i)
            {
                SetupCharacterModelEvents(_models[i]);
            }
#if !DISABLE_CUSTOM_CHARACTER_DATA
            Entity.onPublicIntsOperation -= OnPublicIntsOperation;
            Entity.onPublicIntsOperation += OnPublicIntsOperation;
#endif
        }

        public void ClearEvents()
        {
            if (_models != null)
            {
                for (int i = 0; i < _models.Length; ++i)
                {
                    ClearCharacterModelEvents(_models[i]);
                }
            }
#if !DISABLE_CUSTOM_CHARACTER_DATA
            Entity.onPublicIntsOperation -= OnPublicIntsOperation;
#endif
        }

        public void SetupCharacterModelEvents(BaseCharacterModel model)
        {
            ClearCharacterModelEvents(model);
            model.onBeforeUpdateEquipmentModels += OnBeforeUpdateEquipmentModels;
        }

        public void ClearCharacterModelEvents(BaseCharacterModel model)
        {
            model.onBeforeUpdateEquipmentModels -= OnBeforeUpdateEquipmentModels;
        }

        public void ApplyModelAndColorBySavedData()
        {
#if !DISABLE_CUSTOM_CHARACTER_DATA
            ApplyModelAndColorBySavedData(Entity.PublicInts);
#endif
        }

        public void ApplyModelAndColorBySavedData(IList<CharacterDataInt32> publicInts)
        {
            _currentModelIndex = 0;
            _currentColorIndex = 0;
            byte foundCount = 0;
            int hashedModelSettingId = GetHashedModelSettingId();
            int hashedColorSettingId = GetHashedColorSettingId();
            for (int i = 0; i < publicInts.Count; ++i)
            {
                if (publicInts[i].hashedKey == hashedModelSettingId)
                {
                    _currentModelIndex = publicInts[i].value;
                    foundCount++;
                }
                if (publicInts[i].hashedKey == hashedColorSettingId)
                {
                    _currentColorIndex = publicInts[i].value;
                    foundCount++;
                }
                if (foundCount == 2)
                    break;
            }
            ApplyModelAndColorBySavedData(_currentModelIndex, _currentColorIndex);
        }

        public void ApplyModelAndColorBySavedData(int modelIndex, int colorIndex)
        {
            _currentModelIndex = modelIndex;
            _currentColorIndex = colorIndex;
            if (_currentModelIndex >= MaxModelOptions)
                _currentModelIndex = 0;
            if (_currentColorIndex >= MaxColorOptions)
                _currentColorIndex = 0;
            // Update model later
            Entity.MarkToUpdateAppearances();
        }

        /// <summary>
        /// This function should be called by server or being called in character creation only, it is not allow client to set custom data.
        /// </summary>
        /// <param name="index"></param>
        public void SetModel(int index)
        {
            if (index < 0 || index >= MaxModelOptions)
                return;
            _currentModelIndex = index;
            _currentColorIndex = 0;
            // Save to entity's `PublicInts`
            Entity.SetPublicInt32(GetHashedModelSettingId(), _currentModelIndex);
            Entity.SetPublicInt32(GetHashedColorSettingId(), _currentColorIndex);
            // Update model later
            Entity.MarkToUpdateAppearances();
        }

        public int GetModel()
        {
            return _currentModelIndex;
        }

        /// <summary>
        /// This function should be called by server or being called in character creation only, it is not allow client to set custom data.
        /// </summary>
        /// <param name="index"></param>
        public void SetColor(int index)
        {
            if (index < 0 || index >= MaxColorOptions)
                return;
            _currentColorIndex = index;
            // Save to entity's `PublicInts`
            Entity.SetPublicInt32(GetHashedModelSettingId(), _currentModelIndex);
            Entity.SetPublicInt32(GetHashedColorSettingId(), _currentColorIndex);
            // Update model later
            Entity.MarkToUpdateAppearances();
        }

        public int GetColor()
        {
            return _currentColorIndex;
        }

        private void OnBeforeUpdateEquipmentModels(
            CancellationTokenSource cancellationTokenSource,
            BaseCharacterModel characterModel,
            Dictionary<string, EquipmentModel> showingModels,
            Dictionary<string, EquipmentModel> storingModels,
            HashSet<string> unequippingSockets)
        {
            characterModel.SetupEquippingModels(cancellationTokenSource, showingModels, storingModels, unequippingSockets, options[_currentModelIndex].models, CreateFakeEquipPosition(), CreateFakeCharacterItem(), false, 0, OnShowEquipmentModel).Forget();
        }

        private void OnPublicIntsOperation(LiteNetLibSyncList.Operation op, int index)
        {
            ApplyModelAndColorBySavedData();
        }

        protected virtual void OnShowEquipmentModel(EquipmentModel model, GameObject modelObject, BaseEquipmentEntity equipmentEntity, EquipmentInstantiatedObjectGroup instantiatedObjectGroup, EquipmentContainer equipmentContainer)
        {
            // Get mesh's material to change color
            if (model == null)
                return;

            if (model.indexOfModel < 0 || options[_currentModelIndex].colors.Length <= 0 || model.indexOfModel >= options[_currentModelIndex].colors[_currentColorIndex].ModelColorSettings.Length)
                return;

            ModelColorSetting modelColorSetting = options[_currentModelIndex].colors[_currentColorIndex].ModelColorSettings[model.indexOfModel];

            if (modelObject != null && modelColorSetting.materials.Length > 0)
            {
                SetMaterial(modelObject, modelColorSetting.materials);
            }

            if (instantiatedObjectGroup != null && modelColorSetting.materialGroups.Length > 0)
            {
                for (int i = 0; i < instantiatedObjectGroup.instantiatedObjects.Length; ++i)
                {
                    if (i >= modelColorSetting.materialGroups.Length)
                        break;
                    SetMaterial(instantiatedObjectGroup.instantiatedObjects[i], modelColorSetting.materialGroups[i].materials);
                }
            }
        }

        private void SetMaterial(GameObject modelObject, Material[] materials)
        {
            Renderer renderer = modelObject.GetComponentInChildren<Renderer>();
            if (renderer != null)
                renderer.materials = materials;
        }

        public int CreateFakeItemDataId()
        {
            return string.Concat("_BODY_PART_", modelSettingId, "_", _currentModelIndex, "_", _currentColorIndex).GenerateHashId();
        }

        public string CreateFakeEquipPosition()
        {
            return string.Concat("_BODY_PART_", modelSettingId);
        }

        public CharacterItem CreateFakeCharacterItem()
        {
            return new CharacterItem()
            {
                dataId = CreateFakeItemDataId(),
                level = 1,
            };
        }

        public int GetHashedModelSettingId()
        {
            return modelSettingId.GenerateHashId();
        }

        public int GetHashedColorSettingId()
        {
            return colorSettingId.GenerateHashId();
        }
    }
}
