using CustomArchitecture;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;
using static CustomArchitecture.CustomArchitecture;

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

        protected void Awake()
        {
            m_inputAction = ComicGameCore.Instance.MainGameMode.GetInputAsset().FindAction(m_keyParam);
            if (m_inputAction == null)
            {
                m_tAction.enabled = false;
                m_tKey.enabled = false;
                this.enabled = false;
                //Debug.LogError($"Key binding {m_keyParam} does not exist");
                return;
            }

            //InitCurrentKey();
        }

        private void OnEnable()
        {
            InitCurrentKey();
        }

        private void InitCurrentKey()
        {
            ControllerType usedController = ComicGameCore.Instance.MainGameMode.GetGlobalInput().GetUsedController();

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


        protected override void OnUpdate(float delta)
        {
            if (!m_selectetd) return;

            UpdateWaitingKeyText(true, delta);

            ControllerType usedController = ComicGameCore.Instance.MainGameMode.GetGlobalInput().GetUsedController();

            if (usedController == ControllerType.KEYBOARD)
            {
                if (RebindKeyUtils.TryGetKeyboardInputPressed(out KeyControl keyControl))
                {
                    StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                    {
                        // @note : Discard pause input to rebind to a control key
                        if (keyControl == ComicGameCore.Instance.MainGameMode.GetGlobalInput().GetPauseAction().GetKeyboardKeysFromAction().FirstOrDefault())
                            return;
                        m_currentInputControl = keyControl;
                        m_inputAction.RebindKey(keyControl);
                        SetSelected(false);
                        SetKeyTextByAction(m_inputAction, keyControl);
                        //Debug.Log($"Action rebinded [{m_inputAction.name}] with [{keyControl.name}] or [{m_inputAction.GetBindingDisplayString()}]");
                    }));
                }
            }

            if (usedController == ControllerType.GAMEPAD)
            {
                if (RebindKeyUtils.TryGetGamepadInputPressed(out ButtonControl buttonControl))
                {
                    StartCoroutine(CoroutineUtils.InvokeNextFrame(() =>
                    {
                        // @note : Discard pause input to rebind to a control key
                        if (buttonControl == ComicGameCore.Instance.MainGameMode.GetGlobalInput().GetPauseAction().GetGamepadKeysFromAction().FirstOrDefault())
                            return;
                        m_currentInputControl = buttonControl;
                        m_inputAction.RebindKey(buttonControl);
                        SetSelected(false);
                        SetKeyTextByAction(m_inputAction, buttonControl);
                        //Debug.Log($"Action rebinded [{m_inputAction.name}] with [{buttonControl.name}] or [{m_inputAction.GetBindingDisplayString()}]");
                    }));
                }
            }
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

