using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Rendering.Universal;
using System;
using CustomArchitecture;
using Unity.Cinemachine;

namespace Comic
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    [RequireComponent(typeof(CinemachineBrain))]
    public class Comic_CinemachineCamera : BaseBehaviour
    {
        CinemachineBlendDefinition      m_cutBlend;
        CinemachineBlendDefinition      m_smoothBlend;
        private CinemachineBrain        m_brain;
        private Camera                  m_camera;

        public CinemachineBrain GetBrain() => m_brain;
        public Camera GetCamera() => m_camera;
        public void UseSmoothBlend() => m_brain.DefaultBlend = m_smoothBlend;
        public void UseCut() => m_brain.DefaultBlend = m_cutBlend;

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
            m_cutBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);
            m_smoothBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);

            m_brain = GetComponent<CinemachineBrain>();
            m_camera = GetComponent<Camera>();

            UseSmoothBlend();

            if (m_brain == null || m_camera == null)
            {
                Debug.LogError("Comic_CinemachineCamera : error on init");
                return;
            }
        }
    #endregion
}
}
