using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class UIVehicleHornPressHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public UIVehicleHorn ui;
        private int _pointerId = int.MinValue;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _pointerId != int.MinValue || ui == null ||
                ui.buttonHorn == null || !ui.buttonHorn.IsInteractable()) return;
            _pointerId = eventData.pointerId;
            ui.SetPressed(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && eventData.pointerId == _pointerId) Release();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerId == _pointerId) Release();
        }

        private void OnDisable() => Release();
        private void Release()
        {
            _pointerId = int.MinValue;
            if (ui != null) ui.SetPressed(false);
        }
    }
}
