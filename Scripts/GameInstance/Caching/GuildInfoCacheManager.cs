using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GuildInfoCacheManager
    {
        private static readonly Dictionary<int, GuildListEntry> caches = new Dictionary<int, GuildListEntry>();
        private static readonly Dictionary<int, float> cachedTimes = new Dictionary<int, float>();
        private static readonly HashSet<int> loadingIds = new HashSet<int>();
        public static System.Action<GuildListEntry> onSetGuildInfo;

        public static void LoadOrGetGuildInfoFromCache(int guildId, System.Action<GuildListEntry> callback)
        {
            if (loadingIds.Contains(guildId))
            {
                // Guild info is loading
                return;
            }
            if (cachedTimes.ContainsKey(guildId) && Time.unscaledTime - cachedTimes[guildId] < 5f)
            {
                // Can reload after 5 seconds
                callback.Invoke(caches[guildId]);
                return;
            }
            loadingIds.Add(guildId);
            GameInstance.ClientGuildHandlers.RequestGetGuildInfo(new RequestGetGuildInfoMessage()
            {
                guildId = guildId,
            }, (requestHandler, responseCode, response) =>
            {
                loadingIds.Remove(response.guild.Id);
                if (responseCode != AckResponseCode.Success)
                    return;
                SetCache(response.guild);
                callback.Invoke(response.guild);
            });
        }

        public static void SetCache(GuildListEntry guild)
        {
            caches[guild.Id] = guild;
            cachedTimes[guild.Id] = Time.unscaledTime;
            if (onSetGuildInfo != null)
                onSetGuildInfo.Invoke(guild);
        }

        public static void ClearCache(int guildId)
        {
            caches.Remove(guildId);
            cachedTimes.Remove(guildId);
        }
    }
}
