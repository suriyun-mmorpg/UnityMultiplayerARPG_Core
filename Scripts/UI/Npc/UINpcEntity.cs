using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UINpcEntity : UISelectionEntry<NpcEntity>
    {
        public bool placeAsChild;
        public float placeAsChildScale = 1f;
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
            MigrateUIComponents();

            Profiler.BeginSample("UINpcEntity - Update UI");
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
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            if (rootFollower != null)
            {
                if (Data != null)
                    rootFollower.TargetObject = Data.UIElementTransform;
                rootFollower.gameObject.SetActive(Data != null);
            }

            if (placeAsChild)
            {
                transform.SetParent(Data.UIElementTransform);
                transform.localPosition = Vector3.zero;
                transform.localScale = Vector3.one * placeAsChildScale;
            }
        }

        [ContextMenu("Migrate UI Components")]
        public void MigrateUIComponents()
        {
            uiTextTitle = MigrateUIHelpers.SetWrapperToText(textTitle, uiTextTitle);
        }
    }
}
