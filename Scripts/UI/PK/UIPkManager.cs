using Cysharp.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPkManager : UIBase
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {PK Point}")]
        public UILocaleKeySetting formatPkPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Consecutive PK Kills}")]
        public UILocaleKeySetting formatConsecutivePkKills = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public GameObject[] pkOnObjects = new GameObject[0];
        public GameObject[] pkOffObjects = new GameObject[0];
        public TextWrapper uiTextPkPoint;
        public TextWrapper uiTextConsecutivePkKills;

        private BasePlayerCharacterEntity _previousEntity;

        private void Start()
        {
            GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
            GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        private void OnDestroy()
        {
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
            GameInstance_onSetPlayingCharacter(null);
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            RemoveEvents(_previousEntity);
            BasePlayerCharacterEntity playerCharacterEntity = playingCharacterData as BasePlayerCharacterEntity;
            _previousEntity = playerCharacterEntity;
            AddEvents(_previousEntity);
            if (playerCharacterEntity != null)
            {
                PlayingCharacterEntity_onIsPkOnChange(playingCharacterData.IsPkOn);
                PlayingCharacterEntity_onPkPointChange(playingCharacterData.PkPoint);
                PlayingCharacterEntity_onConsecutivePkKillsChange(playingCharacterData.ConsecutivePkKills);
            }
        }

        private void AddEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onIsPkOnChange += PlayingCharacterEntity_onIsPkOnChange;
            PlayingCharacterEntity.onPkPointChange += PlayingCharacterEntity_onPkPointChange;
            PlayingCharacterEntity.onConsecutivePkKillsChange += PlayingCharacterEntity_onConsecutivePkKillsChange;
        }

        private void RemoveEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onIsPkOnChange -= PlayingCharacterEntity_onIsPkOnChange;
            PlayingCharacterEntity.onPkPointChange -= PlayingCharacterEntity_onPkPointChange;
            PlayingCharacterEntity.onConsecutivePkKillsChange -= PlayingCharacterEntity_onConsecutivePkKillsChange;
        }

        private void PlayingCharacterEntity_onIsPkOnChange(bool val)
        {
            foreach (GameObject obj in pkOnObjects)
            {
                obj.SetActive(val);
            }

            foreach (GameObject obj in pkOffObjects)
            {
                obj.SetActive(!val);
            }
        }

        private void PlayingCharacterEntity_onPkPointChange(int val)
        {
            if (uiTextPkPoint != null)
                uiTextPkPoint.text = ZString.Format(LanguageManager.GetText(formatPkPoint), val);
        }

        private void PlayingCharacterEntity_onConsecutivePkKillsChange(int val)
        {
            if (uiTextConsecutivePkKills != null)
                uiTextConsecutivePkKills.text = ZString.Format(LanguageManager.GetText(formatConsecutivePkKills), val);
        }

        public void OnClickTogglePk()
        {
            UISceneGlobal.Singleton.ShowMessageDialog(
                LanguageManager.GetText(UITextKeys.UI_LABEL_WARNING.ToString()),
                GameInstance.Singleton.GameplayRule.GetTurnPkOnWarningMessage(),
                false, true, true, false, onClickYes: GameInstance.PlayingCharacterEntity.Pk.TogglePkMode);
        }
    }
}
