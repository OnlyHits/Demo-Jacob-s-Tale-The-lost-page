using System;
using System.Collections;
using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        private Action<bool> m_onBeforeSwitchPageCallback;
        private Action<bool> m_onAfterSwitchPageCallback;

        private Coroutine m_switchPageCoroutine;
        [SerializeField] private bool m_skipSwitchPageAnimation = false;
        private bool m_hasFinishTurning = false;

        #region CALLBACKS

        public void SubscribeToBeforeSwitchPage(Action<bool> function)
        {
            m_onBeforeSwitchPageCallback -= function;
            m_onBeforeSwitchPageCallback += function;
        }

        public void SubscribeToAfterSwitchPage(Action<bool> function)
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
                m_onBeforeSwitchPageCallback?.Invoke(is_next_page);
                m_currentPageIndex = idxNewPage;
                SwitchPageByIndex(m_currentPageIndex);
                m_onAfterSwitchPageCallback?.Invoke(is_next_page);
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
            m_onBeforeSwitchPageCallback?.Invoke(is_next_page);

            if (ComicGameCore.Instance.MainGameMode.GetCameraManager().IsCameraRegister(URP_OverlayCameraType.Camera_Hud))
            {
                Page current_page = m_unlockedPageList[m_currentPageIndex];
                Page new_page = m_unlockedPageList[idxNewPage];

                current_page.Enable(!is_next_page);
                new_page.Enable(is_next_page);

                yield return StartCoroutine(ComicGameCore.Instance.MainGameMode.GetCameraManager().ScreenAndApplyTexture(is_next_page));

                current_page.Enable(is_next_page);
                new_page.Enable(!is_next_page);

                ComicGameCore.Instance.MainGameMode.GetCameraManager().TurnPage(is_next_page);

                yield return new WaitUntil(() => m_hasFinishTurning == true);

                m_hasFinishTurning = false;
            }

            m_switchPageCoroutine = null;
            m_currentPageIndex = idxNewPage;
            SwitchPageByIndex(m_currentPageIndex);

            m_onAfterSwitchPageCallback?.Invoke(is_next_page);

            yield return null;
        }

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
