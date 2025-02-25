using Mutagen.Bethesda;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Synthesis;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Synthesis.Util;

namespace StaticPatcher
{
    public class Program
    {
        private static Lazy<StaticPatcherSettings> _settings = null!;
        private static ILogger _logger = null!;

        private static void ConfigureLogging()
        {
            LogEventLevel logLevel = _settings.Value.VerboseLogging
                ? LogEventLevel.Debug
                : LogEventLevel.Information;

            string logTemplate =
                logLevel == LogEventLevel.Debug
                    ? "{Timestamp:u}|{Level:u4}|{SourceContext}|{Message:lj}{NewLine}{Exception}"
                    : "{Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                    restrictedToMinimumLevel: logLevel,
                    theme: ConsoleTheme.None,
                    outputTemplate: logTemplate
                )
                .CreateLogger();

            _logger = Log.ForContext<Program>();
        }

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline
                .Instance.AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "StaticPatcher.esp")
                .SetAutogeneratedSettings(
                    nickname: "Settings",
                    path: "settings.json",
                    out _settings
                )
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            ConfigureLogging();
            var settings = Configuration.Load(
                state.ExtraSettingsDataPath,
                _settings.Value.SettingsFileName
            );

            ItemClassifier itemClassifier = new(state.LinkCache);
            LocationClassifier locationClassifier = new(state.LinkCache);
            var patchers = settings.ToDictionary(
                pair => pair.Key,
                pair => new DisableHavokPatcher(pair.Value, itemClassifier, state.LinkCache)
            );
            SkyrimTransformPipeline pipeline = new(state.PatchMod);

            _logger.Information("Disabling physics on configured objects");
            var refrs = state
                .LoadOrder.PriorityOrder.PlacedObject()
                .WinningContextOverrides(state.LinkCache);

            foreach (var (location, patcher) in patchers)
            {
                var matchingLocationRefrs = refrs.Where(context =>
                    context.TryGetParent<ICellGetter>(out var cell)
                    && locationClassifier.Classify(cell).IsEqualOrChildOf(location)
                );
                pipeline.Run(patcher, matchingLocationRefrs);
            }

            _logger.Information("Patched {count} total records", pipeline.PatchedCount);
        }
    }
}
