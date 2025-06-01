using UnityEngine;
using CustomArchitecture;

namespace Comic
{
    public class PanelVisual : BaseBehaviour
    {
        [SerializeField] private SpriteMask                         m_referenceSprite;
        [SerializeField] private SpriteRenderer                     m_hideSprite;
        [ReadOnly, SerializeField] private PanelOutlineController   m_outlineController;

        public SpriteMask PanelReference() => m_referenceSprite;
        public SpriteRenderer GetHideSprite() => m_hideSprite;
        public void LockPosition() => transform.localPosition = Vector3.zero;
        public void Focus() => m_outlineController.Focus();
        public void Unfocus() => m_outlineController.Unfocus();
        public Bounds GetTransformedBounds() => TransformBounds(m_referenceSprite.bounds, transform);

        private Bounds TransformBounds(Bounds localBounds, Transform transform)
        {
            Vector3 center = transform.TransformPoint(localBounds.center);

            // Transform the extents (half-size), considering lossyScale
            Vector3 extents = localBounds.extents;
            Vector3 axisX = transform.TransformVector(extents.x, 0, 0);
            Vector3 axisY = transform.TransformVector(0, extents.y, 0);
            Vector3 axisZ = transform.TransformVector(0, 0, extents.z);

            // Sum absolute extents to get world-space extents
            Vector3 worldExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z)
            );

            return new Bounds(center, worldExtents * 2);
        }

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
        {
            if (m_referenceSprite == null)
                m_referenceSprite = GetComponent<SpriteMask>();

            m_outlineController = GetComponent<PanelOutlineController>();

            m_outlineController.Init();
        }
        #endregion
    }
}
