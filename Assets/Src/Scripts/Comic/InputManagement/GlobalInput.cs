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
        public Action<InputType, bool> onPause;

        #endregion CALLBACKS

        private void FindAction()
        {
            m_pauseAction = ComicGameCore.Instance.MainGameMode.GetInputAsset().FindAction("Pause");
        }

        #region INIT
        public override void Init()
        {
            FindAction();
            InitInputActions();
        }

        private void InitInputActions()
        {
            InputActionStruct<bool> iPause = new InputActionStruct<bool>(m_pauseAction, onPause, false);

            m_inputActionStructsBool.Add(iPause);
        }

        #endregion INIT
    }
}