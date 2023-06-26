using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBodyPartComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [System.Serializable]
        public class MaterialPropertySetting
        {
            public string propertyName;
            public Color color = Color.white;
        }

        [System.Serializable]
        public class MaterialSetting
        {
            public Material material;
            public MaterialPropertySetting[] properties = new MaterialPropertySetting[0];
        }

        [System.Serializable]
        public class ModelColorSetting
        {
            [Tooltip("Material Settings for each mesh's materials, its index is index of `MeshRenderer` -> `materials`")]
            public MaterialSetting[] materialSettings = new MaterialSetting[0];

            public Material[] GetMaterials()
            {
                Material[] result = new Material[materialSettings.Length];
                Material tempMaterial;
                for (int i = 0; i < materialSettings.Length; ++i)
                {
                    tempMaterial = new Material(materialSettings[i].material);
                    for (int j = 0; j < materialSettings[i].properties.Length; ++j)
                    {
                        tempMaterial.SetColor(materialSettings[i].properties[j].propertyName, materialSettings[i].properties[j].color);
                    }
                    result[i] = tempMaterial;
                }
                return result;
            }
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
        private int _currentModelIndex;
        private int _currentColorIndex;
        public IEnumerable<ModelOption> ModelOptions { get => options; }
        public int MaxModelOptions { get => options.Length; }
        public IEnumerable<ColorOption> ColorOptions { get => options[_currentModelIndex].colors; }
        public int MaxColorOptions { get => options[_currentModelIndex].colors.Length; }

        public override void EntityAwake()
        {
            SetModel(0);
        }

        public override void EntityStart()
        {
            SetupEvents();
        }

        public override void EntityOnDestroy()
        {
            ClearEvents();
        }

        public void SetupEvents()
        {
            ClearEvents();
            Entity.CharacterModel.onBeforeUpdateEquipmentModels += OnBeforeUpdateEquipmentModels;
        }

        public void ClearEvents()
        {
            Entity.CharacterModel.onBeforeUpdateEquipmentModels -= OnBeforeUpdateEquipmentModels;
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
            Entity.SetPublicInt32(modelSettingId.GenerateHashId(), _currentModelIndex);
            Entity.SetPublicInt32(colorSettingId.GenerateHashId(), _currentColorIndex);
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
            Entity.SetPublicInt32(modelSettingId.GenerateHashId(), _currentModelIndex);
            Entity.SetPublicInt32(colorSettingId.GenerateHashId(), _currentColorIndex);
            // Update model later
            Entity.MarkToUpdateAppearances();
        }

        public int GetColor()
        {
            return _currentColorIndex;
        }

        private void OnBeforeUpdateEquipmentModels(
            BaseCharacterModel characterModel,
            Dictionary<string, EquipmentModel> showingModels,
            Dictionary<string, EquipmentModel> storingModels,
            HashSet<string> unequippingSockets)
        {
            characterModel.SetupEquippingModels(showingModels, storingModels, unequippingSockets, options[_currentModelIndex].models, CreateFakeItemDataId(), 1, CreateFakeEquipPosition(), false, 0, OnShowEquipmentModel);
        }

        private void OnShowEquipmentModel(EquipmentModel model, GameObject modelObject, BaseEquipmentEntity equipmentEntity, EquipmentContainer equipmentContainer)
        {
            // Get mesh's material to change color
            if (model == null || modelObject == null)
                return;

            if (model.indexOfModel < 0 || options[_currentModelIndex].colors.Length <= 0 || model.indexOfModel >= options[_currentModelIndex].colors[_currentColorIndex].ModelColorSettings.Length)
                return;
            
            MeshRenderer meshRenderer = modelObject.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer == null)
                return;
            meshRenderer.materials = options[_currentModelIndex].colors[_currentColorIndex].ModelColorSettings[model.indexOfModel].GetMaterials();
        }

        public int CreateFakeItemDataId()
        {
            return string.Concat("_BODY_PART_", modelSettingId, "_", _currentModelIndex, "_", _currentColorIndex).GenerateHashId();
        }

        public string CreateFakeEquipPosition()
        {
            return string.Concat("_BODY_PART_" , modelSettingId);
        }
    }
}
