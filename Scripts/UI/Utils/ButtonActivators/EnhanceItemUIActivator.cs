using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class EnhanceItemUIActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(BaseUISceneGameplay.Singleton.IsEnhanceSocketItemDialogVisible());
            }
        }
    }
}
