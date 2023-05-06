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
        public float updateRepeatDelay = 5f;

        private BaseCharacterEntity _characterEntity;
        private BasePlayerCharacterEntity _previousEntity;

        private void Start()
        {
            _characterEntity = GetComponentInParent<BaseCharacterEntity>();
            GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
            if (GameInstance.PlayingCharacterEntity != null)
                GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
        }

        private void OnDestroy()
        {
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
            RemoveEvents(_previousEntity);
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            RemoveEvents(_previousEntity);
            BasePlayerCharacterEntity playerCharacterEntity = playingCharacterData as BasePlayerCharacterEntity;
            _previousEntity = playerCharacterEntity;
            AddEvents(_previousEntity);
            SetupUpdating();
        }

        private void AddEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onPartyIdChange += PlayingCharacterEntity_onPartyIdChange;
            PlayingCharacterEntity.onGuildIdChange += PlayingCharacterEntity_onGuildIdChange;
            PlayingCharacterEntity.onFactionIdChange += PlayingCharacterEntity_onFactionIdChange;
        }

        private void RemoveEvents(BasePlayerCharacterEntity PlayingCharacterEntity)
        {
            if (PlayingCharacterEntity == null)
                return;
            PlayingCharacterEntity.onPartyIdChange -= PlayingCharacterEntity_onPartyIdChange;
            PlayingCharacterEntity.onGuildIdChange -= PlayingCharacterEntity_onGuildIdChange;
            PlayingCharacterEntity.onFactionIdChange -= PlayingCharacterEntity_onFactionIdChange;
        }

        private void PlayingCharacterEntity_onPartyIdChange(int obj)
        {
            SetupUpdating();
        }

        private void PlayingCharacterEntity_onGuildIdChange(int obj)
        {
            SetupUpdating();
        }

        private void PlayingCharacterEntity_onFactionIdChange(int obj)
        {
            SetupUpdating();
        }

        private void SetupUpdating()
        {
            CancelInvoke(nameof(UpdateIndicator));
            InvokeRepeating(nameof(UpdateIndicator), 0, updateRepeatDelay);
        }

        private void UpdateIndicator()
        {
            if (_characterEntity == null)
            {
                DestroyImmediate(this);
                return;
            }

            HideAll();
            if (!_characterEntity.IsClient || (_characterEntity.IsServer && _characterEntity.Identity.CountSubscribers() == 0) || GameInstance.PlayingCharacterEntity == null || Vector3.Distance(_characterEntity.EntityTransform.position, GameInstance.PlayingCharacterEntity.EntityTransform.position) > updateWithinRange)
                return;

            EntityInfo entityInfo = _characterEntity.GetInfo();
            List<GameObject> showingObjects = new List<GameObject>();
            bool isShowing;

            isShowing = GameInstance.PlayingCharacterEntity == _characterEntity;
            if (owningIndicator != null && isShowing && !showingObjects.Contains(owningIndicator))
                showingObjects.Add(owningIndicator);

            isShowing = _characterEntity.IsAlly(GameInstance.PlayingCharacterEntity.GetInfo());
            if (allyIndicator != null && isShowing && !showingObjects.Contains(allyIndicator))
                showingObjects.Add(allyIndicator);

            isShowing = entityInfo.PartyId > 0 && entityInfo.PartyId == GameInstance.PlayingCharacter.PartyId;
            if (partyMemberIndicator != null && isShowing && !showingObjects.Contains(partyMemberIndicator))
                showingObjects.Add(partyMemberIndicator);

            isShowing = entityInfo.GuildId > 0 && entityInfo.GuildId == GameInstance.PlayingCharacter.GuildId;
            if (guildMemberIndicator != null && isShowing && !showingObjects.Contains(guildMemberIndicator))
                showingObjects.Add(guildMemberIndicator);

            isShowing = _characterEntity.IsEnemy(GameInstance.PlayingCharacterEntity.GetInfo());
            if (enemyIndicator != null && isShowing && !showingObjects.Contains(enemyIndicator))
                showingObjects.Add(enemyIndicator);

            isShowing = _characterEntity.IsNeutral(GameInstance.PlayingCharacterEntity.GetInfo());
            if (neutralIndicator != null && isShowing && !showingObjects.Contains(neutralIndicator))
                showingObjects.Add(neutralIndicator);

            for (int i = 0; i < showingObjects.Count; ++i)
            {
                showingObjects[i].SetActive(true);
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