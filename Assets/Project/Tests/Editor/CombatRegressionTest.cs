using UnityEngine;

// Player combat against the TestDummy: input buffering, i-frame timing, stamina gating and regen,
// hit interrupts, poise absorb/break, dummy stagger, poise reset. Enemies are switched off.
public class CombatRegressionTest : PlayModeTest
{
    public override string Name => "CombatRegression";
    protected override float TimeLimit => 120f;

    static readonly System.Reflection.FieldInfo DummyNextAttack = typeof(TestDummyAttacker).GetField("nextAttack", Private);

    CombatStats dummyStats;
    int phase;
    float phaseStart, lastPress = -10f;
    bool seen;

    // measured from the log
    float dodgeStart = -1f, ifOn = -1f, ifOff = -1f, ifOnRel, ifOffRel, rollEnded = -1f;
    float windupAt = -1f, attackStart = -1f, hitOffset = -1f, cycle = -1f, lastAttackEnded = -1f;
    bool attackDropped, dodgeDropped, interrupted, dodged, hitLanded, absorbedMidCombo, windupCancelled;

    // stamina regen
    float dropStamina, regenStart = -1f, regenStartStamina;
    bool regenRateChecked;

    // timed dodges and the dummy combo
    float dodgeAt = -1f, comboStart = -1f, poiseHitAt = -1f;
    int comboAttacks;

    protected override void Setup()
    {
        dummyStats = Dummy.GetComponent<CombatStats>();
        Dummy.enabled = false;
        foreach (EnemyBrain enemy in Object.FindObjectsByType<EnemyBrain>())
            enemy.gameObject.SetActive(false);
    }

    protected override void OnLog(string line)
    {
        float now = Time.time;
        if (line.Contains("Locomotion -> Dodging")) dodgeStart = now;
        if (line.Contains("Dodging -> Locomotion")) rollEnded = now;
        if (line.Contains("i-frames on") && ifOn < 0f) ifOn = now;
        if (line.Contains("i-frames off") && ifOff < 0f) ifOff = now;
        if (line.Contains("AttackPressed dropped")) attackDropped = true;
        if (line.Contains("DodgePressed dropped")) dodgeDropped = true;
        if (line.Contains("winding up")) windupAt = now;
        if (line.Contains("Attacking -> Locomotion")) lastAttackEnded = now;
        if (phase == 2 && line.Contains("Locomotion -> Attacking")) attackStart = now;
        if (phase == 2 && line.Contains("no lock-on target") && hitOffset < 0f && attackStart > 0f) hitOffset = now - attackStart;
        if (phase == 2 && line.Contains("Attacking -> Locomotion") && cycle < 0f && attackStart > 0f) cycle = now - attackStart;
        if (phase == 5 && line.Contains("Attacking -> HitReact")) interrupted = true;
        if (line.Contains("DODGED")) dodged = true;
        if (line.StartsWith("[Hit] TestDummy") && line.Contains(": HIT")) hitLanded = true;
        if (phase == 10 && line.Contains("absorbed by poise, no interrupt (Attacking)")) absorbedMidCombo = true;
        if (line.Contains("staggered, wind-up cancelled")) windupCancelled = true;

        if (line.Contains("no lock-on target")) return;
        Log($"{line}   | ST {PlayerStamina.Current:F1} | {Fsm.State} | poise P {PlayerStats.CurrentPoise:F0} D {dummyStats.CurrentPoise:F0}");
    }

    void Go(int next, string title)
    {
        phase = next;
        phaseStart = Time.time;
        seen = false;
        dodgeAt = -1f;
        Log($"--- {title}");
    }

    // stand next to the dummy, facing it
    void Place()
    {
        Vector3 dummyPos = Dummy.transform.position;
        Vector3 dir = Fsm.transform.position - dummyPos;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.forward;
        Teleport(dummyPos + dir.normalized * 1.5f, -dir);
    }

    // dodge so the dummy's next hit lands `offset` seconds into the roll
    void TimedDodge(float now, float offset)
    {
        if (!seen && windupAt > phaseStart && Fsm.State == CombatState.Locomotion)
        {
            Place();
            dodgeAt = windupAt + Dummy.windUpTime - offset;
            seen = true;
            dodged = false;
            hitLanded = false;
            Log($"   dodge timed so the hit lands {offset:F3}s into the roll (i-frames {ifOnRel:F3}-{ifOffRel:F3}s)");
        }
        if (seen && dodgeAt > 0f && now >= dodgeAt)
        {
            PressDodge();
            dodgeAt = -1f;
        }
    }

    bool DummyHitResolved(float now) => seen && dodgeAt < 0f && now > windupAt + Dummy.windUpTime + 0.3f;

    protected override bool Step(float now)
    {
        if (phase > 0 && now - phaseStart > 25f)
        {
            Check($"phase {phase} finishes", false);
            return true;
        }

        switch (phase)
        {
            case 0:
                if (Fsm.State != CombatState.Locomotion) break;
                Go(1, "buffering: dodge, then attack pressed every 0.2 s during the roll");
                PressDodge();
                lastPress = now;
                break;

            case 1:
                if (Fsm.State == CombatState.Dodging) seen = true;
                if (seen && Fsm.State == CombatState.Dodging && now - lastPress >= 0.2f)
                {
                    PressAttack();
                    lastPress = now;
                }
                if (seen && Fsm.State == CombatState.Attacking)
                {
                    ifOnRel = ifOn - dodgeStart;
                    ifOffRel = ifOff - dodgeStart;
                    Check($"buffered attack starts right after the roll ({now - rollEnded:F3}s)", rollEnded > 0f && now - rollEnded < 0.1f);
                    Check($"i-frames open and close during the roll (+{ifOnRel:F3}s to +{ifOffRel:F3}s)", ifOnRel > 0f && ifOffRel > ifOnRel);
                    Go(2, "stamina: attack each time we're back in Locomotion until a press is dropped");
                }
                break;

            case 2:
                if (Fsm.State == CombatState.Locomotion && now - lastPress > 0.15f && !attackDropped)
                {
                    PressAttack();
                    lastPress = now;
                }
                if (attackDropped)
                {
                    PressDodge();
                    dropStamina = PlayerStamina.Current;
                    Go(3, "regen after the last action ends");
                }
                break;

            case 3:
                if (regenStart < 0f && PlayerStamina.Current > dropStamina + 0.01f)
                {
                    regenStart = now;
                    regenStartStamina = PlayerStamina.Current;
                }
                if (regenStart > 0f && !regenRateChecked && now - regenStart >= 1f)
                {
                    regenRateChecked = true;
                    float rate = (PlayerStamina.Current - regenStartStamina) / (now - regenStart);
                    Check("attack press dropped at <= 0 stamina", attackDropped);
                    Check("dodge press dropped at <= 0 stamina", dodgeDropped);
                    float delay = regenStart - lastAttackEnded;
                    Check($"regen starts {PlayerStamina.regenDelay}s after the action ends ({delay:F2}s)", Mathf.Abs(delay - PlayerStamina.regenDelay) < 0.15f);
                    Check($"regen rate ~{PlayerStamina.regenRate}/s ({rate:F1}/s)", Mathf.Abs(rate - PlayerStamina.regenRate) < 3f);
                    Log($"   measured: player hit lands +{hitOffset:F3}s into an attack, attack lasts {cycle:F3}s");
                    Go(4, "setup: wait for stamina, stand by the dummy");
                }
                break;

            case 4:
                if (PlayerStamina.Current >= 60f && Fsm.State == CombatState.Locomotion)
                {
                    Place();
                    PlayerStats.RestoreFullHealth();
                    Dummy.enabled = true;
                    Go(5, "hit interrupt: attack on the dummy's wind-up, its hit breaks player poise mid-swing");
                }
                break;

            case 5:
                if (windupAt > phaseStart && !seen)
                {
                    PressAttack();
                    seen = true;
                }
                if (seen && Fsm.State == CombatState.HitReact)
                {
                    Check("dummy hit breaks poise and interrupts the attack", interrupted);
                    Go(6, "i-frames INSIDE: hit lands mid-window");
                }
                break;

            case 6:
                TimedDodge(now, (ifOnRel + ifOffRel) / 2f);
                if (DummyHitResolved(now))
                {
                    Check("hit inside the i-frame window is dodged", dodged && !hitLanded);
                    Go(7, "i-frames BEFORE: hit lands 0.02 s into the roll");
                }
                break;

            case 7:
                TimedDodge(now, 0.02f);
                if (DummyHitResolved(now))
                {
                    Check("hit before the i-frame window lands", hitLanded && !dodged);
                    Go(8, "i-frames AFTER: hit lands 0.1 s after the window, roll still playing");
                }
                break;

            case 8:
                TimedDodge(now, ifOffRel + 0.1f);
                if (DummyHitResolved(now))
                {
                    Check("hit after the i-frame window lands", hitLanded && !dodged);
                    // raise player poise (play mode only) so the dummy's swing can't interrupt the combo
                    PlayerStats.poiseBase = 20f;
                    PlayerStats.RestoreFullHealth();
                    Go(9, "setup for the dummy stagger");
                }
                break;

            case 9:
                if (Fsm.State == CombatState.Locomotion && PlayerStats.CurrentPoise > 10f && PlayerStamina.Current >= PlayerStamina.Max - 0.1f)
                {
                    Place();
                    LockOnto(Dummy.GetComponent<LockOnTarget>());
                    // the third hit should land 0.3 s into a wind-up
                    float nextWindup = (float)DummyNextAttack.GetValue(Dummy);
                    float period = Dummy.attackInterval + Dummy.windUpTime;
                    comboStart = nextWindup + period + 0.3f - hitOffset - 2f * cycle;
                    while (comboStart < now + 0.05f) comboStart += period;
                    Go(10, "dummy stagger: 2 hits, the third lands during a wind-up");
                }
                break;

            case 10:
                if (comboAttacks == 0 && now >= comboStart)
                {
                    PressAttack();
                    comboAttacks = 1;
                    lastPress = now;
                }
                else if (comboAttacks > 0 && comboAttacks < 3 && Fsm.State == CombatState.Locomotion && now - lastPress > 0.15f)
                {
                    PressAttack();
                    comboAttacks++;
                    lastPress = now;
                }
                if (comboAttacks == 3 && Fsm.State == CombatState.Locomotion && now - lastPress > 2f)
                {
                    Check("dummy swing during the combo is absorbed, no interrupt", absorbedMidCombo);
                    Check("third hit in the wind-up staggers the dummy and cancels the wind-up", windupCancelled);
                    Go(11, "poise reset: one more hit, then wait");
                }
                break;

            case 11:
                if (!seen && Fsm.State == CombatState.Locomotion)
                {
                    PressAttack();
                    seen = true;
                }
                if (poiseHitAt < 0f && dummyStats.CurrentPoise < dummyStats.MaxPoise) poiseHitAt = now;
                if (poiseHitAt > 0f && dummyStats.CurrentPoise >= dummyStats.MaxPoise)
                {
                    float after = now - poiseHitAt;
                    Check($"dummy poise resets {dummyStats.poiseResetDelay}s after the last hit ({after:F2}s)", Mathf.Abs(after - dummyStats.poiseResetDelay) < 0.2f);
                    return true;
                }
                break;
        }
        return false;
    }
}
