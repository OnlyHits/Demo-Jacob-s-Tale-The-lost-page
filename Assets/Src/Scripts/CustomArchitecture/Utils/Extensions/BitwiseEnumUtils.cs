using System;

namespace CustomArchitecture
{
    public static class BitwiseEnumUtils
    {
        // Adds a flag to the enum
        public static T AddFlag<T>(this T current, T flag) where T : Enum
        {
            return (T)(object)((Convert.ToInt32(current) | Convert.ToInt32(flag)));
        }

        // Removes a flag from the enum
        public static T RemoveFlag<T>(this T current, T flag) where T : Enum
        {
            return (T)(object)((Convert.ToInt32(current) & ~Convert.ToInt32(flag)));
        }

        // Checks if a flag is set
        public static bool HasFlag<T>(this T current, T flag) where T : Enum
        {
            return (Convert.ToInt32(current) & Convert.ToInt32(flag)) == Convert.ToInt32(flag);
        }

        // Toggles a flag (adds if missing, removes if present)
        public static T ToggleFlag<T>(this T current, T flag) where T : Enum
        {
            return (T)(object)((Convert.ToInt32(current) ^ Convert.ToInt32(flag)));
        }
    }
}