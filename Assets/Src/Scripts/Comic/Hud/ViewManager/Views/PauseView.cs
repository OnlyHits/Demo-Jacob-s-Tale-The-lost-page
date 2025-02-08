using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using CustomArchitecture;
using System.Linq;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.Rendering;

namespace Comic
{
    public class PauseView : AView
    {
        [Serializable]
        public struct PanelData
        {
            public PanelType type;
            public GameObject panelObj;

            // Old usage
            [HideInInspector] public Button startButton;
            [HideInInspector] public List<Button> buttons;

            public UIBehaviour startElement;
            public List<UIBehaviour> selectableElements;
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
        [SerializeField] private Color m_baseColorElement = Color.black;
        [SerializeField] private Color m_selectedColor = Color.red;

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
        [SerializeField, ReadOnly] private int m_currentElementIdx = 0;
        [SerializeField, ReadOnly] private UIBehaviour m_currentElement;

        // Old usage
        // private int m_currentButtonIdx = 0;
        // private Button m_currentButton;

        [SerializeField, ReadOnly] private PanelType m_basePanelType = PanelType.BASE;
        [SerializeField, ReadOnly] private PanelType m_currentPanelType = PanelType.NONE;
        [SerializeField, ReadOnly] private PanelType m_lastPanelType = PanelType.NONE;

        #region UNITY CALLBACKS

        private void Awake()
        {
            m_bPlay.onClick.AddListener(Play);
            m_bOptions.onClick.AddListener(() => ShowPanel(PanelType.OPTIONS));
            m_bExit.onClick.AddListener(Exit);
            m_bBack.onClick.AddListener(ShowBasePanel);
        }

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

        #endregion UNITY CALLBACKS

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

        private void OnInputVertical(Vector2 value)
        {
            int destIndex = m_currentElementIdx;

            if (value.y < 0)
            {
                destIndex = m_currentElementIdx + 1 >= m_currentPanelData.buttons.Count ? 0 : m_currentElementIdx + 1;
            }
            else if (value.y > 0)
            {
                destIndex = m_currentElementIdx - 1 < 0 ? m_currentPanelData.buttons.Count - 1 : m_currentElementIdx - 1;
            }

            if (destIndex == m_currentElementIdx)
                return;

            // TrySetElement(out m_currentElement)
            if (TrySetElementByIndex(out m_currentElement, destIndex))
            {
                m_currentElementIdx = destIndex;
                Debug.Log("---- > Navigate on button = " + m_currentElement.name);
            }
        }
        private void OnInputHorizontal(Vector2 value)
        {
            if (m_currentElement is Slider slider)
            {
                slider.value = slider.value + (value.x / 10);
            }
            else if (m_currentElement is TextMeshProUGUI text)
            {
                if (value.x > 0)
                {
                    text.text = (text.text == "english") ? "french" : "english";
                }
                else
                {
                    text.text = (text.text == "english") ? "french" : "english";

                }
            }
        }

        private void OnNavigateInputChanged(InputType inputType, Vector2 value)
        {
            if (isCd) return;
            isCd = true;

            if (value.y > 0 || value.y < 0)
            {
                OnInputVertical(value);
            }
            else if (value.x > 0 || value.x < 0)
            {
                OnInputHorizontal(value);
            }
        }
        private void OnValidateInput(InputType inputType, bool value)
        {
            if (value)
            {
                Debug.Log("---> Validate " + value.ToString());
                if (m_currentElement is Button button)
                {
                    button.onClick?.Invoke();
                }
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


        #region UI ELEMENTS

        private bool TrySetStartingElement(out UIBehaviour element)
        {
            UIBehaviour startElement = m_currentPanelData.startElement;

            if (TrySetElement(out element, startElement))
            {
                SelectElement(element, m_currentPanelData.selectableElements);
                return true;
            }
            Debug.LogWarning("Starting element [" + m_currentPanelData.startElement.name + "] of PauseView panels is not in panel elements list.");
            return false;
        }

        private bool TrySetElementByIndex(out UIBehaviour destElement, int newIndex)
        {
            UIBehaviour newElement = m_currentPanelData.selectableElements.ElementAt(newIndex);

            if (newElement == null)
            {
                destElement = m_currentElement;
                Debug.LogWarning("Failed to select element at index " + newIndex.ToString() + ".");
                return false;
            }
            if (TrySetElement(out destElement, newElement))
            {
                SelectElement(destElement, m_currentPanelData.selectableElements);
                return true;
            }
            Debug.LogWarning("Failed to select element by index");
            return false;
        }
        private bool TrySetElement(out UIBehaviour destElement, UIBehaviour element)
        {
            List<UIBehaviour> elements = m_currentPanelData.selectableElements;

            m_currentElementIdx = elements.IndexOf(element);
            if (m_currentElementIdx < 0)
            {
                m_currentElementIdx = 0;
                destElement = m_currentElement;
                return false;
            }
            destElement = elements[m_currentElementIdx];
            return true;
        }

        private void SelectElement(UIBehaviour selectedElement, List<UIBehaviour> others)
        {
            foreach (UIBehaviour element in others)
            {
                Color destColor = default;

                if (element == selectedElement)
                    destColor = m_selectedColor;
                else
                    destColor = m_baseColorElement;

                if (element is Button button)
                {
                    button.transform.GetComponentInChildren<TextMeshProUGUI>().color = destColor;
                    button.image.color = destColor;
                }
                else if (element is Slider slider)
                {
                    slider.handleRect.GetComponentInChildren<Image>().color = destColor;
                    slider.fillRect.GetComponentInChildren<Image>().color = destColor;
                }
                else if (element is TextMeshProUGUI textMesh)
                {
                    textMesh.color = destColor;
                }
            }
        }

        #endregion

        // Old Usage
        #region BUTTONS
        /*
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
                            button.transform.GetComponentInChildren<TextMeshProUGUI>().color = m_baseColorElement;
                            button.image.color = Color.white;
                        }
                    }
                }
        */
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

            //TrySetStartingButton(out m_currentButton);
            TrySetStartingElement(out m_currentElement);
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
