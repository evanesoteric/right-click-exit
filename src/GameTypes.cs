// Maps the game's types to a single set of names for both builds.
//
// On the Mono branch the game's code lives in the "ScheduleOne.*" namespaces.
// On the IL2CPP branch, MelonLoader generates wrapper assemblies that prefix
// every namespace with "Il2Cpp". Everything else in this mod uses only the
// aliases below, so the rest of the source has no #if blocks for type names.

#if IL2CPP
global using GameInput = Il2CppScheduleOne.GameInput;
global using ExitAction = Il2CppScheduleOne.ExitAction;
global using ExitType = Il2CppScheduleOne.ExitType;
global using PauseMenu = Il2CppScheduleOne.UI.PauseMenu;
global using PlayerCamera = Il2CppScheduleOne.PlayerScripts.PlayerCamera;
global using TaskManager = Il2CppScheduleOne.PlayerTasks.TaskManager;
global using ObjectSelector = Il2CppScheduleOne.UI.Management.ObjectSelector;
global using TransitEntitySelector = Il2CppScheduleOne.UI.Management.TransitEntitySelector;
global using Il2CppScheduleOne.DevUtilities;
#else
global using GameInput = ScheduleOne.GameInput;
global using ExitAction = ScheduleOne.ExitAction;
global using ExitType = ScheduleOne.ExitType;
global using PauseMenu = ScheduleOne.UI.PauseMenu;
global using PlayerCamera = ScheduleOne.PlayerScripts.PlayerCamera;
global using TaskManager = ScheduleOne.PlayerTasks.TaskManager;
global using ObjectSelector = ScheduleOne.UI.Management.ObjectSelector;
global using TransitEntitySelector = ScheduleOne.UI.Management.TransitEntitySelector;
global using ScheduleOne.DevUtilities;
#endif
