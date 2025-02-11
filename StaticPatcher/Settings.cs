using System.Collections.Frozen;
using Mutagen.Bethesda.WPF.Reflection.Attributes;
using Noggog;
using Tomlyn;

namespace StaticPatcher;

public class StaticPatcherSettings
{
    [Tooltip("Log additional information during execution. Only turn on if reporting an issue.")]
    public bool VerboseLogging = false;

    public string SettingsFileName = "StaticPatcherSettings.toml";
}

public class TomlSettings
{
    public Dictionary<LocationType, List<ItemCategory>> Location { get; } = [];

    public static FrozenDictionary<LocationType, FrozenSet<ItemCategory>> Load(
        DirectoryPath? extraSettingsPath,
        string fileName
    )
    {
        if (extraSettingsPath is null)
        {
            throw new DirectoryNotFoundException($"Missing ExtraSettingsDataPath argument");
        }
        var settingsPath = Path.Combine(extraSettingsPath, fileName);

        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException($"Could not find settings file: {settingsPath}");
        }
        var options = new TomlModelOptions
        {
            ConvertToModel = (obj, type) =>
            {
                if (obj is string str)
                {
                    if (type == typeof(ItemCategory))
                    {
                        return ItemCategory.FromValue(str.ToLower());
                    }
                    if (type == typeof(LocationType))
                    {
                        return LocationType.FromValue(str.ToLower());
                    }
                }

                return null;
            },
        };

        var toml = Toml.ToModel<TomlSettings>(
            File.ReadAllText(settingsPath),
            settingsPath,
            options: options
        );
        return toml.Location.ToFrozenDictionary(pair => pair.Key, pair => pair.Value.ToFrozenSet());
    }
}
