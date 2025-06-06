using Comic;
using Unity.Cinemachine;
using UnityEngine;
using CustomArchitecture;

namespace Comic
{
    [RequireComponent(typeof(CinemachinePanTilt))]
    public class PanelCinemachineCamera : CinemachineCameraExtended
    {
        [Header("Pan tilt")]
        [SerializeField, Range(0, 180f)] private float m_angle;
        private CinemachinePanTilt m_panTilt;

        #region BaseBehaviour
        public override void Init(params object[] parameters)
        {
            base.Init();

            m_panTilt = GetComponent<CinemachinePanTilt>();
            m_panTilt.PanAxis.Range = new Vector2(-m_angle, m_angle);
        }
        #endregion BaseBehaviour

        public void SetPanValue(float factor)
        {
            factor = Mathf.Clamp(factor, -1f, 1f);
            m_panTilt.PanAxis.Value = m_angle * factor;
        }
    }
}