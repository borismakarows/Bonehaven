using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace BoneHaven
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class PlayerLocomotion : MonoBehaviour
    {
        #region Move Settings
        [Header("Move")]
        [Tooltip("Move speed of the character in m/s (Walk with Shift)")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s (Default Run)")]
        public float SprintSpeed = 8.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        #endregion

        #region Cinemachine Settings
        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degrees to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        #endregion

        #region Internal References & State
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private bool _isMovementLocked = false;

        private CharacterController _controller;
        private PlayerInputManager _input;
        private GameObject _mainCamera;
#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private const float _threshold = 0.01f;

        public event Action<float, float> OnLocomotionUpdated;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput != null && _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }
        #endregion

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputManager>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
        }

        private void Update()
        {
            if (!_isMovementLocked)
            {
                Move();
            }
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        #region Camera Rotation (Original Starter Assets Logic)
        private void CameraRotation()
        {
            if (_input == null || CinemachineCameraTarget == null) return;

            // If there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // Clamp rotations so values are limited to 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                _cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw,
                0.0f
            );
        }
        #endregion

        #region Movement Calculation (Original Logic)
        private void Move()
        {
            if (_input == null) return;

            // Shift basılıyken Walk, normalde Run
            float targetSpeed = _input.sprint ? MoveSpeed : SprintSpeed;

            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            float speedOffset = 0.1f;
            float inputMagnitude = _input.move.magnitude;

            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            if (_input.move != Vector2.zero && _mainCamera != null)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            OnLocomotionUpdated?.Invoke(_animationBlend, inputMagnitude);
        }
        #endregion

        #region Decoupled Public API
        public void LockMovement(bool isLocked)
        {
            _isMovementLocked = isLocked;
            if (isLocked)
            {
                _speed = 0f;
                _animationBlend = 0f;
                OnLocomotionUpdated?.Invoke(0f, 0f);
            }
        }

        public void ManualMove(Vector3 motion)
        {
            _controller.Move(motion);
        }

        public Vector3 GetCameraRelativeDirection(Vector2 rawInput)
        {
            if (rawInput.sqrMagnitude < _threshold) return transform.forward;

            Vector3 camFwd = _mainCamera != null ? _mainCamera.transform.forward : Vector3.forward;
            Vector3 camRight = _mainCamera != null ? _mainCamera.transform.right : Vector3.right;
            camFwd.y = 0f;
            camRight.y = 0f;

            return (camFwd.normalized * rawInput.y + camRight.normalized * rawInput.x).normalized;
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }
        #endregion
    }
}