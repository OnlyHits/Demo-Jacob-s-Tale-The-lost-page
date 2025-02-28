//using UnityEngine;
//using CustomArchitecture;
//using DG.Tweening;
//using static PageHole;

//namespace Comic
//{
//    public class CanvasVisualManager : BaseBehaviour
//    {
//        [Header("Switch Page Anim")]
//        [SerializeField] private Transform m_destTransform;
//        [SerializeField, ReadOnly] private Vector3 m_baseRot;
//        [SerializeField, ReadOnly] private Vector3 m_destRot;
//        [SerializeField, ReadOnly] private float m_duration = 1f;
//        private Tween m_switchPageTween = null;

//        #region BaseBehaviour
//        protected override void OnFixedUpdate()
//        { }
//        protected override void OnLateUpdate()
//        { }
//        protected override void OnUpdate()
//        { }
//        public override void LateInit(params object[] parameters)
//        { }
//        public override void Init(params object[] parameters)
//        { }
//        #endregion

//        private void Awake()
//        {
//            m_baseRot = transform.position;
//            m_destRot = m_destTransform.eulerAngles;
//        }

//        #region SWITCH PAGE

//        private void OnBeforeSwitchPage(bool nextPage, Page p1, Page p2)
//        {
//            if (!nextPage)
//            {
//                float from = m_destRot.y;
//                float to = m_baseRot.y;
//                TranslateCanvas(from, to);
//            }
//            else if (nextPage)
//            {
//                float from = m_baseRot.y;
//                float to = m_destRot.y;
//                TranslateCanvas(from, to);
//            }
//        }

//        private void OnMiddleSwitchPage(bool nextPage, Page p1, Page p2)
//        {
//        }

//        private void OnAfterSwitchPage(bool nextPage, Page p1, Page p2)
//        {
//        }

//        private void TranslateCanvas(float from, float to)
//        {
//            if (m_switchPageTween != null)
//            {
//                m_switchPageTween.Kill();
//            }

//            float value = from;

//            m_switchPageTween = DOTween.To(() => from, x => value = x, to, m_duration)
//                .SetEase(Ease.Linear)
//                .OnUpdate(() =>
//                {
//                    transform.eulerAngles = new Vector3(0, value, 0);
//                })
//                .OnComplete(() =>
//                {
//                    ResetTransformToBase();
//                });
//        }

//        private void ResetTransformToBase()
//        {
//            transform.eulerAngles = new Vector3(0, m_baseRot.y, 0);
//        }

//        #endregion SWITCH PAGE


//    }
//}
