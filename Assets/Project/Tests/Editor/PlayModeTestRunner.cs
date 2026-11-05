using System.Linq;
using UnityEditor;
using UnityEngine;

// Menu items for the scripted Play-mode tests. Each test runs in a fresh play session.
// The queue lives in SessionState because entering play mode reloads scripts.
[InitializeOnLoad]
public static class PlayModeTestRunner
{
    const string QueueKey = "MTDW.TestQueue";
    const string Menu = "MTDW/Tests/";

    static PlayModeTestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem(Menu + "Run All", priority = 0)]
    static void RunAll() => Queue("CombatRegression", "EnemyPerceptionLeash", "EnemyCombatDodge", "EnemyStaggerDeath");

    [MenuItem(Menu + "Combat Regression", priority = 20)]
    static void RunCombat() => Queue("CombatRegression");

    [MenuItem(Menu + "Enemy Perception and Leash", priority = 21)]
    static void RunPerception() => Queue("EnemyPerceptionLeash");

    [MenuItem(Menu + "Enemy Combat and Dodge", priority = 22)]
    static void RunEnemyCombat() => Queue("EnemyCombatDodge");

    [MenuItem(Menu + "Enemy Stagger and Death", priority = 23)]
    static void RunStagger() => Queue("EnemyStaggerDeath");

    [MenuItem(Menu + "Cancel Queued Tests", priority = 40)]
    static void Cancel()
    {
        SessionState.EraseString(QueueKey);
        Debug.Log("[Test] queue cleared");
    }

    static PlayModeTest Create(string name)
    {
        switch (name)
        {
            case "CombatRegression": return new CombatRegressionTest();
            case "EnemyPerceptionLeash": return new EnemyPerceptionLeashTest();
            case "EnemyCombatDodge": return new EnemyCombatDodgeTest();
            case "EnemyStaggerDeath": return new EnemyStaggerDeathTest();
        }
        return null;
    }

    static void Queue(params string[] names)
    {
        SessionState.SetString(QueueKey, string.Join(";", names));
        Debug.Log($"[Test] queued: {string.Join(", ", names)}");
        // always start from edit mode so every test gets a fresh scene
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        else EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        string queue = SessionState.GetString(QueueKey, "");
        if (queue == "") return;

        if (change == PlayModeStateChange.EnteredEditMode)
        {
            EditorApplication.EnterPlaymode();
        }
        else if (change == PlayModeStateChange.EnteredPlayMode)
        {
            string[] names = queue.Split(';');
            string rest = string.Join(";", names.Skip(1));
            if (rest == "") SessionState.EraseString(QueueKey);
            else SessionState.SetString(QueueKey, rest);

            // keeps the game running if the editor loses focus; only lasts for this play session
            Application.runInBackground = true;

            PlayModeTest test = Create(names[0]);
            test.Finished = EditorApplication.ExitPlaymode;
            test.Begin();
        }
    }
}
