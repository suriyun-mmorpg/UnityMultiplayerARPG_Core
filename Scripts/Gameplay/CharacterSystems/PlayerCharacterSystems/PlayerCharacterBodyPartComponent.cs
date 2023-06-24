using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayerCharacterBodyPartComponent : BaseGameEntityComponent<BasePlayerCharacterEntity>
    {
        public enum EColorSetupModes
        {
            ChangeMaterial,
            ChangeMaterialColor,
        }

        [System.Serializable]
        public class ColorSetting
        {
            [Header("Settings for UI")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];

            [Header("Settings for `Change Material Mode`")]
            public Material material;

            [Header("Settings for `Change Material Color Mode`")]
            public Color color = Color.white;
            public string materialPropertyName;

            public string Title
            {
                get { return Language.GetText(languageSpecificTitles, defaultTitle); }
            }
        }

        [System.Serializable]
        public class ModelSetting
        {
            [Header("Settings for UI")]
            public string defaultTitle = string.Empty;
            public LanguageData[] languageSpecificTitles = new LanguageData[0];

            [Header("Settings for in-game appearance")]
            public EquipmentModel model;
            public EColorSetupModes colorSetupMode;
            public ColorSetting[] colorSettings = new ColorSetting[0];

            public string Title
            {
                get { return Language.GetText(languageSpecificTitles, defaultTitle); }
            }
        }

        public string modelSettingId;
        public string colorSettingId;
        public ModelSetting[] settings = new ModelSetting[0];
        private bool _applying;
        private int _currentModelIndex;
        private int _currentColorIndex;
        public int MaxModelOptions { get => settings.Length; }
        public int MaxColorOptions { get => settings[_currentModelIndex].colorSettings.Length; }

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
