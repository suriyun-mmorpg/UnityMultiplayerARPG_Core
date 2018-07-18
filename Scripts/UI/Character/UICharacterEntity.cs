using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UICharacterEntity : UISelectionEntry<BaseCharacterEntity>
    {
        [Tooltip("Visible when hit duration for non owning character")]
        public float visibleWhenHitDuration = 2f;
        public UIFollowWorldObject rootFollower;
        public Text textTitle;
        public UICharacter uiCharacter;
        private float lastShowTime;

        private Canvas cacheCanvas;
        public Canvas CacheCanvas
        {
            get
            {
                if (cacheCanvas == null)
                    cacheCanvas = GetComponent<Canvas>();
                return cacheCanvas;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            CacheCanvas.enabled = false;
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            if (Data == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                CacheCanvas.enabled = false;
                return;
            }

            BaseCharacterEntity targetCharacter;
            if (BasePlayerCharacterController.OwningCharacter == Data)
                CacheCanvas.enabled = true;
            else if (BasePlayerCharacterController.OwningCharacter.TryGetTargetEntity(out targetCharacter) && targetCharacter.ObjectId == Data.ObjectId)
                CacheCanvas.enabled = true;
            else
                CacheCanvas.enabled = false;

            if (uiCharacter != null)
            {
                if (Data.CurrentHp > 0)
                {
                    if (!uiCharacter.IsVisible())
                        uiCharacter.Show();
                }
                else
                {
                    if (uiCharacter.IsVisible())
                        uiCharacter.Hide();
                }
            }
        }

        protected override void UpdateData()
        {
            if (textTitle != null)
            {
                if (Data != null)
                    textTitle.text = Data.Title;
                textTitle.gameObject.SetActive(Data != null);
            }

            if (uiCharacter != null)
            {
                if (Data != null)
                {
                    if (!uiCharacter.IsVisible())
                        uiCharacter.Show();
                    uiCharacter.Data = Data;
                }
                else
                {
                    if (uiCharacter.IsVisible())
                        uiCharacter.Hide();
                }
            }

            if (rootFollower != null)
            {
                if (Data != null)
                    rootFollower.TargetObject = Data.UIElementTransform;
                rootFollower.gameObject.SetActive(Data != null);
            }
        }
    }
}
