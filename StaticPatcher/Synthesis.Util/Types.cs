using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Records;

namespace Synthesis.Util
{
    /// <summary>
    /// Value object containing a source record and the corresponding winning context
    /// </summary>
    /// <typeparam name="TMod"></typeparam>
    /// <typeparam name="TModGetter"></typeparam>
    /// <typeparam name="TMajor"></typeparam>
    /// <typeparam name="TMajorGetter"></typeparam>
    /// <param name="Source"></param>
    /// <param name="Winning"></param>
    public sealed record ForwardRecordContext<TMod, TModGetter, TMajor, TMajorGetter>(
        TMajorGetter Source,
        IModContext<TMod, TModGetter, TMajor, TMajorGetter> Winning
    )
        where TMod : IMod, TModGetter
        where TModGetter : IModGetter
        where TMajor : TMajorGetter
        where TMajorGetter : IMajorRecordQueryableGetter;

    /// <summary>
    /// Value object containing the winning record context and the data needed to patch the record
    /// </summary>
    /// <typeparam name="TMod"></typeparam>
    /// <typeparam name="TModGetter"></typeparam>
    /// <typeparam name="TMajor"></typeparam>
    /// <typeparam name="TMajorGetter"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="Context"></param>
    /// <param name="Values"></param>
    public sealed record PatchingData<TMod, TModGetter, TMajor, TMajorGetter, TValue>(
        IModContext<TMod, TModGetter, TMajor, TMajorGetter> Context,
        TValue Values
    )
        where TMod : IMod, TModGetter
        where TModGetter : IModGetter
        where TMajor : TMajorGetter
        where TMajorGetter : IMajorRecordQueryableGetter
        where TValue : notnull;

    /// <summary>
    /// Stores common record information.
    /// </summary>
    /// <param name="FormKey">The record FormKey</param>
    /// <param name="EditorID">The record's editor ID, if present</param>
    /// <param name="Name">The record's name, if present</param>
    public readonly record struct RecordInfo(FormKey FormKey, string? EditorID, string? Name)
    {
        public override string ToString() =>
            $$"""{ FormKey = {{FormKey}}, EditorID = {{EditorID}}, Name = "{{Name}}" }""";
    }

    /// <summary>
    /// Convenience class providing extension method constructors for the above types since who wants to type out all those generics???
    /// </summary>
    public static class ConstructorExtensions
    {
        public static ForwardRecordContext<TMod, TModGetter, TMajor, TMajorGetter> WithContext<
            TMod,
            TModGetter,
            TMajor,
            TMajorGetter
        >(this TMajorGetter record, ILinkCache<TMod, TModGetter> linkCache)
            where TMod : class, IContextMod<TMod, TModGetter>, TModGetter
            where TModGetter : class, IModGetter
            where TMajor : class, IMajorRecord, TMajorGetter
            where TMajorGetter : class, IMajorRecordGetter
        {
            var context = record
                .ToLinkGetter()
                .ResolveContext<TMod, TModGetter, TMajor, TMajorGetter>(linkCache)!;

            return new ForwardRecordContext<TMod, TModGetter, TMajor, TMajorGetter>(
                record,
                context
            );
        }

        public static PatchingData<TMod, TModGetter, TMajor, TMajorGetter, TValue> WithPatchingData<
            TMod,
            TModGetter,
            TMajor,
            TMajorGetter,
            TValue
        >(this IModContext<TMod, TModGetter, TMajor, TMajorGetter> context, TValue data)
            where TMod : IMod, TModGetter
            where TModGetter : IModGetter
            where TMajor : TMajorGetter
            where TMajorGetter : IMajorRecordQueryableGetter
            where TValue : notnull => new(context, data);

        /// <summary>
        /// Convenience method to grab common info from a record
        /// </summary>
        /// <typeparam name="TMajorGetter">The record getter type to extract info from</typeparam>
        /// <param name="record">The record to extract info from</param>
        /// <returns>A RecordInfo object containing common information from the record</returns>
        public static RecordInfo GetInfo<TMajorGetter>(this TMajorGetter record)
            where TMajorGetter : IMajorRecordGetter =>
            new(record.FormKey, record.EditorID, (record as INamedGetter)?.Name);
    }
}
