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
    }
}
