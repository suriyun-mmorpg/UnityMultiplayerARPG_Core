using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;
using Newtonsoft.Json;

namespace MultiplayerARPG
{
    public class UILanConnection : UIBase
    {
        [Header("Lan connection UI elements")]
        public InputField inputNetworkAddress;

        [Header("Discovery connection UI elements")]
        public GameObject listEmptyObject;
        public UIDiscoveryEntry discoveryEntryPrefab;
        public Transform discoveryEntryContainer;

        public string DefaultNetworkAddress
        {
            get { return GameInstance.Singleton.NetworkSetting.networkAddress; }
        }

        public int DefaultNetworkPort
        {
            get { return GameInstance.Singleton.NetworkSetting.networkPort; }
        }

        public string NetworkAddress
        {
            get { return inputNetworkAddress == null ? DefaultNetworkAddress : inputNetworkAddress.text; }
        }

        private Dictionary<string, DiscoveryData> discoveries = new Dictionary<string, DiscoveryData>();
        private Dictionary<string, IPEndPoint> remoteEndPoints = new Dictionary<string, IPEndPoint>();

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = discoveryEntryPrefab.gameObject;
                    cacheList.uiContainer = discoveryEntryContainer;
                }
                return cacheList;
            }
        }

        private UIDiscoveryEntrySelectionManager cacheSelectionManager;
        public UIDiscoveryEntrySelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.AddComponent<UIDiscoveryEntrySelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.Toggle;
                return cacheSelectionManager;
            }
        }

        private LiteNetLibDiscovery cacheDiscovery;
        private LiteNetLibDiscovery CacheDiscovery
        {
            get
            {
                if (cacheDiscovery == null)
                {
                    LanRpgNetworkManager networkManager = BaseGameNetworkManager.Singleton as LanRpgNetworkManager;
                    if (networkManager == null || networkManager.CacheDiscovery == null)
                    {
                        Debug.LogWarning("[UIDiscoveryConnection] networkManager or its discovery is empty");
                        return null;
                    }
                    cacheDiscovery = networkManager.CacheDiscovery;
                }
                return cacheDiscovery;
            }
        }

        private void OnEnable()
        {
            if (inputNetworkAddress != null)
                inputNetworkAddress.text = DefaultNetworkAddress;
            if (CacheDiscovery != null)
            {
                CacheDiscovery.onReceivedBroadcast += OnReceivedBroadcast;
                CacheDiscovery.StartClient();
            }
        }

        private void OnDisable()
        {
            if (CacheDiscovery != null)
            {
                CacheDiscovery.onReceivedBroadcast -= OnReceivedBroadcast;
                CacheDiscovery.StopClient();
            }
        }

        private void OnReceivedBroadcast(IPEndPoint remoteEndPoint, string data)
        {
            DiscoveryData characterData = JsonConvert.DeserializeObject<DiscoveryData>(data);
            discoveries[characterData.id] = characterData;
            remoteEndPoints[characterData.id] = remoteEndPoint;
            UpdateList();
        }

        private void UpdateList()
        {
            CacheSelectionManager.Clear();
            CacheList.Generate(discoveries.Values, (index, data, ui) =>
            {
                UIDiscoveryEntry entry = ui.GetComponent<UIDiscoveryEntry>();
                entry.Data = data;
                CacheSelectionManager.Add(entry);
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(discoveries.Count == 0);
        }

        public IPEndPoint GetSelectedRemoteEndPoint()
        {
            UIDiscoveryEntry selectedUI = CacheSelectionManager.SelectedUI;
            return remoteEndPoints[selectedUI.Data.id];
        }
    }
}
