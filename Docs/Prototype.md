# Ricomet Prototype (v0.2)

Gray-box prototype. v0.1 asked **is dashing fun?** v0.2 adds the roguelite run loop:
waves, upgrades between waves, and a sector to clear.
Everything is built from code at runtime: no prefabs, no art, no UI assets.

## Run it

1. Open the project in Unity 6000.3.
2. Open `Assets/Scenes/Prototype.unity` (it's first in Build Settings).
3. Press Play. Set the Game view to a landscape size (e.g. 1920x1080 or an iPhone landscape preset).

To try it on an iPhone, build for iOS as normal; the prototype scene is the first scene.

## Controls

| | Touch / mouse | Gamepad | Keyboard |
|---|---|---|---|
| Aim | Press anywhere and drag back (slingshot) | Left stick points where you'll go | (mouse) |
| Dash | Release | A or RT | (mouse) |
| Cancel | Drag back to where you started, then release | Let go of the stick | |
| Pick upgrade | Tap a card | D-pad + A | 1 / 2 / 3, or arrows + Enter |
| Reroll | Tap Reroll | X or Y | R |
| End screen | Tap a button | A = main button, X/Y = New Run | Enter / R |

Debug keys: **B** toggles a base wall bounce (on top of Rebound), **F** toggles the Focus limit.

## The run (Sector 1)

1. Four waves: Drifters only, then Darts, then Splitters, then everything.
2. Clear a wave (every enemy spawned and destroyed) for a time bonus if you beat par.
3. Pick 1 of 3 upgrades. One free reroll per sector.
4. After wave 4: **Sector Clear**. Keep Going starts endless waves (with an upgrade first),
   or start a New Run. The boss arrives in 0.3.

## Enemies

- **Drifter** (triangle): drifts toward you.
- **Dart** (arrow): stops, shows an orange line, then charges. Faded and harmless while recovering.
- **Splitter** (hexagon): slow, takes 2 hits, bursts into 2 small fast Drifters.

## Upgrades (12)

| Upgrade | Rarity | Effect | Max |
|---|---|---|---|
| Rebound | Common | Bounce off walls, +1 bounce per stack | 3 |
| Seeker | Rare | Kills redirect your dash at the nearest enemy | 3 |
| Shockwave | Common | Landing blasts nearby enemies; radius grows | 3 |
| Plasma Trail | Rare | Dash leaves a trail that destroys enemies | 3 |
| Arc | Rare | Direct kills zap nearby enemies, +1 target per stack | 3 |
| Shatter | Epic | Direct kills burst into fragments | 2 |
| Reserve | Common | +1 dash charge | 2 |
| Siphon | Common | +0.1 kill refund | 3 |
| Clarity | Common | +50% Focus, faster charge regen | 2 |
| Overdrive | Common | Dash 20% faster and 15% farther | 3 |
| Bulwark | Rare | +1 max shield (and repair it) | 2 |
| Phase | Rare | Invulnerable briefly after each dash | 2 |

Arc and Shatter only trigger from dash and Shockwave kills, so chains can't loop forever.
Upgrade offers and spawns use separate seeded random streams: the same seed always gives the
same spawns, whatever upgrades you take.

## Tuning

All numbers live in `PrototypeTuning`. To tweak them:

1. Assets > Create > Ricomet > Prototype Tuning.
2. Either put it at `Assets/Resources/PrototypeTuning.asset`, or drag it onto the
   **Prototype Bootstrap** object's `Tuning` field.
3. Change values in the Inspector, even during Play mode (most apply immediately; arena size,
   charges, shields and focus length apply on restart).

Set **Seed** on the bootstrap to a number to replay the same spawn pattern.

## Questions to answer while playing

- Does a full drag feel like the right dash length? Too short or too long?
- Is slow motion while aiming exciting, or does it slow the game down too much? Limited or unlimited Focus?
- Does the kill refund (0.35) keep you moving when you play well and punish misses enough?
- Is stopping dangerous enough? Too punishing?
- Does wall bounce (B) feel like it should be a base mechanic or an upgrade?
- Which upgrades feel great, and which feel like nothing happened?
- Is Sector 1 the right length (target 3-5 minutes)? Too easy / too hard?
- Does the picker break the flow, or is it a nice breather?
- Are the Darts readable? Is the warning long enough on a phone?
- Where does it start to feel hectic, and is that fun or frustrating?

## Known limits (on purpose)

- HUD uses IMGUI and the default font. A proper uGUI HUD comes in the vertical slice.
- Shapes are LineRenderers. Proper vector sprites come with the art pass.
- No audio yet.
- No boss, shop, Sparks, menus or meta progression yet.
- Upgrades and waves are defined in code (`UpgradeCatalog`, `SectorCatalog`); they move to
  ScriptableObjects when they need icons and designer tuning.
