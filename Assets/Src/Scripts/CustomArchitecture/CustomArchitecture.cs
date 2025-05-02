using System;

namespace CustomArchitecture
{
    public static class CustomArchitecture
    {
        public enum ControllerType
        {
            NONE,
            KEYBOARD,
            GAMEPAD,
        }

        public enum InputType
        {
            NONE = 0,
            PRESSED = 1,
            COMPUTED = 2,
            RELEASED = 3,
        }

        [Flags]
        public enum Direction : int
        {
            None = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Up = 1 << 2,
            Down = 1 << 3
        }

    }
}
