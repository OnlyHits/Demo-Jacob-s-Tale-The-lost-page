using UnityEngine;
using CustomArchitecture;

namespace Comic
{
    [ExecuteAlways]
    public class PanelVisual : BaseBehaviour
    {
        [SerializeField] private SpriteRenderer                     m_referenceSprite;
        [SerializeField] private SpriteRenderer                     m_hideSprite;
        [ReadOnly, SerializeField] private PanelOutlineController   m_outlineController;

        public SpriteRenderer PanelReference() => m_referenceSprite;
        public SpriteRenderer GetHideSprite() => m_hideSprite;
        public void LockPosition() => transform.localPosition = Vector3.zero;
        public void Focus() => m_outlineController.Focus();
        public void Unfocus() => m_outlineController.Unfocus();

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
                m_referenceSprite = GetComponent<SpriteRenderer>();

            m_outlineController = GetComponent<PanelOutlineController>();

            m_outlineController.Init();
        }
        #endregion
    }
}
