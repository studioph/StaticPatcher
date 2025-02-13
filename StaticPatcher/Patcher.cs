using System.Collections.Frozen;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Synthesis.Util;

namespace StaticPatcher;

public sealed record StaticifyData(PlacedObject.DefaultMajorFlag Flags, ScriptEntry Script);

public class DisableHavokPatcher(
    IEnumerable<ItemCategory> categories,
    ItemClassifier classifier,
    ILinkCache<ISkyrimMod, ISkyrimModGetter> linkCache
) : ITransformPatcher<IPlacedObject, IPlacedObjectGetter, StaticifyData>
{
    private readonly FrozenSet<ItemCategory> _categories = [.. categories];

    private readonly ItemClassifier _classifier = classifier;

    private readonly ILinkCache<ISkyrimMod, ISkyrimModGetter> _linkCache = linkCache;

    private static readonly string _scriptName = "defaultDisableHavokOnLoad";

    private static readonly ScriptEntry _disableHavokScript = CreateEntry();

    private static ScriptEntry CreateEntry()
    {
        ScriptEntry script = new();
        script.Flags.SetFlag(ScriptEntry.Flag.Local, true);
        script.Name = _scriptName;
        return script;
    }

    public StaticifyData Apply(IPlacedObjectGetter refr)
    {
        PlacedObject.DefaultMajorFlag flags = (PlacedObject.DefaultMajorFlag)
            refr.SkyrimMajorRecordFlags;
        var newFlags = flags | PlacedObject.DefaultMajorFlag.DontHavokSettle;
        return new(newFlags, _disableHavokScript);
    }

    public bool Filter(IPlacedObjectGetter refr) =>
        refr.IsMoveable(_linkCache)
        && _categories.Any(cat => _classifier.Classify(refr).IsEqualOrChildOf(cat))
        && (
            refr.VirtualMachineAdapter is null
            || !refr.VirtualMachineAdapter.Scripts.Any(script => script.Name == _scriptName)
        );

    public void Patch(IPlacedObject target, StaticifyData data)
    {
        target.VirtualMachineAdapter ??= new VirtualMachineAdapter();
        target.VirtualMachineAdapter.Scripts.Add(data.Script);
        target.SkyrimMajorRecordFlags = (SkyrimMajorRecord.SkyrimMajorRecordFlag)data.Flags;
    }
}
