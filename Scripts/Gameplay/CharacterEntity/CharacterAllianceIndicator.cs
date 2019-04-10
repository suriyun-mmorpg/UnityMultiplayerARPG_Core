using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterAllianceIndicator : MonoBehaviour
    {
        [Tooltip("This will activate when character is owning character")]
        public GameObject owningIndicator;
        [Tooltip("This will activate when character is ally with owning character")]
        public GameObject allyIndicator;
        [Tooltip("This will activate when character is enemy with owning character")]
        public GameObject enemyIndicator;
        [Tooltip("This will activate when character is neutral with owning character")]
        public GameObject neutralIndicator;
        public float updateWithinRange = 30f;
        public float updateRepeatRate = 0.5f;
        [HideInInspector, System.NonSerialized]
        public BaseCharacterEntity characterEntity;
        private float lastUpdateTime;

        private void Awake()
        {
            if (characterEntity == null)
                characterEntity = GetComponentInParent<BaseCharacterEntity>();
        }

        private void Update()
        {
            if (characterEntity == null ||
                BasePlayerCharacterController.OwningCharacter == null ||
                Vector3.Distance(characterEntity.CacheTransform.position, BasePlayerCharacterController.OwningCharacter.CacheTransform.position) > updateWithinRange)
            {
                if (owningIndicator != null)
                    owningIndicator.SetActive(false);
                if (allyIndicator != null)
                    allyIndicator.SetActive(false);
                if (enemyIndicator != null)
                    enemyIndicator.SetActive(false);
                if (neutralIndicator != null)
                    neutralIndicator.SetActive(false);
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;
                if (owningIndicator != null)
                    owningIndicator.SetActive(false);
                if (allyIndicator != null)
                    allyIndicator.SetActive(false);
                if (enemyIndicator != null)
                    enemyIndicator.SetActive(false);
                if (neutralIndicator != null)
                    neutralIndicator.SetActive(false);
                if (BasePlayerCharacterController.OwningCharacter == characterEntity)
                {
                    if (owningIndicator != null)
                        owningIndicator.SetActive(true);
                }
                if (characterEntity.IsAlly(BasePlayerCharacterController.OwningCharacter))
                {
                    if (allyIndicator != null)
                        allyIndicator.SetActive(true);
                }
                if (characterEntity.IsEnemy(BasePlayerCharacterController.OwningCharacter))
                {
                    if (enemyIndicator != null)
                        enemyIndicator.SetActive(true);
                }
                if (characterEntity.IsNeutral(BasePlayerCharacterController.OwningCharacter))
                {
                    if (neutralIndicator != null)
                        neutralIndicator.SetActive(true);
                }
            }
        }
    }
}
