using UnityEngine;

// Enemy perception and leash: view cone, Alert then Chase, lose-track and Return home, leash break,
// noticing the player again on the way home, and fighting back when hit while returning.
// Also its chase speed and sliding, a running player pulling away, and hearing (walk vs run behind it).
public class EnemyPerceptionLeashTest : PlayModeTest
{
    public override string Name => "EnemyPerceptionLeash";
    protected override float TimeLimit => 120f;

    static readonly Vector3 FarAway = new Vector3(28f, 0f, -8f);
    static readonly Vector3 AwayFromSpawn = new Vector3(0.6f, 0f, -0.8f); // towards open floor
    static readonly Vector3 EscapeDir = new Vector3(-0.6f, 0f, -0.8f);    // open floor, clear of the dummy
    static readonly System.Reflection.PropertyInfo MoveProp = typeof(PlayerInputHandler).GetProperty("Move");

    int step;
    float stepStart;
    bool placed, alertedWhileBehind, provoked, leashedDuringGrace, heard, noticedWhileWalking;
    float alertAt = -1f, provokedAt = -1f, chaseSince = -1f;
    float chaseSpeedSum, chaseClipSpeedSum, escapeStart = -1f, escapeStartGap;
    int chaseSamples;
    string lastPath = "";

    protected override void Setup()
    {
        Dummy.enabled = false;
    }

    protected override void OnLog(string line)
    {
        if (line.StartsWith("[Enemy]") || line.StartsWith("[Hit] Player")) Log(line);
        if (line.Contains("provoked while returning")) provoked = true;
        if (line.Contains("heard the player")) heard = true;
    }

    void Next(string title)
    {
        step++;
        stepStart = Time.time;
        placed = false;
        Log($"--- {title}");
    }

    protected override bool Step(float now)
    {
        float inStep = now - stepStart;
        Vector3 enemyPos = Enemy.transform.position;
        string path = Enemy.StatePath;
        if (path != lastPath)
        {
            Log($"STATE {lastPath} -> {path}  (dist {Enemy.DistanceToTarget:F1}, fromSpawn {Enemy.DistanceFromSpawn:F1})");
            if (path == "Alert") alertAt = now;
            if (path == "Chase") chaseSince = now;
            lastPath = path;
        }
        if (PlayerStats.Health < 50) PlayerStats.RestoreFullHealth();
        if (step > 0 && inStep > 30f)
        {
            Check($"step {step} finishes", false);
            return true;
        }

        EnemyConfig config = Enemy.config;
        switch (step)
        {
            case 0:
                Teleport(enemyPos - Enemy.transform.forward * 6f);
                Next("player 6 m behind the enemy for 2 s");
                break;

            case 1:
                if (path != "Idle") alertedWhileBehind = true;
                if (inStep > 2f)
                {
                    Check("ignores the player behind it", !alertedWhileBehind);
                    Teleport(enemyPos + Enemy.transform.forward * 6f);
                    Next("player 6 m in front");
                }
                break;

            case 2:
                if (path == "Chase")
                {
                    float pause = now - alertAt;
                    Check($"notices the player in front: Alert for {config.alertTime}s, then Chase ({pause:F2}s)", alertAt > stepStart && Mathf.Abs(pause - config.alertTime) < 0.1f);
                    Next("let it close in for 2 s");
                }
                break;

            case 3:
                if (inStep > 2f)
                {
                    Teleport(FarAway);
                    Next("player teleported far away");
                }
                break;

            case 4:
                if (path == "Idle")
                {
                    float rotErr = Quaternion.Angle(Enemy.transform.rotation, Enemy.SpawnRotation);
                    Check($"loses track, walks home and faces its spawn direction ({Enemy.DistanceFromSpawn:F2} m, {rotErr:F1} deg)",
                        Enemy.DistanceFromSpawn <= config.arriveDistance + 0.1f && rotErr < 1f);
                    Teleport(enemyPos + Enemy.transform.forward * 5f);
                    Next("kite: keep the player ~4 m ahead, moving away from spawn");
                }
                break;

            case 5:
                if (path == "Chase")
                {
                    Teleport(enemyPos + AwayFromSpawn * 4f);
                    // a straight run once it's up to speed
                    if (now - chaseSince > 1f)
                    {
                        float speed = Enemy.Motor.Velocity.magnitude;
                        chaseSpeedSum += speed;
                        Vector3 clip = Enemy.Animator.velocity;
                        clip.y = 0f;
                        chaseClipSpeedSum += Vector3.Dot(clip, Enemy.Motor.Velocity.normalized); // along the run, the clips sway sideways
                        chaseSamples++;
                    }
                }
                if (path == "Return")
                {
                    float speed = chaseSpeedSum / Mathf.Max(chaseSamples, 1);
                    float ratio = chaseClipSpeedSum / Mathf.Max(chaseSpeedSum, 0.01f);
                    Check($"chases at its own rolled run speed {Enemy.RunSpeed:F2} m/s ({speed:F2}), inside {config.runSpeedMin}-{config.runSpeedMax}",
                        chaseSamples > 0 && Mathf.Abs(speed - Enemy.RunSpeed) <= Enemy.RunSpeed * 0.1f &&
                        Enemy.RunSpeed >= config.runSpeedMin && Enemy.RunSpeed <= config.runSpeedMax);
                    Check($"run animation matches the speed, no sliding (clip/actual {ratio:F2})", ratio > 0.85f && ratio < 1.15f);
                    Check($"leash breaks at {config.leashDistance} m from spawn ({Enemy.DistanceFromSpawn:F1} m)", Mathf.Abs(Enemy.DistanceFromSpawn - config.leashDistance) < 0.5f);
                    Next("player waits on its way home");
                }
                break;

            case 6:
                if (!placed)
                {
                    placed = true;
                    Vector3 home = Enemy.SpawnPoint - enemyPos;
                    home.y = 0f;
                    Teleport(enemyPos + home.normalized * 6f);
                }
                if (path == "Alert")
                {
                    float limit = config.leashDistance - config.reaggroMargin;
                    Check($"notices the player again once {config.reaggroMargin} m inside the leash ({Enemy.DistanceFromSpawn:F1} m from spawn)", Enemy.DistanceFromSpawn <= limit + 0.1f);
                    Next("kite past the leash again");
                }
                break;

            case 7:
                if (path == "Chase") Teleport(enemyPos + AwayFromSpawn * 4f);
                if (path == "Return") Next("hit it from behind while it walks home");
                break;

            case 8:
                // give it time to turn towards home, then swing at its back
                if (!placed && inStep > 1.2f)
                {
                    placed = true;
                    Vector3 behind = enemyPos - Enemy.transform.forward * 1f;
                    Teleport(behind, enemyPos - behind);
                    LockOnto(Enemy.GetComponent<LockOnTarget>());
                    PressAttack();
                }
                if (placed && provokedAt < 0f && path != "Return") provokedAt = now;
                if (provokedAt > 0f && path == "Return" && now - provokedAt < config.provokedLeashGrace - 0.1f) leashedDuringGrace = true;
                if (provokedAt > 0f && now - provokedAt > 3f)
                {
                    Check("fights back when hit while returning", provoked);
                    Check($"doesn't leash again during the {config.provokedLeashGrace}s grace", !leashedDuringGrace);
                    Teleport(FarAway);
                    Next("player teleported far away");
                }
                if (placed && inStep > 6f && provokedAt < 0f)
                {
                    Check("fights back when hit while returning", false);
                    return true;
                }
                break;

            case 9:
                if (path == "Idle")
                {
                    Check("walks home again after losing track", Enemy.DistanceFromSpawn <= config.arriveDistance + 0.1f);
                    Teleport(enemyPos + Enemy.transform.forward * 5f);
                    Next("disengage: once it chases, the player runs away");
                }
                break;

            case 10:
                if (escapeStart < 0f && path == "Chase" && now - chaseSince > 0.5f)
                {
                    escapeStart = now;
                    escapeStartGap = Enemy.DistanceToTarget;
                    Fsm.input.enabled = false;
                }
                if (escapeStart > 0f)
                {
                    // camera-relative stick that points along EscapeDir
                    Transform cam = Fsm.locomotion.cameraTransform;
                    Vector3 right = cam.right, forward = cam.forward;
                    right.y = forward.y = 0f;
                    MoveProp.SetValue(Fsm.input, new Vector2(Vector3.Dot(EscapeDir, right.normalized), Vector3.Dot(EscapeDir, forward.normalized)).normalized);
                    if (now - escapeStart >= 3f)
                    {
                        float gap = Enemy.DistanceToTarget;
                        Check($"a running player ({Fsm.locomotion.config.runSpeed} m/s) pulls away from its {Enemy.RunSpeed:F2} m/s chase (gap {escapeStartGap:F1} -> {gap:F1} m)", gap > escapeStartGap + 0.15f);
                        MoveProp.SetValue(Fsm.input, Vector2.zero);
                        Teleport(FarAway);
                        Next("hearing: once it's home, walk past 4.5 m behind it");
                    }
                }
                break;

            case 11: // walk behind, too quiet to hear
            case 12: // run behind, heard
                bool running = step == 12;
                if (!placed)
                {
                    if (path != "Idle") break;
                    placed = true;
                    stepStart = now;
                    heard = false;
                    Vector3 behind = Enemy.transform.position - Enemy.transform.forward * 4.5f - Enemy.transform.right * (running ? 3.5f : 1.4f);
                    Teleport(behind, Enemy.transform.right);
                    break;
                }
                // camera-relative stick along the enemy's right, past its back
                Transform camera = Fsm.locomotion.cameraTransform;
                Vector3 camRight = camera.right, camForward = camera.forward;
                camRight.y = camForward.y = 0f;
                Vector3 across = Enemy.transform.right;
                Vector2 stick = new Vector2(Vector3.Dot(across, camRight.normalized), Vector3.Dot(across, camForward.normalized)).normalized;
                MoveProp.SetValue(Fsm.input, stick * (running ? 1f : 0.3f));
                if (!running && path != "Idle") noticedWhileWalking = true;
                if (inStep > 2f)
                {
                    MoveProp.SetValue(Fsm.input, Vector2.zero);
                    if (!running)
                    {
                        Check($"doesn't hear the player walking 4.5 m behind it (walk noise {Fsm.GetComponent<PlayerNoise>().walkRadius} m)", !noticedWhileWalking);
                        Teleport(FarAway);
                        Next("run past 4.5 m behind it");
                    }
                    else
                    {
                        Check($"hears the player running 4.5 m behind it (run noise {Fsm.GetComponent<PlayerNoise>().runRadius} m) and turns to Alert", heard && path != "Idle");
                        return true;
                    }
                }
                break;
        }
        return false;
    }
}
