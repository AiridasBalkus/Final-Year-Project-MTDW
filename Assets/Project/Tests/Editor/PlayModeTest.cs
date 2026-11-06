using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Base for the scripted Play-mode tests. A test drives the player through the same
// handlers the input system calls, records Check() results, and writes a log to Temp/.
public abstract class PlayModeTest
{
    public abstract string Name { get; }
    protected virtual float TimeLimit => 90f;

    protected PlayerCombatFSM Fsm;
    protected CombatStats PlayerStats;
    protected Stamina PlayerStamina;
    protected LockOnController LockOn;
    protected TestDummyAttacker Dummy;
    protected EnemyBrain Enemy;

    protected const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    static readonly MethodInfo AttackMethod = typeof(PlayerCombatFSM).GetMethod("OnAttackPressed", Private);
    static readonly MethodInfo DodgeMethod = typeof(PlayerCombatFSM).GetMethod("OnDodgePressed", Private);
    static readonly MethodInfo LockMethod = typeof(LockOnController).GetMethod("Lock", Private);

    readonly List<string> failures = new List<string>();
    CharacterController controller;
    string logPath;
    int passed;
    float startTime;
    bool started, finished;

    const float SettleTime = 1.5f;

    public System.Action Finished;

    // return true when the test is done
    protected abstract bool Step(float now);
    protected virtual void Setup() { }
    protected virtual void OnLog(string line) { }

    public void Begin()
    {
        Fsm = Object.FindAnyObjectByType<PlayerCombatFSM>();
        PlayerStats = Fsm.GetComponent<CombatStats>();
        PlayerStamina = Fsm.GetComponent<Stamina>();
        controller = Fsm.GetComponent<CharacterController>();
        LockOn = Object.FindAnyObjectByType<LockOnController>();
        Dummy = Object.FindAnyObjectByType<TestDummyAttacker>();
        Enemy = Object.FindAnyObjectByType<EnemyBrain>();

        logPath = $"Temp/{Name}_log.txt";
        File.WriteAllText(logPath, "");
        startTime = Time.time;
        Log($"===== {Name}");

        Application.logMessageReceived += HandleLog;
        EditorApplication.update += Tick;
    }

    void Tick()
    {
        if (finished) return;
        // the first frames of a play session are slow and would skew the timing checks
        if (!started)
        {
            if (Time.time - startTime < SettleTime) return;
            started = true;
            Setup();
            return;
        }
        if (!EditorApplication.isPlaying)
        {
            Fail("play mode stopped before the test finished");
            Finish();
            return;
        }
        if (Time.time - startTime > TimeLimit)
        {
            Fail($"timed out after {TimeLimit:F0} s");
            Finish();
            return;
        }
        if (Step(Time.time)) Finish();
    }

    void HandleLog(string message, string stack, LogType type)
    {
        if (!started || finished || message.StartsWith("[Test]")) return;
        string line = message.Split('\n')[0];
        if (type == LogType.Error || type == LogType.Exception) Fail($"console {type}: {line}");
        OnLog(line);
    }

    void Finish()
    {
        finished = true;
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= HandleLog;

        int total = passed + failures.Count;
        Log(failures.Count == 0 ? $"PASSED {passed}/{total}" : $"FAILED {failures.Count}/{total}");
        string summary = $"[Test] {Name}: {(failures.Count == 0 ? "PASSED" : "FAILED")} ({passed}/{total} checks) - log in {logPath}";
        if (failures.Count == 0) Debug.Log(summary);
        else Debug.LogError(summary + "\n" + string.Join("\n", failures));

        Finished?.Invoke();
    }

    protected void Log(string line)
    {
        File.AppendAllText(logPath, $"{Time.time,7:F2}  {line}\n");
    }

    protected void Check(string description, bool ok)
    {
        if (ok)
        {
            passed++;
            Log("PASS " + description);
        }
        else
        {
            Fail(description);
        }
    }

    void Fail(string description)
    {
        failures.Add(description);
        Log("FAIL " + description);
    }

    // helpers

    protected void PressAttack() => AttackMethod.Invoke(Fsm, null);
    protected void PressDodge() => DodgeMethod.Invoke(Fsm, null);
    protected void LockOnto(LockOnTarget target) => LockMethod.Invoke(LockOn, new object[] { target });

    protected void Teleport(Vector3 position, Vector3 face = default)
    {
        controller.enabled = false;
        position.y = 0f;
        Fsm.transform.position = position;
        face.y = 0f;
        if (face.sqrMagnitude > 0.001f) Fsm.transform.rotation = Quaternion.LookRotation(face);
        controller.enabled = true;
    }

    protected static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
