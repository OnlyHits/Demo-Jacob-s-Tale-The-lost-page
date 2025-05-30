using UnityEngine;
using UnityEngine.UI;
using CustomArchitecture;
using TMPro;
using UnityEngine.EventSystems;
using static CustomArchitecture.CustomArchitecture;
using UnityEngine.InputSystem;

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

        public bool IsSettingsPanel => m_currentPanelIndex == 1;
        public bool IsControlsPanel => m_currentPanelIndex == 2;
        public bool IsBasePanelShown => m_currentPanelIndex == m_basePanelIndex;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void LateInit(params object[] parameters)
        {
            base.LateInit(parameters);
        }
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToNavigate(OnNavigateInputChanged);
            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToValidate(OnValidateInput);
            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToCancel(OnCanceledInput);

            m_tLanguage.text = ComicGameCore.Instance.GetSettings().Language.ToString();
            m_sVolumeEffect.value = ComicGameCore.Instance.GetSettings().MusicVolume;
            m_sVolumeMusic.value = ComicGameCore.Instance.GetSettings().SoundEffectVolume;

            m_bPlay.onClick.AddListener(Play);
            m_bOptions.onClick.AddListener(() => ShowPanelByIndex(1));
            m_bControls.onClick.AddListener(() => ShowPanelByIndex(2));
            m_bCredits.onClick.AddListener(() => ComicGameCore.Instance.MainGameMode.GetViewManager().Show<CreditView>());
            m_bExit.onClick.AddListener(Exit);
            m_bBack.onClick.AddListener(ShowBasePanel);

            //ShowPanelByIndex(m_basePanelIndex);
        }
        #endregion

        #region INTERNAL

        public override void ActiveGraphic(bool active)
        {
            base.ActiveGraphic(active);
        }

        #endregion INTERNAL


        #region CALLBACKS

        private void OnInputVertical(Vector2 value)
        {
            int destIndex = m_currentElementIdx;
            int saveIndex = m_currentElementIdx;

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
                if (IsControlsPanel)
                {
                    UnSelectControlElemByIndex(saveIndex);
                }
                if (m_debug) Debug.Log("---- > Navigate on element = " + m_currentElement.name);
            }
        }
        private void OnInputHorizontal(Vector2 value)
        {
            if (!IsSettingsPanel)
                return;

            if (m_currentElement is Slider slider)
            {
                float volume = slider.value + (value.x / 10);

                slider.value = volume;
                // Move music volume (the only slider is the volume)
                if (slider == m_sVolumeEffect)
                {
                    ComicGameCore.Instance.GetSettings().SetSoundEffectVolume(volume);
                }
                // Move effect volume (the only slider is the volume)
                else if (slider == m_sVolumeMusic)
                {
                    ComicGameCore.Instance.GetSettings().SetMusicVolume(volume);
                }
            }

            // Move language (the only text is the lang)
            else if (m_currentElement is TextMeshProUGUI text)
            {
                Language currentLang = ComicGameCore.Instance.GetSettings().Language;
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

                ComicGameCore.Instance.GetSettings().SetLanguage(destLang);
                text.text = destLang.ToString();
            }
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
            if (inputType == InputType.RELEASED)
            {
                if (m_debug) Debug.Log("---> Validate " + value.ToString());
                if (m_currentElement is Button button)
                {
                    button.onClick?.Invoke();
                }
                else if (IsControlsPanel)
                {
                    if (!IsBidingKey())
                    {
                        SelectCurrentControlElem();
                    }
                }
            }
        }

        private void OnCanceledInput(InputType inputType, bool value)
        {
            if (inputType == InputType.RELEASED)
            {
                ControllerType usedController = ComicGameCore.Instance.GetDeviceManager().GetUsedController();

                if (m_debug) Debug.Log("---> Cancel " + value.ToString());

                if (IsBasePanelShown)
                {
                    if (usedController == ControllerType.GAMEPAD)
                    {
                        Play();
                    }
                }

                if (!IsControlsPanel)
                {
                    ShowBasePanel();
                }
                else
                {
                    if (!IsBidingKey())
                    {
                        ShowBasePanel();
                    }
                    else
                    {
                        if (usedController == ControllerType.KEYBOARD)
                        {
                            UnSelectCurrentControlElem();
                        }
                    }
                    //UnSelectAllBindingElemens();
                }
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


        #region CONTROL PANEL METHODS

        public bool IsBidingKey()
        {
            if (!IsControlsPanel)
                return false;

            foreach (UIBehaviour elem in m_currentPanelData.selectableElements)
            {
                if (elem.GetComponentInParent<KeyBindUI>() is KeyBindUI keyBind)
                {
                    if (keyBind.IsBindingKey)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UnSelectAllBindingElemens()
        {
            if (!IsControlsPanel)
                return;
            foreach (UIBehaviour elem in m_currentPanelData.selectableElements)
            {
                KeyBindUI keyBindUI = elem.GetComponentInParent<KeyBindUI>();
                if (keyBindUI == null)
                    continue;
                keyBindUI.SetSelected(false);
            }
        }

        private void UnSelectControlElemByIndex(int index)
        {
            if (!IsControlsPanel)
                return;
            UIBehaviour elem = m_currentPanelData.selectableElements[index];
            KeyBindUI keyBindUI = elem.GetComponentInParent<KeyBindUI>();
            keyBindUI.SetSelected(false);
        }

        private void UnSelectCurrentControlElem()
        {
            if (!IsControlsPanel)
                return;
            if (m_currentElement.GetComponentInParent<KeyBindUI>() is KeyBindUI keyBind)
            {
                keyBind.ResetKey();
                keyBind.SetSelected(false);
            }
        }

        private void SelectCurrentControlElem()
        {
            if (!IsControlsPanel)
                return;
            KeyBindUI elem = m_currentElement.GetComponentInParent<KeyBindUI>();
            elem.SetSelected(true);
        }

        #endregion CONTROL PANEL METHODS

        private void Play()
        {
            ComicGameCore.Instance.MainGameMode.GetNavigationManager().TryPause();
        }

        private void Exit()
        {
            Application.Quit();
        }
    }
}
