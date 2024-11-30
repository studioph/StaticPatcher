using Mutagen.Bethesda.WPF.Reflection.Attributes;

namespace StaticPatcher;

public class StaticPatcherSettings
{
    [Tooltip("Log additional information during execution. Only turn on if reporting an issue.")]
    public bool VerboseLogging = false;

    public static readonly string SettingsFileName = "StaticPatcherSettings.toml";

    private static readonly string Include = "include";
    private static readonly string Exclude = "exclude";
}
