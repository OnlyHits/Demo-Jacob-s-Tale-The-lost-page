using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CustomArchitecture;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.Utilities;

namespace Comic
{
    public class ProgressionView : NavigableView
    {
        [Header("Pages")]
        [SerializeField] private PageGUI m_pageGUIPrefab;
        [SerializeField] private Transform m_grid;


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

            foreach (Chapters chapterType in chapterTypeByConfig.Keys)
            {
                bool chapterIsUnlocked = ComicGameCore.Instance.MainGameMode.IsChapterUnlocked(chapterType);
                List<int> pageIndexes = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetPagesByChapter(chapterType);

                foreach (int index in pageIndexes)
                {
                    PageGUI pageGUI = firstPanelData.selectableElements[indexPageUI].GetComponent<PageGUI>();

                    if (indexPageUI >= lastPageIndex)
                    {
                        pageGUI.SetSpecial();
                    }
                    else
                    {
                        if (chapterIsUnlocked)
                            pageGUI.SetUnlocked();
                        else
                            pageGUI.SetLocked();
                    }

                    indexPageUI += 1;
                }
            }
        }

        private void SpawnPagesUI()
        {
            if (m_panelsData.IsNullOrEmpty()) return;

            PanelData firstPanelData = m_panelsData[0];
            Dictionary<Chapters, ChapterConfig> chapterTypeByConfig = ComicGameCore.Instance.MainGameMode.GetGameConfig().m_config;

            foreach (Chapters chapterType in chapterTypeByConfig.Keys)
            {
                bool chapterIsUnlocked = ComicGameCore.Instance.MainGameMode.IsChapterUnlocked(chapterType);
                List<int> pageIndexes = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetPagesByChapter(chapterType);

                foreach (int index in pageIndexes)
                {
                    PageGUI pageGUI = InstantiatePage(firstPanelData);

                    if (chapterIsUnlocked)
                        pageGUI.SetUnlocked();
                    else
                        pageGUI.SetLocked();
                }
            }

            if (firstPanelData.selectableElements.Count > 0)
            {
                PageGUI pageGUI = InstantiatePage(firstPanelData);

                pageGUI.SetSpecial();
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
