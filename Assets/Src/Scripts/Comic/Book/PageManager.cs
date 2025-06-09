using System.Collections.Generic;
using CustomArchitecture;
using static CustomArchitecture.CustomArchitecture;
using Sirenix.Utilities;
using UnityEngine;
using Comc;
using System.Collections;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        [SerializeField] private List<Page> m_pageList = new List<Page>();
        [SerializeField, ReadOnly] private Page m_currentPage;
        [SerializeField, ReadOnly] private int m_currentPageIndex;
        [SerializeField, ReadOnly] private List<Page> m_unlockedPageList = new List<Page>();
        [SerializeField, Header("Turning Page")] private TurningPageManager m_turnPageMgr;

        public TurningPageManager GetTurningPageManager() { return m_turnPageMgr; }
        //public SpriteRenderer GetPageSpriteRenderer() => m_pageRenderSprite;
        public Page GetCurrentPage() => m_currentPage;
        public Panel GetCurrentPanel() => m_currentPage.GetCurrentPanel();

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            foreach (var page in m_pageList)
            {
                page.LateInit();
            }
        }
        public override void Init(params object[] parameters)
        {
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockChapter(OnUnlockChapter);
            ComicGameCore.Instance.MainGameMode.SubscribeToLockChapter(OnLockChapter);

            foreach (var data in ComicGameCore.Instance.MainGameMode.GetUnlockChaptersData())
            {
                var gameConf = ComicGameCore.Instance.MainGameMode.GetGameConfig();
                var pagesByChapter = gameConf.GetPagesByChapter(data.m_chapterType);

                UnlockPages(pagesByChapter);
            }

            foreach (var page in m_pageList)
            {
                page.gameObject.SetActive(true);
                page.Init();
                page.gameObject.SetActive(false);
            }

            m_turnPageMgr.Init(this);
        }
        #endregion

        #region Turning Page
        public IEnumerator TurnMultiplePages(bool is_next, int page_number, float duration)
        {
            yield return StartCoroutine(m_turnPageMgr.TurnMultiplePagesCoroutine(is_next, page_number, duration));
        }

        public IEnumerator TurnCover(float duration)
        {
            yield return StartCoroutine(m_turnPageMgr.TurnCoverCoroutine(true, duration));
        }

        public IEnumerator TurnPage(bool next_page, float duration)
        {
            yield return StartCoroutine(m_turnPageMgr.TurnPageCoroutine(next_page, duration));
        }

        public IEnumerator TurnPageError(bool next_page, float duration)
        {
            yield return StartCoroutine(m_turnPageMgr.TurnPageErrorCoroutine(next_page, duration));
        }

        #endregion Turning Page

        // tmp
        public Bounds GetValidBounds()
        {
            foreach (var p in m_pageList)
                return p.GetPageSpriteRenderer().bounds;

            return new Bounds();
        }

        public Bounds GetPageBounds()
        {
            return m_currentPage.GetPageSpriteRenderer().sprite.bounds;
        }

        public void SetStartingPage()
        {
            SwitchPageByIndex(m_currentPageIndex);
        }

        public void DisableCurrentPage()
        {
            m_currentPage.Enable(false);
        }

        #region ON LOCK & UNLOCK CHAPTERS
        private void OnUnlockChapter(Chapters chapterUnlocked)
        {
            var gameConf = ComicGameCore.Instance.MainGameMode.GetGameConfig();
            var pagesByChapter = gameConf.GetPagesByChapter(chapterUnlocked);

            UnlockPages(pagesByChapter);
        }

        private void OnLockChapter(Chapters chapterUnlocked)
        {
            var gameConf = ComicGameCore.Instance.MainGameMode.GetGameConfig();
            var pagesByChapter = gameConf.GetPagesByChapter(chapterUnlocked);

            LockPages(pagesByChapter);
        }

        #endregion ON LOCK & UNLOCK CHAPTERS

        #region LOCK & UNLOCK PAGES
        private void LockPages(List<int> pageIndexes)
        {
            if (pageIndexes.IsNullOrEmpty())
            {
                Debug.LogError("Could not get pages indexes because the list is null");
            }
            foreach (int index in pageIndexes)
            {
                if (index >= m_pageList.Count)
                    continue;

                var page = m_pageList[index];
                m_unlockedPageList.Remove(page);

                if (m_currentPageIndex == index)
                {
                    m_currentPageIndex = m_unlockedPageList.Count - 1;
                    SwitchPageByIndex(m_currentPageIndex);
                }
            }
        }

        private void UnlockPages(List<int> pageIndexes)
        {
            if (pageIndexes.IsNullOrEmpty())
            {
                Debug.LogError("Could not get pages indexes because the list is null");
                return;
            }
            foreach (int index in pageIndexes)
            {
                if (index >= m_pageList.Count)
                    continue;

                var page = m_pageList[index];
                m_unlockedPageList.Add(page);
            }
        }

        #endregion LOCK & UNLOCK PAGES
        public Transform GetSpawnPointByPageIndex(int indexPage)
        {
            if (indexPage >= m_pageList.Count)
            {
                Debug.LogWarning("Try to get page index " + indexPage.ToString() + " which does not exist in PageManager");
                return null;
            }
            Page page = m_pageList[indexPage];

            return page.TryGetSpawnPoint();
        }
    }
}