using UnityEngine;

// Enemy B in Area01: retaliation, high poise, hyper armour and the sweep's wide arc.
// 1. three hits spread over more than retaliateWindow: no retaliation
// 2. three back-to-back slashes (mashing): the third makes it sweep straight away, and the sweep lands
// 3. hit and back off (pairs of hits): no retaliation, five hits don't stagger, the sixth does
// 4. a poise break during the heavy's wind-up doesn't cancel it (hyper armour), damage still applies, the heavy lands
// 5. the same poise break during the sweep does cancel it
// 6. the sweep's 120 deg arc catches a player 100 deg off to the side, the heavy's 45 deg doesn't
// Play mode only: the other enemies are switched off, Enemy B gets a config copy with zero attack weights
// so it only attacks when retaliating or forced, and its walk speed is zeroed so it stands and turns instead of
// strafing (the player can't turn mid-swing, so a strafe makes slashes miss at random), player poise is raised so the sweep doesn't interrupt
// the combos, and Enemy B is restored to full before each part so it can't die.
public class EnemyBTest : PlayModeTest
{
    public override string Name => "EnemyB";

    static readonly System.Reflection.FieldInfo WalkSpeedField = typeof(EnemyBrain).GetField("<WalkSpeed>k__BackingField", Private);

    EnemyBrain b;
    CombatStats bStats;
    AttackData heavy, sweep;
    int phase;
    float phaseStart, lastPress = -10f;
    int hits;                 // player hits landing on Enemy B this part
    float firstHitAt, lastHitAt, pairFirstHitAt;
    bool retaliated, hyperLogged, staggerCancelLogged, enemyHit;
    int hitsAtRetaliate;
    float retaliatedAt = -1f, hyperHitAt = -1f;
    bool checkedSweepStart, checkedHyper, started, checkedFive;
    string lastPath = "";

    protected override float TimeLimit => 120f;

    protected override void Setup()
    {
        b = GameObject.Find("EnemyB_Corridor").GetComponent<EnemyBrain>();
        bStats = b.GetComponent<CombatStats>();
        foreach (EnemyBrain e in Object.FindObjectsByType<EnemyBrain>())
            if (e != b) e.gameObject.SetActive(false);

        EnemyConfig config = Object.Instantiate(b.config);
        foreach (EnemyAttack a in config.attacks) a.weight = 0f;
        b.config = config;
        heavy = config.attacks[0].attack;
        sweep = config.attacks[config.retaliateAttack].attack;

        PlayerStats.poiseBase = 100f;
        PlayerStats.RestoreAll();
        Teleport(new Vector3(0f, 0f, 28.3f), Vector3.forward);
        Log($"--- Enemy B: {bStats.MaxHealth} HP, poise {bStats.MaxPoise}, retaliates on {config.retaliateHits} hits in {config.retaliateWindow} s with {sweep.name}");
    }

    protected override void OnLog(string line)
    {
        if (line.StartsWith("[Hit] Player, Target: EnemyB_Corridor") && line.Contains(": HIT"))
        {
            hits++;
            lastHitAt = Time.time;
            if (hits == 1) firstHitAt = Time.time;
            if (hits % 2 == 1) pairFirstHitAt = Time.time;
        }
        if (line.StartsWith("[Hit] EnemyB_Corridor, Target: Player") && line.Contains(": HIT")) enemyHit = true;
        if (line.Contains("EnemyB_Corridor retaliates"))
        {
            retaliated = true;
            retaliatedAt = Time.time;
            hitsAtRetaliate = hits + 1; // logged inside the damage call, before that hit's own [Hit] line
        }
        if (line.Contains("hyper armour keeps")) hyperLogged = true;
        if (line.Contains("EnemyB_Corridor staggered, attack cancelled")) staggerCancelLogged = true;
        if (line.StartsWith("[Enemy]") || line.StartsWith("[Hit]"))
            Log($"{line}   | B {b.StatePath} poise {bStats.CurrentPoise:F0}/{bStats.MaxPoise:F0} HP {bStats.Health}");
    }

    void NextPhase(int next, float now, string description)
    {
        phase = next;
        phaseStart = now;
        started = false;
        hits = 0;
        retaliated = false;
        enemyHit = false;
        Log("--- " + description);
    }

    // press attack whenever the previous swing is over, like holding the button down
    void AttackWhenReady(float now)
    {
        if (Fsm.State != CombatState.Locomotion || now - lastPress < 0.2f) return;
        PressAttack();
        lastPress = now;
    }

    protected override bool Step(float now)
    {
        string path = b.StatePath;
        if (path != lastPath)
        {
            Log($"STATE {lastPath} -> {path} (dist {b.DistanceToTarget:F2})");
            lastPath = path;
        }
        if (PlayerStats.Health < 50) PlayerStats.RestoreFullHealth();
        if (now - phaseStart > 25f)
        {
            Check($"part {phase} finishes in time", false);
            return true;
        }

        switch (phase)
        {
            case 0:
                if (!path.StartsWith("Combat")) break;
                WalkSpeedField.SetValue(b, 0f);
                Teleport(b.transform.position + b.transform.forward * 1.9f, -b.transform.forward);
                LockOnto(b.GetComponent<LockOnTarget>());
                bStats.RestoreAll();
                NextPhase(1, now, "1. three hits spread wider than the window");
                break;

            case 1:
                if (hits < 2) AttackWhenReady(now);
                // the third has to land more than the window after the first
                else if (hits == 2 && now - firstHitAt > b.config.retaliateWindow - 0.3f) AttackWhenReady(now);
                if (hits == 3 && now - lastHitAt > 0.3f)
                {
                    Check($"3 hits over {lastHitAt - firstHitAt:F1} s (window {b.config.retaliateWindow} s) don't trigger retaliation", !retaliated && path != "Combat/Attack");
                    Check("3 slashes don't stagger it", path != "Staggered");
                    NextPhase(2, now, "2. mashing: back-to-back slashes");
                }
                break;

            case 2:
                // let part 1's hits age out of the window first
                if (now - phaseStart < b.config.retaliateWindow + 0.2f) break;
                if (hits == 0 && Mathf.Abs(bStats.CurrentPoise - bStats.MaxPoise) > 0.1f) bStats.RestoreAll();
                if (!retaliated && hits < 3) AttackWhenReady(now);
                if (retaliated && !checkedSweepStart && now - retaliatedAt > 0.1f)
                {
                    checkedSweepStart = true;
                    Check($"the {b.config.retaliateHits}rd mashed hit triggers retaliation (on hit {hitsAtRetaliate}, {lastHitAt - firstHitAt:F1} s apart)", hitsAtRetaliate == b.config.retaliateHits);
                    Check($"it goes straight into the sweep ({path}, {b.config.attacks[b.Combat.Attack.attackIndex].attack.name})",
                        path == "Combat/Attack" && b.Combat.Attack.attackIndex == b.config.retaliateAttack);
                }
                if (checkedSweepStart && (enemyHit || now - retaliatedAt > 2.5f))
                {
                    Check("the retaliation sweep lands on the player", enemyHit);
                    NextPhase(3, now, "3. hit and back off in pairs until it staggers");
                }
                else if (!retaliated && hits >= 3 && now - lastHitAt > 0.5f)
                {
                    Check("mashing triggers retaliation", false);
                    return true;
                }
                break;

            case 3:
                if (!started)
                {
                    if (path != "Combat/Reposition") break; // wait out the sweep and its recovery
                    bStats.RestoreAll();
                    started = true;
                }
                if (retaliated)
                {
                    Check("hitting in pairs and backing off never triggers retaliation", false);
                    return true;
                }
                // a new pair starts once the last pair's first hit will be out of the window when this one lands
                bool pairDone = hits % 2 == 0;
                if (hits < 6 && (!pairDone || hits == 0 || now - pairFirstHitAt > b.config.retaliateWindow - 0.45f))
                    AttackWhenReady(now);
                if (hits == 5 && !checkedFive && now - lastHitAt > 0.1f)
                {
                    checkedFive = true;
                    Check($"5 slashes don't stagger it (poise {bStats.CurrentPoise:F0}/{bStats.MaxPoise:F0})", path != "Staggered");
                }
                if (hits == 6 && now - lastHitAt > 0.1f)
                {
                    Check("the 6th slash staggers it", path == "Staggered");
                    Check("no retaliation while hitting in pairs", !retaliated);
                    NextPhase(4, now, "4. hyper armour: break poise during the heavy's wind-up");
                }
                break;

            case 4:
                if (hyperHitAt < 0f)
                {
                    if (path != "Combat/Reposition" || Fsm.State != CombatState.Locomotion) break;
                    bStats.RestoreAll();
                    PlayerStamina.Refill(); // part 3's slashes can leave it at 0, which drops the press
                    bStats.TakePoiseDamage(bStats.MaxPoise - 5f); // one slash from breaking
                    b.Combat.ForceAttack(0);
                    PressAttack();
                    lastPress = now;
                    hyperHitAt = now;
                    hits = 0;
                    Log($"   forced {heavy.name}, slash pressed to land in its wind-up");
                    break;
                }
                if (hits == 1 && !checkedHyper && now - lastHitAt > 0.1f)
                {
                    checkedHyper = true;
                    Check("a poise break in the heavy's wind-up is absorbed by hyper armour", hyperLogged);
                    Check($"the heavy keeps going ({path})", path == "Combat/Attack");
                    Check($"the slash's damage still applies (HP {bStats.Health}/{bStats.MaxHealth})", bStats.Health < bStats.MaxHealth);
                }
                if (checkedHyper && (enemyHit || path != "Combat/Attack"))
                {
                    Check("the heavy still lands", enemyHit);
                    NextPhase(5, now, "5. the same poise break during the sweep cancels it");
                }
                else if (hits == 0 && now - hyperHitAt > 2f)
                {
                    Check($"the slash lands during the heavy's wind-up (player {Fsm.State}, stamina {PlayerStamina.Current:F0}, lock {LockOn.Current})", false);
                    return true;
                }
                break;

            case 5:
                if (path != "Combat/Reposition") break; // wait out the heavy and its recovery
                bStats.RestoreAll();
                b.Combat.ForceAttack(b.config.retaliateAttack);
                phase = 51;
                phaseStart = now;
                break;

            case 51:
                if (now - phaseStart < 0.2f) break;
                bStats.TakePoiseDamage(999f);
                Check("a poise break during the sweep (no hyper armour) staggers it and cancels the swing", staggerCancelLogged && b.StatePath == "Staggered");
                NextPhase(6, now, "6. sweep arc vs heavy arc, player 100 deg off to the side");
                break;

            case 6:
                Vector3 side = Quaternion.Euler(0f, 100f, 0f) * b.transform.forward;
                Teleport(b.transform.position + side * 2f);
                PlayerStats.RestoreAll();
                b.Resolver.BeginSwing(sweep);
                HitResult withSweep = b.Resolver.Resolve(PlayerStats);
                b.Resolver.BeginSwing(heavy);
                HitResult withHeavy = b.Resolver.Resolve(PlayerStats);
                Check($"{sweep.name} ({sweep.halfAngle} deg) hits at 100 deg ({withSweep})", withSweep == HitResult.Hit || withSweep == HitResult.HitPoiseBroken);
                Check($"{heavy.name} ({heavy.halfAngle} deg) misses at 100 deg ({withHeavy})", withHeavy == HitResult.OutOfReach);
                return true;
        }
        return false;
    }
}
