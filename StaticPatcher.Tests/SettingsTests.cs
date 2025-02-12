using Noggog;

namespace StaticPatcher.Tests;

public class SettingsTests
{
    private static readonly DirectoryPath _dataFolder = "data";

    [Theory]
    [InlineData("lowercase.toml")]
    [InlineData("camelcase.toml")]
    [InlineData("empty.toml")]
    public void TestTableFormat(string fileName)
    {
        Configuration.Load(_dataFolder, fileName);
    }
}
