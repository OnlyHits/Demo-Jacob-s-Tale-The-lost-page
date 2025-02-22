using CustomArchitecture;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;

namespace Comic
{
    public class KeyBindUI : BaseBehaviour
    {
        [SerializeField] private string m_keyParam = "";
        [SerializeField] private TextMeshProUGUI m_tAction;
        [SerializeField] private TextMeshProUGUI m_tKey;
        [SerializeField] private float m_durationKeyChange = 0f;
        public InputAction m_inputAction;
        public KeyControl m_currentKeyControl;
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

            m_currentKeyControl = m_inputAction.GetKeyBoardKeysFromAction().FirstOrDefault();
            m_tKey.text = m_inputAction.GetActionName(m_currentKeyControl);
            //Debug.Log($"> {m_inputAction.name} : [{m_inputAction.GetActionName(keyControl)}]");
        }

        protected override void OnUpdate(float delta)
        {
            if (!m_selectetd) return;

            UpdateWaitingKeyText(true, delta);

            if (RebindKeyUtils.TryGetKeyPressed(out KeyControl keyControl))
            {
                m_currentKeyControl = keyControl;
                m_inputAction.RebindKey(keyControl);
                SetSelected(false);
                SetKeyTextByAction(m_inputAction, keyControl);
                //Debug.Log($"Action rebinded [{m_inputAction.name}] with [{keyControl.name}] or [{m_inputAction.GetBindingDisplayString()}]");
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

        private void SetKeyTextByAction(InputAction inputAction, KeyControl keyControl)
        {
            m_tKey.text = inputAction.GetActionName(keyControl);
        }

        public void SetSelected(bool selected)
        {
            m_selectetd = selected;
        }

        public void ResetKey()
        {
            SetKeyTextByAction(m_inputAction, m_currentKeyControl);
        }

        // Security, but not needed for now
        //private void OnDisable()
        //{
        //    SetKeyTextByAction(m_inputAction, m_currentKeyControl);
        //}
    }
}

