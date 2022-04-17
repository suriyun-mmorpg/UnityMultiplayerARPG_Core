using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.NEW_CHARACTER_SETTING_FILE, menuName = GameDataMenuConsts.NEW_CHARACTER_SETTING_MENU, order = GameDataMenuConsts.NEW_CHARACTER_SETTING_ORDER)]
    public partial class NewCharacterSetting : ScriptableObject
    {
        [Header("New Character Configs")]
        [Tooltip("Amount of gold that will be added to character when create new character")]
        public int startGold = 0;
        [Tooltip("Items that will be added to character when create new character")]
        [ArrayElementTitle("item")]
        public ItemAmount[] startItems;
    }
}
