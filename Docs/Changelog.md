# Ricomet Changelog

Versions are 0.x during development; 1.0 is launch. Each version has its own `v<version>` branch.

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
