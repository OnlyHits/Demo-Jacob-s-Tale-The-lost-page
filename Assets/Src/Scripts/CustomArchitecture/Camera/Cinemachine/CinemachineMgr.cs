using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Cinemachine;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    [DefaultExecutionOrder((int)CAExecutionOrder.EO_CMCameraMgr)]
    public abstract class CinemachineMgr<T> : BaseBehaviour where T : CinemachineMgr<T>
    {
        private static T m_instance;

        private CinemachineBrain            m_brain;

        // I choose to don't use CinemachineCameraExtended wrapper to keep it straight forward
        [SerializeField, ReadOnly] private List<CinemachineCamera>     m_permanentCameras;
        [SerializeField, ReadOnly] private List<CinemachineCamera>     m_dynamicCameras;

        // Brain parameters
        private CinemachineBlendDefinition  m_cutBlend;
        private CinemachineBlendDefinition  m_smoothBlend;

        private float                       m_blendTimeout = 3f;
        private bool                        m_isLocked = false;

        public CinemachineBlendDefinition CutBlend { get { return m_cutBlend; } }
        public CinemachineBlendDefinition SmoothBlend { get { return m_smoothBlend; } }

        #region Singleton
        // Prevent direct instantiation
        protected CinemachineMgr() { }

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindFirstObjectByType<T>();

                    if (m_instance == null)
                    {
                        GameObject singletonObject = new GameObject("CinemachineManager");
                        m_instance = singletonObject.AddComponent<T>();
                    }
                }
                return m_instance;
            }
            private set
            {
                m_instance = value;
            }
        }
        #endregion

        #region Setup
        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("Already instantiate");
                Destroy(this.gameObject);
                return;
            }

            Instance = (T)this;
            DontDestroyOnLoad(gameObject);

            OnAwake();

            Init();
            LateInit();
        }

        private void OnAwake()
        {
            m_permanentCameras = new List<CinemachineCamera>(30);
            m_dynamicCameras = new List<CinemachineCamera>(30);

            m_cutBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            m_smoothBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

            m_brain = GetComponentInChildren<CinemachineBrain>();

            transform.position = Vector3.zero;
        }
        #endregion Setup

        #region Camera register
        public void ClearPermanentCameras() => m_permanentCameras.Clear();
        public void RegisterPermanentCamera(CinemachineCamera camera) => m_permanentCameras.Add(camera);
        public void UnregisterPermanentCamera(CinemachineCamera camera) => m_permanentCameras.Remove(camera);
        public void ClearDynamicCameras() => m_dynamicCameras.Clear();
        public void RegisterDynamicCamera(CinemachineCamera camera) => m_dynamicCameras.Add(camera);
        public void UnregisterDynamicCamera(CinemachineCamera camera) => m_dynamicCameras.Remove(camera);
        #endregion Camera register

        #region Focus Management
        public IEnumerator FocusCamera(CinemachineCamera target, bool lock_blend = false, CinemachineBlendDefinition blend_def = default)
        {
            //Debug.Log(target.gameObject.name);
            if (m_isLocked)
            {
                Debug.LogWarning("Can't focus camera, CinemachineMgr is locked");
                yield break;
            }

            if (!IsCameraRegistered(target))
            {
                Debug.LogWarning("Camera not registered");
                yield break;
            }

            OnFocusCamera(target);
            m_brain.DefaultBlend = blend_def;

            m_isLocked = lock_blend;

            float elapsed = 0f;
            while (elapsed < m_blendTimeout)
            {
                if (m_brain.ActiveVirtualCamera != null &&
                    m_brain.ActiveVirtualCamera as CinemachineCamera == target)
                {
                    while (m_brain.IsBlending)
                        yield return null;

                    m_isLocked = false;

                    yield break;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            Debug.LogWarning("Focus camera timeout");

            m_isLocked = false;
        }

        private bool IsCameraRegistered(CinemachineCamera camera)
        {
            foreach (var cam in m_dynamicCameras)
                if (cam == camera) return true;
            foreach (var cam in m_permanentCameras)
                if (cam == camera) return true;

            return false;
        }

        private void OnFocusCamera(CinemachineCamera target, int priority = 10)
        {
            foreach (var cam in m_dynamicCameras)
                cam.Priority = (cam == target) ? priority : 0;
            foreach (var cam in m_permanentCameras)
                cam.Priority = (cam == target) ? priority : 0;
        }
        #endregion Focus Management
    }
}