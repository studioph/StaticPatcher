using FluentAssertions;
using Mutagen.Bethesda.Skyrim;

namespace StaticPatcher.Tests;

public class CategoryTests
{
    public static object[][] SubcategoryTestData =
    [
        [ItemCategory.Armor, ItemCategory.Armor, true],
        [ItemCategory.Silverware, ItemCategory.Clutter, true],
        [ItemCategory.Food, ItemCategory.Clutter, false],
        [LocationType.NordicRuin, LocationType.Dungeon, true],
        [LocationType.Farm, LocationType.House, false],
    ];

    [Theory]
    [MemberData(nameof(SubcategoryTestData))]
    public void TestSubCategories<TMajorGetter>(
        CategoryBase<TMajorGetter> source,
        CategoryBase<TMajorGetter> target,
        bool expected
    )
        where TMajorGetter : class, ISkyrimMajorRecordGetter =>
        source.IsEqualOrChildOf(target).Should().Be(expected);
}
