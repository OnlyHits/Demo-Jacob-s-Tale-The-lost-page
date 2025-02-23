using System;
using System.Collections;
using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering;

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

            //player.transform.position = newPosition;
        }


        private float DistanceToBounds(Bounds spriteBounds, Bounds colliderBounds)
        {
            float dx = Mathf.Max(0, spriteBounds.min.x - colliderBounds.max.x, colliderBounds.min.x - spriteBounds.max.x);
            float dy = Mathf.Max(0, spriteBounds.min.y - colliderBounds.max.y, colliderBounds.min.y - spriteBounds.max.y);

            return dx * dx + dy * dy;
        }

        private void TryNextPageInternal(bool is_next_page, int idxNewPage)
        {
            if (m_switchPageCoroutine != null)
                return;

            Vector3 corrected_position = GetCorrectedPlayerPosition(m_unlockedPageList[idxNewPage]);

            if (m_unlockedPageList[idxNewPage].CanAccessPanel(corrected_position))
            {
                m_switchPageCoroutine = StartCoroutine(SwitchPageCoroutine(is_next_page, idxNewPage, corrected_position));
            }
            else
            {
                m_switchPageCoroutine = StartCoroutine(SwitchPageErrorCoroutine(is_next_page, idxNewPage, corrected_position));
            }
        }

        private IEnumerator SwitchPageErrorCoroutine(bool is_next_page, int idxNewPage, Vector3 corrected_position)
        {
            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();
            Vector3 player_position = player.transform.position;

            m_onBeforeSwitchPageCallback?.Invoke(is_next_page);

            if (ComicGameCore.Instance.MainGameMode.GetCameraManager().IsCameraRegister(URP_OverlayCameraType.Camera_Hud))
            {
                Page current_page = m_unlockedPageList[m_currentPageIndex];
                Page new_page = m_unlockedPageList[idxNewPage];

                if (is_next_page)
                    player.transform.position = corrected_position;

                current_page.Pause(true);
                new_page.Pause(true);

                if (is_next_page)
                {
                    new_page.Enable(true);
                    current_page.Enable(false);
                }
                else
                {
                    current_page.Enable(true);
                    new_page.Enable(false);
                }

                yield return StartCoroutine(ComicGameCore.Instance.MainGameMode.GetCameraManager().ScreenAndApplyTexture(is_next_page));

                if (is_next_page)
                    player.transform.position = player_position;
                else
                    player.transform.position = corrected_position;

                current_page.Enable(!is_next_page);
                new_page.Enable(is_next_page);

                if (is_next_page)
                {
                    new_page.Enable(false);
                    current_page.Enable(true);
                }
                else
                {
                    current_page.Enable(false);
                    new_page.Enable(true);
                }

                ComicGameCore.Instance.MainGameMode.GetCameraManager().TurnPageError(is_next_page);

                yield return new WaitUntil(() => m_hasFinishTurning == true);

                current_page.Enable(true);
                new_page.Enable(false);

                m_hasFinishTurning = false;

                current_page.Pause(false);
                new_page.Pause(false);
            }

            m_switchPageCoroutine = null;

            player.transform.position = player_position;
            m_onAfterSwitchPageCallback?.Invoke(is_next_page);

            yield return null;
        }

        private IEnumerator SwitchPageCoroutine(bool is_next_page, int idxNewPage, Vector3 corrected_position)
        {
            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();
            Vector3 player_position = player.transform.position;

            m_onBeforeSwitchPageCallback?.Invoke(is_next_page);

            if (ComicGameCore.Instance.MainGameMode.GetCameraManager().IsCameraRegister(URP_OverlayCameraType.Camera_Hud))
            {
                Page current_page = m_unlockedPageList[m_currentPageIndex];
                Page new_page = m_unlockedPageList[idxNewPage];

                if (is_next_page)
                    player.transform.position = corrected_position;

                current_page.Pause(true);
                new_page.Pause(true);

                current_page.Enable(!is_next_page);
                new_page.Enable(is_next_page);

                yield return StartCoroutine(ComicGameCore.Instance.MainGameMode.GetCameraManager().ScreenAndApplyTexture(is_next_page));

                if (is_next_page)
                    player.transform.position = player_position;
                else
                    player.transform.position = corrected_position;

                current_page.Enable(is_next_page);
                new_page.Enable(!is_next_page);

                ComicGameCore.Instance.MainGameMode.GetCameraManager().TurnPage(is_next_page);

                yield return new WaitUntil(() => m_hasFinishTurning == true);

                m_hasFinishTurning = false;

                current_page.Pause(false);
                new_page.Pause(false);
            }

            m_switchPageCoroutine = null;
            SwitchPageByIndex(idxNewPage);
            player.transform.position = corrected_position;

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
