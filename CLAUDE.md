# ArcadeStyleRoguelite

Unity 6 (6000.3) URP project, iOS first. Arcade-style roguelite.

## Layout

- `Packages/com.mmsgamingco.core/`: reusable boilerplate package (audio, preferences,
  scene loading, saves, cloud save, safe area, pooling). Use it instead of rewriting
  those systems. Keep game-specific code out of it.
- `Assets/Scripts/`: game code.

## Code style

Matches Morgan's existing projects (ZombCube). Follow it for all new code.

### Namespaces and files
- `Com.MMsGamingCo.<Project>` for game code, `Com.MMsGamingCo.Core` for the package.
- One public type per file, file named after the type.
- Blank line after the `namespace {` line and before its closing brace.
- Allman braces, 4-space indent. Short `if` bodies may skip braces on the next line;
  one-liners like `if (x == null) return;` are fine.

### Naming
- Types, methods, properties: `PascalCase`.
- Fields: `camelCase`, no prefix. Public fields are normal for inspector values.
  `[SerializeField] private` when other scripts shouldn't touch it.
- The only underscore field is the singleton `_instance`.
- Events: `public static event Action<T> somethingHappened;` (camelCase).
- Constants: `PascalCase` (`public const string ResourcePath = "LevelDatabase";`).
- Inspector UI references are named by role: `musicSlider`, `waveTxt`, `pauseScreen`.

### Unity patterns
- Managers: derive from `Singleton<T>` (Core). Override `Awake`, call `base.Awake()`, then
  `if (IsDuplicate) return;`. Hand-written singletons use:
  ```csharp
  private static GameManager _instance;
  public static GameManager Instance { get { return _instance; } }
  ```
- `[Tooltip("...")]` on inspector fields whose purpose isn't obvious; `[Header]` to group.
- Static helper classes for stateless systems (`PreferencesManager`, `SceneLoader`).
- ScriptableObject databases with `[CreateAssetMenu]`, loaded via `Resources.Load` behind a
  lazy `Instance` property, and a `ResourcePath` const.
- Stable string IDs for anything saved (cosmetics, upgrades, levels). Never rename or reorder
  shipped IDs; add new ones at the end.
- Save data: public fields with defaults, derive from `SaveDataBase`, upgrade old data in
  `Normalize()`, bump `CurrentSchemaVersion` when the format changes.
- Platform code goes in `#if (UNITY_IOS || UNITY_ANDROID)` style blocks.
- Subscribe in `OnEnable`, unsubscribe in `OnDisable`.
- Use `this.gameObject` when destroying or persisting self.
- Pool anything spawned often (`PrefabPool`). Avoid `Find*` calls in `Update`.
- `#region` blocks in larger classes: `Fields/Variables`, `MonoBehaviour Methods`,
  `Public Methods`, `Private Methods`.

### Comments
- `/// <summary>` on public methods and non-obvious classes, one or two plain sentences.
- Inline comments explain why, not what. Short and plain.
- Error messages shown to players start with `Error: ` and are a full sentence.

### Tests
- NUnit edit mode tests in a `Tests` assembly. One-line comment at the top of the file saying
  what it checks. Test names are plain sentences in PascalCase (`DefaultIsAlwaysOwned`).
- Add tests for anything that touches saves, IDs or data migration.
