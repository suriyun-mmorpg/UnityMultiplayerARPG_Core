using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class NpcQuestIndicator : MonoBehaviour
    {
        public float visibleDistance = 30f;
        public GameObject haveInProgressQuestIndicator;
        public GameObject haveNewQuestIndicator;
        public float updateRepeatRate = 0.5f;
        public NpcEntity npcEntity;
        private float lastUpdateTime;

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
                if (haveInProgressQuestIndicator != null)
                    haveInProgressQuestIndicator.SetActive(false);
                if (haveNewQuestIndicator != null)
                    haveNewQuestIndicator.SetActive(false);
                if (Vector3.Distance(BasePlayerCharacterController.OwningCharacter.CacheTransform.position, npcEntity.CacheTransform.position) <= visibleDistance)
                {
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
                lastUpdateTime = Time.unscaledTime;
            }
        }
    }
}
