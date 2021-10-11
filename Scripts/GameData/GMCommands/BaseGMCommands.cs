using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGMCommands : ScriptableObject
    {
        /// <summary>
        /// Return `TRUE` if message contains GM command
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public abstract bool IsGMCommand(string chatMessage, out string command);
        /// <summary>
        /// Return `TRUE` if character can use command
        /// </summary>
        /// <param name="characterEntity"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public abstract bool CanUseGMCommand(BasePlayerCharacterEntity characterEntity, string command);
        /// <summary>
        /// Return response message, it's a message which send to command user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="chatMessage"></param>
        /// <returns></returns>
        public abstract string HandleGMCommand(BasePlayerCharacterEntity characterEntity, string chatMessage);
    }
}
