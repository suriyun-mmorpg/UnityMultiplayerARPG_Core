using UnityEngine;

namespace MultiplayerARPG
{
    public class ShopUIActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(GameInstance.ItemUIVisibilityManager.IsShopDialogVisible());
            }
        }
    }
}
