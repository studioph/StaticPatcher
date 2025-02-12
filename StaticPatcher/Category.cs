using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Ardalis.SmartEnum;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace StaticPatcher;

/// <summary>
/// Base class for enum-like categories
/// </summary>
/// <typeparam name="TMajorGetter"></typeparam>
/// <param name="name"></param>
/// <param name="keywords"></param>
/// <param name="members"></param>
/// <param name="nameHints"></param>
public abstract class CategoryBase<TMajorGetter>(
    string name,
    IEnumerable<IFormLinkGetter<IKeywordGetter>>? keywords = null,
    IEnumerable<IFormLinkGetter<TMajorGetter>>? members = null,
    IEnumerable<string>? nameHints = null
) : SmartEnum<CategoryBase<TMajorGetter>, string>(name, name.ToLower())
    where TMajorGetter : class, ISkyrimMajorRecordGetter
{
    /// <summary>
    /// Keywords are the most generic method of classifying a record.
    /// However, they are sometimes either incorrect or imprecise, necessitating more specific methods below.
    /// Keywords should be used as much as possible to avoid more work to manually classify records using one of the below options.
    /// </summary>
    public readonly FrozenSet<IFormLinkGetter<TMajorGetter>> Members =
        members?.ToFrozenSet() ?? FrozenSet<IFormLinkGetter<TMajorGetter>>.Empty;

    /// <summary>
    /// If keywords are incorrect or too generic, specific records can be listed to override the keyword classification
    /// </summary>
    public readonly FrozenSet<IFormLinkGetter<IKeywordGetter>> Keywords =
        keywords?.ToFrozenSet() ?? FrozenSet<IFormLinkGetter<IKeywordGetter>>.Empty;

    /// <summary>
    /// If all else fails, look for specific substrings in a record's name (does it have the word "cup" in the name, etc)
    /// </summary>
    public readonly Regex? NamePattern =
        (nameHints is not null && nameHints.Any())
            ? new(
                $@"\b({string.Join('|', nameHints)})\b",
                RegexOptions.IgnoreCase | RegexOptions.Compiled
            )
            : null;

    // Stores the original name list to facilitate composite categories
    private readonly ImmutableArray<string> _nameHints = nameHints?.ToImmutableArray() ?? [];

    /// <summary>
    /// Constructs a composite category consisting of all the properties of the member categories.
    /// </summary>
    /// <param name="name">The name of the category</param>
    /// <param name="subCategories"></param>
    public CategoryBase(string name, params CategoryBase<TMajorGetter>[] subCategories)
        : this(
            name,
            keywords: subCategories.SelectMany(sub => sub.Keywords),
            members: subCategories.SelectMany(sub => sub.Members),
            nameHints: subCategories.SelectMany(sub => sub._nameHints)
        ) { }
}

/// <summary>
/// Enum-like class for classifying placed objects
/// </summary>
public sealed class ItemCategory : CategoryBase<IPlaceableObjectGetter>
{
    public ItemCategory(
        string name,
        IEnumerable<IFormLinkGetter<IKeywordGetter>>? keywords = null,
        IEnumerable<IFormLinkGetter<IPlaceableObjectGetter>>? items = null,
        IEnumerable<string>? nameHints = null
    )
        : base(name, keywords, items, nameHints) { }

    /// <inheritdoc>
    public ItemCategory(string name, params ItemCategory[] subCategories)
        : base(name, subCategories) { }

    #region Categories
    #region Single categories

    public static readonly ItemCategory Alchemy =
        new(nameof(Alchemy), new[] { Skyrim.Keyword.VendorItemIngredient });

    public static readonly ItemCategory Ammo =
        new(
            nameof(Ammo),
            new[] { Skyrim.Keyword.VendorItemArrow, Skyrim.Keyword.WeapTypeBoundArrow }
        );

    public static readonly ItemCategory Armor =
        new(nameof(Armor), new[] { Skyrim.Keyword.VendorItemArmor });

    public static readonly ItemCategory Bone =
        new(
            nameof(Bone),
            items:
            [
                Skyrim.MiscItem.BoneHumanSkullFull,
                Skyrim.MiscItem.BoneTrollSkull01,
                Skyrim.MoveableStatic.BoneDeerSkull,
                Skyrim.MoveableStatic.BoneDeerSkullHorned,
                Skyrim.MoveableStatic.BoneHumanArmL,
                Skyrim.MoveableStatic.BoneHumanArmR,
                Skyrim.MoveableStatic.BoneHumanBloodyArm,
                Skyrim.MoveableStatic.BoneHumanBloodyBone,
                Skyrim.MoveableStatic.BoneHumanBloodyHand,
                Skyrim.MoveableStatic.BoneHumanBloodyRibcage,
                Skyrim.MoveableStatic.BoneHumanBloodyLeg,
                Skyrim.MoveableStatic.BoneHumanBloodyShoulder,
                Skyrim.MoveableStatic.BoneHumanBloodySkull,
                Skyrim.MoveableStatic.BoneHumanFootL,
                Skyrim.MoveableStatic.BoneHumanFootR,
                Skyrim.MoveableStatic.BoneHumanSkull,
                Skyrim.MoveableStatic.BoneHumanHandL,
                Skyrim.MoveableStatic.BoneHumanHandR,
                Skyrim.MoveableStatic.BoneHumanLegL,
                Skyrim.MoveableStatic.BoneHumanLegR,
                Skyrim.MoveableStatic.BoneHumanRibcage,
                Skyrim.MoveableStatic.BoneHumanRibcageFull,
                Skyrim.MoveableStatic.BoneHumanSpine,
                Dawnguard.MiscItem.sc_SkeletonHorseHead,
                Dawnguard.MiscItem.sc_SkeletonHorseLeg,
                Dawnguard.MiscItem.sc_SkeletonHorseLeg00,
                Dragonborn.MiscItem.DLC2dunKarstaagSkullItem,
                Dragonborn.MiscItem.DLC2dunKarstaagSkullItemNoName,
                Dragonborn.MiscItem.DLC2dunKolbjornSkull,
            ],
            nameHints: ["bone", "skull"]
        );

    public static readonly ItemCategory Book =
        new(
            nameof(Book),
            [Skyrim.Keyword.VendorItemBook, Skyrim.Keyword.VendorItemRecipe],
            items:
            [
                Skyrim.MiscItem.BurnedBook01,
                Skyrim.MiscItem.RuinedBook,
                Skyrim.MiscItem.RuinedBook02
            ]
        );

    public static readonly ItemCategory BuildingMaterial =
        new(
            nameof(BuildingMaterial),
            [HearthFires.Keyword.BYOHHouseCraftingCategorySmithing],
            items:
            [
                HearthFires.MiscItem.BYOHMaterialClay,
                HearthFires.MiscItem.BYOHMaterialStraw,
                HearthFires.MiscItem.BYOHMaterialGlass
            ]
        );

    public static readonly ItemCategory Clothing =
        new(
            nameof(Clothing),
            new[] { Skyrim.Keyword.VendorItemClothing, Skyrim.Keyword.ArmorClothing }
        );

    public static readonly ItemCategory Clutter =
        new(nameof(Clutter), new[] { Skyrim.Keyword.VendorItemClutter });

    public static readonly ItemCategory Food =
        new(
            nameof(Food),
            new[] { Skyrim.Keyword.VendorItemFood, Skyrim.Keyword.VendorItemFoodRaw }
        );

    public static readonly ItemCategory Gem =
        new(
            nameof(Gem),
            [Skyrim.Keyword.VendorItemGem],
            items:
            [
                Skyrim.MiscItem.GemAmethyst,
                Skyrim.MiscItem.GemAmethystFlawless,
                Skyrim.MiscItem.GemDiamond,
                Skyrim.MiscItem.GemDiamondFlawless,
                Skyrim.MiscItem.GemEmerald,
                Skyrim.MiscItem.GemEmeraldFlawless,
                Skyrim.MiscItem.GemGarnet,
                Skyrim.MiscItem.GemGarnetFlawless,
                Skyrim.MiscItem.GemRuby,
                Skyrim.MiscItem.gemRubyFlawless,
                Skyrim.MiscItem.GemSapphire,
                Skyrim.MiscItem.GemSapphireFlawless,
                Dragonborn.MiscItem.DLC2TGGemSapphire
            ]
        );

    public static readonly ItemCategory Gold =
        new(
            nameof(Gold),
            items:
            [
                Skyrim.MiscItem.Gold001,
                Skyrim.Flora.CoinPurseSmall,
                Skyrim.Flora.CoinPurseMedium,
                Skyrim.Flora.CoinPurseLarge,
            ]
        );

    // Ores and ingots share a single keyword so to split them up items need to be explicitly listed
    public static readonly ItemCategory Ingot =
        new(
            nameof(Ingot),
            items:
            [
                Skyrim.MiscItem.IngotCorundum,
                Skyrim.MiscItem.IngotDwarven,
                Skyrim.MiscItem.IngotEbony,
                Skyrim.MiscItem.IngotGold,
                Skyrim.MiscItem.IngotIMoonstone,
                Skyrim.MiscItem.IngotIron,
                Skyrim.MiscItem.IngotMalachite,
                Skyrim.MiscItem.IngotOrichalcum,
                Skyrim.MiscItem.IngotQuicksilver,
                Skyrim.MiscItem.ingotSilver,
                Skyrim.MiscItem.IngotSteel
            ],
            nameHints: ["ingot"]
        );

    public static readonly ItemCategory Jewelry =
        new(
            nameof(Jewelry),
            new[] { Skyrim.Keyword.ArmorJewelry, Skyrim.Keyword.VendorItemJewelry }
        );

    public static readonly ItemCategory Key =
        new(nameof(Key), new[] { Skyrim.Keyword.VendorItemKey });

    public static readonly ItemCategory Ore =
        new(
            nameof(Ore),
            items:
            [
                Skyrim.MiscItem.OreCorundum,
                Skyrim.MiscItem.OreEbony,
                Skyrim.MiscItem.OreGold,
                Skyrim.MiscItem.OreIron,
                Skyrim.MiscItem.OreMalachite,
                Skyrim.MiscItem.OreMalachite,
                Skyrim.MiscItem.OreMoonstone,
                Skyrim.MiscItem.OreOrichalcum,
                Skyrim.MiscItem.OreOrichalcum,
                Skyrim.MiscItem.OreQuicksilver,
                Skyrim.MiscItem.OreSilver,
                Dragonborn.MiscItem.DLC2OreStalhrim,
                Dragonborn.MiscItem.DLC2TT2HeartStone
            ],
            nameHints: ["ore"]
        );

    public static readonly ItemCategory Pelt =
        new(nameof(Pelt), [Skyrim.Keyword.VendorItemAnimalHide], nameHints: ["pelt"]);

    public static readonly ItemCategory Potion =
        new(
            nameof(Potion),
            [Skyrim.Keyword.VendorItemPotion, Skyrim.Keyword.VendorItemPoison],
            nameHints: ["potion", "poison"]
        );

    public static readonly ItemCategory Scrap =
        new(
            nameof(Scrap),
            items:
            [
                Skyrim.MiscItem.DwarvenCog,
                Skyrim.MiscItem.DwarvenGear,
                Skyrim.MiscItem.DwarvenCenturionDynamo,
                Skyrim.MiscItem.DwarvenGyro,
                Skyrim.MiscItem.DwarvenLargeScrap,
                Skyrim.MiscItem.DwarvenLargeScrap2,
                Skyrim.MiscItem.DwarvenLargeScrap3,
                Skyrim.MiscItem.DwarvenScrapBent,
                Skyrim.MiscItem.DwarvenScrapLever,
                Skyrim.MiscItem.DwarvenScrapLever02,
                Skyrim.MiscItem.DwarvenScrapMetal,
                Skyrim.MiscItem.DwarvenPlateMetalLarge,
                Skyrim.MiscItem.DwarvenPlateMetalSmall,
            ]
        );

    public static readonly ItemCategory Scroll =
        new(nameof(Scroll), new[] { Skyrim.Keyword.VendorItemScroll });

    public static readonly ItemCategory Silverware =
        new(
            nameof(Silverware),
            items:
            [
                Dawnguard.MiscItem.DLC01TankardBloody01,
                Dawnguard.MiscItem.DLC1SilverGobletBlood01,
                Dawnguard.MiscItem.DLC1SilverGobletBlood02,
                Dawnguard.MiscItem.DLC1SilverJugBlood01,
                Dawnguard.MiscItem.DLC1VampireChalice,
                Dawnguard.MiscItem.DLC1VampireChalicewithBlood,
                Dawnguard.MiscItem.DLC1WineBottle03Empty,
                Dawnguard.MiscItem.DLC1WineBottle04Empty,
                Skyrim.MiscItem.BasicFork01,
                Skyrim.MiscItem.BasicKnife01,
                Skyrim.MiscItem.BasicPlate01,
                Skyrim.MiscItem.BasicPlate02,
                Skyrim.MiscItem.BasicTankard01,
                Skyrim.MiscItem.BasicWoodenBowl01,
                Skyrim.MiscItem.BasicWoodenPlate01,
                Skyrim.MiscItem.CastIronPotMedium01,
                Skyrim.MiscItem.CastIronPotSmall01,
                Skyrim.MiscItem.DwarvenBowl01,
                Skyrim.MiscItem.DwarvenBowl02,
                Skyrim.MiscItem.DwarvenBowl03,
                Skyrim.MiscItem.DwarvenFork,
                Skyrim.MiscItem.DwarvenHighBowl01,
                Skyrim.MiscItem.DwarvenHighBowl02,
                Skyrim.MiscItem.DwarvenHighCup01,
                Skyrim.MiscItem.DwarvenHighCup02,
                Skyrim.MiscItem.DwarvenHighCup03,
                Skyrim.MiscItem.DwarvenHighPlate01,
                Skyrim.MiscItem.DwarvenHighPot01,
                Skyrim.MiscItem.DwarvenKnife,
                Skyrim.MiscItem.DwarvenSpoon,
                Skyrim.MiscItem.Flagon,
                Skyrim.MiscItem.Glazed02Jug01,
                Skyrim.MiscItem.GlazedBowl01,
                Skyrim.MiscItem.GlazedBowl01Nordic,
                Skyrim.MiscItem.GlazedBowl01Nordic,
                Skyrim.MiscItem.GlazedBowl02,
                Skyrim.MiscItem.GlazedBowl02Nordic,
                Skyrim.MiscItem.GlazedCup01,
                Skyrim.MiscItem.GlazedCup01Nordic,
                Skyrim.MiscItem.GlazedGoblet01,
                Skyrim.MiscItem.GlazedGoblet01Nordic,
                Skyrim.MiscItem.GlazedJugLarge01,
                Skyrim.MiscItem.GlazedJugLarge01Nordic,
                Skyrim.MiscItem.GlazedJugSmall01,
                Skyrim.MiscItem.GlazedJugSmall01Nordic,
                Skyrim.MiscItem.GlazedPlate01,
                Skyrim.MiscItem.GlazedPlate01Nordic,
                Skyrim.MiscItem.Kettle01,
                Skyrim.MiscItem.MS11YsgramorsSoupSpoon,
                Skyrim.MiscItem.SilverBowl01,
                Skyrim.MiscItem.SilverBowl02,
                Skyrim.MiscItem.SilverCandleStick01Off,
                Skyrim.MiscItem.SilverCandleStick02Off,
                Skyrim.MiscItem.SilverCandleStick03Off,
                Skyrim.MiscItem.SilverGoblet01,
                Skyrim.MiscItem.SilverGoblet02,
                Skyrim.MiscItem.SilverJug01,
                Skyrim.MiscItem.SilverPlate01,
                Skyrim.MiscItem.SilverPlatter01,
                Skyrim.MiscItem.SilverPlatter01IdleCups,
                Skyrim.MiscItem.SilverPlatter01IdleFood,
                Skyrim.MiscItem.WineBottle01AEmpty,
                Skyrim.MiscItem.WineBottle01BEmpty,
                Skyrim.MiscItem.WineBottle02AEmpty,
                Skyrim.MiscItem.WineBottle02BEmpty,
                Skyrim.MiscItem.WineSolitudeSpicedBottleEmpty,
                Skyrim.MiscItem.WoodenLadle01,
            ],
            nameHints:
            [
                "pot",
                "cup",
                "plate",
                "bowl",
                "fork",
                "knife",
                "spoon",
                "ladle",
                "tankard",
                "flagon",
                "goblet",
                "platter",
                "kettle",
                "jug",
                "chalice",
                "couldron",
                "pan",
                "pitcher",
                "bottle",
                "mug"
            ]
        );

    public static readonly ItemCategory SoulGem =
        new(nameof(SoulGem), new[] { Skyrim.Keyword.VendorItemSoulGem });

    public static readonly ItemCategory Tool =
        new(
            nameof(Tool),
            [Skyrim.Keyword.VendorItemTool],
            items:
            [
                Skyrim.MiscItem.ClothesIron,
                Dawnguard.MiscItem.DLC01DrawKnife,
                Dawnguard.MiscItem.DLC01TortureTool01,
                Dragonborn.MoveableStatic.DLC2ToolsChisel01
            ]
        );

    public static readonly ItemCategory Weapon =
        new(
            nameof(Weapon),
            [
                Skyrim.Keyword.VendorItemWeapon,
                Skyrim.Keyword.VendorItemStaff,
                Skyrim.Keyword.WeapTypeSword,
                Skyrim.Keyword.WeapTypeBow,
                Skyrim.Keyword.WeapTypeGreatsword,
                Skyrim.Keyword.WeapTypeBattleaxe,
                Skyrim.Keyword.WeapTypeDagger,
                Skyrim.Keyword.WeapTypeMace,
                Skyrim.Keyword.WeapTypeStaff,
                Skyrim.Keyword.WeapTypeWarAxe,
                Skyrim.Keyword.WeapTypeWarhammer
            ],
            nameHints: ["sword", "staff", "bow", "mace", "dagger", "spear"]
        );

    public static readonly ItemCategory Unknown = new(nameof(Unknown));

    #endregion
    #region Composite categories

    public static readonly ItemCategory Equipment =
        new(nameof(Equipment), Armor, Clothing, Jewelry, Weapon, Tool);

    public static readonly ItemCategory Paper = new(nameof(Paper), Book, Scroll);

    public static readonly ItemCategory Smithing =
        new(nameof(Smithing), Ingot, Ore, Pelt, Gem, BuildingMaterial);

    public static readonly ItemCategory Wearable = new(nameof(Wearable), Armor, Clothing, Jewelry);

    #endregion
    #endregion

    /// <summary>
    /// The order of categories to use when classifying items
    /// </summary>
    public static readonly IEnumerable<ItemCategory> Ordered =
    [
        Alchemy,
        Ammo,
        SoulGem,
        Book,
        Scroll,
        Potion,
        Food,
        Key,
        Tool, // Tool is before weapon so that items like pickaxes are put here instead of weapon
        Weapon,
        Jewelry,
        // Clothing is before armor since it is kind of a subset.
        // While I haven't seen any cases of something both Armor *and* Clothing keywords this is just to be sure
        Clothing,
        Armor,
        Gem,
        Ingot,
        Ore,
        Pelt,
        BuildingMaterial,
        Bone,
        Gold,
        Scrap,
        Silverware,
        Clutter, // Clutter is last since it's pretty generic and includes other more specific categories
        // COMPOSITE CATEGORIES - Are these actually useful?
        Paper,
        Smithing,
        Wearable,
        Equipment
    ];
}

/// <summary>
/// Enum-like class for classifying locations
/// </summary>
public sealed class LocationType : CategoryBase<ILocationGetter>
{
    public LocationType(
        string name,
        IEnumerable<IFormLinkGetter<IKeywordGetter>>? keywords = null,
        IEnumerable<IFormLinkGetter<ILocationGetter>>? locations = null,
        IEnumerable<IFormLinkGetter<ICellGetter>>? cells = null,
        IEnumerable<string>? nameHints = null
    )
        : base(name, keywords, locations, nameHints) =>
        Cells = cells?.ToFrozenSet() ?? FrozenSet<IFormLinkGetter<ICellGetter>>.Empty;

    public LocationType(string name, params LocationType[] subTypes)
        : base(name, subTypes) => Cells = subTypes.SelectMany(sub => sub.Cells).ToFrozenSet();

    /// <summary>
    /// As a last resort, the exact cell(s) can be specified to override all other criteria. Use this sparingly.
    /// </summary>
    public readonly FrozenSet<IFormLinkGetter<ICellGetter>> Cells;

    #region Locations
    #region Single locations

    public static readonly LocationType Barracks =
        new(nameof(Barracks), new[] { Skyrim.Keyword.LocTypeBarracks });

    public static readonly LocationType BanditCamp =
        new(nameof(BanditCamp), new[] { Skyrim.Keyword.LocTypeBanditCamp });

    public static readonly LocationType CastlePalace =
        new(nameof(CastlePalace), new[] { Skyrim.Keyword.LocTypeCastle });

    public static readonly LocationType Cave =
        new(
            nameof(Cave),
            new[]
            {
                Skyrim.Keyword.LocTypeAnimalDen,
                Skyrim.Keyword.LocSetCave,
                Skyrim.Keyword.LocSetCaveIce,
            }
        );

    public static readonly LocationType City =
        new(nameof(City), new[] { Skyrim.Keyword.LocTypeCity });

    public static readonly LocationType Dungeon =
        new(nameof(Dungeon), new[] { Skyrim.Keyword.LocTypeDungeon });

    public static readonly LocationType DwemerRuin =
        new(
            nameof(DwemerRuin),
            new[] { Skyrim.Keyword.LocTypeDwarvenAutomatons, Skyrim.Keyword.LocSetDwarvenRuin },
            locations: new[] { Skyrim.Location.ReachwindEyrieLocation }
        );

    public static readonly LocationType Farm =
        new(nameof(Farm), new[] { Skyrim.Keyword.LocTypeFarm });

    public static readonly LocationType Forsworn =
        new(
            nameof(Forsworn),
            new[] { Skyrim.Keyword.LocTypeForswornCamp, Skyrim.Keyword.LocTypeHagravenNest }
        );

    public static readonly LocationType Fort =
        new(
            nameof(Fort),
            new[] { Skyrim.Keyword.LocSetMilitaryFort, Skyrim.Keyword.LocTypeMilitaryFort },
            cells: new[] { Skyrim.Cell.HelgenKeep01 }
        );

    public static readonly LocationType GiantCamp =
        new(nameof(GiantCamp), new[] { Skyrim.Keyword.LocTypeGiantCamp });

    public static readonly LocationType Guild =
        new(
            nameof(Guild),
            [Skyrim.Keyword.LocTypeGuild],
            [
                Dawnguard.Location.DLC1HunterHQLocationInterior,
                Dawnguard.Location.DLC1VampireCastleGuildhallLocation,
                Skyrim.Location.SolitudeCastleDourLocation,
                Skyrim.Location.DawnstarSanctuaryLocation,
                Dawnguard.Location.DLC1VampireCastleDungeonLocation,
                Dawnguard.Location.DLC1VampireCastleLocation,
                Dawnguard.Location.DLC1HunterHQLocation
            ]
        );

    public static readonly LocationType HallOfTheDead =
        new(nameof(HallOfTheDead), new[] { Skyrim.Keyword.LocTypeCemetery });

    public static readonly LocationType House =
        new(nameof(House), new[] { Skyrim.Keyword.LocTypeHouse });

    public static readonly LocationType Inn = new(nameof(Inn), new[] { Skyrim.Keyword.LocTypeInn });

    public static readonly LocationType Jail =
        new(nameof(Jail), new[] { Skyrim.Keyword.LocTypeJail });

    public static readonly LocationType NordicRuin =
        new(
            nameof(NordicRuin),
            new[]
            {
                Skyrim.Keyword.LocSetNordicRuin,
                Skyrim.Keyword.LocTypeDraugrCrypt,
                Skyrim.Keyword.LocTypeDragonPriestLair
            }
        );

    public static readonly LocationType Mill =
        new(nameof(Mill), new[] { Skyrim.Keyword.LocTypeLumberMill });

    public static readonly LocationType Mine =
        new(
            nameof(Mine),
            new[] { Skyrim.Keyword.LocTypeMine },
            locations: new[] { Skyrim.Location.EmbershardLocation },
            nameHints: new[] { "mine" }
        );

    public static readonly LocationType PlayerHome =
        new(nameof(PlayerHome), new[] { Skyrim.Keyword.LocTypePlayerHouse });

    public static readonly LocationType Settlement =
        new(nameof(Settlement), new[] { Skyrim.Keyword.LocTypeSettlement });

    public static readonly LocationType Ship =
        new(nameof(Ship), new[] { Skyrim.Keyword.LocTypeShip });

    public static readonly LocationType Store =
        new(nameof(Store), new[] { Skyrim.Keyword.LocTypeStore });

    public static readonly LocationType Stronghold =
        new(nameof(Stronghold), new[] { Skyrim.Keyword.LocTypeOrcStronghold });

    public static readonly LocationType Temple =
        new(nameof(Temple), new[] { Skyrim.Keyword.LocTypeTemple });

    public static readonly LocationType Town =
        new(nameof(Town), new[] { Skyrim.Keyword.LocTypeTown });

    public static readonly LocationType Unknown = new(nameof(Unknown));

    #endregion
    #region Composite locations

    public static readonly LocationType Camp = new(nameof(Camp), BanditCamp, Forsworn, GiantCamp);

    public static readonly LocationType Government =
        new(nameof(Government), CastlePalace, Barracks, Jail);

    public static readonly LocationType Industry = new(nameof(Industry), Mill, Farm);

    public static readonly LocationType Inhabited =
        new(nameof(Inhabited), City, Town, Settlement, Stronghold);

    public static readonly LocationType Religious = new(nameof(Religious), Temple, HallOfTheDead);

    public static readonly LocationType Building =
        new(nameof(Building), Government, Store, Inn, Industry, Religious, House);

    #endregion
    #endregion

    /// <summary>
    /// The order of location types to use when classifying locations
    /// </summary>
    public static readonly IEnumerable<LocationType> Ordered =
    [
        PlayerHome,
        Inn,
        Store,
        House,
        Guild,
        CastlePalace,
        Jail,
        Barracks,
        Temple,
        HallOfTheDead,
        City,
        Town,
        Farm,
        Mill,
        Mine, // Mine comes before cave as mines also have LocTypeCave set
        Settlement,
        Stronghold,
        Ship,
        Fort,
        DwemerRuin,
        NordicRuin,
        Forsworn,
        GiantCamp,
        BanditCamp,
        Cave,
        Dungeon, // Dungeon is last since it encompasses other more specific location types
        // COMPOSITE LOCATION TYPES
        Camp,
        Religious,
        Industry,
        Government,
        Inhabited,
        Building
    ];
}
