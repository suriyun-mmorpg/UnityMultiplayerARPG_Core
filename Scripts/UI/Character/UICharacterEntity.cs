using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Canvas))]
    public class UICharacterEntity : UIDamageableEntity<BaseCharacterEntity>
    {
        public enum Visibility
        {
            VisibleWhenSelected,
            VisibleWhenNearby,
            AlwaysVisible,
        }
        public Visibility visibility;
        [Tooltip("Visible when hit duration for non owning character")]
        public float visibleWhenHitDuration = 2f;
        public float visibleDistance = 30f;
        
        [Header("Character Entity - Display Format")]
        [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}")]
        public string mpFormat = "Mp: {0}/{1}";

        [Header("Character Entity - UI Elements")]
        public TextWrapper uiTextMp;
        public Image imageMpGage;

        private float lastShowTime;
        private BasePlayerCharacterEntity tempOwningCharacter;
        private BaseCharacterEntity tempTargetCharacter;
        protected int currentMp;
        protected int maxMp;

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

        protected override void Update()
        {
            base.Update();

            currentMp = 0;
            maxMp = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.CacheMaxMp;
            }

            if (uiTextMp != null)
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacterEntity - Update UI");
            if (Data == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                CacheCanvas.enabled = false;
                return;
            }

            if (Data.CurrentHp == 0)
            {
                CacheCanvas.enabled = false;
                return;
            }

            tempOwningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (tempOwningCharacter == Data)
                CacheCanvas.enabled = true;
            else
            {
                switch (visibility)
                {
                    case Visibility.VisibleWhenSelected:
                        CacheCanvas.enabled = tempOwningCharacter.TryGetTargetEntity(out tempTargetCharacter) &&
                            tempTargetCharacter.ObjectId == Data.ObjectId &&
                            Vector3.Distance(tempOwningCharacter.CacheTransform.position, Data.CacheTransform.position) <= visibleDistance;
                        break;
                    case Visibility.VisibleWhenNearby:
                        CacheCanvas.enabled = Vector3.Distance(tempOwningCharacter.CacheTransform.position, Data.CacheTransform.position) <= visibleDistance;
                        break;
                    case Visibility.AlwaysVisible:
                        CacheCanvas.enabled = true;
                        break;
                }
            }
            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
        }
    }
}
