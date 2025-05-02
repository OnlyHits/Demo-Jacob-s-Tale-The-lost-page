using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;

// todo : what is the use case? May be too precise to be in utils 
[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpritePicker : BaseBehaviour
{
    [SerializeField, ReadOnly] private SpriteRenderer m_spriteRd;
    [SerializeField] private List<Sprite> m_spriteList;

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
    { }
    #endregion

    private void Awake()
    {
        m_spriteRd = GetComponent<SpriteRenderer>();

        if (m_spriteList?.Count <= 0)
        {
            Debug.LogWarning("None sprite assigned in list to randomize & set one in RandomSpritePicker");
            return;
        }

        m_spriteRd.sprite = m_spriteList[Random.Range(0, m_spriteList.Count - 1)];
    }
}
