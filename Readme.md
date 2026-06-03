Easy Setup: 
AI (FSM) tested in MechanicalTesting Scene.
To be sure it is working: 
1. Player armature with required components:
	1. Third Person Controller
	2. Starter Assets Input
	3. Combat
	4. Health 
	5. Inventory Manager
	6. and Player armature works with Main Camera(prefab) and Player follow camera(prefab) //Third Person Starter Assets.
2. FSM requirements: 
	1. FSM works with Agent Settings which defines skills and movement sets.
	2. Requires NavMesh Surface
	3. Environment Manager and creating checkpoint gameobject with Checkpoint tag for patrolling.
	
3. UI requirements:
	1. UI Manager (prefab) 
	2. Inventory Menu (prefab)

4. Some scripts might need small assignments from inspector panel.
