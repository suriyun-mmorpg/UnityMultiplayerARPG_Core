using Insthync.CameraAndInput;
using UnityEngine;

namespace MultiplayerARPG
{
    public class InputStateManager
    {
        public enum InputType
        {
            Button,
            Mouse,
            Key,
        }

        private InputType _inputType;
        private string _buttonName;
        private int _mouseButton;
        private KeyCode _keyCode;
        private float _holdDuration;
        private bool _isHolding;
        private bool _isHolded;
        private float _holdTime;
        private float _lastDeltaTime;

        public bool IsPress { get; private set; }
        public bool IsRelease { get; private set; }
        public bool IsPressed { get; private set; }
        public bool IsHold { get; private set; }

        public InputStateManager(string buttonName, float holdDuration)
        {
            _inputType = InputType.Button;
            _buttonName = buttonName;
            _holdDuration = holdDuration;
        }

        public InputStateManager(string buttonName) : this(buttonName, 1f) { }

        public InputStateManager(int mouseButton, float holdDuration)
        {
            _inputType = InputType.Mouse;
            _mouseButton = mouseButton;
            _holdDuration = holdDuration;
        }

        public InputStateManager(int mouseIndex) : this(mouseIndex, 1f) { }

        public InputStateManager(KeyCode keyCode, float holdDuration)
        {
            _inputType = InputType.Key;
            _keyCode = keyCode;
            _holdDuration = holdDuration;
        }

        public InputStateManager(KeyCode keyCode) : this(keyCode, 1f) { }

        public void Reset()
        {
            _isHolding = false;
            _isHolded = false;
            _holdTime = 0f;
            _lastDeltaTime = 0f;
            IsPress = false;
            IsRelease = false;
            IsPressed = false;
            IsHold = false;
        }

        public void OnUpdate(float deltaTime)
        {
            _lastDeltaTime = deltaTime;
            IsPress = false;
            IsRelease = false;
            IsPressed = false;
            switch (_inputType)
            {
                case InputType.Button:
                    OnUpdate_Button(deltaTime);
                    break;
                case InputType.Mouse:
                    OnUpdate_Mouse(deltaTime);
                    break;
                case InputType.Key:
                    OnUpdate_Key(deltaTime);
                    break;
            }
            _isHolding = IsPress || IsPressed;
            if (_holdTime >= _holdDuration)
            {
                // Holded, so clear input states
                IsPress = false;
                IsRelease = false;
                IsPressed = false;
                // Set is hold to true just one time, in future frames it will be false
                IsHold = !_isHolded;
                if (IsHold && !_isHolded)
                    _isHolded = true;
            }
        }

        private void OnUpdate_Button(float deltaTime)
        {
            if (InputManager.GetButtonDown(_buttonName))
                IsPress = true;
            else if (InputManager.GetButtonUp(_buttonName))
                IsRelease = true;
            else if (InputManager.GetButton(_buttonName))
                IsPressed = true;
        }

        private void OnUpdate_Mouse(float deltaTime)
        {
            if (InputManager.GetMouseButtonDown(_mouseButton))
                IsPress = true;
            else if (InputManager.GetMouseButtonUp(_mouseButton))
                IsRelease = true;
            else if (InputManager.GetMouseButton(_mouseButton))
                IsPressed = true;
        }

        private void OnUpdate_Key(float deltaTime)
        {
            if (InputManager.GetKeyDown(_keyCode))
                IsPress = true;
            else if (InputManager.GetKeyUp(_keyCode))
                IsRelease = true;
            else if (InputManager.GetKey(_keyCode))
                IsPressed = true;
        }

        public void OnLateUpdate()
        {
            if (_isHolding)
            {
                // Update hode time
                _holdTime += _lastDeltaTime;
            }
            else
            {
                // Reset hold state
                _holdTime = 0f;
                _isHolded = false;
            }
        }
    }
}
