using UnityEngine;

namespace MultiplayerARPG
{
    public class DismantleItemUIActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(GameInstance.ItemUIVisibilityManager.IsDismantleItemDialogVisible());
            }
        }
    }
}
