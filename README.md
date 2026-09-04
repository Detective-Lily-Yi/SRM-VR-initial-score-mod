# SRM-VR Initial Score

This is a minimal BepInEx 6 IL2CPP plugin for SRM-VR. It intercepts the live
mahjong-state initialization and replaces the hard-coded starting score.

## Build and install

Close the game, open PowerShell in this directory, and run:

```powershell
dotnet build -c Release
```

The build automatically copies `SrmVrInitialScore.dll` to:

```text
<SRM-VR>\BepInEx\plugins\SrmVrInitialScore\
```

To build without deploying it, use:

```powershell
dotnet build -c Release -p:DeployToGame=false
```

## Configure

Run the game once with the plugin installed. BepInEx creates:

```text
<SRM-VR>\BepInEx\config\com.example.srmvr.initialscore.cfg
```

Edit `InitialScore` under `[Mahjong]`, then restart the game. The default value
provided by this plugin is `10000`.

## Verify

After starting the game, check `BepInEx\LogOutput.log` for these messages:

```text
Loaded SRM-VR Initial Score; initial score is 10000.
Changed initial score for player 0: 1000 -> 10000.
Changed initial score for player 1: 1000 -> 10000.
```

The first line confirms that BepInEx loaded the plugin. The following lines
confirm that the patch replaced the score written into each live player record.

The game contains a field named `MahjongMain._init_tokuten`, but this build does
not read that field when it creates the score records. `MahjongMain.fre()` passes
the literal value `1000` to `fa.cnx(int playerIndex, int score)` instead. The mod
marks that initialization window and changes only those calls, so later score
changes of exactly 1000 are left alone.

## Why these references are local

SRM-VR uses IL2CPP. Game types such as `MahjongMain` are exposed through proxy
assemblies generated in `BepInEx\interop`; for this patch the relevant assembly
is `P5.dll`. The project references the exact BepInEx and game DLLs installed
beside it and marks them as non-copying references, so the plugin output contains
only the mod DLL.
