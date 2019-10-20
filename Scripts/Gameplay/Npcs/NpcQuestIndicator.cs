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
        public float updateWithinRange = 30f;
        public float updateRepeatRate = 0.5f;
        [HideInInspector, System.NonSerialized]
        public NpcEntity npcEntity;
        private float lastUpdateTime;

        private bool tempVisibleResult;

        private void Awake()
        {
            if (npcEntity == null)
                npcEntity = GetComponentInParent<NpcEntity>();
        }

        private void Update()
        {
            if (npcEntity == null ||
                BasePlayerCharacterController.OwningCharacter == null ||
                Vector3.Distance(npcEntity.CacheTransform.position, BasePlayerCharacterController.OwningCharacter.CacheTransform.position) > updateWithinRange)
            {
                if (haveInProgressQuestIndicator != null && haveInProgressQuestIndicator.activeSelf)
                    haveInProgressQuestIndicator.SetActive(false);
                if (haveNewQuestIndicator != null && haveNewQuestIndicator.activeSelf)
                    haveNewQuestIndicator.SetActive(false);
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;
                // Indicator priority haveInProgress > haveNewQuest
                tempVisibleResult = npcEntity.HaveInProgressQuests(BasePlayerCharacterController.OwningCharacter);
                if (haveInProgressQuestIndicator != null && haveInProgressQuestIndicator.activeSelf != tempVisibleResult)
                    haveInProgressQuestIndicator.SetActive(tempVisibleResult);

                if (!tempVisibleResult)
                    tempVisibleResult = npcEntity.HaveNewQuests(BasePlayerCharacterController.OwningCharacter);
                else
                    tempVisibleResult = false;

                if (haveNewQuestIndicator != null && haveNewQuestIndicator.activeSelf != tempVisibleResult)
                    haveNewQuestIndicator.SetActive(tempVisibleResult);
            }
        }
    }
}
