# Ricomet Prototype (v0.1)

Gray-box prototype for milestone M0. Its only job is to answer: **is dashing fun?**
Everything is built from code at runtime: no prefabs, no art, no UI assets.

## Run it

1. Open the project in Unity 6000.3.
2. Open `Assets/Scenes/Prototype.unity` (it's first in Build Settings).
3. Press Play. Set the Game view to a landscape size (e.g. 1920x1080 or an iPhone landscape preset).

To try it on an iPhone, build for iOS as normal; the prototype scene is the first scene.

## Controls

| | Touch / mouse | Gamepad |
|---|---|---|
| Aim | Press anywhere and drag back (slingshot) | Left stick points where you'll go |
| Dash | Release | A or RT |
| Cancel | Drag back to where you started, then release | Let go of the stick |
| Restart after game over | Tap / click | A or Start (or Space / Enter) |

Keyboard debug toggles: **B** turns wall bouncing on/off, **F** turns the Focus limit on/off.

## What's in it

- Comet with 3 dash charges (pips above it), kill refunds, slow regen while stopped.
- Slow motion while aiming, limited by the Focus ring (yellow arc).
- Invulnerable while dashing, vulnerable while stopped; 3 shields (top left).
- Dash hits are swept along the path, so fast dashes never skip enemies.
- Drifter (triangle): drifts toward you.
- Dart (arrow): stops, shows an orange line, then charges. Faded and harmless while recovering.
- Endless spawning that ramps up over 3 minutes, with warning rings before each spawn.
- Combo multiplier, multi-kill bonuses (3+ kills in one dash), combo milestone popups.
- Hit stop, screen shake, kill bursts, landing rings, bloom.
- Best score, best combo and best time saved locally (Core `SaveSystem`, slot `prototype`).

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
- Are the Darts readable? Is the warning long enough on a phone?
- Where does it start to feel hectic, and is that fun or frustrating?

## Known limits (on purpose)

- HUD uses IMGUI and the default font. A proper uGUI HUD comes in the vertical slice.
- Shapes are LineRenderers. Proper vector sprites come with the art pass.
- No audio yet.
- No upgrades, waves, bosses or menus yet (milestone M1).
