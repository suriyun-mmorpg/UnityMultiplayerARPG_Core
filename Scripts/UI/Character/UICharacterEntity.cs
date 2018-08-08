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
        public float visibleDistance = 30f;
        public UIFollowWorldObject rootFollower;
        public Text textTitle;
        public TextWrapper uiTextTitle;
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
            MigrateUIComponents();

            if (Data == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                CacheCanvas.enabled = false;
                return;
            }

            BaseCharacterEntity targetCharacter;
            if (BasePlayerCharacterController.OwningCharacter == Data)
                CacheCanvas.enabled = true;
            else if (Vector3.Distance(BasePlayerCharacterController.OwningCharacter.CacheTransform.position, Data.CacheTransform.position) > visibleDistance)
                CacheCanvas.enabled = false;
            else if (BasePlayerCharacterController.OwningCharacter.TryGetTargetEntity(out targetCharacter) && targetCharacter.ObjectId == Data.ObjectId)
                CacheCanvas.enabled = true;
            else
                CacheCanvas.enabled = false;

            if (uiTextTitle != null)
            {
                if (Data != null)
                    uiTextTitle.text = Data.Title;
                uiTextTitle.gameObject.SetActive(Data != null);
            }

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

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
        }
    }
}
