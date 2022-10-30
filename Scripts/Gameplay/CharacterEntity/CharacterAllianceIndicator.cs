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
        [Tooltip("This will activate when character is in the same party with owning character")]
        public GameObject partyMemberIndicator;
        [Tooltip("This will activate when character is in the same guild with owning character")]
        public GameObject guildMemberIndicator;
        [Tooltip("This will activate when character is enemy with owning character")]
        public GameObject enemyIndicator;
        [Tooltip("This will activate when character is neutral with owning character")]
        public GameObject neutralIndicator;
        public float updateWithinRange = 30f;
        public float updateRepeatRate = 0.5f;
        private BaseCharacterEntity characterEntity;
        private float lastUpdateTime;

        private void Awake()
        {
            characterEntity = GetComponentInParent<BaseCharacterEntity>();
        }

        private void Update()
        {
            if (characterEntity == null || !characterEntity.IsClient || (characterEntity.IsServer && characterEntity.Identity.CountSubscribers() == 0) ||
                GameInstance.PlayingCharacterEntity == null || Vector3.Distance(characterEntity.EntityTransform.position, GameInstance.PlayingCharacterEntity.EntityTransform.position) > updateWithinRange)
            {
                HideAll();
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;
                HideAll();
                EntityInfo entityInfo = characterEntity.GetInfo();
                List<GameObject> showingObjects = new List<GameObject>();
                bool isShowing;

                isShowing = GameInstance.PlayingCharacterEntity == characterEntity;
                if (owningIndicator != null && isShowing && !showingObjects.Contains(owningIndicator))
                    showingObjects.Add(owningIndicator);

                isShowing = characterEntity.IsAlly(GameInstance.PlayingCharacterEntity.GetInfo());
                if (allyIndicator != null && isShowing && !showingObjects.Contains(allyIndicator))
                    showingObjects.Add(allyIndicator);

                isShowing = entityInfo.PartyId > 0 && entityInfo.PartyId == GameInstance.PlayingCharacter.PartyId;
                if (partyMemberIndicator != null && isShowing && !showingObjects.Contains(partyMemberIndicator))
                    showingObjects.Add(partyMemberIndicator);

                isShowing = entityInfo.GuildId > 0 && entityInfo.GuildId == GameInstance.PlayingCharacter.GuildId;
                if (guildMemberIndicator != null && isShowing && !showingObjects.Contains(guildMemberIndicator))
                    showingObjects.Add(guildMemberIndicator);

                isShowing = characterEntity.IsEnemy(GameInstance.PlayingCharacterEntity.GetInfo());
                if (enemyIndicator != null && isShowing && !showingObjects.Contains(enemyIndicator))
                    showingObjects.Add(enemyIndicator);

                isShowing = characterEntity.IsNeutral(GameInstance.PlayingCharacterEntity.GetInfo());
                if (neutralIndicator != null && isShowing && !showingObjects.Contains(neutralIndicator))
                    showingObjects.Add(neutralIndicator);

                for (int i = 0; i < showingObjects.Count; ++i)
                {
                    showingObjects[i].SetActive(true);
                }
            }
        }

        public void HideAll()
        {
            if (owningIndicator != null && owningIndicator.activeSelf)
                owningIndicator.SetActive(false);
            if (allyIndicator != null && allyIndicator.activeSelf)
                allyIndicator.SetActive(false);
            if (partyMemberIndicator != null && partyMemberIndicator.activeSelf)
                partyMemberIndicator.SetActive(false);
            if (guildMemberIndicator != null && guildMemberIndicator.activeSelf)
                guildMemberIndicator.SetActive(false);
            if (enemyIndicator != null && enemyIndicator.activeSelf)
                enemyIndicator.SetActive(false);
            if (neutralIndicator != null && neutralIndicator.activeSelf)
                neutralIndicator.SetActive(false);
        }
    }
}
