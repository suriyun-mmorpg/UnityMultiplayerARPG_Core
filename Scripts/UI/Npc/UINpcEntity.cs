using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UINpcEntity : UISelectionEntry<NpcEntity>
    {
        public float visibleDistance = 30f;
        public UIFollowWorldObject rootFollower;
        public Text textTitle;
        public TextWrapper uiTextTitle;

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
            UpdateUIComponents();

            if (Data == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                CacheCanvas.enabled = false;
                return;
            }

            NpcEntity targetNpc;
            if (Vector3.Distance(BasePlayerCharacterController.OwningCharacter.CacheTransform.position, Data.CacheTransform.position) > visibleDistance)
                CacheCanvas.enabled = false;
            else if (BasePlayerCharacterController.OwningCharacter.TryGetTargetEntity(out targetNpc) && targetNpc.ObjectId == Data.ObjectId)
                CacheCanvas.enabled = true;
            else
                CacheCanvas.enabled = false;

            if (uiTextTitle != null)
            {
                uiTextTitle.text = Data.Title;
                uiTextTitle.gameObject.SetActive(Data != null);
            }
        }

        protected override void UpdateData()
        {
            if (rootFollower != null)
            {
                if (Data != null)
                    rootFollower.TargetObject = Data.UIElementTransform;
                rootFollower.gameObject.SetActive(Data != null);
            }
        }

        [ContextMenu("Update UI Components")]
        public void UpdateUIComponents()
        {
            uiTextTitle = UIWrapperHelpers.SetWrapperToText(textTitle, uiTextTitle);
        }
    }
}
