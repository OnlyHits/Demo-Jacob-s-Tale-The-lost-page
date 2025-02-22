using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace CustomArchitecture
{
    public static class RebindKeyUtils
    {
        public static void RebindKey(this InputAction inputAction, Key newKey)
        {
            inputAction.ApplyBindingOverride("<Keyboard>/" + newKey.ToString().ToLower());
        }

        public static void RebindKey(this InputAction inputAction, KeyControl newKey)
        {
            inputAction.ApplyBindingOverride(newKey.path);
        }

        public static bool TryGetKeyPressed(out Key destKey)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (Key key in System.Enum.GetValues(typeof(Key)))
                {
                    if (Keyboard.current[key].wasPressedThisFrame)
                    {
                        destKey = key;
                        return true;
                    }
                }
            }
            destKey = default;
            return false;
        }

        // @todo : 
        // - Discard [ESCAPE] & [ENTER] keys
        // - Released & Pressed fuctions (arguments or diff functions)

        //prevent the escape bindinng + going back to back panel
        public static bool TryGetKeyPressed(out KeyControl destKeyControl)
        {
            foreach (var keyControl in Keyboard.current.allKeys)
            {
                if (keyControl.wasPressedThisFrame)
                {
                    destKeyControl = keyControl;
                    return true;
                }
            }
            destKeyControl = default;
            return false;
        }

        public static string GetActionName(this InputAction inputAction, KeyControl keyControl)
        {
            if (keyControl.name == "space")
                return "space";

            string res = inputAction.GetBindingDisplayString();
            string[] parts = res.Split("|");

            if (parts.Length <= 0)
            {
                return "error";
            }
            res = parts[0];
            res = res.Trim();

            //if (res == "Left Arrow") return "<-";
            //if (res == "Right Arrow") return "->";
            //if (res == "Down Arrow") return "v";
            //if (res == "Up Arrow") return "^";
            return res;
        }

        public static List<KeyControl> GetKeyBoardKeysFromAction(this InputAction inputAction)
        {
            List<KeyControl> keys = new List<KeyControl>();

            foreach (InputBinding binding in inputAction.bindings)
            {
                // Skip for compoiste bindings (exemple : WASD, etc)
                if (binding.isPartOfComposite)
                    continue;

                InputControl control = InputSystem.FindControl(binding.effectivePath);

                if (control is KeyControl keyControl)
                {
                    keys.Add(keyControl);
                }
            }
            return keys;
        }

        ///////////// 
        // Example : 

        // private InputAction m_inputAction;
        // private void Update()
        // {
        //     if (RebindKeyUtils.TryGetKeyPressed(out KeyControl keyControl))
        //     {
        //         m_inputAction.RebindKey(keyControl);
        //     }
        // }

        ///////////// 

    }
}
