# MMs Gaming Co Core (`com.mmsgamingco.core`)

Reusable boilerplate pulled out of ZombCube and cleaned up for new projects.
Namespace: `Com.MMsGamingCo.Core` (services: `Com.MMsGamingCo.Core.Services`).

## Install in another project

Add this line to `Packages/manifest.json` under `dependencies`:

```json
"com.mmsgamingco.core": "https://github.com/mmhousto/ArcadeStyleRoguelite.git?path=/Packages/com.mmsgamingco.core#Chad"
```

Pin a tag or commit instead of `#Chad` once the package is stable. If the package later
gets its own repository, only that URL changes.

## What's inside

| Area | Types | Notes |
|---|---|---|
| Core | `Singleton<T>`, `AppInfo`, `Haptics`, `IDamageable<T>`, `IUseable` | `Singleton<T>` is the ZombCube `_instance` / `Instance` pattern as a base class. |
| Preferences | `PreferencesManager`, `PrefKeys` | Typed PlayerPrefs. Volumes are stored 0-1. `preferenceChanged` event. |
| Audio | `AudioManager`, `AudioPlayerSettings`, `ButtonSound`, `AudioMath` | Mixer volumes, music crossfades, pooled SFX with pitch variance. |
| Scenes | `SceneLoader`, `LoadingScreen` | Load by name, optional loading scene, `sceneLoadStarted` / `sceneLoadFinished`. |
| Save | `SaveSystem`, `SaveDataBase` | JSON saves with temp-file writes, `.bak` backup, schema versions and `Normalize()`. |
| UI | `SafeArea`, `ErrorManager`, `TabGroup`, `ShowSliderValue`, `VersionLabel` | Notch-safe layout, queued error popups, settings tabs. |
| Pooling | `PrefabPool`, `PooledObject`, `IPoolable` | Inspector-friendly wrapper over `UnityEngine.Pool.ObjectPool`. |
| Services (optional) | `AuthenticationManager`, `CloudSaveData` | Only compiles when `com.unity.services.authentication` and `com.unity.services.cloudsave` (3.x) are installed. |

## Quick start

### 1. Managers prefab
Make a `Managers` prefab in your first scene with `AudioManager`, `ErrorManager` and
(optionally) `AuthenticationManager`. Each one is a `Singleton<T>` that persists across scenes.

### 2. Audio mixer
Create an AudioMixer with `Master`, `Music` and `SFX` groups. Expose each group's volume
as `MasterVol`, `MusicVol` and `SFXVol` (or change the names on `AudioManager`).
Assign the mixer and the Music / SFX groups on `AudioManager`.

```csharp
AudioManager.Instance.PlayMusic(menuTheme);
AudioManager.Instance.PlaySFX(coinPickup, 1f, 0.1f);           // +/-10% pitch
AudioManager.Instance.PlaySFXAt(explosion, transform.position);
```

Settings screen: add `AudioPlayerSettings` and assign three 0-1 sliders.

### 3. Saving

```csharp
[System.Serializable]
public class PlayerSave : SaveDataBase
{
    public int coins = 0;
    public string[] unlockedIds;

    public override int CurrentSchemaVersion { get { return 1; } }

    public override void Normalize()
    {
        if (unlockedIds == null) unlockedIds = new string[0];
        base.Normalize();
    }
}

PlayerSave save = SaveSystem.Load<PlayerSave>();
save.coins += 10;
SaveSystem.Save(save);
```

With services installed, sync with the cloud after sign-in:

```csharp
PlayerSave local = SaveSystem.Load<PlayerSave>();
PlayerSave newest = await CloudSaveData.Sync(AuthenticationManager.Instance.PlayerId, local);
SaveSystem.Save(newest);
```

### 4. Scenes
Add a `LoadingScene` with a `LoadingScreen` component (slider and labels are optional)
and add every scene to Build Settings.

```csharp
SceneLoader.mainMenuSceneName = "MainMenu";   // defaults shown
SceneLoader.loadingSceneName = "LoadingScene";

SceneLoader.LoadScene("Run");          // through the loading screen
SceneLoader.ReloadCurrentScene();      // instant retry
SceneLoader.ToMainMenu();
```

### 5. Safe area
On each Canvas, add a full-screen child called `SafeArea` with the `SafeArea` component
and put the UI under it.

## Changes from ZombCube

- Volumes are saved as 0-1 instead of raw decibels; `AudioMath` converts them. Old ZombCube
  volume prefs are not compatible.
- `AudioManager` no longer calls `FindWithTag` every frame; `AudioPlayerSettings` pushes changes.
- `SceneLoader` loads by name, not build index, and resets `Time.timeScale`.
- `SaveSystem` is generic (any `SaveDataBase`), keeps a backup, and drops `BinaryFormatter`
  (insecure and obsolete).
- Cloud save and sign-in are split from player data and UI. Errors are reported through
  events (`CloudSaveData.cloudSaveError`, `AuthenticationManager.signInFailed`) so you can
  hook them to `ErrorManager.Show`.
- `CloudSaveData` uses the Cloud Save 3.x `Data.Player` API instead of the deprecated
  `ForceSaveAsync` / `LoadAsync` calls.

## Tests
Edit mode tests live in `Tests/Editor`. The project lists this package under `testables` in
`Packages/manifest.json` so they show in the Test Runner.
