using System.Reflection;
using UnityEngine;

// Player movement: gait speeds, no foot sliding (also when turning), roll distance, lock-on strafe
// speed and the slash lunge. Input is faked by switching the input handler off.
public class PlayerLocomotionTest : PlayModeTest
{
    public override string Name => "PlayerLocomotion";

    static readonly PropertyInfo MoveProp = typeof(PlayerInputHandler).GetProperty("Move");
    static readonly PropertyInfo SprintProp = typeof(PlayerInputHandler).GetProperty("SprintHeld");

    const float RollClipDistance = 3.13f; // measured Dodge_Roll travel at scale 1
    static readonly Vector3 OpenFloor = new Vector3(-6f, 0f, -6f); // 12 m clear in every direction
    const float SegmentTime = 1.8f;
    const float MeasureFrom = 1f;         // skip the turn and acceleration at the start of a segment

    struct Gait
    {
        public string name;
        public float stick, speed;
        public bool sprint;
    }

    PlayerInputHandler input;
    PlayerLocomotion locomotion;
    Animator animator;
    CharacterController controller;
    Gait[] gaits;

    int phase, gaitIndex;
    float phaseStart, speedSum, clipSpeedSum, worstSlide;
    int samples;
    Vector3 rollStart, rollForward;
    bool rolled, strafeSetUp, swung;
    HitResolver resolver;
    float firstMove = -1f, lastMove = -1f, lungeTimeAt1 = -1f, clipStep, lungeDistance;

    protected override void Setup()
    {
        Dummy.enabled = false;
        foreach (EnemyBrain enemy in Object.FindObjectsByType<EnemyBrain>())
            enemy.gameObject.SetActive(false);

        input = Fsm.input;
        locomotion = Fsm.locomotion;
        animator = locomotion.GetComponent<Animator>();
        controller = locomotion.GetComponent<CharacterController>();
        input.enabled = false;
        // the spawn has the dummy 3 m in front
        Teleport(OpenFloor, Vector3.forward);
        // a play-mode copy so changing the roll scale below doesn't touch the asset
        locomotion.config = Object.Instantiate(locomotion.config);

        PlayerMovementConfig c = locomotion.config;
        gaits = new[]
        {
            new Gait { name = "walk", stick = 0.3f, speed = c.walkSpeed },
            new Gait { name = "run", stick = 1f, speed = c.runSpeed },
            new Gait { name = "sprint", stick = 1f, speed = c.sprintSpeed, sprint = true },
        };
        phaseStart = Time.time;
        Log($"--- gaits: walk {c.walkSpeed} run {c.runSpeed} sprint {c.sprintSpeed} m/s");
    }

    void SetInput(Vector2 move, bool sprint)
    {
        MoveProp.SetValue(input, move);
        SprintProp.SetValue(input, sprint);
    }

    protected override bool Step(float now)
    {
        float t = now - phaseStart;
        switch (phase)
        {
            case 0: // each gait forward then back, so the player stays near the start
                if (gaitIndex < gaits.Length)
                {
                    Gait g = gaits[gaitIndex];
                    int segment = t < SegmentTime ? 0 : 1;
                    SetInput(new Vector2(0f, segment == 0 ? g.stick : -g.stick), g.sprint);

                    // sideways, or backwards while turning
                    Vector3 vel = controller.velocity;
                    vel.y = 0f;
                    Vector3 local = locomotion.transform.InverseTransformDirection(vel);
                    worstSlide = Mathf.Max(worstSlide, Mathf.Abs(local.x), -local.z);

                    float inSegment = t - segment * SegmentTime;
                    if (inSegment >= MeasureFrom)
                    {
                        Vector3 v = controller.velocity;
                        v.y = 0f;
                        speedSum += v.magnitude;
                        Vector3 clip = animator.velocity;
                        clip.y = 0f;
                        clipSpeedSum += Vector3.Dot(clip, v.normalized); // along the movement, clips sway sideways
                        samples++;
                    }

                    if (t >= 2f * SegmentTime)
                    {
                        float speed = speedSum / samples;
                        float ratio = clipSpeedSum / speedSum;
                        Check($"{g.name} moves at {g.speed:F2} m/s ({speed:F2})", Mathf.Abs(speed - g.speed) <= g.speed * 0.05f);
                        Check($"{g.name} animation matches the speed, no sliding (clip/actual {ratio:F2})", ratio > 0.85f && ratio < 1.15f);
                        Check($"{g.name} turns around without sliding sideways or backwards (worst {worstSlide:F2} m/s)", worstSlide < 0.2f);
                        worstSlide = 0f;
                        gaitIndex++;
                        speedSum = clipSpeedSum = 0f;
                        samples = 0;
                        phaseStart = now;
                    }
                }
                else
                {
                    SetInput(Vector2.zero, false);
                    if (t > 0.6f)
                    {
                        phase = 1;
                        phaseStart = now;
                        Log("--- roll at scale 1, then at 1.25");
                    }
                }
                break;

            case 1:
            case 2:
                float scale = phase == 1 ? 1f : 1.25f;
                if (!rolled && Fsm.State == CombatState.Locomotion && t > 0.3f)
                {
                    locomotion.config.dodgeDistanceScale = scale;
                    rollStart = locomotion.transform.position;
                    rollForward = locomotion.transform.forward;
                    PressDodge();
                    rolled = true;
                }
                if (rolled && Fsm.State == CombatState.Locomotion && t > 1.5f)
                {
                    Vector3 d = locomotion.transform.position - rollStart;
                    d.y = 0f;
                    float forward = Vector3.Dot(d, rollForward);
                    float side = Mathf.Abs(Vector3.Dot(d, Vector3.Cross(Vector3.up, rollForward)));
                    float expected = RollClipDistance * scale;
                    Check($"roll at scale {scale} travels {expected:F2} m ({forward:F2})", Mathf.Abs(forward - expected) <= expected * 0.1f);
                    Check($"roll at scale {scale} goes straight ({side:F2} m sideways)", side < 0.1f);
                    phase++;
                    phaseStart = now;
                    rolled = false;
                    if (phase == 3) Log("--- locked onto the dummy, strafe right");
                }
                break;

            case 3:
                if (!strafeSetUp)
                {
                    // the dummy is moved next to the open floor for this session only
                    strafeSetUp = true;
                    Teleport(OpenFloor, Vector3.forward);
                    Vector3 dummyPos = OpenFloor + Vector3.forward * 5f;
                    dummyPos.y = Dummy.transform.position.y;
                    Dummy.transform.position = dummyPos;
                    LockOnto(Dummy.GetComponent<LockOnTarget>());
                }
                SetInput(new Vector2(1f, 0f), false);
                if (t >= 1f)
                {
                    Vector3 v = controller.velocity;
                    v.y = 0f;
                    Vector3 clip = animator.velocity;
                    clip.y = 0f;
                    speedSum += v.magnitude;
                    clipSpeedSum += Vector3.Dot(clip, v.normalized);
                    samples++;
                }
                if (t >= 2.6f)
                {
                    SetInput(Vector2.zero, false);
                    float speed = speedSum / samples;
                    float ratio = clipSpeedSum / speedSum;
                    float cap = locomotion.config.lockedRunSpeed;
                    Check($"locked-on strafe moves at {cap:F2} m/s ({speed:F2})", Mathf.Abs(speed - cap) <= cap * 0.05f);
                    Check($"strafe animation matches the speed, no sliding (clip/actual {ratio:F2})", ratio > 0.85f && ratio < 1.15f);
                    // a play-mode copy so the swing speed can be pinned per phase
                    resolver = Fsm.GetComponent<HitResolver>();
                    resolver.attack = Object.Instantiate(resolver.attack);
                    lungeDistance = resolver.attack.lungeDistance;
                    phase = 4;
                    phaseStart = now;
                    Log($"--- slash: no lunge, then the {resolver.attack.lungeDistance} m lunge at swing speed 1 and 1.25");
                }
                break;

            case 4:
            case 5:
            case 6:
                // 4: no lunge, measures the clip's own net step; 5: lunge at x1; 6: lunge at x1.25
                float swingSpeed = phase == 6 ? 1.25f : 1f;
                if (!swung && Fsm.State == CombatState.Locomotion && t > 0.3f)
                {
                    resolver.attack.lungeDistance = phase == 4 ? 0f : lungeDistance;
                    resolver.attack.speedMin = resolver.attack.speedMax = swingSpeed;
                    rollStart = locomotion.transform.position;
                    rollForward = locomotion.transform.forward;
                    firstMove = now;
                    lastMove = -1f;
                    PressAttack();
                    swung = true;
                }
                if (swung && lastMove < 0f && phase > 4 && Vector3.Dot(locomotion.transform.position - rollStart, rollForward) >= clipStep + lungeDistance * 0.5f)
                    lastMove = now; // time to cover half the lunge
                if (swung && Fsm.State == CombatState.Locomotion && now - firstMove > 2f)
                {
                    Vector3 d = locomotion.transform.position - rollStart;
                    d.y = 0f;
                    float forward = Vector3.Dot(d, rollForward);
                    if (phase == 4)
                    {
                        clipStep = forward;
                        Log($"   the slash clip's own net step: {clipStep:F3} m");
                    }
                    else
                    {
                        float expected = clipStep + lungeDistance;
                        Check($"slash at x{swingSpeed} moves its own step + the {lungeDistance} m lunge = {expected:F2} m ({forward:F2})", Mathf.Abs(forward - expected) <= 0.05f);
                        float halfTime = lastMove - firstMove;
                        if (phase == 5)
                        {
                            lungeTimeAt1 = halfTime;
                        }
                        else
                        {
                            float ratio = lungeTimeAt1 / halfTime;
                            Check($"the lunge speeds up with the swing (half the lunge at {lungeTimeAt1:F3}s at x1, {halfTime:F3}s at x1.25, ratio {ratio:F2})", lastMove > 0f && Mathf.Abs(ratio - 1.25f) <= 0.1f);
                            return true;
                        }
                    }
                    phase++;
                    phaseStart = now;
                    swung = false;
                }
                break;
        }
        return false;
    }
}
