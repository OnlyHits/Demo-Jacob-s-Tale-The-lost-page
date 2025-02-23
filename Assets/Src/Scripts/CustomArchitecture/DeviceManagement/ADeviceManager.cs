using System;
using UnityEngine.InputSystem;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    public abstract class ADeviceManager : BaseBehaviour
    {
        #region CALLBACKS
        public Action<ControllerType> onDeviceChanged;

        #endregion CALLBACKS

        #region DEVICES / CONTROLLERS
        protected ControllerType m_controllerUsed = ControllerType.NONE;
        public ControllerType GetUsedController() => m_controllerUsed;

        protected Keyboard m_keyboard;
        protected Gamepad m_gamepad;
        public Gamepad GetGamepad() => m_controllerUsed == ControllerType.GAMEPAD ? m_gamepad : null;
        public Keyboard GetKeyboard() => m_controllerUsed == ControllerType.KEYBOARD ? m_keyboard : null;

        #endregion DEVICES / CONTROLLERS

        protected virtual void Awake()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
        }

        public abstract void Init();

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

        protected virtual void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            onDeviceChanged?.Invoke(m_controllerUsed);
        }

        #region DEVICES
        protected void SelectKeyboard()
        {
            m_controllerUsed = ControllerType.KEYBOARD;
            m_keyboard = Keyboard.current;
            //Debug.Log("-> Using Keyboard");
        }

        protected void SelectGamepad()
        {
            m_controllerUsed = ControllerType.GAMEPAD;
            m_gamepad = Gamepad.current;
            //Debug.Log("-> Using Gamepad: " + m_gamepad.name);
        }
        #endregion DEVICES

    }
}
