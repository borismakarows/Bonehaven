BONE HAVEN - Dynamic Fast-paced Movement and Combat Prototype Game

1. QUICK START & SCENE SETUP
a. Open the project in Unity Hub (Unity 6.3.7f1 LTS exact engine version).

b. Navigate to the main scene in the Project window:
Assets/Scenes/Demo.unity (or Assets/Scene/Demo.unity)
Open the "Demo" scene.

c. NavMesh Setup:
If enemy navigation does not calculate on first load, open
Window -> AI -> Navigation and click "Bake".
d. Press Play in the Editor toolbar to test the encounter.

3. CONTROLS
---

* Move:                  W / A / S / D
* Walk / Sprint:         Shift (Hold to walk, Default: Run)
* Camera Aim:            Mouse (Orbital Cinemachine control)
* Melee Attack:          Left Mouse Button (3-hit combo chain)
* Black Powder:          Q (Throws powder cone to unbalance enemies)
* Shoot / Execute:       Right Mouse Button (Quick-shot / execution on stunned targets)
* Evade:                 Space
* Walk:					 Shift
* Interact:              E (Contextual interaction with chests/levers/props)

3. REQUIRED PREFABS & SCENE ARCHITECTURE
All core managers and systems are modular prefabs located in Assets/Prefabs/:

A. Managers:

* CombatJuiceManager: Micro hit-stops (Time.timeScale) and camera shakes.
* ObjectPooler: Pre-allocates and recycles projectile instances (BombProjectile).
* WaveSpawner: Dynamic squad spacing, NavMesh area sampling, and encounter lifecycle.

B. Camera & UI:

* MainCamera, PlayerFollowCamera: Main Camera with CinemachineBrain, Impulse Listener, and Virtual Camera.
* Combat_Hud : Displays Player HP, Powder count, Ammo count, and equipped weapons.

C. Characters & Enemies:

* Player: CharacterController, Locomotion, PlayerCombatFSM, Stats, Inventory, and Target Locking.
* Deckhand_Enemy: Melee enemy configured with AI, NavMeshAgent, EnemyCombatManager, and EnemyConfigSO.
* Bombardier_Enemy: Ranged lobber configured with projectile throw arcs and fuse timing.

D. Projectiles & Triggers:

* PF_Bomb_Projectile: PooledObject with BombProjectile logic and explosion radius.
* PF_BattleTriggerZone: Box trigger collider managing battle activation and wave asset execution.

4. TECHNICAL ARCHITECTURE & DESIGN PATTERNS
* Pathfinding:
* NavMeshAgent integration for melee and ranged archetypes.
* Waypoint patrol navigation and line-of-sight target pursuit.
* Automatic arena floor projection via NavMesh.SamplePosition.


* Finite State Machine (FSM):
* Enemy AI: Idle, Patrol, Pursue, Attack, Hurt, Unbalanced, Stunned, Dead ('State.cs', 'AI.cs').
* Player Combat: FreeMovement, Attack1, Attack2, Attack3, BlackPowderThrow, ExecutionWindup, DashRoll, Downed ('PlayerCombatFSM.cs').


* Observer Pattern:
* Loosely coupled C# Events and Actions decoupling gameplay logic from UI:
* PlayerStats: OnHealthChanged, OnPlayerDied.
* PlayerInventory: OnPowderChanged, OnAmmoChanged, OnSwordEquipped, OnPistolEquipped[cite: 1].
* EnemyCombatManager: OnDamaged, OnStunStateEntered, OnDeath.
* WaveSpawner: OnBattleStarted, OnWaveStarted, OnBattleCompleted.


* Object Pooling:
* ObjectPooler and PooledObject dynamically recycle projectile instances without runtime garbage collection spikes.

* ScriptableObjects:
* EnemyConfigSO: Modular enemy stats, vision ranges, and drop rates.
* SwordWeaponItem / PistolWeaponItem: Weapon scaling, combos, and damage multipliers.
* WaveConfigSO: Data-driven wave encounters, squad compositions, delays, and progression rewards[cite: 4].

* Combat & Status Mechanics:
* 3-hit light/heavy combo chain applying Black Powder primer.
* Status unbalancing leading to lethal execution quick-shots.
* Dynamic invulnerability frames (i-frames) during evasive rolls.

* Generic Interaction:
* IInteractable interface with PlayerInteractionDetector raycast/overlap handling.

* * Juice & Game Feel:
* Screen shake impulses via CinemachineImpulseSource and custom hit-stop time dilation.

FOR the Latest Version:
Clone the repository:
git clone [https://github.com/borismakarows/Bonehaven.git](https://www.google.com/search?q=https://github.com/borismakarows/Bonehaven.git)


