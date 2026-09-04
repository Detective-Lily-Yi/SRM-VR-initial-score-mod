using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace SrmVrInitialScore;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
[BepInProcess("SRM-VR.exe")]
public sealed class Plugin : BasePlugin
{
    public const string PluginGuid = "com.example.srmvr.initialscore";
    public const string PluginName = "SRM-VR Initial Score";
    public const string PluginVersion = "1.1.0";

    internal static ConfigEntry<int> InitialScore { get; private set; } = null!;
    internal static ManualLogSource PluginLog { get; private set; } = null!;

    private Harmony? _harmony;

    public override void Load()
    {
        PluginLog = Log;
        InitialScore = Config.Bind(
            "Mahjong",
            "InitialScore",
            10_000,
            "The score assigned at the start of a mahjong game. The game default is 1000.");

        _harmony = new Harmony(PluginGuid);
        _harmony.PatchAll(typeof(Plugin).Assembly);

        Log.LogInfo($"Loaded {PluginName}; initial score is {InitialScore.Value}.");
    }

    public override bool Unload()
    {
        _harmony?.UnpatchSelf();
        return true;
    }
}

internal static class ScoreInitializationContext
{
    [ThreadStatic]
    internal static bool IsInitializing;
}

// MahjongMain.fre() constructs the live mahjong state and fills every player's
// score array. This build passes a hard-coded 1000 to fa.cnx(); it does not read
// the similarly named MahjongMain._init_tokuten field.
[HarmonyPatch(typeof(MahjongMain), nameof(MahjongMain.fre))]
internal static class MahjongStateInitializationPatch
{
    private static void Prefix()
    {
        ScoreInitializationContext.IsInitializing = true;
    }

    private static void Postfix()
    {
        ScoreInitializationContext.IsInitializing = false;
    }
}

[HarmonyPatch(typeof(fa), nameof(fa.cnx))]
internal static class InitialPlayerScorePatch
{
    private static void Prefix(
        [HarmonyArgument(0)] int playerIndex,
        [HarmonyArgument(1)] ref int score)
    {
        if (!ScoreInitializationContext.IsInitializing)
        {
            return;
        }

        var originalScore = score;
        score = Math.Max(1, Plugin.InitialScore.Value);
        Plugin.PluginLog.LogInfo(
            $"Changed initial score for player {playerIndex}: {originalScore} -> {score}.");
    }
}
