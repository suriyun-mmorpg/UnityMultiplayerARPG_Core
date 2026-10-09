using System;
using Newtonsoft.Json.Linq;

namespace MultiplayerARPG
{
    [Serializable]
    public sealed class DataPatchSettings
    {
        public bool enabled;
        public string serviceUrl = "";
        public string environment = "staging";
        public string startupReleaseId = "";
        [UnityEngine.Tooltip("Enable for a server hosted from a regular player build or Editor. Dedicated server builds always load at startup.")]
        public bool loadAtStartup;
        public int requestTimeoutSeconds = 30;
    }

    [Serializable]
    public sealed class DataPatchEntry
    {
        public string dataType;
        public int dataId;
        public JObject data;
    }

    [Serializable]
    public sealed class DataPatchRelease
    {
        public string id;
        public string databaseId;
        public string environment;
        public int version;
        public int schemaVersion;
        public string payloadHash;
        public string payloadJson;
        public string publishTime;
    }
}
