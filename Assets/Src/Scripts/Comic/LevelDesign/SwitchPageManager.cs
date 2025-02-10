using System;
using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        private Action<bool, Page, Page> m_onBeforeSwitchPageCallback;
        // remove
        private Action<bool, Page, Page> m_onMiddleSwitchPageCallback;
        private Action<bool, Page, Page> m_onAfterSwitchPageCallback;

        #region CALLBACKS

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

        private void SwitchPage(bool is_next_page, int idxNewPage)
        {
            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[idxNewPage];
            //            m_onBeforeSwitchPageCallback?.Invoke(isNextPage, currentPage, newPage);

            if (ComicGameCore.Instance.MainGameMode.GetCameraManager().IsCameraRegister(URP_OverlayCameraType.Camera_Hud))
            {
                current_page.Enable(!is_next_page);
                new_page.Enable(is_next_page);

                StartCoroutine(ComicGameCore.Instance.MainGameMode.GetCameraManager().ScreenAndApplyTexture(is_next_page));

                current_page.Enable(is_next_page);
                new_page.Enable(!is_next_page);

                ComicGameCore.Instance.MainGameMode.GetCameraManager().Test(is_next_page);
            }


            m_currentPageIndex = idxNewPage;
            SwitchPageByIndex(m_currentPageIndex);
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
