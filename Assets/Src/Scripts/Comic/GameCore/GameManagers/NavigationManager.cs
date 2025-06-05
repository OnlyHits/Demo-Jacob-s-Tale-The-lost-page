using System.Collections;
using Comc;
using CustomArchitecture;
using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class NavigationManager : BaseBehaviour
    {
        public enum NavigationFocus
        {
            Focus_Game,
            Focus_Hud,
            Focus_Panel,
        }

        public enum TurnSequenceType
        {
            Sequence_Pause,
            Sequence_TurnPage,
            Sequence_OpenBook,
            Sequence_Production,
            Sequence_None,
        }

        // Input managers
        private NavigationInput m_navigationInput;
        private GlobalInput m_globalInput;
        private PageInput m_pageInput;
        private PanelInput m_panelInput;

        // Global managers
        private HudManager m_hudManager;
        private GameManager m_gameManager;

        private bool m_isRunning = false;
        private NavigationFocus m_navigationFocus;

        public NavigationInput GetNavigationInput() => m_navigationInput;
        public GlobalInput GetGlobalInput() => m_globalInput;
        public PageInput GetPageInput() => m_pageInput;
        public PanelInput GetPanelInput() => m_panelInput;
        public bool IsRunning() => m_isRunning;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            if (m_navigationInput != null)
                m_navigationInput.LateInit();

            if (m_pageInput != null)
                m_pageInput.LateInit();

            if (m_globalInput != null)
                m_globalInput.LateInit();

            if (m_panelInput != null)
                m_panelInput.LateInit();

            // could be remove or on preprocessor call
            InitInputFocus();
        }
        public override void Init(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not GameManager)
                Debug.LogWarning("Unable to get GameManager");
            else
                m_gameManager = (GameManager)parameters[0];

            if (parameters.Length < 2 || parameters[1] is not HudManager)
                Debug.LogWarning("Unable to get HudManager");
            else
                m_hudManager = (HudManager)parameters[1];

            if (parameters.Length < 3 || parameters[2] is not GlobalInput)
                Debug.LogWarning("Unable to get GlobalInput");
            else
                m_globalInput = (GlobalInput)parameters[2];

            if (!ComponentUtils.GetOrCreateComponent<PageInput>(gameObject, out m_pageInput))
                Debug.LogWarning("Unable to get or create PageInput");
            else
                m_pageInput.Init(ComicGameCore.Instance.GetInputAsset());

            if (!ComponentUtils.GetOrCreateComponent<NavigationInput>(gameObject, out m_navigationInput))
                Debug.LogWarning("Unable to get or create NavigationInput");
            else
                m_navigationInput.Init();

            if (!ComponentUtils.GetOrCreateComponent<PanelInput>(gameObject, out m_panelInput))
                Debug.LogWarning("Unable to get or create PanelInput");
            else
                m_panelInput.Init(ComicGameCore.Instance.GetInputAsset());

            m_pageInput.SubscribeToNextPage(TryNextPage);
            m_pageInput.SubscribeToPreviousPage(TryPrevPage);
            m_globalInput.SubscribeToActivatePanelNav(TryActivatePanelNavigation);
        }
        #endregion

        #region Panel Navigations
        private IEnumerator ActivePanelCameraTransition()
        {
            m_isRunning = true;

            PauseAllInputs();

            StartCoroutine(m_gameManager.GetVolumeAnimator().AnimateVignette(true, ComicCinemachineMgr.Instance.SmoothBlend.BlendTime));

            yield return StartCoroutine(ComicCinemachineMgr.Instance.FocusCamera(
                m_gameManager.GetPageManager().GetCurrentPage().GetStartingNavigable().GetCinemachineCamera().Camera,
                ComicCinemachineMgr.CameraList.Managed_Cameras,
                true,
                ComicCinemachineMgr.Instance.SmoothBlend));

            m_gameManager.GetPageManager().GetCurrentPage().StartNavigate();

            ChangeInputFocus(NavigationFocus.Focus_Panel);
        }

        private IEnumerator UnactivePanelCameraTransition()
        {
            PauseAllInputs();

            m_gameManager.GetPageManager().GetCurrentPage().StopNavigate();

            StartCoroutine(m_gameManager.GetVolumeAnimator().AnimateVignette(false, ComicCinemachineMgr.Instance.SmoothBlend.BlendTime));

            yield return StartCoroutine(ComicCinemachineMgr.Instance.FocusCamera(m_gameManager.GetBgCinemachineCamera().Camera,
                ComicCinemachineMgr.CameraList.Managed_Cameras,
                true,
                ComicCinemachineMgr.Instance.SmoothBlend));

            ChangeInputFocus(NavigationFocus.Focus_Game);

            m_isRunning = false;
        }

        public void TryActivatePanelNavigation(InputType input, bool b)
        {
            if (m_gameManager.GetPageManager().GetCurrentPage() == null)
            {
                Debug.LogWarning("You try to activate panel navigation on a null page");
                return;
            }

            if (input == InputType.RELEASED)
            {

                if (m_isRunning && m_gameManager.GetPageManager().GetCurrentPage().IsRunning())
                {
                    StartCoroutine(UnactivePanelCameraTransition());
                }
                else if (!m_isRunning && !m_gameManager.GetPageManager().GetCurrentPage().IsRunning())
                {
                    StartCoroutine(ActivePanelCameraTransition());
                }
            }
        }


        #endregion Panel Navigation

        #region Navigate Sequence
        public void TryNextPage(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
                if (m_hudManager != null && m_gameManager == null)
                {
                    // not implemented
                }
                else if (m_hudManager == null && m_gameManager != null)
                {
                    if (m_gameManager.GetPageManager().CanChangePage(true))
                        m_gameManager.GetPageManager().ChangePageDirty(true);
                }
                else if (m_hudManager != null && m_gameManager != null)
                {
                    StartCoroutine(TryChangePageAll(true));
                }
            }
        }

        public void TryPrevPage(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
                if (m_hudManager != null && m_gameManager == null)
                {
                    // not implemented
                }
                else if (m_hudManager == null && m_gameManager != null)
                {
                    if (m_gameManager.GetPageManager().CanChangePage(false))
                        m_gameManager.GetPageManager().ChangePageDirty(false);
                }
                else
                if (m_hudManager != null && m_gameManager != null)
                {
                    StartCoroutine(TryChangePageAll(false));
                }
            }
        }

        private IEnumerator TryChangePageAll(bool is_next)
        {
            if (!IsRunning() && m_gameManager.GetPageManager().CanChangePage(is_next))
            {
                m_isRunning = true;

                OnBeforeScreenshot(is_next, TurnSequenceType.Sequence_TurnPage);

                yield return StartCoroutine(ComicCinemachineMgr.Instance.Screenshoter.TakePageScreenshot());
                    
                OnAfterScreenshot(is_next, TurnSequenceType.Sequence_TurnPage);

                bool can_change_page = m_gameManager.GetPageManager().IsAbleToAccessPage();

                if (can_change_page)
                {
                    yield return StartCoroutine(m_hudManager.TurnPage(is_next, m_gameManager.GetPageSpriteRenderer(true).bounds, 0.8f));
                }
                else
                {
                    yield return StartCoroutine(m_hudManager.TurnPageError(is_next, m_gameManager.GetPageSpriteRenderer(true).bounds, 0.8f));
                }

                OnTurnSequenceFinish(is_next, TurnSequenceType.Sequence_TurnPage);

                m_isRunning = false;
            }
            else
                yield break;
        }
        #endregion

        #region Begin game sequence
        public void StartGameSequence()
        {
            if (m_hudManager != null && m_gameManager != null)
            {
                StartCoroutine(StartGameCoroutine());
            }
        }

        private IEnumerator StartGameCoroutine()
        {
            if (IsRunning())
                yield break;

            m_isRunning = true;

            m_gameManager.GetCharacterManager().PauseAllCharacters(true);

            OnBeforeScreenshot(true, TurnSequenceType.Sequence_OpenBook);

            yield return StartCoroutine(ComicCinemachineMgr.Instance.Screenshoter.TakeCoverScreenshot());

            OnAfterScreenshot(true, TurnSequenceType.Sequence_OpenBook);

            yield return StartCoroutine(m_hudManager.TurnCover(m_gameManager.GetCoverSpriteRenderer(true).bounds, 4f));

            OnTurnSequenceFinish(true, TurnSequenceType.Sequence_OpenBook);

            yield return new WaitForSeconds(1f);

            OnBeforeScreenshot(true, TurnSequenceType.Sequence_Production);

            yield return StartCoroutine(ComicCinemachineMgr.Instance.Screenshoter.TakePageScreenshot());

            OnAfterScreenshot(true, TurnSequenceType.Sequence_Production);

            yield return StartCoroutine(m_hudManager.TurnMultiplePages(true, m_gameManager.GetPageSpriteRenderer(true).bounds, 10, .7f));

            OnTurnSequenceFinish(true, TurnSequenceType.Sequence_Production);

            m_isRunning = false;
        }

        #endregion

        #region Pause Sequence
        public void TryPause()
        {
            if (m_hudManager != null && m_gameManager == null)
            {
                // not implemented
            }
            else if (m_hudManager == null && m_gameManager != null)
            {
                // not implemented
            }
            else if (m_hudManager != null && m_gameManager != null)
            {
                StartCoroutine(TryPauseAll());
            }
        }

        private IEnumerator TryPauseAll()
        {
            // mdr
            bool pause = m_navigationFocus == NavigationFocus.Focus_Game;

            if (IsRunning() || (!pause && !m_hudManager.CanResume()))
                yield break;

            m_isRunning = true;

            OnBeforeScreenshot(pause, TurnSequenceType.Sequence_Pause);

            yield return StartCoroutine(ComicCinemachineMgr.Instance.Screenshoter.TakePageScreenshot());

            OnAfterScreenshot(pause, TurnSequenceType.Sequence_Pause);

            yield return StartCoroutine(m_hudManager.TurnMultiplePages(pause, m_gameManager.GetPageSpriteRenderer(true).bounds, 5, .6f));

            OnTurnSequenceFinish(pause, TurnSequenceType.Sequence_Pause);

            m_isRunning = false;

            yield return null;
        }

        #endregion

        #region Before screenshot
        private void OnBeforeScreenshot(bool is_next, TurnSequenceType setup_type)
        {
            PauseAllInputs();

            switch (setup_type)
            {
                case TurnSequenceType.Sequence_Pause:
                    SetupPause(is_next);
                    break;
                case TurnSequenceType.Sequence_TurnPage:
                    SetupTurnPage(is_next);
                    break;
                case TurnSequenceType.Sequence_Production:
                    SetupFirstPage();
                    break;
                case TurnSequenceType.Sequence_OpenBook:
                    SetupBookCover(is_next);
                    break;
                default:
                    break;
            }
        }

        private void SetupTurnPage(bool is_next)
        {
            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
            m_gameManager.GetPageManager().OnBeforeScreenshot(is_next);
        }

        private void SetupBookCover(bool is_next)
        {
            m_hudManager.GetViewManager().Show<BookCoverView>();
            m_gameManager.EnableGameBackground(false);
        }

        private void SetupPause(bool pause)
        {
            m_hudManager.SetupPageForScreenshot(pause);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
        }
        public void SetupFirstPage()
        {
            m_gameManager.GetPageManager().SetStartingPage();
            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
        }
        #endregion

        #region After screenshot
        private void OnAfterScreenshot(bool is_next, TurnSequenceType setup_type)
        {
            PauseAllInputs();

            switch (setup_type)
            {
                case TurnSequenceType.Sequence_Pause:
                    RestorePause(is_next);
                    break;
                case TurnSequenceType.Sequence_TurnPage:
                    RestoreTurnPage(is_next);
                    break;
                case TurnSequenceType.Sequence_Production:
                    RestoreFirstPage();
                    break;
                case TurnSequenceType.Sequence_OpenBook:
                    RestoreOpenBook();
                    break;
                default:
                    break;
            }
        }

        private void RestoreTurnPage(bool is_next)
        {
            m_gameManager.GetPageManager().OnAfterScreenshot(is_next);
        }

        private void RestoreOpenBook()
        {
            m_hudManager.GetViewManager().Show<ProductionView>();
            m_gameManager.EnableGameBackground(true);
            m_gameManager.EnableBookBackground(false, false);
        }

        private void RestorePause(bool pause)
        {
            m_hudManager.RestorePageAfterScreenshot(pause);
        }

        private void StopBookCover()
        {
            m_gameManager.EnableBookBackground(true, false);
        }

        private void RestoreFirstPage()
        {
            m_hudManager.GetViewManager().Show<ProgressionView>();
            m_gameManager.GetPageManager().DisableCurrentPage();
        }
        #endregion

        #region On sequence finished
        private void OnTurnSequenceFinish(bool is_next, TurnSequenceType setup_type)
        {
            PauseAllInputs();

            switch (setup_type)
            {
                case TurnSequenceType.Sequence_Pause:
                    StopPause(is_next);
                    break;
                case TurnSequenceType.Sequence_TurnPage:
                    StopTurnPage(is_next);
                    break;
                case TurnSequenceType.Sequence_Production:
                    StopFirstPage();
                    break;
                case TurnSequenceType.Sequence_OpenBook:
                    StopBookCover();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called when pause sequence has ended
        /// </summary>
        /// <param name="pause"></param>
        private void StopPause(bool pause)
        {
            m_hudManager.OnPageChangeEnd(pause);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(pause);
            m_gameManager.GetCharacterManager().PauseAllCharacters(pause);

            ChangeInputFocus(m_navigationFocus == NavigationFocus.Focus_Hud ? NavigationFocus.Focus_Game : NavigationFocus.Focus_Hud);
        }

        /// <summary>
        /// Called when turn page sequence ended
        /// </summary>
        /// <param name="pause"></param>
        private void StopTurnPage(bool is_next)
        {
            bool can_change_page = m_gameManager.GetPageManager().IsAbleToAccessPage();

            m_gameManager.GetPageManager().OnTurnSequenceFinish(is_next, !can_change_page);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(false);
            m_gameManager.GetCharacterManager().PauseAllCharacters(false);

            ChangeInputFocus(NavigationFocus.Focus_Game);
        }

        /// <summary>
        /// Called when turn page "OnlyhitProduction" ended
        /// </summary>
        /// <param name="pause"></param>
        private void StopFirstPage()
        {
            m_gameManager.GetCharacterManager().PauseAllCharacters(false);

            m_gameManager.GetPageManager().SetStartingPage();
            m_gameManager.GetPageManager().GetCurrentPage().Pause(false);

            ChangeInputFocus(NavigationFocus.Focus_Game);
        }
        #endregion

        #region Input Management
        private void InitInputFocus()
        {
            if (m_hudManager != null && m_gameManager == null)
            {
                ChangeInputFocus(NavigationFocus.Focus_Hud);
            }
            else if (m_hudManager == null && m_gameManager != null)
            {
                ChangeInputFocus(NavigationFocus.Focus_Game);
            }
            else if (m_hudManager != null && m_gameManager != null)
            {
                ChangeInputFocus(NavigationFocus.Focus_Game);
            }
        }

        /// <summary>
        /// Pause all input :
        //  on turn page sequence or on game start for exemple
        /// </summary>
        public void PauseAllInputs()
        {
            m_navigationInput.Pause(true);
            m_pageInput.Pause(true);
            m_globalInput.Pause(true);
            m_panelInput.Pause(true);

            if (m_gameManager != null)
                m_gameManager.GetCharacterManager().GetInputController().Pause(true);
        }

        /// <summary>
        /// Change the input managers which going to be used (for ui navigation or game navigation)
        /// </summary>
        /// <param name="new_focus"></param>
        private void ChangeInputFocus(NavigationFocus new_focus)
        {
            m_navigationFocus = new_focus;

            if (m_navigationFocus == NavigationFocus.Focus_Game)
            {
                m_globalInput.Pause(false);
                m_pageInput.Pause(false);
                m_navigationInput.Pause(true);
                m_panelInput.Pause(true);

                if (m_gameManager != null)
                    m_gameManager.GetCharacterManager().GetInputController().Pause(false);
            }
            else if (m_navigationFocus == NavigationFocus.Focus_Hud)
            {
                m_navigationInput.Pause(false);
                m_globalInput.Pause(false);
                m_pageInput.Pause(true);
                m_panelInput.Pause(true);

                if (m_gameManager != null)
                    m_gameManager.GetCharacterManager().GetInputController().Pause(true);
            }
            else if (m_navigationFocus == NavigationFocus.Focus_Panel)
            {
                m_globalInput.Pause(false);
                m_panelInput.Pause(false);
                m_navigationInput.Pause(true);
                m_pageInput.Pause(true);

                if (m_gameManager != null)
                    m_gameManager.GetCharacterManager().GetInputController().Pause(false);
            }

        }
        #endregion
    }
}