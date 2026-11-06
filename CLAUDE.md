# MTDW – Souls-like vertical slice (final year project)

## Tech stack
- Unity 6.6 (6000.6), Windows PC target. Check installed package versions in Packages/manifest.json before using an API.
- New Input System ONLY (com.unity.inputsystem) using an Input Actions asset with a generated C# class. Never use UnityEngine.Input or the legacy Input Manager.
- Cinemachine 3.x ONLY (namespace Unity.Cinemachine: CinemachineCamera, CinemachineOrbitalFollow, CinemachineRotationComposer, CinemachineInputAxisController, CinemachineTargetGroup). Never use Cinemachine 2 APIs (CinemachineVirtualCamera, CinemachineFreeLook).
- Movement is root motion: the Animator drives position, applied in OnAnimatorMove via CharacterController.Move. Character rotation is handled in code, not by root rotation.

## Design rules
- Scope: one area + boss room, two enemy types. Don't add features beyond the task.
- Enemy AI will be a hierarchical FSM (not behaviour trees).
- Combat is event-driven with NO colliders/hitboxes: animation events trigger computation (e.g. "AttackHit" resolves a hit).
- Dodging uses an invincibility window set by IFrameStart/IFrameEnd animation events; inside it the hit equation always returns Dodged, outside it the roll gives no protection. Whether an attack connects is deterministic; probability is only used for damage variance and crits.
- Player combat uses a finite state machine with a FIFO event queue (Queue<T>). Events that arrive in a state that can't handle them stay queued; stale events expire after a short buffer time.
- Use flags to prevent double-processing (e.g. a swing resolves its hit only once).
- Reactions (Hit, Dodge, Attack) are Animator Triggers with Any State transitions ("Can Transition To Self" off). Only the FSM sets triggers.

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
