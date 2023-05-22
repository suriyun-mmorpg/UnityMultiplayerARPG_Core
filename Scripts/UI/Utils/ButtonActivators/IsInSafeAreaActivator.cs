using UnityEngine;

namespace MultiplayerARPG {
    public class IsInSafeAreaActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private DamageableEntity _damageableEntity;

        private void Start()
        {
            _damageableEntity = GetComponentInParent<DamageableEntity>();
            if (_damageableEntity == null)
            {
                GameInstance.onSetPlayingCharacter += GameInstance_onSetPlayingCharacter;
                if (GameInstance.PlayingCharacterEntity != null)
                    GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacterEntity);
            }
        }

        private void OnDestroy()
        {
            GameInstance.onSetPlayingCharacter -= GameInstance_onSetPlayingCharacter;
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playingCharacterData)
        {
            _damageableEntity = playingCharacterData as BasePlayerCharacterEntity;
        }

        private void LateUpdate()
        {
            bool isInSafeArea = _damageableEntity.IsInSafeArea;
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(isInSafeArea);
            }
        }
    }
}