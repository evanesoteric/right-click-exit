# How Right Click Exit works

This page is for people reviewing the code, or for updating the mod after a
game patch. All game names below are from Schedule I v0.4.7f12.

## The game's exit system

`ScheduleOne.GameInput` owns two input actions:

| Field                 | Default binding | Dispatches `ExitAction` of type |
| --------------------- | --------------- | ------------------------------- |
| `PrimaryExitAction`   | Escape          | `ExitType.Primary`              |
| `SecondaryExitAction` | Right mouse     | `ExitType.Secondary`            |

Screens subscribe with `GameInput.RegisterExitListener(listener, priority)`.
The list `GameInput.exitListeners` is sorted by ascending priority, and
`GameInput.Exit` calls listeners **from the end** (highest priority first)
until one calls `ExitAction.Use()`.

Most screens only react to `Primary`, for example:

```csharp
// ScheduleOne.UI.DialogueCanvas
if (!action.Used && action.Type == ExitType.Primary && IsOpen && ...)
```

The same check exists in `ATMInterface`, `PawnShopInterface`, `HandoverScreen`,
`CasinoGameController`, `VehicleModMenu`, `CharacterCustomizationUI`,
`HospitalBillScreen` and more. `ShopInterface` and `MenuScreen` explicitly
reject `Secondary`. That's why right-click does nothing in those screens.

A few screens (the phone/inventory `GameplayMenu`, the management clipboard)
already accept `Secondary`. The newer `UIScreenManager` handles right-click as
"back" for screens registered with it. In that case `GameInput` doesn't
dispatch `Secondary` at all. That explains the mixed behavior, such as the ATM
going back with right-click but needing Escape to leave.

The pause menu registers a catch-all `Primary` listener at priority `-9999999`
that toggles pause whenever no other listener used the Escape press.

## What the mod adds

`ExitBridge` registers one listener at priority `int.MinValue`, so it always
runs **last**, and only when no game listener used the right-click. When it
receives an unused `Secondary` action and a menu is open, it:

1. creates a new `ExitAction(ExitType.Primary)`,
2. replays it through `GameInput.exitListeners` with the same loop shape and
   stop rule as `GameInput.Exit`, skipping itself and, unless the game is
   paused, every listener at or below the pause menu's priority,
3. marks the original right-click as used if a screen accepted the replay.

Because the replay goes through the game's own listeners, each screen runs its
own close logic. The mod doesn't patch, detour or replace any game method,
which makes it resilient to updates and avoids IL2CPP inlining issues that can
silently break Harmony patches.

## When a right-click is promoted

`ExitBridge.ShouldPromote` returns true only if all of these hold:

- the player isn't typing (`GameInput.IsTyping`);
- either there is no local `PlayerCamera` (main menu / character creation), or:
  - no station task is active (`TaskManager.IsTaskActive`), unless the
    `CloseTasks` preference is on, **and**
  - the pause menu is open, **or** at least one screen is open
    (`PlayerCamera.ActiveUIElementCount > 0`; every game screen registers
    itself there), **and**
  - neither management selector (`ObjectSelector`, `TransitEntitySelector`)
    is open, because right-click clears the selection there.

Inventory slots are handled by the game itself: `GameInput` doesn't dispatch
`Secondary` while the cursor hovers an item slot.

## Lifetime and cost

- `GameInput` clears `exitListeners` on every scene change, so the listener is
  re-registered in `OnSceneWasInitialized`, with a once-per-second check in
  `OnUpdate` as a fallback. That check is a scan of about 30 entries.
- The promotion logic runs only on the frame of an unhandled right-click. The
  selector check (`FindObjectsByType`) only runs in that case and only while a
  screen is open.
- Every entry point is wrapped in `try`/`catch`. Errors are logged at most once
  every 10 seconds and never reach the game's input loop.

## Updating after a game patch

If the console shows an error from Right Click Exit after an update:

1. Decompile `Assembly-CSharp.dll` (Mono branch) with ILSpy or dnSpyEx.
2. Check that `GameInput.exitListeners`, `GameInput.RegisterExitListener`,
   `GameInput.ExitListener.priority/listenerFunction`, `ExitAction`,
   `ExitType`, `PauseMenu.IsPaused`, `PlayerCamera.ActiveUIElementCount`,
   `TaskManager.IsTaskActive` and the two selectors' `IsOpen` still exist.
3. Check the pause menu's listener priority in `PauseMenu.Start`/`Awake` and
   update `ExitBridge.PauseMenuPriority` if it changed.
4. Re-run the [test checklist](TESTING.md).
