using System;
using UnityEngine;
#if IL2CPP
using Il2CppInterop.Runtime;
#endif

namespace RightClickExit
{
    /// <summary>
    /// Turns an unhandled right-click into the game's Escape ("primary exit") while
    /// a menu or screen is open.
    /// </summary>
    /// <remarks>
    /// <para><b>How the game handles exits.</b> <c>GameInput</c> keeps a priority-sorted
    /// list of exit listeners. Escape dispatches an <c>ExitAction</c> of type
    /// <c>Primary</c>; right-click dispatches one of type <c>Secondary</c>. Listeners
    /// run from highest to lowest priority until one marks the action as used. Most
    /// screens (dialogue, ATM, shops, storage, pawn shop, casino tables...) only
    /// react to <c>Primary</c>, which is why right-click does nothing there.</para>
    ///
    /// <para><b>What this class does.</b> It adds one listener at the lowest possible
    /// priority, so it only runs when no game listener used the right-click. If a
    /// menu is open, it replays the exit as <c>Primary</c> through the same
    /// listeners, in the same order the game uses. It skips the pause menu's
    /// catch-all listener, so a right-click can never open the pause menu. Because
    /// it reuses the game's own listeners, each screen closes through its own code
    /// (sounds, animations, "are you sure" prompts and multiplayer sync all behave
    /// exactly as with Escape). It does not patch or replace any game method.</para>
    ///
    /// <para><b>When it does nothing.</b> Right-click keeps its normal meaning during
    /// regular gameplay (no menu open), while typing, while a station task is active
    /// (unless <see cref="Preferences.CloseTasks"/> is on), and while the management
    /// clipboard's object/route selectors are open, where right-click clears the
    /// selection.</para>
    ///
    /// <para><b>Cost.</b> Nothing runs per frame except a once-per-second check that the
    /// listener is still registered. The game clears all listeners on scene change.
    /// The promotion logic runs only on the frame of an unhandled right-click.</para>
    /// </remarks>
    internal static class ExitBridge
    {
        /// <summary>
        /// Our listener's priority. The game sorts ascending and dispatches from the
        /// end, so the minimum value means "run after every game listener".
        /// </summary>
        private const int ListenerPriority = int.MinValue;

        /// <summary>
        /// Priority the game's <c>PauseMenu</c> registers its catch-all listener with
        /// (it toggles the pause menu on any unused Escape). Listeners at or below
        /// this priority are skipped unless the game is already paused.
        /// </summary>
        private const int PauseMenuPriority = -9999999;

        /// <summary>How often (seconds, unscaled) to verify our listener survived a scene change.</summary>
        private const float RegistrationCheckInterval = 1f;

        /// <summary>Minimum seconds between repeated error logs, so a broken update can't flood the console.</summary>
        private const float ErrorLogCooldown = 10f;

        // Strong references so the delegates are never garbage collected while the
        // game holds them. On IL2CPP the game holds a native delegate wrapping our
        // managed one; both must stay alive.
        private static Action<ExitAction> _managedListener;
        private static GameInput.ExitDelegate _gameListener;

        private static bool _dispatching;
        private static float _nextRegistrationCheck;
        private static float _nextErrorLogTime;

        /// <summary>Called on scene load: registers the listener if it isn't already.</summary>
        internal static void OnSceneInitialized()
        {
            EnsureRegistered();
        }

        /// <summary>Called every frame; only does work once per <see cref="RegistrationCheckInterval"/>.</summary>
        internal static void Tick()
        {
            float now = Time.unscaledTime;
            if (now < _nextRegistrationCheck)
            {
                return;
            }

            _nextRegistrationCheck = now + RegistrationCheckInterval;
            EnsureRegistered();
        }

        /// <summary>Adds our listener to the game's exit-listener list if it is missing.</summary>
        private static void EnsureRegistered()
        {
            try
            {
                if (IsRegistered())
                {
                    return;
                }

                if (_gameListener == null)
                {
                    _managedListener = OnExit;
#if IL2CPP
                    // Il2CppInterop generates an implicit conversion from a managed
                    // Action<T> to the game's native delegate type.
                    _gameListener = _managedListener;
#else
                    _gameListener = new GameInput.ExitDelegate(_managedListener);
#endif
                }

                GameInput.RegisterExitListener(_gameListener, ListenerPriority);
                if (Preferences.VerboseLogging)
                {
                    Core.Log.Msg("Exit listener registered.");
                }
            }
            catch (Exception ex)
            {
                LogError("registering the exit listener", ex);
            }
        }

        /// <summary>Returns true if our listener is in the game's exit-listener list.</summary>
        private static bool IsRegistered()
        {
            var listeners = GameInput.exitListeners;
            if (listeners == null || _gameListener == null)
            {
                return false;
            }

            int count = listeners.Count;
            for (int i = 0; i < count; i++)
            {
                if (IsOurs(listeners[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Identifies our own entry by both its priority and its delegate.</summary>
        private static bool IsOurs(GameInput.ExitListener listener)
        {
            if (listener == null || listener.priority != ListenerPriority || _gameListener == null)
            {
                return false;
            }

            var fn = listener.listenerFunction;
#if IL2CPP
            // Wrapper objects differ between reads; compare the native objects.
            return fn != null && fn.Pointer == _gameListener.Pointer;
#else
            return ReferenceEquals(fn, _gameListener);
#endif
        }

        /// <summary>
        /// Our exit listener. The game calls it last, and only if no other listener
        /// used the action.
        /// </summary>
        private static void OnExit(ExitAction action)
        {
            // _dispatching guards against re-entry from our own replay below.
            if (_dispatching || action == null || action.Used || action.Type != ExitType.Secondary)
            {
                return;
            }

            try
            {
                if (!Preferences.Enabled || !ShouldPromote(out bool paused))
                {
                    return;
                }

                var promoted = new ExitAction(ExitType.Primary);
                _dispatching = true;
                try
                {
                    Replay(promoted, includePauseMenu: paused);
                }
                finally
                {
                    _dispatching = false;
                }

                if (promoted.Used)
                {
                    // Stop the game's own dispatch loop; the click was handled.
                    action.Use();
                }

                if (Preferences.VerboseLogging)
                {
                    Core.Log.Msg(promoted.Used
                        ? "Right-click handled as Escape."
                        : "Right-click replayed as Escape, but no screen accepted it.");
                }
            }
            catch (Exception ex)
            {
                // Never let an exception escape into the game's input loop.
                LogError("handling a right-click", ex);
            }
        }

        /// <summary>
        /// Decides whether an unused right-click should act as Escape right now.
        /// </summary>
        /// <param name="paused">Set to true when the pause menu is open.</param>
        private static bool ShouldPromote(out bool paused)
        {
            paused = false;

            if (GameInput.IsTyping)
            {
                return false;
            }

            paused = Singleton<PauseMenu>.InstanceExists && Singleton<PauseMenu>.Instance.IsPaused;

            // No local player camera means we are outside gameplay (main menu,
            // character creation). Every listener there is a menu, so always promote.
            if (!PlayerSingleton<PlayerCamera>.InstanceExists)
            {
                return true;
            }

            if (!Preferences.CloseTasks
                && Singleton<TaskManager>.InstanceExists
                && Singleton<TaskManager>.Instance.IsTaskActive)
            {
                return false;
            }

            if (paused)
            {
                return true;
            }

            // The game registers every open screen here (dialogue, ATM, shops,
            // storage, phone call, TV, ...). Zero means plain gameplay, where
            // right-click must keep its normal meaning (aim, vehicle, skateboard).
            if (PlayerSingleton<PlayerCamera>.Instance.ActiveUIElementCount == 0)
            {
                return false;
            }

            // In the management selectors right-click means "clear selection";
            // replaying it as Escape would submit the cleared selection instead.
            return !IsAnySelectorOpen();
        }

        /// <summary>
        /// Sends <paramref name="action"/> through the game's exit listeners, using the
        /// same order and stop rule as <c>GameInput.Exit</c>.
        /// </summary>
        private static void Replay(ExitAction action, bool includePauseMenu)
        {
            var listeners = GameInput.exitListeners;
            if (listeners == null)
            {
                return;
            }

            // Same loop shape as the game: Count is re-read every iteration because a
            // listener that closes its screen may deregister itself mid-dispatch.
            for (int i = 0; i < listeners.Count; i++)
            {
                int index = listeners.Count - (1 + i);
                if (index < 0)
                {
                    break;
                }

                var listener = listeners[index];
                if (listener == null || listener.listenerFunction == null || IsOurs(listener))
                {
                    continue;
                }

                if (!includePauseMenu && listener.priority <= PauseMenuPriority)
                {
                    continue;
                }

                listener.listenerFunction.Invoke(action);
                if (action.Used)
                {
                    break;
                }
            }
        }

        /// <summary>True if the management clipboard's object or route selector is open.</summary>
        private static bool IsAnySelectorOpen()
        {
#if IL2CPP
            foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<ObjectSelector>(), FindObjectsSortMode.None))
            {
                var selector = obj?.TryCast<ObjectSelector>();
                if (selector != null && selector.IsOpen)
                {
                    return true;
                }
            }

            foreach (var obj in UnityEngine.Object.FindObjectsByType(Il2CppType.Of<TransitEntitySelector>(), FindObjectsSortMode.None))
            {
                var selector = obj?.TryCast<TransitEntitySelector>();
                if (selector != null && selector.IsOpen)
                {
                    return true;
                }
            }
#else
            foreach (var selector in UnityEngine.Object.FindObjectsByType<ObjectSelector>(FindObjectsSortMode.None))
            {
                if (selector != null && selector.IsOpen)
                {
                    return true;
                }
            }

            foreach (var selector in UnityEngine.Object.FindObjectsByType<TransitEntitySelector>(FindObjectsSortMode.None))
            {
                if (selector != null && selector.IsOpen)
                {
                    return true;
                }
            }
#endif
            return false;
        }

        /// <summary>Logs an error at most once per <see cref="ErrorLogCooldown"/> seconds.</summary>
        private static void LogError(string context, Exception ex)
        {
            float now = Time.unscaledTime;
            if (now < _nextErrorLogTime)
            {
                return;
            }

            _nextErrorLogTime = now + ErrorLogCooldown;
            Core.Log.Error($"Error while {context}. If this started after a game update, please report it " +
                           $"with your game and MelonLoader versions.\n{ex}");
        }
    }
}
