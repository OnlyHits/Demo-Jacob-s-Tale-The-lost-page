using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CoroutineRunner");
                instance = obj.AddComponent<CoroutineRunner>();
                DontDestroyOnLoad(obj); // Keeps it alive across scenes
            }
            return instance;
        }
    }

    public void RunCoroutine(IEnumerator coroutine)
    {
        StartCoroutine(coroutine);
    }
}


public static class GamepadUtils
{
    public static void RumbleController(this Gamepad gamepad, float lowFrequency, float highFrequency, float duration = 0.5f)
    {
        Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
        CoroutineRunner.Instance.RunCoroutine(CoroutineUtils.InvokeOnDelay(duration, gamepad.StopRumble));
    }

    public static void StopRumble(this Gamepad gamepad)
    {
        gamepad.ResetHaptics();
    }

    public static void RumbleMicro(this Gamepad gamepad, float duration = 0.5f)
    {
        gamepad.RumbleController(0.1f, 0.1f, duration);
    }

    public static void RumbleLight(this Gamepad gamepad, float duration = 0.5f)
    {
        gamepad.RumbleController(0.3f, 0.3f, duration);
    }

    public static void RumbleMedium(this Gamepad gamepad, float duration = 0.5f)
    {
        gamepad.RumbleController(0.6f, 0.6f, duration);
    }

    public static void RumbleHard(this Gamepad gamepad, float duration = 0.5f)
    {
        gamepad.RumbleController(1.0f, 1.0f, duration);
    }

    public static void RumbleCustom(this Gamepad gamepad, float lowFrequency, float highFrequency, float duration = 0.5f)
    {
        gamepad.RumbleController(lowFrequency, highFrequency, duration);
    }
}