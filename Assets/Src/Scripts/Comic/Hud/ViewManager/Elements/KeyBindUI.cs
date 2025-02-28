using CustomArchitecture;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;
using static CustomArchitecture.CustomArchitecture;
using static PageHole;

namespace Comic
{
    public class KeyBindUI : BaseBehaviour
    {
        [SerializeField] private string m_keyParam = "";
        [SerializeField] private TextMeshProUGUI m_tAction;
        [SerializeField] private TextMeshProUGUI m_tKey;
        [SerializeField] private float m_durationKeyChange = 0f;
        public InputAction m_inputAction;
        public InputControl m_currentInputControl;
        private bool m_selectetd = false;
        private float m_elapsedTime = 0f;

        public bool IsBindingKey => m_selectetd;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (!m_selectetd) return;

            UpdateWaitingKeyText(true, Time.deltaTime);

            ControllerType usedController = ComicGameCore.Instance.GetDeviceManager().GetUsedController();

            if (usedController == ControllerType.KEYBOARD)
                TryRebindKeyboard();
            if (usedController == ControllerType.GAMEPAD)
                TryRebindGamepad();
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
        }
        #endregion

        protected void Awake() // should be done in init
        {
            m_inputAction = ComicGameCore.Instance.GetInputAsset().FindAction(m_keyParam);
            if (m_inputAction == null)
            {
                m_tAction.enabled = false;
                m_tKey.enabled = false;
                this.enabled = false;
                //Debug.LogError($"Key binding {m_keyParam} does not exist");
                return;
            }

            ComicGameCore.Instance.GetDeviceManager().SubscribeToDeviceChanged(OnDeviceChanged);
        }

        private void OnEnable()
        {
            UpdareCurrentKey();
        }

        private void OnDeviceChanged(ControllerType newControllerType)
        {
            UpdareCurrentKey();
        }

        #region REBIND KEYS
        private void TryRebindKeyboard()
        {
            if (RebindKeyUtils.TryGetKeyboardInputPressed(out KeyControl keyControl))
            {
                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    // @note : Discard pause input to rebind to a control key
                    if (keyControl == ComicGameCore.Instance.GetGlobalInput().GetPauseAction().GetKeyboardKeysFromAction().FirstOrDefault())
                        return;
                    m_currentInputControl = keyControl;
                    m_inputAction.RebindKey(keyControl);
                    SetSelected(false);
                    SetKeyTextByAction(m_inputAction, keyControl);
                    //Debug.Log($"Action rebinded [{m_inputAction.name}] with [{keyControl.name}] or [{m_inputAction.GetBindingDisplayString()}]");
                }));
            }
        }

        private void TryRebindGamepad()
        {
            if (RebindKeyUtils.TryGetGamepadInputPressed(out ButtonControl buttonControl))
            {
                StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                {
                    // @note : Discard pause input to rebind to a control key
                    if (buttonControl == ComicGameCore.Instance.GetGlobalInput().GetPauseAction().GetGamepadKeysFromAction().FirstOrDefault())
                        return;
                    m_currentInputControl = buttonControl;
                    m_inputAction.RebindKey(buttonControl);
                    SetSelected(false);
                    SetKeyTextByAction(m_inputAction, buttonControl);
                    //Debug.Log($"Action rebinded [{m_inputAction.name}] with [{buttonControl.name}] or [{m_inputAction.GetBindingDisplayString()}]");
                }));
            }
        }
        #endregion REBIND KEYS


        #region UPDATE UI
        private void UpdareCurrentKey()
        {
            ControllerType usedController = ComicGameCore.Instance.GetDeviceManager().GetUsedController();

            switch (usedController)
            {
                case ControllerType.KEYBOARD:
                    m_currentInputControl = m_inputAction.GetKeyboardKeysFromAction().FirstOrDefault();
                    break;
                case ControllerType.GAMEPAD:
                    m_currentInputControl = m_inputAction.GetGamepadKeysFromAction().FirstOrDefault();
                    break;
            }

            SetKeyTextByAction(m_inputAction, m_currentInputControl);
            //Debug.Log($"> {m_inputAction.name} : [{m_inputAction.GetActionNameByInputControl(m_currentInputControl)}]");
        }

        private void UpdateWaitingKeyText(bool isWaitingForKey, float delta = 0f)
        {
            if (isWaitingForKey)
            {
                m_elapsedTime += delta;
                if (m_elapsedTime > m_durationKeyChange)
                {
                    m_elapsedTime = 0f;
                    m_tKey.text = m_tKey.text != "_" ? "_" : " ";
                }
            }
        }

        private void SetKeyTextByAction(InputAction inputAction, InputControl inputControl)
        {
            m_tKey.text = inputAction.GetActionNameByInputControl(inputControl);
        }

        #endregion UPDATE UI


        public void SetSelected(bool selected)
        {
            m_selectetd = selected;
        }

        public void ResetKey()
        {
            SetKeyTextByAction(m_inputAction, m_currentInputControl);
        }

        // Security, but not needed for now
        //private void OnDisable()
        //{
        //    SetKeyTextByAction(m_inputAction, m_currentKeyControl);
        //}
    }
}

