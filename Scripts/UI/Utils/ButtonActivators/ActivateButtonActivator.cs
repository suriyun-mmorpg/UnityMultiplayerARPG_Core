using UnityEngine;

namespace MultiplayerARPG
{
    public class ActivateButtonActivator : MonoBehaviour
    {
        public GameObject[] activateObjects;

        private void LateUpdate()
        {
            bool canActivate = BasePlayerCharacterController.Singleton.ShouldShowActivateButtons();
            foreach (GameObject obj in activateObjects)
            {
                obj.SetActive(canActivate);
            }
        }
    }
}
