# MTDW – Souls-like vertical slice (final year project)

## Tech stack
- Unity 6.6 (6000.6), Windows PC target
- New Input System ONLY (com.unity.inputsystem) using an Input Actions asset with a generated C# class. Never use UnityEngine.Input or the legacy Input Manager.
- Cinemachine 3.x ONLY (namespace Unity.Cinemachine: CinemachineCamera, CinemachineOrbitalFollow, CinemachineRotationComposer, CinemachineInputAxisController, CinemachineTargetGroup). Never use Cinemachine 2 APIs (CinemachineVirtualCamera, CinemachineFreeLook).
- Movement is root motion: the Animator drives position, applied in OnAnimatorMove via CharacterController.Move. Character rotation is handled in code, not by root rotation.

## Design rules
- Combat will NOT use collision/hitboxes. Hits are resolved with stats/probability, triggered by animation events.
- Enemy AI will be a hierarchical FSM (not behaviour trees).
- Scope: one area + boss room, two enemy types. Don't add features beyond the task.

## Code conventions
- Scripts live in Assets/_Project/Scripts/<Feature>/
- Public fields are fine for inspector settings and references; keep internal runtime state private
- [SerializeField] private only when a value must be in the inspector but other scripts shouldn't touch it (rare)
- No [Tooltip]s. Use [Header] sparingly, only on longer components
- Short, plain // comments where something isn't obvious (units, gotchas). No XML <summary> docs
- Keep code simple and direct; don't add defensive null checks/fallbacks for references that are set up in the scene
- Separate input reading from gameplay logic; keep MonoBehaviours small and single-purpose

## Boundaries
- Don't modify anything outside Assets/_Project/ without asking (no Packages/, ProjectSettings/)
- Don't install packages, delete, or rename assets/scenes without asking
- Before changing scenes or prefabs, say what you'll change
- After code changes, check the Unity console for compile errors and fix them
- Work in small steps; summarise what you did after each one

## Tooling
- The open Unity Editor can be driven from the terminal with the Unity CLI: `unity command eval` (run editor C#), console read/clear, play mode, etc. Prefer it over hand-editing .unity/.prefab/.asset YAML.
