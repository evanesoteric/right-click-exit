using MelonLoader;

namespace RightClickExit
{
    /// <summary>
    /// User settings, stored by MelonLoader in <c>UserData/MelonPreferences.cfg</c>
    /// under the <c>[RightClickExit]</c> section. Values can be edited in that file
    /// (with the game closed) or live with an in-game preferences manager mod.
    /// </summary>
    internal static class Preferences
    {
        private const string CategoryId = "RightClickExit";

        private static MelonPreferences_Entry<bool> _enabled;
        private static MelonPreferences_Entry<bool> _closeTasks;
        private static MelonPreferences_Entry<bool> _verboseLogging;

        /// <summary>Master switch. When false the mod does nothing.</summary>
        internal static bool Enabled => _enabled?.Value ?? true;

        /// <summary>
        /// When true, right-click also cancels station tasks (mixing, packaging,
        /// chemistry, cauldron, etc.) the way Escape does. Off by default because
        /// many of those tasks use right-click to grab several items at once.
        /// </summary>
        internal static bool CloseTasks => _closeTasks?.Value ?? false;

        /// <summary>Log each promoted right-click to the MelonLoader console.</summary>
        internal static bool VerboseLogging => _verboseLogging?.Value ?? false;

        /// <summary>Creates (or loads) the preference entries. Call once at startup.</summary>
        internal static void Initialize()
        {
            MelonPreferences_Category category = MelonPreferences.CreateCategory(CategoryId, "Right Click Exit");

            _enabled = category.CreateEntry(
                "Enabled", true, "Enabled",
                "Right-click closes menus, dialogue and other screens the same way Escape does.");

            _closeTasks = category.CreateEntry(
                "CloseTasks", false, "Close station tasks",
                "Also let right-click cancel station tasks (mixing, packaging, chemistry, cauldron...). " +
                "Off by default because those tasks use right-click to grab several items at once.");

            _verboseLogging = category.CreateEntry(
                "VerboseLogging", false, "Verbose logging",
                "Write a console line every time a right-click is turned into an exit. Useful for bug reports.");
        }
    }
}
