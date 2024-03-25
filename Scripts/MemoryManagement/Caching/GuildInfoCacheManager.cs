using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GuildInfoCacheManager
    {
        private static readonly Dictionary<int, GuildListEntry> s_caches = new Dictionary<int, GuildListEntry>();
        private static readonly Dictionary<int, float> s_cachedTimes = new Dictionary<int, float>();
        private static readonly HashSet<int> s_loadingIds = new HashSet<int>();
        public static System.Action<GuildListEntry> onSetGuildInfo;

        public static void LoadOrGetGuildInfoFromCache(int guildId, System.Action<GuildListEntry> callback)
        {
            if (s_loadingIds.Contains(guildId))
            {
                // Guild info is loading
                return;
            }
            if (s_cachedTimes.ContainsKey(guildId) && Time.unscaledTime - s_cachedTimes[guildId] < 5f)
            {
                // Can reload after 5 seconds
                callback.Invoke(s_caches[guildId]);
                return;
            }
            s_loadingIds.Add(guildId);
            GameInstance.ClientGuildHandlers.RequestGetGuildInfo(new RequestGetGuildInfoMessage()
            {
                guildId = guildId,
            }, (requestHandler, responseCode, response) =>
            {
                s_loadingIds.Remove(response.guild.Id);
                if (responseCode != AckResponseCode.Success)
                    return;
                SetCache(response.guild);
                callback.Invoke(response.guild);
            });
        }

        public static void SetCache(GuildListEntry guild)
        {
            s_caches[guild.Id] = guild;
            s_cachedTimes[guild.Id] = Time.unscaledTime;
            if (onSetGuildInfo != null)
                onSetGuildInfo.Invoke(guild);
        }

        public static void ClearCache(int guildId)
        {
            s_caches.Remove(guildId);
            s_cachedTimes.Remove(guildId);
        }
    }
}
