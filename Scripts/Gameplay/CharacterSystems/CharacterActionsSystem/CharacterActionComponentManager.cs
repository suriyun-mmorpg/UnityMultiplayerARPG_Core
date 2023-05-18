using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterActionComponentManager : MonoBehaviour
    {
        public float actionAcceptanceDuration = 0.05f;
        protected float _lastAcceptTime;

        public bool IsAcceptNewAction()
        {
            return Time.unscaledTime - _lastAcceptTime > actionAcceptanceDuration;
        }

        public void ActionAccepted()
        {
            _lastAcceptTime = Time.unscaledTime;
        }
    }
}
