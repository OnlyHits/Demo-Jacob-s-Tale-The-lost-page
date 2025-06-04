using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using static Comic.Panel3DBuilder;

namespace Comic
{
    public class ClampProps : AProps
    {
        [SerializeField] protected PanelPart3D m_roomPlacement = PanelPart3D.Panel_None;

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
            base.Init();
        }
        #endregion

        #region Props Behaviour
        public override void StartBehaviour()
        { }
        public override void StopBehaviour()
        { }
        public override void PauseBehaviour(bool pause)
        { }
        #endregion Props Behaviour

        #region Room placement
#if UNITY_EDITOR
        public override void Editor_Build(Panel3DBuilder builder)
        {
            if (m_root == null || m_spriteRenderer == null || m_roomPlacement == PanelPart3D.Panel_None)
                return;

            var pos = m_root.transform.localPosition;

            switch (m_roomPlacement)
            {
                case PanelPart3D.Panel_Ceil:
                    pos.y = builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.y;
                    break;
                case PanelPart3D.Panel_Ground:
                    pos.y = builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.y;
                    break;
                case PanelPart3D.Panel_LeftWall:
                    pos.x = builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.x;
                    break;
                case PanelPart3D.Panel_RightWall:
                    pos.x = builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.x;
                    break;
                case PanelPart3D.Panel_BackWall:
                    pos.z = builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.z;
                    break;
                default:
                    break;
            }

            m_root.transform.localPosition = pos;
        }
#endif
        #endregion Room placement
    }
}
