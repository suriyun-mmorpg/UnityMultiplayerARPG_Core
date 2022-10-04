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
            if (characterEntity == null || !characterEntity.IsClient ||
                GameInstance.PlayingCharacterEntity == null ||
                (characterEntity.IsServer && characterEntity.Identity.CountSubscribers() == 0) ||
                Vector3.Distance(characterEntity.EntityTransform.position, GameInstance.PlayingCharacterEntity.EntityTransform.position) > updateWithinRange)
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
                return;
            }

            if (Time.unscaledTime - lastUpdateTime >= updateRepeatRate)
            {
                lastUpdateTime = Time.unscaledTime;

                bool isShowing;

                isShowing = GameInstance.PlayingCharacterEntity == characterEntity;
                if (owningIndicator != null && owningIndicator.activeSelf != isShowing)
                    owningIndicator.SetActive(isShowing);

                isShowing = characterEntity.IsAlly(GameInstance.PlayingCharacterEntity.GetInfo());
                if (allyIndicator != null && allyIndicator.activeSelf != isShowing)
                    allyIndicator.SetActive(isShowing);

                BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                if (playerCharacterEntity != null)
                {
                    isShowing = playerCharacterEntity.PartyId > 0 && playerCharacterEntity.PartyId == GameInstance.PlayingCharacter.PartyId;
                    if (partyMemberIndicator != null && partyMemberIndicator.activeSelf != isShowing)
                        partyMemberIndicator.SetActive(isShowing);
                    isShowing = playerCharacterEntity.GuildId > 0 && playerCharacterEntity.GuildId == GameInstance.PlayingCharacter.GuildId;
                    if (guildMemberIndicator != null && guildMemberIndicator.activeSelf != isShowing)
                        guildMemberIndicator.SetActive(isShowing);
                }
                else
                {
                    isShowing = false;
                    if (partyMemberIndicator != null && partyMemberIndicator.activeSelf != isShowing)
                        partyMemberIndicator.SetActive(isShowing);
                    if (guildMemberIndicator != null && guildMemberIndicator.activeSelf != isShowing)
                        guildMemberIndicator.SetActive(isShowing);
                }

                isShowing = characterEntity.IsEnemy(GameInstance.PlayingCharacterEntity.GetInfo());
                if (enemyIndicator != null && enemyIndicator.activeSelf != isShowing)
                    enemyIndicator.SetActive(isShowing);

                isShowing = characterEntity.IsNeutral(GameInstance.PlayingCharacterEntity.GetInfo());
                if (neutralIndicator != null && neutralIndicator.activeSelf != isShowing)
                    neutralIndicator.SetActive(isShowing);
            }
        }
    }
}
