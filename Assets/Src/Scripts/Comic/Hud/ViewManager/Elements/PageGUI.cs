using CustomArchitecture;
using UnityEngine;
using UnityEngine.UI;

public class PageGUI : BaseBehaviour
{
    public Image Element;
    //[SerializeField] private Image m_pageImage;

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

    public bool IsLocked() => m_isLocked;

    [Space]
    [SerializeField, ReadOnly] private bool m_isLocked = true;
    [SerializeField] private GameObject m_spriteLocked;
    [SerializeField] private GameObject m_spriteUnlocked;
    [SerializeField] private GameObject m_spriteLastPage;

    public void SetUnlocked()
    {
        m_isLocked = false;
        m_spriteUnlocked.SetActive(true);
        m_spriteLocked.SetActive(false);
        m_spriteLastPage.SetActive(false);
    }

    public void SetLocked()
    {
        m_isLocked = true;
        m_spriteUnlocked.SetActive(false);
        m_spriteLocked.SetActive(true);
        m_spriteLastPage.SetActive(false);
    }

    public void SetSpecial()
    {
        m_spriteUnlocked.SetActive(false);
        m_spriteLocked.SetActive(false);
        m_spriteLastPage.SetActive(true);
    }


}
