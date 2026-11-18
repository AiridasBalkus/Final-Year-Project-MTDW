> \[High-concept sentence\]

<img src="./media/image1.png" style="width:6.01667in;height:6.01667in" alt="Images with solid fill" />

> Name: Airidas Balkus
>
> Student ID: 20107228
>
> Course: BSc (Hons) Applied Computing in Game Development
>
> Supervisor: Giuseppe "Pino" De Francesco
>
> Submission Date: \[DD/MM/YYYY\]

# Contents

- [Revision History](#revision-history)
- [Purpose of this Document](#purpose-of-this-document)
- [1. Basic Design Information](#1-basic-design-information)
  - [1.2 Target Platform](#12-target-platform)
  - [1.3 Tools and Third-Party Packages](#13-tools-and-third-party-packages)
- [2. Game Overview](#2-game-overview)
  - [2.1. Game Concept](#21-game-concept)
    - [2.1.1 High Concept](#211-high-concept)
    - [2.1.2 Design Goals](#212-design-goals)
  - [2.2. Feature Set](#22-feature-set)
  - [2.3. Genre](#23-genre)
  - [2.4. Target Audience](#24-target-audience)
  - [2.5. Game Flow Summary](#25-game-flow-summary)
  - [2.6. Look and Feel](#26-look-and-feel)
  - [2.7. Project Scope](#27-project-scope)
- [3. Unique Selling Points](#3-unique-selling-points)
- [4. Gameplay and Mechanics](#4-gameplay-and-mechanics)
  - [4.1. Gameplay](#41-gameplay)
    - [4.1.1. Objectives](#411-objectives)
    - [4.1.2. Game Progression](#412-game-progression)
    - [4.1.3. Mission / Challenge Structure](#413-mission--challenge-structure)
    - [4.1.4. Play Flow](#414-play-flow)
    - [4.1.5. Player and Genre Expectations](#415-player-and-genre-expectations)
  - [4.2. Mechanics](#42-mechanics)
    - [4.2.1. Physics](#421-physics)
    - [4.2.2. Movement](#422-movement)
    - [4.2.3. Objects](#423-objects)
    - [4.2.4. Actions](#424-actions)
    - [4.2.5. Combat](#425-combat)
    - [4.2.6. Statistics System](#426-statistics-system)
    - [4.2.7. Economy](#427-economy)
    - [4.2.8. Difficulty](#428-difficulty)
  - [4.3. Screen Flow](#43-screen-flow)
    - [4.3.1. Screen Flow Chart](#431-screen-flow-chart)
    - [4.3.2. Screen Descriptions](#432-screen-descriptions)
  - [4.4. Game Options](#44-game-options)
  - [4.5. Replaying and Saving](#45-replaying-and-saving)
  - [4.6. Accessibility](#46-accessibility)
    - [4.6.1. Physical](#461-physical)
    - [4.6.2. Hearing](#462-hearing)
    - [4.6.3. Visual](#463-visual)
- [5. Story, Setting and Character](#5-story-setting-and-character)
  - [5.1 Story and Narrative](#51-story-and-narrative)
    - [5.1.1 Back Story](#511-back-story)
    - [5.1.2 Plot Elements](#512-plot-elements)
    - [5.1.3 Game Progression](#513-game-progression)
    - [5.1.4 Cut Scenes](#514-cut-scenes)
  - [5.2 Game World](#52-game-world)
    - [5.2.1 General Look and Feel of World](#521-general-look-and-feel-of-world)
    - [5.2.2 The Area](#522-the-area)
    - [5.2.3 Boss Room](#523-boss-room)
  - [5.3 Characters](#53-characters)
    - [5.3.1 Player Character](#531-player-character)
    - [5.3.2 Enemy A](#532-enemy-a)
    - [5.3.3 Enemy B](#533-enemy-b)
    - [5.3.4 Boss](#534-boss)
- [6. Levels](#6-levels)
  - [6.1 Level 1 – The Area](#61-level-1--the-area)
    - [6.1.1 Synopsis](#611-synopsis)
    - [6.1.2 Introductory Material](#612-introductory-material)
    - [6.1.3 Objectives](#613-objectives)
    - [6.1.4 Map](#614-map)
    - [6.1.5 Critical Path](#615-critical-path)
    - [6.1.6 Encounters](#616-encounters)
    - [6.1.7 Level Walkthrough](#617-level-walkthrough)
    - [6.1.8 Closing Material](#618-closing-material)
  - [6.2 Training Level](#62-training-level)
- [7. Interface](#7-interface)
  - [7.1 Visual System](#71-visual-system)
    - [7.1.1 HUD](#711-hud)
    - [7.1.2 Menus](#712-menus)
    - [7.1.3 Rendering System](#713-rendering-system)
    - [7.1.4 Camera](#714-camera)
    - [7.1.4.1 Positioning](#7141-positioning)
    - [7.1.4.2 Collision Handling](#7142-collision-handling)
    - [7.1.5 Lighting Models](#715-lighting-models)
  - [7.2 Control System](#72-control-system)
  - [7.3 Audio](#73-audio)
  - [7.4 Music](#74-music)
  - [7.5 Sound Effects](#75-sound-effects)
  - [7.6 Help System](#76-help-system)
- [8. Artificial Intelligence](#8-artificial-intelligence)
  - [8.1 Enemy AI](#81-enemy-ai)
    - [8.1.1 Architecture](#811-architecture)
    - [8.1.2 States and Transitions](#812-states-and-transitions)
    - [8.1.3 Attack Selection](#813-attack-selection)
    - [8.1.4 Group Coordination](#814-group-coordination)
  - [8.2 Boss AI](#82-boss-ai)
  - [8.3 Support AI](#83-support-ai)
    - [8.3.1 Player and Collision Detection](#831-player-and-collision-detection)
    - [8.3.2 Pathfinding](#832-pathfinding)
- [9. Technical](#9-technical)
  - [9.1 Target Hardware](#91-target-hardware)
  - [9.2 Development Hardware and Software](#92-development-hardware-and-software)
  - [9.3 Development Procedures and Standards](#93-development-procedures-and-standards)
  - [9.4 Game Engine](#94-game-engine)
  - [9.5 Scripting Language](#95-scripting-language)
  - [9.6 AI-Assisted Development](#96-ai-assisted-development)
- [10. Game Art](#10-game-art)
  - [10.1 Concept Art](#101-concept-art)
  - [10.2 Style Guides](#102-style-guides)
  - [10.3 Characters](#103-characters)
  - [10.4 Environments](#104-environments)
  - [10.5 Equipment](#105-equipment)
  - [10.6 Miscellaneous](#106-miscellaneous)
- [11. Project Management](#11-project-management)
  - [11.1 Project Methodology](#111-project-methodology)
  - [11.2 Detailed Schedule](#112-detailed-schedule)
  - [11.3 Schedule Management](#113-schedule-management)
  - [11.4 Version Control](#114-version-control)
  - [11.5 Risk Analysis](#115-risk-analysis)
  - [11.6 Test Plan](#116-test-plan)
    - [11.6.1 Functional Testing](#1161-functional-testing)
    - [11.6.2 Playtesting and Evaluation](#1162-playtesting-and-evaluation)
    - [11.6.3 Performance Testing](#1163-performance-testing)
- [12. Appendices](#12-appendices)
  - [12.1. Credits](#121-credits)
    - [12.1.1 Images](#1211-images)
    - [12.1.2 Models](#1212-models)
    - [12.1.3 Animations](#1213-animations)
    - [12.1.4 Audio](#1214-audio)
    - [12.1.5 Other](#1215-other)
  - [12.2. References](#122-references)
  - [12.3. Project Logs](#123-project-logs)

# Revision History

| Date       | Version | Description                                                                                                                                                                                                                                                         | Author         |
|------------|---------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|
| 15/10/2026 | 0.1     | Template created from the university GDD outline and supervisor template.                                                                                                                                                                                           | Airidas Balkus |
| 15/10/2026 | 0.2     | Some Content Added from my FYP Plan v2, with feedback from the supervisor, scoped reduced, hour budget corrected, engine version corrected, evaluation plan added, references added, AI Usage logged.                                                               | Airidas Balkus |
| 21/10/2026 | 0.3     | Sprint 1: Implementation documented, movement, camera and lock-on, controls, footstep audio and combat model (event-driven state machine with FIFO event queue)                                                                                                     | Airidas Balkus |
| 12/11/2026 | 0.4     | Sprint 2: code-driven movement (supervisor feedback), stats and damage formula, stamina, poise and stagger, block, heal, Enemy A with the hierarchical FSM and perception, automated Play-mode tests. Movement, combat, AI, interface and testing sections written. | Airidas Balkus |

# Purpose of this Document

This game Design Document is the blueprint for Mino The Dark Worlds, a Souls-like third-person action RPG vertical slice developed in unity as a final year project. The deliverable is the product: the playable slice and this document. Any feature not described in this document is out of scope. The document is updated each sprint, and changes are recorded in the revision history.

# 1. Basic Design Information

***Person responsible:*** Airidas Balkus

***Supervisor:*** Giuseppe "Pino" De Francesco

***Type of Development:*** Game - Final year project vertical slice

***Genre(s):*** Souls-like third-person action RPG

***Deliverables:*** Grey-box Demo, playable Demo, GDD, demo video

***File Repository:*** \[GitHub Repo URL\]

***Build versus buy approach:*** Mix of both. All gameplay systems (player controller, combat, stats, enemy AI, boss) are built in C#, Character models, animations and audio are all sourced from free asset libraries (e.g. Mixamo). Custom assets will be made in Blender in semester 2 if time allows.

## 1.2 Target Platform

| Platform     | Version       | Description                                                |
|--------------|---------------|------------------------------------------------------------|
| Windows (PC) | Windows 10/11 | Sole target platform, Keyboard and mouse and gamepad input |

## 1.3 Tools and Third-Party Packages

| Tool/ Package               | Version          | Licence/Royalty         | Purpose                                |
|-----------------------------|------------------|-------------------------|----------------------------------------|
| Unity                       | 6.6              | Free                    | Game Engine                            |
| Visual Studio               | 2026             | Free                    | C# Development                         |
| Git + GitHub (Git with LFS) | 2.55.0.windows.3 | Free                    | Version Control and repository hosting |
| Trello                      | \-               | Free Plan               | Sprint board and backlog               |
| Mixamo (Adobe)              | \-               | Free for commercial use | Character animations and characters    |
| Blender                     | 5.2.2 LTS        | Free                    | Custom assets - stretch                |
| AI Navigation               |                  | Unity Package           | NavMesh pathfinding for enemies        |
| Cinemachine                 |                  | Unity Package           | Camera work                            |
| Input System                |                  | Unity Package           | Keyboard/mouse                         |

# 2. Game Overview

## 2.1. Game Concept

### 2.1.1 High Concept

A Souls-like combat slice of slow, deliberate combat in which every enemy attack is clearly telegraphed, and every player action costs stamina. Hits are resolved by stats and probability rather than physics collisions, while the timing of dodges and blocks remains down to player skill.

### 2.1.2 Design Goals

The design is built on three pillars, each expressed as a target player experience in the sense of the MDA framework [(Hunicke, LeBlanc and Zubek, 2004).](#122-references) Each pillar has a rule the design must follow, and a measure used in the evaluation (see [Test Plan](#116-test-plan)).

| **Pillar**          | **Design rule**                                                                                                                                                   | **Measured by**                                                        |
|---------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------|
| Satisfying impact   | Every successful hit produces immediate feedback, hit-stop, camera shake, hit reaction and audio are triggered on the same animation event that resolves the hit. | Questionnaire items on hit impact; GEQ positive affect                 |
| Fair and readable   | Every enemy attack has a visual and audio wind-up. Randomness affects how much damage a hit deals, never whether a correctly timed dodge or block succeeds.       | Questionnaire items on readability and fairness; GEQ tension/annoyance |
| Weighted commitment | Attacks, dodges, blocks and heals all cost stamina and have a recovery window. No action is risk-free.                                                            | Questionnaire items on stamina; observed deaths from stamina depletion |

## 2.2. Feature Set

| **Feature**                                                                                         | **Priority**                  |
|-----------------------------------------------------------------------------------------------------|-------------------------------|
| Third-person player controller: move, sprint, lock-on, dodge with invincibility frames, block, heal | Core                          |
| Stat-based combat with animation-event hit resolution (range and angle checks)                      | Core                          |
| Stamina and poise/stagger systems                                                                   | Core                          |
| Data-driven hierarchical FSM enemy AI framework                                                     | Core                          |
| Two regular enemy types built mostly through configuration                                          | Core                          |
| Two-phase boss                                                                                      | Core                          |
| One area plus a boss room, with a checkpoint and enemy reset on death                               | Core                          |
| HUD, main menu, pause, death and victory screens                                                    | Core                          |
| Stat upgrades and loot                                                                              | Stretch - only if time allows |
| Shortcut back to the hub checkpoint                                                                 | Stretch                       |
| Attack token system for groups                                                                      | Stretch                       |
| Custom Blender assets, extra weapons                                                                | Stretch                       |

## 2.3. Genre

Third-person action RPG, Souls-like (in the lineage of FromSoftware's Dark Souls series).

## 2.4. Target Audience

Players who enjoy challenging action games, in particular players familiar with Souls-like titles, on Windows PC with keyboard and mouse or gamepad. Target rating: \[PEGI 16\]. Playtest participants are students aged 18 or over (see [Test Plan](#116-test-plan)).

## 2.5. Game Flow Summary

Main Menu into New Game into tutorial segment into hub checkpoint into combat segments into pre-boss room into boss room into victory screen into credits into Main Menu. On death the death screen is shown, and the player respawns at the last checkpoint with regular enemies reset. The pause menu is available at any time during gameplay.

## 2.6. Look and Feel

Dark fantasy with heavy, desaturated tones and warm accents. Two reference directions are under consideration. Direction A is a lighter, painterly palette dominated by off-white, grey, coral/orange and maroon. Direction B is a typical dark Souls-like palette of near-black stone lit by warm torchlight with cold blue highlights. The slice remains grey box until semester 2, the final direction is chosen before the art pass.

<img src="./media/image3.png" style="width:5.2in;height:2.74444in" />

> Figure 1 - Reference direction A [(source: ArtStation.com).](#1211-images)

<img src="./media/image4.png" style="width:4.4in;height:1.50972in" />

> Figure 1.1 - Palette extracted from direction A.
>
> <img src="./media/image5.png" style="width:3in;height:2.975in" />
>
> Figure 2 - Reference direction B [(source: ArtStation.com)](#1211-images).

<img src="./media/image6.png" style="width:3in;height:2.52361in" />

> Figure 2.1 - Palette for direction B

## 2.7. Project Scope

| **Item**            | **Quantity**  | **Notes**                                         |
|---------------------|---------------|---------------------------------------------------|
| Areas               | 1 + boss room | Tutorial, hub and combat segments within one area |
| Player characters   | 1             |                                                   |
| Weapons             | 1             |                                                   |
| Regular enemy types | 2             | Built on one shared HFSM framework                |
| Bosses              | 1             | Two phases                                        |
| NPCs                | 0             |                                                   |
| Checkpoints         | 1-2           | Hub checkpoint, maybe pre-boss checkpoint         |
| Player stats        | 5             | Vitality, Endurance, Strength, Precision, Poise   |
| Stat upgrades       | -             | Stretch - only if time allows                     |

# 3. Unique Selling Points

*Will be updated - either when grey box is done or slice is playable - each point will be demonstrable in the demo video.*

# 4. Gameplay and Mechanics

## 4.1. Gameplay

### 4.1.1. Objectives

### 4.1.2. Game Progression

### 4.1.3. Mission / Challenge Structure

### 4.1.4. Play Flow

### 4.1.5. Player and Genre Expectations

## 4.2. Mechanics

### 4.2.1. Physics

### 4.2.2. Movement

#### 4.2.2.1. General Movement

Movement is driven by code. Speeds are set in metres per second in a data asset (Player_Movement) and the character is moved through Unity's CharacterController, which handles collision with the level. The first version used root motion, where the animation clips decided how far the character moved. It was replaced after supervisor feedback so that speed is a design value rather than a property of a clip, and so that different characters do not all move at exactly the same speed.

| **Gait**           | **Input**                            | **Speed** |
|--------------------|--------------------------------------|-----------|
| Walk               | Stick pushed less than halfway       | 1.35 m/s  |
| Run                | Full stick, or any keyboard movement | 3.5 m/s   |
| Sprint             | Sprint held while moving             | 5.0 m/s   |
| Strafe (locked on) | Any input while locked on            | 3.0 m/s   |

**No foot sliding.** Every locomotion clip's real speed was measured in Play mode, because Unity's own estimate was wrong for the run clip (2.89 m/s reported, 3.72 m/s real). The blend trees use these measured speeds and are fed the character's actual velocity. Where no blend of clips matches the speed, a playback multiplier speeds the clips up or slows them down. The automated tests measure clip speed against actual speed at 1.00-1.07 for every gait.

**Turning.** Input is camera-relative. Without lock-on, the character always moves the way it faces and slows down into sharp turns, so turning around while sprinting plants and turns instead of sliding sideways.

#### 4.2.2.2. Other Movement

**Lock-on strafing.** While locked on, the character faces the target and can move in any direction at 3.0 m/s, using a strafe clip for every direction.

**Dodge roll.** The roll goes in the direction of the input (or straight ahead without input) and travels 3.13 m in a straight line. The clip supplies the shape of the movement and data supplies the amount: only the clip's forward motion is kept, which removes its sideways drift, and it is scaled by dodgeDistanceScale. The roll always plays at normal speed, so its invincibility window never changes (see 4.2.5.3).

**Enemies** use the same code-driven approach, and each enemy rolls its own speeds when it spawns (see 8.3.2).

### 4.2.3. Objects

### 4.2.4. Actions

### 4.2.5. Combat

#### 4.2.5.1. Combat Model

Combat uses no colliders or physics hitboxes. Every outcome is computed by code triggered by animation events. An animation event is placed at a specific frame of a clip that triggers a computation, and a state machine decides whether that computation is allowed to run.

**Hit resolution:** When an attack reaches its impact frame, an AttackHit event asks the attacker's HitResolver to resolve that swing against its target. The checks run in this order:

| **Step** | **Check**                                                                                        | **Result**                           |
|----------|--------------------------------------------------------------------------------------------------|--------------------------------------|
| 1        | Has this swing already been resolved? (hitResolved flag, cleared when a swing starts)            | Ignored. A swing can never hit twice |
| 2        | Is the target within the attack's range and inside its reach (the arc in front of the attacker)? | If not: out of reach, no effect      |
| 3        | Is the target inside its dodge invincibility window?                                             | Dodged, always, with no random roll. |
| 4        | Otherwise                                                                                        | Hit: Damage is applied               |

Whether an attack connects is deterministic. Probability only affects how much damage a hit deals, and not whether a correctly timed dodge works, as seen in the [Fair and Readable Pillar](#212-design-goals)

**Player Combat State Machine:** The player's combat system uses a finite state machine with five states. Attacks and dodges can only start from "Locomotion" state, a hit interrupts every state except if the state is HitReact and or Dead, and each reaction returns through the end event on its clip.

| **State**  | **Entered when**                                      | **Left when**                                                |
|------------|-------------------------------------------------------|--------------------------------------------------------------|
| Locomotion | Play starts, or a reaction finishes                   | Attack or Dodge pressed; a hit lands                         |
| Attacking  | Attack pressed in Locomotion                          | AttackEnd event; a hit lands                                 |
| Dodging    | Dodge pressed in Locomotion                           | DodgeEnd event; a hit lands outside the invincibility window |
| HitReact   | A hit lands (from any state except HitReact and Dead) | HitReactEnd event                                            |
| Dead       | Health reaches 0                                      | Respawn                                                      |

**Event queue.** Every input and combat event (AttackPressed, DodgePressed, HitReceived and AnimationFinished) is added to one first-in, first-out queue with a timestamp, using C#'s Queue\<T\>. Only the state machine reads the queue and only it sets the Animator's Attack, Dodge and Hit triggers. Each frame the queue is processed once, in order. An event older than 0.3 s is found to be expired and is dropped. An event the current state can handle is processed, which may change the state. Any other event is put back in its original order to wait. Because a single state machine handles one event at a time, two events can never act on the player at once, which removes race conditions such as a hit and an attack setting animation trigger in the same frame.

**Input buffering.** The queue means when an input is pressed at the wrong moment it is not lost. An attack pressed just before the current swing ends waits in the queue and plays straight after it, instead of being ignored or cancelling the swing. A press made too early still expires, so the player cannot queue up a long chain of attacks. Processing strictly from the front of the queue was tried first and deadlocked: a waiting attack press sat at the front and blocked the swing's own AnimationFinished event behind it, so the swing could never end and the press expired. Letting each event be checked in order, with waiting events put back, fixed this.

**Flags.** Flags stop events being processed twice or at the wrong time:

| **Flag / rule**  | **Purpose**                                                                                                                                                                                   |
|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| hitResolved      | Set on a swing's first resolve and cleared when the next swing starts, so one swing only hits once.                                                                                           |
| Invincible       | Open between IFrameStart and IFrameEnd, set only by the state machine. Any state change closes it, so a dodge interrupted before IFrameEnd cannot leave the player invincible.                |
| Stale end events | An AnimationFinished event whose state no longer matches the current one is discarded, because a clip that is blending out can still fire its End event after the state machine has moved on. |
| Trigger reset    | Entering a state resets the other triggers before setting its own, so no old trigger stays armed and fires later.                                                                             |
| Death            | The Died event moves straight to Dead, clears the queue and closes the invincibility window, so no buffered input plays after death.                                                          |

#### 4.2.5.2. Attacks

Each attack is a data asset (AttackData) holding its range, arc, damage multiplier, stamina cost, poise damage, block stamina cost, lunge and speed range. Balancing is done per move rather than per weapon.

| **Attack**              | **Range / arc** | **Multiplier** | **Stamina** | **Poise damage** | **Speed range** |
|-------------------------|-----------------|----------------|-------------|------------------|-----------------|
| Player slash            | 2.5 m / +/-60°    | x1.0           | 20          | 10               | 0.95-1.05       |
| Enemy A slash and heavy | see 5.3.2.2     | -              | from config | -                | 0.9-1.1         |

**Lunge.** Attacks move the same way as the dodge: a clip with forward motion keeps it, scaled by lungeScale. For clips without any motion, a code lunge adds a distance and a curve over part of the swing. The player's slash lunges 0.5 m and stops 1.2 m from the target, so it closes the gap without pushing into the enemy.

**Speed variation.** Every swing rolls a playback speed from its range, so attacks are not all identically timed. The hit event, end, wind-up and recovery all scale with the rolled speed, so the attack stays readable and learnable; measured hit timing is within 0.005 s of the expected time. Speed only changes *when* a hit happens, never *whether* it lands.

#### 4.2.5.3. Defence and Stamina

**Dodge.** The roll costs 15 stamina and carries two animation events, IFrameStart and IFrameEnd, which open an invincibility window from 5% to 67% of the clip (about 0.52 s of the 0.83 s roll). While the window is open the hit equation always returns Dodged; during the roll's start-up and recovery it gives no protection, so timing matters. In testing, 10 of 10 hits inside the window were dodged and 10 of 10 outside it landed.

**Block.** Holding block raises the shield. The player moves at walk speed, and an upper-body animation layer holds the guard pose while the legs keep walking. A hit from an attacker within +/-60° in front is Blocked: the player takes 20% of the damage, pays the attack's block stamina cost and takes no poise damage. Blocking from behind does not work. Stamina regenerates at 0.3x speed while blocking.

**Guard break.** If a blocked hit takes stamina below 0, the guard breaks and the player is staggered for 1.5 s, with a red flash (a normal block flashes white).

**Heal.** The player has 3 heal charges, shown as pips on the HUD. A heal costs 15 stamina and restores 40 health at a HealApply animation event about 0.9 s in. The charge is spent when the heal starts, so if a poise break interrupts the heal before HealApply, the charge is lost. The player moves at walk speed while healing, and a heal is refused with no charges or no stamina. Charges will refill at checkpoints.

**Stamina.** Attacking, dodging, blocking hits and healing all cost stamina. An action can start with any stamina above 0, so one action can take it negative; at 0 or below, actions are refused. Stamina regenerates at 30 per second, starting 1 second after the current attack or dodge ends; regeneration is paused while an action is in progress.

#### 4.2.5.4. Damage Calculation

*Damage = WeaponBase x AttackMultiplier x (1 + Strength x k) x 100 / (100 + Defence) x Variance x Crit*

| **Term**              | **Meaning**                                                                                 |
|-----------------------|---------------------------------------------------------------------------------------------|
| WeaponBase            | The weapon's base damage (sword: 20).                                                       |
| AttackMultiplier      | The move's multiplier from its AttackData (slash: x1.0).                                    |
| (1 + Strength x k)    | Strength scaling, with k = 0.03 (see 4.2.6).                                                |
| 100 / (100 + Defence) | Diminishing returns: 100 Defence halves damage, and Defence never makes a character immune. |
| Variance              | Random value between 0.9 and 1.1.                                                           |
| Crit                  | x1.5 when the crit roll succeeds, otherwise x1. Crit chance comes from Precision.           |

The result is rounded with a minimum of 1. At default stats (Strength 10, Defence 5) the player's slash deals about 25 damage, so the test dummy dies in 4 hits. Every hit logs each term of the formula so the numbers can be checked by hand.

#### 4.2.5.5. Hit Reactions and Poise

Every character has a hidden poise value: max poise = base + 0.8 x Poise stat, so the player has 8 at default stats. Each hit that lands reduces poise by the attack's poise damage. Only a hit that breaks poise interrupts: the player goes into HitReact (see 4.2.5.1), and an enemy is staggered, which cancels any wind-up in progress. A hit that does not break poise deals damage without interrupting. Poise snaps back to full after 3 seconds without poise damage, and also resets when it breaks. Nothing in poise is random.

At default stats one enemy hit staggers the player, while Enemy A staggers on the third player slash. Hyper armour for some heavy enemy attacks is planned.

#### 4.2.5.6. Death and Respawn

When the player's health reaches 0, the Died event moves the state machine straight to Dead and clears the event queue, so no buffered input plays after death. When an enemy dies, its body falls forward and its lock-on target is disabled, so the player's lock-on drops automatically. Respawning at the last checkpoint with regular enemies reset (see 2.5) is still to be built.

### 4.2.6. Statistics System

Each stat ranges from 10 to 20. The derived values are linear, base + per point x stat, with both numbers tunable in the inspector:

| **Stat**  | **Effect**                               | **At 10** | **At 20** |
|-----------|------------------------------------------|-----------|-----------|
| Vitality  | Max health = 50 + 5 x Vitality           | 100       | 150       |
| Endurance | Max stamina = 50 + 5 x Endurance         | 100       | 150       |
| Strength  | Damage term 1 + Strength x k             | x1.3      | x1.6      |
| Precision | Crit chance = 1% x Precision (crit x1.5) | 10%       | 20%       |
| Poise     | Max poise = base + 0.8 x Poise           | 8         | 16        |

**Strength constant k = 0.03.** Strength starts at 10, so a base character already deals x1.3 damage and maximum Strength deals x1.6. Each point above the minimum adds 3 percentage points, so k sets how much investing in Strength matters. It was chosen so that maxing Strength gives about 23% more damage than the base (1.6 / 1.3).

### 4.2.7. Economy

### 4.2.8. Difficulty

## 4.3. Screen Flow

### 4.3.1. Screen Flow Chart

### 4.3.2. Screen Descriptions

#### 4.3.2.1. Main Menu Screen

#### 4.3.2.2. Options Screen

#### 4.3.2.3. Gameplay / HUD Screen

#### 4.3.2.4. Pause Screen

#### 4.3.2.5. Death Screen

#### 4.3.2.6. Victory Screen

## 4.4. Game Options

## 4.5. Replaying and Saving

## 4.6. Accessibility

### 4.6.1. Physical

### 4.6.2. Hearing

### 4.6.3. Visual

# 5. Story, Setting and Character

## 5.1 Story and Narrative

### 5.1.1 Back Story

### 5.1.2 Plot Elements

### 5.1.3 Game Progression

### 5.1.4 Cut Scenes

## 5.2 Game World

### 5.2.1 General Look and Feel of World

### 5.2.2 The Area

#### 5.2.2.1 General Description

#### 5.2.2.2 Physical Characteristics

#### 5.2.2.3 Levels that Use Area

#### 5.2.2.4 Connections to Other Areas

### 5.2.3 Boss Room

## 5.3 Characters

### 5.3.1 Player Character

#### 5.3.1.1 Back Story

#### 5.3.1.2 Look

#### 5.3.1.2.1 Physical Characteristics

#### 5.3.1.2.2 Animations

#### 5.3.1.3 Special Abilities

#### 5.3.1.4 Statistics

### 5.3.2 Enemy A 

#### 5.3.2.1 Look

Greybox placeholder: the player's Mixamo character tinted a different colour, holding a greybox sword whose blade glows during attack wind-ups.

#### 5.3.2.2 Attacks

Enemy A has two attacks, a slash and a heavy slash, chosen by weighted random selection (see 8.1.3). Each attack glows orange during its wind-up, and the heavy slash moves forward with its clip's motion. Each swing's speed varies between 0.9x and 1.1x, with the wind-up and hit timing scaled to match.

#### 5.3.2.3 Statistics

Each Enemy A rolls its own walk speed (1.15-1.55 m/s) and run speed (3.0-3.4 m/s) when it spawns, slightly slower than the player's run so the player can disengage. It staggers on the third player slash and recovers for 0.8 s after each attack. All values live in its config asset, EnemyA_Config.

### 5.3.3 Enemy B 

#### 5.3.3.1 Look

#### 5.3.3.2 Attacks

#### 5.3.3.3 Statistics

### 5.3.4 Boss 

#### 5.3.4.1 Look

#### 5.3.4.2 Phase 1 Attacks

#### 5.3.4.3 Phase 2 Attacks

#### 5.3.4.4 Statistics

# 6. Levels

## 6.1 Level 1 - The Area

### 6.1.1 Synopsis

### 6.1.2 Introductory Material

### 6.1.3 Objectives

### 6.1.4 Map

### 6.1.5 Critical Path

### 6.1.6 Encounters

### 6.1.7 Level Walkthrough

### 6.1.8 Closing Material

## 6.2 Training Level

# 7. Interface

## 7.1 Visual System

### 7.1.1 HUD

The HUD sits in the top-left: a red health bar, a green stamina bar below it, and pips for the remaining heal charges. Poise is hidden from the player.

### 7.1.2 Menus

### 7.1.3 Rendering System

### 7.1.4 Camera

The camera is built with Cinemachine 3 and uses two cameras: a free-look orbit camera driven by the Look input, and a lock-on camera that frames both the player and the target. Locking on raises the lock-on camera's priority and Cinemachine blends between them.

| **Lock-on rule** | **Behaviour**                                                                                                                                             |
|------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------|
| Acquire          | Targets within 20 m, inside 60° of the camera's forward direction and in line of sight. The one closest to the centre of the screen is chosen.            |
| Release          | Pressing lock-on again, the target dying, or the target moving beyond 25 m. The gap between 20 m and 25 m stops the lock flickering at the edge of range. |
| Switch           | A stick flick or a fast mouse swipe, with a 0.5 s cooldown. The nearest target on screen in that direction is chosen.                                     |

### 7.1.4.1 Positioning

### 7.1.4.2 Collision Handling

### 7.1.5 Lighting Models

## 7.2 Control System

Controls use the Input System with one Input Actions asset. A single PlayerInputHandler reads input and passes it to the rest of the game, so no gameplay script reads devices directly. Keyboard and mouse and gamepad are both supported.

| **Action**   | **Keyboard and mouse** | **Gamepad**   |
|--------------|------------------------|---------------|
| Move         | WASD                   | Left stick    |
| Look         | Mouse                  | Right stick   |
| Sprint       | \[confirm\]            | \[confirm\]   |
| Lock-on      | \[confirm\]            | \[confirm\]   |
| Attack       | Left mouse button      | West button   |
| Dodge        | Space                  | East button   |
| Block (hold) | Right mouse button     | Left shoulder |
| Heal         | R                      | North button  |

## 7.3 Audio

## 7.4 Music

## 7.5 Sound Effects

**Footsteps.** OnFootstep animation events sit on every foot contact in the locomotion and strafe clips. Events from clips with less than half the blend weight are ignored, so blend trees do not play double footsteps. Each step plays a random clip with slight pitch and volume variation and never repeats the same clip twice in a row.

## 7.6 Help System

# 8. Artificial Intelligence

## 8.1 Enemy AI

### 8.1.1 Architecture

Enemy AI uses a small generic hierarchical finite state machine: states can contain their own sub-state machines, so behaviour like "Combat" is broken into smaller states without one large switch statement. All behaviour values (ranges, speeds, attack list, weights, cooldowns, recovery time) live in a config asset, so Enemy B is planned as mostly a new config and a prefab variant. Enemies reuse the player's combat systems: CombatStats, HitResolver, AttackData, poise and the lock-on target.

### 8.1.2 States and Transitions

| **State**          | **Behaviour**                                            | **Leaves when**                                                                |
|--------------------|----------------------------------------------------------|--------------------------------------------------------------------------------|
| Idle / Patrol      | Waits or patrols near its spawn point                    | Notices the player                                                             |
| Alert              | Turns towards the player                                 | Moves to Chase                                                                 |
| Chase              | Follows a NavMesh path at its rolled run speed           | In attack range (Combat); player beyond 18 m or enemy 20 m from spawn (Return) |
| Combat: Reposition | Holds 1.3-1.8 m from the player, strafing or backing off | An attack is chosen                                                            |
| Combat: Attack     | Wind-up with orange glow, then the swing                 | The attack ends                                                                |
| Combat: Recover    | 0.8 s recovery window, open to punishment                | Back to Reposition                                                             |
| Staggered          | Poise broken; the current attack is cancelled            | Stagger ends                                                                   |
| Return             | Walks back to its spawn point; fights back if hit        | Arrives home, or notices the player again 5 m back inside the leash            |
| Dead               | Body falls; lock-on target disabled                      | -                                                                              |

### 8.1.3 Attack Selection

In Reposition, the enemy picks an attack by weighted random choice from the attacks that are off cooldown and that it has enough stamina for. Each attack's weight and cooldown are set in the config, so different enemy types can favour different attacks without code changes.

### 8.1.4 Group Coordination

## 8.2 Boss AI

## 8.3 Support AI

### 8.3.1 Player and Collision Detection

Enemies notice the player by sight or by hearing. Sight covers 10 m in a 60° cone either side of the enemy's forward direction and needs line of sight. Hearing depends on how the player moves: walking can be heard within 2 m, running within 7 m and sprinting within 12 m, so the player can sneak up behind an enemy by walking. Once chasing, an enemy gives up if the player is more than 18 m away or it is more than 20 m from its spawn point.

### 8.3.2 Pathfinding

Pathfinding uses Unity's AI Navigation package with a baked NavMesh. The NavMeshAgent only plans the path; code moves the enemy along it at the speeds it rolled on spawn and keeps it on the mesh, using the same no-sliding blend setup as the player.

# 9. Technical

## 9.1 Target Hardware

## 9.2 Development Hardware and Software

## 9.3 Development Procedures and Standards

Work is done on feature branches on GitHub, and I make all commits myself after reviewing the changes. AI-assisted work is done with Claude Code, constrained by a CLAUDE.md file in the project that records the tech stack, the design rules (no colliders, event-driven combat, deterministic hits) and boundaries (no package installs or changes outside the project folder without asking). Each task starts with a plan that I review before any code is written, and ends with automated Play-mode tests (see 11.6.1).

## 9.4 Game Engine

Unity 6.6 will be used for this project; it is a supported version and will continue to be supported for a while as there is no end-of-support date announced.

## 9.5 Scripting Language

C#

## 9.6 AI-Assisted Development

| **Date**   | **Tool**                | **Task**                                                                            | **How the output was checked**                                                         |
|------------|-------------------------|-------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------|
| 10/2026    | Claude (Anthropic)      | Template for the FYP plan                                                           | Reviewed and filled in by me.                                                          |
| 10/2026    | Claude (Anthropic)      | Initial damage formula                                                              | Explained and then adjusted by me.                                                     |
| 10/2026    | Claude (Anthropic)      | Colour palette extraction from reference art                                        | Checked against the reference images.                                                  |
| 15/10/2026 | Claude (Anthropic)      | GDD structure combined from two templates                                           | Checked against both templates removed unnecessary parts.                              |
| 10/2026    | Claude Code (Anthropic) | Player controller, camera and lock-on scripts                                       | Plan reviewed before coding; tested in Play mode; scripts explained to the supervisor. |
| 29/10/2026 | Claude Code (Anthropic) | Footstep audio, combat state machine with event queue, hit resolution with i-frames | Plan reviewed; scripted Play-mode timing tests; animation events placed by me.         |
| 11/2026    | Claude Code (Anthropic) | Stats, damage formula, stamina, poise, Enemy A hierarchical FSM, automated tests    | Plans reviewed; damage numbers checked by hand; tests run from the editor menu.        |
| 12/11/2026 | Claude Code (Anthropic) | Code-driven movement, block, heal, enemy hearing                                    | Plan reviewed; 80 automated checks; play-tested by me.                                 |

# 10. Game Art

## 10.1 Concept Art

## 10.2 Style Guides

## 10.3 Characters

## 10.4 Environments

## 10.5 Equipment

## 10.6 Miscellaneous

# 11. Project Management

## 11.1 Project Methodology

For the project I will be going for a solo adaptation of Scrum. The reason I am going for this approach is because the design depends on playtest feedback and the supervisor meets me every Friday, which gives a natural sprint review

## 11.2 Detailed Schedule

The FYP is worth 15 credits, which corresponds to 375-450 hours for the whole year, including all contingency.

| **Period** | **Weeks**              | **Hours per week** | **Total hours**               |
|------------|------------------------|--------------------|-------------------------------|
| Semester 1 | 8 (sprints 1-4)        | 12.5               | 100                           |
| Semester 2 | \[~12\] (sprints 5-10) | 25                 | 300                           |
| Total      |                        |                    | 400 (320 planned + 80 buffer) |

| **Sprint** | **Weeks** | **Goal**                                                                                                                                   | **Milestone**                           |
|------------|-----------|--------------------------------------------------------------------------------------------------------------------------------------------|-----------------------------------------|
| 1          | 1-2       | Project setup (Unity 6.6, GitHub, Trello); player movement and camera, evaluation plan and Ethics form                                     | Ethics form submitted                   |
| 2          | 3-4       | Stats and damage formula, stamina, poise and stagger, Enemy A with hierarchical FSM, code-driven movement, block and heal, automated tests | Player combat feature-complete; Enemy A |
| 3          |           |                                                                                                                                            |                                         |
| 4          |           |                                                                                                                                            |                                         |
| 5          |           |                                                                                                                                            |                                         |
| 6          |           |                                                                                                                                            |                                         |
| 7          |           |                                                                                                                                            |                                         |
| 8          |           |                                                                                                                                            |                                         |
| 9          |           |                                                                                                                                            |                                         |
| 10         |           |                                                                                                                                            |                                         |
| 11         |           |                                                                                                                                            |                                         |
| 12         |           |                                                                                                                                            |                                         |

*The GDD is written alongside development in every sprint.*

## 11.3 Schedule Management

Trello. It is free, its Kanban board matches a solo sprint workflow (Backlog, Sprint, In Progress, Done), and card checklists and due dates are enough for one developer. Board link: \[available when I create one\]

## 11.4 Version Control

Git with GitHub. GitHub provides free private hosting and a full commit history as evidence of progress. A Unity .gitignore excludes generated folders, and Git LFS stores large binary assets (models, textures, audio).

## 11.5 Risk Analysis

| **ID** | **Risk**                                    | **Likelihood** | **Impact** | **Mitigation**                                                                                                 |
|--------|---------------------------------------------|----------------|------------|----------------------------------------------------------------------------------------------------------------|
| R1     | Tasks take longer than planned              | High           | Medium     | 20% buffer per sprint; stretch goals cut first                                                                 |
| R2     | Scope exceeds the 400-hour budget           | Medium         | High       | Scope reduced to one area, boss room and two enemy types; hours logged per sprint and compared with the budget |
| R3     | Animation-driven combat feels wrong         | Medium         | High       | Combat test scene in sprint 2 to tune timings before level work                                                |
| R4     | Lack of art and animation assets            | Medium         | Medium     | Mixamo and free assets; grey box until semester 2                                                              |
| R5     | Clashes with other modules' deadlines       | High           | Medium     | Small sprint goals; progress reviewed at Friday calls                                                          |
| R6     | Ethics approval delayed, blocking playtests | Medium         | High       | Ethics form submitted in sprint 1                                                                              |

## 11.6 Test Plan

### 11.6.1 Functional Testing

Each feature is checked against its description in this document before its Trello card is closed. The combat test scene shows a debug readout for every hit (range, angle, each term of the damage formula) to verify the formula.

**Automated tests.** Six Play-mode tests with 80 checks run from the editor menu (MTDW then Tests then Run All), each in a fresh Play session, and print a pass or fail verdict. All pass.

| **Test**             | **Checks** | **Covers**                                                                  |
|----------------------|------------|-----------------------------------------------------------------------------|
| PlayerLocomotion     | 18         | Speeds, no sliding, sprint-turn sliding, roll distance, strafe speed, lunge |
| CombatRegression     | 13         | Input buffering, i-frames, stamina, poise, stagger                          |
| BlockHeal            | 21         | Block, guard break, heal and its interruption                               |
| EnemyPerceptionLeash | 13         | Sight, hearing, leash, chase speed, disengaging                             |
| EnemyCombatDodge     | 8          | Attack loop, telegraph, dodging enemy attacks                               |
| EnemyStaggerDeath    | 7          | Stagger and death                                                           |

The tests caught real bugs during development: the slash creeping forward on every swing, sliding when turning while sprinting, the lunge pushing the player into the enemy, and an unreliable dodge test.

### 11.6.2 Playtesting and Evaluation

The product is evaluated through playtesting with questionnaires. The plan below is the basis for the Ethics form, which is submitted in sprint 1. No data is collected before approval.

| **Item**      | **Plan**                                                                                                                                                                                                                                                    |
|---------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Sessions      | Informal grey-box playtest in sprint 4 (feedback notes only). Formal round 1 in sprint 8 and round 2 in sprint 10.                                                                                                                                          |
| Participants  | 10 volunteer students aged 18+ per formal round. Prior Souls-like experience recorded as a category (none / some / a lot).                                                                                                                                  |
| Procedure     | Information sheet and consent form, 10-20 minutes of play with a printed control sheet and no other help, questionnaire about 5 to 10 minutes long.                                                                                                         |
| Instrument 1  | Game Experience Questionnaire core module [(IJsselsteijn, de Kort and Poels, 2013)](#122-references), competence, challenge, tension/annoyance and positive affect.                                                                                             |
| Instrument 2  | Custom 5-point Likert items mapped to the design pillars, e.g. "Enemy attacks were easy to read before they hit", "When I was hit, I understood why", "Damage variation felt fair", "Hits felt impactful", "When I ran out of stamina it was my own fault". |
| Instrument 3  | Open questions: best moment, most frustrating moment, suggested change.                                                                                                                                                                                     |
| Observation   | Anonymous counts recorded by the game or the observer: deaths, cause of death (e.g. stamina depleted), time to reach the boss.                                                                                                                              |
| Data handling | Anonymous, no names or identifying data stored. Stored on OneDrive and deleted after grading.                                                                                                                                                               |
| Analysis      | Descriptive statistics per item and per pillar; round 1 and round 2 compared; findings added to the backlog and summarised in this document.                                                                                                                |

### 11.6.3 Performance Testing

Unity Profiler checks against the 60-fps target at the end of each semester-2 sprint.

# 12. Appendices

## 12.1. Credits

### 12.1.1 Images

| **Asset**                  | **Author / Source**      | **Licence** | **Link**                                                                                                                                                                                                |
|----------------------------|--------------------------|-------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Reference art, direction A | Luis Lopers, ArtStation  | \-          | \[[ArtStation - souls like](https://www.artstation.com/artwork/xzO5d4)                                                                                                                                  |
| Reference art, direction B | Oana Runaldi, ArtStation | \-          | \[[ArtStation - Radiant Resilience: A Glowing Escape from Chaos and Despair 5 \| Artworks](https://www.artstation.com/marketplace/p/YBW7y/radiant-resilience-a-glowing-escape-from-chaos-and-despair-5) |

### 12.1.2 Models

### 12.1.3 Animations

| **Asset**             | **Author / Source** | **Licence** | **Link**                            |
|-----------------------|---------------------|-------------|-------------------------------------|
| Sword and Shield Pack | Mixamo (Adobe)      | Free        | [Mixamo](https://www.mixamo.com/#/) |

### 12.1.4 Audio

### 12.1.5 Other

## 12.2. References

Hunicke, R., Leblanc, M. and Zubek, R. (2004). *MDA: A formal approach to game design and game research*. \[online\] Northwestern University, pp.2. Available at: https://users.cs.northwestern.edu/~hunicke/MDA.pdf \[Accessed 15 Oct. 2026\].

W.A. IJsselsteijn, Kort, Y.A.W. de and K. Poels (2025). *The game experience questionnaire*. \[online\] Research portal Eindhoven University of Technology. Available at: https://research.tue.nl/en/publications/the-game-experience-questionnaire/ \[Accessed 15 Oct. 2026\].

## 12.3. Project Logs

| **Sprint** | **Planned**                                                             | **Completed**                                                                                                                                        | **Hours** | **Notes**                                                              |
|------------|-------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------|-----------|------------------------------------------------------------------------|
| 1          | Project setup; player movement and camera; evaluation plan; Ethics form | Setup; controller; camera with lock-on; Ethics form submitted. Ahead of plan: footsteps, combat state machine with event queue, dodge with i-frames. | \[ \]     | Movement later changed from root motion to code (supervisor feedback). |
| 2          | Combat systems and first enemy                                          | Stats, damage formula, stamina, HUD, poise, Enemy A, code-driven movement, block, heal, 80 automated checks.                                         | \[ \]     | Next: Enemy B, grey-box level and checkpoint.                          |
