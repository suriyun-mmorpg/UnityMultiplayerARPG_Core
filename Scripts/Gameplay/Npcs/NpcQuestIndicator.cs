using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class NpcQuestIndicator : MonoBehaviour
    {
        [Tooltip("This will activate when there are in progress quests")]
        public GameObject haveInProgressQuestIndicator;
        [Tooltip("This will activate when there are new quests")]
        public GameObject haveNewQuestIndicator;
        public float updateRepeatRate = 0.5f;
        [HideInInspector, System.NonSerialized]
        public NpcEntity npcEntity;
        private float lastUpdateTime;

        private void Awake()
        {
            if (npcEntity == null)
                npcEntity = GetComponentInParent<NpcEntity>();
        }

        private void Update()
        {
            if (npcEntity == null || BasePlayerCharacterController.OwningCharacter == null)
            {
                if (haveInProgressQuestIndicator != null)
                    haveInProgressQuestIndicator.SetActive(false);
                if (haveNewQuestIndicator != null)
                    haveNewQuestIndicator.SetActive(false);
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;
                if (haveInProgressQuestIndicator != null)
                    haveInProgressQuestIndicator.SetActive(false);
                if (haveNewQuestIndicator != null)
                    haveNewQuestIndicator.SetActive(false);
                if (npcEntity.HaveInProgressQuests(BasePlayerCharacterController.OwningCharacter))
                {
                    if (haveInProgressQuestIndicator != null)
                        haveInProgressQuestIndicator.SetActive(true);
                }
                else if (npcEntity.HaveNewQuests(BasePlayerCharacterController.OwningCharacter))
                {
                    if (haveNewQuestIndicator != null)
                        haveNewQuestIndicator.SetActive(true);
                }
            }
        }
    }
}
