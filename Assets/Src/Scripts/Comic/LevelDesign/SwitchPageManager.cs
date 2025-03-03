using System;
using System.Collections;
using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;
using Unity.VisualScripting;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        private Action<bool>            m_onBeforeSwitchPageCallback;
        private Action<bool>            m_onAfterSwitchPageCallback;

        [SerializeField]                private bool m_forceSwitchPageDebug = false;
        [SerializeField, ReadOnly]      private CorrectionData m_correctionData;

        private struct CorrectionData
        {
            public Vector3  m_correctedPosition;
            public Vector3  m_originalPosition;
            public int      m_indexNewPage;
            public bool     m_isRunning;
        }

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

        private Vector3 GetCorrectedPlayerPosition(Page evaluated_page)
        {
            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            Vector2 playerPosition = player.transform.position;
            Bounds colliderBounds = player.GetCollider().bounds;
            List<SpriteRenderer> sprites = evaluated_page.GetPanelsSpriteRenderer();

            if (sprites == null || player == null)
                return Vector3.zero;

            SpriteRenderer closestSprite = sprites
                .OrderBy(sprite => DistanceToBounds(sprite.bounds, colliderBounds))
                .FirstOrDefault();

            Bounds spriteBounds = closestSprite.bounds;

            Vector2 newPosition = playerPosition;
            newPosition.x = Mathf.Clamp(newPosition.x, spriteBounds.min.x + colliderBounds.extents.x, spriteBounds.max.x - colliderBounds.extents.x);
            newPosition.y = Mathf.Clamp(newPosition.y, spriteBounds.min.y + colliderBounds.extents.y, spriteBounds.max.y - colliderBounds.extents.y);

            return newPosition;
        }


        private float DistanceToBounds(Bounds spriteBounds, Bounds colliderBounds)
        {
            float dx = Mathf.Max(0, spriteBounds.min.x - colliderBounds.max.x, colliderBounds.min.x - spriteBounds.max.x);
            float dy = Mathf.Max(0, spriteBounds.min.y - colliderBounds.max.y, colliderBounds.min.y - spriteBounds.max.y);

            return dx * dx + dy * dy;
        }

        public void ChangePageDirty(bool is_next_page)
        {
            int idxNewPage = is_next_page ? m_currentPageIndex + 1 : m_currentPageIndex - 1;

            Vector3 corrected_position = GetCorrectedPlayerPosition(m_unlockedPageList[idxNewPage]);

            if ((!m_forceSwitchPageDebug && m_unlockedPageList[idxNewPage].CanAccessPanel(corrected_position))
                || m_forceSwitchPageDebug)
            {
                Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();
                player.transform.position = corrected_position;

                SwitchPageByIndex(idxNewPage);
            }
        }

        public bool IsAbleToAccessPage()
        {
            return m_unlockedPageList[m_correctionData.m_indexNewPage].CanAccessPanel(m_correctionData.m_correctedPosition);
        }

        public void SetupPageForScreenshot(bool is_next_page)
        {
            m_onBeforeSwitchPageCallback?.Invoke(is_next_page);

            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            m_correctionData.m_indexNewPage = is_next_page ? m_currentPageIndex + 1 : m_currentPageIndex - 1;
            m_correctionData.m_correctedPosition = GetCorrectedPlayerPosition(m_unlockedPageList[m_correctionData.m_indexNewPage]);
            m_correctionData.m_originalPosition = player.transform.position;

            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[m_correctionData.m_indexNewPage];

            if (is_next_page)
                player.transform.position = m_correctionData.m_correctedPosition;

            current_page.Pause(true);
            new_page.Pause(true);

            new_page.Enable(is_next_page);
            current_page.Enable(!is_next_page);
        }

        public void RestorePageAfterScreenshot(bool is_next_page)
        {
            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[m_correctionData.m_indexNewPage];

            if (is_next_page)
                player.transform.position = m_correctionData.m_originalPosition;
            else
                player.transform.position = m_correctionData.m_correctedPosition;

            new_page.Enable(!is_next_page);
            current_page.Enable(is_next_page);
        }

        public void OnPageChangeEnd(bool is_next, bool is_error)
        {
            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[m_correctionData.m_indexNewPage];

            if (is_error)
            {
                current_page.Enable(true);
                new_page.Enable(false);

                player.transform.position = m_correctionData.m_originalPosition;
            }
            else
            {
                SwitchPageByIndex(m_correctionData.m_indexNewPage);
                player.transform.position = m_correctionData.m_correctedPosition;
            }

            current_page.Pause(false);
            new_page.Pause(false);
            m_onAfterSwitchPageCallback?.Invoke(is_next);
        }

        public bool CanChangePage(bool nextPage)
        {
            if (nextPage)
                return m_currentPageIndex + 1 < m_unlockedPageList.Count;
            else
                return m_currentPageIndex - 1 >= 0;
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
            if (m_currentPageIndex >= m_unlockedPageList.Count)
            {
                Debug.LogWarning("Try to switch to page " + index.ToString() + " which is not unlocked");
                return;
            }

            m_unlockedPageList[m_currentPageIndex].DisablePage();
            m_currentPageIndex = index;
            m_unlockedPageList[m_currentPageIndex].EnablePage();

            m_currentPage = m_unlockedPageList[m_currentPageIndex];
        }
    }
}
