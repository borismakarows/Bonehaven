using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum InputMaps
{
	Roam,
	UI
}

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputManager : MonoBehaviour
{
		[Header("Inputs")]
		[SerializeField] private PlayerInput playerInput;
		[SerializeField] private InputActionReference guardActionRef;
		private const string roamMapName = "Roam";
		private const string UIMapName = "UI";
		private InputMaps previousInputMap;
		private InputMaps currentInputMap; 

		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool guard;
		public bool canJump;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;
		
#region Events
		public static event Action OnInventoryInterfaceOpened;
		public static event Action OnSlashFired;
		public static event Action OnWithdrawFired;
		public static event Action OnShootFired;
		public static event Action<bool> OnGuardFired;
#endregion



#region Unity Funcs.
        void OnValidate()
        {
            if (playerInput == null) {playerInput = GetComponent<PlayerInput>();}
        }

        void Awake()
        {
            if (playerInput == null) {playerInput = GetComponent<PlayerInput>();}
			playerInput.defaultActionMap = roamMapName;
        }

	    void OnEnable()
    	{
			if (guardActionRef != null && guardActionRef.action != null)
			{
				guardActionRef.action.Enable();

				guardActionRef.action.performed += ctx => HandleGuardInput(ctx);
				guardActionRef.action.canceled += ctx => HandleGuardInput(ctx);
			}

			Combat.OnArmed += onarmed => CanJump(!onarmed);
    	}

	    void OnDisable()
    	{
			if (guardActionRef != null && guardActionRef.action != null)
			{
				guardActionRef.action.performed -= ctx => HandleGuardInput(ctx);
				guardActionRef.action.canceled -= ctx => HandleGuardInput(ctx);

				guardActionRef.action.Disable();
			}

			Combat.OnArmed -= onarmed => CanJump(!onarmed);
    	}
#endregion

#region Input Events
#if ENABLE_INPUT_SYSTEM
    	public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			if (canJump){JumpInput(value.isPressed);}
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnInventory(InputValue value)
		{
			if (value.isPressed)
			{
				if (currentInputMap == InputMaps.UI) {SwitchInputMap(previousInputMap);}
				else
				{
					previousInputMap = currentInputMap;
					SwitchInputMap(InputMaps.UI);
				}
				OnInventoryInterfaceOpened?.Invoke();
			}
		}

		public void OnDraw(InputValue value)
		{
			if (value.isPressed) {OnWithdrawFired?.Invoke();}
		}

		public void OnSlash(InputValue value)
		{
			if (!value.isPressed) return;
			
			OnSlashFired?.Invoke();
		}

		public void OnShoot(InputValue value)
		{
			if (!value.isPressed) return;
			OnShootFired?.Invoke();
		}

		private void HandleGuardInput(InputAction.CallbackContext context)
		{
			GuardInput(context.ReadValueAsButton());
		}
#endif
#endregion

#region Input Values
		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void GuardInput(bool newGuardState)
		{
			if (guard == newGuardState) return;
			guard = newGuardState;
		}

		private void CanJump(bool newCanJumpState)
		{
			canJump = newCanJumpState;
		}

#endregion

#region Cursor Functions
		private void OnApplicationFocus(bool hasFocus)
		{
			SetCursorState(cursorLocked);
		}

		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
#endregion

#region Input Map 

		public InputMaps GetCurrentInputMap() {return currentInputMap;}
		
		public void SwitchInputMap(InputMaps _InputMap)
		{
			switch (_InputMap)
			{
				case InputMaps.Roam:
				SwitchToRoamInput();
				break;
				case InputMaps.UI:
				SwitchToUIInput();
				break;
				default:
				Debug.Log("Couldn't find the selected Input Map and Changed to Roam as Default Choice");
				SwitchToRoamInput();
				break;
			}
		}
		
		private void SwitchToRoamInput()
		{
			if(playerInput.actions.FindActionMap(roamMapName) != null) 
			{
				playerInput.SwitchCurrentActionMap(roamMapName);
				currentInputMap = InputMaps.Roam;
				cursorLocked = true;
				SetCursorState(cursorLocked);	
			}
		}

		private void SwitchToUIInput()
		{
			if(playerInput.actions.FindActionMap(UIMapName) != null)
			{
				playerInput.SwitchCurrentActionMap(UIMapName);
				currentInputMap = InputMaps.UI;
				cursorLocked = false;
				SetCursorState(cursorLocked);
			}
		}
#endregion

}
	
