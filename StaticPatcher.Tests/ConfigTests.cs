using Noggog;

namespace StaticPatcher.Tests;

public class ConfigTests
{
    private static readonly DirectoryPath _dataFolder = "data";

    [Theory]
    [InlineData("lowercase.toml")]
    [InlineData("camelcase.toml")]
    [InlineData("empty.toml")]
    public void TestLoadConfiguration(string fileName)
    {
        Configuration.Load(_dataFolder, fileName);
    }
}
