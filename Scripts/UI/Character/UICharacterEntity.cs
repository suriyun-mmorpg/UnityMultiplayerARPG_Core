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
        [Tooltip("Level Format => {0} = {Level}")]
        public string levelFormat = "Lv: {0}";
        [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}")]
        public string mpFormat = "Mp: {0}/{1}";
        [Tooltip("Skill Cast Format => {0} = {Count down duration}")]
        public string skillCastFormat = "{0}";

        [Header("Character Entity - UI Elements")]
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextMp;
        public Image imageMpGage;
        public GameObject uiSkillCastContainer;
        public TextWrapper uiTextSkillCast;
        public Image imageSkillCastGage;
        public UICharacter uiCharacter;

        private float lastShowTime;
        private BasePlayerCharacterEntity tempOwningCharacter;
        private BaseCharacterEntity tempTargetCharacter;
        protected int currentMp;
        protected int maxMp;
        protected float castingSkillCountDown;
        protected float castingSkillDuration;

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

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Data == null ? "0" : Data.Level.ToString("N0"));

            currentMp = 0;
            maxMp = 0;
            castingSkillCountDown = 0;
            castingSkillDuration = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.CacheMaxMp;
                castingSkillCountDown = Data.castingSkillCountDown;
                castingSkillDuration = Data.castingSkillDuration;
            }

            if (uiTextMp != null)
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            if (uiSkillCastContainer != null)
                uiSkillCastContainer.SetActive(castingSkillCountDown > 0 && castingSkillDuration > 0);

            if (uiTextSkillCast != null)
                uiTextSkillCast.text = string.Format(skillCastFormat, castingSkillCountDown.ToString("N0"));

            if (imageSkillCastGage != null)
                imageSkillCastGage.fillAmount = castingSkillDuration <= 0 ? 0 : 1 - (castingSkillCountDown / castingSkillDuration);
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
            {
                // Always show the UI when character is owning character
                CacheCanvas.enabled = true;
            }
            else
            {
                switch (visibility)
                {
                    case Visibility.VisibleWhenSelected:
                        tempTargetCharacter = null;
                        if (BasePlayerCharacterController.Singleton.SelectedEntity != null)
                            tempTargetCharacter = BasePlayerCharacterController.Singleton.SelectedEntity as BaseCharacterEntity;
                        CacheCanvas.enabled = tempTargetCharacter != null &&
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

            // Update character UI every `updateUIRepeatRate` seconds
            if (uiCharacter != null)
                uiCharacter.Data = Data;

            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
        }
    }
}
