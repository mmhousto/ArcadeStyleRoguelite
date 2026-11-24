# Ricomet Prototype (v0.4)

Gray-box prototype. v0.1 asked **is dashing fun?** v0.2 added the roguelite run loop
(waves and upgrades), v0.3 the Sector 1 boss, and v0.4 a tutorial and a playtest bot.
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

Debug keys: **B** toggles the standard wall bounce (on by default), **F** toggles the Focus limit,
**P** turns the playtest bot on/off, **T** resets the tutorial.

When a dash ends, a landing pulse (the ring) knocks back and briefly stuns enemies around you.
Taking a hit pushes enemies further away. Tune both under **Knockback** in `PrototypeTuning`.

## The run (Sector 1)

1. Four waves: Drifters only, then Darts, then Splitters, then everything.
2. Clear a wave (every enemy spawned and destroyed) for a time bonus if you beat par.
3. Pick 1 of 3 upgrades. One free reroll per sector.
4. After wave 4: one more upgrade, then the **Prism** boss.
5. Beat it for **Sector Clear**. Keep Going starts endless waves (with an upgrade first),
   or start a New Run.

## The Prism (boss)

- **Core** (two spinning triangles): a bumper. Dash into it to damage it and you ricochet off.
- **Shield ring**: one-way. Solid segments deflect dashes coming in (free, no wall bounce used),
  but you can always dash out. Slip through a gap, or bank a shot off the ring. The aim line
  shows deflections and the core ricochet.
- **Beams**: a thin flickering line warns where a beam will appear, then it sweeps. Beams and
  orbs only hurt you when you're stopped, like everything else.
- **Phases** at 66% and 33% (ticks on the health bar): more gaps but a faster shield that
  changes direction, more beams, orb rings, and Darts and Splitters as minions. It flashes
  and can't be hurt for a moment between phases.
- Damage to bosses: dash 2, Shockwave 1, other effects 0.5. Health 24, so about 12 dashes.
  Tune it on the Prism object in the Inspector during Play (Health, shield radius, beam timings).

## Enemies

- **Drifter** (triangle): drifts toward you.
- **Dart** (arrow): stops, shows an orange line, then charges. Faded and harmless while recovering.
- **Splitter** (hexagon): slow, takes 2 hits, bursts into 2 small fast Drifters.

## Upgrades (12)

| Upgrade | Rarity | Effect | Max |
|---|---|---|---|
| Rebound | Common | +1 wall bounce per dash (1 is standard) | 3 |
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

## Tutorial

Hints appear in a panel near the bottom of the screen while you play, from the first drag up
to the Prism. Each one disappears when you do what it says ("Nice!") and is saved, so it never
shows again. Hints about Darts, Splitters, the upgrade picker and the boss appear as soon as
they're relevant. Reset with **T** or Ricomet > Saves > Reset Tutorial.

## Playtest bot

- **Watch it:** Ricomet > Playtest > Watch Bot Play. It plays run after run (restarting 3
  seconds after each). Or press **P** any time during play to hand over / take back control.
- **How it plays:** every 0.04s it simulates 216 dashes (72 directions x 3 powers, including
  wall bounces and shield ricochets), scores kills and boss hits against how dangerous each
  landing spot is, and keeps a charge in reserve for escapes.
- **Upgrades:** always a Rare or better card if one is offered (rarest first), otherwise
  Reserve > Siphon > Shockwave > Rebound > Overdrive > Clarity.
- **Automated runs:** Ricomet > Playtest > Run Bot Playtests opens the Test Runner. Under
  **PlayMode**, run `BotPlaytest`. It plays Sector 1 on seeds 101, 202 and 303 at 3x speed
  (a couple of minutes each) without touching your saves. A test fails if the bot dies.
- **Reports:** `Logs/Playtests/` (Ricomet > Playtest > Open Playtest Reports) gets a Markdown
  report per run (outcome, time to boss, boss phase reached, hits taken and where, upgrades) and
  a line in `summary.csv` for comparing balance changes over time.

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
- Does the landing pulse make stopping fair without making it too safe?
- Which upgrades feel great, and which feel like nothing happened?
- Is Sector 1 the right length (target 3-5 minutes)? Too easy / too hard?
- Prism: are the gaps readable? Is ricocheting off the shield fun or frustrating?
- Are the beam telegraphs long enough? Is the fight too long (24 health)?
- Does the picker break the flow, or is it a nice breather?
- Are the Darts readable? Is the warning long enough on a phone?
- Where does it start to feel hectic, and is that fun or frustrating?

## Known limits (on purpose)

- HUD uses IMGUI and the default font. A proper uGUI HUD comes in the vertical slice.
- Shapes are LineRenderers. Proper vector sprites come with the art pass.
- No audio yet.
- No shop, Sparks, menus or meta progression yet. Only Sector 1.
- Upgrades and waves are defined in code (`UpgradeCatalog`, `SectorCatalog`); they move to
  ScriptableObjects when they need icons and designer tuning.
