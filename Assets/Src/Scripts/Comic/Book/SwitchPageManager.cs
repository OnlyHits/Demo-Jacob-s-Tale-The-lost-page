using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System.Linq;

namespace Comic
{
    public partial class PageManager : BaseBehaviour
    {
        [SerializeField]                private bool m_forceSwitchPageDebug = false;
        [SerializeField, ReadOnly]      private CorrectionData m_correctionData;

        private struct CorrectionData
        {
            public Vector3  m_correctedPosition;
            public Vector3  m_originalPosition;
            public int      m_indexNewPage;
            public bool     m_isRunning;
        }

        // Internal call. You can call it yourself but NavigationManager should do the job
        public void ChangePageDirty(bool is_next_page)
        {
            int idxNewPage = is_next_page ? m_currentPageIndex + 1 : m_currentPageIndex - 1;

            Vector3 corrected_position = GetCorrectedPlayerPosition(m_unlockedPageList[idxNewPage]);

            if ((!m_forceSwitchPageDebug && m_unlockedPageList[idxNewPage].CanAccessPanel(corrected_position))
                || m_forceSwitchPageDebug)
            {
                NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();
                player.transform.position = corrected_position;

                SwitchPageByIndex(idxNewPage);
            }
        }

        #region Page accesibility

        // Is panel lock.
        // This function is used internaly during screenshot and compute player position
        // on a given frame. Don't use this function outside NavigationManager
        // TODO : make another function that compute current player position
        // TODO : Make this function only accessible to NavigationManager (friend c# equivalent?)
        public bool IsAbleToAccessPage()
        {
            return m_unlockedPageList[m_correctionData.m_indexNewPage].CanAccessPanel(m_correctionData.m_correctedPosition);
        }

        public bool CanChangePage(bool nextPage)
        {
            if (nextPage)
                return m_currentPageIndex + 1 < m_unlockedPageList.Count;
            else
                return m_currentPageIndex - 1 >= 0;
        }
        #endregion

        #region Screenshot Management
        public void OnBeforeScreenshot(bool is_next_page)
        {
            NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

            m_correctionData.m_indexNewPage = is_next_page ? m_currentPageIndex + 1 : m_currentPageIndex - 1;
            m_correctionData.m_correctedPosition = GetCorrectedPlayerPosition(m_unlockedPageList[m_correctionData.m_indexNewPage]);
            m_correctionData.m_originalPosition = player.transform.position;

            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[m_correctionData.m_indexNewPage];

            if (is_next_page)
            {
                player.GetRigidbody().simulated = false;
                player.transform.position = m_correctionData.m_correctedPosition;
            }

            current_page.Pause(true);
            new_page.Pause(true);

            new_page.Enable(is_next_page);
            current_page.Enable(!is_next_page);
        }

        public void OnAfterScreenshot(bool is_next_page)
        {
            NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

            Page current_page = m_unlockedPageList[m_currentPageIndex];
            Page new_page = m_unlockedPageList[m_correctionData.m_indexNewPage];

            if (is_next_page)
                player.transform.position = m_correctionData.m_originalPosition;
            else
                player.transform.position = m_correctionData.m_correctedPosition;

            player.GetRigidbody().simulated = true;

            new_page.Enable(!is_next_page);
            current_page.Enable(is_next_page);
        }

        public void OnTurnSequenceFinish(bool is_next, bool is_error)
        {
            NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

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
        }
        #endregion

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

        #region Player Position Correction
        private Vector3 GetCorrectedPlayerPosition(Page evaluated_page)
        {
            NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

            Vector2 playerPosition = player.transform.position;
            Bounds colliderBounds = player.GetCollider().bounds;
            var bounds = evaluated_page.GetPanelsInnerBound();

            if (bounds == null || player == null)
                return Vector3.zero;

            Bounds innerBound = bounds
                .OrderBy(b => DistanceToBounds(b, colliderBounds))
                .FirstOrDefault();

            Vector2 newPosition = playerPosition;
            newPosition.x = Mathf.Clamp(newPosition.x, innerBound.min.x + colliderBounds.extents.x, innerBound.max.x - colliderBounds.extents.x);
            newPosition.y = Mathf.Clamp(newPosition.y, innerBound.min.y + colliderBounds.extents.y, innerBound.max.y - colliderBounds.extents.y);

            return newPosition;
        }


        private float DistanceToBounds(Bounds spriteBounds, Bounds colliderBounds)
        {
            float dx = Mathf.Max(0, spriteBounds.min.x - colliderBounds.max.x, colliderBounds.min.x - spriteBounds.max.x);
            float dy = Mathf.Max(0, spriteBounds.min.y - colliderBounds.max.y, colliderBounds.min.y - spriteBounds.max.y);

            return dx * dx + dy * dy;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(m_correctionData.m_correctedPosition, ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().GetCollider().bounds.size);
        }

        #endregion
    }
}
