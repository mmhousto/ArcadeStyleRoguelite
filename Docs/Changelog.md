# Ricomet Changelog

Versions are 0.x during development; 1.0 is launch. Each version has its own `v<version>` branch.

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
