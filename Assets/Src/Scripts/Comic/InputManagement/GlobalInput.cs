using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    public class GlobalInput : AInputManager
    {

        #region ACTIONS
        private InputAction m_pauseAction;
        public InputAction GetPauseAction() => m_pauseAction;

        #endregion ACTIONS


        #region CALLBACKS
        public Action<InputType, bool> onPause;

        #endregion CALLBACKS

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            InitInputActions();
        }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length != 1
                || parameters[0] is not InputActionAsset)
            {
                Debug.LogWarning("GlobalInput : Unable to find InputActionAsset");
                return;
            }

            FindAction((InputActionAsset)parameters[0]);
        }
        #endregion

        private void FindAction(InputActionAsset inputActionAsset)
        {
            m_pauseAction = inputActionAsset.FindAction("Pause");
        }

        private void InitInputActions()
        {
            InputActionStruct<bool> iPause = new(m_pauseAction, onPause, false);

            // in case of reloading the game
            m_inputActionStructsBool.Clear();

            m_inputActionStructsBool.Add(iPause);
        }
    }
}