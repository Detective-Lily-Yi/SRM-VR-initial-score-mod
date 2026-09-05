# SRM-VR Initial Score Mod

[![](https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/3718190/98ac8863e8362f32687934637fe7a776bff8b34b/library_header_japanese_2x.jpg)](https://store.steampowered.com/app/3718190/)

This is a BepInEx 6 mod for SRM-VR (スーパーリアル麻雀 Venus Returns / Super Real Mahjong Venus Returns / 超真实麻将 Venus Returns). It intercepts the live
mahjong-state initialization and replaces the hard-coded starting score.

You need [this patcher](https://github.com/arition/SRM-VR-BepInEx-Patcher) to load the mod.

## Install

Download the dll from <https://github.com/arition/SRM-VR-initial-score-mod/releases/latest>, put it in `<SRM-VR>\BepInEx\plugins\SrmVrInitialScore\SrmVrInitialScore.dll`.

## Build

Clone the repo into `<SRM-VR>\Mods`, open terminal, and run:

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
