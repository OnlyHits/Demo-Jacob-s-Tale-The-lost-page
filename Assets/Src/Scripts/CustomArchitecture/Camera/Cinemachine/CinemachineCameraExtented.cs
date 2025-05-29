using Comic;
using Unity.Cinemachine;
using UnityEngine;

namespace CustomArchitecture
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineCameraExtended : BaseBehaviour
    {
        [Header("Focus parameters")]
        [SerializeField] private bool                       m_fitSelf = false;
        [SerializeField, Range(0.1f, 100f)] private float   m_widthFactor = 1f;
        [SerializeField, Range(0.1f, 100f)] private float   m_heightFactor = 1f;

        private CinemachineCamera       m_camera = null;

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

            if (m_fitSelf)
                FitSelfBounds();
        }
        #endregion

        #region Features
        private void Fit(Bounds bounds)
        {
            float panelWidth = bounds.size.x;
            float panelHeight = bounds.size.y;

            float cameraAspect = (float)Screen.width / Screen.height;

            float verticalSize = panelHeight * m_heightFactor / 2f;
            float horizontalSize = panelWidth * m_widthFactor / 2f / cameraAspect;

            float targetOrthographicSize = Mathf.Max(verticalSize, horizontalSize);

            m_camera.Follow = transform;
            m_camera.Lens.OrthographicSize = targetOrthographicSize;
        }

        public void FitSelfBounds()
        {
            Bounds? bounds = gameObject.TryGetBounds();

            if (bounds == null)
            {
                Debug.LogWarning("CinemachineCameraExtended : can't focus self (no bound source)");
                return;
            }

            Fit(bounds.Value);
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