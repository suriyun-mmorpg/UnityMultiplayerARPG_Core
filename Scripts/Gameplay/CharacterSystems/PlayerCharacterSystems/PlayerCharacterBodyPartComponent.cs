using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBodyPartComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [System.Serializable]
        public class MaterialColorSetting
        {
            public string propertyName;
            public Color color = Color.white;
        }

        [System.Serializable]
        public class ColorSetting
        {
            public Material material;
            public MaterialColorSetting[] materialSettings = new MaterialColorSetting[0];
        }

        [System.Serializable]
        public class ColorOption
        {
            [Header("Settings for UI")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];

            [Header("Settings for in-game appearance")]
            [Tooltip("Color settings for each model, its index will be the same as `models`'s index")]
            public ColorSetting[] settings = new ColorSetting[0];

            public string Title
            {
                get { return Language.GetText(languageSpecificTitles, defaultTitle); }
            }
        }

        [System.Serializable]
        public class ModelOption
        {
            [Header("Settings for UI")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];

            [Header("Settings for in-game appearance")]
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
        public int MaxModelOptions { get => options.Length; }
        public int MaxColorOptions { get => options[_currentModelIndex].colors.Length; }

        public override void EntityAwake()
        {
            SetModel(0);
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

        public void Apply()
        {
            // TODO: Instantiate or activate game object then change material or change material's color
        }
    }
}
