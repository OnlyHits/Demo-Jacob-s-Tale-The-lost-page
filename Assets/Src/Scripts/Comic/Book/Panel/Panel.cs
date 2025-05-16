using UnityEngine;
using CustomArchitecture;
using System;
using DG.Tweening;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public interface PanelInterface
    {
        public bool ContainPosition(Vector3 position);
        public void Flip(Direction direction);
    }

#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class Panel : Navigable, PanelInterface
    {
        [SerializeField] private        PanelVisual m_panelVisual;
        [SerializeField] private        Transform m_propsContainer;
        // this value must be save
        [SerializeField] private bool   m_isLock = false;

        private List<AProps>            m_props = null;
        private SpriteRenderer          m_margin;
        private Tween                   m_rotateTween;
        private bool                    m_isRotating = false;

        public bool IsLock() => m_isLock;
        public PanelVisual GetPanelVisual() => m_panelVisual;
        public List<AProps> GetProps() => m_props;


        #region Navigable
        public override Bounds GetGlobalBounds() => m_panelVisual.PanelReference().bounds;
        public override void Focus() => m_panelVisual.Focus();
        public override void Unfocus() => m_panelVisual.Unfocus();
        #endregion

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
            if (parameters.Length != 2
                || parameters[0] is not List<Navigable>
                || parameters[1] is not SpriteRenderer)
            {
                Debug.LogWarning("Bad parameters");
                return;
            }

            base.Init(parameters[0]);

            m_margin = (SpriteRenderer)parameters[1];
            m_panelVisual.Init();

            InitProps();
        }
        #endregion

        #region PanelBehaviour
        public void Flip(Direction direction)
        {
            if (m_isRotating || direction == Direction.None)
                return;

            Vector3 axis = Vector3.zero;
            float angle = 180f;

            switch (direction)
            {
                case Direction.Left:
                case Direction.Right:
                    axis = Vector3.up; // Y axis
                    break;
                case Direction.Up:
                case Direction.Down:
                    axis = Vector3.right; // X axis
                    break;
                default:
                    Debug.LogWarning("Unsupported flip direction: " + direction);
                    return;
            }

            m_isRotating = true;

            Vector3 to = transform.eulerAngles + axis * angle;

            m_rotateTween = transform.DORotate(to, 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    m_isRotating = false;
                    m_rotateTween = null;
                });
        }

        public void RotateContinuously(Direction direction, float speed = 90f)
        {
            if (direction == Direction.None)
                return;

            Vector3 axis = Vector3.zero;

            switch (direction)
            {
                case Direction.Left:
                    axis = Vector3.up; speed = -Mathf.Abs(speed); break;
                case Direction.Right:
                    axis = Vector3.up; speed = Mathf.Abs(speed); break;
                case Direction.Up:
                    axis = Vector3.right; speed = -Mathf.Abs(speed); break;
                case Direction.Down:
                    axis = Vector3.right; speed = Mathf.Abs(speed); break;
                default:
                    Debug.LogWarning("Unsupported direction: " + direction);
                    return;
            }

            transform.Rotate(axis, speed * Time.deltaTime, Space.World);
        }

        #endregion PanelBehaviour

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

            Vector3 playerPos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().transform.position;
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
