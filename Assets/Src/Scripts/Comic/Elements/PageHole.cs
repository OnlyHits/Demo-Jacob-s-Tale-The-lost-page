using System;
using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static Comic.Comic;

public class PageHole : BaseBehaviour
{
    [Serializable]
    public struct AnimHoleElements
    {
        public List<Sprite> m_spriteFrames;
    }

    [SerializeField] private List<AnimHoleElements> m_animHoleElements;
    [SerializeField, ReadOnly] private float m_duration = 1f;
    private AnimHoleElements m_animHole;
    private SpriteRenderer m_spriteRd;

    public float GetDuration() => m_duration;
    public int GetNbFrames() => m_animHoleElements[0].m_spriteFrames.Count;

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
        m_spriteRd = GetComponentInChildren<SpriteRenderer>();

        if (m_animHoleElements?.Count <= 0)
        {
            Debug.LogWarning("None hole anim elements filled in PageHole");
            return;
        }

        m_animHole = m_animHoleElements[UnityEngine.Random.Range(0, m_animHoleElements.Count - 1)];
    }
    #endregion

    public void Setup(Vector3 spawnPos, int sortingLayerID, float totalDurationAnim)
    {
        transform.position = spawnPos;
        m_spriteRd.sortingLayerID = sortingLayerID;
        m_duration = totalDurationAnim;
    }

    public void Play(float startDelay = 0f)
    {
        float delay = startDelay;
        float delayToAdd = m_duration / m_animHole.m_spriteFrames.Count;

        m_spriteRd.sprite = m_animHole.m_spriteFrames[0];

        for (int i = 0; i < m_animHole.m_spriteFrames.Count; ++i)
        {
            int indexSpriteFrame = i;
            StartCoroutine(CoroutineUtils.InvokeOnDelay(delay, () =>
            {
                if (indexSpriteFrame > 0)
                {
                    m_spriteRd.sortingLayerID = defaultLayerId;
                }
                m_spriteRd.sprite = m_animHole.m_spriteFrames[indexSpriteFrame];
            }));
            delay += delayToAdd;
        }

        StartCoroutine(CoroutineUtils.InvokeOnDelay(m_duration, () =>
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }));
    }
}
