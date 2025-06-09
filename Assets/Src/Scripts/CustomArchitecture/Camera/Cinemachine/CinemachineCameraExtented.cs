    using Comic;
using Unity.Cinemachine;
using UnityEngine;

namespace CustomArchitecture
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineCameraExtended : BaseBehaviour
    {
        [Header("Focus parameters")]
        [SerializeField, Range(0.1f, 100f)] private float   m_widthFactor = 1f;
        [SerializeField, Range(0.1f, 100f)] private float   m_heightFactor = 1f;

        private CinemachineCamera       m_camera = null;

        public float WidthFactor { get { return m_widthFactor; } protected set { } }
        public float HeightFactor { get { return m_widthFactor; } protected set { } }
        public CinemachineCamera Camera
        {
            get { return m_camera; }
            protected set { }
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
            m_camera = GetComponent<CinemachineCamera>();
        }
        #endregion

        #region Features
        private void Fit(Bounds bounds)
        {
            if (m_camera.Lens.Orthographic)
            {
                float panelWidth = bounds.size.x * m_heightFactor;
                float panelHeight = bounds.size.y * m_widthFactor;

                float cameraAspect = (float)Screen.width / Screen.height;

                float verticalSize = panelHeight * .5f;
                float horizontalSize = panelWidth * .5f / cameraAspect;

                float targetOrthographicSize = Mathf.Max(verticalSize, horizontalSize);

                m_camera.Lens.OrthographicSize = targetOrthographicSize;
            }
            else if (m_camera.Lens.IsPhysicalCamera)
            {
                Debug.LogWarning("Fit() doesn't support physical camera mode yet.");
                return;
            }
            else
            {
                float distance = Mathf.Abs(m_camera.transform.localPosition.z * m_camera.transform.lossyScale.z);

                float panelWidth = bounds.size.x * m_widthFactor;
                float panelHeight = bounds.size.y * m_heightFactor;

                float aspect = (float)Screen.width / Screen.height;

                float fovVerticalRad = 2f * Mathf.Atan((panelHeight / 2f) / distance);
                float fovHorizontalRad = 2f * Mathf.Atan((panelWidth / 2f) / distance);

                float fovHorizontalToVerticalRad = 2f * Mathf.Atan(Mathf.Tan(fovHorizontalRad / 2f) / aspect);

                float finalFovRad = Mathf.Max(fovVerticalRad, fovHorizontalToVerticalRad);
                float finalFovDeg = Mathf.Clamp(finalFovRad * Mathf.Rad2Deg, 1f, 179f);

                m_camera.Lens.FieldOfView = finalFovDeg;
            }
        }

        public void FitBounds(Bounds bounds)
        {
            if (bounds.size == Vector3.zero)
            {
                Debug.LogWarning("CinemachineCameraExtended : can't focus empty bounds");
                return;
            }

            Fit(bounds);
        }
        #endregion Features
    }
}