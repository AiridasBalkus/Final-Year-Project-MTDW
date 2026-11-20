using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

// Resting at checkpoints, death and respawn. Regular enemies are every active EnemyBrain in the scene.
public class GameLoop : MonoBehaviour
{
    public CombatStats player;
    public LockOnController lockOn;
    public DeathScreen deathScreen;
    public Transform cameraTarget;     // what the cameras follow, warped with the player
    public float deathScreenTime = 3f; // seconds from death to respawn

    public Checkpoint LastCheckpoint { get; private set; }

    PlayerCombatFSM playerFsm;
    PlayerLocomotion playerLocomotion;
    Stamina playerStamina;
    HealFlask playerFlask;
    EnemyBrain[] enemies;
    Vector3 startPosition; // respawn point before the first rest
    Quaternion startRotation;

    void Awake()
    {
        playerFsm = player.GetComponent<PlayerCombatFSM>();
        playerLocomotion = player.GetComponent<PlayerLocomotion>();
        playerStamina = player.GetComponent<Stamina>();
        playerFlask = player.GetComponent<HealFlask>();
        enemies = FindObjectsByType<EnemyBrain>();
        startPosition = player.transform.position;
        startRotation = player.transform.rotation;
    }

    void OnEnable()
    {
        player.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        player.Died -= OnPlayerDied;
    }

    // enemies are left as they are, they only come back when the player dies
    public void Rest(Checkpoint checkpoint)
    {
        LastCheckpoint = checkpoint;
        RefillPlayer();
        Debug.Log($"[Level] rested at {checkpoint.name}, player refilled");
    }

    void OnPlayerDied()
    {
        Debug.Log("[Level] player died");
        deathScreen.Show();
        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(deathScreenTime);
        Respawn();
    }

    void Respawn()
    {
        ResetEnemies();

        Vector3 position = LastCheckpoint != null ? LastCheckpoint.respawnPoint.position : startPosition;
        Quaternion rotation = LastCheckpoint != null ? LastCheckpoint.respawnPoint.rotation : startRotation;
        Vector3 delta = position - player.transform.position;
        playerLocomotion.Teleport(position, rotation);
        // so the cameras cut to the new spot instead of flying there
        CinemachineCore.OnTargetObjectWarped(cameraTarget, delta);

        RefillPlayer();
        playerFsm.Respawn();
        deathScreen.Hide();
        Debug.Log($"[Level] respawned at {(LastCheckpoint != null ? LastCheckpoint.name : "start")}");
    }

    void RefillPlayer()
    {
        player.RestoreAll();
        playerStamina.Refill();
        playerFlask.Refill();
    }

    void ResetEnemies()
    {
        // a dead target coming back would otherwise keep the old lock
        lockOn.Unlock();
        // the test scripts switch some enemies off
        foreach (EnemyBrain enemy in enemies)
            if (enemy.gameObject.activeInHierarchy) enemy.ResetToSpawn();
    }
}
