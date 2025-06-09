using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using static Comic.Panel3DBuilder;

namespace Comic
{
    public class ClampProps : AProps
    {
        [SerializeField] protected PanelPart3D m_roomPlacement = PanelPart3D.Panel_None;
        [SerializeField, ReadOnly] private Panel3DBuilder m_builder = null;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && m_builder != null)
            {
                // tkt ya du lourd qui arrives
            }
#endif
        }
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

            m_builder = builder;
            var pos = m_root.transform.localPosition;
            Quaternion rotation = Quaternion.identity;

            switch (m_roomPlacement)
            {
                case PanelPart3D.Panel_Ceil:
                    pos.y = m_builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.y;
                    break;
                case PanelPart3D.Panel_Ground:
                    pos.y = m_builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.y;
                    break;
                case PanelPart3D.Panel_LeftWall:
                    pos.x = m_builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.x;
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    break;
                case PanelPart3D.Panel_RightWall:
                    pos.x = m_builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.x;
                    rotation = Quaternion.Euler(0f, -90f, 0f);
                    break;
                case PanelPart3D.Panel_BackWall:
                    pos.z = m_builder.Editor_GetPanel3DPart(m_roomPlacement).transform.localPosition.z;
                    break;
                default:
                    break;
            }

            m_root.transform.rotation = rotation;
            m_root.transform.localPosition = pos;
        }
#endif
        #endregion Room placement
    }
}
