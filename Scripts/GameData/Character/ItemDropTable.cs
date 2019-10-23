using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Item Drop Table", menuName = "Create GameData/Item Drop Table", order = -4993)]
    public class ItemDropTable : ScriptableObject
    {
        [ArrayElementTitle("item", new float[] { 1, 0, 0 }, new float[] { 0, 0, 1 })]
        public ItemDrop[] randomItems;
    }
}
