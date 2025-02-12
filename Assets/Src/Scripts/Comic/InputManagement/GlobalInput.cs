using CustomArchitecture;
using UnityEngine.InputSystem;
using System;

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

        protected override void OnUpdate(float elapsed_time)
        {
            base.OnUpdate(elapsed_time);
        }
    }
}