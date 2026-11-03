# Space Invaders 3D – Assignment 1 (CSCI 381)

Built on the class starter project. Controls: **Left/Right arrows** or **A/D** to move, **Space** to shoot, **Enter** to start / restart.

## Required changes

**Aliens speed up as their compatriots are destroyed.**
`AlienFleet.RemoveAlien()` recalculates the fleet speed every time an alien dies, interpolating from `startSpeed` (full fleet) to `maxSpeed` (last alien standing).

**Detect when the game can no longer be won.**
When the fleet drops a row, `AlienFleet.MoveDown()` checks the lowest alien. Once it reaches `loseLineY` (the ship's row), the game ends with a Game Over screen.

**Prevent the spaceship from moving offscreen.**
`Spaceship.Update()` clamps the ship's x-position to the camera's visible width (computed from `orthographicSize * aspect`, so it works at any window size).

## Code issues found and fixed

1. **Redundant `GetComponent` call** – `Alien.OnCollisionEnter` called `fleet.GetComponent<AlienFleet>()` even though `fleet` is already an `AlienFleet`. It now uses `fleet` directly through a `RemoveAlien()` method, so the fleet controls its own list.
2. **Bug in fleet edge detection** – `left` and `right` started at `0` instead of at real alien positions. If every alien was on one side of the center (e.g. all at x > 0), `left` stayed `0` and the wrong edge was reported. They now start at `float.MaxValue` / `float.MinValue`.
3. **Looping over every alien twice per physics step** – `FixedUpdate` moved all aliens in one loop and then looped again to find the edges. Both are now done in a single pass.
4. **UI updated every frame for no reason** – `AlienFleet.Update` copied the count into `AlienCounter` every frame, and `AlienCounter.Update` rebuilt the string every frame (allocating garbage). The counter now has `SetCount()`, which is only called when an alien dies. Both `Update` methods were removed.
5. **String-based `Invoke` and non-idiomatic naming** – `Invoke("startSidewaysMotion", ...)` refers to a method by string (breaks silently if renamed) and used a lowercase method name. The fleet now simply drops a fixed distance when it reaches an edge, so the timer-based `Invoke` is no longer needed.
6. **Duplicated movement code and empty methods** – `Spaceship.Update` repeated the same position code for left and right; it now computes a single direction. Empty `Start()`/`Update()` methods (which Unity still calls every frame) were removed from `Alien`, `AlienBullet`, `Dart` and `Spaceship`. `[Header]` attributes on `static` fields (which never appear in the Inspector) were cleaned up.
7. **`List<GameObject>` instead of `List<Alien>`** – the fleet stored GameObjects and had to call `GetComponent` to reach the `Alien` script. It now stores `Alien` references directly.

## Additional features

1. **Balanced to be harder but still beatable**
   - 4 rows of aliens instead of 3 (40 aliens)
   - Aliens start a bit slower but speed up to much faster by the end
   - Alien bullets move faster (.1 → .15); to keep it beatable, each alien fires slightly less often (`bulletSpawnRate` .002 → .0016)
   - Player shot cooldown raised from 0.05s to 0.35s, so holding fire can't clear the screen
   - Offset by giving the player 3 lives (below)
2. **Starting and ending screens** – `GameManager` shows a title screen (game paused until Enter is pressed), a "YOU WIN!" screen when every alien is destroyed, and a "GAME OVER – YOU HAVE BEEN ELIMINATED" screen when you run out of lives or the aliens reach your row. Enter restarts. All on-screen text (including the lives counter) uses a retro 5x7 pixel font (`PixelFont`), drawn in code so no font asset is needed.
3. **Lives** – The player has 3 lives, shown as pixel-art hearts at the top right (a heart goes dark when a life is lost). Being hit costs a life and the ship blinks while briefly invulnerable. At 0 lives the ship disappears and the game ends.
4. **New background** – `Starfield` replaces the plain blue background with a generated, slowly scrolling starfield.
5. **Better graphics for the aliens and spaceship** – The green spheres are now little aliens riding flying saucers (big slanted eyes, antennae with glowing tips, saucer running lights). Each row has its own color (green, purple, cyan, pink), and the aliens bob and wobble as they fly. The three capsules of the player ship are now a fighter jet with a pointed nose, cockpit canopy, swept wings with red tips, tail fins and a flickering engine flame. Models are built from Unity primitives at runtime (`ModelBuilder`), and the original colliders are kept so gameplay and hit detection are unchanged.
6. **Animations when collisions occur** – When a dart hits an alien, it bursts into a mini explosion: a bright flash plus debris in the alien's color that flies outward, spins and shrinks away (`Explosion`). When the player loses their last life, or the aliens reach the ship's row, the jet blows up in a bigger fiery explosion, and the end screen waits a moment so the explosion can be seen. The same short delay is used after the last alien is destroyed.

## New scripts

- `GameManager.cs` – game state, lives, start/end screens. Created automatically by `AlienFleet`.
- `Starfield.cs` – background. Created automatically by `GameManager`.
- `AlienModel.cs` – flying-saucer alien model and hover animation. Built from `Alien.Awake()`.
- `JetModel.cs` – fighter-jet model and engine flame. Built from `Spaceship.Awake()`.
- `Explosion.cs` – collision explosion effect (flash + debris).
- `PixelFont.cs` – 5x7 pixel font used for all on-screen text.
- `ModelBuilder.cs` – shared helper for building models out of primitives (materials are shared between aliens).

## Credits

- Starter project and assignment by the course instructor (CSCI 381, Fall 2026).
- All graphics (aliens, jet, explosions, starfield, pixel font, hearts) are generated in code; no outside art assets were used.
- Developed with AI-assisted programming support from Claude (Anthropic). This credit is also shown on the start and end screens.
