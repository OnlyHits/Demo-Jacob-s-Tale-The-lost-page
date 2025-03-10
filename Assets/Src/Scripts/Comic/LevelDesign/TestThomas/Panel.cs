using UnityEngine;
using CustomArchitecture;
using System;
using DG.Tweening;
using System.Collections.Generic;

namespace Comic
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class Panel : BaseBehaviour
    {
        [SerializeField] private PanelVisual m_panelVisual;
        [SerializeField] private List<Transform> m_allElements;
        [SerializeField] private Transform m_propsContainer;
        // this value must be save
        [SerializeField] private bool m_isLock = false;
        private List<AProps> m_props = null;
        private SpriteRenderer m_margin;
        private Tween m_rotateTween;
        //private List<Tween> m_rotCaseTweens = new List<Tween>();
        private bool m_isRotating = false;
        private Vector3 m_currentRotation = Vector3.zero;

        public bool IsLock() => m_isLock;
        public PanelVisual GetPanelVisual() => m_panelVisual;
        public List<AProps> GetProps() => m_props;


        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                ClampPosition();
#endif

            m_panelVisual.GetHideSprite().enabled = m_isLock;
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            if (parameters.Length != 1
                || parameters[0] is not SpriteRenderer)
                return;

            m_margin = (SpriteRenderer)parameters[0];
            m_panelVisual.Init();

            InitProps();
        }
        #endregion

        public bool ContainPosition(Vector3 position)
        {
            return m_panelVisual.PanelReference().bounds.Contains(position);
        }

        private void InitProps()
        {
            if (m_props != null && m_props.Count != 0)
                m_props.Clear();
            else if (m_props == null)
                m_props = new();

            foreach (Transform child in m_propsContainer)
            {
                AProps component = child.GetComponent<AProps>();

                if (component != null)
                {
                    m_props.Add(component);
                    component.Init();
                }
            }
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
            return m_isRotating;
        }

        public void Rotate180(float speed, Action endRotateCallback)
        {
            m_isRotating = true;

            Vector3 to = new Vector3(0, transform.eulerAngles.y + 180f, 0f);

            m_rotateTween = transform.DORotate(to, speed, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    m_isRotating = false;
                    endRotateCallback?.Invoke();
                    m_rotateTween = null;
                });
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
