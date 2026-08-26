using Cysharp.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UINetworkTime : MonoBehaviour
    {
        public Text textRtt;
        public Text textServerTimestamp;

        private long _lastRtt = -1;
        private long _lastServerTimestamp = -1;
        private bool _appliedNotAvailable;

        private void Update()
        {
            if (BaseGameNetworkManager.Singleton.IsClientConnected ||
                BaseGameNetworkManager.Singleton.IsServer)
            {
                _appliedNotAvailable = false;
                long rtt = BaseGameNetworkManager.Singleton.Rtt;
                if (textRtt && rtt != _lastRtt)
                {
                    _lastRtt = rtt;
                    textRtt.text = ZString.Format("RTT: {0:N0}", rtt);
                }
                long serverTimestamp = BaseGameNetworkManager.Singleton.ServerTimestamp;
                if (textServerTimestamp && serverTimestamp != _lastServerTimestamp)
                {
                    _lastServerTimestamp = serverTimestamp;
                    textServerTimestamp.text = ZString.Format("ServerTimestamp: {0:N0}", serverTimestamp);
                }
                return;
            }
            _lastRtt = -1;
            _lastServerTimestamp = -1;
            if (_appliedNotAvailable)
                return;
            _appliedNotAvailable = true;
            if (textRtt)
                textRtt.text = "RTT: N/A";
            if (textServerTimestamp)
                textServerTimestamp.text = "ServerTimestamp: N/A";
        }
    }
}
