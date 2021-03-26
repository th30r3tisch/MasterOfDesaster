using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BASIERT AUF DEM CODE VON TOM WEILAND SIEHE:
/// Weiland, Tom, 27 Oct 2019, https://github.com/tom-weiland/tcp-udp-networking/blob/682f3fe3dd0b9a7d74e13896eabbfa48d3c95e20/GameClient/Assets/Scripts/ThreadManager.cs [23.03.2021]
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

public class ThreadManager : MonoBehaviour {
    private static readonly List<Action> _executeOnMainThread = new List<Action>();
    private static readonly List<Action> _executeCopiedOnMainThread = new List<Action>();
    private static bool _actionToExecuteOnMainThread = false;

    private void Update() {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(Action action) {
        if (action == null) {
            Debug.Log("No action to execute on main thread!");
            return;
        }

        lock (_executeOnMainThread) {
            _executeOnMainThread.Add(action);
            _actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public static void UpdateMain() {
        if (_actionToExecuteOnMainThread) {
            _executeCopiedOnMainThread.Clear();
            lock (_executeOnMainThread) {
                _executeCopiedOnMainThread.AddRange(_executeOnMainThread);
                _executeOnMainThread.Clear();
                _actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < _executeCopiedOnMainThread.Count; i++) {
                _executeCopiedOnMainThread[i]();
            }
        }
    }
}
