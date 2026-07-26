using UnityEngine;

    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public PlayerInputManager playerInputManager;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            playerInputManager.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            playerInputManager.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            playerInputManager.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            playerInputManager.SprintInput(virtualSprintState);
        }
        

}
