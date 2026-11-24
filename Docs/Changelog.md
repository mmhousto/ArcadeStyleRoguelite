# Ricomet Changelog

Versions are 0.x during development; 1.0 is launch. Each version has its own `v<version>` branch.

## 0.4: Playtest bot and tutorial

**Fixes after playtesting** (players and the bot all died inside the Prism):
- The Prism's shield is now one-way: it bounces dashes coming in, but you can always dash out.
  Before, the inside of the ring bounced you back in and trapped you with the core and beams.
- The Prism's core is a bumper: a dash that hits it deals damage and ricochets off, so you can
  never end a dash inside the core. Before, dashes passed through and could stop inside it,
  taking a hit every second because knockback doesn't move bosses.
- Safety net: a stopped comet overlapping the core is pushed out.
- The aim line and the bot both show and use the core ricochet. Boss tutorial hints explain it.

**Second playtest round** (bot reached phases 2-3 on all seeds, but every hit came while it
was stopped and out of energy):
- Dash hits on a boss refund 0.75 of a charge (and some Focus). Before, only kills refunded,
  so attacking the Prism drained your charges and stranded you as its attacks peaked.
- Prism phase 3 is more readable: 3 beams (was 4), orb rings of 10 every 4.5s (was 12 every
  3.5s), slightly slower shield. Phase 2 orbs every 5s (was 4.5s). Orb rings never fire while
  beams are up, so the attacks alternate.
- Bot: keeps a reserve charge for boss hits unless the landing is safe, values kills over the
  boss when low on energy, looks 1.2s ahead for beams, and checks each orb's whole path.
- Playtest reports log what caused each hit (beam, orb, which enemy) and the energy left.

**Third playtest round** (regression: the bot died in waves 1-4, every hit with under one
charge left): the previous change let the bot spend its last charge on a single kill, which
stranded it. The bot now predicts its energy after each dash (spend 1, plus kill and boss-hit
refunds) and only attacks if it will still have a full charge to escape with. It spends its
last charge only when staying put means getting hit, preferring an escape that also kills.
It also reacts to approaching enemies earlier.


- **Playtest bot** (`BotPilot`, `BotDirector`): simulates 216 possible dashes (72 directions
  x 3 powers, with wall bounces and shield ricochets) and picks the best by kills and boss hits
  minus landing danger (enemies, Dart charge lines, beams, orbs, boss core, inside the shield).
  Keeps a charge in reserve and escapes when stopping gets dangerous. Always picks a Rare or
  better upgrade when offered (rarest first).
- Watch it: Ricomet > Playtest > Watch Bot Play (loops runs), or press P during play.
- **Automated playtest** (Play Mode test `BotPlaytest`): the bot plays Sector 1 on seeds 101,
  202 and 303 at 3x speed, using a temporary save folder. Each run writes a Markdown report and
  a line in `Logs/Playtests/summary.csv`.
- **Tutorial overlay**: 15 hints from the first drag to the Prism boss (aim, dash, kill,
  energy, slow motion, wall bounce, stopping, combo, Darts, Splitters, upgrade picker, boss
  core, shield ricochet, beams, phases). Each completes when the player does it (or after a few
  seconds for explanations) and is saved so it never shows again. New-enemy, picker and boss
  hints interrupt the basics. Off while the bot plays.
- Reset the tutorial with T during play or Ricomet > Saves > Reset Tutorial.
- Comet input goes through `IAimSource`, so the player and the bot drive it the same way.
- `Logs/` is now git-ignored.

## 0.3: Sector 1 boss

- **Prism**, the Sector 1 boss, after wave 4 (with an upgrade pick first and a WARNING banner):
  - Crystal core: dash through it to damage it (it hurts if you stop on it).
  - Spinning shield ring with gaps. Dashes ricochet off solid segments for free (no wall
    bounce used); the aim line previews the deflection.
  - Sweeping laser beams with a thin telegraph first; orb rings; minion calls.
  - 3 phases (66% / 33%): more gaps but a faster, reversing shield, more beams, orbs, Darts
    and Splitters. Can't skip a phase in one hit; brief invulnerability between phases.
  - Dash deals 2 damage to bosses, Shockwave 1, other effects 0.5, with a short hit cooldown.
  - Defeat: big explosion, minions and orbs cleared, then Sector Clear.
- Dash obstacles: anything implementing `IDashObstacle` deflects dashes (used by the shield).
- Enemy projectiles: hurt the comet when stopped, smashed by dashes.
- HUD: boss health bar with phase ticks, phase banners.
- Save tracks bosses defeated (schema version 3).
- Tests for shield ricochet math, phase thresholds and boss damage.

- Stopping is less punishing: when a dash ends, a landing pulse knocks back and briefly stuns
  nearby enemies (stunned enemies can't move or hurt you). A Dart that is winding up or
  charging gets interrupted.
- Taking a hit blasts nearby enemies further away and stuns them longer.
- Post-dash grace period 0.12s → 0.2s.
- Wall bounce is now standard (1 bounce). Rebound adds +1 per stack.

## 0.2: Run loop

- Waves replace endless spawning. Sector 1 has 4 hand-made waves; clearing it offers endless
  generated waves. Spawns wait while the arena is full.
- Time bonus for clearing a wave under par.
- New enemy: Splitter (2 hits, bursts into 2 fast mini Drifters).
- Upgrade system: 12 upgrades with rarities and stacking (`UpgradeCatalog`), seeded offers,
  1 free reroll per sector. Comet numbers now come from `CometStats` (tuning + upgrades).
- Upgrade effects: Rebound, Seeker, Shockwave, Plasma Trail, Arc, Shatter, Reserve, Siphon,
  Clarity, Overdrive, Bulwark, Phase.
- Upgrade picker between waves (tap, gamepad or keyboard), game paused while picking.
- HUD: wave counter, enemies left, upgrades owned, wave banners, end screen with build
  summary, Sector Clear with Keep Going / New Run.
- Save tracks best wave and sectors cleared (schema version 2).
- Tests for upgrades, offers, stats and waves.

## 0.1: Gray-box prototype (M0)

- Working title set to **Ricomet**. Bundle ID `com.mmsgamingco.ricomet`, company MMs Gaming Co,
  landscape only.
- Prototype scene built from code (`Assets/Scenes/Prototype.unity`): arena, comet, endless
  spawner, IMGUI HUD.
- Comet: slingshot aim (touch, mouse, gamepad), dash, 3 charges with kill refunds, Focus-limited
  slow motion, 3 shields, swept dash hit detection, optional wall bounce.
- Enemies: Drifter and Dart, pooled with the Core `PrefabPool`.
- Score, combo multiplier, multi-kill bonus, combo milestones, best results saved locally.
- Feedback: hit stop, screen shake, burst effects, comet trail, neon bloom volume profile.
- Edit mode tests: seeded random, dash geometry, dash energy, focus, combo.
