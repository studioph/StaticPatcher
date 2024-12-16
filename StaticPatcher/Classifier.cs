using System.Collections.Immutable;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Serilog;
using Synthesis.Util;

namespace StaticPatcher;

/// <summary>
/// Abstract class for classifying records into categories
/// </summary>
/// <typeparam name="TCategory">The resulting category type to classify records into</typeparam>
/// <typeparam name="TMajorGetter">The record type to classify</typeparam>
/// <param name="linkCache">The link cache to use for resolving records</param>
public abstract class ClassifierBase<TCategory, TMajorGetter>(
    ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache
)
    where TMajorGetter : class, ISkyrimMajorRecordGetter
    where TCategory : CategoryBase<TMajorGetter>
{
    protected readonly ILinkCache<ISkyrimMod, ISkyrimModGetter> _linkCache = linkCache;

    /// <summary>
    /// Cache to store already-classified records for faster subsequent lookup
    /// </summary>
    protected readonly Dictionary<FormKey, TCategory> _cache = [];

    /// <summary>
    /// Classification test functions, in the order to try
    /// </summary>
    protected static readonly IEnumerable<Func<TMajorGetter, TCategory, bool>> _predicates =
    [
        IsMemberOf,
        HasKeywordsOf,
        NameFuzzyMatches
    ];

    /// <summary>
    /// Checks if a record is an explicit member of a category
    /// </summary>
    /// <param name="record">The record to check</param>
    /// <param name="category">The category to check for membership</param>
    /// <returns>True if the record is explicitly listed as a member of the category</returns>
    protected static bool IsMemberOf(TMajorGetter record, TCategory category) =>
        category.Members.Contains(record);

    /// <summary>
    /// Checks if a record shares keywords with a category
    /// </summary>
    /// <param name="record">The record to match</param>
    /// <param name="category">The category to match against</param>
    /// <returns>True if the record shares at least 1 keyword with the category</returns>
    protected static bool HasKeywordsOf(TMajorGetter record, TCategory category) =>
        record is IKeywordedGetter<IKeywordGetter> kwd
        && category.Keywords.Overlaps(kwd.Keywords ?? []);

    /// <summary>
    /// Checks if a record name contains any indicator words associated with a category
    /// </summary>
    /// <param name="record">The record whose name to check</param>
    /// <param name="category">The category to check against</param>
    /// <returns>True if the record name contains any indicator words matching the category</returns>
    protected static bool NameFuzzyMatches(TMajorGetter record, TCategory category) =>
        record is INamedGetter named
        && named.Name is not null
        && (category.NamePattern?.IsMatch(named.Name) ?? false);

    /// <summary>
    /// Classifies a record into the given category type.
    /// Will first attempt to lookup the result from the cache, and compute the value if missing.
    /// </summary>
    /// <param name="record">The record to classify</param>
    /// <returns>The result classification for the record</returns>
    public TCategory Classify(TMajorGetter record) =>
        _cache.GetOrAdd(record.FormKey, () => ClassifyInternal(record));

    /// <summary>
    /// Internal method to perform core classification logic
    /// </summary>
    /// <param name="record">The record to classify</param>
    /// <returns>The result classification for the record</returns>
    protected abstract TCategory ClassifyInternal(TMajorGetter record);
}

/// <summary>
/// Implementation that classifies placeable objects and references into item categories.
/// </summary>
/// <param name="linkCache"></param>
public class ItemClassifier(ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    : ClassifierBase<ItemCategory, IPlaceableObjectGetter>(linkCache)
{
    private static readonly ILogger _logger;

    static ItemClassifier()
    {
        _logger = Log.ForContext<ItemClassifier>();
    }

    public ItemCategory Classify(IPlacedObjectGetter refr) =>
        Classify(refr.Base.Resolve(_linkCache));

    protected override ItemCategory ClassifyInternal(IPlaceableObjectGetter obj)
    {
        foreach (var category in ItemCategory.Ordered)
        {
            foreach (var predicate in _predicates)
            {
                if (predicate(obj, category))
                {
                    _logger.Debug(
                        "Classified item {info} into category {category} based on {criteria}",
                        obj.GetInfo(),
                        category,
                        predicate.Method.Name
                    );
                    return category;
                }
            }
        }
        return ItemCategory.Unknown;
    }
}

/// <summary>
/// Implementation that classifies cells and locations into LocationType categories
/// </summary>
/// <param name="linkCache"></param>
public class LocationClassifier(ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache)
    : ClassifierBase<LocationType, ILocationGetter>(linkCache)
{
    private static readonly ILogger _logger;

    static LocationClassifier()
    {
        _logger = Log.ForContext<LocationClassifier>();
    }

    public LocationType Classify(ICellGetter cell) =>
        _cache.GetOrAdd(cell.FormKey, () => ClassifyCellInternal(cell));

    protected override LocationType ClassifyInternal(ILocationGetter location)
    {
        foreach (var locationType in LocationType.Ordered)
        {
            foreach (var predicate in _predicates)
            {
                if (predicate(location, locationType))
                {
                    _logger.Debug(
                        "Classified location {info} into type {location} based on {criteria}",
                        location.GetInfo(),
                        locationType,
                        predicate.Method.Name
                    );
                    return locationType;
                }
            }
        }

        return LocationType.Unknown;
    }

    /// <summary>
    /// Additional internal method to classify Cells.
    ///
    /// Cells can be classified directly or (more often) based on their LCTN field.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private LocationType ClassifyCellInternal(ICellGetter cell)
    {
        RecordInfo cellInfo = cell.GetInfo();

        // Try direct cell classification first
        foreach (var locationType in LocationType.Ordered)
        {
            if (IsMemberOf(cell, locationType))
            {
                _logger.Debug(
                    "Directly classified cell {info} into location type {location}",
                    cellInfo,
                    locationType
                );
                return locationType;
            }
        }

        // Classify based on LCTN field if present
        if (cell.Location.TryResolve(_linkCache, out var cellLocation))
        {
            var result = Classify(cellLocation);
            _logger.Debug("Classified cell {info} into location type {location}", cellInfo, result);
            return result;
        }
        else
        {
            _logger.Debug("Unable to resolve location for cell {cell}, skipping", cellInfo);
            return LocationType.Unknown;
        }
    }

    /// <summary>
    /// Checks if a cell is an explicit member of a location type
    /// </summary>
    /// <param name="cell">The cell to check</param>
    /// <param name="locationType">The location category to check membership of</param>
    /// <returns>True if the cell is explicitly listed as a member</returns>
    private static bool IsMemberOf(ICellGetter cell, LocationType locationType) =>
        locationType.Cells.Contains(cell);
}
