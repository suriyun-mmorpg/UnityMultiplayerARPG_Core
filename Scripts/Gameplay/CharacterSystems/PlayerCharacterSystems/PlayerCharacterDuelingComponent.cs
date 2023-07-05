using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterDuelingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        public float DuelingStartTime { get; private set; } = -1f;
        public bool DuelingStarted
        {
            get
            {
                return DuelingStartTime > 0 && Time.unscaledTime - DuelingStartTime > _countDownDuration;
            }
        }
        public bool DuelingTimeOut
        {
            get
            {
                return DuelingStartTime > 0 && Time.unscaledTime - DuelingStartTime > _countDownDuration + _duelDuration;
            }
        }

        private BasePlayerCharacterEntity _duelingCharacter;
        public BasePlayerCharacterEntity DuelingCharacter
        {
            get
            {
                if (!DuelingStarted && Time.unscaledTime - DuelingRequestTime >= CurrentGameInstance.duelingRequestDuration)
                    _duelingCharacter = null;
                return _duelingCharacter;
            }
            set
            {
                _duelingCharacter = value;
                DuelingRequestTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onRequestDueling;
        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity, float, float> onStartDueling;
        /// <summary>
        /// Action: BasePlayerCharacterEntity loserCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onEndDueling;

        public float DuelingRequestTime { get; private set; }

        public bool DisableDueling
        {
            get
            {
                return CurrentGameInstance.disableDueling || BaseGameNetworkManager.CurrentMapInfo.DisableDueling;
            }
        }

        protected float _countDownDuration;
        protected float _duelDuration;

        public void ClearDuelingData()
        {
            DuelingStartTime = -1f;
            CancelInvoke(nameof(UpdateDueling));
        }

        public void StopDueling()
        {
            if (DuelingCharacter == null)
            {
                ClearDuelingData();
                return;
            }
            // Set dueling state/data for co player character entity
            DuelingCharacter.Dueling.ClearDuelingData();
            DuelingCharacter.Dueling.DuelingCharacter = null;
            // Set dueling state/data for player character entity
            ClearDuelingData();
            DuelingCharacter = null;
        }

        public bool CallServerSendDuelingRequest(uint objectId)
        {
            RPC(ServerSendDuelingRequest, objectId);
            return true;
        }

        [ServerRpc]
        protected void ServerSendDuelingRequest(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DisableDueling)
            {
                // Dueling is disabled
                return;
            }
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND);
                return;
            }
            if (targetCharacterEntity.Dueling.DuelingCharacter != null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_DUELING);
                return;
            }
            if (!Entity.IsGameEntityInDistance(targetCharacterEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }
            if (targetCharacterEntity.IsInSafeArea)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_IN_SAFE_AREA);
                return;
            }
            DuelingCharacter = targetCharacterEntity;
            targetCharacterEntity.Dueling.DuelingCharacter = Entity;
            // Send receive dueling request to player
            DuelingCharacter.Dueling.CallOwnerReceiveDuelingRequest(ObjectId);
#endif
        }

        public bool CallOwnerReceiveDuelingRequest(uint objectId)
        {
            RPC(TargetReceiveDuelingRequest, ConnectionId, objectId);
            return true;
        }

        [TargetRpc]
        protected void TargetReceiveDuelingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onRequestDueling != null)
                onRequestDueling.Invoke(playerCharacterEntity);
        }

        public bool CallServerAcceptDuelingRequest()
        {
            RPC(ServerAcceptDuelingRequest);
            return true;
        }

        [ServerRpc]
        protected void ServerAcceptDuelingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DuelingCharacter == null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCEPT_DUELING_REQUEST);
                StopDueling();
                return;
            }
            if (!Entity.IsGameEntityInDistance(DuelingCharacter, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                StopDueling();
                return;
            }
            if (Entity.IsInSafeArea)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_IN_SAFE_AREA);
                StopDueling();
                return;
            }
            float countDownDuration = CurrentGameInstance.duelingCountDownDuration;
            float duelDuration = CurrentGameInstance.duelingDuration;
            // Set dueling state/data for co player character entity
            DuelingCharacter.Dueling.ClearDuelingData();
            DuelingCharacter.Dueling.StartDueling(countDownDuration, duelDuration);
            DuelingCharacter.Dueling.CallOwnerAcceptedDuelingRequest(ObjectId, countDownDuration, duelDuration);
            // Set dueling state/data for player character entity
            ClearDuelingData();
            StartDueling(countDownDuration, duelDuration);
            CallOwnerAcceptedDuelingRequest(DuelingCharacter.ObjectId, countDownDuration, duelDuration);
#endif
        }

        public bool CallServerDeclineDuelingRequest()
        {
            RPC(ServerDeclineDuelingRequest);
            return true;
        }

        [ServerRpc]
        protected void ServerDeclineDuelingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DuelingCharacter != null)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DuelingCharacter.ConnectionId, UITextKeys.UI_ERROR_DUELING_REQUEST_DECLINED);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DUELING_REQUEST_DECLINED);
            StopDueling();
#endif
        }

        public bool CallOwnerAcceptedDuelingRequest(uint objectId, float countDownDuration, float duelDuration)
        {
            RPC(TargetAcceptedDuelingRequest, ConnectionId, objectId, countDownDuration, duelDuration);
            return true;
        }

        [TargetRpc]
        protected void TargetAcceptedDuelingRequest(uint objectId, float countDownDuration, float duelDuration)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (!IsServer)
            {
                // Already setup in accept request function, so don't setup again
                DuelingCharacter = playerCharacterEntity;
                DuelingCharacter.Dueling.DuelingCharacter = Entity;
                DuelingCharacter.Dueling.ClearDuelingData();
                DuelingCharacter.Dueling.StartDueling(countDownDuration, duelDuration);
                ClearDuelingData();
                StartDueling(countDownDuration, duelDuration);
            }
            if (onStartDueling != null)
                onStartDueling.Invoke(playerCharacterEntity, countDownDuration, duelDuration);
        }

        public bool CallOwnerEndDueling(uint loserObjectId)
        {
            if (!DuelingStarted)
                return false;
            RPC(TargetEndDueling, ConnectionId, loserObjectId);
            return true;
        }

        [TargetRpc]
        protected void TargetEndDueling(uint loserObjectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(loserObjectId, out playerCharacterEntity))
                playerCharacterEntity = null;
            StopDueling();
            if (onEndDueling != null)
                onEndDueling.Invoke(playerCharacterEntity);
        }

        protected void StartDueling(float countDownDuration, float duelDuration)
        {
            DuelingStartTime = Time.unscaledTime;
            _countDownDuration = countDownDuration;
            _duelDuration = duelDuration;
            if (IsServer)
            {
                CancelInvoke(nameof(UpdateDueling));
                InvokeRepeating(nameof(UpdateDueling), 0f, 1f);
            }
        }

        protected void UpdateDueling()
        {
            if (DuelingTimeOut)
            {
                EndDueling(null);
                return;
            }

            if (Entity.IsInSafeArea)
            {
                EndDueling(Entity);
                return;
            }
        }

        public void EndDueling(BasePlayerCharacterEntity loser)
        {
            uint loserObjectId = loser != null ? loser.ObjectId : 0;
            if (DuelingCharacter != null)
                DuelingCharacter.Dueling.CallOwnerEndDueling(loserObjectId);
            CallOwnerEndDueling(loserObjectId);
            StopDueling();
        }

        public override void EntityOnDestroy()
        {
            // Player disconnect?
            if (IsServer && DuelingStarted)
                EndDueling(Entity);
        }
    }
}