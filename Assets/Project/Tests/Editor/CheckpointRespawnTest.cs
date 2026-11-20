using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

// Checkpoints, death and respawn in Area01.
// 1. dying before any rest: YOU DIED fades in, the body stays put, respawn at the level start
// 2. resting (through Interact at Checkpoint_01): prompt shown, health/stamina/heal charges refilled,
//    respawn point set, enemies left alone (a dead one stays dead, a hurt one stays hurt); no prompt mid-roll
// 3. killed by Enemy A after resting: it stops attacking the body, respawn at the checkpoint with
//    everything refilled, and every enemy reset (alive, at spawn, full health/poise, idle, lockable)
// Play mode only: Enemy B and two of the hall enemies are switched off.
public class CheckpointRespawnTest : PlayModeTest
{
    public override string Name => "CheckpointRespawn";

    static readonly MethodInfo InteractMethod = typeof(PlayerInteractor).GetMethod("OnInteract", Private);

    GameLoop gameLoop;
    Checkpoint checkpoint;
    PlayerInteractor interactor;
    HealFlask flask;
    Text prompt;
    CanvasGroup deathScreen;
    EnemyBrain startEnemy, hallEnemy;
    Vector3 levelStart, deathPosition;
    int phase;
    float phaseStart, deathAt = -1f, rollSeenAt = -1f;
    bool checkedFade, rested, rolled, setUpDeath, sawReturn;
    int hitsAfterDeath;

    protected override float TimeLimit => 90f;

    protected override void Setup()
    {
        gameLoop = Object.FindAnyObjectByType<GameLoop>();
        checkpoint = GameObject.Find("Checkpoint_01").GetComponent<Checkpoint>();
        interactor = Fsm.GetComponent<PlayerInteractor>();
        flask = Fsm.GetComponent<HealFlask>();
        prompt = GameObject.Find("InteractPrompt").GetComponent<Text>();
        deathScreen = GameObject.Find("DeathScreen").GetComponent<CanvasGroup>();
        startEnemy = GameObject.Find("EnemyA_Start").GetComponent<EnemyBrain>();
        hallEnemy = GameObject.Find("EnemyA_Hall_1").GetComponent<EnemyBrain>();
        foreach (string n in new[] { "EnemyB_Corridor", "EnemyA_Hall_2", "EnemyA_Hall_3" })
            GameObject.Find(n).SetActive(false);

        levelStart = Fsm.transform.position;
        Teleport(new Vector3(0f, 0f, 24f), Vector3.forward);
        PlayerStats.TakeDamage(9999);
        Begin(1, Time.time, "1. die before resting");
    }

    protected override void OnLog(string line)
    {
        if (line.StartsWith("[Level]") || line.StartsWith("[Enemy]")) Log(line);
        if (line.Contains("EnemyA_Hall_1 target died, returning")) sawReturn = true;
        if (deathAt > 0f && line.StartsWith("[Hit] EnemyA") && line.Contains("Target: Player")) hitsAfterDeath++;
    }

    void Begin(int next, float now, string description)
    {
        phase = next;
        phaseStart = now;
        deathAt = -1f;
        checkedFade = false;
        Log("--- " + description);
    }

    protected override bool Step(float now)
    {
        if (now - phaseStart > 25f)
        {
            Check($"part {phase} finishes in time", false);
            return true;
        }

        switch (phase)
        {
            case 1:
                if (deathAt < 0f)
                {
                    deathAt = phaseStart;
                    deathPosition = Fsm.transform.position;
                }
                if (CheckDeathScreen(now)) break;
                if (now - deathAt > gameLoop.deathScreenTime + 0.3f)
                {
                    CheckRespawned(levelStart, "the level start");
                    Begin(2, now, "2. rest at Checkpoint_01");
                }
                break;

            case 2:
                if (!rested)
                {
                    // a dead enemy, a hurt enemy (it notices, gives up at this range and walks home), a worn-out player
                    startEnemy.GetComponent<CombatStats>().TakeDamage(9999);
                    hallEnemy.GetComponent<CombatStats>().TakeDamage(30);
                    PlayerStats.TakeDamage(50);
                    PlayerStamina.Spend(60f);
                    flask.UseCharge();
                    Teleport(checkpoint.transform.position + Vector3.left * 1.3f, Vector3.forward);
                    rested = true;
                    break;
                }
                if (now - phaseStart < 0.3f) break;
                if (!rolled)
                {
                    Check($"prompt shows \"{prompt.text}\" at the checkpoint", prompt.enabled && prompt.text == "E  Rest" && interactor.Current == checkpoint);
                    InteractMethod.Invoke(interactor, null);
                    Check($"rest refills health {PlayerStats.Health}/{PlayerStats.MaxHealth}, stamina {PlayerStamina.Current:F0}/{PlayerStamina.Max:F0}, charges {flask.Charges}/{flask.maxCharges}",
                        PlayerStats.Health == PlayerStats.MaxHealth && Mathf.Approximately(PlayerStamina.Current, PlayerStamina.Max) && flask.Charges == flask.maxCharges);
                    Check("rest sets Checkpoint_01 as the respawn point", gameLoop.LastCheckpoint == checkpoint);
                    Check("rest leaves a killed enemy dead", startEnemy.GetComponent<CombatStats>().IsDead);
                    CombatStats hall = hallEnemy.GetComponent<CombatStats>();
                    Check($"rest leaves a hurt enemy hurt (HP {hall.Health}/{hall.MaxHealth})", hall.Health < hall.MaxHealth);
                    PressDodge();
                    rolled = true;
                    break;
                }
                if (Fsm.State == CombatState.Dodging && rollSeenAt < 0f) rollSeenAt = now;
                // the interactor and prompt update in their own Update, a frame after the state change
                if (rollSeenAt > 0f && now - rollSeenAt > 0.1f)
                {
                    Check($"no interaction mid-roll ({Fsm.State})", Fsm.State == CombatState.Dodging && interactor.Current == null && !prompt.enabled);
                    Begin(3, now, "3. killed by Enemy A after resting");
                }
                else if (now - phaseStart > 1.5f)
                {
                    Check("the dodge starts", false);
                    return true;
                }
                break;

            case 3:
                if (deathAt < 0f)
                {
                    if (!setUpDeath)
                    {
                        if (Fsm.State != CombatState.Locomotion) break; // let the roll finish
                        // in front of the hall enemy's spawn, one hit kills, a charge is used so the refill shows
                        setUpDeath = true;
                        flask.UseCharge();
                        Vector3 spot = hallEnemy.SpawnPoint + hallEnemy.SpawnRotation * Vector3.forward * 3f;
                        Teleport(spot, hallEnemy.SpawnPoint - spot);
                        PlayerStats.TakeDamage(PlayerStats.Health - 1);
                    }
                    if (Fsm.State == CombatState.Dead)
                    {
                        deathAt = now;
                        deathPosition = Fsm.transform.position;
                        hitsAfterDeath = 0;
                    }
                    break;
                }
                if (CheckDeathScreen(now)) break;
                if (now - deathAt > gameLoop.deathScreenTime + 0.3f)
                {
                    Check("the enemy that killed the player stops and walks home", sawReturn);
                    Check($"no hits land on the body ({hitsAfterDeath})", hitsAfterDeath == 0);
                    CheckRespawned(checkpoint.respawnPoint.position, "Checkpoint_01");
                    CheckReset(startEnemy);
                    CheckReset(hallEnemy);
                    Check("lock-on cleared", LockOn.Current == null);
                    return true;
                }
                break;
        }
        return false;
    }

    // true while still waiting on the death screen
    bool CheckDeathScreen(float now)
    {
        if (!checkedFade && now - deathAt > 1.5f)
        {
            checkedFade = true;
            Check($"YOU DIED faded in (alpha {deathScreen.alpha:F2})", deathScreen.alpha > 0.95f);
            float moved = FlatDistance(Fsm.transform.position, deathPosition);
            Check($"the body stays where it fell (moved {moved:F2} m)", moved < 0.3f);
            Check("still dead behind the screen", Fsm.State == CombatState.Dead);
        }
        return now - deathAt < 1.5f;
    }

    void CheckRespawned(Vector3 expected, string where)
    {
        float off = FlatDistance(Fsm.transform.position, expected);
        Check($"respawned at {where} ({off:F2} m off)", off < 0.2f);
        Check($"back in Locomotion ({Fsm.State})", Fsm.State == CombatState.Locomotion);
        Check($"health {PlayerStats.Health}/{PlayerStats.MaxHealth}, stamina {PlayerStamina.Current:F0}/{PlayerStamina.Max:F0}, charges {flask.Charges}/{flask.maxCharges} refilled",
            PlayerStats.Health == PlayerStats.MaxHealth && Mathf.Approximately(PlayerStamina.Current, PlayerStamina.Max) && flask.Charges == flask.maxCharges);
        Check($"death screen hidden (alpha {deathScreen.alpha:F2})", deathScreen.alpha < 0.01f);
    }

    void CheckReset(EnemyBrain enemy)
    {
        CombatStats stats = enemy.GetComponent<CombatStats>();
        float off = FlatDistance(enemy.transform.position, enemy.SpawnPoint);
        Check($"{enemy.name} reset: alive {stats.Health}/{stats.MaxHealth}, poise {stats.CurrentPoise:F0}/{stats.MaxPoise:F0}, {off:F2} m from spawn, {enemy.StatePath}",
            !stats.IsDead && stats.Health == stats.MaxHealth && Mathf.Approximately(stats.CurrentPoise, stats.MaxPoise) && off < 0.2f && enemy.StatePath == "Idle");
        Check($"{enemy.name} lockable again", enemy.GetComponent<LockOnTarget>().enabled && enemy.GetComponent<Collider>().enabled);
    }
}
