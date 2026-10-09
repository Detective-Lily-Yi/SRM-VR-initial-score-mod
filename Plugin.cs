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
//33 F6 C7 83 ? ? ? ? ? ? ? ? 89 B3 ? ? ? ? 48 8B 05 ? ? ? ? 39 B0 ? ? ? ? 75 ? 48 8B C8 E8 ? ? ? ? 48 8B 05 ? ? ? ? 48 8B 80 ? ? ? ? 89 70 ? 48 8B 83 ? ? ? ? 66 89 B3
[HarmonyPatch(typeof(MahjongMain), nameof(MahjongMain.gai))]
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

//fl__bnm(this: v29, a: v17++, b: 1000, method: nullptr);
//45 33 C9 41 B8 E8 03 00 00 8B D6 48 8B CF E8 ? ? ? ? FF C6 3B F5 7C
[HarmonyPatch(typeof(fl), nameof(fl.bnm))]
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
