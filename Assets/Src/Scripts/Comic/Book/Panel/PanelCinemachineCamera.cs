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
        [SerializeField] private float m_panLerpSpeed = 5f;
        [SerializeField] private float m_panThreshold = 0f;

        private CinemachinePanTilt m_panTilt;

        public float PanThreshold { get { return m_panThreshold; } protected set { } }

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
            float target = m_angle * factor;

            m_panTilt.PanAxis.Value = Mathf.Lerp(m_panTilt.PanAxis.Value, target, Time.deltaTime * m_panLerpSpeed);
        }
    }
}