# Play-mode tests

Scripted tests that play the Playground scene: they drive the player through the same
handlers the input system calls, check the results, and report PASSED or FAILED.

## Running them in the Editor

1. Open `Assets/Scenes/Playground.unity`.
2. Pick a test from the **MTDW → Tests** menu:
   - **Run All** runs the four tests below, one after another
   - **Combat Regression** (~45 s): player vs the TestDummy. Input buffering, i-frame timing (hit before, inside, after the window), stamina dropping attack/dodge at 0, regen delay and rate, hit interrupts, poise absorb/break, dummy stagger cancelling its wind-up, poise reset.
   - **Enemy Perception and Leash** (~50 s): view cone, Alert then Chase, lose-track and walk home, leash break at 20 m, noticing the player again on the way home, fighting back when hit while returning.
   - **Enemy Combat and Dodge** (~25 s): Reposition, Attack, Recover loop, wind-up glow on and off, hits landing, dodging inside vs too early.
   - **Enemy Stagger and Death** (~25 s): third hit in the wind-up cancels the attack, stagger length, death drops the lock-on, corpse on the floor.
3. Don't touch the controls while a test runs. Each test enters Play mode on its own and stops it when done.
4. Read the result in the Console: `[Test] <name>: PASSED (n/n checks)`, or a red `FAILED` error listing the failed checks.
   The full step-by-step log is in `Temp/<name>_log.txt` in the project folder.

**Cancel Queued Tests** clears a Run All that's still waiting. To stop a running test, exit Play mode (it reports FAILED).

## Notes

- Each test starts a fresh Play session, so it always starts from the saved scene.
- Values changed during a test (player poise, enemy vitality, switching the dummy or enemy off) only last for that Play session.
- The tests set `Application.runInBackground` for their session, so they keep running if the Editor loses focus.
- The scripts are Editor code in `Tests/Editor/`, so they aren't included in builds.
- They aren't Test Runner tests. The Test Framework (1.8.0) is installed, but its tests have to live in an
  assembly definition, and asmdefs can't reference the game scripts (they compile into the default
  Assembly-CSharp with no asmdefs). Moving the game into asmdefs would make Test Runner tests possible.

## Running them from the command line

With the Editor open: `unity command editor invoke_menu` with the menu path, e.g. `MTDW/Tests/Run All`,
then read `Temp/<name>_log.txt` or the Console.
