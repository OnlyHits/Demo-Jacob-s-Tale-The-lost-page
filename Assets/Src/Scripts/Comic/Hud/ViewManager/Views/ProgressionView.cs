using UnityEngine;
using CustomArchitecture;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Sirenix.Utilities;
using System.Linq;
using DG.Tweening;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class ProgressionView : NavigableView
    {
        [Header("Pages")]
        [SerializeField] private PageGUI m_pageGUIPrefab;
        [SerializeField] private Transform m_grid;
        [SerializeField] private PageGUI m_lastPage;

        [Header("Tweens")]
        [SerializeField] private float m_delayAppearPages = 0.1f;
        [SerializeField] private float m_durationAppearPage = 0.25f;


        #region INTERNAL

        public override void ActiveGraphic(bool active)
        {
            base.ActiveGraphic(active);
        }

        public override void Hide(bool partialy = false, bool instant = false)
        {
            if (instant)
            {
                gameObject.SetActive(false);
                return;
            }
            if (m_panelsData.IsNullOrEmpty()) return;

            PanelData firstPanelData = m_panelsData[0];
            Tween lastTween = null;
            Vector3 from = Vector3.one;
            Vector3 to = Vector3.one * 0.001f;

            for (int i = 0; i < firstPanelData.selectableElements.Count; ++i)
            {
                Transform element = firstPanelData.selectableElements[i].transform;

                element.DOKill();
                element.DOScale(to, m_durationAppearPage).From(from);
            }

            m_lastPage.transform.DOKill();
            lastTween = m_lastPage.transform.DOScale(to, m_durationAppearPage).From(from).SetEase(Ease.InBack).SetDelay(m_delayAppearPages);
            lastTween?.OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
        }

        public override void Show(bool instant = false)
        {
            gameObject.SetActive(true);

            if (m_panelsData.IsNullOrEmpty()) return;

            PanelData firstPanelData = m_panelsData[0];
            Tween lastTween = null;
            Vector3 from = Vector3.one * 0.001f;
            Vector3 to = Vector3.one;
            float delayToAdd = m_delayAppearPages;
            int count = firstPanelData.selectableElements.Count;

            for (int i = 0; i < firstPanelData.selectableElements.Count; ++i)
            {
                Transform element = firstPanelData.selectableElements[i].transform;
                float delay = i * delayToAdd;

                element.DOKill();
                if (instant)
                {
                    element.localScale = to;
                }
                else
                {
                    element.localScale = from;
                    element.DOScale(to, m_durationAppearPage).From(from).SetDelay(delay);
                }

            }

            m_lastPage.transform.DOKill();
            lastTween = m_lastPage.transform.DOScale(to, m_durationAppearPage).From(from).SetEase(Ease.OutBack).SetDelay(count * m_delayAppearPages);
        }

        public override void Init()
        {
            base.Init();

            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToNavigate(OnNavigateInputChanged);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToValidate(OnValidateInput);
            ComicGameCore.Instance.MainGameMode.GetNavigationInput().SubscribeToCancel(OnCanceledInput);

            ComicGameCore.Instance.MainGameMode.SubscribeToLockChapter(OnChapterLocked);
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockChapter(OnChapterUnlocked);

            SpawnPagesUI();
        }

        #endregion INTERNAL


        #region CALLBACKS

        private void OnNavigateInputChanged(InputType inputType, Vector2 value)
        {
            if (!gameObject.activeSelf) return;
            if (isCd) return;
            isCd = true;

            int destIndex = m_currentElementIdx;
            int lastIndex = m_currentPanelData.selectableElements.Count;

            if (value.x > 0 || value.y < 0)
            {
                destIndex = m_currentElementIdx + 1;
            }
            else if (value.x < 0 || value.y > 0)
            {
                destIndex = m_currentElementIdx - 1;
            }

            if (destIndex < 0)
            {
                destIndex = Mathf.Clamp(lastIndex - 1, 0, lastIndex - 1);
            }
            else if (destIndex >= lastIndex)
            {
                destIndex = 0;
            }

            if (TrySetElementByIndex(out m_currentElement, destIndex))
            {
                if (m_debug) Debug.Log("---- > Navigate on element = " + m_currentElement.name);
            }
        }

        private void OnValidateInput(InputType inputType, bool value)
        {
            //ShowPanelByIndex(1);
        }

        private void OnCanceledInput(InputType inputType, bool value)
        {
            //ShowBasePanel();
        }

        private void OnChapterLocked(Chapters typeUnlocked)
        {
            UpdatePagesUI();
        }

        private void OnChapterUnlocked(Chapters typeUnlocked)
        {
            UpdatePagesUI();
        }

        #endregion CALLBACKS

        private void UpdatePagesUI()
        {
            if (m_panelsData.IsNullOrEmpty()) return;

            PanelData firstPanelData = m_panelsData[0];
            Dictionary<Chapters, ChapterConfig> chapterTypeByConfig = ComicGameCore.Instance.MainGameMode.GetGameConfig().m_config;
            int indexPageUI = 0;
            int lastPageIndex = firstPanelData.selectableElements.Count - 1;

            // @note: Iterate from the smallest enum (prequel, to the last enum)
            foreach (Chapters chapterType in chapterTypeByConfig.Keys.OrderBy(i => (int)i))
            {
                bool chapterIsUnlocked = ComicGameCore.Instance.MainGameMode.IsChapterUnlocked(chapterType);
                List<int> pageIndexes = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetPagesByChapter(chapterType);

                foreach (int index in pageIndexes)
                {
                    PageGUI pageGUI = firstPanelData.selectableElements[indexPageUI].GetComponent<PageGUI>();

                    //if (indexPageUI >= lastPageIndex)
                    //{
                    //    pageGUI.SetSpecial();
                    //}
                    //else
                    //{
                    if (chapterIsUnlocked)
                        pageGUI.SetUnlocked();
                    else
                        pageGUI.SetLocked();
                    //}

                    indexPageUI += 1;
                }
            }
        }

        private void SpawnPagesUI()
        {
            if (m_panelsData.IsNullOrEmpty()) return;

            PanelData firstPanelData = m_panelsData[0];
            Dictionary<Chapters, ChapterConfig> chapterTypeByConfig = ComicGameCore.Instance.MainGameMode.GetGameConfig().m_config;

            // @note: Iterate from the smallest enum (prequel, to the last enum)
            foreach (Chapters chapterType in chapterTypeByConfig.Keys.OrderBy(i => (int)i))
            {
                bool chapterIsUnlocked = ComicGameCore.Instance.MainGameMode.IsChapterUnlocked(chapterType);
                List<int> pageIndexes = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetPagesByChapter(chapterType);

                foreach (int index in pageIndexes)
                {
                    PageGUI pageGUI = InstantiatePage(firstPanelData);

                    //m_pages.Add(pageGUI);
                    if (chapterIsUnlocked)
                        pageGUI.SetUnlocked();
                    else
                        pageGUI.SetLocked();
                }
            }

            if (firstPanelData.selectableElements.Count > 0)
            {
                //PageGUI pageGUI = InstantiatePage(firstPanelData);
                //pageGUI.SetSpecial();

                firstPanelData.SetStartingElement(firstPanelData.selectableElements[0]);
            }
        }

        private PageGUI InstantiatePage(PanelData panelToAdd)
        {
            PageGUI pageGUI = Instantiate(m_pageGUIPrefab, m_grid);
            UIBehaviour pageUIBehaviour = pageGUI.Element;

            panelToAdd.selectableElements.Add(pageUIBehaviour);

            return pageGUI;
        }

    }
}
