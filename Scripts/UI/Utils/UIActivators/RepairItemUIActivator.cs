using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class RepairItemUIActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(BaseUISceneGameplay.Singleton.IsRepairItemDialogVisible());
            }
        }
    }
}
