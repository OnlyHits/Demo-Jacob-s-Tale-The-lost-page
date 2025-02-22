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

        #endregion ACTIONS

        #region CALLBACKS
        public Action<InputType, bool> onPause;

        #endregion CALLBACKS

        #region CONTROLLERS
        private ControllerType m_controllerUsed = ControllerType.NONE;
        private UnityEngine.InputSystem.Keyboard m_keyboard;
        private UnityEngine.InputSystem.Gamepad m_gamepad;

        public ControllerType GetUsedController() => m_controllerUsed;

        #endregion CONTROLLERS

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
                m_controllerUsed = ControllerType.KEYBOARD;
                m_keyboard = Keyboard.current;
                Debug.Log("------> Using Keyboard");
            }
            else if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame && m_controllerUsed != ControllerType.GAMEPAD)
            {
                m_controllerUsed = ControllerType.GAMEPAD;
                m_gamepad = Gamepad.current;
                Debug.Log("------> Using Gamepad: " + m_gamepad.name);
            }
        }
    }
}