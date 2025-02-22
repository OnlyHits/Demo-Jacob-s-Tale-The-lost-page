using System;
using System.Collections.Generic;
using System.Diagnostics;
using Comic;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace CustomArchitecture
{
    public static class RebindKeyUtils
    {
        public static void RebindKey(this InputAction inputAction, Key newKey)
        {
            inputAction.ApplyBindingOverride("<Keyboard>/" + newKey.ToString().ToLower());
        }

        // @note : useless
        public static void RebindKey(this InputAction inputAction, KeyControl newKey)
        {
            inputAction.ApplyBindingOverride(newKey.path);
        }

        // @note : useless
        public static void RebindKey(this InputAction inputAction, ButtonControl newButton)
        {
            inputAction.ApplyBindingOverride(newButton.path);
        }

        // @note : the one we are using for both keyboard & gamepad
        public static void RebindKey(this InputAction inputAction, InputControl newInput)
        {
            inputAction.ApplyBindingOverride(newInput.path);
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

        public static bool TryGetKeyboardInputPressed(out KeyControl destKeyControl)
        {
            if (Keyboard.current == null)
            {
                destKeyControl = default;
                return false;
            }
            foreach (var keyControl in Keyboard.current.allKeys)
            {
                //excepted the CancelKey or ValidateKey
                if (keyControl.wasPressedThisFrame)
                {
                    destKeyControl = keyControl;
                    return true;
                }
            }
            destKeyControl = default;
            return false;
        }

        public static bool TryGetGamepadInputPressed(out ButtonControl destButtonControl)
        {
            if (Gamepad.current == null)
            {
                destButtonControl = default;
                return false;
            }
            foreach (InputControl control in Gamepad.current.allControls)
            {
                //excepted the CancelKey or ValidateKey
                if (control.IsPressed() && control is ButtonControl buttonControl)
                {
                    destButtonControl = buttonControl;
                    return true;
                }
            }

            destButtonControl = default;
            return false;
        }

        public static string GetActionNameByInputControl(this InputAction inputAction, InputControl inputControl)
        {
            if (inputControl?.device is Keyboard && inputControl is KeyControl keyControl)
            {
                return inputAction.GetActionNameByKeyControl(keyControl);
            }
            if (inputControl?.device is Gamepad && inputControl is ButtonControl buttonControl)
            {
                return inputAction.GetActionNameByButtonControl(buttonControl);
            }

            return null;
        }

        public static string GetActionNameByButtonControl(this InputAction inputAction, ButtonControl buttonControl)
        {
            //return buttonControl.name;
            string res = inputAction.GetBindingDisplayString();
            string[] parts = res.Split("|");

            if (parts.Length <= 0)
            {
                return "error";
            }
            // Warnign of index out of range
            res = parts[1];
            res = res.Trim();
            return res;
        }

        public static string GetActionNameByKeyControl(this InputAction inputAction, KeyControl keyControl)
        {
            //return keyControl.name;
            if (keyControl.name == "space")
                return "space";
            if (keyControl.name == "tab")
                return "tab";

            string res = inputAction.GetBindingDisplayString();
            string[] parts = res.Split("|");

            if (parts.Length <= 0)
            {
                return "error";
            }
            // Warnign of index out of range
            res = parts[0];
            res = res.Trim();
            //if (res == "Left Arrow") return "<-";
            //if (res == "Right Arrow") return "->";
            //if (res == "Down Arrow") return "v";
            //if (res == "Up Arrow") return "^";
            return res;
        }

        public static List<KeyControl> GetKeyboardKeysFromAction(this InputAction inputAction)
        {
            List<KeyControl> keys = new List<KeyControl>();

            foreach (InputBinding binding in inputAction.bindings)
            {
                // Skip for compoiste bindings (exemple : WASD, etc)
                if (binding.isPartOfComposite)
                    continue;

                InputControl control = InputSystem.FindControl(binding.effectivePath);

                if (control?.device is Keyboard)
                {
                    if (control is KeyControl keyControl)
                    {
                        keys.Add(keyControl);
                    }
                }
            }
            return keys;
        }

        public static List<ButtonControl> GetGamepadKeysFromAction(this InputAction inputAction)
        {
            List<ButtonControl> keys = new List<ButtonControl>();

            foreach (InputBinding binding in inputAction.bindings)
            {
                // Skip for compoiste bindings (exemple : WASD, etc)
                if (binding.isPartOfComposite)
                    continue;

                InputControl control = InputSystem.FindControl(binding.effectivePath);

                if (control?.device is Gamepad)
                {
                    //if (control is StickControl stickControl)
                    //if (control is DpadControl stickControl)
                    //if (control is AxisControl stickControl)
                    if (control is ButtonControl buttonControl)
                    {
                        keys.Add(buttonControl);
                    }
                }
            }
            return keys;
        }

        ///////////// 
        // Example : 

        //private InputAction m_inputAction;
        //
        //private void Update()
        //{
        //    // For gamepad
        //    if (RebindKeyUtils.TryGetGamepadInputPressed(out ButtonControl buttonControl))
        //    {
        //        m_inputAction.RebindKey(buttonControl);
        //    }
        //
        //    // For keyboard
        //    if (RebindKeyUtils.TryGetKeyboardInputPressed(out KeyControl keyControl))
        //    {
        //        m_inputAction.RebindKey(keyControl);
        //    }
        //}

        ///////////// 

    }
}
