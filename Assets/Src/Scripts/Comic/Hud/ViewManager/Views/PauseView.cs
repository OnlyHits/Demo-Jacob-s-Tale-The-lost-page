using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using CustomArchitecture;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;

namespace Comic
{
    public class PauseView : AView
    {
        [Serializable]
        public struct PanelData
        {
            public PanelType type;
            public GameObject panelObj;
            public Button startButton;
            public List<Button> buttons;
        }

        public enum PanelType
        {
            NONE = 0,
            BASE = 1,
            OPTIONS = 2,
        }

        [Header("PauseView")]
        [SerializeField] private List<PanelData> m_panelsData = new List<PanelData>();

        [Header("Buttons")]
        [SerializeField] private Color m_baseColorButtonText = Color.black;
        [Space]
        [SerializeField] private Button m_bPlay;
        [SerializeField] private Button m_bOptions;
        [SerializeField] private Button m_bExit;
        [SerializeField] private Button m_bBack;

        [Header("Cooldowns")]
        [SerializeField, ReadOnly] private bool isCd = false;
        [SerializeField] private float cd = 0.5f;
        private float timer = 0.1f;

        [Header("Panels Datas")]
        [SerializeField, ReadOnly] private PanelData m_currentPanelData;
        [SerializeField, ReadOnly] private int m_currentButtonIdx = 0;
        [SerializeField, ReadOnly] private Button m_currentButton;

        [SerializeField, ReadOnly] private PanelType m_basePanelType = PanelType.BASE;
        [SerializeField, ReadOnly] private PanelType m_currentPanelType = PanelType.NONE;
        [SerializeField, ReadOnly] private PanelType m_lastPanelType = PanelType.NONE;

        private void Awake()
        {
            m_bPlay.onClick.AddListener(Play);
            m_bOptions.onClick.AddListener(() => ShowPanel(PanelType.OPTIONS));
            m_bExit.onClick.AddListener(Exit);
            m_bBack.onClick.AddListener(ShowBasePanel);
        }

        #region INTERNAL

        public override void ActiveGraphic(bool active)
        {
            ShowPanel(m_basePanelType);
        }

        public override void Init()
        {
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToNavigate(OnNavigateInputChanged);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToValidate(OnValidateInput);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToCancel(OnCanceledInput);

            ShowPanel(m_basePanelType);
        }

        #endregion INTERNAL

        #region CALLBACKS

        protected override void OnUpdate(float elapsed_time)
        {
            base.OnUpdate(elapsed_time);

            if (isCd)
            {
                timer += elapsed_time;
                if (timer >= cd)
                {
                    isCd = false;
                    timer = 0f;
                }
            }
        }

        private void OnNavigateInputChanged(InputType inputType, Vector2 value)
        {
            if (isCd) return;
            isCd = true;

            int destIndex = m_currentButtonIdx;

            if (value.y < 0)
            {
                destIndex = m_currentButtonIdx + 1 >= m_currentPanelData.buttons.Count ? 0 : m_currentButtonIdx + 1;
            }
            else if (value.y > 0)
            {
                destIndex = m_currentButtonIdx - 1 < 0 ? m_currentPanelData.buttons.Count - 1 : m_currentButtonIdx - 1;
            }

            if (TrySetButtonByIndex(out m_currentButton, destIndex))
            {
                m_currentButtonIdx = destIndex;
                Debug.Log("---- > Navigate on button = " + m_currentButton.name);
            }
        }
        private void OnValidateInput(InputType inputType, bool value)
        {
            if (value)
            {
                Debug.Log("---> Validate " + value.ToString());
                m_currentButton.onClick?.Invoke();
            }
        }
        private void OnCanceledInput(InputType inputType, bool value)
        {
            if (value)
            {
                Debug.Log("---> Cancel " + value.ToString());
                ShowBasePanel();
            }
        }

        #endregion CALLBACKS


        #region BUTTONS

        private bool TrySetStartingButton(out Button button)
        {
            Button startButton = m_currentPanelData.startButton;

            if (TrySetButton(out button, startButton))
            {
                SelectButton(button, m_currentPanelData.buttons);
                return true;
            }
            Debug.LogWarning("Starting button [" + m_currentPanelData.startButton.name + "] of PauseView panels is not in panel button list.");
            return false;
        }

        private bool TrySetButtonByIndex(out Button destButton, int newIndex)
        {
            Button newButton = m_currentPanelData.buttons.ElementAt(newIndex);

            if (newButton == null)
            {
                destButton = m_currentButton;
                Debug.LogWarning("Failed to select button by index");
                return false;
            }

            if (TrySetButton(out destButton, newButton))
            {
                SelectButton(destButton, m_currentPanelData.buttons);
                return true;
            }
            Debug.LogWarning("Failed to select button by index");
            return false;
        }

        private bool TrySetButton(out Button destButton, Button button)
        {
            List<Button> buttons = m_currentPanelData.buttons;

            m_currentButtonIdx = buttons.IndexOf(button);
            if (m_currentButtonIdx < 0)
            {
                m_currentButtonIdx = 0;
                destButton = m_currentButton;
                return false;
            }
            destButton = buttons[m_currentButtonIdx];
            return true;
        }

        private void SelectButton(Button selectedButton, List<Button> others)
        {
            foreach (Button button in others)
            {
                if (button == selectedButton)
                {
                    button.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;
                    button.image.color = Color.red;
                }
                else
                {
                    button.transform.GetComponentInChildren<TextMeshProUGUI>().color = m_baseColorButtonText;
                    button.image.color = Color.white;
                }
            }
        }

        #endregion BUTTONS


        #region PANELS

        private void ShowPanel(PanelType panelType)
        {
            m_lastPanelType = m_currentPanelType;
            m_currentPanelType = panelType;

            foreach (PanelData data in m_panelsData)
            {
                if (data.type == panelType)
                {
                    m_currentPanelData = data;
                    data.panelObj.SetActive(true);
                }
                else
                {
                    data.panelObj.SetActive(false);
                }
            }

            TrySetStartingButton(out m_currentButton);
        }

        private void ShowBasePanel()
        {
            ShowPanel(m_basePanelType);
        }

        private void ShowPrevPanel()
        {
            if (m_lastPanelType == PanelType.NONE)
            {
                return;
            }
            ShowPanel(m_lastPanelType);
        }

        #endregion PANELS


        private void Play()
        {
            ComicGameCore.Instance.MainGameMode.GetViewManager().ShowLast();
        }

        private void Exit()
        {
            Application.Quit();
        }

    }
}
