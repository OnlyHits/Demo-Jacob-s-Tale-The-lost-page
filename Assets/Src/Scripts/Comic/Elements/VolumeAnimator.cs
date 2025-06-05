using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

namespace Comic
{
    // one of last non-BaseBehaviour
    // i dont find an utility to reference it
    public class VolumeAnimator : MonoBehaviour
    {
        [SerializeField] private VolumeProfile m_volume;
        private Vignette m_vignette = null;
        private float m_minVignetteIntensity = 0.3f;
        private float m_maxVignetteIntensity = 0.5f;

        private void Awake()
        {
            if (m_volume != null && m_volume.TryGet(out m_vignette))
            {
                m_vignette.active = true;
                m_vignette.intensity.overrideState = true;
                m_vignette.intensity.Override(m_minVignetteIntensity);
                m_volume.isDirty = true;
                Debug.Log("Vignette");
            }
            else
                Debug.Log("No vignette");
        }

        public IEnumerator AnimateVignette(bool zoom, float duration)
        {
            StartCoroutine(Animate_vignetteCoroutine(
                zoom ? m_minVignetteIntensity : m_maxVignetteIntensity,
                zoom ? m_maxVignetteIntensity : m_minVignetteIntensity,
                duration));
            yield return null;
        }

        private IEnumerator Animate_vignetteCoroutine(float from, float to, float duration)
        {
            if (m_vignette == null) yield break;

            Debug.Log("from : " + from.ToString() + ", to : "+ to.ToString() + ", duration : "+ duration.ToString());

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                m_vignette.intensity.Override(Mathf.Lerp(from, to, t));

                yield return null;
            }

            m_vignette.intensity.Override(to);
        }
    }
}