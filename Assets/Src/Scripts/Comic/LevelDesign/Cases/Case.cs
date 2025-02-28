using System;
using System.Collections.Generic;
using CustomArchitecture;
using DG.Tweening;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public class Case : BaseBehaviour
    {
        [SerializeField] private List<Transform> m_allElements;
        [SerializeField] private Transform m_elements;
        [SerializeField] private SpriteRenderer m_caseSprite;

        // do we want to buffer rotation?
        private List<Tween> m_rotCaseTweens = new List<Tween>();
        private bool m_isRotating = false;
        private Vector3 m_currentRotation = Vector3.zero;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        { }
        #endregion


        public bool IsPlayerInCase()
        {
            bool isPlayerIn = false;
            bool isInWidth = false;
            bool isInHeight = false;

            Vector3 playerPos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().transform.position;
            Vector3 casePos = m_caseSprite.transform.position;

            float width = m_caseSprite.bounds.size.x / 2f;
            float height = m_caseSprite.bounds.size.y / 2f;

            if (casePos.x - width < playerPos.x && playerPos.x < casePos.x + width)
            {
                isInWidth = true;
            }

            if (casePos.y - height < playerPos.y && playerPos.y < casePos.y + height)
            {
                isInHeight = true;
            }

            isPlayerIn = isInHeight && isInWidth;

            return isPlayerIn;
        }

        public bool ContainPosition(Vector3 position)
        {
            return m_caseSprite.bounds.Contains(position);
        }

        public bool IsRotating()
        {
            return m_isRotating;
        }

        // depreciated
        public void Rotate180(float speed, Action endRotateCallback)
        {
            if (m_rotCaseTweens.Count > 0)
                return;

            m_isRotating = true;

            Vector3 destRot = m_currentRotation + new Vector3(0, 0, 180);
            m_currentRotation += new Vector3(0, 0, 180);

            foreach (Transform t in m_allElements)
            {
                Tween tween = t.DOLocalRotate(destRot, 0.5f);
                tween.OnComplete(() =>
                    {
                        if (m_rotCaseTweens.Contains(tween))
                        {
                            m_rotCaseTweens.Remove(tween);
                            m_isRotating = false;
                            endRotateCallback?.Invoke();
                        }
                        tween = null;
                    });
                m_rotCaseTweens.Add(tween);
            }
        }

        public Transform GetCaseTransform()
        {
            return m_elements.transform;
        }
    }
}
