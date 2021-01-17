using UnityEngine;

namespace MultiplayerARPG
{
    public class EnhanceSocketItemUIActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(GameInstance.ItemUIVisibilityManager.IsEnhanceSocketItemDialogVisible());
            }
        }
    }
}
