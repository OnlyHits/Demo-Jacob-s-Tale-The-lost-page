using UnityEngine;
using UnityEngine.UI;
using CustomArchitecture;
using TMPro;

namespace Comic
{
    public class PauseView : NavigableView
    {

        [Header("Buttons")]
        [SerializeField] private Button m_bPlay;
        [SerializeField] private Button m_bOptions;
        [SerializeField] private Button m_bControls;
        [SerializeField] private Button m_bCredits;
        [SerializeField] private Button m_bExit;
        [SerializeField] private Button m_bBack;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI m_tLanguage;

        [Header("Slider")]
        [SerializeField] private Slider m_sVolumeEffect;
        [SerializeField] private Slider m_sVolumeMusic;


        #region UNITY CALLBACKS

        private void Awake()
        {
            m_tLanguage.text = ComicGameCore.Instance.GetSettings().m_settingDatas.m_language.ToString();
            m_sVolumeEffect.value = ComicGameCore.Instance.GetSettings().m_settingDatas.m_musicVolume;
            m_sVolumeMusic.value = ComicGameCore.Instance.GetSettings().m_settingDatas.m_effectVolume;

            m_bPlay.onClick.AddListener(Play);
            m_bOptions.onClick.AddListener(() => ShowPanelByIndex(1));
            m_bControls.onClick.AddListener(() => ShowPanelByIndex(2));
            m_bCredits.onClick.AddListener(() => ComicGameCore.Instance.MainGameMode.GetViewManager().Show<CreditView>());
            m_bExit.onClick.AddListener(Exit);
            m_bBack.onClick.AddListener(ShowBasePanel);
        }

        #endregion UNITY CALLBACKS

        #region INTERNAL

        public override void ActiveGraphic(bool active)
        {
            base.ActiveGraphic(active);
        }

        public override void Init()
        {
            base.Init();
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToNavigate(OnNavigateInputChanged);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToValidate(OnValidateInput);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToCancel(OnCanceledInput);
            //ShowPanelByIndex(m_basePanelIndex);
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
            //if (m_currentPanelIndex == 1)
            //{
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
                    int nextValue = ((int)currentLang - 1 + langCount) % langCount;
                    destLang = (Language)nextValue;
                }

                ComicGameCore.Instance.GetSettings().m_settingDatas.m_language = destLang;
                text.text = destLang.ToString();
            }
            //}
        }

        private void OnNavigateInputChanged(InputType inputType, Vector2 value)
        {
            if (!gameObject.activeSelf) return;
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
                if (m_currentPanelIndex == 2)
                {
                    Debug.Log("Listen for a key biding");
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


        #region PANELS

        protected override void ShowPanelByIndex(int panelIndex)
        {
            base.ShowPanelByIndex(panelIndex);

            m_bBack.gameObject.SetActive(panelIndex != m_basePanelIndex);
        }

        #endregion PANELS

        private void Play()
        {
            ComicGameCore.Instance.MainGameMode.Pause(false);
        }

        private void Exit()
        {
            Application.Quit();
        }
    }
}
