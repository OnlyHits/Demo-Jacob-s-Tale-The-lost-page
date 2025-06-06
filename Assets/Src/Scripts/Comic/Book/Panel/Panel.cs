using UnityEngine;
using CustomArchitecture;
using System;
using DG.Tweening;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.DebugUI;

namespace Comic
{
    // not updated. Probably not needed, think about removing it
    public interface IPanelInterface
    {
        public bool ContainPosition(Vector3 position);
        public void Flip(Direction direction);
    }

    [RequireComponent(typeof(PanelVisualData))]
    [RequireComponent(typeof(SortingGroup))]
    public class Panel : Navigable, IPanelInterface
    {
        [SerializeField] private        Transform               m_propsContainer;

        [SerializeField, ReadOnly] private PanelVisualData      m_visualDatas;
        [SerializeField] private        Panel3DBuilder          m_panel3DBuilder;
        [SerializeField] private        Panel2DBuilder          m_panel2DBuilder;
        [SerializeField] private        PanelOutlineBuilder     m_outlineBuilder;

        // this value must be save
        [SerializeField] private bool   m_isLock = false;

        private List<AProps>            m_props = null;
        private SpriteRenderer          m_margin;
        private Tween                   m_rotateTween;
        private bool                    m_isRotating = false;

        [SerializeField] private PanelCinemachineCamera m_cinemachineCamera = null;

        public bool IsLock() => m_isLock;
        public List<AProps> GetProps() => m_props;
        public PanelCinemachineCamera GetCinemachineCamera() => m_cinemachineCamera;
        public Panel3DBuilder GetPanel3DBuilder() => m_panel3DBuilder;
        public Panel2DBuilder GetPanel2DBuilder() => m_panel2DBuilder;
        public PanelOutlineBuilder GetOutlineBuilder() => m_outlineBuilder;

        public PanelVisualData GetVisualData() => m_visualDatas != null ? m_visualDatas : GetComponent<PanelVisualData>();

        #region Bounding Utils
        public Bounds GetInnerPanelBounds() => m_panel2DBuilder.GetInnerPanelBounds(GetGlobalBounds());
        public Bounds GetGroundToCeilBounds() => m_panel2DBuilder.GetGroundToCeilBounds(GetGlobalBounds());
        #endregion Bounding Utils

        #region Navigable
        public override Bounds GetGlobalBounds() => GetVisualData().GetGlobalBounds();
        public override void Focus()
        {
            base.Focus();
            m_outlineBuilder.Focus();
            m_cinemachineCamera.SetPanValue(0f);
        }
        public override void Unfocus()
        {
            base.Unfocus();
            m_outlineBuilder.Unfocus();
            m_cinemachineCamera.SetPanValue(0f);
        }
        #endregion Navigable

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (m_isFocus)
            {
                if (IsPlayerInPanel())
                {
                    m_cinemachineCamera.SetPanValue(GetPlayerDistanceFromCenterAsRatio());
                }
            }
        }
        public override void LateInit(params object[] parameters)
        {
            m_cinemachineCamera.LateInit();
            m_cinemachineCamera.FitBounds(GetGlobalBounds());
        }
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

            m_visualDatas = GetComponent<PanelVisualData>();

            m_cinemachineCamera.Init();

            m_outlineBuilder.Init();
            m_panel2DBuilder.Init();
            m_panel3DBuilder.Init(this);

            m_margin = (SpriteRenderer)parameters[1];

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

        #region Panel Utility
        /// <summary>
        /// Compute the ratio between player distance from panel center
        /// and half panel horizontal size
        /// </summary>
        /// <returns>ratio between -1 and 1</returns>
        public float GetPlayerDistanceFromCenterAsRatio()
        {
            Vector3 player_pos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().transform.position;

            float distance = Mathf.Abs(transform.position.x - player_pos.x);
            float width = GetGlobalBounds().size.x * .5f;
            bool left = player_pos.x < transform.position.x;

            Debug.Log(distance / width);

            return Mathf.Clamp(distance / width, 0f, 1f) * (left ? -1f : 1f);
        }
        public bool ContainPosition(Vector3 position)
        {
            return GetVisualData().GetGlobalBounds().Contain2D((Vector2)position);
        }
        public bool IsPlayerInPanel()
        {
            Vector3 player_pos = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter().transform.position;
            return ContainPosition(player_pos);
        }
        #endregion Panel Utility

        private void InitProps()
        {
            if (m_props != null && m_props.Count != 0)
                m_props.Clear();
            else
                m_props ??= new();

            foreach (Transform child in m_propsContainer)
            {
                if (child.TryGetComponent<AProps>(out var component))
                {
                    m_props.Add(component);
                    component.Init();
                }
            }
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

        #region Editor
#if UNITY_EDITOR
        public void Editor_Build()
        {
            var sorting_group = GetComponent<SortingGroup>();
            sorting_group.sortingLayerName = "Panel";
            sorting_group.sortAtRoot = true;
            sorting_group.sortingOrder = 0;

            // hard lock
            GetVisualData().Ref.transform.localPosition = Vector3.zero;

            m_outlineBuilder.Editor_Build(GetGlobalBounds());
            // Build 2D before 3D. 3D use 2D's datas
            m_panel2DBuilder.Editor_Build(GetGlobalBounds(), GetVisualData());
            m_panel3DBuilder.Editor_Build(GetGroundToCeilBounds(), GetVisualData());

            InitProps();

            foreach (var props in m_props)
                props.Editor_Build(m_panel3DBuilder);
        }
#endif
        #endregion Editor
    }
}
