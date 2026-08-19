using UnityEngine;
using UnityEngine.InputSystem;

namespace BoneHaven
{
    [RequireComponent(typeof(CharacterController))]
    public class CombatLungeTester : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] private SoftTargetLock targetLock;
        [SerializeField] private CombatLunge combatLunge;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSmoothTime = 0.12f;

        private CharacterController controller;
        private float targetRotation = 0.0f;
        private float rotationVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (targetLock == null)
                targetLock = GetComponent<SoftTargetLock>() ?? GetComponentInChildren<SoftTargetLock>();

            if (combatLunge == null)
                combatLunge = GetComponent<CombatLunge>() ?? GetComponentInChildren<CombatLunge>();

            ResolveCamera();
        }

        private void ResolveCamera()
        {
            // Cinemachine drives the Camera tagged as MainCamera with CinemachineBrain
            if (cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    cameraTransform = Camera.main.transform;
                }
                else
                {
                    Camera anyCam = FindAnyObjectByType<Camera>();
                    if (anyCam != null)
                    {
                        cameraTransform = anyCam.transform;
                    }
                }
            }
        }

        private void Update()
        {
            // Fallback safety in case the camera initialized after Awake
            if (cameraTransform == null)
            {
                ResolveCamera();
            }

            // 1. Read Movement Input (Starter Assets / New Input System)
            Vector2 rawInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                float x = 0f;
                float y = 0f;

                if (Keyboard.current.aKey.isPressed) x -= 1f;
                if (Keyboard.current.dKey.isPressed) x += 1f;
                if (Keyboard.current.sKey.isPressed) y -= 1f;
                if (Keyboard.current.wKey.isPressed) y += 1f;

                rawInput = new Vector2(x, y);
            }

            if (Gamepad.current != null && rawInput.sqrMagnitude < 0.01f)
            {
                rawInput = Gamepad.current.leftStick.ReadValue();
            }

            Vector3 inputDir = new Vector3(rawInput.x, 0f, rawInput.y).normalized;

            // 2. Camera-Relative Movement (Matching Starter Assets rotation style)
            if (inputDir.sqrMagnitude > 0.01f)
            {
                float targetYaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
                targetRotation = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + targetYaw;
                
                float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, smoothYaw, 0.0f);

                Vector3 targetMoveDir = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
                controller.Move(targetMoveDir.normalized * (moveSpeed * Time.deltaTime));
            }

            // 3. Attack Trigger
            bool attackPressed = false;

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                attackPressed = true;
            }
            else if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                attackPressed = true;
            }

            if (attackPressed && targetLock != null && combatLunge != null)
            {
                Transform cam = cameraTransform != null ? cameraTransform : transform;
                Transform target = targetLock.GetTarget(inputDir, cam);
                Vector3 fallbackDir = inputDir.sqrMagnitude > 0.01f ? transform.forward : transform.forward;

                combatLunge.ExecuteLunge(target, fallbackDir);

                if (target != null)
                {
                    Debug.DrawLine(transform.position + Vector3.up, target.position + Vector3.up, Color.green, 1.0f);
                }
                else
                {
                    Debug.DrawRay(transform.position + Vector3.up, transform.forward * 3f, Color.red, 1.0f);
                }
            }
        }
    }
}