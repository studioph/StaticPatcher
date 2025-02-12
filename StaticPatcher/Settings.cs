using System.Collections.Frozen;
using Mutagen.Bethesda.WPF.Reflection.Attributes;
using Noggog;
using Serilog;
using Tomlyn;

namespace StaticPatcher;

public class StaticPatcherSettings
{
    [Tooltip("Log additional information during execution. Only turn on if reporting an issue.")]
    public bool VerboseLogging = false;

    public string SettingsFileName = "StaticPatcherSettings.toml";
}

public class Configuration
{
    public Dictionary<LocationType, List<ItemCategory>> Location { get; } = [];

    public static FrozenDictionary<LocationType, FrozenSet<ItemCategory>> Load(
        DirectoryPath? configDir,
        string fileName
    )
    {
        Log.Logger.Information("Loading configuration from {dir}/{file}", configDir, fileName);
        if (configDir is null)
        {
            throw new DirectoryNotFoundException(configDir);
        }
        var settingsPath = Path.Combine(configDir, fileName);

        if (!File.Exists(settingsPath))
        {
            throw new FileNotFoundException(
                $"Could not find configuration file: \"{settingsPath}\". A configuration file must be provided containing which locations and items to patch."
            );
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

        var toml = Toml.ToModel<Configuration>(
            File.ReadAllText(settingsPath),
            settingsPath,
            options: options
        );
        if (!toml.Location.Any())
        {
            Log.Logger.Warning(
                "Configuration file contains no mappings, no records will be patched. Edit {path} to add entries for what to patch.",
                settingsPath
            );
        }
        return toml.Location.ToFrozenDictionary(pair => pair.Key, pair => pair.Value.ToFrozenSet());
    }
}
