using System.Collections.Generic;
using CustomArchitecture;
using DG.Tweening;
using UnityEngine;
using static Comic.Comic;

namespace Comic
{
    public class PageVisualManager : BaseBehaviour
    {
        // [Header("Switch Page Anim")]
        // [SerializeField] private Transform m_destTransform;
        // [SerializeField, ReadOnly] private Quaternion m_destRotQuat;
        // [SerializeField, ReadOnly] private float m_duration = 1f;
        // private Tween m_switchPageTween = null;

        [Header("Animation Hole")]
        [SerializeField] private PageHole m_holePrefab;
        [SerializeField, ReadOnly] private PageHole m_hole;

        [Header("Page Visuals")]
        public GameObject m_bgBookVisual;
        public GameObject m_coverPage;
        public GameObject m_endPage;
        // [SerializeField, ReadOnly] private List<PageVisual> m_pageVisuals = new List<PageVisual>();

        // private void Awake()
        // {
        // }

        // Now is init into PageManager.Init()
        // private void Start()
        // {
        //     Init();
        // }

        public void Init()
        {
            ComicGameCore.Instance.MainGameMode.SubscribeToBeforeSwitchPage(OnBeforeSwitchPage);
            ComicGameCore.Instance.MainGameMode.SubscribeToAfterSwitchPage(OnAfterSwitchPage);

            // m_duration = ComicGameCore.Instance.MainGameMode.GetPageManager().GetSwitchPageDuration();

            // m_destRotQuat = m_destTransform.rotation;

            // var pages = GetComponentsInChildren<PageVisual>(true);
            // m_pageVisuals.AddRange(pages);
        }


        #region SWITCH PAGE

        private void OnBeforeSwitchPage(bool nextPage, Page currentPage, Page newPage)
        {
            /*
            if (nextPage == true)
            {
                Quaternion from = m_destRotQuat;
                Quaternion to = currentPage.GetBaseVisualRot();
                TranslatePage(from, to, newPage);
                newPage.gameObject.GetComponent<PageVisual>().PushFront();
            }
            else if (nextPage == false)
            {
                Quaternion from = currentPage.GetBaseVisualRot();
                Quaternion to = m_destRotQuat;
                TranslatePage(from, to, currentPage);
                currentPage.gameObject.GetComponent<PageVisual>().PushFront();

                ComicGameCore.Instance.GetGameMode<MainGameMode>().GetPlayer().EnableShadowVisual(true);
            }
            */
        }

        private void OnAfterSwitchPage(bool nextPage, Page currentPage, Page newPage)
        {
            /*
            float shadowAnimCanel = 0.5f;
            Player player = ComicGameCore.Instance.GetGameMode<MainGameMode>().GetPlayer();

            if (nextPage == true)
            {
                float firstHoleFrameDuration = m_holePrefab.GetDuration() / m_holePrefab.GetNbFrames();
                float firstFramDelay = firstHoleFrameDuration + shadowAnimCanel;
                float totalDurtation = m_holePrefab.GetDuration() + shadowAnimCanel;

                newPage.gameObject.GetComponent<PageVisual>().ResetDefault();
                player.EnableShadowVisual(true);

                //check can turn page or not
                StartCoroutine(CoroutineUtils.InvokeOnDelay(shadowAnimCanel, () =>
                {
                    InstantiateHole(newPage);
                    player.EnableVisual(false);
                    player.EnableShadowVisual(false);
                }));
                StartCoroutine(CoroutineUtils.InvokeOnDelay(firstFramDelay, () =>
                {
                    player.EnableVisual(true);
                }));
                StartCoroutine(CoroutineUtils.InvokeOnDelay(totalDurtation, () =>
                {
                    if (player.IsInWall())
                    {
                        Debug.Log("CANCEL SWITCH PAGE !!!");
                        ComicGameCore.Instance.GetGameMode<MainGameMode>().GetPageManager().TryPrevPage();
                    }
                    else
                    {
                        ComicGameCore.Instance.GetGameMode<MainGameMode>().GetCharacterManager().PauseAllCharacters(false);
                    }
                }));
            }
            else if (nextPage == false)
            {
                currentPage.gameObject.GetComponent<PageVisual>().ResetDefault();
                StartCoroutine(CoroutineUtils.InvokeOnDelay(shadowAnimCanel, () =>
                {
                    if (player.IsInWall())
                    {
                        Debug.Log("CANCEL SWITCH PAGE !!!");
                        ComicGameCore.Instance.GetGameMode<MainGameMode>().GetPageManager().TryNextPage();
                    }
                    else
                    {
                        player.EnableShadowVisual(false);
                        ComicGameCore.Instance.GetGameMode<MainGameMode>().GetCharacterManager().PauseAllCharacters(false);
                    }
                }));
            }
            */
        }

        private void InstantiateHole(Page page, float delayPlay = 0f)
        {
            Vector3 playerPos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().transform.position;

            m_hole = Instantiate(m_holePrefab, page.transform);
            m_hole.Init();
            m_hole.Setup(playerPos, frontLayerId, 0.5f);
            m_hole.Play(delayPlay);
        }

        // private void TranslatePage(Quaternion from, Quaternion to, Page page)
        // {
        //     if (m_switchPageTween != null)
        //     {
        //         m_switchPageTween.Kill();
        //     }

        //     if (Quaternion.Dot(from, to) < 0)
        //     {
        //         to = new Quaternion(-to.x, -to.y, -to.z, -to.w);
        //     }

        //     m_switchPageTween = page.GetVisualTransform().DORotateQuaternion(to, m_duration)
        //         .From(from)
        //         .SetEase(Ease.Linear)
        //         .OnComplete(() =>
        //         {
        //             ResetTransformToBase(page);
        //         });
        // }

        // private void ResetTransformToBase(Page page)
        // {
        //     page.ResetBaseVisualRot();
        // }

        #endregion SWITCH PAGE

    }

}
