using System.Collections.Generic;
using UnityEngine;

// Enemy combat loop against a standing player: Reposition -> Attack -> Recover, wind-up glow on and off,
// hits landing, then a dodge inside the i-frame window (dodged) and one just before it (hit).
// Swing speeds stay in their AttackData range and the hit lands at (1x hit time) / speed.
public class EnemyCombatDodgeTest : PlayModeTest
{
    public override string Name => "EnemyCombatDodge";

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    Renderer[] glowRenderers;
    readonly Dictionary<string, float> hitTimes = new Dictionary<string, float>(); // seconds from attack start to AttackHit, at speed 1
    readonly HashSet<string> statesSeen = new HashSet<string>();

    int mode; // 0 watch attacks, 1 dodge inside window, 2 dodge before window
    int attacks, hitsLanded;
    float attackAt = -1f, dodgeAt = -1f, glowOnCheckAt = -1f, glowOffCheckAt = -1f;
    string attackName = "";
    float attackSpeed = 1f;
    bool glowOnOk = true, glowOffOk = true, dodgeScheduled, dodged, hit;
    bool speedsInRange = true, hitTimingScales = true;
    int timingChecks;
    float worstTimingError;
    string lastPath = "";

    protected override void Setup()
    {
        Dummy.enabled = false;
        glowRenderers = Enemy.GetComponent<EnemyWindUpCue>().renderers;
        Teleport(Enemy.transform.position + Enemy.transform.forward * 6f);
    }

    protected override void OnLog(string line)
    {
        float now = Time.time;
        if (line.StartsWith("[Enemy]") && line.Contains(" attack "))
        {
            // "[Enemy] EnemyA attack EnemyA_Slash x1.06"
            string[] parts = line.Split(' ');
            attackAt = now;
            attackName = parts[parts.Length - 2];
            attackSpeed = float.Parse(parts[parts.Length - 1].Substring(1), System.Globalization.CultureInfo.InvariantCulture);
            AttackData data = FindAttack(attackName);
            if (attackSpeed < data.speedMin - 0.005f || attackSpeed > data.speedMax + 0.005f) speedsInRange = false;
            attacks++;
            glowOnCheckAt = now + 0.35f;
            dodgeScheduled = false;
            dodged = false;
            hit = false;
            if (mode > 0 && hitTimes.TryGetValue(attackName, out float hitTime))
            {
                float into = mode == 1 ? 0.3f : 0.02f; // seconds into the roll the hit should land
                dodgeAt = now + hitTime / attackSpeed - into;
                dodgeScheduled = true;
                Log($"   dodge timed so the {attackName} (x{attackSpeed:F2}) lands {into:F2}s into the roll");
            }
        }
        if (line.StartsWith("[Hit] EnemyA"))
        {
            float took = now - attackAt;
            if (!hitTimes.TryGetValue(attackName, out float baseTime))
            {
                hitTimes[attackName] = took * attackSpeed;
            }
            else
            {
                float error = Mathf.Abs(took - baseTime / attackSpeed);
                worstTimingError = Mathf.Max(worstTimingError, error);
                timingChecks++;
                if (error > 0.05f) hitTimingScales = false;
            }
            glowOffCheckAt = now + 0.1f;
            if (line.Contains("DODGED")) dodged = true;
            if (line.Contains(": HIT"))
            {
                hit = true;
                if (mode == 0) hitsLanded++;
            }
        }
        if (line.StartsWith("[Enemy]") || line.StartsWith("[Hit] EnemyA") || line.Contains("i-frames") || line.Contains("-> Dodging") || line.Contains("-> HitReact"))
            Log($"{line}   | enemy {Enemy.StatePath}");
    }

    AttackData FindAttack(string name)
    {
        foreach (EnemyAttack a in Enemy.config.attacks)
            if (a.attack.name == name) return a.attack;
        return null;
    }

    bool Glowing()
    {
        foreach (Renderer r in glowRenderers)
            if (r.material.GetColor(EmissionId).maxColorComponent <= 0.01f) return false;
        return true;
    }

    protected override bool Step(float now)
    {
        if (Enemy.StatePath != lastPath)
        {
            Log($"STATE {lastPath} -> {Enemy.StatePath} (dist {Enemy.DistanceToTarget:F2})");
            lastPath = Enemy.StatePath;
            statesSeen.Add(lastPath);
        }
        if (PlayerStats.Health < 50) PlayerStats.RestoreFullHealth();

        if (glowOnCheckAt > 0f && now >= glowOnCheckAt)
        {
            glowOnCheckAt = -1f;
            if (!Glowing()) glowOnOk = false;
        }
        if (glowOffCheckAt > 0f && now >= glowOffCheckAt)
        {
            glowOffCheckAt = -1f;
            if (Glowing()) glowOffOk = false;
        }
        if (dodgeAt > 0f && now >= dodgeAt)
        {
            // roll towards the enemy so the swing stays in reach and only the timing decides
            Teleport(Fsm.transform.position, Enemy.transform.position - Fsm.transform.position);
            PressDodge();
            dodgeAt = -1f;
        }

        bool attackOver = attackAt > 0f && now > attackAt + 1.8f && Fsm.State == CombatState.Locomotion;
        switch (mode)
        {
            case 0:
                if (attacks >= 3 && attackOver)
                {
                    Check("goes Reposition -> Attack -> Recover", statesSeen.Contains("Combat/Reposition") && statesSeen.Contains("Combat/Attack") && statesSeen.Contains("Combat/Recover"));
                    Check($"its attacks land on a standing player ({hitsLanded}/3)", hitsLanded >= 3);
                    Check("body and blade glow during each wind-up", glowOnOk);
                    Check("glow switches off at the hit", glowOffOk);
                    mode = 1;
                    dodgeScheduled = false;
                    Log("--- next attack: dodge so the hit lands inside the i-frame window");
                }
                break;

            case 1:
                if (dodgeScheduled && attackOver)
                {
                    Check("dodging inside the i-frame window avoids the hit", dodged && !hit);
                    mode = 2;
                    dodgeScheduled = false;
                    Log("--- next attack: dodge too early, the hit lands before the window opens");
                }
                break;

            case 2:
                if (dodgeScheduled && attackOver)
                {
                    Check("dodging too early still gets hit", hit && !dodged);
                    Check("every swing's speed is inside its AttackData range", speedsInRange);
                    Check($"hit lands at (1x time) / speed ({timingChecks} swings, worst {worstTimingError:F3}s off)", timingChecks > 0 && hitTimingScales);
                    return true;
                }
                break;
        }
        return false;
    }
}
