using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Menu items for the scripted Play-mode tests. Each test runs in a fresh play session,
// in its own scene (opened before entering play mode).
// The queue lives in SessionState because entering play mode reloads scripts.
[InitializeOnLoad]
public static class PlayModeTestRunner
{
    const string QueueKey = "MTDW.TestQueue";
    const string Menu = "MTDW/Tests/";
    const string Playground = "Assets/Scenes/Playground.unity";
    const string Area01 = "Assets/Scenes/Area01.unity";

    static PlayModeTestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem(Menu + "Run All", priority = 0)]
    static void RunAll() => Queue("PlayerLocomotion", "CombatRegression", "BlockHeal", "EnemyPerceptionLeash", "EnemyCombatDodge", "EnemyStaggerDeath", "EnemyB", "CheckpointRespawn");

    [MenuItem(Menu + "Player Locomotion", priority = 19)]
    static void RunLocomotion() => Queue("PlayerLocomotion");

    [MenuItem(Menu + "Combat Regression", priority = 20)]
    static void RunCombat() => Queue("CombatRegression");

    [MenuItem(Menu + "Block and Heal", priority = 20)]
    static void RunBlockHeal() => Queue("BlockHeal");

    [MenuItem(Menu + "Enemy Perception and Leash", priority = 21)]
    static void RunPerception() => Queue("EnemyPerceptionLeash");

    [MenuItem(Menu + "Enemy Combat and Dodge", priority = 22)]
    static void RunEnemyCombat() => Queue("EnemyCombatDodge");

    [MenuItem(Menu + "Enemy Stagger and Death", priority = 23)]
    static void RunStagger() => Queue("EnemyStaggerDeath");

    [MenuItem(Menu + "Enemy B", priority = 24)]
    static void RunEnemyB() => Queue("EnemyB");

    [MenuItem(Menu + "Checkpoint and Respawn", priority = 25)]
    static void RunCheckpoint() => Queue("CheckpointRespawn");

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
            case "PlayerLocomotion": return new PlayerLocomotionTest();
            case "CombatRegression": return new CombatRegressionTest();
            case "BlockHeal": return new BlockHealTest();
            case "EnemyPerceptionLeash": return new EnemyPerceptionLeashTest();
            case "EnemyCombatDodge": return new EnemyCombatDodgeTest();
            case "EnemyStaggerDeath": return new EnemyStaggerDeathTest();
            case "EnemyB": return new EnemyBTest();
            case "CheckpointRespawn": return new CheckpointRespawnTest();
        }
        return null;
    }

    static string SceneFor(string name) => name == "EnemyB" || name == "CheckpointRespawn" ? Area01 : Playground;

    static void Queue(params string[] names)
    {
        // tests switch scenes, so unsaved edits get the usual save prompt first
        if (!EditorApplication.isPlaying && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        SessionState.SetString(QueueKey, string.Join(";", names));
        Debug.Log($"[Test] queued: {string.Join(", ", names)}");
        // always start from edit mode so every test gets a fresh scene
        if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        else StartNext(names[0]);
    }

    static void StartNext(string name)
    {
        string scene = SceneFor(name);
        if (SceneManager.GetActiveScene().path != scene) EditorSceneManager.OpenScene(scene);
        EditorApplication.EnterPlaymode();
    }

    static void OnPlayModeChanged(PlayModeStateChange change)
    {
        string queue = SessionState.GetString(QueueKey, "");
        if (queue == "") return;

        if (change == PlayModeStateChange.EnteredEditMode)
        {
            StartNext(queue.Split(';')[0]);
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
