using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSummon : UIDataForCharacter<CharacterSummon>
    {
        public CharacterSummon CharacterSummon { get { return Data; } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Remains Duration}")]
        public UILocaleKeySetting formatKeySummonRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Stack Amount}")]
        public UILocaleKeySetting formatKeySummonStack = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public Image imageIcon;
        public TextWrapper uiTextRemainsDuration;
        public TextWrapper uiTextStack;
        public UICharacter uiCharacter;

        [Header("Events")]
        public UnityEvent onTypeIsSkill;
        public UnityEvent onTypeIsPet;
        public UnityEvent onStackEntriesEmpty;
        public UnityEvent onStackEntriesNotEmpty;

        protected readonly Dictionary<uint, CharacterSummon> stackingEntries = new Dictionary<uint, CharacterSummon>();
        protected float summonRemainsDuration;
        private BaseGameData tempSummonData;

        private void OnDisable()
        {
            summonRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (summonRemainsDuration <= 0f)
            {
                summonRemainsDuration = CharacterSummon.summonRemainsDuration;
                if (summonRemainsDuration <= 1f)
                    summonRemainsDuration = 0f;
            }

            if (summonRemainsDuration > 0f)
            {
                summonRemainsDuration -= Time.deltaTime;
                if (summonRemainsDuration <= 0f)
                    summonRemainsDuration = 0f;
            }
            else
                summonRemainsDuration = 0f;

            // Update UIs
            if (uiTextRemainsDuration != null)
            {
                uiTextRemainsDuration.text = string.Format(
                    LanguageManager.GetText(formatKeySummonRemainsDuration),
                    summonRemainsDuration.ToString("N0"));
                uiTextRemainsDuration.gameObject.SetActive(summonRemainsDuration > 0);
            }
        }

        protected override void UpdateData()
        {
            tempSummonData = null;
            summonRemainsDuration = CharacterSummon.summonRemainsDuration;

            switch (Data.type)
            {
                case SummonType.Skill:
                    onTypeIsSkill.Invoke();
                    tempSummonData = Data.GetSkill();
                    break;
                case SummonType.Pet:
                    onTypeIsPet.Invoke();
                    tempSummonData = Data.GetPetItem();
                    break;
            }

            if (uiTextTitle != null)
            {
                uiTextTitle.text = string.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    !tempSummonData ? LanguageManager.GetUnknowTitle() : tempSummonData.Title);
            }

            if (imageIcon != null)
            {
                Sprite iconSprite = !tempSummonData ? null : tempSummonData.icon;
                imageIcon.gameObject.SetActive(iconSprite);
                imageIcon.sprite = iconSprite;
            }

            if (uiCharacter != null)
            {
                if (!tempSummonData || !Data.CacheEntity)
                    uiCharacter.Hide();
                else
                {
                    uiCharacter.Show();
                    uiCharacter.Data = Data.CacheEntity;
                }
            }
        }

        public override void Setup(CharacterSummon data, ICharacterData character, int indexOfData)
        {
            base.Setup(data, character, indexOfData);
            ClearStackingEntries();
        }

        private void OnStackingEntriesUpdate()
        {
            if (uiTextStack != null)
            {
                uiTextStack.text = string.Format(
                    LanguageManager.GetText(formatKeySummonStack),
                    stackingEntries.Count + 1);
            }

            if (stackingEntries.Count > 0)
                onStackEntriesNotEmpty.Invoke();
            else
                onStackEntriesEmpty.Invoke();
        }

        public void AddStackingEntry(CharacterSummon summon)
        {
            stackingEntries[summon.objectId] = summon;
            OnStackingEntriesUpdate();
        }

        public void RemoveStackingEntry(uint objectId)
        {
            stackingEntries.Remove(objectId);
            OnStackingEntriesUpdate();
        }

        public void ClearStackingEntries()
        {
            stackingEntries.Clear();
            OnStackingEntriesUpdate();
        }

        public void OnClickUnSummon()
        {
            if (CharacterSummon.type == SummonType.Pet)
                OwningCharacter.RequestUnSummon(CharacterSummon.objectId);
        }
    }
}
