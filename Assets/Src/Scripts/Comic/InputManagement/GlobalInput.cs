using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class GlobalInput : AInputManager
    {

        #region ACTIONS
        private InputAction m_pauseAction;

        public InputAction GetPauseAction() => m_pauseAction;

        #endregion ACTIONS


        #region CALLBACKS
        // @note: input callbacks
        public Action<InputType, bool> onPause;

        // @note: others callbacks
        public Action<ControllerType> onDeviceChanged;

        #endregion CALLBACKS


        #region CONTROLLERS
        private ControllerType m_controllerUsed = ControllerType.NONE;
        private UnityEngine.InputSystem.Keyboard m_keyboard;
        private UnityEngine.InputSystem.Gamepad m_gamepad;

        public ControllerType GetUsedController() => m_controllerUsed;

        #endregion CONTROLLERS


        #region SUB CALLBACKS
        public void SubscribeToDeviceChanged(Action<ControllerType> function)
        {
            onDeviceChanged -= function;
            onDeviceChanged += function;
        }

        #endregion SUB CALLBACKS

        private void Awake()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        public override void Init()
        {
            FindAction();
            InitInputActions();
        }

        private void FindAction()
        {
            m_pauseAction = ComicGameCore.Instance.MainGameMode.GetInputAsset().FindAction("Pause");
        }

        private void InitInputActions()
        {
            InputActionStruct<bool> iPause = new InputActionStruct<bool>(m_pauseAction, onPause, false);

            m_inputActionStructsBool.Add(iPause);
        }

        // ADD A VIBRATION FONCTION FOR GAMEPAD CONTROLLER !! (called in cancelSwitchPage)

        protected override void OnUpdate(float elapsed_time)
        {
            base.OnUpdate(elapsed_time);

            if (Keyboard.current.wasUpdatedThisFrame && m_controllerUsed != ControllerType.KEYBOARD)
            {
                SelectKeyboard();
            }
            else if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame && m_controllerUsed != ControllerType.GAMEPAD)
            {
                SelectGamepad();
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    Debug.Log($"Device connected: {device.name}");
                    if (device is Gamepad)
                    {
                        SelectGamepad();
                    }
                    break;
                case InputDeviceChange.Removed:
                    Debug.Log($"Device disconnected: {device.name}");
                    if (device is Gamepad)
                    {
                        SelectKeyboard();
                    }
                    break;
            }

            onDeviceChanged?.Invoke(m_controllerUsed);
        }

        private void SelectKeyboard()
        {
            m_controllerUsed = ControllerType.KEYBOARD;
            m_keyboard = Keyboard.current;
            //Debug.Log("Using Keyboard");
        }

        private void SelectGamepad()
        {
            m_controllerUsed = ControllerType.GAMEPAD;
            m_gamepad = Gamepad.current;
            //Debug.Log("Using Gamepad: " + m_gamepad.name);
        }
    }
}