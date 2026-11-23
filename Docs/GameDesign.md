# Ricomet: Game Design Document

**Working title:** Ricomet (ricochet + comet). No existing game or app found with this name; still needs an App Store and trademark check.
**Genre:** Arcade action roguelite
**Platforms:** iOS first, then Steam (PC/Mac). Android later if it does well.
**Orientation:** Landscape
**Run length:** 3-8 minutes
**Business model:** Free to play with ads and in-app purchases (mobile). Premium on Steam.
**Version:** 0.1 (first draft). Versions: 0.x during development, 1.0 = launch.

---

## 1. Pitch

You are a comet of neon light. Drag back, let go, and smash through everything in your path.
Every kill gives back dash energy, so a good chain keeps you flying and a miss leaves you
stranded in the middle of the swarm. Between waves, pick upgrades that turn your comet into
something ridiculous: splitting, bouncing, chaining lightning, leaving trails of fire.

## 2. Design Pillars

1. **Every input feels huge.** One drag and release should feel like a power move: slow-mo
   while aiming, a big impact, screen shake, particles and sound.
2. **Momentum is life.** Kills refund energy. Playing well keeps you moving; playing badly
   leaves you exposed. That risk is what makes each run exciting.
3. **Builds get wild.** Upgrades stack and combine. By the end of a good run the screen should
   be chaos you caused.
4. **Pick up and play.** A run fits in a coffee break. No tutorial walls; the first run teaches
   itself in 30 seconds.
5. **Readable at a glance.** Simple neon shapes with strict color rules so you can read a busy
   screen instantly.

---

## 3. Core Gameplay

### 3.1 Controls

| Action | Touch (iOS) | Mouse and keyboard (Steam) | Gamepad (Steam / iOS controller) |
|---|---|---|---|
| Aim | Touch anywhere and drag. Direction is opposite the drag (slingshot) | Hold left mouse, drag | Left stick |
| Dash | Release | Release left mouse | Release stick, or press RT / A |
| Cancel aim | Drag back to the center dead zone | Right click | B |
| Ability (later) | Two-finger tap, or on-screen button | Space | X |
| Pause | On-screen button / app goes to background | Esc | Start |

- Dragging is **relative**: start the drag anywhere, so your thumb never covers your comet.
- A dotted trajectory line shows the path, including the first wall bounce when the player has a
  bounce upgrade.
- Drag length sets dash power (distance). Short flick = short hop, full drag = full-length dash.
- Built on the Input System package, so touch, mouse and gamepad share one set of actions.

### 3.2 Dash and Energy

- The comet has **3 dash charges** (shown as pips around the comet).
- Each dash uses 1 charge.
- **Each kill refunds 0.35 charge.** Hitting 3 enemies in one dash refunds about one dash.
- When standing still, charges come back slowly (1 charge every 2.5 seconds).
- While dashing, the comet is **invulnerable** and destroys anything weak enough in its path.
- While stopped, the comet is **vulnerable**. This is when you take damage.

### 3.3 Focus (slow motion)

- While aiming, time slows to 25%.
- A **Focus meter** drains while you aim (about 2.5 seconds of real time when full) and refills
  while you dash. This stops players from aiming in slow motion forever while still letting
  them line up a great shot.
- When Focus is empty, aiming happens at full speed.

### 3.4 Health

- 3 **shields** (hearts). Getting hit while stopped costs one shield, followed by 1 second of
  invulnerability (blinking).
- Lose all shields and the run ends (one revive allowed per run, see Monetization).
- Shields can be restored by rare pickups and some upgrades.

### 3.5 Combo and Score

- Each kill within 1.5 seconds of the last adds to the **combo**.
- Score multiplier = 1 + combo / 10 (capped at x5 before upgrades).
- Combo milestones (10, 25, 50, 100) trigger a short flash, a sound sting and bonus Shards.
- **Trick bonuses:** multi-kill (3+ in one dash), wall-bounce kill, "Loop" (dash path crosses
  itself around enemies, which triggers a burst inside the loop), close call (an enemy shot
  passes very close).

### 3.6 Beat Pulse (stretch goal)

The music has a visible beat pulse on the arena floor. Dashing on the beat gives a small
damage and score bonus. Optional skill layer, never required.

---

## 4. Run Structure

A run is **3 sectors**. Each sector has **4 waves and a boss**. Target: about 6 minutes for a
full winning run, about 3 minutes for a typical early death.

```
Sector 1: Wave 1 > Upgrade > Wave 2 > Upgrade > Wave 3 > Upgrade > Wave 4 > Upgrade > BOSS > Shop
Sector 2: same, harder enemies and new arena
Sector 3: same, final boss
Victory > Endless mode (optional) or cash out
```

- **Waves** last 20-30 seconds or until every enemy is destroyed. Clearing fast gives a time bonus.
- **Upgrade pick:** choose 1 of 3 upgrade cards. One free reroll per sector.
- **Shop** (after each boss): spend run currency (**Sparks**, dropped by enemies) on upgrades,
  shield repair or rerolls. Sparks reset each run.
- **Endless mode:** after winning, keep going with harder looping sectors for score.
- **Arenas:** one per sector, each with its own color theme and a layout twist (pillars to
  bounce off, moving walls, bumpers, hazard zones).

### 4.1 Game Modes

| Mode | Unlock | Notes |
|---|---|---|
| Standard Run | Start | Main mode. |
| Daily Run | After first win (or 3 runs) | Same seed for everyone each day, one scored attempt, global leaderboard. |
| Endless | After first win | Score attack. |
| Challenge Runs | Later update | Fixed rules, e.g. "no stopping" or "one shield". |

---

## 5. Upgrades

Upgrades are picked during a run and lost when it ends. Each has a stable string ID, a rarity
(Common, Rare, Epic, Legendary) and tags used for synergies.

### 5.1 Starting Upgrade Pool (about 30 for launch)

**Impact**
- `impact_shockwave`: Landing creates a small shockwave.
- `impact_heavy`: Dash damage +50%, dash speed -10%.
- `impact_pierce`: Dash passes through armored enemies.
- `impact_quake`: Every 3rd dash lands with a huge shockwave.

**Bounce**
- `bounce_wall`: Bounce off walls once per dash.
- `bounce_enemy`: Kills redirect you toward the nearest enemy.
- `bounce_extra`: +1 bounce (stacks).
- `bounce_rubber`: Each bounce increases damage.

**Trail**
- `trail_fire`: Your dash leaves a burning trail for 1.5 seconds.
- `trail_frost`: The trail slows enemies.
- `trail_mine`: Drops a mine at the start of each dash.
- `trail_long`: Trails last longer (stacks).

**Split**
- `split_twin`: Each dash fires a smaller copy at an angle.
- `split_shatter`: Kills burst into 3 fragments that damage nearby enemies.
- `split_echo`: An echo of your last dash repeats 0.5 seconds later.

**Chain**
- `chain_lightning`: Kills zap 2 nearby enemies.
- `chain_magnet`: Sparks and pickups are pulled to you.
- `chain_overload`: Every 10 combo, release a lightning nova.

**Defense**
- `def_shield`: +1 max shield.
- `def_barrier`: A barrier blocks one hit, refreshes each wave.
- `def_phase`: Invulnerable for 0.5 seconds after a dash ends.
- `def_thorns`: Enemies that hit you take damage.

**Energy**
- `energy_capacity`: +1 dash charge.
- `energy_refund`: Kill refund +0.1.
- `energy_focus`: Focus meter +50%.
- `energy_rush`: Dash speed +20%.

**Economy**
- `eco_greed`: +25% Sparks.
- `eco_interest`: Earn 10% of held Sparks at the end of each wave.
- `eco_lucky`: Higher chance of Rare or better upgrades.

### 5.2 Synergies and Evolutions

Holding certain upgrades together unlocks an **evolution** card (Legendary) in the next pick.

| Needs | Evolution | Effect |
|---|---|---|
| `trail_fire` + `bounce_extra` | **Inferno Web** | Bounces leave fire lines between bounce points. |
| `split_twin` + `chain_lightning` | **Storm Pair** | Your twin is linked to you by a lightning beam. |
| `impact_shockwave` + `impact_heavy` | **Meteor** | Landing creates a crater that keeps damaging. |
| `trail_frost` + `split_shatter` | **Diamond Dust** | Frozen enemies shatter into ice shards. |
| `energy_capacity` + `energy_rush` | **Lightspeed** | Chain up to 3 dashes without stopping. |

More synergies get added with each content update. This system keeps runs fresh.

---

## 6. Enemies

All enemies are simple glowing geometric shapes. Shape tells you behavior; color tells you
danger. See Art Direction for the color rules.

| ID | Name | Shape | Behavior | How to beat it |
|---|---|---|---|---|
| `enemy_drifter` | Drifter | Triangle | Slowly drifts toward you. | Basic fodder. |
| `enemy_dart` | Dart | Thin arrow | Pauses, then charges in a straight line. | Dodge, hit it while it recovers. |
| `enemy_shield` | Bulwark | Square with a front bar | Front is shielded. | Hit from the side or behind, or bounce. |
| `enemy_splitter` | Splitter | Hexagon | Splits into 2 small hexagons, then 4 triangles. | Area damage. |
| `enemy_turret` | Turret | Circle with a barrel | Stationary, shoots slow orbs. | Punishes standing still. |
| `enemy_anchor` | Anchor | Ring | Pulls your dash toward it. | Use the pull to curve shots. |
| `enemy_mine` | Spike Mine | Star | Explodes when hit; damages enemies too. | Hit it next to groups. |
| `enemy_phantom` | Phantom | Dashed outline | Only solid (hittable) for 1 second out of every 3. | Timing. |
| `enemy_healer` | Mender | Plus sign | Repairs nearby enemies. | Priority target. |
| `enemy_spawner` | Hive | Octagon | Releases Drifters. | Priority target. |

**Elites:** any enemy can spawn as an elite with a glowing outline, more health and one
modifier (fast, shielded, explosive). Elites drop extra Sparks.

### 6.1 Bosses

| Sector | Boss | Idea |
|---|---|---|
| 1 | **Prism** | A big rotating crystal. Shoot through gaps in its spinning shield to hit the core; it fires laser beams that rotate. |
| 2 | **Serpent** | A long chain of segments. Each segment is a hit; the head is armored. Breaking the middle splits it in two. |
| 3 | **The Core** | Fills the arena center, spawns waves, changes phase at 66% and 33% health; the arena shrinks. |

---

## 7. Meta Progression (between runs)

Players earn **Shards** at the end of each run (based on score, waves cleared and combo
milestones). Shards are the permanent soft currency.

### 7.1 Comets (characters)

Each comet has a unique passive and starting upgrade. 1 free at start, others unlocked with
Shards or bought.

| ID | Comet | Passive |
|---|---|---|
| `comet_nova` | Nova | Balanced. Starts with `impact_shockwave`. (Free) |
| `comet_volt` | Volt | Kills chain lightning, but has 2 shields. |
| `comet_ember` | Ember | Leaves fire trails, burns enemies. |
| `comet_glacier` | Glacier | Slower dash, freezes on hit, +1 shield. |
| `comet_twin` | Gemini | Always splits into two. Lower damage each. |
| `comet_void` | Void | Black hole on landing pulls enemies in. Hard mode comet. |

### 7.2 Upgrade Tree

A small permanent tree bought with Shards: +1 reroll, start with 50 Sparks, 5% more Shards,
unlock new upgrades into the pool, etc. Kept **modest** so skill matters more than grinding,
especially for leaderboards.

### 7.3 Unlocks Through Play

- New upgrades enter the pool by completing achievements ("Get a 50 combo", "Beat Sector 2
  with Ember"). Gives players goals and makes early runs simpler.
- Game Center / Steam achievements mirror these.

### 7.4 Cosmetics

Pure visual, no gameplay effect: comet skins, trail styles, impact effects, kill explosions,
arena color themes, music tracks, profile banners. This is where most IAP lives (see 8).

---

## 8. Monetization (mobile)

Goal: earn well from a free game without making it feel pay-to-win. Ads and IAP should never
interrupt the action. On Steam the game is premium with no ads, and cosmetic packs become DLC,
so every IAP needs a Steam equivalent.

### 8.1 Currencies

| Currency | Earned | Bought | Spent on |
|---|---|---|---|
| **Sparks** | During a run | No | In-run shop. Resets each run. |
| **Shards** | End of each run | Yes (packs) | Comets, upgrade tree, some cosmetics. |
| **Prisms** | Rare (achievements, pass, daily streaks) | Yes (packs) | Premium cosmetics, premium comets, pass. |

### 8.2 Ads (LevelPlay mediation)

**Rewarded (player chooses to watch):**
- Revive once per run (one shield, clear nearby enemies).
- Double Shards at the end of a run.
- Free daily cosmetic crate.
- Extra upgrade reroll (once per run).

**Interstitial:**
- Only between runs, never mid-run.
- None during the first 3 runs or first day.
- At most one every 3 runs and 3 minutes.
- **Any purchase removes interstitials** (rewarded ads stay available because players choose them).

**Banner:** none. They clash with landscape gameplay and look cheap.

### 8.3 In-App Purchases (Unity IAP)

| Product | Type | Example price (USD) | Notes |
|---|---|---|---|
| Remove Ads | Non-consumable | $3.99 | Removes interstitials. Rewarded ads become free rewards (no video). |
| Starter Pack | Non-consumable, one-time | $1.99 | Shown after run 3. Comet + Shards + Prisms. Best-value hook. |
| Comet Unlocks | Non-consumable | $1.99-$2.99 each | Every comet can also be unlocked with Shards (slower). |
| Comet Bundle | Non-consumable | $7.99 | All current comets. |
| Shard Packs | Consumable | $0.99 / $4.99 / $9.99 / $19.99 | Bonus % on bigger packs. |
| Prism Packs | Consumable | $1.99 / $4.99 / $9.99 / $24.99 / $49.99 | Premium cosmetic currency. |
| Cosmetic Packs | Non-consumable | $2.99-$4.99 | Themed sets: trail + skin + explosion + arena theme + track. |
| Season Pass | Non-consumable per season | $4.99 | Free and premium tracks, 6-8 week seasons, cosmetics + Prisms. |
| Neon+ Subscription | Auto-renewable | $4.99 / month | Remove Ads, daily Prisms, +25% Shards, exclusive monthly cosmetic. |

**Rules we follow:**
- No gameplay power that affects leaderboards can only be bought. Daily Run leaderboards use
  comets/upgrades anyone can earn.
- Any random crate shows its drop odds (Apple requires this).
- **Restore Purchases** button in settings (Apple requires this).
- Prices set per App Store price tier; the table is a starting point.
- A rotating **Store** with featured cosmetics refreshes daily to drive return visits.

### 8.4 Retention Hooks

- Daily login streak (7-day cycle, Prisms on day 7).
- Daily Run with leaderboard.
- 3 daily missions ("Get 3 multi-kills", "Clear a wave in under 15 seconds").
- Seasons with themed cosmetics.
- Push notifications (opt-in only): daily run ready, pass ending soon.

---

## 9. Art Direction

**Style:** simple flat vector shapes with neon glow on a very dark background. Think of a
glowing wireframe arcade cabinet. Everything readable as a silhouette.

- **Technique:** URP 2D Renderer with HDR colors and **Bloom** post-processing to get the glow.
  Shapes are vector sprites (SVG via the Vector Graphics package or sprites exported at high
  resolution) and `LineRenderer` / `TrailRenderer` for trails and lasers.
- **Background:** near black (#07070F) with a faint, slowly scrolling grid and subtle parallax
  particles. The grid ripples on impacts.
- **Color rules (strict for readability):**
  | Thing | Color |
  |---|---|
  | Player, player effects | Cyan / white (#00F0FF) |
  | Enemies | Magenta / hot pink (#FF2E9A) |
  | Enemy projectiles and danger | Orange / red (#FF6A00) |
  | Pickups, Sparks | Yellow (#FFE600) |
  | Elites | Enemy color + white outline pulse |
  | UI | White and cyan, thin lines |
  - Each arena tints the background and grid (purple, green, blue) but never uses these
    gameplay colors.
- **Juice:** hit stop (2-4 frames freeze on big hits), screen shake, chromatic flash on combo
  milestones, particle bursts, the grid warping around impacts, enemies flash white when hit.
- **Accessibility options:** colorblind palettes (swap enemy/danger colors), reduce screen
  shake, reduce flashing (with a photosensitivity warning at first launch), adjustable bloom.

## 10. Audio

- **Music:** synthwave / electronic. Layered tracks where extra layers fade in as the combo
  grows. Boss tracks per sector.
- **SFX:** punchy and short. Pitch rises with combo count (very satisfying). Uses
  `AudioManager.PlaySFX` with pitch variance from the Core package.
- **Haptics (iOS):** light tap on kills, heavier on bosses and damage. Respects the Haptics setting.

---

## 11. UI and Screens

- **Boot** > **Main Menu** (Play, Daily, Comets, Store, Pass, Settings) > **Loading** > **Run**
- **Run HUD:** shields (top left), score and combo (top center), Sparks (top right), dash pips
  on the comet, Focus meter as a ring around the comet, pause button. All inside `SafeArea`.
- **Upgrade picker:** 3 large cards, big icons, short text, synergy hints highlighted.
- **Run summary:** score, best combo, Shards earned, "double with ad" button, Retry (big), Menu.
- Retry to playing again in under 2 seconds. Fast retries are key.

---

## 12. Technical Plan

### 12.1 Stack
- Unity 6 (6000.3), URP 2D Renderer, Input System.
- **Core package** (`com.mmsgamingco.core`): AudioManager, PreferencesManager, SceneLoader,
  SaveSystem, CloudSaveData, AuthenticationManager, SafeArea, PrefabPool, ErrorManager.
- Unity Gaming Services: Authentication, Cloud Save, Analytics, Remote Config (tuning values
  like spawn rates and ad frequency without an app update).
- **Ads:** LevelPlay (Ads Mediation package).
- **IAP:** Unity In-App Purchasing.
- **Leaderboards / achievements:** Game Center (iOS), Steamworks.NET (Steam), behind one
  interface so game code doesn't care which platform it's on.

### 12.2 Game Code Structure (`Assets/Scripts`, namespace `Com.MMsGamingCo.Ricomet`)

| Folder | Contents |
|---|---|
| `Player/` | `CometController` (aim, dash, energy, focus), `CometHealth`, `AimInput` |
| `Combat/` | `IDamageable` users, `DashHitDetector` (circle cast along the dash path), damage numbers |
| `Enemies/` | `Enemy` base, one behavior class per enemy, `EnemyDatabase` (ScriptableObject) |
| `Waves/` | `WaveDirector`, `WaveDefinition` / `SectorDefinition` (ScriptableObjects), `SeededRandom` |
| `Upgrades/` | `UpgradeDefinition` (ScriptableObject), `UpgradeDatabase`, `UpgradeInventory`, effect classes |
| `Run/` | `RunManager` (sector/wave flow, Sparks, score, combo), `RunResult` |
| `Meta/` | `PlayerProfile` (SaveDataBase), currencies, comet unlocks, missions, daily streak |
| `Monetization/` | `AdsManager`, `StoreManager` (IAP), `ProductCatalog` |
| `UI/` | HUD, upgrade picker, run summary, menus |
| `Feedback/` | `ScreenShake`, `HitStop`, `GridRipple`, VFX helpers |

### 12.3 Key Technical Decisions
- **Dash hit detection uses circle casts along the path**, not physics collisions, so fast
  dashes never tunnel through enemies and results are consistent at any frame rate.
- **Seeded randomness:** all run randomness (waves, upgrade offers, drops) comes from one
  `SeededRandom` per run, so Daily Runs are identical for every player and bugs can be replayed.
- **Data-driven content:** upgrades, enemies, waves and sectors are ScriptableObjects with
  stable string IDs, so adding content needs no code changes and saves stay compatible.
- **Pooling** for enemies, projectiles, Sparks and VFX from the start.
- **Performance target:** 60 fps on iPhone 11 and newer, optional 120 fps on ProMotion
  devices. Cap on-screen enemies (about 80) and particles.
- **Tests:** edit mode tests for upgrade synergies, seeded wave generation (same seed = same
  run), save migration and currency math.

---

## 13. Milestones

| # | Milestone | Goal | Done when |
|---|---|---|---|
| M0 | **Prototype** | Is dashing fun? | Gray-box arena, comet aim/dash/energy/focus, 2 enemy types, endless spawns. Feels good with no art. |
| M1 | **Vertical slice** | One complete sector | 4 waves + Prism boss, 10 upgrades, upgrade picker, HUD, neon art pass, juice, music, run summary. |
| M2 | **Full run** | Complete game loop | 3 sectors, 3 bosses, all 10 enemies, 30 upgrades, synergies, shop, saves. |
| M3 | **Meta and monetization** | Earn and spend | Shards, comets, upgrade tree, store, ads, IAP, cloud save, Game Center. |
| M4 | **Soft launch** | Tune with real players | TestFlight, then a limited App Store release. Analytics, Remote Config tuning. |
| M5 | **Global launch** | iOS release | Daily Run, seasons, missions, polish. |
| M6 | **Steam port** | PC/Mac | Mouse and gamepad polish, Steam achievements/leaderboards, premium pricing, cosmetic DLC. |

---

## 14. Open Questions

1. **Name.** Working title is *Ricomet*. A quick search found no game or app with this name
   ("Neon Ricochet" and "Ricochet Rush" are already taken on the App Store). Do a full App Store
   and trademark check before launch.
2. Does the player get a manual ability (bomb, time stop) on a cooldown, or is the dash enough?
   Decide after the M0 prototype.
3. Is Focus (slow motion while aiming) limited by a meter, or unlimited? Test both in M0.
4. Season Pass and subscription at launch, or added after soft launch?
5. Android at the same time as Steam, or later?
