using CustomArchitecture;
using Unity.Cinemachine;
using UnityEngine;
using Comic;
using static PageHole;

public class ShakeCam : BaseBehaviour
{
    public static ShakeCam Inst { get; private set; }
    [SerializeField, ReadOnly] private CinemachineCamera m_cam;
    [SerializeField, ReadOnly] private CinemachineBasicMultiChannelPerlin m_multChanPerlin;
    private float m_shakeTimer;

    #region BaseBehaviour
    protected override void OnFixedUpdate()
    { }
    protected override void OnLateUpdate()
    { }
    protected override void OnUpdate()
    {
        if (m_shakeTimer > 0)
        {
            m_shakeTimer -= Time.deltaTime;

            if (m_shakeTimer <= 0f)
            {
                m_multChanPerlin.AmplitudeGain = 0f;
            }
        }
    }
    public override void LateInit(params object[] parameters)
    { }
    public override void Init(params object[] parameters)
    {
    }
    #endregion

    private void Awake()
    {
        Inst = this;

        m_cam = GetComponentInChildren<CinemachineCamera>();
        m_multChanPerlin = m_cam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
    }

    public void StopShake()
    {
        m_multChanPerlin.AmplitudeGain = 0f;
    }

    public void LoopShake(float intensity, bool state)
    {
        m_multChanPerlin.AmplitudeGain = state ? intensity : 0f;
    }

    public void Shake(float intensity, float time)
    {
        m_multChanPerlin.AmplitudeGain = intensity;
        m_shakeTimer = time;
    }
}