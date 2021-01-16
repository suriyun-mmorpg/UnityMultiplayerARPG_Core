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
        private BaseCharacterEntity characterEntity;
        private float lastUpdateTime;

        private bool tempVisibleResult;

        private void Awake()
        {
            characterEntity = GetComponentInParent<BaseCharacterEntity>();
        }

        private void Update()
        {
            if (characterEntity == null || !characterEntity.IsClient ||
                GameInstance.PlayingCharacterEntity == null ||
                (characterEntity.IsServer && characterEntity.Identity.CountSubscribers() == 0) ||
                Vector3.Distance(characterEntity.CacheTransform.position, GameInstance.PlayingCharacterEntity.CacheTransform.position) > updateWithinRange)
            {
                if (owningIndicator != null && owningIndicator.activeSelf)
                    owningIndicator.SetActive(false);
                if (allyIndicator != null && allyIndicator.activeSelf)
                    allyIndicator.SetActive(false);
                if (enemyIndicator != null && enemyIndicator.activeSelf)
                    enemyIndicator.SetActive(false);
                if (neutralIndicator != null && neutralIndicator.activeSelf)
                    neutralIndicator.SetActive(false);
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;

                tempVisibleResult = GameInstance.PlayingCharacterEntity == characterEntity;
                if (owningIndicator != null && owningIndicator.activeSelf != tempVisibleResult)
                    owningIndicator.SetActive(tempVisibleResult);

                if (tempVisibleResult)
                    return;

                tempVisibleResult = characterEntity.IsAlly(GameInstance.PlayingCharacterEntity);
                if (allyIndicator != null && allyIndicator.activeSelf != tempVisibleResult)
                    allyIndicator.SetActive(tempVisibleResult);

                if (tempVisibleResult)
                    return;

                tempVisibleResult = characterEntity.IsEnemy(GameInstance.PlayingCharacterEntity);
                if (enemyIndicator != null && enemyIndicator.activeSelf != tempVisibleResult)
                    enemyIndicator.SetActive(tempVisibleResult);

                if (tempVisibleResult)
                    return;

                tempVisibleResult = characterEntity.IsNeutral(GameInstance.PlayingCharacterEntity);
                if (neutralIndicator != null && neutralIndicator.activeSelf != tempVisibleResult)
                    neutralIndicator.SetActive(tempVisibleResult);
            }
        }
    }
}
