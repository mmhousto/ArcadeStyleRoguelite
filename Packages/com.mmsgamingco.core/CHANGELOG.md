# Changelog

## [1.1.0] - 2026-11-24

- `AudioManager` works without an AudioMixer: volumes are applied to its sources directly.
- `AudioManager.PlaySFXPitched` plays a sound at an exact pitch (e.g. rising with a combo).
- `Haptics.Impact(HapticStrength)` with an `impactHandler` hook for native haptics plugins.

## [1.0.0] - 2026-11-23

First version, pulled out of ZombCube.

- Core: `Singleton<T>`, `AppInfo`, `Haptics`, shared interfaces.
- Preferences: `PreferencesManager`, `PrefKeys`.
- Audio: `AudioManager`, `AudioPlayerSettings`, `ButtonSound`, `AudioMath`.
- Scenes: `SceneLoader`, `LoadingScreen`.
- Save: `SaveSystem`, `SaveDataBase`.
- UI: `SafeArea`, `ErrorManager`, `TabGroup`, `ShowSliderValue`, `VersionLabel`.
- Pooling: `PrefabPool`, `PooledObject`, `IPoolable`.
- Services (optional): `AuthenticationManager`, `CloudSaveData`.
