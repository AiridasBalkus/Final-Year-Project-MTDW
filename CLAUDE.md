# MTDW – Souls-like vertical slice (final year project)

## Tech stack
- Unity 6.6 (6000.6), Windows PC target. Check installed package versions in Packages/manifest.json before using an API.
- New Input System ONLY (com.unity.inputsystem) using an Input Actions asset with a generated C# class. Never use UnityEngine.Input or the legacy Input Manager.
- Cinemachine 3.x ONLY (namespace Unity.Cinemachine: CinemachineCamera, CinemachineOrbitalFollow, CinemachineRotationComposer, CinemachineInputAxisController, CinemachineTargetGroup). Never use Cinemachine 2 APIs (CinemachineVirtualCamera, CinemachineFreeLook).
- Locomotion is code-driven, not root motion. Speeds are m/s from data: PlayerMovementConfig for the player, EnemyConfig ranges for enemies (each instance rolls its own walk/run speed in Awake). Code moves the character in OnAnimatorMove (CharacterController.Move for the player, agent.nextPosition for enemies; the NavMeshAgent only plans paths). Rotation is handled in code.
- The animation follows the movement: blend tree positions are each clip's measured root speed in m/s and are fed the real velocity, and the LocoSpeedMult state multiplier (Movement/LocoPlayback) stretches playback where no clip mix matches, so feet don't slide. If clips change, re-measure their speed in play mode (AnimationClip.averageSpeed is wrong for Run_Forward).
- Attacks, the dodge and the enemy death use the clip's root motion along facing (sideways drift removed, backward kept), scaled by data (AttackData.lungeScale, PlayerMovementConfig.dodgeDistanceScale). AttackData.lungeDistance adds a code-driven lunge (Movement/AttackLunge) read from the attack state's normalised time, so it scales with the swing speed, and stops at PlayerMovementConfig.lungeStopDistance from the lock-on target; attack states are tagged "Attack".
- Free (unlocked) player movement always moves along facing and slows while turning hard, since it only has forward clips; locked-on movement can go any direction (strafe clips), capped at lockedRunSpeed.
- The Player animator has an UpperBody override layer (avatar mask: arms, hands, head) for the block (IsBlocking, BlockHit, BlockPose lowers the guard) and the heal (IsHealing), so the legs keep the walk/strafe from the base layer.

## Design rules
- Scope: one area + boss room, two enemy types. Don't add features beyond the task.
- Enemy AI will be a hierarchical FSM (not behaviour trees).
- Combat is event-driven with NO colliders/hitboxes: animation events trigger computation (e.g. "AttackHit" resolves a hit).
- Dodging uses an invincibility window set by IFrameStart/IFrameEnd animation events; inside it the hit equation always returns Dodged, outside it the roll gives no protection. Whether an attack connects is deterministic; probability is only used for damage variance and crits.
- Hit equation order (HitResolver): out of reach -> i-frames (Dodged) -> defender's Guard active and attacker inside blockHalfAngle (Blocked) -> Hit. A blocked hit deals damage × (1 - blockReduction) with no crit, costs the defender AttackData.blockStaminaCost, and does no poise damage; if that takes stamina below 0 it's GuardBroken (a timed stagger, Guard.guardBreakTime).
- Blocking is a held state read from input every frame (not queued): Locomotion <-> Blocking while held, walk speed, stamina regen × Guard.regenMultiplier. Attack and dodge can start from a block.
- Healing (HealFlask): starts from Locomotion only, refused with 0 charges or 0 stamina. The charge and stamina are spent on entry; health is restored by the HealApply event, once; a poise break before it loses the charge. Walk speed while healing. The clip is a placeholder (PowerUp).
- Enemies notice the player by sight (view cone + line of sight) or hearing: inside PlayerNoise.Radius, which depends on how fast the player moves (walking is quiet enough to sneak).
- Player combat uses a finite state machine with a FIFO event queue (Queue<T>). Events that arrive in a state that can't handle them stay queued; stale events expire after a short buffer time.
- Each swing plays at a speed rolled from AttackData.speedMin/Max (keep within about ±10%) through the AttackSpeed state multiplier. AttackHit, AttackEnd, the wind-up glow and the enemy Recover time all scale with it. The dodge roll always plays at 1x so the i-frame window never changes.
- Use flags to prevent double-processing (e.g. a swing resolves its hit only once).
- Reactions (Hit, Dodge, Attack, GuardBreak, BlockHit) are Animator Triggers with Any State transitions ("Can Transition To Self" off). Held states (IsBlocking, IsHealing) are bools. Only the FSM sets them.

## Code conventions
- Scripts live in Assets/Project/Scripts/<Feature>/
- Public fields are fine for inspector settings and references; keep internal runtime state private
- [SerializeField] private only when a value must be in the inspector but other scripts shouldn't touch it (rare)
- No [Tooltip]s. Use [Header] sparingly, only on longer components
- Short, plain // comments where something isn't obvious (units, gotchas). No XML <summary> docs
- Keep code simple and direct; don't add defensive null checks/fallbacks for references that are set up in the scene
- Separate input reading from gameplay logic; keep MonoBehaviours small and single-purpose

## Boundaries
- Don't modify anything outside Assets/Project/ without asking (no Packages/, ProjectSettings/)
- Don't install packages, delete, or rename assets/scenes without asking
- Before changing scenes or prefabs, say what you'll change
- After code changes, check the Unity console for compile errors and fix them
- Work in small steps; summarise what you did after each one

## Tooling
- The open Unity Editor can be driven from the terminal with the Unity CLI: `unity command eval` (run editor C#), console read/clear, play mode, etc. Prefer it over hand-editing .unity/.prefab/.asset YAML.
