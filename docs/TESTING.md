# Test checklist

Run through this list before each release, on the IL2CPP build at least. Set
`VerboseLogging = true` in `UserData/MelonPreferences.cfg` so the console shows
each right-click the mod handles.

## Startup

- [ ] Console shows `[Right Click Exit] v<version> loaded for IL2CPP (main branch).`
- [ ] No errors from Right Click Exit in the console after loading a save.
- [ ] Right-click still works in the main menu: sub-screens (Settings, Load Game,
      Credits) go back one level.

## Should close with right-click

- [ ] NPC dialogue (also dialogue with several choice pages)
- [ ] Dialogue the game doesn't let you leave stays open (e.g. some quest dialogue)
- [ ] ATM: back from the amount screen, **and** close from the main ATM menu
- [ ] Shop (gas station, hardware store...)
- [ ] Pawn shop (during negotiation it should go back first, like Escape)
- [ ] Storage rack / container (already closes on right-click without the mod; should still work)
- [ ] Deal handover screen
- [ ] Casino tables (Blackjack, Ride the Bus) when not in a round
- [ ] Vehicle mod menu
- [ ] Character / clothing customization (category first, then close)
- [ ] TV
- [ ] Sleep menu
- [ ] Graffiti / spray surface (with strokes drawn it should ask to confirm, like Escape)
- [ ] Pause menu: right-click closes it (and goes back from its Settings page)

## Should NOT change

- [ ] Normal gameplay, no menu open: right-click does not open the pause menu
- [ ] Aiming a ranged weapon with right-click
- [ ] Driving: right-click does not exit the vehicle
- [ ] Skateboard: right-click does not dismount
- [ ] Inventory: right-click on an item slot behaves as before
- [ ] Phone/inventory menu: right-click behaves as before
- [ ] Station tasks (mixing, packaging, chemistry, cauldron): right-click
      multi-grab works and does not cancel the task
- [ ] With `CloseTasks = true`: right-click cancels a station task like Escape
- [ ] Management clipboard: right-click clears selection in the object/route
      selectors and does not submit/close them
- [ ] Typing in a text field: right-click doesn't close the screen
- [ ] `Enabled = false`: the game behaves exactly as without the mod

## Multiplayer (if possible)

- [ ] As host and as client: dialogue/ATM/shop close normally with right-click
