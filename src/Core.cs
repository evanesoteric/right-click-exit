using MelonLoader;

[assembly: MelonInfo(typeof(RightClickExit.Core), RightClickExit.BuildInfo.Name, RightClickExit.BuildInfo.Version, RightClickExit.BuildInfo.Author, RightClickExit.BuildInfo.DownloadLink)]
[assembly: MelonGame("TVGS", "Schedule I")]
#if IL2CPP
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.IL2CPP)]
#else
[assembly: MelonPlatformDomain(MelonPlatformDomainAttribute.CompatibleDomains.MONO)]
#endif

namespace RightClickExit
{
    /// <summary>
    /// Mod metadata, shared by the MelonInfo attribute and log output.
    /// <c>Version</c> is generated at build time from the csproj &lt;Version&gt;,
    /// so the version number lives in exactly one place.
    /// </summary>
    public static partial class BuildInfo
    {
        /// <summary>Display name.</summary>
        public const string Name = "Right Click Exit";

        /// <summary>Author shown in the MelonLoader console.</summary>
        public const string Author = "Ev";

        /// <summary>Project page.</summary>
        public const string DownloadLink = null;

#if IL2CPP
        /// <summary>Which game branch this build targets.</summary>
        public const string Branch = "IL2CPP (main branch)";
#else
        /// <summary>Which game branch this build targets.</summary>
        public const string Branch = "Mono (alternate branch)";
#endif
    }

    /// <summary>MelonLoader entry point.</summary>
    public sealed class Core : MelonMod
    {
        /// <summary>Logger for the rest of the mod.</summary>
        internal static MelonLogger.Instance Log { get; private set; }

        /// <inheritdoc />
        public override void OnInitializeMelon()
        {
            Log = LoggerInstance;
            Preferences.Initialize();
            Log.Msg($"v{BuildInfo.Version} loaded for {BuildInfo.Branch}. " +
                    (Preferences.Enabled ? "Right-click now closes menus." : "Disabled in MelonPreferences.cfg."));
        }

        /// <inheritdoc />
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            ExitBridge.OnSceneInitialized();
        }

        /// <inheritdoc />
        public override void OnUpdate()
        {
            ExitBridge.Tick();
        }
    }
}
