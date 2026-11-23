# Ricomet Changelog

Versions are 0.x during development; 1.0 is launch. Each version has its own `v<version>` branch.

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
