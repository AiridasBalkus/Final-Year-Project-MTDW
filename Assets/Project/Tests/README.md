# Play-mode tests

Scripted tests that play the Playground or Area01 scene: they drive the player through the same
handlers the input system calls, check the results, and report PASSED or FAILED.

## Running them in the Editor

1. Pick a test from the **MTDW → Tests** menu. Each test opens its own scene first (the first six use
   `Playground`, Enemy B and Checkpoint and Respawn use `Area01`). If the open scene has unsaved
   changes you get the usual save prompt.
   - **Run All** runs the eight tests below, one after another
   - **Player Locomotion** (~25 s): walk, run and sprint reach the PlayerMovementConfig speeds, the animation keeps up with the real speed (no foot sliding), turning around never slides sideways, the roll travels its clip distance times dodgeDistanceScale in a straight line, locked-on strafing reaches lockedRunSpeed, the slash's code lunge covers its distance and speeds up with the swing.
   - **Combat Regression** (~45 s): player vs the TestDummy. Input buffering, i-frame timing (hit before, inside, after the window), stamina dropping attack/dodge at 0, regen delay and rate, hit interrupts, poise absorb/break, dummy stagger cancelling its wind-up, poise reset. The player's swing speed is pinned to 1x for exact timing.
   - **Block and Heal** (~25 s): player vs the TestDummy. Block walk speed and slowed regen, blocked hit in front (reduced damage, block stamina cost, no poise damage, shield flash, no interrupt), a hit from behind lands, guard break below 0 stamina and its length, heal applying at HealApply while walking, an interrupted heal losing its charge, refusals at 0 charges and 0 stamina, HUD charge pips.
   - **Enemy Perception and Leash** (~60 s): view cone, Alert then Chase, chasing at its own rolled run speed without foot sliding, lose-track and walk home, leash break at 20 m, noticing the player again on the way home, fighting back when hit while returning, a running player pulling away, walking behind it unheard and running behind it heard.
   - **Enemy Combat and Dodge** (~25 s): Reposition, Attack, Recover loop, wind-up glow on and off, hits landing, dodging inside vs too early, swing speeds inside their AttackData range and the hit landing at (1x time) / speed.
   - **Enemy Stagger and Death** (~25 s): third hit in the wind-up cancels the attack, stagger length, death drops the lock-on, corpse on the floor.
   - **Enemy B** (~45 s, Area01): three hits spread wider than the retaliation window don't trigger it, three back-to-back slashes make it sweep straight away and the sweep lands, hitting in pairs and backing off never triggers it, five slashes don't stagger it and the sixth does, a poise break in the heavy's wind-up is absorbed by hyper armour (damage still applies, the heavy lands), the same break during the sweep cancels it, the sweep's arc catches a player 100° to the side and the heavy's doesn't. Enemy B gets a config copy with zero attack weights, so it only attacks when retaliating or forced.
   - **Checkpoint and Respawn** (~25 s, Area01): dying before any rest (YOU DIED fades in, the body stays put, respawn at the level start), resting through Interact (prompt, health/stamina/heal charges refilled, respawn point set, killed and hurt enemies left as they are, no prompt mid-roll), then killed by an Enemy A (it stops and walks home, no hits on the body, respawn at the checkpoint refilled, every enemy alive at spawn with full health and poise, idle and lockable, lock-on cleared).
2. Don't touch the controls while a test runs. Each test enters Play mode on its own and stops it when done.
3. Read the result in the Console: `[Test] <name>: PASSED (n/n checks)`, or a red `FAILED` error listing the failed checks.
   The full step-by-step log is in `Temp/<name>_log.txt` in the project folder.

**Cancel Queued Tests** clears a Run All that's still waiting. To stop a running test, exit Play mode (it reports FAILED).

## Notes

- Each test starts a fresh Play session, so it always starts from the saved scene.
- Values changed during a test (player poise, enemy vitality, switching the dummy or enemy off) only last for that Play session.
  Data assets that a test changes (attack speed, roll scale) are swapped for an `Instantiate` copy first, because edits to a ScriptableObject in Play mode are saved to the asset.
- The tests set `Application.runInBackground` for their session, so they keep running if the Editor loses focus.
- The scripts are Editor code in `Tests/Editor/`, so they aren't included in builds.
- They aren't Test Runner tests. The Test Framework (1.8.0) is installed, but its tests have to live in an
  assembly definition, and asmdefs can't reference the game scripts (they compile into the default
  Assembly-CSharp with no asmdefs). Moving the game into asmdefs would make Test Runner tests possible.

## Running them from the command line

With the Editor open: `unity command editor invoke_menu` with the menu path, e.g. `MTDW/Tests/Run All`,
then read `Temp/<name>_log.txt` or the Console.
