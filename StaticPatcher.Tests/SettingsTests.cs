using Noggog;

namespace StaticPatcher.Tests;

public class SettingsTests
{
    private static readonly DirectoryPath _dataFolder = "data";

    [Theory]
    [InlineData("test.toml")]
    // [InlineData("flat-keys.toml")]
    // [InlineData("table.toml")]
    public void TestTableFormat(string fileName)
    {
        TomlSettings.Load(_dataFolder, fileName);
    }
}
