using System.Collections.Immutable;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using Synthesis.Util;

namespace StaticPatcher;

public sealed record StaticifyData(PlacedObject.DefaultMajorFlag Flags, ScriptEntry Script);

public class DisableHavokPatcher(
    IList<ItemCategory> categories,
    IList<string> locations,
    ItemClassifier classifier
) : ITransformPatcher<IPlacedObject, IPlacedObjectGetter, StaticifyData>
{
    private readonly ImmutableArray<ItemCategory> _categories = [.. categories];
    private readonly ImmutableArray<string> _locations = [.. locations];
    private readonly ItemClassifier _classifier = classifier;

    private static readonly string _scriptName = "defaultDisableHavokOnLoad";

    private static readonly ScriptEntry _disableHavokScript = CreateEntry();

    private static ScriptEntry CreateEntry()
    {
        ScriptEntry script = new();
        script.Flags.SetFlag(ScriptEntry.Flag.Local, true);
        script.Name = _scriptName;
        return script;
    }

    public StaticifyData Apply(IPlacedObjectGetter record)
    {
        var newFlags =
            (PlacedObject.DefaultMajorFlag)record.SkyrimMajorRecordFlags
            & ~PlacedObject.DefaultMajorFlag.DontHavokSettle;
        return new(newFlags, _disableHavokScript);
    }

    public bool Filter(IPlacedObjectGetter record) =>
        _categories.Contains(_classifier.Classify(record))
        && (
            record.VirtualMachineAdapter is null
            || !record.VirtualMachineAdapter.Scripts.Any(script => script.Name == _scriptName)
        );

    public void Patch(IPlacedObject target, StaticifyData data)
    {
        target.VirtualMachineAdapter ??= new VirtualMachineAdapter();
        target.VirtualMachineAdapter.Scripts.Add(data.Script);
        target.SkyrimMajorRecordFlags = (SkyrimMajorRecord.SkyrimMajorRecordFlag)data.Flags;
    }
}
