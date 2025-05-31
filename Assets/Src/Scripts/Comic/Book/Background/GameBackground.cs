using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public class GameBackground
    {
        [SerializeField] private GameObject                 m_backgroundVisual;
        [SerializeField] private CinemachineCameraExtended  m_cinemachineCamera = null;
        public CinemachineCameraExtended GetCinemachineCamera() => m_cinemachineCamera;

        public void EnableVisual(bool enable)
        {
            m_backgroundVisual.SetActive(enable);
        }

    }
}