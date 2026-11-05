using UnityEngine;

// Enemy stagger and death: two hits, a third timed into the enemy's wind-up cancels its attack,
// then it's killed: lock-on drops, collider off, the corpse lies on the floor and it stays dead.
// Play mode only: player poise is raised so enemy hits don't interrupt the combo, and the enemy
// gets 150 HP so crits can't kill it before the stagger check.
public class EnemyStaggerDeathTest : PlayModeTest
{
    public override string Name => "EnemyStaggerDeath";

    static readonly System.Reflection.FieldInfo RepositionAttackAt = typeof(EnemyRepositionState).GetField("attackAt", Private);

    CombatStats enemyStats;
    int phase;
    float lastPress = -10f, pressAt = -1f, hitOffset = 0.76f, deadAt = -1f;
    float staggerStart = -1f, staggerEnd = -1f;
    bool locked, cancelled, cancelledSwingLanded, checkedDeath;
    string lastPath = "";

    protected override void Setup()
    {
        Dummy.enabled = false;
        enemyStats = Enemy.GetComponent<CombatStats>();
        PlayerStats.poiseBase = 100f;
        PlayerStats.RestoreFullHealth();
        enemyStats.vitality = 20;
        enemyStats.RestoreFullHealth();
        Teleport(Enemy.transform.position + Enemy.transform.forward * 5f);
    }

    protected override void OnLog(string line)
    {
        if (line.StartsWith("[Hit] Player") && line.Contains(": HIT") && lastPress > 0f) hitOffset = Time.time - lastPress;
        if (line.Contains("staggered, attack cancelled")) cancelled = true;
        if (cancelled && staggerEnd < 0f && line.StartsWith("[Hit] EnemyA")) cancelledSwingLanded = true;
        if (line.StartsWith("[Enemy]") || line.StartsWith("[Hit]"))
            Log($"{line}   | enemy {Enemy.StatePath} poise {enemyStats.CurrentPoise:F0}/{enemyStats.MaxPoise:F0} HP {enemyStats.Health}");
    }

    protected override bool Step(float now)
    {
        string path = Enemy.StatePath;
        if (path != lastPath)
        {
            Log($"STATE {lastPath} -> {path} (dist {Enemy.DistanceToTarget:F2})");
            if (path == "Staggered") staggerStart = now;
            if (lastPath == "Staggered") staggerEnd = now;
            lastPath = path;
        }
        if (PlayerStats.Health < 50) PlayerStats.RestoreFullHealth();

        bool ready = Fsm.State == CombatState.Locomotion && now - lastPress > 0.2f;
        if (!locked)
        {
            if (!path.StartsWith("Combat")) return false;
            LockOnto(Enemy.GetComponent<LockOnTarget>());
            locked = true;
            Log("--- locked on; land hits until the next one breaks poise");
        }
        if (path == "Dead" && phase < 3)
        {
            Check("survives until the stagger check", false);
            return true;
        }

        switch (phase)
        {
            case 0: // 25 -> 15 -> 5
                if (enemyStats.CurrentPoise > 10f)
                {
                    if (ready)
                    {
                        PressAttack();
                        lastPress = now;
                    }
                }
                else
                {
                    phase = 1;
                    Log("--- next hit breaks poise: timing it to land 0.3 s into a wind-up");
                }
                break;

            case 1:
                if (enemyStats.CurrentPoise > 10f)
                {
                    phase = 0;
                    pressAt = -1f;
                    Log("   poise reset before the timed hit, landing hits again");
                    break;
                }
                if (pressAt < 0f && ready && path == "Combat/Reposition")
                {
                    float attackAt = (float)RepositionAttackAt.GetValue(Enemy.Combat.Reposition);
                    float t = attackAt + 0.3f - hitOffset;
                    if (t > now) pressAt = t;
                }
                if (pressAt > 0f && now >= pressAt)
                {
                    PressAttack();
                    lastPress = now;
                    pressAt = -1f;
                    phase = 2;
                }
                break;

            case 2:
                if (!cancelled && now - lastPress > 1.5f)
                {
                    phase = 0;
                    Log("   timed hit didn't land in a wind-up, retrying");
                    break;
                }
                if (staggerEnd > 0f)
                {
                    float duration = staggerEnd - staggerStart;
                    Check("third hit during the wind-up staggers it and cancels the attack", cancelled);
                    Check("the cancelled swing never lands", !cancelledSwingLanded);
                    Check($"stagger lasts {Enemy.config.staggerTime}s ({duration:F2}s)", Mathf.Abs(duration - Enemy.config.staggerTime) < 0.1f);
                    phase = 3;
                    Log("--- attack until it dies");
                }
                break;

            case 3:
                if (path == "Dead")
                {
                    if (deadAt < 0f) deadAt = now;
                }
                else if (ready)
                {
                    PressAttack();
                    lastPress = now;
                }
                if (deadAt > 0f && now - deadAt > 0.2f && !checkedDeath)
                {
                    checkedDeath = true;
                    Check("death switches off its LockOnTarget and collider",
                        !Enemy.GetComponent<LockOnTarget>().enabled && !Enemy.GetComponent<Collider>().enabled);
                    Check("player lock-on drops the dead enemy", LockOn.Current == null);
                }
                // the death clip is 3.8 s and the fall comes near the end
                if (deadAt > 0f && now - deadAt > 4.5f)
                {
                    float hips = Enemy.Animator.GetBoneTransform(HumanBodyBones.Hips).position.y - Enemy.transform.position.y;
                    Check($"corpse lies on the floor (hips {hips:F2} m up)", hips < 0.35f);
                    Check("stays dead", path == "Dead");
                    return true;
                }
                break;
        }
        return false;
    }
}
