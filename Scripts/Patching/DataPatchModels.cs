using System;
using UnityEngine;
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
        [Min(0.1f)]
        [Tooltip("Main-thread preparation budget per frame. One record can exceed this budget; activation remains atomic.")]
        public float preparationFrameBudgetMilliseconds = 2f;
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
        public string manifestJson;
        [NonSerialized, Newtonsoft.Json.JsonIgnore]
        public string[] chunkPaths;
        public string publishTime;
    }

    [Serializable]
    public sealed class DataPatchManifest
    {
        public int schemaVersion = 2;
        public DataPatchChunk[] chunks;
    }

    [Serializable]
    public sealed class DataPatchChunk
    {
        public int index;
        public string dataType;
        public string hash;
        public int bytes;
        public int entryCount;
    }

}
