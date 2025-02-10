using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comic;
using UnityEngine;

public class Shortcuts : MonoBehaviour
{
#if UNITY_EDITOR
    public bool unlock = true;
    public Dictionary<KeyCode, Chapters> keyByChapters = new Dictionary<KeyCode, Chapters>()
    {
        //{ KeyCode.Alpha0, Chapters.The_Prequel },
        { KeyCode.Alpha1, Chapters.The_First_Chapter },
        { KeyCode.Alpha2, Chapters.The_Second_Chapter },
        { KeyCode.Alpha3, Chapters.The_Third_Chapter },
        { KeyCode.Alpha4, Chapters.The_Fourth_Chapter },
    };

    public Dictionary<KeyCode, System.Type> keyByViews = new Dictionary<KeyCode, System.Type>()
    {
        { KeyCode.Alpha6, typeof(DialogueView) },
        { KeyCode.Alpha7, typeof(CreditView) },
        { KeyCode.Alpha8, typeof(ProgressionView) },
        { KeyCode.Alpha9, typeof(PauseView) },
    };

    private void Update()
    {
        CheckInputSave();
        CheckInputViews();

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            ComicGameCore.Instance.MainGameMode.PlayEndGame();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Time.timeScale = 10f;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Time.timeScale = 1f;
        }
        if (Input.GetKeyDown(KeyCode.E))
            ComicGameCore.Instance.MainGameMode.GetGameProgression().ClearGameProgression();
    }

    private void CheckInputViews()
    {
        bool hasComputeKey = false;
        System.Type viewTypeCompute = default;

        foreach (KeyCode key in keyByViews.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                hasComputeKey = true;
                viewTypeCompute = keyByViews[key];
                break;
            }
        }

        if (hasComputeKey)
        {
            var viewManager = ComicGameCore.Instance.MainGameMode.GetViewManager();

            if (viewManager == null) return;

            // Relfection on function and type
            MethodInfo showMethod = viewManager.GetType()
                .GetMethods()
                .FirstOrDefault(m =>
                m.Name == "Show" &&
                m.IsGenericMethodDefinition &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == typeof(bool));

            if (showMethod != null)
            {
                MethodInfo genericShowMethod = showMethod.MakeGenericMethod(viewTypeCompute);
                genericShowMethod.Invoke(viewManager, new object[] { false });
                ComicGameCore.Instance.MainGameMode.GetViewManager().GetCurrentView().ActiveGraphic(true);
            }
        }
    }

    private void CheckInputSave()
    {
        bool hasComputeKey = false;
        Chapters chapterComputed = Chapters.Chapter_None;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            unlock = !unlock;
        }

        foreach (KeyCode key in keyByChapters.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                hasComputeKey = true;
                chapterComputed = keyByChapters[key];
                break;
            }
        }

        if (hasComputeKey)
        {
            if (unlock)
                ComicGameCore.Instance.MainGameMode.UnlockChapter(chapterComputed, true, true);
            else
                ComicGameCore.Instance.MainGameMode.LockChapter(chapterComputed);
        }
    }
#endif
}

