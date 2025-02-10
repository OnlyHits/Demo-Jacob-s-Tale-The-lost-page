using System;
using System.Collections;
using CustomArchitecture;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        private Action<bool, Page, Page> m_onBeforeSwitchPageCallback;
        // remove
        private Action<bool, Page, Page> m_onMiddleSwitchPageCallback;
        private Action<bool, Page, Page> m_onAfterSwitchPageCallback;
        private Coroutine m_switchPageCoroutine;
        [SerializeField] private bool m_skipSwitchPageAnimation = false;
        private bool m_hasFinishTurning = false;

        // remove
        private Action<bool> m_onAfterCloneCanvasCallback;

        #region CALLBACKS

        public void SubscribeToAfterCloneCanvasCallback(Action<bool> function)
        {
            m_onAfterCloneCanvasCallback -= function;
            m_onAfterCloneCanvasCallback += function;
        }

        public void SubscribeToBeforeSwitchPage(Action<bool, Page, Page> function)
        {
            m_onBeforeSwitchPageCallback -= function;
            m_onBeforeSwitchPageCallback += function;
        }

        public void SubscribeToMiddleSwitchPage(Action<bool, Page, Page> function)
        {
            m_onMiddleSwitchPageCallback -= function;
            m_onMiddleSwitchPageCallback += function;
        }

        public void SubscribeToAfterSwitchPage(Action<bool, Page, Page> function)
        {
            m_onAfterSwitchPageCallback -= function;
            m_onAfterSwitchPageCallback += function;
        }

        #endregion CALLBACKS


        public void RegisterSwitchPageManagerCallbacks()
        {
            ComicGameCore.Instance.MainGameMode.GetHudManager().RegisterToEndTurning(() => m_hasFinishTurning = true);
        }

        private void SwitchPage(bool is_next_page, int idxNewPage)
        {
            if (m_skipSwitchPageAnimation)
            {
                m_currentPageIndex = idxNewPage;
                SwitchPageByIndex(m_currentPageIndex);
            }
            else
            {
                if (m_switchPageCoroutine != null)
                    return;

                m_switchPageCoroutine = StartCoroutine(SwitchPageCoroutine(is_next_page, idxNewPage));
            }
        }

        private IEnumerator SwitchPageCoroutine(bool is_next_page, int idxNewPage)
        {
            if (ComicGameCore.Instance.MainGameMode.GetCameraManager().IsCameraRegister(URP_OverlayCameraType.Camera_Hud))
            {
                Page current_page = m_unlockedPageList[m_currentPageIndex];
                Page new_page = m_unlockedPageList[idxNewPage];

                current_page.Enable(!is_next_page);
                new_page.Enable(is_next_page);

                yield return StartCoroutine(ComicGameCore.Instance.MainGameMode.GetCameraManager().ScreenAndApplyTexture(is_next_page));

                current_page.Enable(is_next_page);
                new_page.Enable(!is_next_page);

                ComicGameCore.Instance.MainGameMode.GetCameraManager().Test(is_next_page);

                yield return new WaitUntil(() => m_hasFinishTurning == true);

                m_hasFinishTurning = false;
            }

            m_switchPageCoroutine = null;
            m_currentPageIndex = idxNewPage;
            SwitchPageByIndex(m_currentPageIndex);

            yield return null;
        }

            // Page currentPage = m_unlockedPageList[m_currentPageIndex];
            // Page newPage = m_unlockedPageList[idxNewPage];

            // m_onBeforeSwitchPageCallback?.Invoke(isNextPage, currentPage, newPage);

            // SwitchCanvas(isNextPage, idxNewPage);

            // m_onAfterCloneCanvasCallback?.Invoke(isNextPage);

            // float delayEnableCurrentPage = isNextPage ? 0 : 0;
            // float delayDisableCurrentPage = isNextPage ? m_durationSwitchPage : m_durationSwitchPage / 2;
            // float delayEnableNewPage = isNextPage ? m_durationSwitchPage / 2 : 0;
            // float delayDisableNewPage = isNextPage ? m_durationSwitchPage : m_durationSwitchPage;

            // if (!isNextPage) StartCoroutine(CoroutineUtils.InvokeOnDelay(delayEnableCurrentPage, () => currentPage.Enable(true)));
            // StartCoroutine(CoroutineUtils.InvokeOnDelay(delayDisableCurrentPage, () => currentPage.Enable(false)));
            // StartCoroutine(CoroutineUtils.InvokeOnDelay(delayEnableNewPage, () => newPage.Enable(true)));
            // if (isNextPage) StartCoroutine(CoroutineUtils.InvokeOnDelay(delayDisableNewPage, () => newPage.Enable(false)));

            // StartCoroutine(CoroutineUtils.InvokeOnDelay(m_durationSwitchPage / 2, () =>
            // {
            //     m_onMiddleSwitchPageCallback?.Invoke(isNextPage, currentPage, newPage);
            // }));

            // StartCoroutine(CoroutineUtils.InvokeOnDelay(m_durationSwitchPage, () =>
            // {
            //     DestroyCanvasCopy();
            //     m_onAfterSwitchPageCallback?.Invoke(isNextPage, currentPage, newPage);
            // }));
        // }

        public static void DisableAllMonoBehaviours(GameObject parent)
        {
            if (parent == null)
            {
                Debug.LogWarning("Parent GameObject is null. Cannot disable MonoBehaviours.");
                return;
            }

            BaseBehaviour[] monoBehaviours = parent.GetComponents<BaseBehaviour>();
            foreach (BaseBehaviour mb in monoBehaviours)
            {
                mb.enabled = false;
            }

            foreach (Transform child in parent.transform)
            {
                DisableAllMonoBehaviours(child.gameObject);
            }
        }

        // private void DestroyCanvasCopy()
        // {
        //     if (m_canvasDuplicated != null)
        //     {
        //         Destroy(m_canvasDuplicated);
        //     }
        // }

        private void SwitchPageByIndex(int index)
        {
            foreach (var page in m_pageList)
            {
                page.Enable(false);
            }

            if (m_currentPageIndex >= m_unlockedPageList.Count)
            {
                Debug.LogWarning("Try to switch to page " + index.ToString() + " which is not unlocked");
                return;
            }

            m_currentPage = m_unlockedPageList[m_currentPageIndex];
            m_currentPage.Enable(true);
        }
    }
}
