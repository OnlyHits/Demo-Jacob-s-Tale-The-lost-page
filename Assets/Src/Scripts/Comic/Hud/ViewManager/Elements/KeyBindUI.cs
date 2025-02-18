using CustomArchitecture;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Linq;
using System.Collections.Generic;

namespace Comic
{
    public class KeyBindUI : BaseBehaviour
    {
        [SerializeField] private string m_keyParam = "";
        [SerializeField] private TextMeshProUGUI m_tAction;
        [SerializeField] private TextMeshProUGUI m_tKey;
        [SerializeField] private float m_durationKeyChange = 0f;
        public InputAction m_inputAction;
        private bool m_selectetd = false;
        private float m_elapsedTime = 0f;

        protected void Awake()
        {
            m_inputAction = ComicGameCore.Instance.MainGameMode.GetInputAsset().FindAction(m_keyParam);
            if (m_inputAction == null)
            {
                m_tAction.enabled = false;
                m_tKey.enabled = false;
                this.enabled = false;
                Debug.LogError($"Key binding {m_keyParam} does not exist");
                return;
            }

            //m_tAction.text = m_inputAction.name;
            KeyControl keyControl = m_inputAction.GetKeyBoardKeysFromAction().FirstOrDefault();

            m_tKey.text = m_inputAction.GetActionName(keyControl);
            Debug.Log($"> {m_inputAction.name} : [{m_inputAction.GetActionName(keyControl)}]");
        }

        protected override void OnUpdate(float delta)
        {
            if (!m_selectetd) return;

            UpdateWaitingKeyText(true, delta);

            if (RebindKeyUtils.TryGetKeyPressed(out KeyControl keyControl))
            {
                m_inputAction.RebindKey(keyControl);
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

        public void SetSelected(bool set)
        {
            m_selectetd = set;
        }
    }
}
