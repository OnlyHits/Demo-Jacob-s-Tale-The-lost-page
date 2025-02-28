using System.Collections.Generic;
using CustomArchitecture;
using Sirenix.Utilities;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        [SerializeField] private List<Page> m_pageList = new List<Page>();
        [SerializeField, ReadOnly] private Page m_currentPage;
        [SerializeField, ReadOnly] private int m_currentPageIndex;
        [SerializeField, ReadOnly] private List<Page> m_unlockedPageList = new List<Page>();
//        [SerializeField] private PageVisualManager m_pageVisual;
        [SerializeField] private float m_durationStartGame = 5f;
//        [SerializeField] private float m_durationEndGame = 10f;

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

            ComicGameCore.Instance.MainGameMode.GetHudManager().RegisterToEndTurning(() => m_hasFinishTurning = true);
        }
        public override void Init(params object[] parameters)
        {
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockChapter(OnUnlockChapter);
            ComicGameCore.Instance.MainGameMode.SubscribeToLockChapter(OnLockChapter);
            ComicGameCore.Instance.MainGameMode.SubscribeToEndGame(OnEndGame);

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

//            m_pageVisual.Init();

            SwitchPageByIndex(m_currentPageIndex);

            // #if UNITY_EDITOR
            // #else
            //             OnStartGame();
            // #endif
        }
        #endregion

        #region START & END GAME

        // not the right place to do that, put that in maingamecore
        private void OnStartGame()
        {
            ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().Pause(true);
            ComicGameCore.Instance.MainGameMode.GetViewManager().Pause(true);

            foreach (var page in m_pageList) page.gameObject.SetActive(false);
            // m_pageVisual.m_coverPage.SetActive(true);
            // m_pageVisual.m_bgBookVisual.SetActive(false);
            // m_pageVisual.m_endPage.SetActive(false);

            StartCoroutine(CoroutineUtils.InvokeOnDelay(m_durationStartGame, () =>
            {
                ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().Pause(false);
                ComicGameCore.Instance.MainGameMode.GetViewManager().Pause(false);

                // foreach (var page in m_pageList) page.gameObject.SetActive(true);
                // m_pageVisual.m_coverPage.SetActive(false);
                // m_pageVisual.m_bgBookVisual.SetActive(true);
                // m_pageVisual.m_endPage.SetActive(false);
            }));

        }

        private void OnEndGame()
        {
            // foreach (var page in m_pageList) page.gameObject.SetActive(false);
            // m_pageVisual.m_bgBookVisual.SetActive(true);
            // m_pageVisual.m_endPage.SetActive(true);
            // m_pageVisual.m_coverPage.SetActive(false);

            // StartCoroutine(CoroutineUtils.InvokeOnDelay(m_durationEndGame, () =>
            // {
            //     ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().Pause(false);
            //     ComicGameCore.Instance.MainGameMode.GetViewManager().Pause(false);

            //     foreach (var page in m_pageList) page.gameObject.SetActive(true);
            //     m_pageVisual.m_bgBookVisual.SetActive(true);
            //     m_pageVisual.m_endPage.SetActive(false);
            //     m_pageVisual.m_coverPage.SetActive(false);
            // }));
        }

        #endregion START & END GAME

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

        #region TRY NEXT & PREV PAGE
        public bool TryNextPage()
        {
            int nextIdx = m_currentPageIndex + 1;
            if (nextIdx >= m_unlockedPageList.Count)
            {
                return false;
            }
            TryNextPageInternal(true, nextIdx);
            return true;
        }

        public bool TryPrevPage()
        {
            int prevIdx = m_currentPageIndex - 1;
            if (prevIdx < 0)
            {
                return false;
            }

            TryNextPageInternal(false, prevIdx);
            return true;
        }

        #endregion TRY NEXT & PREV PAGE

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