using UnityEngine;
using CustomArchitecture;
using System;
using DG.Tweening;
using System.Collections.Generic;

namespace Comic
{
    [ExecuteAlways]
    public class Panel : BaseBehaviour
    {
        [SerializeField] private PanelVisual m_panelVisual;
        [SerializeField] private List<Transform> m_allElements;
        private SpriteRenderer m_margin;
        private List<Tween> m_rotCaseTweens = new List<Tween>();
        private bool m_isRotating = false;
        private Vector3 m_currentRotation = Vector3.zero;

        public PanelVisual GetPanelVisual() => m_panelVisual;

        public void Init(SpriteRenderer margin)
        {
            m_margin = margin;
            m_panelVisual.Init();
        }

        protected override void OnUpdate(float elapsed_time)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                ClampPosition();
#endif
        }

        // could be only in unity editor, will see later if needed
        private void ClampPosition()
        {
            if (m_margin == null || m_panelVisual.PanelReference() == null) return;

            Bounds margin_bounds = m_margin.bounds;
            Bounds panel_bounds = m_panelVisual.PanelReference().bounds;

            Vector3 position = transform.position;

            position.x = Mathf.Clamp(position.x,
                margin_bounds.min.x + (panel_bounds.size.x / 2),
                margin_bounds.max.x - (panel_bounds.size.x / 2));

            position.y = Mathf.Clamp(position.y,
                margin_bounds.min.y + (panel_bounds.size.y / 2),
                margin_bounds.max.y - (panel_bounds.size.y / 2));

            transform.position = position;
            m_panelVisual.LockPosition();
        }

        public bool IsRotating()
        {
            return false;
        }

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

        public bool IsPlayerInCase()
        {
            SpriteRenderer sprite = m_panelVisual.PanelReference();

            if (sprite == null)
            {
                Debug.LogWarning("Could not check if player in case, no panel visual sprite set");
                return false;
            }

            bool isPlayerIn = false;
            bool isInWidth = false;
            bool isInHeight = false;

            Vector3 playerPos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer().transform.position;
            Vector3 casePos = sprite.transform.position;

            float width = sprite.bounds.size.x / 2f;
            float height = sprite.bounds.size.y / 2f;

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
    }
}
