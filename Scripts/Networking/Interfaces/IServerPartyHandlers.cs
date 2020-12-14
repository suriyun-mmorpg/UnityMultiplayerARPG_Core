namespace MultiplayerARPG
{
    /// <summary>
    /// These properties and functions will be called at server only
    /// </summary>
    public interface IServerPartyHandlers
    {
        /// <summary>
        /// Get party from server's collection
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="partyData"></param>
        /// <returns></returns>
        bool TryGetParty(int partyId, out PartyData partyData);

        /// <summary>
        /// Set party to server's collection
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="partyData"></param>
        void SetParty(int partyId, PartyData partyData);

        /// <summary>
        /// Remove party from server's collection
        /// </summary>
        /// <param name="partyId"></param>
        void RemoveParty(int partyId);

        /// <summary>
        /// Find invitation
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="characterId"></param>
        /// <returns></returns>
        bool HasPartyInvitation(int partyId, string characterId);

        /// <summary>
        /// Append invitation
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="characterId"></param>
        void AppendPartyInvitation(int partyId, string characterId);

        /// <summary>
        /// Remove invitation
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="characterId"></param>
        void RemovePartyInvitation(int partyId, string characterId);

        /// <summary>
        /// Clear server's collection (and other relates variables)
        /// </summary>
        void ClearParty();
    }
}
