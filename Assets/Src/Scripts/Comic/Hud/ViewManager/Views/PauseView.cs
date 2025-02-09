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

            public UIBehaviour startElement;
            public List<UIBehaviour> selectableElements;
        }

        public enum PanelType
        {
            NONE = 0,
            BASE = 1,
            OPTIONS = 2,
        }

        [Header("Debug")]
        public bool m_debug;

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

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI m_tLanguage;

        [Header("Slider")]
        [SerializeField] private Slider m_sVolumeEffect;
        [SerializeField] private Slider m_sVolumeMusic;

        [Header("Cooldowns")]
        [SerializeField, ReadOnly] private bool isCd = false;
        [SerializeField] private float cd = 0.5f;
        private float timer = 0.1f;

        [Header("Panels Datas")]
        [SerializeField, ReadOnly] private PanelData m_currentPanelData;
        [SerializeField, ReadOnly] private int m_currentElementIdx = 0;
        [SerializeField, ReadOnly] private UIBehaviour m_currentElement;

        [SerializeField, ReadOnly] private PanelType m_basePanelType = PanelType.BASE;
        [SerializeField, ReadOnly] private PanelType m_currentPanelType = PanelType.NONE;
        [SerializeField, ReadOnly] private PanelType m_lastPanelType = PanelType.NONE;

        #region UNITY CALLBACKS

        private void Awake()
        {
            m_tLanguage.text = ComicGameCore.Instance.GetSettings().m_settingDatas.m_language.ToString();
            m_sVolumeEffect.value = ComicGameCore.Instance.GetSettings().m_settingDatas.m_musicVolume;
            m_sVolumeMusic.value = ComicGameCore.Instance.GetSettings().m_settingDatas.m_effectVolume;
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
                destIndex = m_currentElementIdx + 1 >= m_currentPanelData.selectableElements.Count ? 0 : m_currentElementIdx + 1;
            }
            else if (value.y > 0)
            {
                destIndex = m_currentElementIdx - 1 < 0 ? m_currentPanelData.selectableElements.Count - 1 : m_currentElementIdx - 1;
            }

            if (destIndex == m_currentElementIdx)
                return;

            if (TrySetElementByIndex(out m_currentElement, destIndex))
            {
                if (m_debug) Debug.Log("---- > Navigate on element = " + m_currentElement.name);
            }
        }
        private void OnInputHorizontal(Vector2 value)
        {
            if (m_currentElement is Slider slider)
            {
                float volume = slider.value + (value.x / 10);

                slider.value = volume;
                // Move music volume (the only slider is the volume)
                if (slider == m_sVolumeEffect)
                {
                    ComicGameCore.Instance.GetSettings().m_settingDatas.m_effectVolume = volume;
                }
                // Move effect volume (the only slider is the volume)
                else if (slider == m_sVolumeMusic)
                {
                    ComicGameCore.Instance.GetSettings().m_settingDatas.m_musicVolume = volume;
                }
            }

            // Move language (the only text is the lang)
            else if (m_currentElement is TextMeshProUGUI text)
            {
                Language currentLang = ComicGameCore.Instance.GetSettings().m_settingDatas.m_language;
                Language destLang = default;
                int langCount = System.Enum.GetValues(typeof(Language)).Length;

                if (value.x > 0)
                {
                    int nextValue = ((int)currentLang + 1) % langCount;
                    destLang = (Language)nextValue;
                }
                else if (value.x < 0)
                {
                    int nextValue = ((int)currentLang - 1) % langCount;
                    destLang = (Language)nextValue;
                }

                ComicGameCore.Instance.GetSettings().m_settingDatas.m_language = destLang;
                text.text = destLang.ToString();
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
                if (m_debug) Debug.Log("---> Validate " + value.ToString());
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
                if (m_debug) Debug.Log("---> Cancel " + value.ToString());
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
            m_bBack.gameObject.SetActive(panelType != PanelType.BASE);
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
