using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBodyPartComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        [System.Serializable]
        public class ColorSetting
        {
            public Material material;
            public bool changeMaterialColor;
            public string materialPropertyName;
            public Color color = Color.white;
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
        private bool _applying;
        private int _currentModelIndex;
        private int _currentColorIndex;
        public int MaxModelOptions { get => options.Length; }
        public int MaxColorOptions { get => options[_currentModelIndex].colors.Length; }

        public override void EntityAwake()
        {
            SetModel(0);
        }

        public override void EntityUpdate()
        {
            if (_applying)
            {
                Apply();
                _applying = false;
            }
        }

        public void SetModel(int index)
        {
            if (index < 0 || index >= MaxModelOptions)
                return;
            _currentModelIndex = index;
            _currentColorIndex = 0;
            _applying = true;
            // TODO: Save to entity's `PublicInts`
        }

        public int GetModel()
        {
            return _currentModelIndex;
        }

        public void SetColor(int index)
        {
            if (index < 0 || index >= MaxColorOptions)
                return;
            _currentColorIndex = index;
            _applying = true;
            // TODO: Save to entity's `PublicInts`
        }

        public void Apply()
        {
            // TODO: Instantiate or activate game object then change material or change material's color
        }
    }
}
