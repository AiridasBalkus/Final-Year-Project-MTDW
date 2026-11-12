using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

// Block and heal against the TestDummy (enemies off): blocked, unblocked and guard-broken hits,
// healing, an interrupted heal, refusals and the HUD pips. Input is faked like PlayerLocomotion.
public class BlockHealTest : PlayModeTest
{
    public override string Name => "BlockHeal";

    static readonly PropertyInfo MoveProp = typeof(PlayerInputHandler).GetProperty("Move");
    static readonly PropertyInfo BlockProp = typeof(PlayerInputHandler).GetProperty("BlockHeld");
    static readonly PropertyInfo ChargesProp = typeof(HealFlask).GetProperty("Charges");
    static readonly MethodInfo HealMethod = typeof(PlayerCombatFSM).GetMethod("OnHealPressed", Private);
    static readonly FieldInfo DummyNextAttack = typeof(TestDummyAttacker).GetField("nextAttack", Private);
    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
    static readonly Regex BlockedLine = new Regex(@"(BLOCKED|GUARD BROKEN) (\d+) \(([\d.]+) × ([\d.]+).*stamina (-?\d+) - (\d+) → (-?\d+)");
    static readonly Vector3 OpenFloor = new Vector3(-6f, 0f, -6f);

    const float HealApplyDelay = 0.89f; // HealApply at 60% of PowerUp (2.37 s) played at 1.6x

    Guard guard;
    HealFlask flask;
    ShieldFeedback shield;
    HealChargesDisplay pips;
    CharacterController controller;

    int phase;
    float phaseStart, sub = 0f;
    bool started, swung;
    string hitLine;
    float hitAt = -1f, healedAt = -1f, guardBrokenAt = -1f, guardBrokenEnd = -1f;
    bool sawHitReact, sawGuardBrokenLog, sawChargeLost, sawNoCharges, sawNoStamina;
    bool chargeChecked;

    // phase-local measurements
    float speedSum, staminaA;
    int samples, healthBefore, healthMid, chargesBefore;
    float poiseBefore;
    Color flashColor;

    protected override void Setup()
    {
        Dummy.enabled = false;
        foreach (EnemyBrain enemy in Object.FindObjectsByType<EnemyBrain>())
            enemy.gameObject.SetActive(false);

        guard = Fsm.GetComponent<Guard>();
        flask = Fsm.GetComponent<HealFlask>();
        shield = Object.FindAnyObjectByType<ShieldFeedback>();
        pips = Object.FindAnyObjectByType<HealChargesDisplay>();
        controller = Fsm.GetComponent<CharacterController>();
        Fsm.input.enabled = false;
        Go(0, "block while walking: walk speed, regen slowed");
    }

    protected override void OnLog(string line)
    {
        float now = Time.time;
        if (line.StartsWith("[Hit] TestDummy")) { hitLine = line; hitAt = now; }
        if (line.Contains("-> HitReact")) sawHitReact = true;
        if (line.Contains("[FSM] guard broken")) sawGuardBrokenLog = true;
        if (line.Contains("-> GuardBroken")) guardBrokenAt = now;
        if (line.Contains("GuardBroken ->")) guardBrokenEnd = now;
        if (line.Contains("[FSM] healed")) healedAt = now;
        if (line.Contains("charge lost")) sawChargeLost = true;
        if (line.Contains("HealPressed dropped") && line.Contains("no charges")) sawNoCharges = true;
        if (line.Contains("HealPressed dropped") && line.Contains("no stamina")) sawNoStamina = true;
        if (line.StartsWith("[Hit]") || line.StartsWith("[Dummy]") || (line.StartsWith("[FSM]") && !line.Contains("queued")))
            Log($"{line}   | {Fsm.State} HP {PlayerStats.Health} ST {PlayerStamina.Current:F0} charges {flask.Charges}");
    }

    void Go(int next, string title)
    {
        phase = next;
        phaseStart = Time.time;
        started = swung = false;
        hitLine = null;
        hitAt = healedAt = guardBrokenAt = guardBrokenEnd = -1f;
        sawHitReact = sawGuardBrokenLog = sawChargeLost = false;
        speedSum = 0f;
        samples = 0;
        Log($"--- {title}");
    }

    void SetInput(Vector2 move, bool block)
    {
        MoveProp.SetValue(Fsm.input, move);
        BlockProp.SetValue(Fsm.input, block);
    }

    // in front of the dummy, facing it (or facing away)
    void PlaceAtDummy(bool facingDummy)
    {
        Vector3 forward = Dummy.transform.forward;
        forward.y = 0f;
        forward.Normalize();
        Vector3 dummyPos = Dummy.transform.position;
        dummyPos.y = 0f;
        Teleport(dummyPos + forward * 1.5f, facingDummy ? -forward : forward);
    }

    // the dummy winds up now and swings windUpTime later
    void Swing()
    {
        Dummy.enabled = true;
        DummyNextAttack.SetValue(Dummy, Time.time);
        swung = true;
    }

    float FlatSpeed()
    {
        Vector3 v = controller.velocity;
        v.y = 0f;
        return v.magnitude;
    }

    Color ShieldGlow() => shield.renderers[0].material.GetColor(EmissionId);

    protected override bool Step(float now)
    {
        float t = now - phaseStart;
        if (t > 20f)
        {
            Check($"phase {phase} finishes", false);
            return true;
        }
        if (PlayerStats.Health < 30 && phase < 4) PlayerStats.RestoreFullHealth();

        switch (phase)
        {
            case 0: // walk forward with the block held, then stand and measure regen
                if (!started)
                {
                    started = true;
                    Teleport(OpenFloor, Vector3.forward);
                    PlayerStamina.Spend(PlayerStamina.Current - 40f);
                }
                SetInput(t < 1.6f ? new Vector2(0f, 1f) : Vector2.zero, true);
                if (t > 0.8f && t < 1.6f)
                {
                    speedSum += FlatSpeed();
                    samples++;
                }
                if (staminaA == 0f && t >= 1.8f) { staminaA = PlayerStamina.Current; sub = now; }
                if (t >= 3.3f)
                {
                    float speed = speedSum / samples;
                    float walk = Fsm.locomotion.config.walkSpeed;
                    float rate = (PlayerStamina.Current - staminaA) / (now - sub);
                    float expected = PlayerStamina.regenRate * guard.regenMultiplier;
                    Check($"blocking moves at walk speed {walk} m/s ({speed:F2}), even with run input", Fsm.State == CombatState.Blocking && Mathf.Abs(speed - walk) <= walk * 0.05f);
                    Check($"regen while blocking is x{guard.regenMultiplier} = {expected:F1}/s ({rate:F1}/s)", Mathf.Abs(rate - expected) < 1.5f);
                    staminaA = 0f;
                    SetInput(Vector2.zero, false);
                    Go(1, "blocked hit in front");
                }
                break;

            case 1:
                if (!started && Fsm.State == CombatState.Locomotion && PlayerStamina.Current > 60f)
                {
                    started = true;
                    PlaceAtDummy(true);
                    SetInput(Vector2.zero, true);
                    sub = now;
                }
                if (started && !swung && now - sub > 0.4f)
                {
                    poiseBefore = PlayerStats.CurrentPoise;
                    healthBefore = PlayerStats.Health;
                    Swing();
                }
                if (hitAt > 0f && flashColor == default && now - hitAt > 0.03f) flashColor = ShieldGlow();
                if (hitAt > 0f && now - hitAt > 0.4f)
                {
                    Dummy.enabled = false;
                    Match m = BlockedLine.Match(hitLine);
                    bool blocked = m.Success && m.Groups[1].Value == "BLOCKED";
                    Check($"a hit in front while blocking is Blocked ({hitLine.Substring(hitLine.IndexOf(": ") + 2)})", blocked);
                    if (blocked)
                    {
                        int damage = int.Parse(m.Groups[2].Value);
                        float raw = float.Parse(m.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                        int cost = int.Parse(m.Groups[6].Value);
                        int stBefore = int.Parse(m.Groups[5].Value), stAfter = int.Parse(m.Groups[7].Value);
                        int expectedDamage = Mathf.Max(Mathf.RoundToInt(raw * (1f - guard.blockReduction)), 1);
                        Check($"blocked damage is {1f - guard.blockReduction:P0} of the hit ({damage} of {raw:F1}), HP {healthBefore} -> {PlayerStats.Health}", damage == expectedDamage && healthBefore - PlayerStats.Health == damage);
                        Check($"blocking costs the attack's blockStaminaCost ({stBefore} - {cost} -> {stAfter})", cost == Mathf.RoundToInt(Dummy.GetComponent<HitResolver>().attack.blockStaminaCost) && stBefore - cost == stAfter);
                    }
                    Check($"no poise damage on a block ({poiseBefore:F0} -> {PlayerStats.CurrentPoise:F0})", Mathf.Approximately(poiseBefore, PlayerStats.CurrentPoise));
                    Check("a blocked hit doesn't interrupt, still Blocking", Fsm.State == CombatState.Blocking && !sawHitReact);
                    Check($"the shield flashes on the block (emission {flashColor.maxColorComponent:F2})", flashColor.maxColorComponent > 0.5f);
                    flashColor = default;
                    SetInput(Vector2.zero, false);
                    Go(2, "hit from behind while blocking");
                }
                break;

            case 2:
                if (!started && Fsm.State == CombatState.Locomotion)
                {
                    started = true;
                    PlaceAtDummy(false);
                    SetInput(Vector2.zero, true);
                    sub = now;
                }
                if (started && !swung && now - sub > 0.4f) Swing();
                if (hitAt > 0f && now - hitAt > 0.2f)
                {
                    Dummy.enabled = false;
                    Check($"blocking doesn't cover the back, the hit lands ({hitLine.Substring(hitLine.IndexOf(": ") + 2)})", hitLine.Contains(": HIT"));
                    SetInput(Vector2.zero, false);
                    Go(3, "guard break: blocked hit with 10 stamina left");
                }
                break;

            case 3:
                if (!started && (Fsm.State == CombatState.Locomotion) && PlayerStamina.Current > 30f && now - phaseStart > 1f)
                {
                    started = true;
                    PlaceAtDummy(true);
                    SetInput(Vector2.zero, true);
                    sub = now;
                }
                if (started && !swung && now - sub > 0.4f)
                {
                    Swing();
                    PlayerStamina.Spend(PlayerStamina.Current - 10f); // regen stays off until after the hit
                }
                if (guardBrokenAt > 0f && flashColor == default && now - guardBrokenAt > 0.03f) flashColor = ShieldGlow();
                if (guardBrokenEnd > 0f)
                {
                    Dummy.enabled = false;
                    float duration = guardBrokenEnd - guardBrokenAt;
                    Check($"a blocked hit taking stamina below 0 breaks the guard ({hitLine.Substring(hitLine.IndexOf(": ") + 2)})", hitLine.Contains("GUARD BROKEN") && sawGuardBrokenLog);
                    Check($"GuardBroken lasts guardBreakTime {guard.guardBreakTime}s ({duration:F2}s)", Mathf.Abs(duration - guard.guardBreakTime) < 0.1f);
                    Check($"the shield flashes red on a guard break ({flashColor})", flashColor.r > 1f && flashColor.r > flashColor.g * 3f);
                    SetInput(Vector2.zero, false);
                    Go(4, "heal while walking");
                }
                break;

            case 4:
                if (!started && Fsm.State == CombatState.Locomotion && PlayerStamina.Current > 40f)
                {
                    started = true;
                    Teleport(OpenFloor, Vector3.forward);
                    PlayerStats.RestoreFullHealth();
                    PlayerStats.TakeDamage(60);
                    healthBefore = PlayerStats.Health;
                    chargesBefore = flask.Charges;
                    sub = now;
                    HealMethod.Invoke(Fsm, null);
                }
                if (!started) break;
                SetInput(new Vector2(0f, 1f), false);
                if (!chargeChecked && now - sub > 0.05f)
                {
                    chargeChecked = true;
                    Check($"the heal uses a charge as it starts ({chargesBefore} -> {flask.Charges})", flask.Charges == chargesBefore - 1 && Fsm.State == CombatState.Healing);
                }
                if (now - sub > 0.5f && now - sub < 0.8f)
                {
                    speedSum += FlatSpeed();
                    samples++;
                }
                if (now - sub > 0.75f && now - sub < 0.8f) healthMid = PlayerStats.Health; // before HealApply
                if (Fsm.State == CombatState.Locomotion && now - sub > 1f)
                {
                    float delay = healedAt - sub;
                    float speed = speedSum / samples;
                    int expectedHealth = Mathf.Min(healthBefore + flask.healAmount, PlayerStats.MaxHealth);
                    Check($"no health before HealApply (HP {healthMid} at 0.75s)", healthMid == healthBefore);
                    Check($"HealApply restores {flask.healAmount} HP {HealApplyDelay}s in ({healthBefore} -> {PlayerStats.Health} at {delay:F2}s)", healedAt > 0f && PlayerStats.Health == expectedHealth && Mathf.Abs(delay - HealApplyDelay) < 0.1f);
                    Check($"moves at walk speed while healing ({speed:F2} m/s)", Mathf.Abs(speed - Fsm.locomotion.config.walkSpeed) <= Fsm.locomotion.config.walkSpeed * 0.05f);
                    SetInput(Vector2.zero, false);
                    Go(5, "heal interrupted by a poise break before HealApply");
                }
                break;

            case 5:
                if (!started && Fsm.State == CombatState.Locomotion && PlayerStamina.Current > 40f && now - phaseStart > 3.2f)
                {
                    // waited for poise to reset, so the dummy's swing breaks it
                    started = true;
                    PlaceAtDummy(true);
                    sub = now;
                }
                if (started && !swung && now - sub > 0.3f)
                {
                    Swing();
                    healthBefore = PlayerStats.Health;
                    chargesBefore = flask.Charges;
                    HealMethod.Invoke(Fsm, null); // the hit lands windUpTime later, before HealApply
                }
                if (hitAt > 0f && now - hitAt > 1.2f)
                {
                    Dummy.enabled = false;
                    Check($"a poise break before HealApply interrupts the heal ({hitLine.Substring(hitLine.IndexOf(": ") + 2)})", hitLine.Contains("BROKEN") && sawHitReact && sawChargeLost);
                    Check($"the interrupted heal still used its charge ({chargesBefore} -> {flask.Charges}) and restored nothing", flask.Charges == chargesBefore - 1 && healedAt < 0f && PlayerStats.Health < healthBefore);
                    Go(6, "use the last charge, then press heal with 0 charges");
                }
                break;

            case 6:
                if (!started && Fsm.State == CombatState.Locomotion && PlayerStamina.Current > 30f)
                {
                    started = true;
                    Teleport(OpenFloor, Vector3.forward);
                    HealMethod.Invoke(Fsm, null);
                    sub = now;
                }
                if (started && !swung && Fsm.State == CombatState.Locomotion && now - sub > 1.6f)
                {
                    swung = true; // reused as "pressed again"
                    sawNoCharges = false;
                    healthBefore = PlayerStats.Health;
                    HealMethod.Invoke(Fsm, null);
                    sub = now;
                }
                if (swung && now - sub > 0.3f)
                {
                    Check($"heal is refused with 0 charges (charges {flask.Charges})", flask.Charges == 0 && sawNoCharges && Fsm.State == CombatState.Locomotion);
                    Go(7, "press heal with a charge but 0 stamina");
                }
                break;

            case 7:
                if (!started)
                {
                    started = true;
                    ChargesProp.SetValue(flask, 1);
                    PlayerStamina.Spend(PlayerStamina.Current);
                    sawNoStamina = false;
                    HealMethod.Invoke(Fsm, null);
                    sub = now;
                }
                if (now - sub > 0.3f)
                {
                    Check($"heal is refused with 0 stamina (stamina {PlayerStamina.Current:F0}, charges {flask.Charges})", sawNoStamina && flask.Charges == 1 && Fsm.State == CombatState.Locomotion);
                    int lit = 0;
                    foreach (UnityEngine.UI.Image pip in pips.pips)
                        if (pip.color == pips.fullColor) lit++;
                    Check($"HUD shows the charges left ({lit} of {pips.pips.Length} pips lit for {flask.Charges})", lit == flask.Charges);
                    return true;
                }
                break;
        }
        return false;
    }
}
