using System;
using System.Linq;
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

        #region REBIND_KEYS

        public static void RebindKey(this InputAction inputAction, Key newKey)
        {
            inputAction.ApplyBindingOverride("<Keyboard>/" + newKey.ToString().ToLower());
        }

        public static void RebindKey(this InputAction inputAction, InputControl newInput)
        {
            if (newInput == null)
                return;

            // @note: this call erease all bindings to write this one 
            // if there was 2 bindings like gamepad & controller, it will erease both
            //inputAction.ApplyBindingOverride(newInput.path);

            if (newInput.device is Keyboard)
                inputAction.RebindKeyFromKeyboard(newInput);
            if (newInput.device is Gamepad)
                inputAction.RebindKeyFromGamepad(newInput);
        }

        public static void RebindKeyFromKeyboard(this InputAction inputAction, InputControl newInput)
        {
            if (newInput == null)
                return;

            int bindedIndex = GetBindingIndex(inputAction, "<Keyboard>");

            if (bindedIndex >= 0)
            {
                inputAction.ApplyBindingOverride(bindedIndex, newInput.path);
            }
        }

        public static void RebindKeyFromGamepad(this InputAction inputAction, InputControl newInput)
        {
            if (newInput == null)
                return;

            int bindedIndex = GetBindingIndex(inputAction, "<Gamepad>");

            if (bindedIndex >= 0)
            {
                inputAction.ApplyBindingOverride(bindedIndex, newInput.path);
            }
        }

        private static int GetBindingIndex(InputAction inputAction, string deviceLayout)
        {
            for (int i = 0; i < inputAction.bindings.Count; i++)
            {
                if (inputAction.bindings[i].path.Contains(deviceLayout))
                {
                    return i; // Returns the first match index
                }
            }
            return inputAction.bindings.Count; // Not found so returns the last index + 1
        }

        #endregion REBIND_KEYS


        #region TRYGET_KEY_PRESSED

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

        #endregion TRYGET_KEY_PRESSED


        #region GETTERS & SETTERS

        public static string GetActionNameByInputControl(this InputAction inputAction, InputControl inputControl)
        {
            if (inputControl == null)
                return null;

            if (inputControl.device is Keyboard)
                return inputAction.GetActionNameForKeyboard(inputControl);
            if (inputControl.device is Gamepad)
                return inputAction.GetActionNameForGamepad(inputControl);

            return null;
        }

        public static string GetActionNameForGamepad(this InputAction inputAction, InputControl inputControl)
        {
            //return inputControl.name;
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

        public static string GetActionNameForKeyboard(this InputAction inputAction, InputControl inputControl)
        {
            //return inputControl.name;
            if (inputControl.name == "space")
                return "space";
            if (inputControl.name == "tab")
                return "tab";
            if (inputControl.name == "enter")
                return "enter";
            if (inputControl.name == "escape")
                return "escape";

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

        #endregion GETTERS & SETTERS


        #region EXAMPLES

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

        #endregion EXAMPLES

    }
}
