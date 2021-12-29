using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIRewarding : UISelectionEntry<UIRewardingData>
    {
        [Tooltip("Format => {0} = {Exp Amount}")]
        public UILocaleKeySetting formatKeyRewardExp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REWARD_EXP);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyRewardGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REWARD_GOLD);
        [Tooltip("Format => {0} = {Cash Amount}")]
        public UILocaleKeySetting formatKeyRewardCash = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REWARD_CASH);
        public TextWrapper textRewardExp;
        public TextWrapper textRewardGold;
        public TextWrapper textRewardCash;
        public UICharacterItems uiRewardItems;
        public UIRewardItemSkins uiRewardSkins;
        public UIRewardCostumes uiRewardCostumes;
        public UIRewardCharacters uiRewardCharacters;
        public UIRewardCompanions uiRewardCompanions;

        protected override void UpdateData()
        {
            if (textRewardExp != null)
            {
                textRewardExp.SetGameObjectActive(Data.rewardExp != 0);
                textRewardExp.text = string.Format(
                    LanguageManager.GetText(formatKeyRewardExp),
                    Data.rewardExp.ToString("N0"));
            }

            if (textRewardGold != null)
            {
                textRewardGold.SetGameObjectActive(Data.rewardGold != 0);
                textRewardGold.text = string.Format(
                    LanguageManager.GetText(formatKeyRewardGold),
                    Data.rewardGold.ToString("N0"));
            }

            if (textRewardCash != null)
            {
                textRewardCash.SetGameObjectActive(Data.rewardCash != 0);
                textRewardCash.text = string.Format(
                    LanguageManager.GetText(formatKeyRewardCash),
                    Data.rewardCash.ToString("N0"));
            }

            if (uiRewardItems != null)
            {
                if (Data.rewardItems != null && Data.rewardItems.Length > 0)
                {
                    uiRewardItems.UpdateData(GameInstance.PlayingCharacter, Data.rewardItems);
                    uiRewardItems.Show();
                }
                else
                {
                    uiRewardItems.Hide();
                }
            }

            if (uiRewardSkins != null)
            {
                if (Data.rewardSkins != null && Data.rewardSkins.Length > 0)
                {
                    uiRewardSkins.UpdateData(Data.rewardSkins);
                    uiRewardSkins.Show();
                }
                else
                {
                    uiRewardSkins.Hide();
                }
            }

            if (uiRewardCostumes != null)
            {
                if (Data.rewardCostumes != null && Data.rewardCostumes.Length > 0)
                {
                    uiRewardCostumes.UpdateData(Data.rewardCostumes);
                    uiRewardCostumes.Show();
                }
                else
                {
                    uiRewardCostumes.Hide();
                }
            }

            if (uiRewardCharacters != null)
            {
                if (Data.rewardCharacters != null && Data.rewardCharacters.Length > 0)
                {
                    uiRewardCharacters.UpdateData(Data.rewardCharacters);
                    uiRewardCharacters.Show();
                }
                else
                {
                    uiRewardCharacters.Hide();
                }
            }

            if (uiRewardCompanions != null)
            {
                if (Data.rewardCompanions != null && Data.rewardCompanions.Length > 0)
                {
                    uiRewardCompanions.UpdateData(Data.rewardCompanions);
                    uiRewardCompanions.Show();
                }
                else
                {
                    uiRewardCompanions.Hide();
                }
            }
        }
    }
}
