using Comic;
using UnityEngine;


[DefaultExecutionOrder(-5)]
public class ForceInitManager : MonoBehaviour
{
#if UNITY_EDITOR

    [SerializeField] private ComicGameCore gameCore;

    //[SerializeField] private bool m_forceInitHUD = false;
    //[SerializeField] private bool m_forceInitGame = false;


    private void Awake()
    {
        gameCore = GetComponent<ComicGameCore>();
    }

    private void Start()
    {
        //if (m_forceInitHUD)
        //{
        //    gameCore.MainGameMode.InitHud();
        //}
        //if (m_forceInitGame)
        //{
        //    gameCore.MainGameMode.InitGame();
        //}

    }

#endif
}