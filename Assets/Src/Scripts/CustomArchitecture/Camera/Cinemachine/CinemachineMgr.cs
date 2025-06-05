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
        public enum CameraList { All_Cameras, Managed_Cameras }

        private static T m_instance;

        private CinemachineBrain            m_brain;

        // I choose to don't use CinemachineCameraExtended wrapper to keep it straight forward
        private List<CinemachineCamera>     m_managedCameras;
        private CinemachineCamera[]         m_allCameras;

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
            m_managedCameras = new List<CinemachineCamera>(30);

            m_cutBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            m_smoothBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

            m_brain = GetComponentInChildren<CinemachineBrain>();

            transform.position = Vector3.zero;

            RefreshTracking();
        }

        public void RefreshTracking()
        {
            if (m_allCameras != null && m_allCameras.Length > 0)
            {
                m_allCameras = null;
            }

            m_allCameras = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        }
        #endregion Setup

        #region Camera register
        public void ClearManagedCameras() => m_managedCameras.Clear();
        public void RegisterCamera(CinemachineCamera camera) => m_managedCameras.Add(camera);
        public void UnregisterCamera(CinemachineCamera camera) => m_managedCameras.Remove(camera);
        #endregion Camera register

        #region Focus Management
        public IEnumerator FocusCamera(CinemachineCamera target, CameraList list = CameraList.Managed_Cameras, bool lock_blend = false, CinemachineBlendDefinition blend_def = default)
        {
            if (m_isLocked)
                yield break;

            if (!IsCameraRegistered(target, list))
            {
                Debug.LogWarning("Camera not registered");
                yield break;
            }

            OnFocusCamera(target, list);
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

        private bool IsCameraRegistered(CinemachineCamera camera, CameraList list)
        {
            if (list == CameraList.Managed_Cameras)
            {
                foreach (var cam in m_managedCameras)
                    if (cam == camera) return true;
            }
            else if (list == CameraList.All_Cameras)
            {
                foreach (var cam in m_allCameras)
                    if (cam == camera) return true;
            }
            return false;
        }

        private void OnFocusCamera(CinemachineCamera target, CameraList list, int priority = 10)
        {
            if (list == CameraList.Managed_Cameras)
            {
                foreach (var cam in m_managedCameras)
                    cam.Priority = (cam == target) ? priority : 0;
            }
            else if (list == CameraList.All_Cameras)
            {
                foreach (var cam in m_allCameras)
                    cam.Priority = (cam == target) ? priority : 0;
            }
        }
        #endregion Focus Management
    }
}