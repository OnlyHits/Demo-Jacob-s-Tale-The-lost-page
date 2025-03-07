using System.Collections;
using CustomArchitecture;
using UnityEngine;
using static CustomArchitecture.CustomArchitecture;
using static Comic.NavigationManager.NavigationFocus;
using Unity.VisualScripting;

namespace Comic
{
    public class NavigationManager : BaseBehaviour
    {
        public enum NavigationFocus
        {
            Focus_Game,
            Focus_Hud
        }

        // Input managers
        private NavigationInput m_navigationInput;
        private GlobalInput m_globalInput;
        private PageInput m_pageInput;

        // Global managers
        private HudManager m_hudManager;
        private GameManager m_gameManager;
        private URP_CameraManager m_cameraManager;
        private bool m_isInitialized;
        private bool m_isRunning = false;
        private NavigationFocus m_navigationFocus;

        public NavigationInput GetNavigationInput() => m_navigationInput;
        public GlobalInput GetGlobalInput() => m_globalInput;
        public PageInput GetPageInput() => m_pageInput;
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

            InitFocus();
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

            if (parameters.Length < 4 || parameters[3] is not URP_CameraManager)
                Debug.LogWarning("Unable to get URP_CameraManager");
            else
                m_cameraManager = (URP_CameraManager)parameters[3];

            if (!ComponentUtils.GetOrCreateComponent<PageInput>(gameObject, out m_pageInput))
                Debug.LogWarning("Unable to get or create PageInput");
            else
                m_pageInput.Init(ComicGameCore.Instance.GetInputAsset());

            if (!ComponentUtils.GetOrCreateComponent<NavigationInput>(gameObject, out m_navigationInput))
                Debug.LogWarning("Unable to get or create NavigationInput");
            else
                m_navigationInput.Init();

            m_pageInput.SubscribeToNextPage(TryNextPage);
            m_pageInput.SubscribeToPreviousPage(TryPrevPage);
        }
        #endregion

        private void InitFocus()
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

        public void TryNextPage(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
                if (m_hudManager != null && m_gameManager == null)
                {
                    TryChangePageHudOnly(true);
                }
                else if (m_hudManager == null && m_gameManager != null)
                {
                    TryChangePageGameOnly(true);
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
                    TryChangePageHudOnly(false);
                }
                else if (m_hudManager == null && m_gameManager != null)
                {
                    TryChangePageGameOnly(false);
                }
                else if (m_hudManager != null && m_gameManager != null)
                {
                    StartCoroutine(TryChangePageAll(false));
                }
            }
        }

        private void TryChangePageGameOnly(bool is_next)
        {
            if (m_gameManager.GetPageManager().CanChangePage(is_next))
                m_gameManager.GetPageManager().ChangePageDirty(is_next);
        }

        private void TryChangePageHudOnly(bool is_next)
        {
            m_hudManager.TurnPage(is_next, true, false);
        }

        private IEnumerator TryChangePageAll(bool is_next)
        {
            if (!IsRunning() && m_gameManager.GetPageManager().CanChangePage(is_next))
            {
                m_isRunning = true;

                SetupChangePage(is_next);

                yield return StartCoroutine(m_cameraManager.TakeScreenshot(false));

                m_gameManager.GetPageManager().RestorePageAfterScreenshot(is_next);

                bool can_change_page = m_gameManager.GetPageManager().IsAbleToAccessPage();

                m_hudManager.TurnPage(is_next, false, !can_change_page);

                yield return new WaitUntil(() => !m_hudManager.GetTurningPage().IsTurning());

                OnChangePageEnd(is_next, can_change_page);

                m_isRunning = false;
            }
            else
                yield break;
        }

        private void SetupChangePage(bool is_next)
        {
            m_hudManager.MatchBounds(m_cameraManager.GetScreenshotBounds());

            PauseInput();
            m_gameManager.GetPageManager().SetupPageForScreenshot(is_next);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
        }

        private void OnChangePageEnd(bool is_next, bool can_change_page)
        {
            m_gameManager.GetPageManager().OnPageChangeEnd(is_next, !can_change_page);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(false);
            m_gameManager.GetCharacterManager().PauseAllCharacters(false);

            ChangeInputFocus(Focus_Game);
        }

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
            PauseInput();

            SetupOpenBook();

            yield return StartCoroutine(m_cameraManager.TakeScreenshot(true));

            RestoreOpenBook();

            m_hudManager.TurnCover();

            yield return new WaitUntil(() => !m_hudManager.GetTurningPage().IsTurning());

            OnOpenBookEnd();

            yield return new WaitForSeconds(1f);

            SetupFirstPage();

            yield return StartCoroutine(m_cameraManager.TakeScreenshot(false));

            RestoreFirstPage();

            m_hudManager.TurnCover();

            yield return new WaitUntil(() => !m_hudManager.GetTurningPage().IsTurning());

            OnOpenFirstPageEnd();

            ChangeInputFocus(Focus_Game);

            m_isRunning = false;
        }

        public void SetupOpenBook()
        {
            m_hudManager.MatchBounds(m_gameManager.GetCoverSpriteRenderer(true).bounds);
            m_hudManager.GetViewManager().Show<BookCoverView>();
            m_gameManager.EnableGameBackground(false);
        }

        private void RestoreOpenBook()
        {
            m_hudManager.GetViewManager().Show<ProductionView>();
            m_gameManager.EnableGameBackground(true);
            m_gameManager.EnableBookBackground(false, false);
        }

        private void OnOpenBookEnd()
        {
            m_gameManager.EnableBookBackground(true, false);
        }

        public void SetupFirstPage()
        {
            m_hudManager.MatchBounds(m_cameraManager.GetScreenshotBounds());

            m_gameManager.GetPageManager().SetStartingPage();
            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
        }

        private void RestoreFirstPage()
        {
            m_hudManager.GetViewManager().Show<ProgressionView>();
            m_gameManager.GetPageManager().DisableCurrentPage();
        }

        private void OnOpenFirstPageEnd()
        {
            m_gameManager.GetCharacterManager().PauseAllCharacters(false);

            m_gameManager.GetPageManager().SetStartingPage();
            m_gameManager.GetPageManager().GetCurrentPage().Pause(false);
        }

        public void TryPause()
        {
            if (m_hudManager != null && m_gameManager == null)
            {
                // whatever for the moment
                TryPauseHudOnly(true);
            }
            else if (m_hudManager == null && m_gameManager != null)
            {
                // whatever for the moment
                TryPauseGameOnly(true);
            }
            else if (m_hudManager != null && m_gameManager != null)
            {
                StartCoroutine(TryPauseAll());
            }
        }

        private void TryPauseGameOnly(bool pause)
        {

        }

        private void TryPauseHudOnly(bool pause)
        {
        }

        private IEnumerator TryPauseAll()
        {
            bool pause = m_navigationFocus == NavigationFocus.Focus_Game;

            if (IsRunning() || (!pause && !m_hudManager.CanResume()))
                yield break;

            m_isRunning = true;

            SetupPause(pause);

            yield return StartCoroutine(m_cameraManager.TakeScreenshot(false));

            RestorePause(pause);

            m_hudManager.TurnPage(pause, false, false);

            yield return new WaitUntil(() => !m_hudManager.GetTurningPage().IsTurning());

            OnPauseEnd(pause);

            m_isRunning = false;

            yield return null;
        }

        private void SetupPause(bool pause)
        {
            m_hudManager.MatchBounds(m_cameraManager.GetScreenshotBounds());

            m_hudManager.SetupPageForScreenshot(pause);

            PauseInput();

            m_gameManager.GetPageManager().GetCurrentPage().Pause(true);
            m_gameManager.GetCharacterManager().PauseAllCharacters(true);
        }

        private void RestorePause(bool pause)
        {
            m_hudManager.RestorePageAfterScreenshot(pause);
        }

        private void OnPauseEnd(bool pause)
        {
            m_hudManager.OnPageChangeEnd(pause);

            m_gameManager.GetPageManager().GetCurrentPage().Pause(false);
            m_gameManager.GetCharacterManager().PauseAllCharacters(false);

            ChangeInputFocus(m_navigationFocus == Focus_Hud ? Focus_Game : Focus_Hud);
        }


        public void PauseInput()
        {
            m_navigationInput.Pause(true);
            m_pageInput.Pause(true);
            m_globalInput.Pause(true);

            if (m_gameManager != null)
                m_gameManager.GetCharacterManager().GetPlayer().GetInputController().Pause(true);
        }

        private void ChangeInputFocus(NavigationFocus new_focus)
        {
            m_navigationFocus = new_focus;

            if (m_navigationFocus == NavigationFocus.Focus_Game)
            {
                m_globalInput.Pause(false);
                m_pageInput.Pause(false);
                m_navigationInput.Pause(true);

                if (m_gameManager != null)
                    m_gameManager.GetCharacterManager().GetPlayer().GetInputController().Pause(false);
            }
            else if (m_navigationFocus == NavigationFocus.Focus_Hud)
            {
                m_navigationInput.Pause(false);
                m_globalInput.Pause(false);
                m_pageInput.Pause(true);

                if (m_gameManager != null)
                    m_gameManager.GetCharacterManager().GetPlayer().GetInputController().Pause(true);
            }
        }
    }
}